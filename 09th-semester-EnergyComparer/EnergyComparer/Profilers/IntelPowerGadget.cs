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

        public IntelPowerGadget()
        {
            _source = ESource.IntelPowerGadget;
            Initialise();
        }

        public void Start(DateTime date)
        {
            var path = Constants.GetFilePathForSouce(_source, date);

            var success = StartLog(path);
            EnsureSuccess(success);
        }

        public void Stop()
        {
            var success = StopLog();
            EnsureSuccess(success);
        }

        public void Initialise()
        {
            var success =  IntelEnergyLibInitialize();
            EnsureSuccess(success);
        }

        private static void EnsureSuccess(bool success)
        {
            if (!success)
            {
                throw new Exception("IntelPowerGadget failed to initialize");
            }
        }

        public string GetName()
        {
            return _source.ToString();
        }
    }
}
