using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Services;

namespace EnergyComparer.Profilers
{
    public static class EnergyProfilers
    {
        public static List<IEnergyProfiler> GetAll()
        {
            var list = new List<IEnergyProfiler>();

            return list;
        }
    }
}
