using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using EnergyComparer.Services;
using EnergyComparer.Utils;
using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.Connections;
using Microsoft.Win32;
using Serilog;
using System.ComponentModel;
using System.Data;
using System.Management;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
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

        public Worker(ILogger logger, IConfiguration configuration)
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _dataHandler.IncrementVersionForSystem(); // TODO: increment for all systems, not just the current one

            try
            {
                CreateFolderIfNew();

                // TODO: Tie to one single core

                await _adapterService.WaitTillStableState();
                var isExperimentValid = true;

                var currentSoftwareEntity = _adapterService.GetSoftwareEntity(_dataHandler);

                while (!_adapterService.ShouldStopExperiment() && isExperimentValid && !EnoughEntires())
                {
                    var profiler = await _profilerService.GetNext(currentSoftwareEntity, _dataHandler, _adapterService);

                    RemoveDependencies();

                    isExperimentValid = await _experimentService.RunExperiment(profiler, currentSoftwareEntity);

                    InitializeDependencies();

                    _logger.Information("Experiment ended running at: {time}. Next experiment will run ata {time2}", DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(Constants.MinutesBetweenExperiments));
                    await Task.Delay(TimeSpan.FromMinutes(Constants.MinutesBetweenExperiments), stoppingToken);
                }

                await _profilerService.SaveProfilers(_dataHandler);
                _adapterService.Restart();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception when running experiments");
                throw;
            }
            finally
            {
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            }

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