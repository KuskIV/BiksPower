using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EnergyComparer.Models
{
    public class SmartDevice
    {
        public string? ip { get; set; }
        public virtual bool state { get { return ReadState(); } }
        public SmartDevice(string ip)
        {
            this.ip = ip;
        }


        public virtual bool Toggle()
        {
            HtmlDocument _ = PingDevice("&o=1");
            return state;
        }

        public virtual void TurnOn()
        {
            if (!state)
            {
                HtmlDocument _ = PingDevice("&o=1");
            }
        }
        public virtual void TurnOff()
        {
            if (state)
            {
                HtmlDocument _ = PingDevice("&o=1");
            }
        }
        private bool ReadState()
        {
            HtmlDocument document = PingDevice();
            HtmlNode[] links = document.DocumentNode.SelectNodes("//td").ToArray();
            return links[0].InnerHtml.Equals("ON");
        }

        private HtmlDocument PingDevice(string additionalParameter = "")
        {
            var url = $"http://{ip}/?m=1" + additionalParameter;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load(url);
            return document;
        }
    }
}