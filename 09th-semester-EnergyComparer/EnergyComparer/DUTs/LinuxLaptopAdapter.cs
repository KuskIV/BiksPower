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
        
        public float GetTemperature()
        {
            var temperature = LinuxUtils.ExecuteCommandGetOutput("/bin/cat", "/sys/class/thermal/thermal_zone5/temp");

            return temperature / 1000;
        }

        public void DisableNetworking(string interfaceName)
        {
            LinuxUtils.ExecuteCommand("/bin/nmcli", "radio wifi off");
        }

        public void EnableNetworking(string interfaceName)
        {
            LinuxUtils.ExecuteCommand("/bin/nmcli", "radio wifi on");
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

        public IEnergyProfiler GetProfilers(string name)
        {
            if (name == ELinuxProfilers.RAPL.ToString())
            {
                return new RAPL();
            }
            else
            {
                throw new NotImplementedException($"Profiler '{name}' is not valid for system");
            }
        }
    }
}
