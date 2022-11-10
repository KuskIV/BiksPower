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
        public decimal PSys { get; set; }
        public decimal PSysTotal { get; set; }
        public decimal Core { get; set; }
        public decimal CoreTotal { get; set; }
        public decimal Dram { get; set; }
        public decimal DramTotal { get; set; }
        public decimal Uncore { get; set; }
        public decimal UncoreTotal { get; set; }
        public decimal PackageZero { get; set; }
        public decimal PackageZeroTotal { get; set; }
    }
}
