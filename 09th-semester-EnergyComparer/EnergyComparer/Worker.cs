using EnergyComparer.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHardwareMonitorService _hardwareMonitorService;

        public Worker(ILogger logger, IHardwareMonitorService hardwareMonitorService)
        {
            _logger = logger;
            _hardwareMonitorService = hardwareMonitorService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _hardwareMonitorService.GetCpuTemperature();

                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}