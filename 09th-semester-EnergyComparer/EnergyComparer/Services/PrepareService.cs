using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class PrepareService : IPrepareService
    {
        private readonly ILogger _logger;

        public PrepareService(ILogger logger)
        {
            _logger = logger;
        }

        public void DisableWifi()
        {
            _logger.Information("About to disable wifi...");
            System.Diagnostics.Process.Start("ipconfig", "/release");
        }

        public void EnableWifi()
        {
            _logger.Information("About to enable wifi...");
            System.Diagnostics.Process.Start("ipconfig", "/renew");
        }
    }

    public interface IPrepareService
    {
        void DisableWifi();
        void EnableWifi();
    }
}
