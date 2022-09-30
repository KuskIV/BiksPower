using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Repositories;
using MySqlX.XDevAPI.Common;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class ExperimentService : IExperimentService
    {
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private Result<IntelPowerGadgetData> _result;
        private readonly IDataHandler _dataHandler;

        public ExperimentService(ILogger logger, IHardwareMonitorService hardwareMonitorService, Result<IntelPowerGadgetData> result, IDataHandler dataHandler)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
            _result = result;
            _dataHandler = dataHandler;
        }

        public void SaveResults()
        {

        }

        public async Task RunExperiment(IEnergyProfiler energyProfiler)
        {
            await SetupExperiment();
            PerformMeasurings();
            var bw = InitializeWorker();
            SetupCancelationToken(bw);
            
            energyProfiler.Start(_result.GetStartDate());

            bw.RunWorkerAsync();
            await Task.Delay(Constants.DurationOfExperiments);

            energyProfiler.Stop();
            
            PerformMeasurings();
            SaveResults();
        }

        private async Task SetupExperiment()
        {
            _result = new Result<IntelPowerGadgetData>()
            {
                system = await _dataHandler.GetSystem(),
            };
        }

        private static void SetupCancelationToken(BackgroundWorker bw)
        {
            var cts = new CancellationTokenSource(Constants.DurationOfExperiments);
            var cancellationToken = cts.Token;
            cancellationToken.Register(bw.CancelAsync);
        }

        private BackgroundWorker InitializeWorker()
        {
            var bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
            };

            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;

                    while (true)
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                });
            return bw;
        }

        public void PerformMeasurings()
        {
            var coreTemperatures = _hardwareMonitorService.GetCoreTemperatures();

        }
    }

    public interface IExperimentService
    {
    }
}
