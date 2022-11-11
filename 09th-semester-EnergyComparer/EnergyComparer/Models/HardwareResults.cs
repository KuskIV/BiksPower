using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class HardwareResults
    {
        public string TimeSeries { get; set; } = "";
        public string Raw { get; set; } = "";
        public HardwareResults(string timeSeries, string raw)
        {
            TimeSeries = timeSeries;
            Raw = raw;
        }
    }
}
