:: Prepare the environment variables used by the application.

:: Connection string of the DocumentDb instance where telemetry is stored
SETX PCS_STREAMANALYTICS_DOCUMENTDB_CONNSTRING "..."

:: URL of the device telemetry web service
SETX PCS_DEVICETELEMETRY_WEBSERVICE_URL "..."

:: URL of the UI config web service
SETX PCS_UICONFIG_WEBSERVICE_URL "..."

:: URL of the IoT Hub manager web service
SETX PCS_IOTHUBMANAGER_WEBSERVICE_URL "..."

:: Connection details of the Azure Blob where checkpoints are stored
SETX PCS_IOTHUBREACT_AZUREBLOB_ACCOUNT "..."
SETX PCS_IOTHUBREACT_AZUREBLOB_KEY "..."
SETX PCS_IOTHUBREACT_AZUREBLOB_ENDPOINT_SUFFIX "..."

:: see: Endpoints ⇒ Messaging ⇒ Events ⇒ "Event Hub-compatible name"
:: e.g. "my-test-hub"
SETX PCS_IOTHUBREACT_HUB_NAME "..."

:: see: Endpoints ⇒ Messaging ⇒ Events ⇒ "Event Hub-compatible endpoint"
:: e.g. "iothub-ns-my-test-185521-d1bf252916.servicebus.windows.net"
SETX PCS_IOTHUBREACT_HUB_ENDPOINT "..."

:: see: "IoT Hub" ⇒ your hub ⇒ "Shared access policies"
SETX PCS_IOTHUBREACT_ACCESS_CONNSTRING "..."
