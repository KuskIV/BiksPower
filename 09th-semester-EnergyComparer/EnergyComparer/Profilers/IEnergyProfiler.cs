using CsvHelper;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public interface IEnergyProfiler
    {
        string GetName();
        DtoRawData ParseCsv(string path, int experimentId, DateTime startTime);
        public void Start(DateTime date);
        public void Stop();
    }
}
