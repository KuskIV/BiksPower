﻿using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Services
{
    public class EnergyProfilerService : IEnergyProfilerService
    {
        private readonly bool _isProd;
        private Dictionary<string, List<Profiler>> _profilers = new Dictionary<string, List<Profiler>>();

        public EnergyProfilerService(bool iterateOverProfilers)
        {
            _isProd = iterateOverProfilers;
        }

        public async Task<IEnergyProfiler> GetNext(IProgram program, IDataHandler dataHandler, IAdapterService adapterService)
        {
            if (!_isProd)
            {
                return GetDefaultProfiler();
            }

            var profilers = await InitializeProfilers(program, dataHandler);

            return GetCurrentProfilerAndUpdateIsFirst(program, profilers, adapterService);
        }

        public async Task SaveProfilers(IDataHandler dataHandler)
        {
            foreach (var key in _profilers.Keys)
            {
                var profiler = _profilers[key];

                await UpdateIsFirstProfiler(key, profiler, dataHandler);
            }
        }

        private IEnergyProfiler GetCurrentProfiler(IProgram program, List<Profiler> profilers, IAdapterService adapterService)
        {
            var currentProfiler = GetCurrentProfiler(profilers);

            UpdateProfilers(program, profilers);

            return adapterService.MapEnergyProfiler(currentProfiler);
        }

        private IEnergyProfiler GetDefaultProfiler()
        {
            return new IntelPowerGadget();
        }

        private IEnergyProfiler GetCurrentProfilerAndUpdateIsFirst(IProgram program, List<Profiler> profilers, IAdapterService adapterService)
        {
            var currentProfiler = GetCurrentProfiler(profilers);
            var currentProfilerIndex = profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            profilers[currentProfilerIndex].IsCurrent = false;
            profilers[nextIndex].IsCurrent = true;

            UpdateProfilers(program, profilers);

            return adapterService.MapEnergyProfiler(currentProfiler);
        }

        private void UpdateProfilers(IProgram program, List<Profiler> profilers)
        {
            _profilers[program.GetName()] = profilers;
        }

        private static Profiler GetCurrentProfiler(List<Profiler> profilers)
        {
            return profilers.Where(x => x.IsCurrent == true).First();
        }

        private async Task UpdateIsFirstProfiler(string id, List<Profiler> profilers, IDataHandler dataHandler)
        {
            var currentProfiler = profilers.Where(x => x.IsFirst == true).First();
            var currentProfilerIndex = profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            profilers[currentProfilerIndex].IsFirst = false;
            profilers[nextIndex].IsFirst = true;

            await dataHandler.UpdateProfilers(id, profilers);
        }

        private async Task<List<Profiler>> InitializeProfilers(IProgram program, IDataHandler dataHandler)
        {
            var profilers = GetProfilers(program);

            if (profilers.Count == 0)
            {
                profilers = await dataHandler.GetProfilerFromLastRunOrDefault(program);
            }

            return profilers;
        }

        private List<Profiler> GetProfilers(IProgram program)
        {
            var name = program.GetName();

            if (!_profilers.ContainsKey(name))
            {
                _profilers.Add(name, new List<Profiler>());
            }

            return _profilers[name];
        }
    }

    public interface IEnergyProfilerService
    {
        Task<IEnergyProfiler> GetNext(IProgram program, IDataHandler dataHandler, IAdapterService adapterService);
        Task SaveProfilers(IDataHandler dataHandler);
    }
}