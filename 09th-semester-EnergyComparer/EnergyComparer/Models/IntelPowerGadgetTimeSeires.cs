using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class IntelPowerGadgetTimeSeires
    {
        public string SystemTime { get; set; }
        public string RDTSC { get; set; }
        public string ElapsedTimesec { get; set; }
        public string CPUUtilization { get; set; }
        public string CPUFrequency_0MHz { get; set; }
        public string ProcessorPower_0Watt { get; set; }
        public string CumulativeProcessorEnergy_0Joules { get; set; }
        public string CumulativeProcessorEnergy_0mWh { get; set; }
        public string IAPower_0Watt { get; set; }
        public string CumulativeIAEnergy_0Joules { get; set; }
        public string CumulativeIAEnergy_0mWh { get; set; }
        public string PackageTemperature_0C { get; set; }
        public string PackageHot_0 { get; set; }
        public string DRAMPower_0Watt { get; set; }
        public string CumulativeDRAMEnergy_0Joules { get; set; }
        public string CumulativeDRAMEnergy_0mWh { get; set; }
        public string GTPower_0Watt { get; set; }
        public string CumulativeGTEnergy_0Joules { get; set; }
        public string CumulativeGTEnergy_0mWh { get; set; }
        public string PackagePL1_0Watt { get; set; }
        public string PackagePL2_0Watt { get; set; }
        public string PackagePL4_0Watt { get; set; }
        public string PlatformPsysPL1_0Watt { get; set; }
        public string PlatformPsysPL2_0Watt { get; set; }
        public int GTFrequencyMHz { get; set; }
        public string GTUtilization { get; set; }
    }
}
