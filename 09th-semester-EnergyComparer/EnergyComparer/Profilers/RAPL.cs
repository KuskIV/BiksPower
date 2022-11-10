using EnergyComparer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Utils;

namespace EnergyComparer.Profilers
{
    public class RAPL : IEnergyProfiler
    {
        private const string UncoreFolder = ":0:1";
        private const string PsysFolder = ":0:1";
        private const string PackageZeroFolder = ":0";
        private const string DramFolder = ":0:2";
        private const string CoreFolder = ":0:0";
        private readonly ELinuxProfilers _source;
        private readonly string _raplBasePath = "sys/class/powercap/";
        private Dictionary<string, long> _initialValues = new Dictionary<string, long>();
        private Dictionary<string, long> _previousValues = new Dictionary<string, long>();
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
                Value = SumTimeSeries(),
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

        private string SumTimeSeries()
        {
            var data = new RaplData()
            {
                CoreStartInJoules = GetStartValue(CoreFolder),
                CoreStopInJoules = GetStopValue(CoreFolder),
                DramStartInJoules= GetStartValue(DramFolder),
                DramStopInJoules = GetStopValue(DramFolder),
                PackageZeroStartInJoules = GetStartValue(PackageZeroFolder),
                PackageZeroStopInJoules = GetStopValue(PackageZeroFolder),
                PSysStartInJoules = GetStartValue(PsysFolder),
                PSysStopInJoules = GetStopValue(PsysFolder),
                UncoreStartInJoules = GetStartValue(UncoreFolder),
                UncoreStopInJoules = GetStopValue(UncoreFolder),
            };

            return System.Text.Json.JsonSerializer.Serialize(data);
        }

        private double GetStopValue(string coreFolder)
        {
            return _previousValues[CoreFolder] / 1000000;
        }

        private double GetStartValue(string coreFolder)
        {
            return _initialValues[coreFolder] / 1000000;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var (core, coreTotal) = GetCoreMeasurement();
            var (dram, dramTotal) = GetDramMeasurement();
            var (packageZero, packageZeroTotal) = GetPackageZeroMeasurement();
            var (psys, psysTotal) = GetPsysMeasurement();
            var (uncore, uncoreTotal) = GetUncoreMeasurement();

            var data = new RaplDataPoint()
            {
                Core = core,
                CoreTotal = coreTotal,
                Dram = dram,
                DramTotal = dramTotal,
                PackageZero = packageZero,
                PackageZeroTotal = packageZeroTotal,
                Uncore = uncore,
                UncoreTotal = uncoreTotal,
                PSys = psys,
                PSysTotal = psysTotal,
                Time = DateTime.UtcNow,
            };

            AppendToFile(data);
        }

        private (double, double) GetUncoreMeasurement()
        {
            var folderName = UncoreFolder;

            return GetMeasurement(folderName);
        }

        private (double, double) GetPsysMeasurement()
        {
            var folderName = PsysFolder;

            return GetMeasurement(folderName);
        }

        private (double, double) GetPackageZeroMeasurement()
        {
            var folderName = PackageZeroFolder;

            return GetMeasurement(folderName);
        }

        private (double, double) GetDramMeasurement()
        {
            var folderName = DramFolder;

            return GetMeasurement(folderName);
        }

        private (double, double) GetCoreMeasurement()
        {
            var folderName = CoreFolder;

            return GetMeasurement(folderName);
        }

        private (double, double) GetMeasurement(string folderName)
        {
            var path = "/" + _raplBasePath + "intel-rapl" + folderName + "/energy_uj";
            var currentValue = LinuxUtils.ExecuteCommandGetOutputAsSudo("cat", path);

            long initialValue;
            if (!_initialValues.TryGetValue(folderName, out initialValue))
            {
                _initialValues.Add(folderName, currentValue);
                initialValue = currentValue;
            }

            long previousValue;
            if (!_previousValues.TryGetValue(folderName, out previousValue))
            {
                previousValue = 0;
                _previousValues.Add(folderName, previousValue);
            }

            var value = currentValue - initialValue - previousValue;

            _previousValues[folderName] = value;

            return (value / 1000000, currentValue / 1000000);
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
