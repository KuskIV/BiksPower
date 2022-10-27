using CsvHelper.Configuration;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Handlers
{
    public class IntelPowerGadgetTimeSeriesMapper : ClassMap<IntelPowerGadgetTimeSeires>
    {
        public IntelPowerGadgetTimeSeriesMapper()
        {
            Map(m => m.SystemTime).Name("System Time");
            Map(m => m.RDTSC).Name("RDTSC");
            Map(m => m.ElapsedTimesec).Name("Elapsed Time (sec)");
            Map(m => m.CPUUtilization).Name(" CPU Utilization(%)");
            Map(m => m.CPUFrequency_0MHz).Name("CPU Frequency_0(MHz)");
            Map(m => m.ProcessorPower_0Watt).Name("Processor Power_0(Watt)");
            Map(m => m.CumulativeProcessorEnergy_0Joules).Name("Cumulative Processor Energy_0(Joules)");
            Map(m => m.CumulativeProcessorEnergy_0mWh).Name("Cumulative Processor Energy_0(mWh)");
            Map(m => m.IAPower_0Watt).Name("IA Power_0(Watt)");
            Map(m => m.CumulativeIAEnergy_0Joules).Name("Cumulative IA Energy_0(Joules)");
            Map(m => m.CumulativeIAEnergy_0mWh).Name("Cumulative IA Energy_0(mWh)");
            Map(m => m.PackageTemperature_0C).Name("Package Temperature_0(C)");
            Map(m => m.PackageHot_0).Name("Package Hot_0");
            Map(m => m.DRAMPower_0Watt).Name("DRAM Power_0(Watt)");
            Map(m => m.CumulativeDRAMEnergy_0Joules).Name("Cumulative DRAM Energy_0(Joules)");
            Map(m => m.CumulativeDRAMEnergy_0mWh).Name("Cumulative DRAM Energy_0(mWh)");
            Map(m => m.GTPower_0Watt).Name("GT Power_0(Watt)");
            Map(m => m.CumulativeGTEnergy_0Joules).Name("Cumulative GT Energy_0(Joules)");
            Map(m => m.CumulativeGTEnergy_0mWh).Name("Cumulative GT Energy_0(mWh)");
            Map(m => m.PackagePL1_0Watt).Name("Package PL1_0(Watt)");
            Map(m => m.PackagePL2_0Watt).Name("Package PL2_0(Watt)");
            Map(m => m.PackagePL4_0Watt).Name("Package PL4_0(Watt)");
            Map(m => m.PlatformPsysPL1_0Watt).Name("Platform PsysPL1_0(Watt)");
            Map(m => m.PlatformPsysPL2_0Watt).Name("Platform PsysPL2_0(Watt)");
            Map(m => m.GTFrequencyMHz).Name("GT Frequency(MHz)");
            Map(m => m.GTUtilization).Name("GT Utilization(%)");
        }
    }
}
