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
        [Index(0)]
        public string SystemTime { get; set; }
        [Index(1)]
        public string RDTSC { get; set; }
        [Index(2)]
        public string ElapsedTimesec { get; set; }
        [Index(3)]
        public string CPUUtilization { get; set; }
        [Index(4)]
        public string CPUFrequency_0MHz { get; set; }
        [Index(5)]
        public string ProcessorPower_0Watt { get; set; }
        [Index(6)]
        public string CumulativeProcessorEnergy_0Joules { get; set; }
        [Index(7)]
        public string CumulativeProcessorEnergy_0mWh { get; set; }
        [Index(8)]
        public string IAPower_0Watt { get; set; }
        [Index(9)]
        public string CumulativeIAEnergy_0Joules { get; set; }
        [Index(10)]
        public string CumulativeIAEnergy_0mWh { get; set; }
        [Index(11)]
        public string PackageTemperature_0C { get; set; }
        [Index(12)]
        public string PackageHot_0 { get; set; }
        [Index(13)]
        public string DRAMPower_0Watt { get; set; }
        [Index(14)]
        public string CumulativeDRAMEnergy_0Joules { get; set; }
        [Index(15)]
        public string CumulativeDRAMEnergy_0mWh { get; set; }
        [Index(16)]
        public string GTPower_0Watt { get; set; }
        [Index(17)]
        public string CumulativeGTEnergy_0Joules { get; set; }
        [Index(18)]
        public string CumulativeGTEnergy_0mWh { get; set; }
        [Index(19)]
        public string PackagePL1_0Watt { get; set; }
        [Index(20)]
        public string PackagePL2_0Watt { get; set; }
        [Index(21)]
        public string PackagePL4_0Watt { get; set; }
        [Index(22)]
        public string PlatformPsysPL1_0Watt { get; set; }
        [Index(23)]
        public string PlatformPsysPL2_0Watt { get; set; }
        [Index(24)]
        public int GTFrequencyMHz { get; set; }
        [Index(25)]
        public string GTUtilization { get; set; }
    }
}
