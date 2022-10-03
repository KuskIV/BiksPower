using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
            var query = "SELECT * FROM Experiment WHERE StartTime = @startdate AND EndTime = @enddate AND Language = @language AND ProgramId = @programid AND Version = @version AND SystemId = @systemid AND ProfilerId = @profilerid";

            var response = await _connection.QueryFirstAsync<DtoExperiment>(query, new 
            {
                startdate = experiment.StartTime,
                enddate = experiment.EndTime ,
                language = experiment.Language,
                programid = experiment.ProgramId,
                version = experiment.Version,
                systemid = experiment.SystemId,
                profilerid = experiment.ProfilerId
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

        public async Task<bool> SystemExists(string os, string name)
        {
            var query = "SELECT COUNT(*) FROM System WHERE Os = @os AND Name = @name";

            var response = await _connection.ExecuteScalarAsync<int>(query, new { os=os, name=name });

            return response == 1;
        }
    }

    public interface IGetExperimentRepository
    {
        void CloseConnection();
        Task<DtoExperiment> GetExperiment(DtoExperiment experiment);
        Task<DtoProfiler> GetProfiler(IEnergyProfiler energyProfiler);
        Task<DtoProgram> GetProgram(string name);
        Task<DtoSystem> GetSystem(string Os, string Name);
        void InitializeDatabase();
        Task<bool> ProfilerExists(IEnergyProfiler energyProfiler);
        Task<bool> ProgramExists(string name);
        Task<bool> SystemExists(string os, string name);
    }
}
