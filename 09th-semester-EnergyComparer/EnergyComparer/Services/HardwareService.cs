using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Utils;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class HardwareService : IHardwareService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly string wifiAdapterName = "Wi-Fi";

        public HardwareService(ILogger logger)
        {
            _logger = logger;
        }

        public void DisableWifi()
        {
            _logger.Information("About to disable wifi...");
             AdapterUtils.Disable(wifiAdapterName);
        }

        public void EnableWifi()
        {
            _logger.Information("About to enable wifi...");
            AdapterUtils.Enable(wifiAdapterName);
        }

        public void Dispose()
        {
            try
            {
                _logger.Information("Re-enbeling wifi upon shutdown.");
                AdapterUtils.Enable(wifiAdapterName);

            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception when trying to reenable wifi adapter");
            }
        }
    }

    public interface IHardwareService
    {
        void DisableWifi();
        void EnableWifi();
    }
}
