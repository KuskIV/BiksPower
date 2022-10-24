using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class DtoMeasurement
    {
        public int Id { get; set; }
        public int ExperimentId { get; set; }
        public float Value { get; set; }
        public DateTime Time { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
