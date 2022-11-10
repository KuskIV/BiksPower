using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Utils
{
    internal class LinuxUtils
    {
        public static long ExecuteCommandGetOutputAsSudo(string filename, string argument)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash", //"/bin/cat",
                    Arguments = string.Format("-c \"sudo {0} {1}\"", filename, argument), //argument, //"/sys/class/power_supply/BAT1/capacity",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            var output = "";

            proc.Start();

            var reader = proc.StandardOutput;

            output = reader.ReadToEnd();

            proc.WaitForExit();

            return long.Parse(output.Trim());
        }
        
        public static int ExecuteCommandGetOutput(string filename, string argument)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename, //"/bin/cat",
                    Arguments = argument, //"/sys/class/power_supply/BAT1/capacity",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            var output = "";

            proc.Start();

            var reader = proc.StandardOutput;

            output = reader.ReadToEnd();

            proc.WaitForExit();

            return int.Parse(output.Trim());
        }

        public static void ExecuteCommand(string filename, string argument)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename, //"/bin/cat",
                    Arguments = argument, //"/sys/class/power_supply/BAT1/capacity",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();
        }
    }
}
