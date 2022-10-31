using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
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

namespace EnergyComparer.Services
{
    public class ExperimentService : IExperimentService
    {
        private readonly IDutAdapter _dutAdapter;
        private readonly ILogger _logger;
        private readonly bool _isProd;
        private IHardwareMonitorService _hardwareMonitorService;
        private  IOperatingSystemAdapter _adapter;
        private IDataHandler _dataHandler;
        private  IHardwareHandler _hardwareHandler;
        private  IWifiService _wifiService;
        private readonly bool _saveToDb;
        private readonly Func<(IHardwareMonitorService, IOperatingSystemAdapter, IHardwareHandler, IWifiService)> _initializeOfflineDependencies;
        private readonly Func<IDataHandler> _initializeOnlineDependencies;
        private readonly Func<(IHardwareMonitorService, IOperatingSystemAdapter, IDataHandler, IHardwareHandler, IWifiService)> _deleteDependencies;
        private readonly string _wifiAdapterName;
        private Dictionary<string, int> _profilerCounter = new Dictionary<string, int>();
        
        private string _firstProfiler { get; set; } = "";

        public ExperimentService(IDutAdapter dutAdapter, ILogger logger, bool isProd, string wifiAdapterName, bool saveToDb, Func<(IHardwareMonitorService, IOperatingSystemAdapter, IHardwareHandler, IWifiService)> initializeOfflineDependencies, Func<IDataHandler> initializeOnlineDependencies, Func<(IHardwareMonitorService, IOperatingSystemAdapter, IDataHandler, IHardwareHandler, IWifiService)> deleteDependencies)
        {
            _dutAdapter = dutAdapter;
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

        public async Task<bool> RunExperiment(IEnergyProfiler energyProfiler, ITestCase testCase)
        {
            var stopwatch = new Stopwatch();

            var (initialTemperatures, initialBattery) = InitializeExperiment(energyProfiler);

            // TODO: Force garbage collection. This is however not recommended as it is expesive. but it out case it might make sense
            // https://stackoverflow.com/questions/4257372/how-to-force-garbage-collector-to-run
            RunGarbageCollection(); // TODO: Test if this makes a difference

            _logger.Information("The energy profiler has started");
            _logger.Information("The test case {testcase} will now run untill at least {time}", testCase.GetName(), DateTime.UtcNow.AddMinutes(Constants.DurationOfExperimentsInMinutes));
            var startTime = StartTimeAndProfiler(energyProfiler, stopwatch);

            var counter = RunTestcase(testCase, startTime);

            var (stopTime, duration) = StopTimeAndProfiler(energyProfiler, stopwatch);

            await EnableWifiAndDependencies();

            var (endTemperatures, endBattery) = GetEndMeasurements();
            var experimentId = await EndExperiment(testCase, stopTime, startTime, counter, energyProfiler, initialTemperatures, endTemperatures, duration, initialBattery, endBattery);

            return await HandleResultsIfValid(energyProfiler, startTime, experimentId);
        }

        private (DateTime, long) StopTimeAndProfiler(IEnergyProfiler energyProfiler, Stopwatch stopwatch)
        {
            var duration = stopwatch.ElapsedMilliseconds;
            energyProfiler.Stop();
            var stopTime = DateTime.UtcNow;

            return (stopTime, duration);
        }

        private DateTime StartTimeAndProfiler(IEnergyProfiler energyProfiler, Stopwatch stopwatch)
        {
            var startTime = DateTime.UtcNow; // TODO: order of time and start profiler

            stopwatch.Start();
            energyProfiler.Start(startTime);
            return startTime;
        }

        private int RunTestcase(ITestCase testCase, DateTime startTime)
        {
            var output = "";

            var basePath = GetTestCaseBasePath();

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo(testCase.GetExecutablePath(basePath), Constants.DurationOfExperimentsInMinutes.ToString());

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();
            }

            //var counter = 0;
            //if (testCase.GetName() == EWindowsProfilers.E3.ToString())
            //{
            //}
            //else
            //{
            //    while (startTime.AddMinutes(Constants.DurationOfExperimentsInMinutes) > DateTime.UtcNow)
            //    {
            //        testCase.Run(); // TODO: Perhaps the run should return something, and use this for something (sestoft)
            //        counter += 1;
            //    }
            //}
            var counter = output.Trim().Split('\n').Last();
            return int.Parse(counter); ;
        }

        private static DirectoryInfo GetTestCaseBasePath()
        {
            var basePath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent;
            //basePath.MoveTo("09th-semester-test-cases");
            return basePath;
        }

        private (List<DtoMeasurement>, DtoMeasurement) GetEndMeasurements()
        {
            _hardwareMonitorService = new HardwareMonitorService(_logger);

            _logger.Information("The experiment is done. The end temperatures will be measured.");
            var endTemperatures = _hardwareMonitorService.GetCoreTemperatures();
            var endBattery = _dutAdapter.GetCharge();

            return (endTemperatures, endBattery);
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
            var (timeSeries, rawData) = profiler.ParseData(stream, experimentId, date);

            await _dataHandler.InsertRawData(rawData);
            await _dataHandler.InsertTimeSeriesData(timeSeries);
        }

        private string GetPath(IEnergyProfiler profiler, DateTime startTime)
        {
            return Constants.GetFilePathForSouce(profiler.GetName(), startTime);
        }

        private async Task<int> EndExperiment(ITestCase program, DateTime stopTime, DateTime startTime, int counter, IEnergyProfiler energyProfiler, List<DtoMeasurement> initialTemperatures, List<DtoMeasurement> endTemperatures, long duration, DtoMeasurement initialBattery, DtoMeasurement endBattery)
        {
            _logger.Information("The data saved to {path}", Constants.GetPathForSource(energyProfiler.GetName()));

            _logger.Information("The wifi was enabled, the data will now be parsed and saved");
            var system = await _dataHandler.GetDut();
            var profiler = await _dataHandler.GetProfiler(energyProfiler);
            var profilerCount = IncrementAndGetProfilerCount(energyProfiler);
            var configuration = await _dataHandler.GetConfiguration(system.Version);

            var experiment = await _dataHandler.GetExperiment(program.GetProgram().Id, system.Id, profiler.Id, program, startTime, stopTime, counter, profilerCount, _firstProfiler, configuration.Id, duration, system.Version);

            if (_saveToDb)
            {
                await _dataHandler.InsertMeasurement(endTemperatures, experiment.Id, stopTime);
                await _dataHandler.InsertMeasurement(initialTemperatures, experiment.Id, startTime);


                if (initialBattery != null && endBattery != null)
                {
                    var startBattery = new List<DtoMeasurement>() { initialBattery };
                    var endBatteryCharge = new List<DtoMeasurement>() { endBattery };

                    await _dataHandler.InsertMeasurement(startBattery, experiment.Id, startTime);
                    await _dataHandler.InsertMeasurement(endBatteryCharge, experiment.Id, stopTime);
                }

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

        private (List<DtoMeasurement>, DtoMeasurement) InitializeExperiment(IEnergyProfiler energyProfiler)
        {
            if (String.IsNullOrEmpty(_firstProfiler)) _firstProfiler = energyProfiler.GetName();

            _dataHandler.CloseConnection();
            _wifiService.Disable(_isProd);

            _logger.Information("Measuring initial cpu temperatures");
            var initialTemperatures = _hardwareMonitorService.GetCoreTemperatures();
            var initialBattery = _dutAdapter.GetCharge();

            return (initialTemperatures, initialBattery);
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
        Task<bool> RunExperiment(IEnergyProfiler energyProfiler, ITestCase testProgram);
    }
}
