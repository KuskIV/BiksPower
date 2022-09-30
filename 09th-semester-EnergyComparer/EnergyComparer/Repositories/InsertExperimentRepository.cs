using Dapper;
using EnergyComparer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Repositories
{
    public class InsertExperimentRepository : IInsertExperimentRepository
    {
        private readonly IDbConnection _connection;

        public InsertExperimentRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<int> InsertSystem(string name, string os, int version=1)
        {
            var query = "INSERT IGNORE INTO System(Os, Version, Name) VALUES(@os, @version, @name)";

            return await _connection.ExecuteAsync(query, new { os=os, version=version, name=name });
        }
    }

    public interface IInsertExperimentRepository
    {
        Task<int> InsertSystem(string name, string os, int version = 1);
    }
}
