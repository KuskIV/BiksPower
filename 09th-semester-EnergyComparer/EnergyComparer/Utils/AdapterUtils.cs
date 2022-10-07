using Autofac.Core.Activators.Reflection;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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

        public static bool ShouldStopExperiment()
        {
            
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_Battery");

            if (mos == null)
            {
                // TODO: ensure this is what happens if there is not battery.
            }
            else
            {
                foreach (ManagementObject mo in mos.Get())
                {
                    var chargeRemaning = (int)mo["EstimatedChargeRemaining"];
                    return chargeRemaning < Constants.ChargeLimit;
                }
            }

            return true;
        }

        public static void Restart(bool _isProd)
        {
            if (true)
            {
                Process.Start("ShutDown", "/r /t 0");
            }
        }

        public static IProgram GetProgram(IDataHandler dataHandler)
        {
            return new TestProgram(dataHandler);
        }

        public static IEnergyProfiler MapEnergyProfiler(Profiler profiler)
        {
            if (profiler.Name == EWindowsProfilers.IntelPowerGadget.ToString())
            {
                return new IntelPowerGadget();
            }
            else if (profiler.Name == EWindowsProfilers.E3.ToString())
            {
                return new E3();
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
