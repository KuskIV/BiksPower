using CsvHelper.Configuration;
using CsvHelper;
using EnergyComparer;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Diagnostics.Metrics;

namespace TestClient.E3Experiment
{
    public class E3Experiments
    {
        string documentPath;
        string scriptFolder;
        string fileType;
        string fileStart;
        string fileEnd;
        Process openNotpad;
        Process openPaint;
        Process swapperIncremental;

        public E3Experiments()
        {
            documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            scriptFolder = documentPath + "\\GitHub\\BiksPower\\09-semester-ahk-scripts\\";
            fileType = ".csv";
            fileStart = "Start" + fileType;
            fileEnd = "End" + fileType;
            openNotpad = NotepadFocus();
            swapperIncremental = SwapperIncremental();
        }
        public async Task Experiment1(string exName, int duration)
        {
            //Setup
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);
            var openNotpad = NotepadFocus();
            //Experiment
            openNotpad.Start(); //Open and forcus on notpad
            await Task.Delay(5 * 1000);
            InitialMeasure.Start();
            await InitialMeasure.WaitForExitAsync();
            await Task.Delay(duration * 1000);
            FinalMeasure.Start();
            await FinalMeasure.WaitForExitAsync();
            KillProcess("Notepad");

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }

        public async Task Experiment2(string exName, int duration)
        {
            //Setup
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);
            //Experiment
            InitialMeasure.Start();
            await InitialMeasure.WaitForExitAsync();
            await Task.Delay(5 * 1000);
            openNotpad.Start(); //Open and forcus on notpad
            await Task.Delay(duration * 1000);
            FinalMeasure.Start();
            await FinalMeasure.WaitForExitAsync();
            KillProcess("Notepad");

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }

        public async Task Experiment3(string exName, int duration)
        {
            //Setup
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);

            //Experiment
            InitialMeasure.Start();
            await InitialMeasure.WaitForExitAsync();
            await Task.Delay(5 * 1000);
            openNotpad.Start(); //Open and forcus on notpad
            await Task.Delay(duration * 1000);
            KillProcess("Notepad");
            FinalMeasure.Start();
            await FinalMeasure.WaitForExitAsync();

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }

        public async Task Experiment4(string exName, int duration, int increments)
        {
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);

            //Experiment
            openNotpad.Start(); //Open and forcus on notpad
            openNotpad.Start(); //Open and forcus on notpad
            await Task.Delay(5 * 1000);
            InitialMeasure.Start();
            await InitialMeasure.WaitForExitAsync();
            for (int i = 0; i < duration * 1000; i++)
            {
                swapperIncremental.Start();
                await swapperIncremental.WaitForExitAsync();
                await Task.Delay(i);
                i += i + increments;
            }
            FinalMeasure.Start();
            await FinalMeasure.WaitForExitAsync();
            KillProcess("Notepad");

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }

        public async Task Experiment5(string exName, int duration, int fixedSwap) 
        {
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);

            //Experiment
            openNotpad.Start(); //Open and forcus on notpad
            openNotpad.Start(); //Open and forcus on notpad
            await Task.Delay(10 * 1000);
            InitialMeasure.Start();
            await InitialMeasure.WaitForExitAsync();
            for (int i = 0; i < duration * 1000; i++)
            {
                swapperIncremental.Start();
                await Task.Delay(fixedSwap);
                i += fixedSwap;
            }
            FinalMeasure.Start();
            await FinalMeasure.WaitForExitAsync();
            KillProcess("Notepad");

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }

        public async Task Experiment7(string exName, int duration, int delay)
        {
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);

            //Experiment
            await Task.Delay(10 * 1000);
            InitialMeasure.Start();
            await InitialMeasure.WaitForExitAsync();
            for (int i = 0; i < duration*1000; i++)
            {
                openNotpad.Start();
                await openNotpad.WaitForExitAsync();
                await Task.Delay(delay);
                KillProcess("Notepad");
                i += delay;
            }
            FinalMeasure.Start();
            await FinalMeasure.WaitForExitAsync();

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }
        public async Task Experiment9(string exName, int duration, int delay)
        {
            string initialFile = exName + fileStart;
            string finalFile = exName + fileEnd;
            string path = Constants.GetPathForSource(EWindowsProfilers.E3.ToString()) + "\\temp";
            var InitialMeasure = E3Save(path, initialFile);
            var FinalMeasure = E3Save(path, finalFile);
            int i = 0;
            //Experiment
            do
            {
                InitialMeasure.Start();
                await InitialMeasure.WaitForExitAsync();
                await Task.Delay(delay);
                FinalMeasure.Start();
                await FinalMeasure.WaitForExitAsync();
                i += delay;
                if (GetDiff(path, initialFile, finalFile).Count != 0) 
                {
                    Console.WriteLine("succsesfull recording");
                    Console.WriteLine(DateTime.UtcNow);
                }
            } while (i < duration*1000);

            //Fetch results
            var Temp = GetDiff(path, initialFile, finalFile);
            Dictionary<string, List<E3Data>> InactivityState = SeperateState(Temp);
            Dictionary<string, List<E3Data>> Inactive = IsolateApps(InactivityState, " NotUnique");
            Dictionary<string, List<E3Data>> Forcus = IsolateApps(InactivityState, " Focus");
            Dictionary<string, List<E3Data>> Visible = IsolateApps(InactivityState, " Visible");
            Dictionary<string, List<E3Data>> Minimized = IsolateApps(InactivityState, " Minimized");
        }
        private static List<E3Data> GetDiff(string path, string file1, string file2)
        {
            var Initial = GetE3Data(path, file1);
            var Final = GetE3Data(path, file2);
            return Final.GetRange(Initial.Count, Final.Count - Initial.Count);
        }

        private static Dictionary<string, List<E3Data>> SeperateState(List<E3Data> lst)
        {
            Dictionary<string, List<E3Data>> InactivityState = new Dictionary<string, List<E3Data>>();
            foreach (var item in lst)
            {
                if (InactivityState.ContainsKey(item.InteractivityState))
                {
                    InactivityState[item.InteractivityState].Add(item);
                }
                else
                {
                    InactivityState.Add(item.InteractivityState, new List<E3Data>());
                    InactivityState[item.InteractivityState].Add(item);

                }
            }
            return InactivityState;
        }

        private static Process E3Save(string path, string fileName)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = $" /C (cd {path.Replace(" ", "` ")}); (powercfg.exe /srumutil  /output {fileName} /csv);";
            startInfo.Verb = "runas";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            return process;
        }
        private static List<E3Data> GetE3Data(string path, string file)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
                Delimiter = ",",
            };
            using (var fs = File.Open(path + "\\" + file, FileMode.Open))
            {
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(reader, configuration))
                {
                    var data = csv.GetRecords<E3Data>();
                    return data.ToList();
                }

            }
        }

        private Process NotepadFocus()
        {
            Process notepad = new Process();
            notepad.StartInfo.FileName = scriptFolder + "Ex1_openFocus.exe";
            return notepad;
        }
        //private Process openProgram() 
        //{
        //    Process notepad = new Process();
        //    notepad.StartInfo.FileName = "";
        //    return notepad;
        //}
        private Process SwapperIncremental()
        {
            Process notepad = new Process();
            notepad.StartInfo.FileName = scriptFolder + "Ex4_SwapperIncremental.exe";
            return notepad;
        }
        public Dictionary<string, List<E3Data>> IsolateApps(Dictionary<string, List<E3Data>> dict, string key)
        {
            Dictionary<string, List<E3Data>> valuePairs = new Dictionary<string, List<E3Data>>();
            if (dict.ContainsKey(key))
            {
                foreach (var item in dict[key])
                {
                    if (valuePairs.ContainsKey(item.AppId))
                    {
                        valuePairs[item.AppId].Add(item);
                    }
                    else
                    {
                        valuePairs.Add(item.AppId, new List<E3Data>());
                        valuePairs[item.AppId].Add(item);
                    }
                }
            }
            return valuePairs;
        }

        public void KillProcess(string id) 
        {
            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses)
            {
                if (process.ProcessName.Equals(id))
                {
                    process.Kill();
                }                
            }
        }
    }
}
