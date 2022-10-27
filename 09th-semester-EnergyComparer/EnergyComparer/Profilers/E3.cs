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
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EnergyComparer.Profilers
{
    public class E3 : IEnergyProfiler
    {
        string SruDefaultPath = "C:\\Windows\\System32\\sru";
        private DateTime dateTime { get; set; }
        private readonly EWindowsProfilers _source;
        private string fileType;
        private string fileStart;
        private string fileEnd;
        public E3() 
        {
            _source = EWindowsProfilers.E3;
            fileType = ".csv";
            fileStart = "Start" + fileType;
            fileEnd = "End" + fileType;
        }
        public string GetName()
        {
            return EWindowsProfilers.E3.ToString();
        }

        private void Start(DateTime date)
        {
            dateTime = date;
            string path = Constants.GetPathForSource(_source.ToString());
            E3Save(path, fileStart).Start();
        }

        public async Task WaitForStart(DateTime date) 
        {
            dateTime = date;
            await WaitForBlock();
            Start(date);
        }
        public async Task WaitForStop() 
        {
            await WaitForBlock();
            Stop();
        }

        private async Task WaitForBlock() 
        {
            string path = Constants.GetPathForSource(_source.ToString())+"\\temp";
            var start = E3Save(path, "1");
            var exit = E3Save(path, "2");

            start.Start();
            while (!NewBlock(path,"1","2"))
            {
                await Task.Delay(1);
                exit.Start();
            }
        }

        private bool NewBlock(string path, string file1, string file2)
        { 
            var Initial = GetE3Data(path, file1);
            var Final = GetE3Data(path, file2);
            return (Final.GetRange(Initial.Count, Final.Count - Initial.Count).Count < 0);  
        }

        public void Stop()
        {
            CollectLogs();
        }

        private void Clear() 
        {
            string path = Constants.GetPathForSource(_source.ToString());
            Process save = E3Save(path, "tempOld");
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

        //private void StartLogging() 
        //{
        //    Process start = E3StartProcess();
        //    start.Start();
        //    start.WaitForExit();
        //}

        private void CollectLogs() 
        {
            string path = Constants.GetPathForSource(_source.ToString());
            Process save = E3Save(path, "tempNew");
            save.Start();
            save.WaitForExit();
            var Old = GetE3Data(path + "\\temp\\tempOld.csv");
            var New = GetE3Data(path + "\\temp\\tempNew.csv");
            var Temp = New.GetRange(Old.Count, New.Count - Old.Count);
            Dictionary<string, List<E3Data>> InactivityState = new Dictionary<string, List<E3Data>>();
            foreach (var item in Temp)
            {
                if (InactivityState.ContainsKey(item.InteractivityState)) 
                {
                    InactivityState[item.InteractivityState].Add(item);
                }
                else
                {
                    InactivityState.Add(item.InteractivityState, new List<E3Data>());
                    InactivityState[item.InteractivityState].Add(item);

                }
            }
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");


            //foreach (var key in InactivityState.Keys) 
            //{
            //    InactivityState[key] = InactivityState[key].GroupBy(x=> x.AppId).Select(x => x.First()).ToList();
            //}

            List<int> small = New.Select(x => int.Parse(x.TimeInMSec)).ToList();
            
            small.Sort();
            Console.WriteLine("Done");

            //string file = Constants.GetFileName(dateTime);
            //Process collect = E3StopProcess(path, file);
            //collect.Start();
            //collect.WaitForExit();
        }
        public Dictionary<string, List<E3Data>> IsolateApps(Dictionary<string, List<E3Data>> dict, string key) 
        {
            Dictionary<string, List<E3Data>> valuePairs = new Dictionary<string, List<E3Data>>();
            if (dict.ContainsKey(key)) 
            {
                foreach (var item in dict[key])
                {
                    if (valuePairs.ContainsKey(item.AppId))
                    {
                        valuePairs[item.AppId].Add(item);
                    }
                    else
                    {
                        valuePairs.Add(item.AppId, new List<E3Data>());
                        valuePairs[item.AppId].Add(item);

                    }
                }            
            }
            return valuePairs;
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

        private static Process E3Save(string path, string fileName)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = $" /C (cd {path.Replace(" ", "` ")}); (powercfg.exe /srumutil  /output {fileName} /csv);";
            startInfo.Verb = "runas";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
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
        //private Process E3StopProcess(string path, string fileName)
        //{
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo();
        //    startInfo.FileName = "powershell.exe";
        //    startInfo.UseShellExecute = true;
        //    startInfo.Arguments = $" /C (cd {path.Replace(" ", "` ")}); (powercfg.exe /srumutil  /output {fileName} /csv);";
        //    startInfo.Verb = "runas";
        //    process.StartInfo = startInfo;
        //    return process;
        //}

        //private Process AdminPowerProcces(string command) 
        //{
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo();
        //    startInfo.FileName = "powershell.exe";
        //    startInfo.UseShellExecute = true;
        //    startInfo.Arguments = $" /C {command}";
        //    startInfo.Verb = "runas";
        //    process.StartInfo = startInfo;
        //    return process;
        //}

        private static List<E3Data> GetE3Data(string path, string file)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
                Delimiter = ",",
            };
            using (var fs = File.Open(path + "\\" + file, FileMode.Open))
            {
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(reader, configuration))
                {
                    var data = csv.GetRecords<E3Data>();
                    return data.ToList();
                }

            }
        }

        void IEnergyProfiler.Start(DateTime date)
        {
            throw new NotImplementedException();
        }

        (DtoTimeSeries, DtoRawData) IEnergyProfiler.ParseData(string path, int experimentId, DateTime startTime)
        {
            DtoRawData dtoRawData = new DtoRawData();
            dtoRawData.Value = JsonSerializer.Serialize(GetE3Data(path));
            dtoRawData.ExperimentId = experimentId;
            dtoRawData.Time = startTime;

            throw new NotImplementedException("Tell Jeppe to implement this");
        }
    }
}
