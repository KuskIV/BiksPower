using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnergyComparer.Models
{
    [Serializable]
    public class HardwareMonitorData
    {
        public double cpuPowerPacketTotalJ { get; set; }
        public double cpuPowerCoresTotalJ { get; set; }
        public double cpuPowerMemoryTotalJ { get; set; }
        public double cpuPowerPacketAverageJ { get; set; }
        public double cpuPowerCoresAverageJ { get; set; }
        public double cpuPowerMemoryAverageJ { get; set; }

        public override string ToString()
        {
            return "cpuPowerPacketTotalJ: "+ cpuPowerPacketTotalJ + " cpuPowerCoresTotalJ: " + cpuPowerCoresTotalJ + " cpuPowerMemoryTotalJ: "
                + cpuPowerMemoryTotalJ + " cpuPowerPacketAverageJ: " + cpuPowerPacketAverageJ + " cpuPowerCoresAverageJ: " + cpuPowerCoresAverageJ
                + " cpuPowerMemoryAverageJ: " + cpuPowerMemoryAverageJ;
        }

    }


}
