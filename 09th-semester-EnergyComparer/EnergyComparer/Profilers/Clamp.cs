using EnergyComparer.Models;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            return (null,null);
        }

        public void Start(DateTime date)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
