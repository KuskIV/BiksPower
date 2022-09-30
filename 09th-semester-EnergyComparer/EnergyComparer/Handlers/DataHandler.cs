using EnergyComparer.Models;
using EnergyComparer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;

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
    }

    public interface IDataHandler
    {
        Task<DtoSystem> GetSystem();
    }
}
