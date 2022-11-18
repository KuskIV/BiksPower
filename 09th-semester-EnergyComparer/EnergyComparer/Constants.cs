using EnergyComparer.Handlers;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using EnergyComparer.TestCases;
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
        public static int ChargeLowerLimit = 40;
        public static int ChargeUpperLimit = 80;
        public static int TemperatureLowerLimit = 0;
        public static int TemperatureUpperLimit = 200;
        public static string DefaultFolderName = "09-experiment-data";
        public static string DatetimeFormat = "s";
        public static string DataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string Os = Environment.OSVersion.Platform.ToString();
        public static string SurfacePro = "Surface4Pro";
        public static string SurfaceBook = "SurfaceBook";
        public static string PowerKomplett = "PowerKomplett";


        public static List<TestCase> GetTestCases(IDataHandler dataHandler)
        {
            var language = ELanguage.CSharp.ToString();

            var testCases = new List<TestCase>();

            testCases.Add(new TestCase(dataHandler, "TestCaseIdle", language));
            testCases.Add(new TestCase(dataHandler, "BinaryTrees", language));
            //testCases.Add(new TestCase(dataHandler, "ReverseComplement", language));
            testCases.Add(new TestCase(dataHandler, "FannkuchRedux", language));
            testCases.Add(new TestCase(dataHandler, "Nbody", language));
            testCases.Add(new TestCase(dataHandler, "Fasta", language));
            testCases.Add(new TestCase(dataHandler, "DiningPhilosophers", language));

            return testCases;
        }
        public static string GetPathForSource(string source)
        {
            return DataFolderPath + "/" + DefaultFolderName + "/" + source;
        }

        public static string GetExecutablePathForOs(DirectoryInfo path, string name)
        {
            if (Constants.IsWindows())
            {
                return Constants.GetWindowsExecutablePath(path, name);
            }
            else
            {
#if !DEBUG
                return path.Parent.FullName + Constants.GetLinuxExecutablePath(name);;
#else
                return path.FullName + Constants.GetLinuxExecutablePath(name);
#endif
            }
        }

        public static string GetWindowsExecutablePath(DirectoryInfo path, string name)
        {
            return path.FullName + $"\\09th-semester-test-cases\\{name}\\{name}\\bin\\Release\\net6.0\\{name}.exe";
        }

        public static string GetLinuxExecutablePath(string name)
        {
            return $"/09th-semester-test-cases/{name}/{name}/bin/Release/net6.0/linux-x64/{name}";
        }

        public static bool IsWindows()
        {
            return Os == "Win32NT";
        }

        public static string GetFilePathForSouce(string source, DateTime date)
        {
            string fileName = GetFileName(date);
            string root = GetPathForSource(source);

            return new StringBuilder().AppendFormat(@"{0}/{1}.csv", root, fileName).ToString();
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
