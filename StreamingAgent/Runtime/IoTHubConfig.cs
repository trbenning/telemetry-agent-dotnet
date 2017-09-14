// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface IIoTHubConfig
    {
        IConnectionConfig ConnectionConfig { get; }
        IStreamingConfig StreamingConfig { get; }
        ICheckpointingConfig CheckpointingConfig { get; }
    }

    public class IoTHubConfig : IIoTHubConfig
    {
        public IConnectionConfig ConnectionConfig { get; set; }
        public IStreamingConfig StreamingConfig { get; set; }
        public ICheckpointingConfig CheckpointingConfig { get; set; }
    }
}