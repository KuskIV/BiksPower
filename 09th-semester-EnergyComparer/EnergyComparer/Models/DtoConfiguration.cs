using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class DtoConfiguration
    {
        public int Id { get; set; }
        public int MinTemp { get; set; }
        public int MaxTemp { get; set; }
        public int MinBattery { get; set; }
        public int MaxBattery { get; set; }
        public int MinutesBetweenExperiments { get; set; }
        public int MinuteDurationOfExperiments { get; set; }
        public int Version { get; set; }
    }
}
