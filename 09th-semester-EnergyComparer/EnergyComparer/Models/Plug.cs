using EnergyComparer.Models;
using HtmlAgilityPack;
using SmartHomeController.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartHomeController.Devices
{
    public class Plug : SmartDevice
    {

        public PlugStatus Status
        {
            get
            {
                return GetStatus();
            }
        }

        public Plug(string ip) : base(ip)
        {
        }

        private float ReadVoltage(string document)
        {
            return ValueReader("Voltage", document);
        }
        private float ReadCurrent(string document)
        {
            return ValueReader("Current", document);
        }
        private float ReadPower(string document)
        {
            return ValueReader("Power", document);
        }
        private float ReadApparentPower(string document)
        {
            return ValueReader("Apparent Power", document);
        }
        private float ReadReactivePower(string document)
        {
            return ValueReader("Reactive Power", document);
        }
        private float ReadPowerFactor(string document)
        {
            return ValueReader("Power Factor", document);
        }
        private float ReadEnergyToday(string document)
        {
            return ValueReader("Energy Today", document);
        }
        private float ReadEnergyYesterday(string document)
        {
            return ValueReader("Energy Yesterday", document);
        }
        private float ReadEnergyTotal(string document)
        {
            return ValueReader("Energy Total", document);
        }
        private bool ReadState(HtmlDocument document)
        {
            return StateReader(document);
        }

        private PlugStatus GetStatus()
        {
            var url = $"http://{ip}/?m=1";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(url);
            string document = htmlDoc.DocumentNode.InnerHtml;
            PlugStatus plugStatus = new PlugStatus()
            {
                Voltage = ReadVoltage(document),
                Current = ReadCurrent(document),
                Power = ReadPower(document),
                ApparentPower = ReadApparentPower(document),
                ReactivePower = ReadReactivePower(document),
                PowerFactor = ReadPowerFactor(document),
                EnergyToday = ReadEnergyToday(document),
                EnergyYesterday = ReadEnergyYesterday(document),
                EnergyEnergyTotal = ReadEnergyTotal(document),
                On = ReadState(htmlDoc)
            };
            return plugStatus;
        }

        private float ValueReader(string val, string document)
        {
            Regex rx = new Regex(val + @"{m}\s*(\d+|\.)+");
            return ParseField(document, rx);
        }

        private bool StateReader(HtmlDocument document)
        {
            HtmlNode[] links = document.DocumentNode.SelectNodes("//td").ToArray();
            return links[0].InnerHtml.Equals("ON");
        }

        private float ParseField(string doc, Regex regex)
        {
            Regex number = new Regex(@"(\d|\.\d+)+");
            string regionOfIntrest = regex.Match(doc).Value;
            string num = number.Match(regionOfIntrest).Value;
            return float.Parse(num, CultureInfo.InvariantCulture);
        }
    }
}