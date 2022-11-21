using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class IntelPowerGadgetData
    {
        public float TotalElapsedTimeInSeconds { get; set; } = -1;
        public float MeasuredRdtscFrequencyInGhz { get; private set; } = -1;
        public float CumulativeProcessorEnergyInJoules { get; private set; } = -1;
        public float CumulativeProcessorEnergyInMwh { get; private set; } = -1;
        public float AverageProcessorPowerWatt { get; private set; } = -1;
        public float CumulativeIaEnergyInJoules { get; private set; } = -1;
        public float CumulativeIaEnergyInMwh { get; private set; } = -1;
        public float AverageIaPowerInWatt { get; private set; } = -1;
        public float CumulativeDramEnergyInJoules { get; private set; } = -1;
        public float CumulativeDramEnergyInMwh { get; private set; } = -1;
        public float AverageDramPowerInWatt { get; private set; } = -1;
        public float CumulativeGtEnergyInJoules { get; private set; } = -1;
        public float CumulativeGtEnergyInMwh { get; private set; } = -1;
        public float AverageGtPowerInWatt { get; private set; } = -1;

        internal void AddAttributeToData(string row)
        {
            if (!row.Contains('='))
            {
                throw new NotImplementedException();
            }

            var value = row.Split('=')[1].Trim();

            if (row.Contains("Total Elapsed Time (sec)"))
                TotalElapsedTimeInSeconds = float.Parse(value,CultureInfo.InvariantCulture);
            else if (row.Contains("Measured RDTSC Frequency (GHz)"))
                MeasuredRdtscFrequencyInGhz = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative Processor Energy_0 (Joules)"))
                CumulativeProcessorEnergyInJoules = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative Processor Energy_0 (mWh)"))
                CumulativeProcessorEnergyInMwh = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Average Processor Power_0 (Watt)"))
                AverageProcessorPowerWatt = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Average Processor Power_0 (Watt)"))
                AverageProcessorPowerWatt = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative IA Energy_0 (Joules)"))
                CumulativeIaEnergyInJoules = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative IA Energy_0 (mWh)"))
                CumulativeIaEnergyInMwh = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Average IA Power_0 (Watt)"))
                AverageIaPowerInWatt = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative DRAM Energy_0 (Joules)"))
                CumulativeDramEnergyInJoules = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative DRAM Energy_0 (mWh)"))
                CumulativeDramEnergyInMwh = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Average DRAM Power_0 (Watt)"))
                AverageDramPowerInWatt = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative GT Energy_0 (Joules)"))
                CumulativeGtEnergyInJoules = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Cumulative GT Energy_0 (mWh)"))
                CumulativeGtEnergyInMwh = float.Parse(value, CultureInfo.InvariantCulture);
            else if (row.Contains("Average GT Power_0 (Watt)"))
                AverageGtPowerInWatt = float.Parse(value, CultureInfo.InvariantCulture);
            else
                throw new NotImplementedException($"Row '{row}' does not include any known property");
        }
    }
}
