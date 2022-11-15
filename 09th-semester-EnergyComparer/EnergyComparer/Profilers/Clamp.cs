using EnergyComparer.Handlers;
using EnergyComparer.Models;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    internal class Clamp : IEnergyProfiler
    {
        private readonly EProfilers _source;

        public Clamp() 
        {
            _source = EProfilers.Clamp;
        }
        public string GetName()
        {
            return EProfilers.Clamp.ToString();
        }

        public (DtoTimeSeries, DtoRawData) ParseData(string path, int experimentId, DateTime startTime)
        {
            var results = HardwareMeasurementHandler.GetResults().Result;
            DtoTimeSeries dtoTimeSeries = new DtoTimeSeries()
            {
                Time = startTime,
                Value = JsonSerializer.Serialize(results.TimeSeries),
                ExperimentId = experimentId
            };

            DtoRawData dtoRawData = new DtoRawData()
            {
                Time = startTime,
                Value = results.Raw,
                ExperimentId = experimentId

            };
            return (dtoTimeSeries, dtoRawData);
        }

        public void Start(DateTime date)
        {
            HardwareMeasurementHandler.StartMeasurement(date.ToString());
        }

        public void Stop(DateTime stopTime)
        {
            HardwareMeasurementHandler.EndMeasurement(DateTime.Now.ToString());
        }
    }
}
