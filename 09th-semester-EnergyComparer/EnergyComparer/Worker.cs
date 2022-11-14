using EnergyComparer.DUTs;
using EnergyComparer.Handlers;
using EnergyComparer.Services;
using EnergyComparer.Utils;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker
    {
        private readonly ILogger _logger;
        private IExperimentService _experimentService;
        private readonly IConfiguration _configuration;
        private IDataHandler _dataHandler;
        private int _iterationsBeforeRestart;
        private IEnergyProfilerService _profilerService;
        private ExperimentHandler _experimentHandler;
        private string _wifiAdapterName;
        private string _machineName;
        private readonly bool _shouldRestart;
        private readonly bool _hasBattery;
        private readonly bool _isProd;
        private readonly bool _iterateOverProfilers;
        private readonly int _maxIterations;
        private readonly IDutAdapter _dutAdapter;
        private IOperatingSystemAdapter _adapterService;
        private IHardwareMonitorService _hardwareMonitorService;

        public Worker(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _iterationsBeforeRestart = ConfigUtils.GetIterationsBeforeRestart(configuration);
            _iterateOverProfilers = ConfigUtils.GetIterateOverProfilers(configuration);
            _maxIterations = ConfigUtils.GetTotalIterations(configuration);
            _wifiAdapterName = ConfigUtils.GetWifiAdapterName(configuration);
            _shouldRestart = ConfigUtils.GetShouldRestart(configuration);
            _machineName = ConfigUtils.GetMachineName(configuration);
            _hasBattery = ConfigUtils.GetHasBattery(configuration);
            _isProd = ConfigUtils.GetIsProd(configuration);

            if (_isProd && !SystemUtils.IsValidNameForProd(_machineName))
            {
                throw new Exception($"The machine name '{_machineName}' is not valid for prod");
            }

            _hardwareMonitorService = SystemUtils.GetHardwareMonitorService(logger);
            _dutAdapter = SystemUtils.GetDutAdapter(_logger, _hasBattery, _iterateOverProfilers, _hardwareMonitorService);
            _profilerService = new EnergyProfilerService(_iterateOverProfilers, _dutAdapter, logger);

            var saveToDb = ConfigUtils.GetSaveToDb(configuration);
            var isProd = ConfigUtils.GetIsProd(configuration);
            
            InitializeDependencies();

            _experimentService = new ExperimentService(_logger, isProd, saveToDb, InitializeOfflineDependencies, InitializeOnlineDependencies, DeleteDependencies);
        }



        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _dataHandler.IncrementVersionForSystem(); // TODO: increment for all systems, not just the current one
            CreateFolderIfNew();

            await _experimentHandler.WaitTillStableState(); // TODO: Tie to one single core
            var isExperimentValid = true;

            var currentTestCase = await _experimentHandler.GetTestCase(_dataHandler);

            while (!_experimentHandler.ShouldStopExperiment() && isExperimentValid && !EnoughEntires())
            {
                var profiler = await _profilerService.GetNext(currentTestCase, _dataHandler, _adapterService);

                StopUnneccesaryProcesses();
                RemoveDependencies();

                isExperimentValid = await _experimentService.RunExperiment(profiler, currentTestCase);

                InitializeDependencies();

                _logger.Information("Experiment ended running at: {time}. Next experiment will run at {time2}", DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(Constants.MinutesBetweenExperiments));
                await Task.Delay(TimeSpan.FromMinutes(Constants.MinutesBetweenExperiments), stoppingToken);
            }

            await _profilerService.SaveProfilers(_dataHandler);
            _adapterService.Restart();
           
        }

        private void StopUnneccesaryProcesses()
        {
            _adapterService.StopunneccesaryProcesses();
        }

        public async Task EnableWifi()
        {
            var (_, _, _, wifi, _) = InitializeOfflineDependencies();

            await wifi.Enable(_isProd);
        }

        private (IHardwareMonitorService, IOperatingSystemAdapter, IHardwareHandler, IWifiService, IExperimentHandler) InitializeOfflineDependencies()
        {
            var hardwareMonitorService = SystemUtils.GetHardwareMonitorService(_logger);
            var adapter = SystemUtils.InitializeAdapterService(_logger, _isProd,  _shouldRestart, hardwareMonitorService);
            var energyProfilerService = new HardwareHandler(_logger, _wifiAdapterName, adapter);
            var wifiService = new WifiService(energyProfilerService);
            var experimentHandler = new ExperimentHandler(_isProd, _maxIterations, _hasBattery, _iterateOverProfilers, _logger, _dutAdapter, adapter, _machineName);

            return (hardwareMonitorService, adapter, energyProfilerService, wifiService, experimentHandler);
        }

        private IDataHandler InitializeOnlineDependencies()
        {
            return new DataHandler(_logger, _adapterService, GetDbConnectionFactory, _machineName, _dutAdapter);
        }

        private (IOperatingSystemAdapter, IDataHandler, IWifiService, IExperimentHandler) DeleteDependencies()
        {
            return (null, null, null, null);
        }

        private IDbConnection GetDbConnectionFactory()
        {
            var connectionString = ConfigUtils.GetConnectionString(_configuration);
            var con = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            con.Open();
            return con;
        }

        private bool EnoughEntires()
        {
            var profilers = _experimentService.GetProfilerCounters();

            if (profilers.Count() == 0) return false; // the first run

            return profilers.All(x => x >= _iterationsBeforeRestart);
        }

        private void CreateFolderIfNew()
        {
            _logger.Information("Creating non-existing folders");
            var paths = _experimentHandler.GetAllRequiredPaths();

            foreach (var p in paths)
            {
                _experimentHandler.CreateFolder(p);
            }
        }

        private void InitializeDependencies()
        {
            _hardwareMonitorService = SystemUtils.GetHardwareMonitorService(_logger);
            _adapterService = SystemUtils.InitializeAdapterService(_logger, _isProd, _shouldRestart, _hardwareMonitorService);
            _dataHandler = new DataHandler(_logger, _adapterService, GetDbConnectionFactory, _machineName, _dutAdapter);
            _experimentHandler = new ExperimentHandler(_isProd, _maxIterations, _hasBattery, _iterateOverProfilers, _logger, _dutAdapter, _adapterService, _machineName);
            _dataHandler.InitializeConnection();
        }

        private void RemoveDependencies()
        {
            _dataHandler.CloseConnection();
            _hardwareMonitorService = null;
            _adapterService = null;
            _dataHandler = null;
        }
    }
}