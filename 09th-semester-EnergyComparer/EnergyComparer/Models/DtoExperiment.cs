using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class DtoExperiment
    {
        public int ToolId { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public string Language { get; set; }
        public int ProgramId { get; set; }
        public int Version { get; set; }
        public int SystemId { get; set; }
        public int Id { get; set; }
    }
}
