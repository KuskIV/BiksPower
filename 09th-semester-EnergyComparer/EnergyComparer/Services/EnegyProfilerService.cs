using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Programs;
using EnergyComparer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Services
{
    public class EnergyProfilerService : IEnergyProfilerService, IDisposable
    {
        private readonly IDataHandler _dataHandler;
        private readonly bool _iterateOverProfilers;
        private List<Profiler> _profilers = new List<Profiler>();

        public EnergyProfilerService(IDataHandler dataHandler, bool iterateOverProfilers)
        {
            _dataHandler = dataHandler;
            _iterateOverProfilers = iterateOverProfilers;
        }
        public async Task<IEnergyProfiler> GetNext(IProgram program)
        {
            if (!_iterateOverProfilers)
            {
                return GetDefaultProfiler();
            }

            await InitializeProfilers(program);

            var currentProfiler = await GetCurrentProfilerAndUpdateIsFirst(program);

            return AdapterUtils.MapEnergyProfiler(currentProfiler);
        }

        private IEnergyProfiler GetDefaultProfiler()
        {
            return new IntelPowerGadget();
        }

        private async Task<Profiler> GetCurrentProfilerAndUpdateIsFirst(IProgram program)
        {
            var currentProfiler = _profilers.Where(x => x.IsFirst == true).First();
            var currentProfilerIndex = _profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == _profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            _profilers[currentProfilerIndex].IsFirst = false;
            _profilers[nextIndex].IsFirst = true;

            await _dataHandler.UpdateProfilers(program, _profilers);

            return currentProfiler;
        }

        private async Task UpdateIsFirstProfiler(IProgram program)
        {
            var currentProfiler = _profilers.Where(x => x.IsFirst == true).First();
            var currentProfilerIndex = _profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == _profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            _profilers[currentProfilerIndex].IsFirst = false;
            _profilers[nextIndex].IsFirst = true;

            await _dataHandler.UpdateProfilers(program, _profilers);
        }

        private async Task InitializeProfilers(IProgram program)
        {
            if (_profilers.Count == 0)
            {
                _profilers = await _dataHandler.GetProfilerFromLastRunOrDefault(program);
            }
        }

        public async void Dispose()
        {
            await UpdateIsFirstProfiler();
        }
    }

    public interface IEnergyProfilerService
    {
        Task<IEnergyProfiler> GetNext(IProgram program);
    }
}
