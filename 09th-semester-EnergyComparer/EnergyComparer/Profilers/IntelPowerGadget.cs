using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using EnergyComparer.Models;
using EnergyComparer.Services;
using Microsoft.VisualBasic;

namespace EnergyComparer.Profilers
{
    public class IntelPowerGadget : IEnergyProfiler
    {

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool IntelEnergyLibInitialize();

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool StartLog(string szFileName);

        [DllImport("EnergyLib64.dll", CharSet = CharSet.Unicode)]
        private static extern bool StopLog();


        private readonly EWindowsProfilers _source;

        public IntelPowerGadget()
        {
            _source = EWindowsProfilers.IntelPowerGadget;
            Initialise();
        }

        public void Start(DateTime date)
        {
            var path = Constants.GetFilePathForSouce(_source.ToString(), date);

            var success = StartLog(path);
            EnsureSuccess(success);
        }

        public void Stop()
        {
            var success = StopLog();
            EnsureSuccess(success);
        }

        public void Initialise()
        {
            var success = IntelEnergyLibInitialize();
            EnsureSuccess(success);
        }

        private static void EnsureSuccess(bool success, [CallerMemberName] string callerName = "")
        {
            if (!success)
            {
                throw new Exception($"IntelPowerGadget failed to {callerName}");
            }
        }

        public string GetName()
        {
            return _source.ToString();
        }

        public DtoRawData ParseCsv(string path, int experimentId, DateTime startTime)
        {
            var lines = System.IO.File.ReadLines(path).ToList();

            var properties = lines[1].Split(',');

            var data = new IntelPowerGadgetData();

            if (lines.Count > 21)
            {
                data = new IntelPowerGadgetData()
                {
                    SystemTime = DateTime.ParseExact($"01/01/0001 {properties[0]}", "MM/dd/yyyy HH:mm:ss:fff", CultureInfo.CreateSpecificCulture("da-DK")).TimeOfDay,
                    RDTSC = Double.Parse(properties[1]),
                    ElapsedTimesec = Double.Parse(properties[2]),
                    CPUUtilization = Double.Parse(properties[3]),
                    CPUFrequency_0MHz = Double.Parse(properties[4]),
                    ProcessorPower_0Watt = Double.Parse(properties[5]),
                    CumulativeProcessorEnergy_0Joules = Double.Parse(properties[6]),
                    CumulativeProcessorEnergy_0mWh = Double.Parse(properties[7]),
                    IAPower_0Watt = Double.Parse(properties[8]),
                    CumulativeIAEnergy_0Joules = Double.Parse(properties[9]),
                    CumulativeIAEnergy_0mWh = Double.Parse(properties[10]),
                    PackageTemperature_0C = Double.Parse(properties[11]),
                    PackageHot_0 = Double.Parse(properties[12]),
                    DRAMPower_0Watt = Double.Parse(properties[13]),
                    CumulativeDRAMEnergy_0Joules = Double.Parse(properties[14]),
                    CumulativeDRAMEnergy_0mWh = Double.Parse(properties[15]),
                    GTPower_0Watt = lines.Count > 21 ? Double.Parse(properties[16]) : -1,
                    CumulativeGTEnergy_0Joules = lines.Count > 21 ? Double.Parse(properties[17]) : -1,
                    CumulativeGTEnergy_0mWh = lines.Count > 21 ? Double.Parse(properties[18]) : -1,
                    PackagePL1_0Watt = Double.Parse(properties[19]),
                    PackagePL2_0Watt = Double.Parse(properties[20]),
                    PackagePL4_0Watt = Double.Parse(properties[21]),
                    PlatformPsysPL1_0Watt = Double.Parse(properties[22]),
                    PlatformPsysPL2_0Watt = Double.Parse(properties[23]),
                    GTFrequencyMHz = lines.Count > 21 ? Double.Parse(properties[24]) : -1,
                    GTUtilization = lines.Count > 21 ? Double.Parse(properties[25]) : -1,
                    TotalElapsedTimeInSeconds = GetValue(lines, 3),
                    MeasuredRdtscFrequencyInGhz = GetValue(lines, 4),
                    AverageProcessorPower0InWatt = GetValue(lines, 8),
                    AverageIAPower0InWatt = GetValue(lines, 12),
                    AverageDRAMPower0InWatt = GetValue(lines, 16),
                    AverageGTPower0InWatt = lines.Count > 21 ? GetValue(lines, 20) : -1,
                };
            }
            else
            {
                data = new IntelPowerGadgetData()
                {
                    SystemTime = DateTime.ParseExact($"01/01/0001 {properties[0]}", "MM/dd/yyyy HH:mm:ss:fff", CultureInfo.CreateSpecificCulture("da-DK")).TimeOfDay,
                    RDTSC = Double.Parse(properties[1]),
                    ElapsedTimesec = Double.Parse(properties[2]),
                    CPUUtilization = Double.Parse(properties[3]),
                    CPUFrequency_0MHz = Double.Parse(properties[4]),
                    ProcessorPower_0Watt = Double.Parse(properties[5]),
                    CumulativeProcessorEnergy_0Joules = Double.Parse(properties[6]),
                    CumulativeProcessorEnergy_0mWh = Double.Parse(properties[7]),
                    IAPower_0Watt = Double.Parse(properties[8]),
                    CumulativeIAEnergy_0Joules = Double.Parse(properties[9]),
                    CumulativeIAEnergy_0mWh = Double.Parse(properties[10]),
                    PackageTemperature_0C = Double.Parse(properties[11]),
                    PackageHot_0 = Double.Parse(properties[12]),
                    DRAMPower_0Watt = Double.Parse(properties[13]),
                    CumulativeDRAMEnergy_0Joules = Double.Parse(properties[14]),
                    CumulativeDRAMEnergy_0mWh = Double.Parse(properties[15]),
                    PackagePL1_0Watt = Double.Parse(properties[16]),
                    PackagePL2_0Watt = Double.Parse(properties[17]),
                    PackagePL4_0Watt = Double.Parse(properties[18]),
                    PlatformPsysPL1_0Watt = Double.Parse(properties[19]),
                    PlatformPsysPL2_0Watt = Double.Parse(properties[20]),
                    TotalElapsedTimeInSeconds = GetValue(lines, 3),
                    MeasuredRdtscFrequencyInGhz = GetValue(lines, 4),
                    AverageProcessorPower0InWatt = GetValue(lines, 8),
                    AverageIAPower0InWatt = GetValue(lines, 12),
                    AverageDRAMPower0InWatt = GetValue(lines, 16),
                    AverageGTPower0InWatt = -1,
                    CumulativeGTEnergy_0Joules = -1,
                    CumulativeGTEnergy_0mWh = -1,
                    GTFrequencyMHz = -1,
                    GTPower_0Watt = -1,
                    GTUtilization = -1
                };
            }

            return new DtoRawData()
            {
                ExperimentId = experimentId,
                Time = startTime,
                Value = JsonSerializer.Serialize(data)
            };
        }

        private static float GetValue(List<string> lines, int index)
        {
            return float.Parse(lines[index].Split('=')[1].Trim());
        }
    }
}
