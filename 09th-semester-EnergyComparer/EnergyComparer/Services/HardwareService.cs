using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class HardwareHandler : IHardwareHandler, IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _wifiAdapterName;
        private readonly IAdapterService _adapterService;

        public HardwareHandler(ILogger logger, string wifiAdapterName, IAdapterService adapterService)
        {
            _logger = logger;
            _wifiAdapterName = wifiAdapterName;
            _adapterService = adapterService;
        }

        public void EnsurePathsExists()
        {
            foreach (var path in _adapterService.GetAllRequiredPaths())
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public void DisableWifi()
        {
            _logger.Information("About to disable wifi...");
            _adapterService.DisableWifi(_wifiAdapterName);
        }

        public void EnableWifi()
        {
            _logger.Information("About to enable wifi...");
            _adapterService.EnableWifi(_wifiAdapterName);
        }

        public void Dispose()
        {
            try
            {
                _logger.Information("Re-enbeling wifi upon shutdown.");
                _adapterService.EnableWifi(_wifiAdapterName);

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
        void EnsurePathsExists();
    }
}
