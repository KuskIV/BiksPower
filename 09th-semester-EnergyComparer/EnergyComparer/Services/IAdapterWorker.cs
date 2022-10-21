using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;

namespace EnergyComparer.Services
{
    public interface IAdapterService
    {
        void CreateFolder(string folder);
        void DisableWifi(string interfaceName);
        void EnableWifi(string interfaceName);
        List<string> GetAllRequiredPaths();
        List<string> GetAllSouces();
        ISoftwareEntity GetSoftwareEntity(IDataHandler dataHandler);
        IEnergyProfiler MapEnergyProfiler(Profiler profiler);
        void Restart();
        bool ShouldStopExperiment();
        Task WaitTillStableState();
    }
}