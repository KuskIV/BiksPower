﻿using EnergyComparer.Handlers;
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
        DtoMeasurement GetCharge();
        ITestCase GetTestCase(IDataHandler dataHandler);
        IEnergyProfiler MapEnergyProfiler(Profiler profiler);
        void Restart();
        bool ShouldStopExperiment();
        void StopunneccesaryProcesses();
        Task WaitTillStableState();
    }
}