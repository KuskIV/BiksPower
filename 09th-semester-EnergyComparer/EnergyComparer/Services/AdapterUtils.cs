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

        public AdapterWindowsLaptopService(IHardwareMonitorService hardwareMonitorService, ILogger logger)
        {
            _hardwareMonitorService = hardwareMonitorService;
            _logger = logger;
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
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_Battery");

            foreach (ManagementObject mo in mos.Get())
            {
                var chargeRemaning = mo["EstimatedChargeRemaining"].ToString();
                return int.Parse(chargeRemaning);
            }

            return 100;
        }

        public void Restart(bool _isProd)
        {
            if (_isProd)
            {
                Process.Start("ShutDown", "/r /t 0");
            }
        }

        public IProgram GetProgram(IDataHandler dataHandler)
        {
            return new TestProgram(dataHandler);
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

        public async Task WaitTillStableState(bool isProd)
        {
            if (!isProd)
            {
                return;
            }

            while (GetChargeRemaining() < Constants.ChargeUpperLimit || _hardwareMonitorService.GetAverageCpuTemperature() > Constants.TemperatureLowerLimit)
                await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }
}
