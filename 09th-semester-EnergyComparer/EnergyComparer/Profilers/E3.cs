using CsvHelper;
using CsvHelper.Configuration;
using EnergyComparer.Models;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public class E3 : IEnergyProfiler
    {
        string SruDefaultPath = "C:\\Windows\\System32\\sru";
        private DateTime dateTime { get; set; }
        private readonly EWindowsProfilers _source;

        public E3() 
        {
            _source = EWindowsProfilers.E3;
        }
        public string GetName()
        {
            return EWindowsProfilers.E3.ToString();
        }

        public DtoRawData ParseCsv(string path, int experimentId, DateTime startTime)
        {
            throw new NotImplementedException();
        }

        public void Start(DateTime date)
        {
            dateTime = date;
            Clear();
            Console.WriteLine("Cleared");
            StartLogging();
        }

        public void Stop()
        {
            CollectLogs();
        }

        private void Clear() 
        {
            string path = Constants.GetPathForSource(_source.ToString());
            Process save = E3SaveTemp(path, "tempOld");
            save.Start();
            save.WaitForExit();
            //Process clear = E3ClearProcces();
            //if (File.Exists(SruDefaultPath+"\\SRUDB.dat"))
            //{
            //    clear.Start();
            //    clear.WaitForExit();
            //}
        }

        //private Process E3ClearProcces()
        //{
            
        //}

        private void StartLogging() 
        {
            //Process start = E3StartProcess();
            //start.Start();
            //start.WaitForExit();
        }

        private void CollectLogs() 
        {
            string path = Constants.GetPathForSource(_source.ToString());
            Process save = E3SaveTemp(path, "tempNew");
            save.Start();
            save.WaitForExit();
            var Old = GetE3Data(path+"\\temp\\tempOld.csv");
            var New = GetE3Data(path + "\\temp\\tempNew.csv");
            List<int> small = New.Select(x => int.Parse(x.TimeInMSec)).ToList();
            small.Sort();
            Console.WriteLine("Done");

            //string file = Constants.GetFileName(dateTime);
            //Process collect = E3StopProcess(path, file);
            //collect.Start();
            //collect.WaitForExit();
        }

        private List<E3Data> GetE3Data(string path) 
        {
            //List<E3Data> values = File.ReadAllLines(path)
            //                   .Skip(1)
            //                   .Select(v => DailyValues.FromCsv(v))
            //                   .ToList();
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
                Delimiter = ",",
            };
            using (var fs = File.Open(path, FileMode.Open)) 
            {
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(reader, configuration)) 
                {
                    var data = csv.GetRecords<E3Data>();
                    return data.ToList();
                }
                
            }
        } 

        private Process E3SaveTemp(string path, string fileName) 
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = $" /C (cd {path.Replace(" ", "` ")}\\temp); (powercfg.exe /srumutil  /output {fileName}.csv /csv);";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            return process;
        }

        //private Process E3ClearProccesOld() 
        //{
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo();
        //    startInfo.FileName = "powershell.exe";
        //    startInfo.UseShellExecute = true;
        //    startInfo.Arguments = $" /C (cd {SruDefaultPath}) | (sc stop dps)| (rm srudb.dat.bak)| (mv .\\SRUDB.dat srudb.dat.bak)| (sc start dps)";
        //    startInfo.Verb = "runas";
        //    process.StartInfo = startInfo;
        //    return process;
        //}

        //private Process E3StartProcess()
        //{
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo();
        //    startInfo.FileName = "powershell.exe";
        //    startInfo.UseShellExecute = true;
        //    startInfo.Arguments = $" /C (cd {SruDefaultPath})";
        //    startInfo.Verb = "runas";
        //    process.StartInfo = startInfo;
        //    return process;
        //}
        private Process E3StopProcess(string path, string fileName)
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

        private Process AdminPowerProcces(string command) 
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = $" /C {command}";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            return process;
        }
    }
}
