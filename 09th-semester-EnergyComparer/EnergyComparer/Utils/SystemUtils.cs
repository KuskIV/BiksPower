using EnergyComparer.DUTs;
using EnergyComparer.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Utils
{
    public class SystemUtils
    {
        public static IOperatingSystemAdapter InitializeAdapterService(ILogger logger, bool isProd, bool shouldRestart, IHardwareMonitorService hardwareMonitorServic)
        {
            if (Constants.Os == "Win32NT")
            {
                logger.Information("Executing on a {os} machine, the {name} is used.", Constants.Os, "WindowsAdapter");
                return new WindowsAdapter(logger, isProd, shouldRestart, hardwareMonitorServic);
            }
            else
            {
                logger.Information("Executing on a {os} machine, the {name} is used.", Constants.Os, "LinuxAdapter");
                return new LinuxAdapter(logger);
            }
        }

        public static IDutAdapter GetDutAdapter(ILogger logger, bool hasBattery, bool iterateOverProfilers, IHardwareMonitorService hardwareMonitorService)
        {
            if (Constants.Os == "Win32NT")
            {
                if (hasBattery)
                {
                    logger.Information("The DUT is and '{dut}'", "WindowsLaptopAdapter");
                    return new WindowsLaptopAdapter(iterateOverProfilers, logger, hardwareMonitorService);
                }
                else
                {
                    logger.Information("The DUT is and '{dut}'", "WindowsDesktopAdapter");
                    return new WindowsDesktopAdapter(iterateOverProfilers, logger, hardwareMonitorService);
                }
            }
            else
            {
                if (hasBattery)
                {
                    logger.Information("The DUT is and '{dut}'", "LinuxLaptopAdapter");
                    return new LinuxLaptopAdapter();
                }
                else
                {
                    logger.Information("The DUT is and '{dut}'", "LinuxDesktopAdapter");
                    return new LinuxDesktopAdapter();
                }
            }
        }

        public static IHardwareMonitorService GetHardwareMonitorService(ILogger logger)
        {
            if (Constants.Os == "Win32NT")
            {
                return new HardwareMonitorService(logger);
            }

            return null;
        }

        internal static bool IsValidNameForProd(string machineName)
        {
            return machineName == "Surface4Pro" || 
                   machineName == "SurfaceBook" || 
                   machineName == "PowerKomplett";
        }
    }
}
