[![Build][build-badge]][build-url]
[![Issues][issues-badge]][issues-url]
[![Gitter][gitter-badge]][gitter-url]

Telemetry Agent Overview
========================

This service analyzes the telemetry stream, stores messages from Azure IoT Hub
to DocumentDb, and generates alerts according to defined rules.
The IoT stream is analyzed using a set of rules defined in the
[Telemetry service](https://github.com/Azure/device-telemetry-dotnet),
and generates "alarms" when a message matches some of these rules. The alarms
are also stored in DocumentDb.

We provide also a
[Java version](https://github.com/Azure/telemetry-agent-java)
of this project.

Dependencies
============
* [Azure Cosmos DB account](https://ms.portal.azure.com/#create/Microsoft.DocumentDB) use the one created for [Storage Adapter microservice](https://github.com/Azure/pcs-storage-adapter-dotnet)
* [Telemetry](https://github.com/Azure/device-telemetry-dotnet)
* [Config](https://github.com/Azure/pcs-config-dotnet)
* [IoT Hub Manager](https://github.com/Azure/iothub-manager-dotnet)
* [Azure IoT Hub](https://azure.microsoft.com/services/iot-hub) use the one created for [IoT Hub Manager](https://github.com/Azure/iothub-manager-dotnet)

How to use the microservice
===========================

## Quickstart - Running the service with Docker

1. Install Docker Compose: https://docs.docker.com/compose/install
1. Create an instance of an [Azure IoT Hub](https://azure.microsoft.com/services/iot-hub)
1. Store the "IoT Hub" connection string  in the [env-vars-setup](scripts)
   script.
1. Open a terminal console into the project folder, and run these commands to start
   the [Telemetry Agent](https://github.com/Azure/telemetry-agent-dotnet) service
   ```
   cd scripts
   env-vars-setup      // (Bash users: ./env-vars-setup).  This script creates your env. variables
   cd docker
   docker-compose up
   ```
The Docker compose configuration requires the [dependencies](README.md#dependencies) resolved and
environment variables set as described previously. You should now start seeing the stream
content in the console.

Contributing to the solution
============================
Please follow our [contribution guildelines](CONTRIBUTING.md).  We love PRs too.

Troubleshooting
===============

Feedback
========
Please enter issues, bugs, or suggestions as GitHub Issues [here](https://github.com/Azure/telemetry-agent-dotnet/issues)

[build-badge]: https://img.shields.io/travis/Azure/telemetry-agent-dotnet.svg
[build-url]: https://travis-ci.org/Azure/telemetry-agent-dotnet
[issues-badge]: https://img.shields.io/github/issues/azure/telemetry-agent-dotnet.svg
[issues-url]: https://github.com/azure/telemetry-agent-dotnet/issues
[gitter-badge]: https://img.shields.io/gitter/room/azure/iot-pcs.js.svg
[gitter-url]: https://gitter.im/azure/iot-pcs
