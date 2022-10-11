using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using EnergyComparer.Services;
using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;
using Serilog;
using System.ComponentModel;
using System.Management;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IExperimentService _experimentService;
        private readonly IHardwareHandler _hardwareHandler;
        private readonly IDataHandler _dataHandler;
        private readonly IEnergyProfilerService _profilerService;
        private readonly IAdapterService _adapterService;
        private readonly bool _isProd;

        public Worker(ILogger logger, IExperimentService experimentService, IHardwareHandler hardwareHandler, IDataHandler experimentHandler, IEnergyProfilerService profilerService, IConfiguration configuration, IAdapterService adapterService)
        {
            _logger = logger;
            _experimentService = experimentService;
            _hardwareHandler = hardwareHandler;
            _dataHandler = experimentHandler;
            _profilerService = profilerService;
            _adapterService = adapterService;
            _isProd = configuration.GetValue<bool>("IsProd");
            _dataHandler.InitializeConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _dataHandler.IncrementVersionForSystem(); // TODO: increment for all systems, not just the current one
            try
            {
                // TODO: Tie to one single core
                await _adapterService.WaitTillStableState(_isProd);
                var isExperimentValid = true;

                var programToRun = _adapterService.GetProgram(_dataHandler);

                while (!_adapterService.ShouldStopExperiment() && isExperimentValid)
                {
                    var profiler = await _profilerService.GetNext(programToRun);

                    isExperimentValid = await _experimentService.RunExperiment(profiler, programToRun);

                    _logger.Information("Experiment ended running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(TimeSpan.FromMinutes(Constants.MinutesBetweenExperiments), stoppingToken);
                }

                await _profilerService.SaveProfilers();
                _adapterService.Restart(_isProd);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception when running experiments");
                throw;
            }

        }
    }
}