using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern bool IntelEnergyLibInitialize();
        
        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern bool StartLog(string szFileName);

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        public static extern  bool StopLog();

        public IntelPowerGadgetService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> Initialise()
        {
            //var a = IntelEnergyLibInitialize();
            //var b = StartLog(_configuration.GetValue<string>("intelPowergadgetLogPath"));
            //await Task.Delay(TimeSpan.FromSeconds(10));
            //var c = StopLog();

            return true;
        }
    }
}
