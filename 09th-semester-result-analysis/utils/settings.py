from logging import exception
from easysettings import JSONSettings, preferred_file
from pathlib import Path, PurePath
import os

SECRETS_PATH = "secrets/appsettings.secrets.json"
ROOT_DIR = str(Path(__file__).parent.parent)


def _get_user_secrets():
    if os.path.exists(SECRETS_PATH):
        return JSONSettings.from_file(
            preferred_file(
                [
                    ROOT_DIR + "/" + SECRETS_PATH,
                ]
            )
        ).__dict__["data"]
    else:
        raise exception(
            f"\n\n-------\nERROR:\n    User secrets are used in this project, but the file has not been made locally.\n    Please create the file '{SECRETS_PATH}' and input all required secrets and try agian.\n------\n\n"
        )


def _get_attribute(attribute: str) -> str:
    secrets = _get_user_secrets()

    if attribute in secrets:
        return secrets[attribute]
    else:
        raise exception(
            f"\n\n------\nERROR:\n    The '{SECRETS_PATH}' file does not contain the requested attribute '{attribute}'\n    Please insert this attribute and try again.\n------\n\n"
        )


def get_username():
    return _get_attribute("username")


def get_password():
    return _get_attribute("password")


def get_source():
    return _get_attribute("source")


def get_port():
    return _get_attribute("port")


def get_catalog():
    return _get_attribute("catalog")
