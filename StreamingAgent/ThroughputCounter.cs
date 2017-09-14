using System.Threading;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent
{
    internal interface IThroughputCounter
    {
        long Total { get; }

        void Add(int value);
    }

    internal class ThroughputCounter : IThroughputCounter
    {
        private long total = 0;

        public long Total => total;

        public void Add(int value)
        {
            Interlocked.Add(ref total, value);
        }
    }
}
