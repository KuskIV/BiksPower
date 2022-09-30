using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Services;

namespace EnergyComparer.Profilers
{
    public class IntelPowerGadget : IEnergyProfiler
    {

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool IntelEnergyLibInitialize();

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool StartLog(string szFileName);

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool StopLog();


        private readonly ESource _source;

        public IntelPowerGadget(IConfiguration configuration)
        {
            _source = ESource.IntelPowerGadget;
            var success = Initialise();
        }

        public void Start(DateTime date)
        {
            var path = Constants.GetFilePathForSouce(_source, date);

            var success = StartLog(path);
        }

        public void Stop()
        {
            var success = StopLog();
        }

        public bool Initialise()
        {
            return IntelEnergyLibInitialize();
        }

    }
}
