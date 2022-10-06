using EnergyComparer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer
{
    public static class Constants
    {
        public static TimeSpan TimeBetweenExperiments = TimeSpan.FromMinutes(30);
        public static TimeSpan DurationOfExperiments = TimeSpan.FromSeconds(10);
        public static int ChargeLimit = 20;
        public static string DefaultFolderName = "09-experiment-data";
        public static string DatetimeFormat = "s";
        public static string DataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);



        public static List<string> GetAllRequiredPaths()
        {
            return AdapterUtils.GetAllSouces().Select(x => GetPathForSource(x)).ToList();
        }

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
