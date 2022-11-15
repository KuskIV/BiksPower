using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class HardwareResults
    {
        public List<TimeSeries> TimeSeries { get; set; }
        public string Raw { get; set; }
    }

    public class TimeSeries
    {
        public double? C1TrueRMS { get; set; }
        public double? C1ACRMS { get; set; }
        public string TimeStamp { get; set; }
        public double? C1TrueRMSPower { get; set; }
        public double? C1ACRMSPower { get; set; }
    }

    public class HardwareRaw 
    {
        public HardwareRaw(double aCRMSRAW, double trueRMS)
        {
            ACRMSRAW = aCRMSRAW;
            TrueRMS = trueRMS;
        }

        public double ACRMSRAW { get; set; }
        public double TrueRMS { get; set; }
    }
}

