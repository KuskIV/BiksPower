using EnergyComparer.Models;
using EnergyComparer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;
using EnergyComparer.Programs;
using EnergyComparer.Profilers;
using Ubiety.Dns.Core.Records.NotUsed;
using EnergyComparer.Utils;
using System.Text.Json;
using EnergyComparer.Services;
using System.Data;

namespace EnergyComparer.Handlers
{
    public class DataHandler : IDataHandler
    {
        private readonly ILogger _logger;
        private IInsertExperimentRepository _insertRepository;
        private IGetExperimentRepository _getRepository;
        private readonly IAdapterService _adapterService;
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly string _machineName;

        public DataHandler(ILogger logger, IAdapterService adapterService, Func<IDbConnection> connectionFactory, string machineName)
        {
            _logger = logger;
            _adapterService = adapterService;
            _connectionFactory = connectionFactory;
            _machineName = machineName;

            InitializeRepositories();
        }

        public void InitializeRepositories()
        {
            _insertRepository = new InsertExperimentRepository(_logger);
            _getRepository = new GetExperimentRepository();

            InitializeConnection();
        }

        public void InitializeConnection()
        {
            _insertRepository.InitializeDatabase(_connectionFactory);
            _getRepository.InitializeDatabase(_connectionFactory);
        }

        public void CloseConnection()
        {
            _insertRepository.CloseConnection();
            _getRepository.CloseConnection();
        }

        public async Task<DtoExperiment> GetExperiment(int programId, int systemId, int profilerId, ITestCase program, DateTime startTime, DateTime stopTime, int counter, int profilerCount, string firstProfiler, int configurationId, long duration, int version)
        {
            var experiment = new DtoExperiment()
            {
                EndTime = stopTime,
                StartTime = startTime,
                Language = program.GetLanguage(),
                TestCaseId = programId,
                DutId = systemId,
                ProfilerId = profilerId,
                Runs = counter,
                Iteration = profilerCount,
                FirstProfiler = firstProfiler,
                ConfigurationId = configurationId,
                duration = duration,
            };

            await _insertRepository.InsertExperiment(experiment);

            return await _getRepository.GetExperiment(experiment);
        }

        public async Task<DtoProfiler> GetProfiler(IEnergyProfiler energyProfiler)
        {
            if (!await _getRepository.ProfilerExists(energyProfiler))
            {
                await _insertRepository.InsertProfiler(energyProfiler);
            }

            var profiler = await _getRepository.GetProfiler(energyProfiler);

            return profiler;
        }

        public async Task InsertMeasurement(List<DtoMeasurement> temperatures, int id, DateTime date)
        {
            temperatures.ForEach(x => x.Time = date);

            await _insertRepository.InsertMeasurement(temperatures, id);
        }

        public async Task<DtoTestCase> GetProgram(string name)
        {
            if (!await _getRepository.TestCaseExists(name))
            {
                await _insertRepository.InsertTestCase(name);
            }

            var program = await _getRepository.GetTestCase(name);

            return program;
        }

        public async Task<DtoDut> GetSystem()
        {
            var Name = _machineName;
            var Os = Environment.OSVersion.Platform.ToString();


            if (!await _getRepository.DutExists(Os, Name))
            {
                await _insertRepository.InsertDut(Name, Os);
            }

            var system = await _getRepository.GetDut(Os, Name);

            return system;
        }

        public async Task<DtoConfiguration> GetConfiguration(int version)
        {
            var env = Constants.GetEnv();

            if (!await _getRepository.ConfigurationExists(version, env))
            {
                await _insertRepository.InsertConfiguration(version, env);
            }

            return await _getRepository.GetConfiguration(version, env);
        }

        public async Task IncrementVersionForSystem()
        {
            var system = await GetSystem(); 

            await _insertRepository.IncrementVersion(system);
        }

        public async Task<List<Profiler>> GetProfilerFromLastRunOrDefault(ITestCase program)
        {
            var system = await GetSystem();
            var profilers = new List<Profiler>();

            if (await LastRunExistsForSystem(system, program))
            {
                profilers = await _getRepository.GetLastRunForDut(system, program);
            }
            else
            {
                var sources = _adapterService.GetAllSouces();
                profilers =  EnergyProfilerUtils.GetDefaultProfilersForSystem(system, program, sources);

                await _insertRepository.InsertProfilers(profilers, system, program);
            }

            foreach (var p in profilers.Where(x => x.IsFirst == true))
                p.IsCurrent = true;

            return profilers;

        }

        private async Task<bool> LastRunExistsForSystem(DtoDut system, ITestCase program)
        {
            return await _getRepository.RunExistsForDut(system, program);
        }

        public async Task UpdateProfilers(string id, List<Profiler> profilers)
        {
            var system = await GetSystem();

            var systemId = system.Id.ToString();
            var programId = id;
            var value = JsonSerializer.Serialize(profilers);

            await _insertRepository.UpdateProfilers(systemId, programId, value);
        }

        public async Task InsertRawData(DtoRawData data)
        {
            await _insertRepository.InsertRawData(data);
        }
    }

    public interface IDataHandler
    {
        void CloseConnection();
        Task<DtoConfiguration> GetConfiguration(int version);
        Task<DtoExperiment> GetExperiment(int programId, int systemId, int profilerId, ITestCase program, DateTime startTime, DateTime stopTime, int counter, int profilerCount, string firstProfiler, int id, long duration, int version);
        Task<DtoProfiler> GetProfiler(IEnergyProfiler energyProfiler);
        Task<List<Profiler>> GetProfilerFromLastRunOrDefault(ITestCase program);
        Task<DtoTestCase> GetProgram(string name);
        Task<DtoDut> GetSystem();
        Task IncrementVersionForSystem();
        void InitializeConnection();
        Task InsertRawData(DtoRawData data);
        Task InsertMeasurement(List<DtoMeasurement> endTemperatures, int id, DateTime date);
        Task UpdateProfilers(string id, List<Profiler> profilers);
    }
}
