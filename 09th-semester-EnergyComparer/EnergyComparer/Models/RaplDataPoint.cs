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
        public int PSys { get; set; }
        public int Core { get; set; }
        public int Dram { get; set; }
        public int Uncore { get; set; }
        public int PackageZero { get; set; }
    }
}
