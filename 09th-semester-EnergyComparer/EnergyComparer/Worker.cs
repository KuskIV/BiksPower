using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using EnergyComparer.Services;
using EnergyComparer.Utils;
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
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly bool _isProd;

        public Worker(ILogger logger, IExperimentService experimentService, IHardwareHandler hardwareHandler, IDataHandler experimentHandler, IEnergyProfilerService profilerService, IConfiguration configuration, IHardwareMonitorService hardwareMonitorService)
        {
            _logger = logger;
            _experimentService = experimentService;
            _hardwareHandler = hardwareHandler;
            _dataHandler = experimentHandler;
            _profilerService = profilerService;
            _hardwareMonitorService = hardwareMonitorService;
            _isProd = configuration.GetValue<bool>("IsProd");
            _dataHandler.InitializeConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _dataHandler.IncrementVersionForSystem(); // TODO: increment for all systems, not just the current one

            var a = _hardwareMonitorService.GetAverageCpuLoad();

            _logger.Information($"average cpu load: {a}");

            _logger.Information("Waiting for 2 min");
            await Task.Delay(TimeSpan.FromMinutes(2));

            AdapterUtils.Restart(_isProd);
            try
            {
                var programToRun = AdapterUtils.GetProgram(_dataHandler);

                while (!AdapterUtils.ShouldStopExperiment())
                {
                    var profiler = await _profilerService.GetNext(programToRun);

                    await _experimentService.RunExperiment(profiler, programToRun);

                    _logger.Information("Experiment ended running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(Constants.TimeBetweenExperiments, stoppingToken);
                }

                await _profilerService.SaveProfilers();
                
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception when running experiments");
                throw;
            }

        }
    }
}