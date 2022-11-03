using System.Diagnostics;

namespace EnergyComparer.DUTs
{
    internal class LinuxAdapter : IOperatingSystemAdapter
    {
        public void DisableWifi(string interfaceName)
        {
            var command = "nmcli radio wifi off";

            ExecuteCommand(command);
        }

        public void EnableWifi(string interfaceName)
        {
            var command = "nmcli radio wifi on";

            ExecuteCommand(command);
        }

        public void Restart()
        {
            var command = "sudo reboot";

            ExecuteCommand(command);
        }

        public void Shutdowm()
        {
            var command = "sudo shutdown -n now";

            ExecuteCommand(command);
        }

        public void StopunneccesaryProcesses()
        {
            throw new NotImplementedException();
        }

        private static void ExecuteCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = command, };
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
        }
    }
}
