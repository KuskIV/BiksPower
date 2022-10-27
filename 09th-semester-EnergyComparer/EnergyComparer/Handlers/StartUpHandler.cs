using EnergyComparer.Services;
using EnergyComparer.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Handlers
{
    internal class StartUpHandler
    {
        private readonly ILogger _logger;
        private IExperimentService _experimentService;
        private readonly IConfiguration _configuration;
        private IDataHandler _dataHandler;
        private int _iterationsBeforeRestart;
        private IEnergyProfilerService _profilerService;
        private string _wifiAdapterName;
        private string _machineName;
        private readonly bool _shouldRestart;
        private readonly bool _hasBattery;
        private readonly bool _isProd;
        private IAdapterService _adapterService;
        private HardwareMonitorService _hardwareMonitorService;

        public StartUpHandler(ILogger? logger, IConfiguration? configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var iterateOverProfilers = ConfigUtils.GetIterateOverProfilers(configuration);
            _iterationsBeforeRestart = ConfigUtils.GetIterationsBeforeRestart(configuration);
            _profilerService = new EnergyProfilerService(iterateOverProfilers);
            _wifiAdapterName = ConfigUtils.GetWifiAdapterName(configuration);
            _machineName = ConfigUtils.GetMachineName(configuration);
            _shouldRestart = ConfigUtils.GetShouldRestart(configuration);
            _hasBattery = ConfigUtils.GetHasBattery(configuration);
            _isProd = ConfigUtils.GetIsProd(configuration);

            var saveToDb = ConfigUtils.GetSaveToDb(configuration);
            var isProd = ConfigUtils.GetIsProd(configuration);
            _experimentService = new ExperimentService(_logger, isProd, _wifiAdapterName, saveToDb, InitializeOfflineDependencies, InitializeOnlineDependencies, DeleteDependencies);

            InitializeDependencies();
        }

        private async Task EnableWifi()
        {
            var (_, _, _, wifi) = InitializeOfflineDependencies();

            await wifi.Enable(_isProd);
        }

        private (IHardwareMonitorService, IAdapterService, IHardwareHandler, IWifiService) InitializeOfflineDependencies()
        {
            var hardwareMonitorService = new HardwareMonitorService(_logger);
            var adapter = new AdapterWindowsLaptopService(_hardwareMonitorService, _logger, _hasBattery, _isProd, _shouldRestart);
            var energyProfilerService = new HardwareHandler(_logger, _wifiAdapterName, adapter);
            var wifiService = new WifiService(energyProfilerService);

            return (hardwareMonitorService, adapter, energyProfilerService, wifiService);
        }

        private IDataHandler InitializeOnlineDependencies()
        {
            return new DataHandler(_logger, _adapterService, GetDbConnectionFactory, _machineName);
        }

        private (IHardwareMonitorService, IAdapterService, IDataHandler, IHardwareHandler, IWifiService) DeleteDependencies()
        {
            return (null, null, null, null, null);
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
            var paths = _adapterService.GetAllRequiredPaths();

            foreach (var p in paths)
            {
                _adapterService.CreateFolder(p);
            }
        }

        private void InitializeDependencies()
        {
            _hardwareMonitorService = new HardwareMonitorService(_logger);
            _adapterService = new AdapterWindowsLaptopService(_hardwareMonitorService, _logger, _hasBattery, _isProd, _shouldRestart);
            _dataHandler = new DataHandler(_logger, _adapterService, GetDbConnectionFactory, _machineName);
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
