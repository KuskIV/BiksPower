using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Utils;
using Microsoft.AspNetCore.Hosting.Server;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class HardwareHandler : IHardwareHandler, IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _wifiAdapterName;

        public HardwareHandler(ILogger logger, string wifiAdapterName)
        {
            _logger = logger;
            _wifiAdapterName = wifiAdapterName;
        }

        public void EnsurePathsExists()
        {
            foreach (var path in Constants.GetAllRequiredPaths())
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
             AdapterUtils.Disable(_wifiAdapterName);
        }

        public void EnableWifi()
        {
            _logger.Information("About to enable wifi...");
            AdapterUtils.Enable(_wifiAdapterName);
        }

        public void Dispose()
        {
            try
            {
                _logger.Information("Re-enbeling wifi upon shutdown.");
                AdapterUtils.Enable(_wifiAdapterName);

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
