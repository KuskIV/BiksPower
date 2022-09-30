using EnergyComparer.Services;
using EnergyComparer.Utils;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IIntelPowerGadgetService _intelPowerGadget;
        private readonly IHardwareService _prepare;

        public Worker(ILogger logger, IHardwareMonitorService hardwareMonitorService, IIntelPowerGadgetService intelPowerGadget, IHardwareService prepare)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
            _intelPowerGadget = intelPowerGadget;
            _prepare = prepare;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _intelPowerGadget.Initialise(); // TODO: Move to intel powergadget

                InitializeExperiment();

                var a = _hardwareMonitorService.GetCoreTemperatures();

                EndExperiment();

                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);



                var avgTemp = _hardwareMonitorService.GetAverageCpuTemperature();
                var memory = _hardwareMonitorService.GetCpuMemory();
                var avgLoad = _hardwareMonitorService.GetAverageCpuLoad();
                var totalLoad = _hardwareMonitorService.GetTotalLoad();
                var maxTemp = _hardwareMonitorService.GetMaxTemperature();

                _logger.Information($"avg temp  = {avgTemp}");
                _logger.Information($"memory     = {memory}");
                _logger.Information($"avg load   = {avgLoad}");
                _logger.Information($"total load = {totalLoad}");
                _logger.Information($"max temp   = {maxTemp}");
                _logger.Information($"---");

            }
        }

        private void EndExperiment()
        {
            _prepare.EnableWifi();
        }

        private void InitializeExperiment()
        {
            _prepare.DisableWifi();
        }
    }
}