using EnergyComparer.Models;
using EnergyComparer.Services;
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
        internal static List<Profiler> GetDefaultProfilersForSystem(DtoSystem system, Programs.IProgram program, List<string> sources)
        {
            var profilers = new List<Profiler>();

            foreach (var s in sources)
            {
                profilers.Add(new Profiler() { IsFirst = false, Name = s });
            }

            profilers.First().IsFirst = true;

            return profilers;
        }
    }
}
