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
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IDataHandler _dataHandler;
        private readonly IWifiService _wifiService;
        private readonly bool _isProd;
        private Dictionary<string, int> _profilerCounter = new Dictionary<string, int>();
        
        private string _firstProfiler { get; set; } = "";

        public ExperimentService(ILogger logger, IHardwareMonitorService hardwareMonitorService, IDataHandler dataHandler, IConfiguration configuration, IWifiService wifiService)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
            _dataHandler = dataHandler;
            _wifiService = wifiService;
            _isProd = configuration.GetValue<bool>("IsProd");
        }

        public async Task<bool> RunExperiment(IEnergyProfiler energyProfiler, IProgram program)
        {
            var counter = 0;
            _logger.Information("The energy profiler has started");

            InitializeExperiment(energyProfiler);

            // TODO: Force garbage collection. This is however not recommended as it is expesive. but it out case it might make sense
            // https://stackoverflow.com/questions/4257372/how-to-force-garbage-collector-to-run
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var initialTemperatures = _hardwareMonitorService.GetCoreTemperatures();
            var startTime = DateTime.UtcNow; // TODO: order of time and start profiler
            energyProfiler.Start(startTime);

            while (startTime.AddMinutes(Constants.DurationOfExperimentsInMinutes) > DateTime.UtcNow)
            {
                program.Run(); // TODO: Perhaps use the result for something (sestoft)
                counter += 1;
            }

            energyProfiler.Stop();
            var stopTime = DateTime.UtcNow;
            var endTemperatures = _hardwareMonitorService.GetCoreTemperatures();


            _logger.Information("The experiment has stopped, and data saved to {path}", Constants.GetPathForSource(energyProfiler.GetName()));

            await EndExperiment(program, stopTime, startTime, counter, energyProfiler, initialTemperatures, endTemperatures);

            return HandleResultsIfValid(program, startTime);
        }

        private bool HandleResultsIfValid(IProgram program, DateTime date)
        {
            if (_hardwareMonitorService.GetAverageCpuTemperature() < Constants.TemperatureUpperLimit)
            {
                if (_isProd)
                {
                    SaveResults(program, date);
                }
                else
                {
                    _logger.Information("Loggint results. TODO: IMPLEMENT THIS");
                }

                _logger.Information("Saving results");

                return true;
            }
            else
            {
                _logger.Warning("The average temperature is too hight. Results are not saved");
                return false;
            }
        }

        public void SaveResults(IProgram program, DateTime date)
        {
            var data = new List<DtoRawData>();
            var stream = GetPath(program, date);
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                data = program.ParseCsv(csv);

            // TODO: Data to db
        }

        private string GetPath(IProgram program, DateTime startTime)
        {
            return Constants.GetFilePathForSouce(program.GetName(), startTime);
        }

        private async Task EndExperiment(IProgram program, DateTime stopTime, DateTime startTime, int counter, IEnergyProfiler energyProfiler, List<DtoTemperature> initialTemperatures, List<DtoTemperature> endTemperatures)
        {
            await EnableWifi();

            var system = await _dataHandler.GetSystem();
            var profiler = await _dataHandler.GetProfiler(energyProfiler);
            var profilerCount = IncrementAndGetProfilerCount(energyProfiler);
            var configuration = await _dataHandler.GetConfiguration(system.Version);

            var experiment = await _dataHandler.GetExperiment(program.GetProgram().Id, system.Id, profiler.Id, program, startTime, stopTime, counter, profilerCount, _firstProfiler, configuration.Id);
            await _dataHandler.InsertTemperatures(endTemperatures, experiment.Id, stopTime);
            await _dataHandler.InsertTemperatures(initialTemperatures, experiment.Id, startTime);
        }

        private async Task EnableWifi()
        {
            await _wifiService.Enable();
            _dataHandler.InitializeConnection();
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
    }

    public interface IExperimentService
    {
        Task<bool> RunExperiment(IEnergyProfiler energyProfiler, IProgram testProgram);
        void SaveResults(IProgram program, DateTime date);
    }
}
