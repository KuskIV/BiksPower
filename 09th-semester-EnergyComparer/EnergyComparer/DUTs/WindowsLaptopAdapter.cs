using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
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
            //return new HardwareMonitor(_hardwareMonitorService);
            //return new RAPL();
            return _intelPowerGadget;
            //return new E3();
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            profilers.Add(_intelPowerGadget);
            //profilers.Add(new E3());
            profilers.Add(new HardwareMonitor(_hardwareMonitorService));
            
            return profilers;
        }

        public int GetChargeRemaining()
        {
            var mos = new ManagementObjectSearcher("select * from Win32_Battery");

            foreach (ManagementObject mo in mos.Get())
            {
                var chargeRemaning = mo["EstimatedChargeRemaining"].ToString();
                return int.Parse(chargeRemaning);
            }

            throw new NotImplementedException("Not battery found");
        }

        public List<string> GetAllSoucres()
        {
            return Enum.GetNames(typeof(EWindowsProfilers)).ToList();
        }
    }
}
