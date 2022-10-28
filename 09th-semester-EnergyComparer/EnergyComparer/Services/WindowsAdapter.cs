using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.TestCases;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class WindowsAdapter : IOperatingSystemAdapter
    {
        private readonly IDutAdapter _dutAdapter;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly ILogger _logger;
        private readonly bool _hasBattery;
        private readonly bool _isProd;
        private readonly bool _shouldRestart;
        private readonly int _maxIterations;

        public WindowsAdapter(IDutAdapter dutAdapter, IHardwareMonitorService hardwareMonitorService, ILogger logger, bool hasBattery, bool isProd, bool shouldRestart, int maxIterations)
        {
            _dutAdapter = dutAdapter;
            _hardwareMonitorService = hardwareMonitorService;
            _logger = logger;
            _hasBattery = hasBattery;
            _isProd = isProd;
            _shouldRestart = shouldRestart;
            _maxIterations = maxIterations;
        }

        public void EnableWifi(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" enable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        public void DisableWifi(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        public void CreateFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public List<string> GetAllSouces()
        {
            return Enum.GetNames(typeof(EWindowsProfilers)).ToList();
        }

        public bool ShouldStopExperiment()
        {
            var chargeRemaining = _dutAdapter.GetChargeRemaining();
            
            return chargeRemaining < Constants.ChargeLowerLimit;
        }

        public List<string> GetAllRequiredPaths()
        {
            return GetAllSouces().Select(x => Constants.GetPathForSource(x)).ToList();
        }

        public void Restart()
        {
            if (_shouldRestart)
            {
                Process.Start("ShutDown", "/r /t 0");
            }
        }

        public void Shutdowm()
        {
            if (_isProd)
            {
                Process.Start("ShutDown", "/s /t 0");
            }
        }

        public async Task<ITestCase> GetTestCase(IDataHandler dataHandler)
        {
            var profilers = _dutAdapter.GetProfilers().ToList();
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

            Shutdowm();
            throw new Exception("The computer should have shut down by now.");
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

        private async Task<List<DtoProfiler>> GetDtoProfilers(List<IEnergyProfiler> profilers, IDataHandler dataHandler)
        {
            var dtoProfilers = new List<DtoProfiler>();

            foreach (var p in profilers)
                dtoProfilers.Add(await dataHandler.GetProfiler(p));

            return dtoProfilers;
        }

        public IEnergyProfiler MapEnergyProfiler(Profiler profiler)
        {
            return _dutAdapter.GetProfilers(profiler.Name);

            //if (profiler.Name == EWindowsProfilers.IntelPowerGadget.ToString())
            //{
            //    return new IntelPowerGadget();
            //}
            //else if (profiler.Name == EWindowsProfilers.E3.ToString())
            //{
            //    return new E3();
            //}
            //else if (profiler.Name == EWindowsProfilers.HardwareMonitor.ToString())
            //{
            //    return new HardwareMonitor();
            //}
            //else if (profiler.Name == EProfilers.Clam.ToString())
            //{
            //    return new Clam();
            //}
            //else
            //{
            //    throw new NotImplementedException($"{profiler.Name} has not been implemented");
            //}
        }

        public async Task WaitTillStableState()
        {
            if (!_isProd)
            {
                _logger.Information("Experiment is not in prod, so will not wait for stable condition");
                return;
            }

            while (!_dutAdapter.EnoughBattery() || !LowEnoughCpuTemperature())
                await Task.Delay(TimeSpan.FromMinutes(5));

            _logger.Information("Stable condition has been reached");
        }

        private bool LowEnoughCpuTemperature()
        {
            var avgTemp = _hardwareMonitorService.GetAverageCpuTemperature();

            var isTempLowEnough = avgTemp > Constants.TemperatureLowerLimit && avgTemp <= Constants.TemperatureUpperLimit;

            if (!isTempLowEnough)
                _logger.Warning("The temperature is too high: {temp} (min: {min}, max: {max}). Checking again in 5 minutes", avgTemp, Constants.TemperatureLowerLimit, Constants.TemperatureUpperLimit);

            return isTempLowEnough;
        }

        public void StopunneccesaryProcesses()
        {
            if (_isProd)
            {
                _logger.Information("About to disable background processes");
                var ps = PowerShell.Create();

                foreach (var process in WindowsProcessesToStop())
                {
                    try
                    {
                        ps.AddCommand("Stop-Process").AddParameter("Name", process);
                        ps.Invoke();
                    }
                    catch (Exception)
                    {
                        _logger.Warning("Unable to stop process '{name}'. Will continue...", process);
                    }
                }
            }
            else
            {
                _logger.Information("No background processes will be disabled as it is dev");
            }
        }

        public static List<string> WindowsProcessesToStop()
        {
            return new List<string>()
            {
                "AsusUpdateCheck",
                "AsusDownLoadLicense",
                "msedge",
                "OneDrive",
                "GitHubDesktop",
                "Microsoft.Photos",
                "SkypeApp",
                "SkypeBackgroundHost",
            };
        }
    }
}
