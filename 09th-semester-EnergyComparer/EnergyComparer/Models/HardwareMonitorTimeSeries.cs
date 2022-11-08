using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EnergyComparer.Models
{
    [Serializable]
    //[XmlRoot("Measurement")]
    public class HardwareMonitorTimeSeries
    {
        public DateTime time { get; set; }

        // Load
        public float avgLoad { get; set; }

        public float totalLoad { get; set; }
        public float cpuC1T1 { get; set; }

        public float cpuC1T2 { get; set; }

        public float cpuC2T1 { get; set; }

        public float cpuC2T2 { get; set; }

        public float cpuC3T1 { get; set; }

        public float cpuC3T2 { get; set; }

        public float cpuC4T1 { get; set; }

        public float cpuC4T2 { get; set; }

        public float cpuC5T1 { get; set; }
        public float cpuC5T2 { get; set; }

        public float cpuC6T1 { get; set; }
        public float cpuC6T2 { get; set; }

        // CPU temperatures
        public float cpuMaxTemp { get; set; }
        public float cpuAvgTemp { get; set; }
        public float cpuC1Temp { get; set; }
        public float cpuC2Temp { get; set; }
        public float cpuC3Temp { get; set; }
        public float cpuC4Temp { get; set; }
        public float cpuC5Temp { get; set; }
        public float cpuC6Temp { get; set; }

        // CPU Core Clocks
        public float cpuC1Clock { get; set; }
        public float cpuC2Clock { get; set; }
        public float cpuC3Clock { get; set; }
        public float cpuC4Clock { get; set; }
        public float cpuC5Clock { get; set; }
        public float cpuC6Clock { get; set; }
        public float cpuBusSpeed { get; set; }

        // Power
        public float cpuPowerPacket { get; set; }
        public float cpuPowerCores { get; set; }
        public float cpuPowerMemory { get; set; }

        // Voltage
        public float cpuVoltageCores { get; set; }
        public float cpuVoltageC1 { get; set; }
        public float cpuVoltageC2 { get; set; }
        public float cpuVoltageC3 { get; set; }
        public float cpuVoltageC4 { get; set; }
        public float cpuVoltageC5 { get; set; }
        public float cpuVoltageC6 { get; set; }
                
     }


}
