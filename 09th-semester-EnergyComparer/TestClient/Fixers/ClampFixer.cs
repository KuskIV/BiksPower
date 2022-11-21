using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using EnergyComparer.Utils;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using EnergyComparer.Models;
using RestSharp;

namespace TestClient.Fixers
{
    public class ClampFixer
    {
        private IDbConnection _connection;
        public void InitializeDatabase()
        {
            var connectionString = ConfigUtils.GetConnectionString(new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("secrets/appsettings.secrets.json", true)
            .AddJsonFile("appsettings.json", true)
            .Build());
            _connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            _connection.Open();
        }

        public async Task<List<int>> GetExperimentIds() 
        {
            var query = "SELECT ExperimentId FROM `RawData` where Value = 'string2'";
            var responds = await _connection.QueryAsync(query);
            var lst = responds.ToList().Select(x =>(int)((IDictionary<string, object>)x)["ExperimentId"]).ToList(); 
            return lst;
        }

        public async Task GetTimeseries() 
        {
            var ids = await GetExperimentIds();

            foreach (var item in ids)
            {
                await GetTimeseries(item);
            }
        }

        private async Task GetTimeseries(int id) 
        {
            var query = $"SELECT Value FROM `TimeSeries` WHERE ExperimentId = {id}";
            var responds = await _connection.QueryAsync(query);
            var lst = responds.ToList().Select(x => ((IDictionary<string, object>)x)["Value"]).ToList()[0].ToString();
            List<TimeSeries> timeSeries = JsonSerializer.Deserialize<List<TimeSeries>>(lst);
            var FixedTimeSeries = FixEmptyFields(timeSeries);
            await CalculateAndInsertRAW(FixedTimeSeries, id);
            //return lst;
        }

        private async Task CalculateAndInsertRAW(List<TimeSeries> ts, int id) 
        {
            var hr = JsonSerializer.Serialize(new HardwareRaw(ts.Sum(x => x.C1TrueRMSPower).Value, ts.Sum(x => x.C1ACRMSPower).Value));
            var query = $"UPDATE `RawData` SET Value = '{hr}' WHERE ExperimentId = {id}";
            var responds = await _connection.ExecuteAsync(query);
        }

        private List<TimeSeries> FixEmptyFields(List<TimeSeries> timeSeries)
        {
            for (int i = 0; i < timeSeries.Count; i++)
            {
                if (!timeSeries[i].C1ACRMS.HasValue) 
                {
                    if (i+1 >= timeSeries.Count)
                    {
                        timeSeries[i].C1ACRMS = (timeSeries[i - 1].C1ACRMS);
                    }
                    else
                    {
                        timeSeries[i].C1ACRMS = (timeSeries[i - 1].C1ACRMS + timeSeries[i + 1].C1ACRMS) / 2;
                    }
                }

                if (!timeSeries[i].C1TrueRMS.HasValue)
                {
                    if (i + 1 >= timeSeries.Count)
                    {
                        timeSeries[i].C1TrueRMS = (timeSeries[i - 1].C1TrueRMS);
                    }
                    else
                    {
                        timeSeries[i].C1TrueRMS = (timeSeries[i - 1].C1TrueRMS + timeSeries[i + 1].C1TrueRMS) / 2;
                    }
                }
            }
            return timeSeries;
        }
    }
}
