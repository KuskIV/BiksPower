using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using EnergyComparer.Services;
using EnergyComparer.Utils;
using Serilog;
using System.ComponentModel;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IExperimentService _experimentService;
        private readonly IHardwareHandler _hardwareHandler;
        private readonly IDataHandler _dataHandler;

        public Worker(ILogger logger, IExperimentService experimentService, IHardwareHandler hardwareHandler, IDataHandler experimentHandler)
        {
            _logger = logger;
            _experimentService = experimentService;
            _hardwareHandler = hardwareHandler;
            _dataHandler = experimentHandler;

            _dataHandler.InitializeConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intelPowerGadget = new IntelPowerGadget();
            var testProgram = new TestProgram(_dataHandler);

            //await _dataHandler.IncrementVersionForSystem();

            await _experimentService.RunExperiment(intelPowerGadget, testProgram);

            _logger.Information("Experiment ended running at: {time}", DateTimeOffset.Now);
            await Task.Delay(Constants.TimeBetweenExperiments, stoppingToken);
        }


    }
}