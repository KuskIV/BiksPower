using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class E3Data
    {
        [Index(0)]
        public string AppId { get; set; }
        [Index(1)]
        public string UserId { get; set; }
        [Index(2)]
        public string TimeStamp { get; set; }
        [Index(3)]
        public string OnBattery { get; set; }
        [Index(4)]
        public string ScreenOn { get; set; }
        [Index(5)]
        public string BatterySaverActive { get; set; }
        [Index(6)]
        public string LowPowerEpochActive { get; set; }
        [Index(7)]
        public string Foreground { get; set; }
        [Index(8)]
        public string InteractivityState { get; set; }
        [Index(9)]
        public string Container { get; set; }
        [Index(10)]
        public string Committed { get; set; }
        [Index(11)]
        public string TimeInMSec { get; set; }
        [Index(12)]
        public string MeasuredBitmap { get; set; }
        [Index(13)]
        public string EnergyLoss { get; set; }
        [Index(14)]
        public string CPUEnergyConsumption { get; set; }
        [Index(15)]
        public string SocEnergyConsumption { get; set; }
        [Index(16)]
        public string DisplayEnergyConsumption { get; set; }
        [Index(17)]
        public string DiskEnergyConsumption { get; set; }
        [Index(18)]
        public string NetworkEnergyConsumption { get; set; }
        [Index(19)]
        public string MBBEnergyConsumption { get; set; }
        [Index(20)]
        public string OtherEnergyConsumption { get; set; }
        [Index(21)]
        public string EmiEnergyConsumption { get; set; }
        [Index(22)]
        public string CPUEnergyConsumptionWorkOnBehalf { get; set; }
        [Index(23)]
        public string CPUEnergyConsumptionAttributed { get; set; }
        [Index(24)]
        public string TotalEnergyConsumption { get; set; }
    }
    }
