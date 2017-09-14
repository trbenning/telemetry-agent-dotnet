// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime
{
    public interface IServicesConfig
    {
        string MonitoringRulesUrl { get; }
        string DeviceGroupsUrl { get; }
        string DevicesUrl { get; }
        IStorageServiceConfig MessagesStorageServiceConfig { get; }
        IStorageServiceConfig AlarmsStorageServiceConfig { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string MonitoringRulesUrl { get; set; }
        public string DeviceGroupsUrl { get; set; }
        public string DevicesUrl { get; set; }
        public IStorageServiceConfig MessagesStorageServiceConfig { get; set; }
        public IStorageServiceConfig AlarmsStorageServiceConfig { get; set; }
    }
}
