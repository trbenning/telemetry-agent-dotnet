// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface ICheckpointingConfig
    {
        TimeSpan Frequency { get; }
        int CountThreshold { get; }
        TimeSpan TimeThreshold { get; }
        ICheckpointingStorageConfig StorageConfig { get; }
    }

    public class CheckpointingConfig : ICheckpointingConfig
    {
        public TimeSpan Frequency { get; set; }
        public int CountThreshold { get; set; }
        public TimeSpan TimeThreshold { get; set; }
        public ICheckpointingStorageConfig StorageConfig { get; set; }
    }
}