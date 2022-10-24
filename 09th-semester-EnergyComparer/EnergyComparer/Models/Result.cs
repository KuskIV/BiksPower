using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class Result
    {
        public List<DtoMeasurement> temperatures { get; set; } = new List<DtoMeasurement>();
        public List<DtoRawData> data { get; set; } = new List<DtoRawData>();
        public DtoTestCase program { get; set; }
        public DtoExperiment experiment { get; set; }
        public DtoDut system { get; set; }
        public DtoProfiler profiler { get; set; }

        internal int GetProfilerId()
        {
            return profiler.Id;
        }

        internal int GetProgramId()
        {
            return program.Id;
        }

        internal int GetSystemId()
        {
            return system.Id;
        }

        internal int GetVersion()
        {
            return system.Version;
        }
    }
}
