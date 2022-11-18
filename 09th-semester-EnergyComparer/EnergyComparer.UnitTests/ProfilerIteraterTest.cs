using EnergyComparer.Models;
using EnergyComparer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.UnitTests
{
    public class ProfilerIteraterTest
    {
        [Fact]
        public void When_Restarting_IterateOverProfilers()
        {
            var energyProfilerService = new EnergyProfilerService(true, null, null);

            var profilers = new List<Profiler>()
            {
                new Profiler()
                {
                    IsCurrent = true,
                    IsFirst = true,
                    Name = "FIRST",
                },
                new Profiler()
                {
                    IsCurrent = false,
                    IsFirst = false,
                    Name = "SECOND",
                },
                new Profiler()
                {
                    IsCurrent = false,
                    IsFirst = false,
                    Name = "THIRD",
                },
            };

            profilers = energyProfilerService.MoveToNextIsFirstProfiler(profilers);
            Assert.True(GetProfiler(profilers, "FIRST", false));
            Assert.True(GetProfiler(profilers, "SECOND", true));
            Assert.True(GetProfiler(profilers, "THIRD", false));

            profilers = energyProfilerService.MoveToNextIsFirstProfiler(profilers);
            Assert.True(GetProfiler(profilers, "FIRST", false));
            Assert.True(GetProfiler(profilers, "SECOND", false));
            Assert.True(GetProfiler(profilers, "THIRD", true));

            profilers = energyProfilerService.MoveToNextIsFirstProfiler(profilers);
            Assert.True(GetProfiler(profilers, "FIRST", true));
            Assert.True(GetProfiler(profilers, "SECOND", false));
            Assert.True(GetProfiler(profilers, "THIRD", false));
        }

        private static bool GetProfiler(List<Profiler> profilers, string name, bool expectedValue)
        {
            return profilers.Where(x => x.Name == name).Select(x => x.IsFirst == expectedValue).First();
        }
    }
}
