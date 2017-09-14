#!/usr/bin/env bash

cd /app/

dotnet webservice/Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.WebService.dll & \
    dotnet streamingagent/Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.dll && \
    fg
