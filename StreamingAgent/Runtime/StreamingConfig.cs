// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface IStreamingConfig
    {
        string ConsumerGroup { get; }
        int ReceiveBatchSize { get; }
        TimeSpan ReceiveTimeout { get; }
    }

    public class StreamingConfig : IStreamingConfig
    {
        public string ConsumerGroup { get; set; }
        public int ReceiveBatchSize { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
    }
}