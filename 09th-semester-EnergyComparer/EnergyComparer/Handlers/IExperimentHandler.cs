using EnergyComparer.Models;
using EnergyComparer.Programs;

namespace EnergyComparer.Handlers
{
    public interface IExperimentHandler
    {
        void CreateFolder(string folder);
        List<string> GetAllRequiredPaths();
        DtoMeasurement GetCharge();
        Task<ITestCase> GetTestCase(IDataHandler dataHandler);
        bool ShouldStopExperiment();
        Task WaitTillStableState();
    }
}