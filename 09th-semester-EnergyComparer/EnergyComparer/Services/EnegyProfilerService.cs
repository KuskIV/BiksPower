using EnergyComparer.Handlers;
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
        private readonly IDataHandler _dataHandler;
        private readonly bool _isProd;
        private readonly IAdapterService _adapterService;
        private Dictionary<string, List<Profiler>> _profilers = new Dictionary<string, List<Profiler>>();

        public EnergyProfilerService(IDataHandler dataHandler, bool iterateOverProfilers, IAdapterService adapterService)
        {
            _dataHandler = dataHandler;
            _isProd = iterateOverProfilers;
            _adapterService = adapterService;
        }

        public async Task<IEnergyProfiler> GetNext(IProgram program)
        {
            if (!_isProd)
            {
                return GetDefaultProfiler();
            }

            var profilers = await InitializeProfilers(program);

            return GetCurrentProfilerAndUpdateIsFirst(program, profilers);
        }

        public async Task SaveProfilers()
        {
            foreach (var key in _profilers.Keys)
            {
                var profiler = _profilers[key];

                await UpdateIsFirstProfiler(key, profiler);
            }
        }

        private IEnergyProfiler GetCurrentProfiler(IProgram program, List<Profiler> profilers)
        {
            var currentProfiler = GetCurrentProfiler(profilers);

            UpdateProfilers(program, profilers);

            return _adapterService.MapEnergyProfiler(currentProfiler);
        }

        private IEnergyProfiler GetDefaultProfiler()
        {
            return new IntelPowerGadget();
        }

        private IEnergyProfiler GetCurrentProfilerAndUpdateIsFirst(IProgram program, List<Profiler> profilers)
        {
            var currentProfiler = GetCurrentProfiler(profilers);
            var currentProfilerIndex = profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            profilers[currentProfilerIndex].IsCurrent = false;
            profilers[nextIndex].IsCurrent = true;

            UpdateProfilers(program, profilers);

            return _adapterService.MapEnergyProfiler(currentProfiler);
        }

        private void UpdateProfilers(IProgram program, List<Profiler> profilers)
        {
            _profilers[program.GetName()] = profilers;
        }

        private static Profiler GetCurrentProfiler(List<Profiler> profilers)
        {
            return profilers.Where(x => x.IsCurrent == true).First();
        }

        private async Task UpdateIsFirstProfiler(string id, List<Profiler> profilers)
        {
            var currentProfiler = profilers.Where(x => x.IsFirst == true).First();
            var currentProfilerIndex = profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            profilers[currentProfilerIndex].IsFirst = false;
            profilers[nextIndex].IsFirst = true;

            await _dataHandler.UpdateProfilers(id, profilers);
        }

        private async Task<List<Profiler>> InitializeProfilers(IProgram program)
        {
            var profilers = GetProfilers(program);

            if (profilers.Count == 0)
            {
                profilers = await _dataHandler.GetProfilerFromLastRunOrDefault(program);
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
        Task<IEnergyProfiler> GetNext(IProgram program);
        Task SaveProfilers();
    }
}
