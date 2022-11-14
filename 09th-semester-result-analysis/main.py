from utils.database_repository import DataRepository
from utils.objects import *

if __name__ == "__main__":

    config_id = 7
    dut_id = 6
    test_case_id = 4
    language = "CSharp"
    repository = DataRepository()

    experiment = Experiment(config_id, dut_id, test_case_id, language, repository, 200)
