using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class RaplData
    {
        public double PSysStartInJoules { get; set; }
        public double PSysStopInJoules { get; set; }
        public double CoreStartInJoules { get; set; }
        public double CoreStopInJoules { get; set; }
        public double DramStartInJoules { get; set; }
        public double DramStopInJoules { get; set; }
        public double UncoreStartInJoules { get; set; }
        public double UncoreStopInJoules { get; set; }
        public double PackageZeroStartInJoules { get; set; }
        public double PackageZeroStopInJoules { get; set; }
    }
}
