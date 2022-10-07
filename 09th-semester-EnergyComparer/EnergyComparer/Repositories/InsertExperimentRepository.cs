using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Asn1.BC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Repositories
{
    public class InsertExperimentRepository : IInsertExperimentRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private IDbConnection _connection;
        private readonly ILogger _logger;

        public InsertExperimentRepository(Func<IDbConnection> connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public void InitializeDatabase()
        {
            _connection = _connectionFactory();
        }

        public void CloseConnection()
        {
            _connection.Close();
        }

        public async Task IncrementVersion(DtoSystem system)
        {
            var query = "UPDATE System SET Version = Version + 1 WHERE Id = @id AND Name = @name AND Os = @os";

            var count = await _connection.ExecuteAsync(query, new { id=system.Id, name=system.Name, os=system.Os });

            LogCount(system.Name, count);
        }

        public async Task InsertExperiment(DtoExperiment experiment)
        {
            var query = "INSERT INTO Experiment(StartTime, EndTime, Language, ProgramId, Version, SystemId, ProfilerId, Runs) VALUES(@starttime, @endtime, @language, @programid, @version, @systemid, @profilerid, @runs)";

            var count = await _connection.ExecuteAsync(query, new 
            {
                starttime = experiment.StartTime,
                endtime = experiment.EndTime,
                language = experiment.Language,
                programid = experiment.ProgramId,
                version = experiment.Version,
                systemid = experiment.SystemId,
                profilerid = experiment.ProfilerId,
                runs = experiment.Runs,
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

        public async Task InsertProfilers(List<Profiler> profilers, DtoSystem system, IProgram program)
        {
            var systemId = system.Id;
            var programId = program.GetProgram().Id;
            var value = JsonSerializer.Serialize(profilers);

            var query = "INSERT INTO Run(SystemId, ProgramId, Value) VALUES(@systemid, @programid, @value)";

            var count = await _connection.ExecuteAsync(query, new { systemid = systemId, programid = programId, value= value });

            LogCount("RUN", count);
        }
        public async Task UpdateProfilers(string systemId, string programId, string value)
        {
            var query = "UPDATE Run SET Value = @value WHERE SystemId = @systemid AND ProgramId = @programid";

            var count = await _connection.ExecuteAsync(query, new { systemid = systemId, programid = programId, value = value });

            LogCount("RUN", count);
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
        void CloseConnection();
        Task IncrementVersion(DtoSystem system);
        void InitializeDatabase();
        Task InsertExperiment(DtoExperiment experiment);
        Task InsertProfiler(IEnergyProfiler energyProfiler);
        Task InsertProfilers(List<Profiler> profilers, DtoSystem system, IProgram program);
        Task InsertProgram(string name);
        Task InsertSystem(string name, string os, int version = 1);
        Task UpdateProfilers(string systemId, string programId, string value);
    }
}
