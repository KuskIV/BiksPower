using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class IntelPowerGadgetData
    {
        public TimeSpan SystemTime { get; set; }
        public double RDTSC { get; set; }
        public double ElapsedTimesec { get; set; }
        public double CPUUtilization { get; set; }
        public double CPUFrequency_0MHz { get; set; }
        public double ProcessorPower_0Watt { get; set; }
        public double CumulativeProcessorEnergy_0Joules { get; set; }
        public double CumulativeProcessorEnergy_0mWh { get; set; }
        public double IAPower_0Watt { get; set; }
        public double CumulativeIAEnergy_0Joules { get; set; }
        public double CumulativeIAEnergy_0mWh { get; set; }
        public double PackageTemperature_0C { get; set; }
        public double PackageHot_0 { get; set; }
        public double DRAMPower_0Watt { get; set; }
        public double CumulativeDRAMEnergy_0Joules { get; set; }
        public double CumulativeDRAMEnergy_0mWh { get; set; }
        public double GTPower_0Watt { get; set; }
        public double CumulativeGTEnergy_0Joules { get; set; }
        public double CumulativeGTEnergy_0mWh { get; set; }
        public double PackagePL1_0Watt { get; set; }
        public double PackagePL2_0Watt { get; set; }
        public double PackagePL4_0Watt { get; set; }
        public double PlatformPsysPL1_0Watt { get; set; }
        public double PlatformPsysPL2_0Watt { get; set; }
        public double GTFrequencyMHz { get; set; }
        public double GTUtilization { get; set; }
        public double TotalElapsedTimeInSeconds { get; set; }
        public double MeasuredRdtscFrequencyInGhz { get; set; }
        public double AverageProcessorPower0InWatt { get; set; }
        public double AverageIAPower0InWatt { get; set; }
        public double AverageDRAMPower0InWatt { get; set; }
        public double AverageGTPower0InWatt { get; set; }
    }
}
