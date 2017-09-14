// Copyright (c) Microsoft. All rights reserved.


namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface ICheckpointingStorageBlobConfig
    {
        string Protocol { get; }
        string Account { get; }
        string Key { get; }
        string EndpointSuffix { get; }
    }

    public class CheckpointingStorageBlobConfig : ICheckpointingStorageBlobConfig
    {
        public string Protocol { get; set; }
        public string Account { get; set; }
        public string Key { get; set; }
        public string EndpointSuffix { get; set; }
    }
}