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
        public long PSys { get; set; }
        public long Core { get; set; }
        public long Dram { get; set; }
        public long Uncore { get; set; }
        public long PackageZero { get; set; }
    }
}
