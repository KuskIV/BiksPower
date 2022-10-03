using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Repositories
{
    public class InsertExperimentRepository : IInsertExperimentRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger _logger;

        public InsertExperimentRepository(IDbConnection connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task IncrementVersion(DtoSystem system)
        {
            var query = "UPDATE System SET Version = Version + 1 WHERE Id = @id AND Name = @name AND Os = @os";

            var count = await _connection.ExecuteAsync(query, new { id=system.Id, name=system.Name, os=system.Os });

            LogCount(system.Name, count);
        }

        public async Task InsertExperiment(DtoExperiment experiment)
        {
            var query = "INSERT INTO Experiment(StartTime, EndTime, Language, ProgramId, Version, SystemId, ProfilerId)" +
                "VALUES(@starttime, @enddtime, @language, @programid, @version, @systemid, @profilerid)";

            var count = await _connection.ExecuteAsync(query, new 
            { 
                @starttime = experiment.StartTime,
                @enddtime = experiment.EndTime,
                @language = experiment.Language,
                @programid = experiment.ProgramId,
                @version = experiment.Version,
                @systemid = experiment.SystemId,
                @profilerid = experiment.ProfilerId
            });

            LogCount("EXPERIMENT", count);

        }

        public async Task InsertProfiler(IEnergyProfiler energyProfiler)
        {
            var query = "INSERT IGNORE INTO Profiler(Name) VALUES(@name)";
            var name = energyProfiler.GetName();

            var count = await _connection.ExecuteAsync(query, new { name = name });

            LogCount(name, count);
        }

        public async Task InsertProgram(string name)
        {
            var query = "INSERT IGNORE INTO Program(Name) VALUES(@name)";

            var count = await _connection.ExecuteAsync(query, new { name = name });

            LogCount(name, count);

        }

        public async Task InsertSystem(string name, string os, int version=1)
        {
            var query = "INSERT IGNORE INTO System(Os, Version, Name) VALUES(@os, @version, @name)";

            var count = await _connection.ExecuteAsync(query, new { os=os, version=version, name=name });

            LogCount(name, count);
        }

        private void LogCount(string name, int count)
        {
            if (count == 1)
            {
                _logger.Information("Program named {name} has successfully been inserted", name);
            }
            else
            {
                _logger.Information("Program named {name} already existed", name);
            }
        }
    }

    public interface IInsertExperimentRepository
    {
        Task IncrementVersion(DtoSystem system);
        Task InsertExperiment(DtoExperiment experiment);
        Task InsertProfiler(IEnergyProfiler energyProfiler);
        Task InsertProgram(string name);
        Task InsertSystem(string name, string os, int version = 1);
    }
}
