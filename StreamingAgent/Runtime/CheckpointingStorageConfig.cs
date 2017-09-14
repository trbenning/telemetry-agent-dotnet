// Copyright (c) Microsoft. All rights reserved.


namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface ICheckpointingStorageConfig
    {
        string BackendType { get; }
        string Namespace { get; }
        ICheckpointingStorageBlobConfig AzureBlobConfig { get; }
    }

    public class CheckpointingStorageConfig : ICheckpointingStorageConfig
    {
        public string BackendType { get; set; }
        public string Namespace { get; set; }
        public ICheckpointingStorageBlobConfig AzureBlobConfig { get; set; }
    }
}