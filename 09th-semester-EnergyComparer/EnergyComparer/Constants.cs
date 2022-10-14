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
        public static int MinutesBetweenExperiments = 5;
        public static int DurationOfExperimentsInMinutes = 1;
        public static int ChargeLowerLimit = 20;
        public static int ChargeUpperLimit = 100;
        public static int TemperatureLowerLimit = 50;
        public static int TemperatureUpperLimit = 80;
        public static string DefaultFolderName = "09-experiment-data";
        public static string DatetimeFormat = "s";
        public static string DataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static string GetPathForSource(string source)
        {
            return DataFolderPath + "\\" + DefaultFolderName + "\\" + source;
        }

        public static string GetFilePathForSouce(string source, DateTime date)
        {
            string fileName = date.ToString("yyyy-MM-dd-hh-mm-ss");
            string root = GetPathForSource(source);

            return new StringBuilder().AppendFormat(@"{0}\{1}.csv", root, fileName).ToString();
        }


    }
}
