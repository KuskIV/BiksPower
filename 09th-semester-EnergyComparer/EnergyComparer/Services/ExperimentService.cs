using System;
using System.Diagnostics;
using EnergyComparer.DUTs;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.TestCases;
using EnergyComparer.Utils;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class ExperimentService : IExperimentService
    {
        private readonly ILogger _logger;
        private readonly bool _isProd;
        private  IOperatingSystemAdapter _operatingSystemAdapter;
        private IDataHandler _dataHandler;
        private  IWifiService _wifiService;
        private readonly bool _saveToDb;
        private readonly Func<(IHardwareMonitorService, IOperatingSystemAdapter, IHardwareHandler, IWifiService, IExperimentHandler)> _initializeOfflineDependencies;
        private readonly Func<IDataHandler> _initializeOnlineDependencies;
        private readonly Func<(IOperatingSystemAdapter, IDataHandler, IWifiService, IExperimentHandler)> _deleteDependencies;
        private Dictionary<string, int> _profilerCounter = new Dictionary<string, int>();
        private IExperimentHandler _experimentHandler;

        private string _firstProfiler { get; set; } = "";

        public ExperimentService(ILogger logger, bool isProd, bool saveToDb, Func<(IHardwareMonitorService, IOperatingSystemAdapter, IHardwareHandler, IWifiService, IExperimentHandler)> initializeOfflineDependencies, Func<IDataHandler> initializeOnlineDependencies, Func<(IOperatingSystemAdapter, IDataHandler, IWifiService, IExperimentHandler)> deleteDependencies)
        {
            _logger = logger;

            _isProd = isProd;
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
            
            var startTime = StartTimeAndProfiler(energyProfiler, stopwatch, testCase);

            var counter = RunTestcase(testCase, startTime, energyProfiler);

            var (stopTime, duration) = StopTimeAndProfiler(energyProfiler, stopwatch);

            await EnableWifiAndDependencies();

            HandleClampProfiler(startTime, stopTime, energyProfiler);

            var (endTemperatures, endBattery) = GetEndMeasurements();
            var experimentId = await EndExperiment(testCase, stopTime, startTime, counter, energyProfiler, initialTemperatures, endTemperatures, duration, initialBattery, endBattery);

            return await HandleResultsIfValid(energyProfiler, startTime, experimentId);
        }

        private void HandleClampProfiler(DateTime startTime, DateTime stopTime, IEnergyProfiler energyProfiler)
        {
            if (energyProfiler.GetName() == EProfilers.Clamp.ToString())
            {
                energyProfiler.Start(startTime);
                energyProfiler.Stop(stopTime);
            }
        }

        private (DateTime, long) StopTimeAndProfiler(IEnergyProfiler energyProfiler, Stopwatch stopwatch)
        {
            if (energyProfiler.GetName() != EWindowsProfilers.E3.ToString() && energyProfiler.GetName() != EProfilers.Clamp.ToString())
            {
                energyProfiler.Stop(DateTime.MinValue);
                Console.WriteLine("late stop");
            }

            var duration = stopwatch.ElapsedMilliseconds;
            var stopTime = DateTime.UtcNow;

            return (stopTime, duration);
        }

        private DateTime StartTimeAndProfiler(IEnergyProfiler energyProfiler, Stopwatch stopwatch, ITestCase testCase)
        {
            _logger.Information("The energy profiler {name} has started", energyProfiler.GetName());
            _logger.Information("The test case {testcase} will now run untill at least {time}", testCase.GetName(), DateTime.UtcNow.AddMinutes(Constants.DurationOfExperimentsInMinutes));

            var startTime = DateTime.UtcNow; // TODO: order of time and start profiler

            if (energyProfiler.GetName() != EProfilers.Clamp.ToString())
            {
                energyProfiler.Start(startTime);   
            }

            stopwatch.Start();
            
            return startTime;
        }

        private int RunTestcase(ITestCase testCase, DateTime startTime, IEnergyProfiler energyProfiler)
        {
            var output = "";

            var basePath = GetTestCaseBasePath();
            var testCasePath = testCase.GetExecutablePath(basePath);

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo(testCasePath, Constants.DurationOfExperimentsInMinutes.ToString());

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                
                
                output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                if (energyProfiler.GetName() == EWindowsProfilers.E3.ToString())
                {
                    energyProfiler.Stop(DateTime.MinValue);
                    Console.WriteLine("early stop");
                }
            }

            //var counter = 0;
            //    while (startTime.AddMinutes(Constants.DurationOfExperimentsInMinutes) > DateTime.UtcNow)
            //    {
            //        testCase.Run(); // TODO: Perhaps the run should return something, and use this for something (sestoft)
            //        counter += 1;
            //    }

            var counter = output.Trim().Split('\n').Last();
            return int.Parse(counter); ;
        }

        private DirectoryInfo GetTestCaseBasePath()
        {
            if (_isProd)
            {
                var basePath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.Parent;
                return basePath;
            }
            else
            {
                var basePath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent;
                return basePath;
            }

        }

        private (List<DtoMeasurement>, DtoMeasurement) GetEndMeasurements()
        {
            //_hardwareMonitorService = SystemUtils.GetHardwareMonitorService(_logger);

            _logger.Information("The experiment is done. The end temperatures will be measured.");
            var endTemperatures = _operatingSystemAdapter.GetCoreTemperatures();
            var endBattery = _experimentHandler.GetCharge();

            return (endTemperatures, endBattery);
        }

        private async Task EnableWifiAndDependencies()
        {
            (_, _operatingSystemAdapter, _, _wifiService, _experimentHandler) = _initializeOfflineDependencies();
            _logger.Information("The wifi will be enabled");
            await EnableWifi();

            _dataHandler = _initializeOnlineDependencies();
        }

        private void RunGarbageCollection()
        {
            (_operatingSystemAdapter, _dataHandler, _wifiService, _experimentHandler) = _deleteDependencies();
            _logger.Information("Running garbage collector");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private async Task<bool> HandleResultsIfValid(IEnergyProfiler profiler, DateTime date, int experimentId)
        {
            if (_operatingSystemAdapter.GetAverageCpuTemperature() < Constants.TemperatureUpperLimit)
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
            if (energyProfiler.GetName() == EProfilers.Clamp.ToString())
            {
                energyProfiler.Start(DateTime.UtcNow);
            }

            if (String.IsNullOrEmpty(_firstProfiler)) _firstProfiler = energyProfiler.GetName();

            _dataHandler.CloseConnection();
            _wifiService.Disable(_isProd);

            _logger.Information("Measuring initial cpu temperatures");
            var initialTemperatures = _operatingSystemAdapter.GetCoreTemperatures();
            var initialBattery =  _experimentHandler.GetCharge();

            return (initialTemperatures, initialBattery);
        }

        private void InitializeDependencies()
        {
            _dataHandler = _initializeOnlineDependencies();
            (_, _operatingSystemAdapter, _, _wifiService, _experimentHandler) = _initializeOfflineDependencies();
        }
    }

    public interface IExperimentService
    {
        List<int> GetProfilerCounters();
        Task<bool> RunExperiment(IEnergyProfiler energyProfiler, ITestCase testProgram);
    }
}
