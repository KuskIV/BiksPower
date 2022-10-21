using Autofac.Core.Activators.Reflection;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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

        public AdapterWindowsLaptopService(IHardwareMonitorService hardwareMonitorService, ILogger logger, IConfiguration configuration, bool hasBattery, bool isProd, bool shouldRestart)
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

        public ISoftwareEntity GetSoftwareEntity(IDataHandler dataHandler)
        {
            return new DiningPhilosophers(dataHandler);
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
                return;
            }

            while (GetChargeRemaining() < Constants.ChargeUpperLimit || _hardwareMonitorService.GetAverageCpuTemperature() > Constants.TemperatureLowerLimit)
                await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }
}
