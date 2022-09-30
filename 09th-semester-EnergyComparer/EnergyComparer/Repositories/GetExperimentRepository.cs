using Dapper;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Repositories
{
    public class GetExperimentRepository : IGetExperimentRepository
    {
        private readonly IDbConnection _connection;

        public GetExperimentRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<DtoProgram> GetProgram(int name)
        {
            var query = "SELECT * FROM Program WHERE Name = @id";

            var response = await _connection.QueryFirstAsync<DtoProgram>(query, new { name=name });

            return response;
        }

        public async Task<DtoSystem> GetSystem(string Os, string Name)
        {
            var query = "SELECT * FROM System WHERE Os = @os AND Name = @name";

            var response = await _connection.QueryFirstAsync<DtoSystem>(query, new { os=Os, name=Name });

            return response;
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
        Task<DtoProgram> GetProgram(int name);
        Task<DtoSystem> GetSystem(string Os, string Name);
        Task<bool> SystemExists(string os, string name);
    }
}
