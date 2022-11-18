using EnergyComparer.DUTs;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.TestCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public class EnergyProfilerService : IEnergyProfilerService
    {
        private readonly bool _iterateOverProfilers;
        private readonly IDutAdapter _dutAdapter;
        private readonly ILogger _logger;
        private Dictionary<string, List<Profiler>> _profilers = new Dictionary<string, List<Profiler>>();

        public EnergyProfilerService(bool iterateOverProfilers, IDutAdapter dutAdapter, ILogger logger)
        {
            _iterateOverProfilers = iterateOverProfilers;
            _dutAdapter = dutAdapter;
            _logger = logger;
        }

        public async Task<IEnergyProfiler> GetNext(ITestCase program, IDataHandler dataHandler, IOperatingSystemAdapter adapterService)
        {
            if (!_iterateOverProfilers)
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

        private IEnergyProfiler GetDefaultProfiler()
        {
            return _dutAdapter.GetDefaultProfiler();
        }

        private IEnergyProfiler GetCurrentProfilerAndUpdateIsFirst(ITestCase program, List<Profiler> profilers, IOperatingSystemAdapter adapterService)
        {
            var currentProfiler = GetCurrentProfiler(profilers);
            var currentProfilerIndex = profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            profilers[currentProfilerIndex].IsCurrent = false;
            profilers[nextIndex].IsCurrent = true;

            UpdateProfilers(program, profilers);

            var profiler = _dutAdapter.GetProfilers().FirstOrDefault(x => x.GetName() == currentProfiler.Name, null);

            if (profiler != null)
            {
                _logger.Information("Profiler {name} found.", profiler.GetName());
                return profiler;
            }
            else
            {
                _logger.Warning("Profiler {name} is not available. Moving on to next.", currentProfiler.Name);
                return GetCurrentProfilerAndUpdateIsFirst(program, profilers, adapterService);
            }
            //return _dutAdapter.GetProfilers(currentProfiler.Name);
        }

        private void UpdateProfilers(ITestCase program, List<Profiler> profilers)
        {
            _profilers[program.GetName()] = profilers;
        }

        private static Profiler GetCurrentProfiler(List<Profiler> profilers)
        {
            return profilers.Where(x => x.IsCurrent == true).First();
        }

        private async Task UpdateIsFirstProfiler(string id, List<Profiler> profilers, IDataHandler dataHandler)
        {
            profilers = MoveToNextIsFirstProfiler(profilers);

            await dataHandler.UpdateProfilers(id, profilers);
        }

        public List<Profiler> MoveToNextIsFirstProfiler(List<Profiler> profilers)
        {
            var currentProfiler = profilers.Where(x => x.IsFirst == true).First();
            var currentProfilerIndex = profilers.IndexOf(currentProfiler);
            var nextIndex = currentProfilerIndex == profilers.Count - 1 ? 0 : currentProfilerIndex + 1;

            profilers[currentProfilerIndex].IsFirst = false;
            profilers[nextIndex].IsFirst = true;

            return profilers;
        }

        private async Task<List<Profiler>> InitializeProfilers(ITestCase program, IDataHandler dataHandler)
        {
            var profilers = GetProfilers(program);

            if (profilers.Count == 0)
            {
                profilers = await dataHandler.GetProfilerFromLastRunOrDefault(program);
            }

            return profilers;
        }

        private List<Profiler> GetProfilers(ITestCase program)
        {
            var testCaseName = program.GetName();

            if (!_profilers.ContainsKey(testCaseName))
            {
                _profilers.Add(testCaseName, new List<Profiler>());
            }

            return _profilers[testCaseName];
        }
    }

    public interface IEnergyProfilerService
    {
        Task<IEnergyProfiler> GetNext(ITestCase program, IDataHandler dataHandler, IOperatingSystemAdapter adapterService);
        List<Profiler> MoveToNextIsFirstProfiler(List<Profiler> profilers);
        Task SaveProfilers(IDataHandler dataHandler);
    }
}
