using EnergyComparer.Models;
using EnergyComparer.Profilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.DUTs
{
    internal class LinuxDesktopAdapter : IDutAdapter
    {
        public List<string> GetAllSoucres()
        {
            return Enum.GetNames(typeof(ELinuxProfilers)).ToList().Concat(Enum.GetNames(typeof(EProfilers)).ToList()).ToList();
        }

        public int GetChargeRemaining()
        {
            return 100;
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return new RAPL();
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            profilers.Add(new RAPL());
            profilers.Add(new Clamp());

            return profilers;
        }
    }
}
