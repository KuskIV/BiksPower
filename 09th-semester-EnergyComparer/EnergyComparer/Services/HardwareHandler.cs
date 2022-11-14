using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.DUTs;
using Microsoft.AspNetCore.Hosting.Server;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class HardwareHandler : IHardwareHandler, IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _wifiAdapterName;
        private readonly IOperatingSystemAdapter _adapterService;

        public HardwareHandler(ILogger logger, string wifiAdapterName, IOperatingSystemAdapter adapterService)
        {
            _logger = logger;
            _wifiAdapterName = wifiAdapterName;
            _adapterService = adapterService;
        }

        public void DisableWifi()
        {
            _logger.Information("About to disable wifi...");
            _adapterService.DisableNetworking(_wifiAdapterName);
        }

        public void EnableWifi()
        {
            _logger.Information("About to enable wifi...");
            _adapterService.EnableNetworking(_wifiAdapterName);
        }

        public void Dispose()
        {
            try
            {
                _logger.Information("Re-enbeling wifi upon shutdown.");
                _adapterService.EnableNetworking(_wifiAdapterName);

            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception when trying to reenable wifi adapter");
            }
        }
    }

    public interface IHardwareHandler
    {
        void DisableWifi();
        void EnableWifi();
    }
}
