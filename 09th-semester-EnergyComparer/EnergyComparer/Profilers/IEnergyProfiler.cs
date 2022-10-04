using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public interface IEnergyProfiler
    {
        string GetName();
        public void Start(DateTime date);
        public void Stop();
    }
}
