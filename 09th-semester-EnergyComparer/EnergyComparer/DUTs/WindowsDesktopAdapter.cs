using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.DUTs
{
    internal class WindowsDesktopAdapter : IDutAdapter
    {
        private readonly bool _iterateOverProfilers;
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private IntelPowerGadget _intelPowerGadget = new IntelPowerGadget();

        public WindowsDesktopAdapter(bool iterateOverProfilers, ILogger logger, IHardwareMonitorService hardwareMonitorService)
        {
            _iterateOverProfilers = iterateOverProfilers;
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return new  Clamp();
        }

        public int GetChargeRemaining()
        {
            return 100;
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            profilers.Add(_intelPowerGadget);
            //profilers.Add(new E3());
            profilers.Add(new HardwareMonitor(_hardwareMonitorService));
            profilers.Add(new Clamp());

            return profilers;
        }

        public List<string> GetAllSoucres()
        {
            return Enum.GetNames(typeof(EWindowsProfilers)).ToList().Concat(Enum.GetNames(typeof(EProfilers)).ToList()).ToList();
        }
    }
}
