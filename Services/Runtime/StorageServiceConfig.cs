namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime
{
    public interface IStorageServiceConfig
    {
        string StorageType { get; }
        string DocumentDbConnString { get; }
        string DocumentDbDatabase { get; }
        string DocumentDbCollection { get; }
        int DocumentDbRUs { get; }
    }

    public class StorageServiceConfig : IStorageServiceConfig
    {
        public string StorageType { get; set; }
        public string DocumentDbConnString { get; set; }
        public string DocumentDbDatabase { get; set; }
        public string DocumentDbCollection { get; set; }
        public int DocumentDbRUs { get; set; }
    }
}
