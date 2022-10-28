using EnergyComparer.Models;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EnergyComparer.Profilers
{
    public class HardwareMonitor : IEnergyProfiler
    {
        private System.Timers.Timer _timer;
        private HardwareMonitorService _hardwareMonitorService;
        private readonly int IntervalBetweenReadsInSeconds = 1;

        public HardwareMonitor()
        {
            _hardwareMonitorService = new HardwareMonitorService(null);
        }

        public string GetName()
        {
            return EWindowsProfilers.HardwareMonitor.ToString();
        }

        public void Start(DateTime date)
        {
            var path = Constants.GetFilePathForSouce(EWindowsProfilers.HardwareMonitor.ToString(), date);
            CreateXMLFILE();
            _timer = new System.Timers.Timer(1000 * IntervalBetweenReadsInSeconds); // 10 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
            Console.WriteLine("Timer has started");
        }

        public void Stop()
        {
            _timer.Enabled = false;
            Console.WriteLine("Stopped");
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _hardwareMonitorService.UpdateCpuValues();

            var time = DateTime.UtcNow;
            var avgCpuLoad = _hardwareMonitorService.GetAverageCpuLoad(false);
            var totalCpuLoad = _hardwareMonitorService.GetTotalLoad(false);
            var cpuCore1T1 = _hardwareMonitorService.GetCpuCore1T1(false);
            var cpuCore1T2 = _hardwareMonitorService.GetCpuCore1T2(false);
            var cpuCore2T1 = _hardwareMonitorService.GetCpuCore2T1(false);
            var cpuCore2T2 = _hardwareMonitorService.GetCpuCore2T2(false);
            var cpuCore3T1 = _hardwareMonitorService.GetCpuCore3T1(false);
            var cpuCore3T2 = _hardwareMonitorService.GetCpuCore3T2(false);
            var cpuCore4T1 = _hardwareMonitorService.GetCpuCore4T1(false);
            var cpuCore4T2 = _hardwareMonitorService.GetCpuCore4T2(false);
            var cpuCore5T1 = _hardwareMonitorService.GetCpuCore5T1(false);
            var cpuCore5T2 = _hardwareMonitorService.GetCpuCore5T2(false);
            var cpuCore6T1 = _hardwareMonitorService.GetCpuCore6T1(false);
            var cpuCore6T2 = _hardwareMonitorService.GetCpuCore6T2(false);

            // Temperatures
            var cpuMaxTemperature = _hardwareMonitorService.GetMaxTemperature(false);
            var cpuAvgTemperature = _hardwareMonitorService.GetAverageCpuTemperature(false);
            var cpuCore1Temperature = _hardwareMonitorService.GetCpuCore1Temp(false);
            var cpuCore2Temperature = _hardwareMonitorService.GetCpuCore2Temp(false);
            var cpuCore3Temperature = _hardwareMonitorService.GetCpuCore3Temp(false);
            var cpuCore4Temperature = _hardwareMonitorService.GetCpuCore4Temp(false);
            var cpuCore5Temperature = _hardwareMonitorService.GetCpuCore5Temp(false);
            var cpuCore6Temperature = _hardwareMonitorService.GetCpuCore6Temp(false);

            // Core clocks
            var cpuCore1Clock = _hardwareMonitorService.GetCpuClockC1(false);
            var cpuCore2Clock = _hardwareMonitorService.GetCpuClockC2(false);
            var cpuCore3Clock = _hardwareMonitorService.GetCpuClockC3(false);
            var cpuCore4Clock = _hardwareMonitorService.GetCpuClockC4(false);
            var cpuCore5Clock = _hardwareMonitorService.GetCpuClockC5(false);
            var cpuCore6Clock = _hardwareMonitorService.GetCpuClockC6(false);
            var cpuBusSpeed = _hardwareMonitorService.GetCpuBusSpeed(false);

            // Power
            var cpuPowerPacket = _hardwareMonitorService.GetCpuPowerPacket(false);
            var cpuPowerCores = _hardwareMonitorService.GetCpuPowerCores(false);
            var cpuPowerMemory = _hardwareMonitorService.GetCpuPowerMemory(false);

            //Voltage
            var cpuVoltageCores = _hardwareMonitorService.GetCpuVoltage(false);
            var cpuVoltageC1 = _hardwareMonitorService.GetCpuVoltageC1(false);
            var cpuVoltageC2 = _hardwareMonitorService.GetCpuVoltageC2(false);
            var cpuVoltageC3 = _hardwareMonitorService.GetCpuVoltageC3(false);
            var cpuVoltageC4 = _hardwareMonitorService.GetCpuVoltageC4(false);
            var cpuVoltageC5 = _hardwareMonitorService.GetCpuVoltageC5(false);
            var cpuVoltageC6 = _hardwareMonitorService.GetCpuVoltageC6(false);

            var data = new HardwareMonitorData()
            {
                time = time,
                // Load
                avgLoad = avgCpuLoad,
                totalLoad = totalCpuLoad,
                cpuC1T1 = cpuCore1T1,
                cpuC1T2 = cpuCore1T2,
                cpuC2T1 = cpuCore2T1,
                cpuC2T2 = cpuCore2T2,
                cpuC3T1 = cpuCore3T1,
                cpuC3T2 = cpuCore3T2,
                cpuC4T1 = cpuCore4T1,
                cpuC4T2 = cpuCore4T2,
                cpuC5T1 = cpuCore5T1,
                cpuC5T2 = cpuCore5T2,
                cpuC6T1 = cpuCore6T1,
                cpuC6T2 = cpuCore6T2,

                // Temperature
                cpuMaxTemp = cpuMaxTemperature,
                cpuAvgTemp = cpuAvgTemperature,
                cpuC1Temp = cpuCore1Temperature,
                cpuC2Temp = cpuCore2Temperature,
                cpuC3Temp = cpuCore3Temperature,
                cpuC4Temp = cpuCore4Temperature,
                cpuC5Temp = cpuCore5Temperature,
                cpuC6Temp = cpuCore6Temperature,

                // Core clocks
                cpuC1Clock = cpuCore1Clock,
                cpuC2Clock = cpuCore2Clock,
                cpuC3Clock = cpuCore3Clock,
                cpuC4Clock = cpuCore4Clock,
                cpuC5Clock = cpuCore5Clock,
                cpuC6Clock = cpuCore6Clock,
                cpuBusSpeed = cpuBusSpeed,

                // Power
                cpuPowerCores = cpuPowerCores,
                cpuPowerPacket = cpuPowerPacket,
                cpuPowerMemory = cpuPowerMemory,

                // Voltage
                cpuVoltageCores = cpuVoltageCores,
                cpuVoltageC1 = cpuVoltageC1,
                cpuVoltageC2 = cpuVoltageC2,
                cpuVoltageC3 = cpuVoltageC3,
                cpuVoltageC4 = cpuVoltageC4,
                cpuVoltageC5 = cpuVoltageC5,
                cpuVoltageC6 = cpuVoltageC6,



            };

            AddRecordToXML(data);
        }

        public void CreateXMLFILE()
        {
            XmlDocument doc = new XmlDocument();
            Console.WriteLine("XML file created");
            XmlElement root = doc.CreateElement("Sample");
            doc.AppendChild(root);
            doc.Save(@"D:\test.xml");
            Console.WriteLine("New XML file saved");
        }

        // Implement that XML stays open until done.
        public void AddRecordToXML(HardwareMonitorData hwmd)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"D:\test.xml");
            //Console.WriteLine("XML opened");
            var rootNode = doc.GetElementsByTagName("Sample")[0];
            var nav = rootNode.CreateNavigator();
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] {
            XmlQualifiedName.Empty
            });

            using (var writer = nav.AppendChild())
            {
                var serializer = new XmlSerializer(hwmd.GetType());
                writer.WriteWhitespace("");
                serializer.Serialize(writer, hwmd, emptyNamepsaces);
                writer.Close();
            }
            doc.Save(@"D:\test.xml");
        }

        public (DtoTimeSeries, DtoRawData) ParseData(string path, int experimentId, DateTime startTime)
        {
            throw new NotImplementedException();
        }
    }
}
