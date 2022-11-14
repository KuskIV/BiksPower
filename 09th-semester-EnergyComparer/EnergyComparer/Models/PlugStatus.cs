using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeController.Models
{
    public class PlugStatus
    {
        public float Voltage;
        public float Current;
        public float Power;
        public float ApparentPower;
        public float ReactivePower;
        public float PowerFactor;
        public float EnergyToday;
        public float EnergyYesterday;
        public float EnergyEnergyTotal;
        public bool On;

        public override string ToString()
        {
            return $"Voltage: {Voltage}\n" +
                   $"Current: {Current}\n" +
                   $"Power: {Power}\n" +
                   $"ApparentPower: {ApparentPower}\n" +
                   $"ReactivePower: {ReactivePower}\n" +
                   $"PowerFactor: {PowerFactor}\n" +
                   $"EnergyToday: {EnergyToday}\n" +
                   $"EnergyYesterday: {EnergyYesterday}\n" +
                   $"EnergyEnergyTotal: {EnergyEnergyTotal}\n" +
                   (On ? "ON" : "OFF");
        }
    }
}