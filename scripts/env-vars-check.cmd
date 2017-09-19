@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_TELEMETRYAGENT_DOCUMENTDB_CONNSTRING%" == "" (
    echo Error: the PCS_TELEMETRYAGENT_DOCUMENTDB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_TELEMETRY_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_TELEMETRY_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_CONFIG_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_CONFIG_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBMANAGER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_IOTHUBMANAGER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBREACT_AZUREBLOB_ACCOUNT%" == "" (
    echo Error: the PCS_IOTHUBREACT_AZUREBLOB_ACCOUNT environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBREACT_AZUREBLOB_KEY%" == "" (
    echo Error: the PCS_IOTHUBREACT_AZUREBLOB_KEY environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBREACT_AZUREBLOB_ENDPOINT_SUFFIX%" == "" (
    echo Error: the PCS_IOTHUBREACT_AZUREBLOB_ENDPOINT_SUFFIX environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBREACT_HUB_NAME%" == "" (
    echo Error: the PCS_IOTHUBREACT_HUB_NAME environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBREACT_HUB_ENDPOINT%" == "" (
    echo Error: the PCS_IOTHUBREACT_HUB_ENDPOINT environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBREACT_ACCESS_CONNSTRING%" == "" (
    echo Error: the PCS_IOTHUBREACT_ACCESS_CONNSTRING environment variable is not defined.
    exit /B 1
)

endlocal
