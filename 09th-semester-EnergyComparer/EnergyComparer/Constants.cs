using EnergyComparer.Profilers;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer
{
    public static class Constants
    {
        public static int MinutesBetweenExperiments = 0;
        public static int DurationOfExperimentsInMinutes = 1;
        public static int IntervalBetweenReadsInMiliSeconds = 100;
        public static int ChargeLowerLimit = 1;
        public static int ChargeUpperLimit = 100;
        public static int TemperatureLowerLimit = 0;
        public static int TemperatureUpperLimit = 200;
        public static string DefaultFolderName = "09-experiment-data";
        public static string DatetimeFormat = "s";
        public static string DataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string Os = Environment.OSVersion.Platform.ToString();

        public static string GetPathForSource(string source)
        {
            return DataFolderPath + "/" + DefaultFolderName + "/" + source;
        }

        public static bool IsWindows()
        {
            return Os == "Win32NT";
        }

        public static string GetFilePathForSouce(string source, DateTime date)
        {
            string fileName = GetFileName(date);
            string root = GetPathForSource(source);

            return new StringBuilder().AppendFormat(@"{0}\{1}.csv", root, fileName).ToString();
        }

        public static string GetFileName(DateTime date) 
        {
            return date.ToString("yyyy-MM-dd-hh-mm-ss");
        }

        internal static string GetEnv()
        {
#if DEBUG
            return "DEV";
#else
            return "PROD";
#endif
        }
    }
}
