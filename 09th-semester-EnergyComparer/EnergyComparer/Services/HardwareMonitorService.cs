﻿using System;
using System.Collections.Generic;
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
        float GetAverageCpuLoad();
        float GetAverageCpuTemperature();
        List<DtoMeasurement> GetCoreTemperatures();
        float GetCpuMemory();
        float GetMaxTemperature();
        float GetTotalLoad();
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
                //_logger.Error(e, "Exception occured why trying to close computer");
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

        public float GetAverageCpuTemperature()
        {
            return GetAverageValueForSensor(SensorType.Temperature);
        }

        public float GetAverageCpuLoad()
        {
            return GetAverageValueForSensor(SensorType.Load);
        }

        public float GetTotalLoad()
        {
            return GetValueForSensor(SensorType.Load, "CPU Total");
        }

        public float GetMaxTemperature()
        {
            return GetValueForSensor(SensorType.Temperature, "Core Max");
        }

        public float GetCpuMemory()
        {
            return GetValueForSensor(SensorType.Power, "CPU Memory");
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

        private float GetValueForSensor(SensorType sensorType, string key)
        {
            UpdateCpuValues();

            if (_cpuValues.TryGetValue(sensorType, out var data) && data.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                throw new NotImplementedException($"The sensor {sensorType} wiht key {key} is not available in the CPU");
            }
        }

        private float GetAverageValueForSensor(SensorType sensorType)
        {
            UpdateCpuValues();
            
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

        private void UpdateCpuValues()
        {
            _cpuValues = ResetCpuValues();

            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Value.HasValue && _cpuValues.TryGetValue(sensor.SensorType, out var value))
                        value.AddValue(sensor.Name, sensor.Value);
                }
            }
        }

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
