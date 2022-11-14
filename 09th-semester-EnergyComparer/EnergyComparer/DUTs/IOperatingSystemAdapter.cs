using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;

namespace EnergyComparer.DUTs
{
    public interface IOperatingSystemAdapter
    {
        void DisableWifi(string interfaceName);
        void EnableWifi(string interfaceName);
        void Restart();
        void Shutdowm();
        void StopunneccesaryProcesses();
        float GetAverageCpuTemperature();
        List<DtoMeasurement> GetCoreTemperatures();
    }
}