using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public interface IAsyncEnergyProfiler : IEnergyProfiler
    {
        public Task WaitForSave(DateTime datetime);
        public Task WaitForExit();
    }
}
