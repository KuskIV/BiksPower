using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using EnergyComparer.TestCases;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.DUTs
{
    public class WindowsAdapter : IOperatingSystemAdapter
    {
        private readonly ILogger _logger;
        private readonly bool _isProd;
        private readonly bool _shouldRestart;
        private readonly IHardwareMonitorService _hardwareMonitorService;
        private readonly IDutAdapter _dutAdapter;

        public WindowsAdapter(ILogger logger, bool isProd, bool shouldRestart, IHardwareMonitorService hardwareMonitorService, IDutAdapter dutAdapter)
        {
            _logger = logger;
            _isProd = isProd;
            _shouldRestart = shouldRestart;
            _hardwareMonitorService = hardwareMonitorService;
            _dutAdapter = dutAdapter;
        }

        public void EnableNetworking(string interfaceName)
        {
            _dutAdapter.EnableNetworking(interfaceName);
        }

        public void DisableNetworking(string interfaceName)
        {
            _dutAdapter.DisableNetworking(interfaceName);
        }

        public void Restart()
        {
            if (_shouldRestart)
            {
                Process.Start("ShutDown", "/r /t 0");
            }
        }

        public void Shutdowm()
        {
            if (_isProd)
            {
                Process.Start("ShutDown", "/s /t 0");
            }
        }

        public void StopunneccesaryProcesses()
        {
            if (_isProd)
            {
                _logger.Information("About to disable background processes");
                var ps = PowerShell.Create();

                foreach (var process in WindowsProcessesToStop())
                {
                    try
                    {
                        ps.AddCommand("Stop-Process").AddParameter("Name", process);
                        ps.Invoke();
                    }
                    catch (Exception)
                    {
                        _logger.Warning("Unable to stop process '{name}'. Will continue...", process);
                    }
                }
            }
            else
            {
                _logger.Information("No background processes will be disabled as it is dev");
            }
        }

        public static List<string> WindowsProcessesToStop()
        {
            return new List<string>()
            {
                "AsusUpdateCheck",
                "AsusDownLoadLicense",
                "msedge",
                "OneDrive",
                "GitHubDesktop",
                "Microsoft.Photos",
                "SkypeApp",
                "SkypeBackgroundHost",
            };
        }

        public float GetAverageCpuTemperature()
        {
            return _dutAdapter.GetTemperature();
        }

        public List<DtoMeasurement> GetCoreTemperatures()
        {
            return _hardwareMonitorService.GetCoreTemperatures();
        }
    }
}
