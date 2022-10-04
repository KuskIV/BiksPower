using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using MySqlX.XDevAPI.Common;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class ExperimentService : IExperimentService
    {
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private Result<IntelPowerGadgetData> _result;
        private readonly IDataHandler _dataHandler;
        private readonly IHardwareHandler _hardwareHandler;
        private int _counter = 0;
        private BackgroundWorker _bw;

        public ExperimentService(ILogger logger, IHardwareMonitorService hardwareMonitorService, IDataHandler dataHandler, IHardwareHandler hardwareHandler)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
            _dataHandler = dataHandler;
            _hardwareHandler = hardwareHandler;
        }

        public async Task RunExperiment(IEnergyProfiler energyProfiler, IProgram program)
        {
            var startTime = DateTime.UtcNow;
            await InitializeExperiment(program, startTime, energyProfiler);

            _bw = InitializeWorker(program);

            _logger.Information("The energy profiler has started");
            energyProfiler.Start(startTime);

            _bw.RunWorkerAsync();
            _logger.Information("The BackgroudWorker has started. Will run for: {time}", Constants.DurationOfExperiments);

            await Task.Delay(Constants.DurationOfExperiments);
            
            _bw.CancelAsync(); // TODO: shoul it be killed? https://stackoverflow.com/questions/1559255/whats-wrong-with-using-thread-abort/1560567#1560567

            energyProfiler.Stop();
            _logger.Information("The experiment has stopped, and data saved to {path}", Constants.GetPathForSource(energyProfiler.GetName()));

            var stopTime = DateTime.UtcNow;

            await EndExperiment(program, stopTime, startTime);
            SaveResults();
        }

        public void SaveResults()
        {
            // TODO: Parse CSV file and save data
        }

        private async Task EndExperiment(IProgram program, DateTime stopTime, DateTime startTime)
        {
            PerformMeasurings(stopTime);

            _hardwareHandler.EnableWifi();
            await IsWifiEnabled();

            _dataHandler.InitializeConnection();

            _result.experiment = await _dataHandler.GetExperiment(_result, program, startTime, stopTime, _counter);
        }

        private async Task IsWifiEnabled()
        {
            await Task.Delay(TimeSpan.FromSeconds(15));

            while (!PingGoogleSuccessfully())
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        private bool PingGoogleSuccessfully()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task InitializeExperiment(IProgram program, DateTime startTime, IEnergyProfiler energyProfiler)
        {
            await SetupExperiment(program, energyProfiler);

            _dataHandler.CloseConnection();
            _hardwareHandler.DisableWifi();
            
            PerformMeasurings(startTime);
        }

        private async Task SetupExperiment(IProgram program, IEnergyProfiler energyProfiler)
        {
            _result = new Result<IntelPowerGadgetData>()
            {
                system = await _dataHandler.GetSystem(),
                program = program.GetProgram(),
                profiler = await _dataHandler.GetProfiler(energyProfiler)
            };
        }

        private BackgroundWorker InitializeWorker(IProgram program)
        {
            var bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true,
            };

            bw.ProgressChanged += new ProgressChangedEventHandler(BackgrouWorkerProgressChanged);

            bw.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;
                    var counter = 0;

                    while (!b.CancellationPending)
                    {
                        program.Run();
                        counter += 1;
                        b.ReportProgress(counter);
                        _logger.Information("Run complete. Counte: {count}", counter);
                    }
                    
                    _logger.Information("background worker has been cancelled");
                });

            return bw;
        }

        private void BackgrouWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _counter = e.ProgressPercentage;
        }

        public void PerformMeasurings(DateTime date)
        {
            var coreTemperatures = _hardwareMonitorService.GetCoreTemperatures(date);

            _result.temperatures.AddRange(coreTemperatures);
        }
    }

    public interface IExperimentService
    {
        Task RunExperiment(IEnergyProfiler energyProfiler, IProgram testProgram);
        void SaveResults();
    }
}
