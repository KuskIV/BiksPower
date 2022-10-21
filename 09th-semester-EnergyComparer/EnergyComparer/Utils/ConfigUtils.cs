using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Utils
{
    public static class ConfigUtils
    {
        public static bool GetIsProd(IConfiguration config)
        {
#if !DEBUG
            return true;
#else
            return config.GetValue<bool>("IsProd");
# endif
        }

        public static  bool GetHasBattery(IConfiguration config)
        {
            return config.GetValue<bool>("HasBattery");
        }

        public static bool GetSaveToDb(IConfiguration config)
        {
#if !DEBUG
            return true;
#else
            return config.GetValue<bool>("SaveToDb");
# endif
        }

        public static bool GetIterateOverProfilers(IConfiguration config)
        {
//#if !DEBUG
//            return true;
//#else
            return config.GetValue<bool>("IterateOverProfilers");
//# endif
        }

        internal static bool GetShouldRestart(IConfiguration configuration)
        {
#if !DEBUG
            return true;
#else
            return configuration.GetValue<bool>("ShouldRestart");
#endif
        }

        public static string GetWifiAdapterName(IConfiguration config)
        {
            return config.GetValue<string>("wifiAdapterName");
        }

        public static int GetIterationsBeforeRestart(IConfiguration config)
        {
            return config.GetValue<int>("IterationsBeforeRestart");

        }

        public static string GetConnectionString(IConfiguration config)
        {
            return config.GetValue<string>("ConnectionString");
        }

        internal static string GetMachineName(IConfiguration config)
        {
            return config.GetValue<string>("MachineName");

        }

    }
}
