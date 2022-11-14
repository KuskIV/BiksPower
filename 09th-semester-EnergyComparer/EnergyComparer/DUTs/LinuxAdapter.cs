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
        private readonly IDutAdapter _dutAdapter;

        public LinuxAdapter(ILogger logger, IDutAdapter dutAdapter)
        {
            _logger = logger;
            _dutAdapter = dutAdapter;
        }
        public void DisableNetworking(string interfaceName)
        {
            //var command = "nmcli radio wifi off";
            LinuxUtils.ExecuteCommand("/bin/nmcli", "radio wifi off");


            //ExecuteCommand(command);
        }

        public void EnableNetworking(string interfaceName)
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
            return _dutAdapter.GetTemperature();
        }

        public List<DtoMeasurement> GetCoreTemperatures()
        {
            var temp = _dutAdapter.GetTemperature();

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
