﻿using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient.Memcached;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static async Task GetHardwareState() 
        {
            var url = "http://stemlevelup.com/api/RaspberryPi";
            var client = new RestClient(url);
            var request = new RestRequest(url, Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            //var Output = response.Content;
        }
    }
}