using EnergyComparer.Models;
using EnergyComparer.Profilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
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

        public bool EnoughBattery()
        {
            return true;
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return _intelPowerGadget;
        }

        public DtoMeasurement GetCharge()
        {
            return null;
        }

        public int GetChargeRemaining()
        {
            return 100;
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            if (!_iterateOverProfilers)
            {
                profilers.Add(_intelPowerGadget);
            }
            else
            {
                profilers.Add(_intelPowerGadget);
                profilers.Add(new E3());
                profilers.Add(new HardwareMonitor());
                profilers.Add(new Clam());
            }

            return profilers;
        }

        public IEnergyProfiler GetProfilers(string name)
        {
            throw new NotImplementedException();
        }
    }
}
