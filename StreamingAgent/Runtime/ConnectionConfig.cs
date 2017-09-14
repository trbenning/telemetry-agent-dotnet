// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface IConnectionConfig
    {
        string HubName { get; }
        string HubEndpoint { get; }
        string AccessConnString { get; }
    }

    public class ConnectionConfig : IConnectionConfig
    {
        public string HubName { get; set; }
        public string HubEndpoint { get; set; }
        public string AccessConnString { get; set; }
    }
}