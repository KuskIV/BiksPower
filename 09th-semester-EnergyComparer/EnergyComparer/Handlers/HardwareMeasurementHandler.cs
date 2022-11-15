using EnergyComparer.Models;
using Google.Protobuf.WellKnownTypes;
using LibreHardwareMonitor.Hardware;
using MySql.Data.MySqlClient.Memcached;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Method = RestSharp.Method;

namespace EnergyComparer.Handlers
{
    public static class HardwareMeasurementHandler
    {
        public static void StartMeasurement(string message) 
        {
            var url = "http://stemlevelup.com/api/RaspberryPi/StartHW";
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000,  // 1 second
            };
            var client = new RestClient(options);
            var request = new RestRequest(url ,Method.Post);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
            request.AddHeader("Content-Type", "application/json");
            var body = $"{message}";
            var bodyy = JsonConvert.SerializeObject(body);
            request.AddBody(bodyy, "application/json");
            RestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
        }
        public static void EndMeasurement(string message)
        {
            var url = "http://stemlevelup.com/api/RaspberryPi/EndHW";
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000,  // 1 second
            };
            var client = new RestClient(options);
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
            request.AddHeader("Content-Type", "application/json");
            var body = $"{message}";
            var bodyy = JsonConvert.SerializeObject(body);
            request.AddBody(bodyy, "application/json");
            RestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
        }

        public static void SetExId(int id) 
        {
            var url = "http://stemlevelup.com/api/RaspberryPi/Set";
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000,  // 1 second
            };
            var client = new RestClient(options);
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
            request.AddHeader("Content-Type", "application/json");
            var body = $"{id}";
            var bodyy = JsonConvert.SerializeObject(body);
            request.AddBody(bodyy, "application/json");
            RestResponse response = client.Execute(request);
        }

        public static async Task GetHardwareState() 
        {
            var url = "http://stemlevelup.com/api/RaspberryPi";
            var client = new RestClient(url);
            var request = new RestRequest(url, Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            //var Output = response.Content;
        }

        public static async Task<HardwareResults> GetResults() 
        {
            var url = "http://stemlevelup.com/api/RaspberryPi/Results";
            HardwareResults hr = new HardwareResults();
            do
            {
                var client = new RestClient(url);
                var request = new RestRequest(url, Method.Get);
                RestResponse response = await client.ExecuteAsync(request);
                string DirtyJson = response.Content;
                string CleanerJson = DirtyJson.Replace("\\u0022", "\u0022");
                string EvenCleaner = CleanerJson.Replace("\\\"","\"");
                string Cleanest1 = EvenCleaner.Replace("\"[", "[");
                string Cleanest2 = Cleanest1.Replace("]\"", "]");
                string Cleanest3 = Cleanest2.Replace("\"{", "{");
                string Cleanest4 = Cleanest3.Replace("}\"", "}");
                if (!Cleanest4.Equals("{\"TimeSeries\":\"\",\"Raw\":\"test\"}")) 
                {
                    hr = JsonSerializer.Deserialize<HardwareResults>(Cleanest4)!;
                    break;
                }
            } while (true);
            hr.TimeSeries = hr.TimeSeries.Where(x => !ContainsNull(x)).ToList();
            double C1TrueRMSRAW = hr.TimeSeries.Sum(x => x.C1TrueRMSPower.Value);
            double C1ACRMSRAW = hr.TimeSeries.Sum(x => x.C1ACRMSPower.Value);
            hr.Raw = JsonSerializer.Serialize(new HardwareRaw(C1TrueRMSRAW, C1ACRMSRAW));
            ResetResults();
            return hr;
        }

        private static bool ContainsNull(TimeSeries timeSeries) 
        {
            return !(timeSeries.C1TrueRMSPower.HasValue && timeSeries.C1TrueRMS.HasValue && timeSeries.C1ACRMSPower.HasValue && timeSeries.C1ACRMSPower.HasValue);
        }

        private static void ResetResults() 
        {
            string empty = "test";
            var url = "http://stemlevelup.com/api/RaspberryPi/Results?rawJson="+empty;
            var options = new RestClientOptions(url)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000,  // 1 second
            };
            var client = new RestClient(options);
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
            request.AddHeader("Content-Type", "application/json");
            var body = $"";
            var bodyy = JsonConvert.SerializeObject(body);
            request.AddBody(bodyy, "application/json");
            RestResponse response = client.Execute(request);
        }
    }
}
