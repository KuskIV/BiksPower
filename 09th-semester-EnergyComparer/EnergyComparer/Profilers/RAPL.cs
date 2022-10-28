using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public class RAPL : IEnergyProfiler
    {
        private readonly EWindowsProfilers _source;

        public RAPL()
        {
            _source = EWindowsProfilers.IntelPowerGadget;
        }

        public string GetName()
        {
            return _source.ToString();
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
