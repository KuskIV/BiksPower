using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using Google.Protobuf.WellKnownTypes;
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

        public InsertExperimentRepository(ILogger logger)
        {
            _logger = logger;
        }

        public void InitializeDatabase(Func<IDbConnection> connectionFactory)
        {
            _connection = connectionFactory();
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
            var query = "INSERT INTO Experiment(StartTime, EndTime, Language, ProgramId, SystemId, ProfilerId, Runs, Iteration, FirstProfiler, ConfigurationId, Duration) " +
                                    "VALUES(@starttime, @endtime, @language, @programid, @systemid, @profilerid, @runs, @iteration, @firstprofiler, @configurationid, @duration)";

            var count = await _connection.ExecuteAsync(query, new 
            {
                starttime = experiment.StartTime,
                endtime = experiment.EndTime,
                language = experiment.Language,
                programid = experiment.ProgramId,
                systemid = experiment.SystemId,
                profilerid = experiment.ProfilerId,
                runs = experiment.Runs,
                iteration = experiment.Iteration,
                firstprofiler = experiment.FirstProfiler,
                configurationid = experiment.ConfigurationId,
                duration = experiment.duration,
            });
                
            LogCount("EXPERIMENT", count);
        }

        public async Task InsertConfiguration(int version)
        {
            var query = "INSERT INTO Configuration(MinTemp, MaxTemp, MinutesBetweenExperiments, MinuteDurationOfExperiments, MinBattery, MaxBattery, Version)" +
                "VALUES(@mintemp, @maxtemp, @minbetween, @minduration, @minbattery, @maxbattery, @version)";

            var count = await _connection.ExecuteAsync(query, new
            {
                mintemp = Constants.TemperatureLowerLimit,
                maxtemp = Constants.TemperatureUpperLimit,
                minbetween = Constants.MinutesBetweenExperiments,
                minduration = Constants.DurationOfExperimentsInMinutes,
                minbattery = Constants.ChargeLowerLimit,
                maxbattery = Constants.ChargeUpperLimit,
                version = version,
            });

            LogCount("Configuration", count);
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

        public async Task InsertTemperature(List<DtoTemperature> temperatures, int id)
        {
            var query = "INSERT INTO Temperature(ExperimentId, Value, Time, Name) VALUES(@id, @value, @time, @name)";

            var count = 0;

            foreach (var t in temperatures)
            {
                count += await _connection.ExecuteAsync(query, new { id = id, value = t.Value, t.Time, t.Name });
            }

            LogCount("TEMPERATURE", count);
        }

        public async Task InsertRawData(DtoRawData data)
        {
            var query = "INSERT INTO RawData(ExperimentId, Value, Time) VALUES(@experimentid, @value, @time)";

            var count = await _connection.ExecuteAsync(query, new { experimentid = data.ExperimentId, value = data.Value, time=data.Time });

            LogCount("RAW DATA", count);

        }

        public async Task InsertProfilers(List<Profiler> profilers, DtoSystem system, ITestCase program)
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
                _logger.Information("{name} has successfully been inserted", name);
            }
            else
            {
                _logger.Information("{name} already existed", name);
            }
        }


    }

    public interface IInsertExperimentRepository
    {
        void CloseConnection();
        Task IncrementVersion(DtoSystem system);
        void InitializeDatabase(Func<IDbConnection> _connectionFactory);
        Task InsertConfiguration(int version);
        Task InsertExperiment(DtoExperiment experiment);
        Task InsertProfiler(IEnergyProfiler energyProfiler);
        Task InsertProfilers(List<Profiler> profilers, DtoSystem system, ITestCase program);
        Task InsertProgram(string name);
        Task InsertRawData(DtoRawData data);
        Task InsertSystem(string name, string os, int version = 1);
        Task InsertTemperature(List<DtoTemperature> temperatures, int id);
        Task UpdateProfilers(string systemId, string programId, string value);
    }
}
