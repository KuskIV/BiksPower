using EnergyComparer.Models;
using EnergyComparer.Profilers;
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
        private IntelPowerGadget _intelPowerGadget = new IntelPowerGadget();

        public WindowsDesktopAdapter(bool iterateOverProfilers, ILogger logger)
        {
            _iterateOverProfilers = iterateOverProfilers;
            _logger = logger;
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return _intelPowerGadget;
        }

        public int GetChargeRemaining()
        {
            return 100;
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            profilers.Add(_intelPowerGadget);
            profilers.Add(new E3());
            profilers.Add(new HardwareMonitor());
            profilers.Add(new Clamp());

            return profilers;
        }

        public IEnergyProfiler GetProfilers(string name)
        {
            if (name == EWindowsProfilers.IntelPowerGadget.ToString())
            {
                return _intelPowerGadget;
            }
            else if (name == EWindowsProfilers.HardwareMonitor.ToString())
            {
                return new HardwareMonitor();
            }
            else if (name == EWindowsProfilers.E3.ToString())
            {
                return new E3();
            }
            else if (name == EProfilers.Clamp.ToString())
            {
                return new Clamp();
            }
            else
            {
                throw new NotImplementedException($"Profiler '{name}' is not valid for system");
            }
        }

        public List<string> GetAllSoucres()
        {
            return Enum.GetNames(typeof(EWindowsProfilers)).ToList().Concat(Enum.GetNames(typeof(EProfilers)).ToList()).ToList();

        }
    }
}
