using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class RaplData
    {
        public decimal PSysStartInJoules { get; set; }
        public decimal PSysStopInJoules { get; set; }
        public decimal CoreStartInJoules { get; set; }
        public decimal CoreStopInJoules { get; set; }
        public decimal DramStartInJoules { get; set; }
        public decimal DramStopInJoules { get; set; }
        public decimal UncoreStartInJoules { get; set; }
        public decimal UncoreStopInJoules { get; set; }
        public decimal PackageZeroStartInJoules { get; set; }
        public decimal PackageZeroStopInJoules { get; set; }
    }
}
