using EnergyComparer.DUTs;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Services;
using EnergyComparer.TestCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Handlers
{
    public class ExperimentHandler : IExperimentHandler
    {
        private readonly bool _isProd;
        private readonly int _maxIterations;
        private readonly bool _iterateOverProfilers;
        private readonly ILogger _logger;
        private readonly IDutAdapter _dutAdapter;
        private readonly IOperatingSystemAdapter _operatingSystemAdapter;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private bool _hasBattery;

        public ExperimentHandler(bool isProd, int maxIterations, bool hasBattery, bool iterateOverProfilers, ILogger logger, IDutAdapter dutAdapter, IOperatingSystemAdapter operatingSystemAdapter, IHardwareMonitorService hardwareMonitorService)
        {
            _isProd = isProd;
            _maxIterations = maxIterations;
            _hasBattery = hasBattery;
            _iterateOverProfilers = iterateOverProfilers;
            _logger = logger;
            _dutAdapter = dutAdapter;
            _operatingSystemAdapter = operatingSystemAdapter;
            _hardwareMonitorService = hardwareMonitorService;
        }

        public DtoMeasurement GetCharge()
        {
            var charge = _dutAdapter.GetChargeRemaining();

            if (!_hasBattery)
                return null;

            return new DtoMeasurement()
            {
                Name = "Battery charge left",
                Value = charge,
                Type = EMeasurementType.BatteryChargeLeft.ToString()
            };
        }

        public bool HasMaxBattery()
        {
            if (!_hasBattery)
                return true;

            var chargeRemaining = _dutAdapter.GetChargeRemaining();

            return chargeRemaining >= Constants.ChargeUpperLimit;
        }

        public bool EnoughBattery()
        {
            if (!_hasBattery)
                return true;

            var battery = _dutAdapter.GetChargeRemaining();

            var enoughBattery = battery > Constants.ChargeLowerLimit;// && battery <= Constants.ChargeUpperLimit;

            if (!enoughBattery)
                _logger.Warning("The battery is too low: {bat} (min: {min}, max: {max}). Checking again in 5 minutes", battery, Constants.ChargeLowerLimit, Constants.ChargeUpperLimit);

            return enoughBattery;
        }

        public List<string> GetAllRequiredPaths()
        {
            return _dutAdapter.GetAllSoucres().Select(x => Constants.GetPathForSource(x)).ToList();
        }

        public bool ShouldStopExperiment()
        {
            return !EnoughBattery() || !LowEnoughCpuTemperature();
        }

        public void CreateFolder(string folder)
        {
            _logger.Information("About to create folder {folder}", folder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public async Task WaitTillStableState()
        {
            if (!_isProd)
            {
                _logger.Information("Experiment is not in prod, so will not wait for stable condition");
                return;
            }

            _logger.Information("Waiting for stable condition");

            while (!HasMaxBattery() || !LowEnoughCpuTemperature())
            {
                _logger.Information("Waiting for battery to be above {upperBattery} ({currentBattery}) and temperature to be below {upperTemperature} ({currentTemperature})",
                    Constants.ChargeUpperLimit, GetCharge().Value, Constants.TemperatureUpperLimit, _hardwareMonitorService.GetAverageCpuTemperature());
                _logger.Information("retrying in 5 minutes");
                await Task.Delay(TimeSpan.FromMinutes(5));
            }

            _logger.Information("Stable condition has been reached");
        }

        public async Task<ITestCase> GetTestCase(IDataHandler dataHandler)
        {
            List<IEnergyProfiler> profilers = GetEnergyProfilers();
            var dtoProfilers = await GetDtoProfilers(profilers, dataHandler);
            var dut = await dataHandler.GetDut();
            _logger.Information("The profiler(s) for the experiment are as following: {profilers}", string.Join(',', profilers.Select(x => x.GetName())));

            if (!_isProd)
            {
                return new IdleCase(dataHandler);
            }

            var idleCase = new IdleCase(dataHandler);
            if (!await AllProfilersExecutedEnough(dataHandler, dut, idleCase, dtoProfilers))
                return idleCase;
            
            var diningPhilosiphers = new DiningPhilosophers(dataHandler);
            if (!await AllProfilersExecutedEnough(dataHandler, dut, diningPhilosiphers, dtoProfilers))
                return diningPhilosiphers;

            _operatingSystemAdapter.Shutdowm();
            throw new Exception("The computer should have shut down by now.");
        }

        private List<IEnergyProfiler> GetEnergyProfilers()
        {
            if (!_iterateOverProfilers)
            {
                return new List<IEnergyProfiler>()
                {
                    _dutAdapter.GetDefaultProfiler(),
                };
            }

            return _dutAdapter.GetProfilers().ToList();
        }

        private bool LowEnoughCpuTemperature()
        {
            var avgTemp = _hardwareMonitorService.GetAverageCpuTemperature();

            var isTempLowEnough = avgTemp > Constants.TemperatureLowerLimit && avgTemp <= Constants.TemperatureUpperLimit;

            if (!isTempLowEnough)
                _logger.Warning("The temperature is too high: {temp} (min: {min}, max: {max}). Checking again in 5 minutes", avgTemp, Constants.TemperatureLowerLimit, Constants.TemperatureUpperLimit);

            return isTempLowEnough;
        }

        private async Task<List<DtoProfiler>> GetDtoProfilers(List<IEnergyProfiler> profilers, IDataHandler dataHandler)
        {
            var dtoProfilers = new List<DtoProfiler>();

            foreach (var p in profilers)
                dtoProfilers.Add(await dataHandler.GetProfiler(p));

            return dtoProfilers;
        }

        private async Task<bool> AllProfilersExecutedEnough(IDataHandler dataHandler, DtoDut dut, ITestCase testCase, List<DtoProfiler> dtoProfilers)
        {
            foreach (var p in dtoProfilers)
            {
                int experimentsRunOnSetup = await dataHandler.ExperimentsRunOnCurrentSetup(testCase.GetName(), p, dut, testCase.GetLanguage());
                if (_maxIterations > experimentsRunOnSetup)
                {
                    _logger.Information("For test case {name}, profiler {p} has run {min}/{max} runs.", testCase.GetName(), p.Name, experimentsRunOnSetup, _maxIterations);
                    return false;
                }
                else
                {
                    _logger.Information("For test case {name}, profiler {p} has already run {max} ({min})", testCase.GetName(), p.Name, _maxIterations, experimentsRunOnSetup);
                }
            }

            return true;
        }
    }
}
