using CsvHelper.Configuration;
using CsvHelper;
using EnergyComparer;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace TestClient.E3Experiment
{
    public class E3Experiments
    {
        string fileType;
        string fileStart;
        string fileEnd;
        public E3Experiments()
        {
            fileType = ".csv";
            fileStart = "Start" + fileType;
            fileEnd = "End" + fileType;
        }
        public void Experiment1() 
        {
            //Setup
            string exName = "Ex1";
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);
            //Experiment
            au

            //Fetch results
            var Initial = GetE3Data(path, initialFile);
            var Final = GetE3Data(path, finalFile);
        }


        private static Process E3Save(string path, string fileName)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = $" /C (cd {path.Replace(" ", "` ")}); (powercfg.exe /srumutil  /output {fileName} /csv);";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            return process;
        }
        private static List<E3Data> GetE3Data(string path, string file)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
                Delimiter = ",",
            };
            using (var fs = File.Open(path+"\\"+file, FileMode.Open))
            {
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(reader, configuration))
                {
                    var data = csv.GetRecords<E3Data>();
                    return data.ToList();
                }

            }
        }
    }
}
