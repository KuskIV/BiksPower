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
    public class AdapterWindowsLaptopService : IAdapterService
    {
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly ILogger _logger;
        private readonly bool _hasBattery;
        private readonly bool _isProd;
        private readonly bool _shouldRestart;

        public AdapterWindowsLaptopService(IHardwareMonitorService hardwareMonitorService, ILogger logger, bool hasBattery, bool isProd, bool shouldRestart)
        {
            _hardwareMonitorService = hardwareMonitorService;
            _logger = logger;
            _hasBattery = hasBattery;
            _isProd = isProd;
            _shouldRestart = shouldRestart;
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
            var chargeRemaining = GetChargeRemaining();
            
            return chargeRemaining < Constants.ChargeLowerLimit;
        }

        public List<string> GetAllRequiredPaths()
        {
            return GetAllSouces().Select(x => Constants.GetPathForSource(x)).ToList();
        }

        public DtoMeasurement GetCharge()
        {
            var charge = -1;

            if (_hasBattery)
            {
                charge = GetChargeRemaining();
            }

            return new DtoMeasurement()
            {
                Name = "Battery charge left",
                Value = charge,
                Type = EMeasurementType.BatteryChargeLeft.ToString()
            };
        }

        private int GetChargeRemaining()
        {
            if (!_hasBattery) return 100;

            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_Battery");

            foreach (ManagementObject mo in mos.Get())
            {
                var chargeRemaning = mo["EstimatedChargeRemaining"].ToString();
                return int.Parse(chargeRemaning);
            }

            throw new NotImplementedException("Not battery found");
        }

        public void Restart()
        {
            if (_shouldRestart)
            {
                Process.Start("ShutDown", "/r /t 0");
            }
        }

        public ITestCase GetTestCase(IDataHandler dataHandler)
        {
            return new IdleCase(dataHandler);
            //return new DiningPhilosophers(dataHandler);
        }

        public IEnergyProfiler MapEnergyProfiler(Profiler profiler)
        {
            if (profiler.Name == EWindowsProfilers.IntelPowerGadget.ToString())
            {
                return new IntelPowerGadget();
            }
            else if (profiler.Name == EWindowsProfilers.E3.ToString())
            {
                return new E3();
            }
            else if (profiler.Name == EWindowsProfilers.HardwareMonitor.ToString())
            {
                throw new NotImplementedException("HardwareMonitor has not been implemented");
            }
            else
            {
                throw new NotImplementedException($"{profiler.Name} has not been implemented");
            }
        }

        public async Task WaitTillStableState()
        {
            if (!_isProd)
            {
                _logger.Information("Experiment is not in prod, so will not wait for stable condition");
                return;
            }

            while (!EnoughBattery() || !LowEnoughCpuTemperature())
                await Task.Delay(TimeSpan.FromMinutes(5));

            _logger.Information("Stable condition has been reached");
        }

        private bool EnoughBattery()
        {
            if (!_hasBattery) return true;

            var battery = GetChargeRemaining();

            var lowEnoughBattery = battery > Constants.ChargeLowerLimit && battery <= Constants.ChargeUpperLimit;

            if (!lowEnoughBattery)
                _logger.Warning("The battery is too low: {bat} (min: {min}, max: {max}). Checking again in 5 minutes", battery, Constants.ChargeLowerLimit, Constants.ChargeUpperLimit);

            return lowEnoughBattery;
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
