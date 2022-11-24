from utils.database_connection import DatabaseConnection
from utils.objects import *


class DataRepository:
    def __init__(self):
        self.conn = DatabaseConnection()

    def query_all(self, query, data_tuple=()):
        return self.conn.query_all(query, data_tuple)

    def query_one(self, query, data_tuple=()):
        return self.conn.query_one(query, data_tuple)

    def get_experiments(
        self,
        duts,
        os,
        test_cases,
        profilers,
        dut_version,
        min_temp,
        max_temp,
        min_battery,
        max_battery,
        duration,
        between,
        env,
        language,
    ):
        data = {}
        for d in duts:
            data[d] = {}
            for o in os:
                data[d][o] = {}
                for t in test_cases:
                    data[d][o][t] = {}
                    for p in profilers:
                        data[d][o][t][p] = {}
                        data[d][o][t][p]["contains_data"] = False

                        profiler = EnergyProfiler(p, self)
                        version = dut_version[d][o]

                        config = Configuration(
                            min_temp,
                            max_temp,
                            min_battery,
                            max_battery,
                            duration,
                            between,
                            version,
                            self,
                            env,
                        )
                        dut = Dut(d, o, self)
                        test_case = TestCase(t, self)

                        if config.id > 0:
                            config_id = config.id
                            dut_id = dut.id
                            test_case_id = test_case.id
                            profiler_id = profiler.id

                            experiment = Experiment(
                                config_id,
                                dut_id,
                                test_case_id,
                                profiler_id,
                                language,
                                self,
                                200,
                            )

                            if len(experiment.experiments) > 0:
                                print(f"success for {d}, {o}, {t}, {p}")
                                data[d][o][t][p]["config"] = config
                                data[d][o][t][p]["profiler"] = profiler
                                data[d][o][t][p]["dut"] = dut
                                data[d][o][t][p]["test_case"] = test_case
                                data[d][o][t][p]["experiment"] = experiment
                                data[d][o][t][p]["contains_data"] = True
        return data

    def close(self):
        self.conn.close()
