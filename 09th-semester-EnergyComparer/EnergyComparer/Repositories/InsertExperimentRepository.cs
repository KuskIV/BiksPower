using Dapper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.TestCases;
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
        private IDbConnection _connection;
        private readonly ILogger _logger;

        public InsertExperimentRepository(ILogger logger)
        {
            _logger = logger;
        }

        public void InitializeDatabase(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task IncrementVersion(DtoDut dut)
        {
            var query = "UPDATE Dut SET Version = Version + 1 WHERE Id = @id AND Name = @name AND Os = @os";

            var count = await _connection.ExecuteAsync(query, new { id=dut.Id, name=dut.Name, os=dut.Os });

            LogCount(dut.Name, count);
        }

        public async Task InsertExperiment(DtoExperiment experiment)
        {
            var query = "INSERT INTO Experiment(StartTime, EndTime, Language, TestCaseId, DutId, ProfilerId, Runs, Iteration, FirstProfiler, ConfigurationId, Duration) " +
                                    "VALUES(@starttime, @endtime, @language, @programid, @systemid, @profilerid, @runs, @iteration, @firstprofiler, @configurationid, @duration)";

            var count = await _connection.ExecuteAsync(query, new 
            {
                starttime = experiment.StartTime,
                endtime = experiment.EndTime,
                language = experiment.Language,
                programid = experiment.TestCaseId,
                systemid = experiment.DutId,
                profilerid = experiment.ProfilerId,
                runs = experiment.Runs,
                iteration = experiment.Iteration,
                firstprofiler = experiment.FirstProfiler,
                configurationid = experiment.ConfigurationId,
                duration = experiment.duration,
            });
                
            LogCount("EXPERIMENT", count);
        }

        public async Task InsertConfiguration(int version, string env)
        {
            var query = "INSERT INTO Configuration(MinTemp, MaxTemp, MinutesBetweenExperiments, MinuteDurationOfExperiments, MinBattery, MaxBattery, Version, Env)" +
                "VALUES(@mintemp, @maxtemp, @minbetween, @minduration, @minbattery, @maxbattery, @version, @env)";

            var count = await _connection.ExecuteAsync(query, new
            {
                mintemp = Constants.TemperatureLowerLimit,
                maxtemp = Constants.TemperatureUpperLimit,
                minbetween = Constants.MinutesBetweenExperiments,
                minduration = Constants.DurationOfExperimentsInMinutes,
                minbattery = Constants.ChargeLowerLimit,
                maxbattery = Constants.ChargeUpperLimit,
                version = version,
                env = env
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

        public async Task InsertTestCase(string name)
        {
            var query = "INSERT IGNORE INTO TestCase(Name) VALUES(@name)";

            var count = await _connection.ExecuteAsync(query, new { name = name });

            LogCount(name, count);

        }

        public async Task InsertDut(string name, string os, int version=1)
        {
            var query = "INSERT IGNORE INTO Dut(Os, Version, Name) VALUES(@os, @version, @name)";

            var count = await _connection.ExecuteAsync(query, new { os=os, version=version, name=name });

            LogCount(name, count);
        }

        public async Task InsertMeasurement(List<DtoMeasurement> temperatures, int id)
        {
            var query = "INSERT INTO Measurement(ExperimentId, Value, Time, Name, Type) VALUES(@id, @value, @time, @name, @type)";

            var count = 0;

            foreach (var t in temperatures)
            {
                count += await _connection.ExecuteAsync(query, new {
                    id = id, value = t.Value, Time = t.Time, Name = t.Name, Type = t.Type });
            }

            LogCount("TEMPERATURE", count);
        }

        public async Task InsertTimeSeriesData(DtoTimeSeries timeSeries)
        {
            var query = "INSERT INTO TimeSeries(ExperimentId, Value, Time) VALUES(@experimentid, @value, @time)";

            var count = await _connection.ExecuteAsync(query, new { experimentid = timeSeries.ExperimentId, value = timeSeries.Value, time = timeSeries.Time });

            LogCount("TIMESERIES DATA", count);
        }

        public async Task InsertRawData(DtoRawData data)
        {
            var query = "INSERT INTO RawData(ExperimentId, Value, Time) VALUES(@experimentid, @value, @time)";

            var count = await _connection.ExecuteAsync(query, new { experimentid = data.ExperimentId, value = data.Value, time=data.Time });

            LogCount("RAW DATA", count);

        }

        public async Task InsertProfilers(List<Profiler> profilers, DtoDut system, ITestCase program)
        {
            var systemId = system.Id;
            var programId = program.GetProgram().Id;
            var value = JsonSerializer.Serialize(profilers);

            var query = "INSERT INTO Run(DutId, TestCaseId, Value) VALUES(@systemid, @programid, @value)";

            var count = await _connection.ExecuteAsync(query, new { systemid = systemId, programid = programId, value= value });

            LogCount("RUN", count);
        }

        public async Task UpdateProfilers(string systemId, string programId, string value)
        {
            var query = "UPDATE Run SET Value = @value WHERE DutId = @systemid AND TestCaseId = @programid";

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
        Task IncrementVersion(DtoDut system);
        void InitializeDatabase(IDbConnection _connection);
        Task InsertConfiguration(int version, string env);
        Task InsertExperiment(DtoExperiment experiment);
        Task InsertProfiler(IEnergyProfiler energyProfiler);
        Task InsertProfilers(List<Profiler> profilers, DtoDut system, ITestCase program);
        Task InsertTestCase(string name);
        Task InsertRawData(DtoRawData data);
        Task InsertDut(string name, string os, int version = 1);
        Task InsertMeasurement(List<DtoMeasurement> temperatures, int id);
        Task UpdateProfilers(string systemId, string programId, string value);
        Task InsertTimeSeriesData(DtoTimeSeries timeSeries);
    }
}
