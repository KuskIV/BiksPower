using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class RaplDataPoint
    {
        public DateTime Time { get; set; }
        public double PSys { get; set; }
        public double PSysTotal { get; set; }
        public double Core { get; set; }
        public double CoreTotal { get; set; }
        public double Dram { get; set; }
        public double DramTotal { get; set; }
        public double Uncore { get; set; }
        public double UncoreTotal { get; set; }
        public double PackageZero { get; set; }
        public double PackageZeroTotal { get; set; }
    }
}
