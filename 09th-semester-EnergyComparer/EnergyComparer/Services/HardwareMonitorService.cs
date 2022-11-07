using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using EnergyComparer.Models;
using LibreHardwareMonitor.Hardware;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EnergyComparer.Services
{
    public interface IHardwareMonitorService
    {
        float GetAverageCpuLoad(bool update = true);
        float GetAverageCpuTemperature(bool update = true);
        List<DtoMeasurement> GetCoreTemperatures();
        float GetCpuPowerMemory(bool update = true);
        float GetMaxTemperature(bool update = true);
        float GetTotalLoad(bool update = true);
        void UpdateCpuValues();
    }

    public class HardwareMonitorService : IHardwareMonitorService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Computer _computer;
        
        private Dictionary<SensorType, CpuProperty> _cpuValues = new Dictionary<SensorType, CpuProperty>();


        public HardwareMonitorService(ILogger logger)
        {
            _logger = logger;
            _computer = new Computer();
            _computer.IsCpuEnabled = true;
            _computer.Open();
        }

        public void Dispose()
        {
            try
            {
                _computer.Close();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception occured why trying to close computer");
            }
        }

        public List<DtoMeasurement> GetCoreTemperatures()
        {
            var temperatures = new List<DtoMeasurement>();
            var defaultName = "CPU Core #";

            var i = 1;
            var name = defaultName + i;

            while (TryGetValueForSensor(SensorType.Temperature, name, out var value))
            {
                temperatures.Add(
                    new DtoMeasurement()
                    {
                        Name = name,
                        Value = value,
                        Type = EMeasurementType.CpuTemperature.ToString()
                    });

                i += 1;
                name = defaultName + i;
            }

            //_logger.Information("Temperature measured for {count} cores, avg temperature: {temp}", 
                //temperatures.Count(), temperatures.Select(x => x.Value).Sum() / temperatures.Count());

            return temperatures;
        }

        // CPU LOAD
        public float GetAverageCpuLoad(bool update = true)
        {
            return GetAverageValueForSensor(SensorType.Load, update);
        }
        public float GetTotalLoad(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Total", update);
        }
        public float GetCpuCore1T1(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #1 Thread #1", update);
        }
        public float GetCpuCore1T2(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #1 Thread #2", update);
        }
        public float GetCpuCore2T1(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #2 Thread #1", update);
        }
        public float GetCpuCore2T2(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #2 Thread #2", update);
        }
        public float GetCpuCore3T1(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #3 Thread #1", update);
        }
        public float GetCpuCore3T2(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #3 Thread #2", update);
        }
        public float GetCpuCore4T1(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #4 Thread #1", update);
        }
        public float GetCpuCore4T2(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #4 Thread #2", update);
        }
        public float GetCpuCore5T1(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #5 Thread #1", update);
        }
        public float GetCpuCore5T2(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #5 Thread #2", update);
        }
        public float GetCpuCore6T1(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #6 Thread #1", update);
        }
        public float GetCpuCore6T2(bool update = true)
        {
            return GetValueForSensor(SensorType.Load, "CPU Core #6 Thread #2", update);
        }

        // CPU core Temperatures
        public float GetMaxTemperature(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "Core Max", update);
        }
        public float GetAverageCpuTemperature(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "Core Average", update);
        }
        public float GetCpuPackageTemp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "Core Package", update);
        }
        public float GetCpuCore1Temp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "CPU Core #1", update);
        }
        public float GetCpuCore2Temp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "CPU Core #2", update);
        }
        public float GetCpuCore3Temp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "CPU Core #3", update);
        }
        public float GetCpuCore4Temp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "CPU Core #4", update);
        }
        public float GetCpuCore5Temp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "CPU Core #5", update);
        }
        public float GetCpuCore6Temp(bool update = true)
        {
            return GetValueForSensor(SensorType.Temperature, "CPU Core #6", update);
        }
        
        // CPU Core clock
        public float GetCpuClockC1(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "CPU Core #1", update);
        }
        public float GetCpuClockC2(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "CPU Core #2", update);
        }
        public float GetCpuClockC3(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "CPU Core #3", update);
        }
        public float GetCpuClockC4(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "CPU Core #4", update);
        }
        public float GetCpuClockC5(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "CPU Core #5", update);
        }
        public float GetCpuClockC6(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "CPU Core #6", update);
        }
        public float GetCpuBusSpeed(bool update = true)
        {
            return GetValueForSensor(SensorType.Clock, "Bus Speed", update);
        }
        
        // Power
        public float GetCpuPowerPacket(bool update = true)
        {
            return GetValueForSensor(SensorType.Power, "CPU Package", update);
        }
        public float GetCpuPowerCores(bool update = true)
        {
            return GetValueForSensor(SensorType.Power, "CPU Cores", update);
        }
        public float GetCpuPowerMemory(bool update = true)
        {
            return GetValueForSensor(SensorType.Power, "CPU Memory", update);
        }

        // Voltage
        public float GetCpuVoltage(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core", update);
        }
        public float GetCpuVoltageC1(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core #1", update);
        }
        public float GetCpuVoltageC2(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core #2", update); 
        }
        public float GetCpuVoltageC3(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core #3", update);
        }
        public float GetCpuVoltageC4(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core #4", update);
        }
        public float GetCpuVoltageC5(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core #5", update);
        }
        public float GetCpuVoltageC6(bool update = true)
        {
            return GetValueForSensor(SensorType.Voltage, "CPU Core #6", update);
        }


        private bool TryGetValueForSensor(SensorType sensorType, string key, out float value)
        {
            UpdateCpuValues();

            if (_cpuValues.TryGetValue(sensorType, out var data) && data.TryGetValue(key, out value))
            {
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private float GetValueForSensor(SensorType sensorType, string key, bool update = true)
        {
            if (update)
            {
                UpdateCpuValues();
            }

            if (_cpuValues.TryGetValue(sensorType, out var data) && data.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                throw new NotImplementedException($"The sensor {sensorType} wiht key {key} is not available in the CPU");
            }
        }

        private float GetAverageValueForSensor(SensorType sensorType, bool update = true)
        {
            if (update)
            {
                UpdateCpuValues();
            }
            
            if (_cpuValues.TryGetValue(sensorType, out var data))
            {
                var avg = data.GetAverageValues();

                return avg;
            }
            else
            {
                throw new NotImplementedException($"The sensor {sensorType} is not available in the CPU");
            }
        }

        public void UpdateCpuValues()
        {
            _cpuValues = ResetCpuValues();

            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    try
                    {
                        if (sensor.Value.HasValue && _cpuValues.TryGetValue(sensor.SensorType, out var value))
                            value.AddValue(sensor.Name, sensor.Value);
                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                }
            }
        } // Break point bro

        private List<SensorType> GetAllSensorTypes()
        {
            return _computer.Hardware.SelectMany(x => x.Sensors).Select(x => x.SensorType).Distinct().ToList();
        }

        private Dictionary<SensorType, CpuProperty> ResetCpuValues()
        {
            var sensorTypes = GetAllSensorTypes();

            var newValues = new Dictionary<SensorType, CpuProperty>();

            foreach (var sensor in sensorTypes)
            {
                newValues.Add(sensor, new CpuProperty());
            }

            return newValues;
        }
    }
}
