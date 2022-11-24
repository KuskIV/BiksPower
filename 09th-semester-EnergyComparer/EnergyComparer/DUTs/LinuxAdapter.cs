using EnergyComparer.Models;
using EnergyComparer.Utils;
using Google.Protobuf.WellKnownTypes;
using Serilog;
using System.Diagnostics;
using System.Xml.Linq;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.DUTs
{
    internal class LinuxAdapter : IOperatingSystemAdapter
    {
        private readonly ILogger _logger;
        private readonly string _machineName;

        public LinuxAdapter(ILogger logger, string machineName)
        {
            _logger = logger;
            _machineName = machineName;
        }
        public void DisableWifi(string interfaceName)
        {
            //var command = "nmcli radio wifi off";
            LinuxUtils.ExecuteCommand("/bin/nmcli", "radio wifi off");


            //ExecuteCommand(command);
        }

        public void EnableWifi(string interfaceName)
        {
            //var command = "nmcli radio wifi on";

            LinuxUtils.ExecuteCommand("/bin/nmcli", "radio wifi on");


            //ExecuteCommand(command);
        }

        public void Restart()
        {
            //var command = "sudo reboot";

            LinuxUtils.ExecuteCommandAsSudo("reboot", "");

            //ExecuteCommand(command);
        }

        public void Shutdowm()
        {
            LinuxUtils.ExecuteCommandAsSudo("shutdown", "-n now");

            //var command = "sudo shutdown -n now";

            //ExecuteCommand(command);
        }

        public void StopunneccesaryProcesses()
        {
            _logger.Warning("LINUX stop background processes is yet to be implemented");
        }

        public float GetAverageCpuTemperature()
        {
            return GetTemperature();
        }

        private float GetTemperature()
        {
            float temperature = 0;
            if (_machineName.Equals(Constants.PowerKomplett))
            {
                temperature = LinuxUtils.ExecuteCommandGetOutput("/bin/cat", "/sys/class/thermal/thermal_zone5/temp");
            }else if (_machineName.Equals(Constants.SurfaceBook))
            {
                temperature = LinuxUtils.ExecuteCommandGetOutput("/bin/cat", "/sys/class/thermal/thermal_zone8/temp");

            }else if (_machineName.Equals(Constants.SurfacePro))
            {
                temperature = LinuxUtils.ExecuteCommandGetOutput("/bin/cat", "/sys/class/thermal/thermal_zone5/temp");
            }
            else
            {
                throw new NotImplementedException($"Machine name does not exsist {_machineName}");
            }

            return temperature / 1000;
        }

        public List<DtoMeasurement> GetCoreTemperatures()
        {
            var temp = GetTemperature();

            return new List<DtoMeasurement>()
            {
                new DtoMeasurement()
                {
                    Name = "LinuxSocket",
                    Value = temp,
                    Type = EMeasurementType.CpuTemperature.ToString()
                }
            };
        }
    }
}
