using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class Result<T>
    {
        public List<DtoTemperature> temperatures { get; set; } = new List<DtoTemperature>();
        public List<DtoRawData<T>> data { get; set; } = new List<DtoRawData<T>>();
        public DtoProgram program { get; set; }
        public DtoExperiment experiment { get; set; }
        public DtoSystem system { get; set; }
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
