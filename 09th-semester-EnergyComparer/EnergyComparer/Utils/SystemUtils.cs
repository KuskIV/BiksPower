﻿using EnergyComparer.DUTs;
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
        public static IOperatingSystemAdapter InitializeAdapterService(ILogger logger, bool isProd, bool shouldRestart)
        {
            if (Constants.Os == "Win32NT")
            {
                logger.Information("Executing on a {os} machine, the {name} is used.", Constants.Os, "WindowsAdapter");
                return new WindowsAdapter(logger, isProd, shouldRestart);
            }
            else
            {
                logger.Information("Executing on a {os} machine, the {name} is used.", Constants.Os, "LinuxAdapter");
                return new LinuxAdapter();
            }
        }

        public static IDutAdapter GetDutAdapter(ILogger logger, bool hasBattery, bool iterateOverProfilers)
        {
            if (Constants.Os == "Win32NT")
            {
                if (hasBattery)
                {
                    logger.Information("The DUT is and '{dut}'", "WindowsLaptopAdapter");
                    return new WindowsLaptopAdapter(iterateOverProfilers, logger);
                }
                else
                {
                    logger.Information("The DUT is and '{dut}'", "WindowsDesktopAdapter");
                    return new WindowsDesktopAdapter(iterateOverProfilers, logger);
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

        internal static bool IsValidNameForProd(string machineName)
        {
            return machineName == "Surface4Pro" || 
                   machineName == "SurfaceBook" || 
                   machineName == "PowerKomplett";
        }
    }
}
