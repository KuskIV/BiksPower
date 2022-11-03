using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class RaplData
    {
        public int PSysInJoules { get; set; }
        public int CoreInJoules { get; set; }
        public int DramInJoules { get; set; }
        public int UncoreInJoules { get; set; }
        public int PackageZeroInJoules { get; set; }
        public int PSysInWatt { get; set; }
        public int CoreInWatt { get; set; }
        public int DramInWatt { get; set; }
        public int UncoreInWatt { get; set; }
        public int PackageZeroInWatt { get; set; }
    }
}
