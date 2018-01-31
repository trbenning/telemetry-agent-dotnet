// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent
{
    internal interface IAgent
    {
        Task RunAsync(CancellationToken ct);
    }

    internal class Agent : IAgent
    {
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly IEventProcessorFactory processorFactory;
        private readonly IThroughputCounter throughtCounter;
        private readonly IMessages messages;

        private EventProcessorHost eventProcessorHost;

        /// <summary>
        /// Every 60 seconds, reload the processing logic (e.g. reload rules)
        /// </summary>
        private const int RefreshLogicFrequency = 60;

        /// <summary>
        /// Every 30 seconds log the throughput
        /// </summary>
        private const int LogThroughputFrequency = 30;

        /// <summary>
        /// Streaming options: use the checkpoint if available,
        /// otherwise start streaming from 24 hours in the past
        /// </summary>
        private const int StartFrom = 24;

        public Agent(
            ILogger logger,
            IConfig config,
            IEventProcessorFactory factory,
            IThroughputCounter throughtCounter,
            IMessages messages)
        {
            this.config = config;
            this.logger = logger;
            this.processorFactory = factory;
            this.throughtCounter = throughtCounter;
            this.messages = messages;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            PrintBootstrapInfo();

            await RunEventProcessorAsync();

            var refreshLogicTask = RefreshLogicAsync(ct);
            var logThroughtputTask = LogThroughputAsync(ct);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled. Nothing to do
                }
            }

            await eventProcessorHost.UnregisterEventProcessorAsync();
            await Task.WhenAll(refreshLogicTask, logThroughtputTask);
        }

        private void PrintBootstrapInfo()
        {
            logger.Info($"Streaming agent started, ProcessId {Uptime.ProcessId}", () => { });
            logger.Info($"IoT Hub name: {config.IoTHubConfig.ConnectionConfig.HubName}", () => { });
            logger.Info($"IoT Hub endpoint: {config.IoTHubConfig.ConnectionConfig.HubEndpoint}", () => { });
            logger.Info($"Checkpointing storage: {config.IoTHubConfig.CheckpointingConfig.StorageConfig.BackendType}", () => { });
            logger.Info($"Checkpointing namespace: {config.IoTHubConfig.CheckpointingConfig.StorageConfig.Namespace}", () => { });
            logger.Info($"Checkpointing min frequency: {config.IoTHubConfig.CheckpointingConfig.Frequency}", () => { });
        }

        private async Task RunEventProcessorAsync()
        {
            switch (config.IoTHubConfig.CheckpointingConfig.StorageConfig.BackendType)
            {
                case "AzureBlob":
                    await RegisterEventProcessorFactoryWithBlobCheckpointingAsync();
                    break;

                default:
                    // ToDo: support other checkpoints via ICheckpointManager & ILeaseManager
                    logger.Error("Unexpected checkpointing backend storage", () => new { config.IoTHubConfig.CheckpointingConfig.StorageConfig.BackendType });
                    throw new InvalidConfigurationException("Checkpointing backend storage must be Azure Blob.");
            }
        }

        private async Task RegisterEventProcessorFactoryWithBlobCheckpointingAsync()
        {
            var storageCredentials = new StorageCredentials(
                config.IoTHubConfig.CheckpointingConfig.StorageConfig.AzureBlobConfig.Account,
                config.IoTHubConfig.CheckpointingConfig.StorageConfig.AzureBlobConfig.Key);

            var storageAccount = new CloudStorageAccount(
                storageCredentials,
                config.IoTHubConfig.CheckpointingConfig.StorageConfig.AzureBlobConfig.EndpointSuffix,
                config.IoTHubConfig.CheckpointingConfig.StorageConfig.AzureBlobConfig.Protocol == "https");

            var iotHubConnectionBuilder = IotHubConnectionStringBuilder.Create(config.IoTHubConfig.ConnectionConfig.AccessConnString);

            var eventHubConntionStringBuilder = new EventHubsConnectionStringBuilder(
                new Uri(config.IoTHubConfig.ConnectionConfig.HubEndpoint),
                config.IoTHubConfig.ConnectionConfig.HubName,
                iotHubConnectionBuilder.SharedAccessKeyName,
                iotHubConnectionBuilder.SharedAccessKey);

            eventProcessorHost = new EventProcessorHost(
                config.IoTHubConfig.ConnectionConfig.HubName,
                config.IoTHubConfig.StreamingConfig.ConsumerGroup,
                eventHubConntionStringBuilder.ToString(),
                storageAccount.ToString(true),
                config.IoTHubConfig.CheckpointingConfig.StorageConfig.Namespace);

            var options = new EventProcessorOptions
            {
                InitialOffsetProvider = InitialOffset,
                MaxBatchSize = config.IoTHubConfig.StreamingConfig.ReceiveBatchSize,
                ReceiveTimeout = config.IoTHubConfig.StreamingConfig.ReceiveTimeout,
                InvokeProcessorAfterReceiveTimeout = true
            };

            await eventProcessorHost.RegisterEventProcessorFactoryAsync(processorFactory, options);
        }

        private static object InitialOffset(string partitionId)
        {
            return DateTime.UtcNow - TimeSpan.FromHours(StartFrom);
        }

        private async Task RefreshLogicAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(RefreshLogicFrequency), ct);
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled. Nothing to do
                }

                await messages.RefreshLogicAsync();
            }
        }

        private async Task LogThroughputAsync(CancellationToken ct)
        {
            var throughputPreviousTotal = 0L;
            var throughputPreviousTime = DateTime.MinValue;

            while (!ct.IsCancellationRequested)
            {
                var throughputTotal = throughtCounter.Total;
                var now = DateTime.UtcNow;

                if (throughputPreviousTime > DateTime.MinValue)
                {
                    var countDelta = throughputTotal - throughputPreviousTotal;
                    var timeDelta = now - throughputPreviousTime;
                    var throughput = countDelta / timeDelta.TotalSeconds;

                    logger.Info($"Throughput: {throughput} msgs/sec - {countDelta} messages in the last {timeDelta.TotalSeconds} seconds", () => { });
                }

                throughputPreviousTotal = throughputTotal;
                throughputPreviousTime = now;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(LogThroughputFrequency), ct);
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled. Nothing to do
                }
            }
        }
    }
}