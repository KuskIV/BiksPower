using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    internal class E3 : IEnergyProfiler
    {
        public string GetName()
        {
            return EWindowsProfilers.E3.ToString();
        }

        public void Start(DateTime date)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
