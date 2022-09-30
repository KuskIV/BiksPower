using EnergyComparer.Handlers;
using EnergyComparer.Models;
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
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IHardwareHandler _hardwareHandler;
        private readonly IDataHandler _experimentHandler;
        private readonly DtoSystem _system;

        public Worker(ILogger logger, IHardwareMonitorService hardwareMonitorService, IHardwareHandler hardwareHandler, IDataHandler experimentHandler)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
            _hardwareHandler = hardwareHandler;
            _experimentHandler = experimentHandler;

            _system = _experimentHandler.GetSystem().Result;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _hardwareHandler.EnsurePathsExists();
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

            InitializeExperiment();

            var cts = new CancellationTokenSource(Constants.DurationOfExperiments);
            var cancellationToken = cts.Token;
            cancellationToken.Register(bw.CancelAsync);

            
            bw.RunWorkerAsync();
            
            await Task.Delay(Constants.DurationOfExperiments);
                

            EndExperiment();

            _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(Constants.TimeBetweenExperiments, stoppingToken);



                //var avgTemp = _hardwareMonitorService.GetAverageCpuTemperature();
                //var memory = _hardwareMonitorService.GetCpuMemory();
                //var avgLoad = _hardwareMonitorService.GetAverageCpuLoad();
                //var totalLoad = _hardwareMonitorService.GetTotalLoad();
                //var maxTemp = _hardwareMonitorService.GetMaxTemperature();

                //_logger.Information($"avg temp  = {avgTemp}");
                //_logger.Information($"memory     = {memory}");
                //_logger.Information($"avg load   = {avgLoad}");
                //_logger.Information($"total load = {totalLoad}");
                //_logger.Information($"max temp   = {maxTemp}");
                //_logger.Information($"---");
        }

        private void EndExperiment()
        {
            _hardwareHandler.EnableWifi();
        }

        private void InitializeExperiment()
        {
            _hardwareHandler.DisableWifi();
        }
    }
}