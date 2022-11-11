using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.TestCases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace EnergyComparer.Repositories
{
    public class GetExperimentRepository : IGetExperimentRepository
    {
        private IDbConnection _connection;

        public void InitializeDatabase(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<DtoExperiment> GetExperiment(int id)
        {
            var query = "SELECT * FROM Experiment WHERE Id = @id ";

            var response = await _connection.QueryFirstAsync<DtoExperiment>(query, new { id=id });

            return response;
        }

        public async Task<int> GetExperimentCountForSetup(int configId, int dutId, int testCaseId, string language, int profilerId)
        {
            var query = "SELECT COUNT(*) FROM Experiment WHERE ConfigurationId = @configid AND DutId = @dutid AND TestCaseId = @testcaseid AND Language = @language AND ProfilerId = @profilerid";

            var response = await _connection.ExecuteScalarAsync<int>(query, new
            {
                configid = configId,
                dutid = dutId,
                testcaseid = testCaseId,
                language = language,
                profilerid = profilerId
            });

            return response;
        }

        public async Task<DtoExperiment> GetExperiment(DtoExperiment experiment)
        {
            var query = "SELECT * FROM Experiment WHERE Language = @language AND TestCaseId = @programid AND DutId = @systemid AND ProfilerId = @profilerid ORDER BY StartTime DESC";
            
            var response = await _connection.QueryFirstAsync<DtoExperiment>(query, new 
            {
                language = experiment.Language,
                programid = experiment.TestCaseId,
                systemid = experiment.DutId,
                profilerid = experiment.ProfilerId,
            });

            return response;
        }

        public async Task<DtoProfiler> GetProfiler(IEnergyProfiler energyProfiler)
        {
            var query = "SELECT * FROM Profiler WHERE Name = @name";

            var response = await _connection.QueryFirstAsync<DtoProfiler>(query, new { name = energyProfiler.GetName() });

            return response;
        }

        public async Task<DtoTestCase> GetTestCase(string name)
        {
            var query = "SELECT * FROM TestCase WHERE Name = @name";

            var response = await _connection.QueryFirstAsync<DtoTestCase>(query, new { name=name });

            return response;
        }

        public async Task<DtoDut> GetDut(string Os, string Name)
        {
            var query = "SELECT * FROM Dut WHERE Os = @os AND Name = @name";

            var response = await _connection.QueryFirstAsync<DtoDut>(query, new { os=Os, name=Name });

            return response;
        }

        public async Task<DtoConfiguration> GetConfiguration(int version, string env)
        {
            var query = "SELECT * FROM Configuration WHERE MinTemp = @mintemp AND MaxTemp = @maxtemp AND MinutesBetweenExperiments = @between AND " +
                "MinuteDurationOfExperiments = @duration AND MinBattery = @minbattery AND MaxBattery = @maxbattery AND Version = @version AND Env = @env";

            var response = await _connection.QueryFirstAsync<DtoConfiguration>(query, new 
            { 
                mintemp = Constants.TemperatureLowerLimit,
                maxtemp = Constants.TemperatureUpperLimit,
                between = Constants.MinutesBetweenExperiments,
                duration = Constants.DurationOfExperimentsInMinutes,
                minbattery = Constants.ChargeLowerLimit,
                maxbattery = Constants.ChargeUpperLimit,
                version = version,
                env = env,
            });

            return response;

        }

        public async Task<bool> ProfilerExists(IEnergyProfiler energyProfiler)
        {
            var query = "SELECT COUNT(*) FROM Profiler WHERE Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { name = energyProfiler.GetName() });

            return response == 1;
        }

        public async Task<bool> TestCaseExists(string name)
        {
            var query = "SELECT COUNT(*) FROM TestCase WHERE Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { name = name });

            return response == 1;
        }

        public async Task<bool> ConfigurationExists(int version, string env)
        {
            var query = "SELECT * FROM Configuration WHERE MinTemp = @mintemp AND MaxTemp = @maxtemp AND MinutesBetweenExperiments = @between AND " +
                        "MinuteDurationOfExperiments = @duration AND MinBattery = @minbattery AND MaxBattery = @maxbattery AND Version = @version AND Env = @env";

            var response = await _connection.ExecuteScalarAsync<int>(query, new
            {
                mintemp = Constants.TemperatureLowerLimit,
                maxtemp = Constants.TemperatureUpperLimit,
                between = Constants.MinutesBetweenExperiments,
                duration = Constants.DurationOfExperimentsInMinutes,
                minbattery = Constants.ChargeLowerLimit,
                maxbattery = Constants.ChargeUpperLimit,
                version = version,
                env = env
            });

            return response >= 1;
        }

        public async Task<bool> DutExists(string os, string name)
        {
            var query = "SELECT COUNT(*) FROM Dut WHERE Os = @os AND Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { os=os, name=name });

            return response == 1;
        }

        public async Task<List<Profiler>> GetLastRunForDut(DtoDut dut, ITestCase testCase)
        {
            var systemId = dut.Id;
            var programId = testCase.GetProgram().Id;

            var query = "SELECT Value FROM Run WHERE DutId = @systemid AND programid = @programid";

            var response = await _connection.QueryFirstAsync<string>(query, new { systemId = systemId, programId = programId });

            var profilers =  JsonSerializer.Deserialize<List<Profiler>>(response);

            return profilers;
        }

        public async Task<bool> RunExistsForDut(DtoDut dut, ITestCase testCase)
        {
            var systemId = dut.Id;
            var programId = testCase.GetProgram().Id;

            var query = "SELECT COUNT(*) FROM Run WHERE DutId = @systemid AND TestCaseId = @programid";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { systemid = systemId, programid = programId });

            return response == 1;
        }
    }

    public interface IGetExperimentRepository
    {
        Task<bool> ConfigurationExists(int version, string env);
        Task<DtoConfiguration> GetConfiguration(int version, string env);
        Task<DtoExperiment> GetExperiment(DtoExperiment experiment);
        Task<List<Profiler>> GetLastRunForDut(DtoDut system, ITestCase program);
        Task<DtoProfiler> GetProfiler(IEnergyProfiler energyProfiler);
        Task<DtoTestCase> GetTestCase(string name);
        Task<DtoDut> GetDut(string Os, string Name);
        void InitializeDatabase(IDbConnection _connection);
        Task<bool> ProfilerExists(IEnergyProfiler energyProfiler);
        Task<bool> TestCaseExists(string name);
        Task<bool> RunExistsForDut(DtoDut system, ITestCase program);
        Task<bool> DutExists(string os, string name);
        Task<int> GetExperimentCountForSetup(int configId, int dutId, int testCaseId, string language, int profilerId);
    }
}
