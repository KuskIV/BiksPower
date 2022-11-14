using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.DUTs
{
    public class WindowsLaptopAdapter : IDutAdapter
    {
        private readonly bool _iterateOverProfilers;
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private IntelPowerGadget _intelPowerGadget = new IntelPowerGadget();


        public WindowsLaptopAdapter(bool iterateOverProfilers, ILogger logger, IHardwareMonitorService hardwareMonitorService)
        {
            _iterateOverProfilers = iterateOverProfilers;
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return new HardwareMonitor(_hardwareMonitorService);
            //return new RAPL();
            //return _intelPowerGadget;
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            profilers.Add(_intelPowerGadget);
            profilers.Add(new E3());
            profilers.Add(new HardwareMonitor(_hardwareMonitorService));
            
            return profilers;
        }

        public int GetChargeRemaining()
        {
            var mos = new ManagementObjectSearcher("select * from Win32_Battery");
            var batteries = new List<int>();

            foreach (ManagementObject mo in mos.Get())
            {
                var chargeRemaning = mo["EstimatedChargeRemaining"].ToString();
                batteries.Add(int.Parse(chargeRemaning));
            }

            if (batteries.Count() > 0)
            {
                return batteries.Sum() / batteries.Count();
            }

            throw new NotImplementedException("Not battery found");
        }

        public IEnergyProfiler GetProfilers(string name)
        {

            if (name == EWindowsProfilers.IntelPowerGadget.ToString())
            {
                return _intelPowerGadget;
            }
            else if (name == EWindowsProfilers.HardwareMonitor.ToString())
            {
                return new HardwareMonitor(_hardwareMonitorService);
            }
            else if (name == EWindowsProfilers.E3.ToString())
            {
                return new E3();
            }
            else
            {
                throw new NotImplementedException($"Profiler '{name}' is not valid for system");
            }

        }

        public float GetTemperature()
        {
            return _hardwareMonitorService.GetAverageCpuTemperature(update:true);
        }

        public void DisableNetworking(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        public void EnableNetworking(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        public void DisableNetworking()
        {
            throw new NotImplementedException();
        }

        public void EnableNetworking()
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllSoucres()
        {
            return Enum.GetNames(typeof(EWindowsProfilers)).ToList();
        }
    }
}
