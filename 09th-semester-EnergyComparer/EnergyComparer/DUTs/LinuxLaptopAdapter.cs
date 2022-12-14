using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.DUTs
{
    public class LinuxLaptopAdapter : IDutAdapter
    {
        public List<string> GetAllSoucres()
        {
            return Enum.GetNames(typeof(ELinuxProfilers)).ToList().ToList();

        }

        public int GetChargeRemaining()
        {
            return LinuxUtils.ExecuteCommandGetOutput("/bin/cat", "/sys/class/power_supply/BAT1/capacity");
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return new RAPL();
        }

        public List<IEnergyProfiler> GetProfilers()
        {
            var profilers = new List<IEnergyProfiler>();

            profilers.Add(new RAPL());

            return profilers;
        }
    }
}
