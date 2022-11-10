using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class Plug
    {
        private string ip;

        public Plug(string ip)
        {
            this.ip = ip;
        }    

        public bool TurnOn()
        {
            var url = $"http://{ip}/?m=1&o=1";
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000,  // 1 second
            };
            var client = new RestClient(options);
            var request = new RestRequest(url, Method.Get);
            RestResponse response = client.Execute(request);
            return true;
        }
        public bool TurnOff()
        {
            var url = $"http://{ip}/?m=1&o=0";
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000,  // 1 second
            };
            var client = new RestClient(options);
            var request = new RestRequest(url, Method.Get);
            RestResponse response = client.Execute(request);
            return false;
        }
    }
}
