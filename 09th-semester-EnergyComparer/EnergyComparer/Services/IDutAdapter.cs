using EnergyComparer.Models;
using EnergyComparer.Profilers;

namespace EnergyComparer.Services
{
    public interface IDutAdapter
    {
        bool EnoughBattery();
        DtoMeasurement GetCharge();
        int GetChargeRemaining();
        IEnergyProfiler GetDefaultProfiler();
        List<IEnergyProfiler> GetProfilers();
        IEnergyProfiler GetProfilers(string name);
    }
}