using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
            var counter = 0;
            await InitializeExperiment(program, startTime, energyProfiler);

            var bw = InitializeWorker(program, counter);

            _logger.Information("The energy profiler has started");
            energyProfiler.Start(startTime);

            bw.RunWorkerAsync();
            _logger.Information("The BackgroudWorker has started. Will run for: {time}", Constants.DurationOfExperiments);

            await Task.Delay(Constants.DurationOfExperiments);
            
            bw.CancelAsync();

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
            await Task.Delay(TimeSpan.FromSeconds(30)); // Give the wifi time to reconnect

            _result.experiment = await _dataHandler.GetExperiment(_result, program, startTime, stopTime);
        }

        private async Task InitializeExperiment(IProgram program, DateTime startTime, IEnergyProfiler energyProfiler)
        {
            await SetupExperiment(program, energyProfiler);

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

        private BackgroundWorker InitializeWorker(IProgram program, int counter)
        {
            var bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,

            };

            bw.DoWork += new DoWorkEventHandler(
                async delegate (object o, DoWorkEventArgs args)
                {
                    BackgroundWorker b = o as BackgroundWorker;

                    while (true)
                    {
                        await program.Run();
                        counter += 1;
                        _logger.Information("Run complete. Counte: {count}", counter);
                    }
                });

            return bw;
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
