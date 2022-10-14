using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class DtoRawData
    {
        public DateTime Time { get; set; }
        public string Value { get; set; }
        public int ExperimentId { get; set; }
        public int Id { get; set; }
    }
}
