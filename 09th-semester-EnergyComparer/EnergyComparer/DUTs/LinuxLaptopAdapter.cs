using EnergyComparer.Models;
using EnergyComparer.Profilers;
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
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    //FileName = "/bin/bash",
                    FileName = "/bin/cat",
                    Arguments = "/sys/class/power_supply/BAT1/capacity",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            var output = "";

            proc.Start();

            var reader = proc.StandardOutput;

            output = reader.ReadToEnd();

            proc.WaitForExit();

            return int.Parse(output.Trim());
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
