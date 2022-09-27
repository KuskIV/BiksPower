using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerManagerTest
{
    public class E3Measurement
    {
        public string LogLocation = "";
        public string Problem;
        public string Machine;
        public string Tool;
        private DateTime _beginTime;
        private DateTime _endTime;

        public E3Measurement(string logLocation, string problem, string machine, string tool)
        {
            LogLocation = logLocation;
            Problem = problem;
            Machine = machine;
            Tool = tool;   
        }

        public void Start()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = "  /C (cd C:\\Windows\\System32\\sru) ; (sc stop dps) ; (rm .\\SRUDB.dat);  (sc start dps) ; (Start-Sleep -Seconds 10.0)";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
            _beginTime = DateTime.Now;
        }

        public void End()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = $"  /C (cd C:\\Notes); (powercfg.exe /srumutil  /output {Machine+"_"+Problem+"_"+Tool} /csv); (Start-Sleep -Seconds 10.0)";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
            _endTime = DateTime.Now;
        }

    }
}
