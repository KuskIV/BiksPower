using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using EnergyComparer.TestCases;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Utils
{
    public static class EnergyProfilerUtils
    {
        internal static List<Profiler> GetDefaultProfilersForSystem(DtoDut system, ITestCase program, List<IEnergyProfiler> sources)
        {
            var profilers = new List<Profiler>();

            foreach (var s in sources)
            {
                profilers.Add(new Profiler() { IsFirst = false, Name = s.GetName() });
            }

            profilers.First().IsFirst = true;

            return profilers;
        }
    }
}
