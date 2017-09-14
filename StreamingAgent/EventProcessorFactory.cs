// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.EventHubs.Processor;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent
{
    internal class EventProcessorFactory : IEventProcessorFactory
    {
        private readonly DependencyResolution.IFactory factory;

        public EventProcessorFactory(DependencyResolution.IFactory factory)
        {
            this.factory = factory;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return factory.Resolve<IEventProcessor>();
        }
    }
}