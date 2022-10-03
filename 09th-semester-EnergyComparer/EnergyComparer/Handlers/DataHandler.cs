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

        public async Task<DtoExperiment> GetExperiment(Result<IntelPowerGadgetData> result, IProgram program, DateTime startTime, DateTime stopTime)
        {
            var experiment = new DtoExperiment()
            {
                EndTime = stopTime,
                StartTime = startTime,
                Language = program.GetLanguage(),
                ProgramId = result.GetProgramId(),
                SystemId = result.GetSystemId(),
                Version = result.GetVersion(),
                ProfilerId = result.GetProfilerId()
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
    }

    public interface IDataHandler
    {
        Task<DtoExperiment> GetExperiment(Result<IntelPowerGadgetData> result, IProgram program, DateTime startTime, DateTime stopTime);
        Task<DtoProfiler> GetProfiler(EnergyComparer.Profilers.IEnergyProfiler energyProfiler);
        Task<DtoProgram> GetProgram(string name);
        Task<DtoSystem> GetSystem();
        Task IncrementVersionForSystem();
    }
}
