// Copyright (c) Microsoft. All rights reserved.


namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.WebService.Runtime
{
    public interface IConfig
    {
        /// <summary>Web service listening port</summary>
        int Port { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "telemetryagent:";
        private const string PortKey = ApplicationKey + "webservice_port";

        /// <summary>Web service listening port</summary>
        public int Port { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PortKey);
        }
    }
}
