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
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Serilog.ILogger;

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
        private List<E3Data> result;
        private DateTime PrevMeasure;

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
            //var clear = E3ClearProccesOld();
            //clear.Start();
            //string error = "";
            //Console.WriteLine($"Clearing of old data starting attempts at {DateTime.UtcNow}");
            //do
            //{
            //    error = clear.StandardError.ReadToEnd();
            //    if (error.Length > 0) 
            //    {
            //        Console.WriteLine($"Error orrcurred at clearing trying again Time: {DateTime.UtcNow}");
            //    }
            //}
            //while (error.Length > 0);
            //Console.WriteLine($"Clearing completed at: {DateTime.UtcNow}");
            //await clear.WaitForExitAsync();
            PrevMeasure = await WaitForBlock();
            Start(date);
        }
        public async Task WaitForStop() 
        {
            _ = await WaitForBlock();
            Stop();
        }

        private async Task<DateTime> WaitForBlock() 
        {
            string path = Constants.GetPathForSource(_source.ToString())+"\\temp";
            var start = E3Save(path, "1");
            var exit = E3Save(path, "2");

            start.Start();
            await start.WaitForExitAsync();
            do
            {
                await Task.Delay(1);
                exit.Start();
                await exit.WaitForExitAsync();
            }
            while (!NewBlock(path, "1", "2"));
            Console.WriteLine("New-Block encountered");
            return GetMostRecentTimstamp(GetE3Data(path,"1"));
        }

        private bool NewBlock(string path, string file1, string file2)
        {
            try
            {
                var Initial = GetMostRecentTimstamp(GetE3Data(path, file1));
                var Final = GetMostRecentTimstamp(GetE3Data(path, file2));
                return (Initial.CompareTo(Final) != 0);  
            }
            catch (Exception)
            { 
                return false;
            }
        }

        public void Stop()
        {
            CollectLogs();
        }

        private DateTime GetMostRecentTimstamp(List<E3Data> e3Datas) 
        {
            return GetTimeStamp(e3Datas.Last());
        }

        private DateTime GetTimeStamp(E3Data e3Data) 
        {
            string format = "yyyy-MM-dd:HH:mm:ss.ffff";//2022-10-28:11:58:00.0000

            if (DateTime.TryParseExact(e3Data.TimeStamp, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                return dateTime;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        private void CollectLogs() 
        {
            string path = Constants.GetPathForSource(_source.ToString())+"\\temp\\";
            var Final = GetE3Data(path, "2");
            var Result = Final.Where(x => GetTimeStamp(x).CompareTo(PrevMeasure) > 0).ToList();
            Console.WriteLine("Done");
        }

        private List<E3Data> GetE3Data(string path) 
        {
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
        //    startInfo.RedirectStandardError = true;
        //    startInfo.Arguments = $" /C (cd {SruDefaultPath}); (sc stop dps); (rm .\\SRUDB.dat); (sc start dps)";
        //    startInfo.Verb = "runas";
        //    startInfo.CreateNoWindow = true;
        //    startInfo.UseShellExecute = false;
        //    process.StartInfo = startInfo;
        //    return process;
        //}

        private static List<E3Data> GetE3Data(string path, string file)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim,
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
            dtoRawData.Value = JsonSerializer.Serialize(result);
            dtoRawData.ExperimentId = experimentId;
            dtoRawData.Time = startTime;

            DtoTimeSeries dtoTimeSeries = new DtoTimeSeries();
            dtoTimeSeries.Value = "{}";
            dtoTimeSeries.ExperimentId = experimentId;
            dtoTimeSeries.Time = startTime;

            return (dtoTimeSeries, dtoRawData);
        }
    }
}
