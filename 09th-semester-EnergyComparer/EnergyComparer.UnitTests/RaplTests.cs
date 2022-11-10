using EnergyComparer.Profilers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.UnitTests
{
    public class RaplTests
    {
        [Fact]
        public void When_ReadingRaplValues_Convert()
        {
            var rapl = new RAPL();

            var (measurement1, measurementtotal1) = rapl.GetMeasurementFromValue("folder", 1000000);
            Assert.Equal(0, measurement1);
            Assert.Equal(1, measurementtotal1);

            var (measurement2, measurementtotal2) = rapl.GetMeasurementFromValue("folder", 1500000);
            Assert.Equal((decimal)0.5f, measurement2);
            Assert.Equal((decimal)1.5f, measurementtotal2);

            var (measurement3, measurementtotal3) = rapl.GetMeasurementFromValue("folder", 2000000);
            Assert.Equal((decimal)0.5f, measurement3);
            Assert.Equal((decimal)2.0f, measurementtotal3);

            var (measurement4, measurementtotal4) = rapl.GetMeasurementFromValue("folder", 4000000);
            Assert.Equal((decimal)2.0f, measurement4);
            Assert.Equal((decimal)4.0f, measurementtotal4);

            var (measurement5, measurementtotal5) = rapl.GetMeasurementFromValue("folder", 8750000);
            Assert.Equal((decimal)4.75f, measurement5);
            Assert.Equal((decimal)8.75f, measurementtotal5);
        }
    }
}
