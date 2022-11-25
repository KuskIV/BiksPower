from utils.database_connection import DatabaseConnection
from utils.objects import *


class DataRepository:
    def __init__(self):
        self.conn = DatabaseConnection()

    def query_all(self, query, data_tuple=()):
        return self.conn.query_all(query, data_tuple)

    def query_one(self, query, data_tuple=()):
        return self.conn.query_one(query, data_tuple)

    def execute_one(self, query, data_tuple=()):
        return self.conn.execute_one(query, data_tuple)

    def get_parameters(self, parameters_for_query, repository):
        query = "SELECT K, LookBack FROM OutlierHyperParameters WHERE Dut = %s AND OS = %s AND TestCase = %s AND Version = %s AND Profiler = %s"

        return repository.query_one(query, parameters_for_query)

    def insert_parameters(
        self, parameters_for_query, parameters_for_insert, repository
    ):
        if self.parameters_exists(parameters_for_query, repository):
            return self.update_parameter(parameters_for_insert, repository)
        else:
            query = "INSERT INTO OutlierHyperParameters(K, LookBack, Dut, OS, TestCase, Version, Profiler) VALUES (%s, %s, %s, %s, %s, %s, %s)"
            return repository.execute_one(query, parameters_for_insert)

    def parameters_exists(self, parameters_for_query, repository):
        query = "SELECT Count(*) FROM OutlierHyperParameters WHERE Dut = %s AND OS = %s AND TestCase = %s AND Version = %s AND Profiler = %s"
        return repository.query_one(query, parameters_for_query)[0]

    def update_parameter(self, parameters_for_insert, repository):
        query = "UPDATE OutlierHyperParameters SET K = %s, LookBack = %s WHERE Dut = %s AND OS = %s AND TestCase = %s AND Version = %s AND Profiler = %s"
        print("update")
        return repository.execute_one(query, parameters_for_insert)

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
        special_between,
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

                        if o in special_between and p in special_between[o]:
                            version = special_between[o][p]
                        else:
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
                                dut.dut_name,
                                o,
                                t,
                                version,
                                p,
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
