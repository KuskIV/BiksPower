using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Repositories;
using EnergyComparer.Services;
using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.Connections;
using Microsoft.Win32;
using Serilog;
using System.ComponentModel;
using System.Data;
using System.Management;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private IExperimentService _experimentService;
        private readonly Func<IDbConnection> _connectionFactory;
        private IDataHandler _dataHandler;
        private IEnergyProfilerService _profilerService;
        private IAdapterService _adapterService;
        private HardwareMonitorService _hardwareMonitorService;
        private readonly bool _isProd;

        public Worker(ILogger logger, IExperimentService experimentService, IConfiguration configuration, Func<IDbConnection> connectionFactory)
        {
            _logger = logger;
            _experimentService = experimentService;
            _connectionFactory = connectionFactory;
            _isProd = configuration.GetValue<bool>("IsProd");
            _profilerService = new EnergyProfilerService(_isProd);
            
            InitializeDependencies();
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
                    var profiler = await _profilerService.GetNext(programToRun, _dataHandler, _adapterService);

                    RemoveDependencies();

                    isExperimentValid = await _experimentService.RunExperiment(profiler, programToRun);

                    InitializeDependencies();

                    _logger.Information("Experiment ended running at: {time}. Next experiment will run ata {time2}", DateTimeOffset.Now, DateTimeOffset.Now.AddMinutes(Constants.MinutesBetweenExperiments));
                    await Task.Delay(TimeSpan.FromMinutes(Constants.MinutesBetweenExperiments), stoppingToken);
                }

                await _profilerService.SaveProfilers(_dataHandler);
                _adapterService.Restart(_isProd);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception when running experiments");
                throw;
            }

        }

        private void InitializeDependencies()
        {
            _hardwareMonitorService = new HardwareMonitorService(_logger);
            _adapterService = new AdapterWindowsLaptopService(_hardwareMonitorService, _logger);
            _dataHandler = new DataHandler(_logger, _adapterService, _connectionFactory);
            _dataHandler.InitializeConnection();
        }

        private void RemoveDependencies()
        {
            _dataHandler.CloseConnection();
            _hardwareMonitorService = null;
            _adapterService = null;
            _dataHandler = null;
        }
    }
}