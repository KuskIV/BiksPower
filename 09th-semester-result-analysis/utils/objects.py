import json


class Experiment(object):
    def __init__(
        self, config_id, dut_id, test_case_id, language, repository, count=200
    ):
        data_tuple = (config_id, dut_id, test_case_id, language, count)
        data = repository.query_all(
            "SELECT * FROM Experiment Where ConfigurationId = %s AND DutId = %s AND TestCaseId = %s AND Language = %s "
            + "ORDER BY StartTime DESC  LIMIT %s",
            data_tuple,
        )

        if not data is None:
            self.experiments = []
            self.config_id = config_id
            self.dut_id = dut_id
            self.test_case_id = test_case_id
            self.language = language
            self.count = count

            for d in data:
                self.experiments.append(
                    RawData(
                        d[0],
                        d[5],
                        d[6],
                        d[0],
                        d[9],
                        d[10],
                        d[11],
                        repository,
                    )
                )


class Dut(object):
    def __init__(self, dut_name, dut_os, repository):
        data_tuple = (
            dut_name,
            dut_os,
        )
        data = repository.query_one(
            "SELECT * FROM Dut WHERE Name = %s AND OS = %s", data_tuple
        )

        if len(data) == 4:
            self.dut_name = dut_name
            self.dut_os = dut_os
            self.id = data[0]
            self.version = data[3]


class Measurements(object):
    def __init__(self, experiment_id, repository):
        data_tuple = (experiment_id,)
        data = repository.query_all(
            "SELECT * FROM Measurement WHERE ExperimentId = %s", data_tuple
        )
        self.data = []

        if len(data) > 0:
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

        if len(data) == 2:
            self.energy_profiler_name = energy_profiler_name
            self.id = data[0]
        else:
            raise Exception(
                f"Data of incorrect format, should have two properties. Data was {data} for name {name}."
            )


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
            raise Exception(f"Could not find any configuration for setup.")


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


class RawData(object):
    def __init__(
        self,
        experiment_id,
        start_time,
        end_time,
        runs,
        iteration,
        first_profiler,
        duration,
        repository,
    ):
        data_tuple = (experiment_id,)
        data = repository.query_one(
            "SELECT Value FROM RawData WHERE ExperimentId = %s", data_tuple
        )

        if len(data) == 1:
            self.__dict__ = json.loads(data[0])
            self.id = experiment_id
            self.start_time = start_time
            self.end_time = end_time
            self.runs = runs
            self.iteration = iteration
            self.first_profiler = first_profiler
            self.duration = duration
        else:
            raise Exception(
                f"Could not find any RawData for experiment id '{experiment_id}'"
            )
