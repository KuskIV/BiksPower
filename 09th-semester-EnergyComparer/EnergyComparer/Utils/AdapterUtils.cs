using EnergyComparer.Models;
using EnergyComparer.Profilers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Utils
{
    public static class AdapterUtils
    {
        public static void Enable(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" enable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }

        public static void Disable(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
        }
        public static List<string> GetAllSouces()
        {
            return Enum.GetNames(typeof(EWindowsProfilers)).ToList();
        }

        public static IEnergyProfiler MapEnergyProfiler(Profiler profiler)
        {
            if (profiler.Name == EWindowsProfilers.IntelPowerGadget.ToString())
            {
                return new IntelPowerGadget();
            }
            else if (profiler.Name == EWindowsProfilers.E3.ToString())
            {
                throw new NotImplementedException("E3 has not been implemented");
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
    }
}
