using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class DtoExperiment
    {
        public int Id { get; set; }
        public int ProfilerId { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public string Language { get; set; }
        public int ProgramId { get; set; }
        public int SystemId { get; set; }
        public int Runs { get; set; }
        public int Iteration { get; internal set; }
        public string FirstProfiler { get; internal set; }
        public int ConfigurationId { get; set; }
        public long duration { get; set; }
    }
}
