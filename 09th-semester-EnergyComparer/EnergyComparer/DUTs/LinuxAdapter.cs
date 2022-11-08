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

        public LinuxAdapter(ILogger logger)
        {
            _logger = logger;
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

            LinuxUtils.ExecuteCommand("/sbin/reboot", "");

            //ExecuteCommand(command);
        }

        public void Shutdowm()
        {
            LinuxUtils.ExecuteCommand("/sbin/shutdown", "-n now");

            //var command = "sudo shutdown -n now";

            //ExecuteCommand(command);
        }

        public void StopunneccesaryProcesses()
        {
            _logger.Warning("LINUX stop background processes is yet to be implemented");
        }

        private static void ExecuteCommand(string command)
        {
            var startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = command, };
            var proc = new Process() { StartInfo = startInfo, };
            proc.Start();
        }

        public float GetAverageCpuTemperature()
        {
            return GetTemperature();
        }

        private static float GetTemperature()
        {
            var temperature = LinuxUtils.ExecuteCommandGetOutput("/bin/cat", "/sys/class/thermal/thermal_zone5/temp");

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
