using EnergyComparer.Models;
using EnergyComparer.Profilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class WindowsLaptopAdapter : IDutAdapter
    {
        private readonly bool _iterateOverProfilers;
        private readonly ILogger _logger;
        private IntelPowerGadget _intelPowerGadget = new IntelPowerGadget();


        public WindowsLaptopAdapter(bool iterateOverProfilers, ILogger logger)
        {
            _iterateOverProfilers = iterateOverProfilers;
            _logger = logger;
        }

        public IEnergyProfiler GetDefaultProfiler()
        {
            return _intelPowerGadget;
        }

        public DtoMeasurement GetCharge()
        {
            var charge = GetChargeRemaining();
            return new DtoMeasurement()
            {
                Name = "Battery charge left",
                Value = charge,
                Type = EMeasurementType.BatteryChargeLeft.ToString()
            };
        }

        public bool EnoughBattery()
        {
            var battery = GetChargeRemaining();

            var lowEnoughBattery = battery > Constants.ChargeLowerLimit && battery <= Constants.ChargeUpperLimit;

            if (!lowEnoughBattery)
                _logger.Warning("The battery is too low: {bat} (min: {min}, max: {max}). Checking again in 5 minutes", battery, Constants.ChargeLowerLimit, Constants.ChargeUpperLimit);

            return lowEnoughBattery;
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
            }



            return profilers;
        }

        public int GetChargeRemaining()
        {
            var mos = new ManagementObjectSearcher("select * from Win32_Battery");

            foreach (ManagementObject mo in mos.Get())
            {
                var chargeRemaning = mo["EstimatedChargeRemaining"].ToString();
                return int.Parse(chargeRemaning);
            }

            throw new NotImplementedException("Not battery found");
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
            else
            {
                throw new NotImplementedException($"Profiler '{name}' is not valid for system");
            }

        }
    }
}
