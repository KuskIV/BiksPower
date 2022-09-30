using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Profilers
{
    public class EnergyProfiler
    {
        private readonly ILogger _logger;

        public void SaveResults()
        {

        }

        public async Task RunExperiment()
        {
            PerformMeasurings();
            StartExperiment();

            // wait for some time
            await Task.Delay(Constants.DurationOfExperiments);

            EndExperiment();
            PerformMeasurings();
            SaveResults();
        }

        public void PerformMeasurings()
        {

        }

        public void StartExperiment()
        {
            throw new NotImplementedException();
        }

        public void EndExperiment()
        {
            throw new NotImplementedException();
        }
    }
}
