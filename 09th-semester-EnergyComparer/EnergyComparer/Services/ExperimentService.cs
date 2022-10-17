using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using MySqlX.XDevAPI.Common;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Serilog.ILogger;
using Result = EnergyComparer.Models.Result;

namespace EnergyComparer.Services
{
    public class ExperimentService : IExperimentService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly Func<IDbConnection> _connectionFactory;
        private IHardwareMonitorService _hardwareMonitorService;
        private IAdapterService _adapter;
        private IDataHandler _dataHandler;
        private IHardwareHandler _energyProfilerService;
        private IWifiService _wifiService;
        private readonly bool _isProd;
        private readonly bool _saveToDb;
        private readonly string _wifiAdapterName;
        private Dictionary<string, int> _profilerCounter = new Dictionary<string, int>();
        
        private string _firstProfiler { get; set; } = "";

        public ExperimentService(ILogger logger, IConfiguration configuration, Func<IDbConnection> connectionFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionFactory = connectionFactory;
            _isProd = configuration.GetValue<bool>("IsProd");
            _saveToDb = configuration.GetValue<bool>("SaveToDb");
            _wifiAdapterName = configuration.GetValue<string>("wifiAdapterName");

            InitializeDependencies();
        }

        public async Task<bool> RunExperiment(IEnergyProfiler energyProfiler, IProgram program)
        {
            var counter = 0;
            _logger.Information("The energy profiler has started");

            InitializeExperiment(energyProfiler);


            _logger.Information("Measuring initial cpu temperatures");
            var initialTemperatures = _hardwareMonitorService.GetCoreTemperatures();

            // TODO: Force garbage collection. This is however not recommended as it is expesive. but it out case it might make sense
            // https://stackoverflow.com/questions/4257372/how-to-force-garbage-collector-to-run

            RunGarbageCollection();

            _logger.Information("The experiment will now run untill at least {time}", DateTime.UtcNow.AddMinutes(Constants.DurationOfExperimentsInMinutes));
            var startTime = DateTime.UtcNow; // TODO: order of time and start profiler
            energyProfiler.Start(startTime);

            while (startTime.AddMinutes(Constants.DurationOfExperimentsInMinutes) > DateTime.UtcNow)
            {
                program.Run(); // TODO: Perhaps the run should return something, and use this for something (sestoft)
                counter += 1;
            }

            energyProfiler.Stop();
            var stopTime = DateTime.UtcNow;

            var endTemperatures = GetEndTemperatures();
            
            await EnableWifiAndDependencies();

            _logger.Information("The data saved to {path}", Constants.GetPathForSource(energyProfiler.GetName()));

            var experimentId = await EndExperiment(program, stopTime, startTime, counter, energyProfiler, initialTemperatures, endTemperatures);

            return await HandleResultsIfValid(energyProfiler, startTime, experimentId);
        }

        private List<DtoTemperature> GetEndTemperatures()
        {
            _hardwareMonitorService = new HardwareMonitorService(_logger);

            _logger.Information("The experiment is done. The end temperatures will be measured.");
            var endTemperatures = _hardwareMonitorService.GetCoreTemperatures();
            return endTemperatures;
        }

        private async Task EnableWifiAndDependencies()
        {
            InitializeOfflineDependencies();
            _logger.Information("The wifi will be enabled");
            await EnableWifi();

            InitializeOnlineDependencies();
        }

        private void RunGarbageCollection()
        {
            DeleteDependencies();
            _logger.Information("Running garbage collector");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private async Task<bool> HandleResultsIfValid(IEnergyProfiler profiler, DateTime date, int experimentId)
        {
            if (_hardwareMonitorService.GetAverageCpuTemperature() < Constants.TemperatureUpperLimit)
            {
                if (_saveToDb)
                {
                    _logger.Information("Saving results");
                    await SaveResults(profiler, date, experimentId);
                }
                else
                {
                    await SaveResults(profiler, date, experimentId);

                    _logger.Information("Loggint results. TODO: IMPLEMENT THIS");
                }

                return true;
            }
            else
            {
                _logger.Warning("The average temperature is too high. Results are not saved");
                return false;
            }
        } 

        public async Task SaveResults(IEnergyProfiler profiler, DateTime date, int experimentId)
        {
            var stream = GetPath(profiler, date);
            var data = profiler.ParseCsv(stream, experimentId, date);

            await _dataHandler.InsertRawData(data);
        }

        private string GetPath(IEnergyProfiler profiler, DateTime startTime)
        {
            return Constants.GetFilePathForSouce(profiler.GetName(), startTime);
        }

        private async Task<int> EndExperiment(IProgram program, DateTime stopTime, DateTime startTime, int counter, IEnergyProfiler energyProfiler, List<DtoTemperature> initialTemperatures, List<DtoTemperature> endTemperatures)
        {
            _logger.Information("The wifi was enabled, the data will now be parsed and saved");
            var system = await _dataHandler.GetSystem();
            var profiler = await _dataHandler.GetProfiler(energyProfiler);
            var profilerCount = IncrementAndGetProfilerCount(energyProfiler);
            var configuration = await _dataHandler.GetConfiguration(system.Version);

            var experiment = await _dataHandler.GetExperiment(program.GetProgram().Id, system.Id, profiler.Id, program, startTime, stopTime, counter, profilerCount, _firstProfiler, configuration.Id);

            if (_saveToDb)
            {
                await _dataHandler.InsertTemperatures(endTemperatures, experiment.Id, stopTime);
                await _dataHandler.InsertTemperatures(initialTemperatures, experiment.Id, startTime);
            }

            return experiment.Id;
        }

        private async Task EnableWifi()
        {
            await _wifiService.Enable();
        }

        private int IncrementAndGetProfilerCount(IEnergyProfiler energyProfiler)
        {
            if (!_profilerCounter.ContainsKey(energyProfiler.GetName()))
            {
                _profilerCounter.Add(energyProfiler.GetName(), 0);
            }

            _profilerCounter[energyProfiler.GetName()] += 1;

            return _profilerCounter[energyProfiler.GetName()];
        }

        private void InitializeExperiment(IEnergyProfiler energyProfiler)
        {
            if (String.IsNullOrEmpty(_firstProfiler)) _firstProfiler = energyProfiler.GetName();

            _dataHandler.CloseConnection();
            _wifiService.Disable();
        }

        private void InitializeDependencies()
        {
            InitializeOnlineDependencies();
            InitializeOfflineDependencies();
        }

        private void InitializeOnlineDependencies()
        {
            _dataHandler = new DataHandler(_logger, _adapter, _connectionFactory);
        }

        private void InitializeOfflineDependencies()
        {
            _hardwareMonitorService = new HardwareMonitorService(_logger);
            _adapter = new AdapterWindowsLaptopService(_hardwareMonitorService, _logger, _configuration);
            _energyProfilerService = new HardwareHandler(_logger, _wifiAdapterName, _adapter);
            _wifiService = new WifiService(_energyProfilerService);
        }

        private void DeleteDependencies()
        {
            _hardwareMonitorService = null;
            _adapter = null;
            _dataHandler = null;
            _energyProfilerService = null;
            _wifiService = null;
        }
    }

    public interface IExperimentService
    {
        Task<bool> RunExperiment(IEnergyProfiler energyProfiler, IProgram testProgram);
    }
}
