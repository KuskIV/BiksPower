using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Services
{
    public interface IHardwareMonitorService
    {
        void GetCpuTemperature();
    }

    public class HardwareMonitorService : IHardwareMonitorService
    {
        private readonly ILogger _logger;

        public HardwareMonitorService(ILogger logger)
        {
            _logger = logger;
        }

        public void GetCpuTemperature()
        {
            throw new NotImplementedException();
        }
    }
}
