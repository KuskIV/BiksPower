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

namespace EnergyComparer.Handlers
{
    public class DataHandler : IDataHandler
    {
        private readonly ILogger _logger;
        private readonly IInsertExperimentRepository _insertRepository;
        private readonly IGetExperimentRepository _getRepository;

        public DataHandler(ILogger logger, IInsertExperimentRepository insertRepository, IGetExperimentRepository getRepository)
        {
            _logger = logger;
            _insertRepository = insertRepository;
            _getRepository = getRepository;
        }

        public void InitializeConnection()
        {
            _insertRepository.InitializeDatabase();
            _getRepository.InitializeDatabase();
        }

        public void CloseConnection()
        {
            _insertRepository.CloseConnection();
            _getRepository.CloseConnection();
        }

        public async Task<DtoExperiment> GetExperiment(Result<IntelPowerGadgetData> result, IProgram program, DateTime startTime, DateTime stopTime, int counter)
        {
            var experiment = new DtoExperiment()
            {
                EndTime = stopTime,
                StartTime = startTime,
                Language = program.GetLanguage(),
                ProgramId = result.GetProgramId(),
                SystemId = result.GetSystemId(),
                Version = result.GetVersion(),
                ProfilerId = result.GetProfilerId(),
                Runs = counter
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

        public async Task<DtoProgram> GetProgram(string name)
        {
            if (!await _getRepository.ProgramExists(name))
            {
                await _insertRepository.InsertProgram(name);
            }

            var program = await _getRepository.GetProgram(name);

            return program;
        }

        public async Task<DtoSystem> GetSystem()
        {
            var Name = Environment.MachineName;
            var Os = Environment.OSVersion.VersionString;

            if (!await _getRepository.SystemExists(Os, Name))
            {
                await _insertRepository.InsertSystem(Name, Os);
            }

            var system = await _getRepository.GetSystem(Os, Name);

            return system;
        }

        public async Task IncrementVersionForSystem()
        {
            var system = await GetSystem(); 

            await _insertRepository.IncrementVersion(system);
        }

        public async Task<List<Profiler>> GetProfilerFromLastRunOrDefault(IProgram program)
        {
            var system = await GetSystem();

            if (await LastRunExistsForSystem(system, program))
            {
                return await _getRepository.GetLastRunForSystem(system, program);
            }

            var profilers =  EnergyProfilerUtils.GetDefaultProfilersForSystem(system, program);

            await _insertRepository.InsertProfilers(profilers, system, program);

            return profilers;

        }

        private async Task<bool> LastRunExistsForSystem(DtoSystem system, IProgram program)
        {
            return await _getRepository.RunExistsForSystem(system, program);
        }

        public async Task UpdateProfilers(IProgram program, List<Profiler> profilers)
        {
            var system = await GetSystem();

            var systemId = system.Id;
            var programId = program.GetProgram().Id;
            var value = JsonSerializer.Serialize(profilers);

            await _insertRepository.UpdateProfilers(systemId, programId, value);
        }
    }

    public interface IDataHandler
    {
        void CloseConnection();
        Task<DtoExperiment> GetExperiment(Result<IntelPowerGadgetData> result, IProgram program, DateTime startTime, DateTime stopTime, int _counter);
        Task<DtoProfiler> GetProfiler(EnergyComparer.Profilers.IEnergyProfiler energyProfiler);
        Task<List<Profiler>> GetProfilerFromLastRunOrDefault(IProgram program);
        Task<DtoProgram> GetProgram(string name);
        Task<DtoSystem> GetSystem();
        Task IncrementVersionForSystem();
        void InitializeConnection();
        Task UpdateProfilers(IProgram program, List<Profiler> profilers);
    }
}
