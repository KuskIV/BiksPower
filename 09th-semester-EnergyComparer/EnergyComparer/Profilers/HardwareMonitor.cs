using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Profilers
{
    public class HardwareMonitor : IEnergyProfiler
    {
        private System.Timers.Timer _timer;
        private readonly int IntervalBetweenReadsInSeconds = 1;

        public string GetName()
        {
            return EWindowsProfilers.HardwareMonitor.ToString();
        }

        public DtoRawData ParseCsv(string path, int experimentId, DateTime startTime)
        {
            throw new NotImplementedException();
        }

        public void Start(DateTime date)
        {
            _timer = new System.Timers.Timer(1000 * IntervalBetweenReadsInSeconds); // 10 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
            Console.WriteLine("Timer has started");
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.Write("doing gods work");
        }
    }
}
