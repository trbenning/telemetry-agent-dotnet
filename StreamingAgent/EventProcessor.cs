// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent
{
    internal class EventProcessor : IEventProcessor
    {
        private readonly ILogger logger;
        private readonly IConfig config;
        private readonly IMessages messagesProcessor;
        private readonly IThroughputCounter throughtCounter;

        private readonly string processorId;

        private DateTime nextCheckpoint;
        private int messagesSinceLastCheckpoint;

        public EventProcessor(
            ILogger logger,
            IConfig config,
            IMessages messagesProcessor,
            IThroughputCounter throughtCounter)
        {
            this.logger = logger;
            this.config = config;
            this.messagesProcessor = messagesProcessor;
            this.throughtCounter = throughtCounter;

            this.processorId = Guid.NewGuid().ToString();
            logger.Info("EventProcessor created", () => new { processorId });
        }

        public async Task OpenAsync(PartitionContext context)
        {
            logger.Info("Partition opened", () => new { processorId, context });

            ResetCheckpointThreshold();
            await Task.FromResult(0);
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            logger.Info("Partition closed", () => new { processorId, context, reason });

            await Task.FromResult(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            if (messages == null)
            {
                messages = new EventData[] { };
            }

            var totalMessages = messages.Count();
            throughtCounter.Add(totalMessages);
            logger.Info($"{totalMessages} message(s) received", () => new { processorId, context });

            foreach (var message in messages)
            {
                await messagesProcessor.ProcessAsync(message);
            }

            messagesSinceLastCheckpoint += totalMessages;
            if (messagesSinceLastCheckpoint >= config.IoTHubConfig.CheckpointingConfig.CountThreshold || DateTime.UtcNow > nextCheckpoint)
            {
                await context.CheckpointAsync();

                ResetCheckpointThreshold();
            }
        }

        public async Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            logger.Warn("Processor error", () => new { processorId, context, error });

            await Task.FromResult(0);
        }

        private void ResetCheckpointThreshold()
        {
            nextCheckpoint = DateTime.UtcNow + config.IoTHubConfig.CheckpointingConfig.TimeThreshold;
            messagesSinceLastCheckpoint = 0;
        }
    }
}