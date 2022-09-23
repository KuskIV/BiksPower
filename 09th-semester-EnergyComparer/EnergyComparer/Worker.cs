using EnergyComparer.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IIntelPowerGadgetService _intelPowerGadget;

        public Worker(ILogger logger, IHardwareMonitorService hardwareMonitorService, IIntelPowerGadgetService intelPowerGadget)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
            _intelPowerGadget = intelPowerGadget;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _intelPowerGadget.Initialise();

                var avgTemp = _hardwareMonitorService.GetAverageCpuTemperature();
                var memory = _hardwareMonitorService.GetCpuMemory();
                var avgLoad = _hardwareMonitorService.GetAverageCpuLoad();
                var totalLoad = _hardwareMonitorService.GetTotalLoad();
                var maxTemp = _hardwareMonitorService.GetMaxTemperature();

                _logger.Information($"avg temp = {avgTemp}\nmax temp = {maxTemp}\nmemory={memory}\navg load = {avgLoad}\ntotal load {totalLoad}\n---");

                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}