using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EnergyComparer.Repositories
{
    public class GetExperimentRepository : IGetExperimentRepository
    {
        private IDbConnection _connection;
        private readonly Func<IDbConnection> _connectionFactory;

        public GetExperimentRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void InitializeDatabase()
        {
            _connection = _connectionFactory();
        }

        public void CloseConnection()
        {
            _connection.Close();
        }

        public async Task<DtoExperiment> GetExperiment(int id)
        {
            var query = "SELECT * FROM Experiment WHERE Id = @id ";

            var response = await _connection.QueryFirstAsync<DtoExperiment>(query, new { id=id });

            return response;
        }

        public async Task<DtoExperiment> GetExperiment(DtoExperiment experiment)
        {
            var query = "SELECT * FROM Experiment WHERE Language = @language AND ProgramId = @programid AND SystemId = @systemid AND ProfilerId = @profilerid ORDER BY StartTime DESC";
            
            var response = await _connection.QueryFirstAsync<DtoExperiment>(query, new 
            {
                language = experiment.Language,
                programid = experiment.ProgramId,
                systemid = experiment.SystemId,
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

        public async Task<DtoProgram> GetProgram(string name)
        {
            var query = "SELECT * FROM Program WHERE Name = @name";

            var response = await _connection.QueryFirstAsync<DtoProgram>(query, new { name=name });

            return response;
        }

        public async Task<DtoSystem> GetSystem(string Os, string Name)
        {
            var query = "SELECT * FROM System WHERE Os = @os AND Name = @name";

            var response = await _connection.QueryFirstAsync<DtoSystem>(query, new { os=Os, name=Name });

            return response;
        }

        public async Task<DtoConfiguration> GetConfiguration()
        {
            var query = "SELECT * FROM Configuration WHERE MinTemp = @mintemp AND MaxTemp = @maxtemp AND MinutesBetweenExperiments = @between AND " +
                "MinuteDurationOfExperiments = @duration AND MinBattery = @minbattery AND MaxBattery = @maxbattery";

            var response = await _connection.QueryFirstAsync<DtoConfiguration>(query, new 
            { 
                mintemp = Constants.TemperatureLowerLimit,
                maxtemp = Constants.TemperatureUpperLimit,
                between = Constants.MinutesBetweenExperiments,
                duration = Constants.DurationOfExperimentsInMinutes,
                minbattery = Constants.ChargeLowerLimit,
                maxbattery = Constants.ChargeUpperLimit,
            });

            return response;

        }

        public async Task<bool> ProfilerExists(IEnergyProfiler energyProfiler)
        {
            var query = "SELECT COUNT(*) FROM Profiler WHERE Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { name = energyProfiler.GetName() });

            return response == 1;
        }

        public async Task<bool> ProgramExists(string name)
        {
            var query = "SELECT COUNT(*) FROM Program WHERE Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { name = name });

            return response == 1;
        }

        public async Task<bool> ConfigurationExists(int version)
        {
            var query = "SELECT * FROM Configuration WHERE MinTemp = @mintemp AND MaxTemp = @maxtemp AND MinutesBetweenExperiments = @between AND " +
                        "MinuteDurationOfExperiments = @duration AND MinBattery = @minbattery AND MaxBattery = @maxbattery";

            var response = await _connection.ExecuteScalarAsync<int>(query, new
            {
                mintemp = Constants.TemperatureLowerLimit,
                maxtemp = Constants.TemperatureUpperLimit,
                between = Constants.MinutesBetweenExperiments,
                duration = Constants.DurationOfExperimentsInMinutes,
                minbattery = Constants.ChargeLowerLimit,
                maxbattery = Constants.ChargeUpperLimit,
            });

            return response == 1;
        }

        public async Task<bool> SystemExists(string os, string name)
        {
            var query = "SELECT COUNT(*) FROM System WHERE Os = @os AND Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { os=os, name=name });

            return response == 1;
        }

        public async Task<List<Profiler>> GetLastRunForSystem(DtoSystem system, IProgram program)
        {
            var systemId = system.Id;
            var programId = program.GetProgram().Id;

            var query = "SELECT Value FROM Run WHERE SystemId = @systemid AND programid = @programid";

            var response = await _connection.QueryFirstAsync<string>(query, new { systemId = systemId, programId = programId });

            var profilers =  JsonSerializer.Deserialize<List<Profiler>>(response);

            return profilers;
        }

        public async Task<bool> RunExistsForSystem(DtoSystem system, IProgram program)
        {
            var systemId = system.Id;
            var programId = program.GetProgram().Id;

            var query = "SELECT COUNT(*) FROM Run WHERE SystemId = @systemid AND ProgramId = @programid";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { systemid = systemId, programid = programId });

            return response == 1;
        }


    }

    public interface IGetExperimentRepository
    {
        void CloseConnection();
        Task<bool> ConfigurationExists(int version);
        Task<DtoConfiguration> GetConfiguration();
        Task<DtoExperiment> GetExperiment(DtoExperiment experiment);
        Task<List<Profiler>> GetLastRunForSystem(DtoSystem system, Programs.IProgram program);
        Task<DtoProfiler> GetProfiler(IEnergyProfiler energyProfiler);
        Task<DtoProgram> GetProgram(string name);
        Task<DtoSystem> GetSystem(string Os, string Name);
        void InitializeDatabase();
        Task<bool> ProfilerExists(IEnergyProfiler energyProfiler);
        Task<bool> ProgramExists(string name);
        Task<bool> RunExistsForSystem(DtoSystem system, Programs.IProgram program);
        Task<bool> SystemExists(string os, string name);
    }
}
