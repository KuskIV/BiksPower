using EnergyComparer.Models;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    internal class Clam : IEnergyProfiler
    {
        public string GetName()
        {
            return EProfilers.Clam.ToString();
        }

        public (DtoTimeSeries, DtoRawData) ParseData(string path, int experimentId, DateTime startTime)
        {
            throw new NotImplementedException();
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
