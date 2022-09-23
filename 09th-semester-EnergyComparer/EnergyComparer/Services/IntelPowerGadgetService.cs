using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Services
{
    public interface IIntelPowerGadgetService
    {
        Task<bool> Initialise();
    }



    public class IntelPowerGadgetService : IIntelPowerGadgetService
    {
        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern bool IntelEnergyLibInitialize();
        
        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern bool StartLog(string szFileName);

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern  bool StopLog();

        public async Task<bool> Initialise()
        {
            var a = IntelEnergyLibInitialize();
            var b = StartLog("C:\\Users\\Mads Kusk\\Documents\\log.log");
            await Task.Delay(TimeSpan.FromSeconds(10));
            var c = StopLog();

            return true;
        }
    }
}
