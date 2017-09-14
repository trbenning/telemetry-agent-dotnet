// Copyright (c) Microsoft. All rights reserved.

using System.Threading;
using Autofac;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = DependencyResolution.Setup();
            var agent = container.Resolve<IAgent>();

            var cts = new CancellationTokenSource();
            var task = agent.RunAsync(cts.Token);

            // Wait until task completed or canceled
            task.Wait();
        }
    }
}
