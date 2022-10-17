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
using EnergyComparer.Utils;
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
        private readonly bool _isProd;
        private IHardwareMonitorService _hardwareMonitorService;
        private  IAdapterService _adapter;
        private IDataHandler _dataHandler;
        private  IHardwareHandler _hardwareHandler;
        private  IWifiService _wifiService;
        private readonly bool _saveToDb;
        private readonly Func<(IHardwareMonitorService, IAdapterService, IHardwareHandler, IWifiService)> _initializeOfflineDependencies;
        private readonly Func<IDataHandler> _initializeOnlineDependencies;
        private readonly Func<(IHardwareMonitorService, IAdapterService, IDataHandler, IHardwareHandler, IWifiService)> _deleteDependencies;
        private readonly string _wifiAdapterName;
        private Dictionary<string, int> _profilerCounter = new Dictionary<string, int>();
        
        private string _firstProfiler { get; set; } = "";

        public ExperimentService(ILogger logger, bool isProd, string wifiAdapterName, bool saveToDb, Func<(IHardwareMonitorService, IAdapterService, IHardwareHandler, IWifiService)> initializeOfflineDependencies, Func<IDataHandler> initializeOnlineDependencies, Func<(IHardwareMonitorService, IAdapterService, IDataHandler, IHardwareHandler, IWifiService)> deleteDependencies)
        {
            _logger = logger;

            _isProd = isProd;
            _wifiAdapterName = wifiAdapterName;
            _saveToDb = saveToDb;
            
            _initializeOfflineDependencies = initializeOfflineDependencies;
            _initializeOnlineDependencies = initializeOnlineDependencies;
            _deleteDependencies = deleteDependencies;
            
            InitializeDependencies();
        }

        public List<int> GetProfilerCounters()
        {
            return _profilerCounter.Values.ToList();
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
            (_hardwareMonitorService, _adapter, _hardwareHandler, _wifiService) = _initializeOfflineDependencies();
            _logger.Information("The wifi will be enabled");
            await EnableWifi();

            _dataHandler = _initializeOnlineDependencies();
        }

        private void RunGarbageCollection()
        {
            (_hardwareMonitorService, _adapter, _dataHandler, _hardwareHandler, _wifiService) = _deleteDependencies();
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
            await _wifiService.Enable(_isProd);
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
            _wifiService.Disable(_isProd);
        }

        private void InitializeDependencies()
        {
            _dataHandler = _initializeOnlineDependencies();
            (_hardwareMonitorService, _adapter, _hardwareHandler, _wifiService) = _initializeOfflineDependencies();
        }
    }

    public interface IExperimentService
    {
        List<int> GetProfilerCounters();
        Task<bool> RunExperiment(IEnergyProfiler energyProfiler, IProgram testProgram);
    }
}
