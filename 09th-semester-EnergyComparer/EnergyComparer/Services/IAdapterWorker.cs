﻿using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;

namespace EnergyComparer.Services
{
    public interface IAdapterService
    {
        void DisableWifi(string interfaceName);
        void EnableWifi(string interfaceName);
        List<string> GetAllRequiredPaths();
        List<string> GetAllSouces();
        IProgram GetProgram(IDataHandler dataHandler);
        IEnergyProfiler MapEnergyProfiler(Profiler profiler);
        void Restart(bool _isProd);
        bool ShouldStopExperiment();
        Task WaitTillStableState(bool isProd);
    }
}