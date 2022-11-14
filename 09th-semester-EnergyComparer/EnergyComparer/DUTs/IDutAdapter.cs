using EnergyComparer.Models;
using EnergyComparer.Profilers;

namespace EnergyComparer.DUTs
{
    public interface IDutAdapter
    {
        List<string> GetAllSoucres();
        int GetChargeRemaining();
        IEnergyProfiler GetDefaultProfiler();
        List<IEnergyProfiler> GetProfilers();
        float GetTemperature();
        void DisableNetworking(string interfaceName);
        void EnableNetworking(string interfaceName);
    }
}