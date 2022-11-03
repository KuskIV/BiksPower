using EnergyComparer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public class RAPL : IEnergyProfiler
    {
        private readonly ELinuxProfilers _source;
        private readonly string _raplBasePath = "sys/class/powercap/";
        private string _filePath;
        private System.Timers.Timer _timer;


        public RAPL()
        {
            _source = ELinuxProfilers.RAPL;
        }

        public string GetName()
        {
            return _source.ToString();
        }

        public (DtoTimeSeries, DtoRawData) ParseData(string path, int experimentId, DateTime startTime)
        {
            var data = ReadFile();

            var timeSeries = new DtoTimeSeries()
            {
                ExperimentId = experimentId,
                Time = startTime,
                Value = JsonConvert.SerializeObject(data)
            };

            var rawData = new DtoRawData()
            {
                ExperimentId = experimentId,
                Time = startTime,
                Value = SumTimeSeries(data),
            };

            return (timeSeries, rawData);
        }

        public void Start(DateTime date)
        {
            _filePath = Constants.GetFilePathForSouce(_source.ToString(), date);
            
            CreateFile();

            _timer = new System.Timers.Timer(Constants.IntervalBetweenReadsInMiliSeconds);
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        public void Stop()
        {
            _timer.Enabled = false;

        }

        private string SumTimeSeries(RaplTimeSeries data)
        {
            throw new NotImplementedException();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var data = new RaplDataPoint()
            {
                Core = GetCoreMeasurement(),
                Dram = GetDramMeasurement(),
                PackageZero = GetPackageZeroMeasurement(),
                PSys = GetPsysMeasurement(),
                Time = DateTime.UtcNow,
                Uncore = GetUncoreMeasurement(),
            };

            AppendToFile(data);
        }

        private int GetUncoreMeasurement()
        {
            var folderName = ":0:1";

            return GetMeasurement(folderName);
        }

        private int GetPsysMeasurement()
        {
            var folderName = ":0:1";

            return GetMeasurement(folderName);
        }

        private int GetPackageZeroMeasurement()
        {
            var folderName = ":0";

            return GetMeasurement(folderName);
        }

        private int GetDramMeasurement()
        {
            var folderName = ":0:2";

            return GetMeasurement(folderName);
        }

        private int GetCoreMeasurement()
        {
            var folderName = ":0:0";

            return GetMeasurement(folderName);
        }

        private int GetMeasurement(string folderName)
        {
            return 1;

            //var path = _filePath + "intel-rapl" + folderName + "/energy_uj";
            //var value = File.ReadAllText(path);

            //return Convert.ToInt32(value);
        }

        private void AppendToFile(RaplDataPoint data)
        {
            var raplTimeSeries = ReadFile();

            raplTimeSeries.DataPoints.Add(data);

            WriteFile(raplTimeSeries);
        }

        private void WriteFile(RaplTimeSeries raplTimeSeries)
        {
            var jsonData = JsonConvert.SerializeObject(raplTimeSeries);
            File.WriteAllText(_filePath, jsonData);
        }

        private RaplTimeSeries ReadFile()
        {
            var jsonData = File.ReadAllText(_filePath);
            var raplTimeSeries = JsonConvert.DeserializeObject<RaplTimeSeries>(jsonData);
            return raplTimeSeries;
        }

        private void CreateFile()
        {
            var raplTimeSeries = new RaplTimeSeries()
            {
                DataPoints = new List<RaplDataPoint>(),
            };

            WriteFile(raplTimeSeries);
        }
    }
}
