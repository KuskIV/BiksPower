using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Models
{
    public class CpuProperty
    {
        private Dictionary<string, float> _data { get; set; } = new Dictionary<string, float>();

        public float GetAverageValues()
        {
            var avg = _data.Where(x => x.Key.ToLower().Contains("average"));

            if (avg.Count() != 0)
                return avg.First().Value;

            var data = _data.Where(x => x.Key.Contains("CPU Core"));

            if (data.Count() == 0)
                throw new Exception("Unable to tage average, as no values were found.");

            return data.Average(x => x.Value);
        }

        public bool TryGetValue(string key, out float value)
        {
            return _data.TryGetValue(key, out value);
        }

        public void AddValue(string key, float? value)
        {
            _data.Add(key, value!.Value);
        }
    }
}
