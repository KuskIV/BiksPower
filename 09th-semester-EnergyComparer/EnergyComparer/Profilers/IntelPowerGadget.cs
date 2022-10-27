using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Services;
using Microsoft.VisualBasic;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using MissingFieldException = CsvHelper.MissingFieldException;

namespace EnergyComparer.Profilers
{
    public class IntelPowerGadget : IEnergyProfiler
    {

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool IntelEnergyLibInitialize();

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool ReadSample();

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool StartLog(string szFileName);

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool StopLog();

        private readonly EWindowsProfilers _source;
        private System.Timers.Timer _timer;
        private readonly int IntervalBetweenReadsInMiliSeconds = 100;

        public IntelPowerGadget()
        {
            _source = EWindowsProfilers.IntelPowerGadget;
            Initialise();
        }

        public void Start(DateTime date)
        {
            var path = Constants.GetFilePathForSouce(_source.ToString(), date);

            var success = StartLog(path);
            EnsureSuccess(success);

            _timer = new System.Timers.Timer(IntervalBetweenReadsInMiliSeconds); // 10 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        public void Stop()
        {
            _timer.Enabled = false;
            var success = StopLog();
            EnsureSuccess(success);
        }

        public void Initialise()
        {
            var success = IntelEnergyLibInitialize();
            EnsureSuccess(success);
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var success = ReadSample();
            EnsureSuccess(success);
        }

        private static void EnsureSuccess(bool success, [CallerMemberName] string callerName = "")
        {
            if (!success)
            {
                throw new Exception($"IntelPowerGadget failed to {callerName}");
            }
        }

        public string GetName()
        {
            return _source.ToString();
        }

        public (DtoTimeSeries, DtoRawData) ParseData(string path, int experimentId, DateTime startTime)
        {
            var intelPowerGadgetTimeSeries = new List<IntelPowerGadgetTimeSeires>();
            var intelPowerGadgetData = new IntelPowerGadgetData();

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<IntelPowerGadgetTimeSeriesMapper>();

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var row = csv.Parser.RawRecord;
                    
                    if (row.Contains(','))
                    {
                        var record = csv.GetRecord<IntelPowerGadgetTimeSeires>();
                        intelPowerGadgetTimeSeries.Add(record!);
                    }
                    else
                    {
                        intelPowerGadgetData.AddAttributeToData(row);
                    }
                }
            }

            var timeSeries = new DtoTimeSeries()
            {
                ExperimentId = experimentId,
                Time = startTime,
                Value = JsonSerializer.Serialize(intelPowerGadgetTimeSeries)
            };

            var rawData = new DtoRawData()
            {
                ExperimentId = experimentId,
                Time = startTime,
                Value = JsonSerializer.Serialize(intelPowerGadgetData)
            };

            
            return (timeSeries, rawData);
        }
    }
}
