from utils.database_repository import DataRepository
from utils.objects import *

if __name__ == "__main__":
    repository = DataRepository()

    # CONFIGURATION
    min_temp = 0
    max_temp = 200

    min_battery = 40
    max_battery = 80

    between = 0
    duration = 1

    language = "CSharp"

    env = "PROD"

    limit = 200

    ## OS
    windows_os = "Win32NT"
    linux_os = "Unix"

    os = [
        windows_os,
        # linux_os
    ]

    ## Test cases
    idle_case = "TestCaseIdle"
    dining_philosophers = "DiningPhilosophers"
    binary_tress = "BinaryTrees"
    reverse_complement = "ReverseComplement"
    fannkuch_redux = "FannkuchRedux"
    n_body = "Nbody"
    fasta = "Fasta"

    test_cases = [
        idle_case,
        # dining_philosophers,
        binary_tress,
        # # reverse_complement,
        # fannkuch_redux,
        # n_body,
        # fasta
    ]

    ## Profilers
    intel_power_gadget = "IntelPowerGadget"
    rapl = "RAPL"
    hardware_monitor = "HardwareMonitor"
    clamp = "Clamp"
    e3 = "E3"

    profilers = [
        # intel_power_gadget,
        # rapl,
        # hardware_monitor,
        clamp,
        # e3
    ]

    ## DUT
    surface_pro_4 = "Surface4Pro"
    surface_book = "SurfaceBook"
    power_komplett = "PowerKomplett"

    duts = [
        # surface_pro_4,
        # surface_book,
        power_komplett
    ]

    dut_version = {
        surface_book: {windows_os: 6, linux_os: 1},
        surface_pro_4: {
            windows_os: 12,
            linux_os: 1,
        },
        power_komplett: {
            windows_os: 8,
            linux_os: 1,
        },
    }

    data = {}

    for d in duts:
        data[d] = {}
        for o in os:
            version = dut_version[d][o]
            data[d][version] = {}
            data[d][version][o] = {}
            for t in test_cases:
                data[d][version][o][t] = {}
                for p in profilers:
                    data[d][version][o][t][p] = {}
                    data[d][version][o][t][p]["contains_data"] = False

                    profiler = EnergyProfiler(p, repository)
                    config = Configuration(
                        min_temp,
                        max_temp,
                        min_battery,
                        max_battery,
                        duration,
                        between,
                        version,
                        repository,
                        env,
                    )
                    dut = Dut(d, o, repository)
                    test_case = TestCase(t, repository)

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
                            repository,
                            200,
                        )

                        if len(experiment.experiments) > 0:
                            print(f"success for {d}, {o}, {t}, {p}")
                            data[d][version][o][t][p]["config"] = config
                            data[d][version][o][t][p]["profiler"] = profiler
                            data[d][version][o][t][p]["dut"] = dut
                            data[d][version][o][t][p]["test_case"] = test_case
                            data[d][version][o][t][p]["experiment"] = experiment
                            data[d][version][o][t][p]["contains_data"] = True
