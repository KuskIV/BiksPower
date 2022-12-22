import json
import random


class Experiment(object):
    def __init__(
        self,
        config_id,
        dut_id,
        test_case_id,
        profiler_id,
        language,
        repository,
        dut,
        os,
        test_case,
        version,
        profiler,
        count=200,
    ):
        data_tuple = (config_id, dut_id, test_case_id, language, profiler_id, count)
        data = repository.query_all(
            "SELECT * FROM Experiment Where ConfigurationId = %s AND DutId = %s AND TestCaseId = %s AND Language = %s "
            + " and ProfilerId = %s ORDER BY StartTime DESC  LIMIT %s",
            data_tuple,
        )

        if not data is None:
            self.experiments = []
            self.config_id = config_id
            self.dut_id = dut_id
            self.test_case_id = test_case_id
            self.language = language
            self.count = count

            parameters_for_query = (dut, os, test_case, version, profiler)

            if repository.parameters_exists(parameters_for_query, repository):
                (k, look_back) = repository.get_parameters(
                    parameters_for_query, repository
                )
                self.k = k
                self.look_back = look_back
                self.has_outlier_parameters = True
            else:
                self.has_outlier_parameters = False

            get_time_series = True

            for d in data:
                raw_data = RawData(
                    d[0],
                    d[5],
                    d[6],
                    d[7],
                    d[8],
                    d[9],
                    d[10],
                    d[11],
                    get_time_series,
                    repository,
                )
                if raw_data.is_valid == True:
                    self.experiments.append(raw_data)
            get_time_series = False


class Dut(object):
    def __init__(self, dut_name, dut_os, repository):
        data_tuple = (
            dut_name,
            dut_os,
        )
        data = repository.query_one(
            "SELECT * FROM Dut WHERE Name = %s AND OS = %s", data_tuple
        )

        if not data is None and len(data) == 4:
            self.dut_name = dut_name
            self.dut_os = dut_os
            self.id = data[0]
            self.version = data[3]
        else:
            self.dut_id = -1


class Measurements(object):
    def __init__(self, experiment_id, measurement_type, repository):
        data_tuple = (
            experiment_id,
            measurement_type,
        )
        data = repository.query_all(
            "SELECT * FROM Measurement WHERE ExperimentId = %s AND Type = %s",
            data_tuple,
        )
        self.data = []

        if data is not None and len(data) > 0:
            for d in data:
                self.data.append(Measuring(d[0], d[2], d[3], d[4], d[5]))


class Measuring(object):
    def __init__(self, id, value, time, name, measuring_type):
        self.id = id
        self.value = value
        self.time = time
        self.name = name
        self.type = measuring_type


class EnergyProfiler(object):
    def __init__(self, energy_profiler_name, repository):
        data_tuple = (energy_profiler_name,)
        data = repository.query_one(
            "SELECT * FROM Profiler Where Name = %s", data_tuple
        )

        if data is not None and len(data) == 2:
            self.energy_profiler_name = energy_profiler_name
            self.id = data[0]
        else:
            self.id = -1


class Configuration(object):
    def __init__(
        self,
        min_temp,
        max_temp,
        min_battery,
        max_battery,
        duration,
        between,
        version,
        repository,
        env,
    ):
        data_tuple = (
            min_temp,
            max_temp,
            min_battery,
            max_battery,
            between,
            duration,
            version,
            env,
        )
        data = repository.query_one(
            "SELECT Id, Env FROM Configuration WHERE MinTemp = %s AND MaxTemp = %s AND MinBattery = %s AND MaxBattery = %s AND "
            + "MinutesBetweenExperiments = %s AND MinuteDurationOfExperiments = %s AND Version = %s AND Env = %s",
            data_tuple,
        )

        if not data is None:
            self.min_temp = min_temp
            self.max_temp = max_temp
            self.min_battery = min_battery
            self.max_battery = max_battery
            self.between = between
            self.duration = duration
            self.version = version
            self.id = data[0]
            self.env = data[1]
        else:
            self.id = -1
        # else:
        #     raise Exception(f"Could not find any configuration for setup.")


class TestCase(object):
    def __init__(self, test_case_name, repository):
        data_tuple = (test_case_name,)
        data = repository.query_one(
            "SELECT Id FROM TestCase WHERE Name = %s", data_tuple
        )

        if len(data) == 1:
            self.id = data[0]
            self.test_case_name = test_case_name
        else:
            raise Exception(f"Could not find any test case for '{test_case_name}'")


class TimeSeries(object):
    def __init__(self, experiment_id, repository):
        data_tuple = (experiment_id,)
        data = repository.query_one(
            "SELECT * FROM TimeSeries WHERE ExperimentId = %s", data_tuple
        )
        self.time = data[3]
        self.id = data[0]
        self.experiment_id = data[1]
        self.data_point = []

        data_points = json.loads(data[2])

        if "DataPoints" in data_points:
            for d in data_points["DataPoints"]:
                self.data_point.append(DataPoint(d))
        else:
            for d in data_points:
                self.data_point.append(DataPoint(d))


class DataPoint(object):
    def __init__(self, data):
        self.__dict__ = data


class RawData(object):
    def __init__(
        self,
        experiment_id,
        start_time,
        end_time,
        profiler_id,
        runs,
        iteration,
        first_profiler,
        duration,
        take_time_series,
        repository,
    ):
        data_tuple = (experiment_id,)
        data = repository.query_one(
            "SELECT Value FROM RawData WHERE ExperimentId = %s", data_tuple
        )

        if not data is None and len(data) == 1:
            if data[0] == "string2":
                self.is_valid = False
                return

            json_data = json.loads(data[0])

            if type(json_data) == dict:
                self.__dict__ = json_data
            elif type(json_data) == list and len(json_data) > 0:
                if "AppId" in json_data[0]:
                    correct_app = [
                        x for x in json_data if "09th-semester-test-cases" in x["AppId"]
                    ]
                    if len(correct_app) == 0:
                        self.AppId = ["EMPTY"]
                        self.EnergyLosses = [0]
                        self.CPUEnergyConsumptions = [0]
                        self.TotalEnergyConsumptions = [0]
                        self.TimeInMSecs = [0]
                        self.TimeStamps = ["9999-12-06:16:08:00.0000"]
                        self.EnergyLoss = sum(self.EnergyLosses)
                        self.CPUEnergyConsumption = sum(self.CPUEnergyConsumptions)
                        self.TotalEnergyConsumption = sum(self.TotalEnergyConsumptions)
                    else:
                        self.AppId = [x["AppId"] for x in correct_app]
                        self.EnergyLosses = [int(x["EnergyLoss"]) for x in correct_app]
                        self.CPUEnergyConsumptions = [
                            int(x["CPUEnergyConsumption"]) for x in correct_app
                        ]
                        self.TotalEnergyConsumptions = [
                            int(x["TotalEnergyConsumption"]) for x in correct_app
                        ]
                        self.TimeInMSecs = [int(x["TimeInMSec"]) for x in correct_app]
                        self.TimeStamps = [x["TimeStamp"] for x in correct_app]
                        self.EnergyLoss = sum(self.EnergyLosses)
                        self.CPUEnergyConsumption = sum(self.CPUEnergyConsumptions)
                        self.TotalEnergyConsumption = sum(self.TotalEnergyConsumptions)
                else:
                    self.__dict__ = json_data[0]
            else:
                self.__dict__ = {}

            self.is_valid = True

            # print(type(json_data))

            # if len(json_data) == 1:
            #     self.__dict__ = json.loads(json_data[0])
            # elif len(json_data) == 0:
            #     self.__dict__ = {}
            # elif "AppId" in json_data:
            #     correct_app = [
            #         x for x in json_data if "09th-semester-test-cases" in x.AppId
            #     ]
            #     print(correct_app)
            #     if len(correct_app.keys()) == 0:
            #         self.__dict__ = {}
            #     else:
            #         self.__dict__ = correct_app[-1]

            self.id = experiment_id
            self.start_time = start_time
            self.end_time = end_time
            self.runs = runs
            self.profiler_id = profiler_id
            self.iteration = iteration
            self.first_profiler = first_profiler
            self.duration = duration

            self.time_series = TimeSeries(experiment_id, repository)
            self.has_time_series = True

            # if take_time_series or random.randint(0, 20) == 1:
            #     self.time_series = TimeSeries(experiment_id, repository)
            #     self.has_time_series = True
            # else:
            #     self.has_time_series = False

            self.start_temperature = GetMeasurements(
                experiment_id, "CpuTemperature", repository, min
            )
            self.stop_temperature = GetMeasurements(
                experiment_id, "CpuTemperature", repository, max
            )

            self.start_battery = GetMeasurements(
                experiment_id, "BatteryChargeLeft", repository, min
            )
            self.stop_battery = GetMeasurements(
                experiment_id, "BatteryChargeLeft", repository, max
            )

        else:
            self.is_valid = False
            # raise Exception(
            #     f"Could not find any RawData for experiment id '{experiment_id}'"
            # )


class Bucket(object):
    pass


def GetMeasurements(experiment_id, measurement_type, repository, ordering):
    measurements = Measurements(experiment_id, measurement_type, repository)
    measurements_dates = [x.time for x in measurements.data]
    measurements.data = [
        x for x in measurements.data if x.time == ordering(measurements_dates)
    ]

    return measurements
