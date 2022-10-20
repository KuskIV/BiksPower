using EnergyComparer.Models;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

        public DtoRawData ParseCsv(string path, int experimentId, DateTime startTime)
        {
            throw new NotImplementedException();
        }

        public void Start(DateTime date)
        {
            CreateXMLFILE();
            _timer = new System.Timers.Timer(1000 * IntervalBetweenReadsInSeconds); // 10 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
            Console.WriteLine("Timer has started");
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.Write("in timer_Elapsed");
            var avgCpuTemp = _hardwareMonitorService.GetAverageCpuTemperature();
            var time = DateTime.UtcNow;

            var data = new HardwareMonitorData()
            {
                avgTemperature = avgCpuTemp,
                time = time,
            };

            AddRecordToXML(data);
            Console.Write("doing gods work");
        }

        public void CreateXMLFILE()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Run");
            doc.AppendChild(root);
            doc.Save(@"D:\test.xml");
            Console.WriteLine("XML file created");
        }
        public void AddRecordToXML(HardwareMonitorData hwmd)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"D:\test.xml");
            Console.WriteLine("XML opened");
            var rootNode = doc.GetElementsByTagName("Run")[0];
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
    }
}
