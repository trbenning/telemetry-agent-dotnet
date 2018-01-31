// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime
{
    public interface IConfig
    {
        /// <summary>
        /// IoTHub connection configuration
        /// </summary>
        IIoTHubConfig IoTHubConfig { get; }

        /// <summary>
        /// Service layer configuration
        /// </summary>
        IServicesConfig ServicesConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string ApplicationKey = "telemetryagent:";

        private const string IotHubKey = ApplicationKey + "iothub:";
        private const string ConnectionKey = IotHubKey + "connection:";
        private const string HubNameKey = ConnectionKey + "hubName";
        private const string HubEndpointKey = ConnectionKey + "hubEndpoint";
        private const string AccessConnStringKey = ConnectionKey + "accessConnString";

        private const string StreamingKey = IotHubKey + "streaming:";
        private const string ConsumerGroupKey = StreamingKey + "consumerGroup";
        private const string ReceiveBatchSizeKey = StreamingKey + "receiveBatchSize";
        private const string ReceiveTimeoutKey = StreamingKey + "receiveTimeout";

        private const string CheckpointingKey = IotHubKey + "checkpointing:";
        private const string FrequencyKey = CheckpointingKey + "frequency";
        private const string CountThresholdKey = CheckpointingKey + "countThreshold";
        private const string TimeThresholdKey = CheckpointingKey + "timeThreshold";

        private const string StorageKey = CheckpointingKey + "storage:";
        private const string BackendTypeKey = StorageKey + "backendType";
        private const string NamespaceKey = StorageKey + "namespace";

        private const string AzureBlobKey = StorageKey + "azureblob:";
        private const string ProtocolKey = AzureBlobKey + "protocol";
        private const string AccountKey = AzureBlobKey + "account";
        private const string KeyKey = AzureBlobKey + "key";
        private const string EndpointSuffixKey = AzureBlobKey + "endpointSuffix";

        private const string MonitoringRulesUrlKey = ApplicationKey + "monitoringRulesUrl";
        private const string DeviceGroupsUrlKey = ApplicationKey + "deviceGroupsUrl";
        private const string DevicesUrlKey = ApplicationKey + "devicesUrl";

        private const string MessagesKey = ApplicationKey + "messages:";
        private const string MessagesStorageTypeKey = MessagesKey + "storageType";
        private const string MessagesDocDbConnStringKey = MessagesKey + "documentDb:connString";
        private const string MessagesDocDbDatabaseKey = MessagesKey + "documentDb:database";
        private const string MessagesDocDbCollectionKey = MessagesKey + "documentDb:collection";
        private const string MessagesDocDbRUsKey = MessagesKey + "documentDb:RUs";

        private const string AlarmsKey = ApplicationKey + "alarms:";
        private const string AlarmsStorageTypeKey = AlarmsKey + "storageType";
        private const string AlarmsDocDbConnStringKey = AlarmsKey + "documentDb:connString";
        private const string AlarmsDocDbDatabaseKey = AlarmsKey + "documentDb:database";
        private const string AlarmsDocDbCollectionKey = AlarmsKey + "documentDb:collection";
        private const string AlarmsDocDbRUsKey = AlarmsKey + "documentDb:RUs";

        /// <summary>
        /// IoTHub connection configuration
        /// </summary>
        public IIoTHubConfig IoTHubConfig { get; }

        /// <summary>
        /// Service layer configuration
        /// </summary>
        public IServicesConfig ServicesConfig { get; }

        public Config(IConfigData configData)
        {
            IoTHubConfig = new IoTHubConfig
            {
                ConnectionConfig = new ConnectionConfig
                {
                    HubName = configData.GetString(HubNameKey),
                    HubEndpoint = parseIoTHubEndpoint(configData.GetString(HubEndpointKey)),
                    AccessConnString = configData.GetString(AccessConnStringKey)
                },
                StreamingConfig = new StreamingConfig
                {
                    ConsumerGroup = configData.GetString(ConsumerGroupKey),
                    ReceiveBatchSize = configData.GetInt(ReceiveBatchSizeKey),
                    ReceiveTimeout = configData.GetTimeSpan(ReceiveTimeoutKey)
                },
                CheckpointingConfig = new CheckpointingConfig
                {
                    Frequency = configData.GetTimeSpan(FrequencyKey),
                    CountThreshold = configData.GetInt(CountThresholdKey),
                    TimeThreshold = configData.GetTimeSpan(TimeThresholdKey),
                    StorageConfig = new CheckpointingStorageConfig
                    {
                        BackendType = configData.GetString(BackendTypeKey),
                        Namespace = configData.GetString(NamespaceKey),
                        AzureBlobConfig = new CheckpointingStorageBlobConfig
                        {
                            Protocol = configData.GetString(ProtocolKey),
                            Account = configData.GetString(AccountKey),
                            Key = configData.GetString(KeyKey),
                            EndpointSuffix = configData.GetString(EndpointSuffixKey)
                        }
                    }
                }
            };

            ServicesConfig = new ServicesConfig
            {
                MonitoringRulesUrl = configData.GetString(MonitoringRulesUrlKey),
                DeviceGroupsUrl = configData.GetString(DeviceGroupsUrlKey),
                DevicesUrl = configData.GetString(DevicesUrlKey),
                MessagesStorageServiceConfig = new StorageServiceConfig
                {
                    StorageType = configData.GetString(MessagesStorageTypeKey),
                    DocumentDbConnString = configData.GetString(MessagesDocDbConnStringKey),
                    DocumentDbDatabase = configData.GetString(MessagesDocDbDatabaseKey),
                    DocumentDbCollection = configData.GetString(MessagesDocDbCollectionKey),
                    DocumentDbRUs = configData.GetInt(MessagesDocDbRUsKey)
                },
                AlarmsStorageServiceConfig = new StorageServiceConfig
                {
                    StorageType = configData.GetString(AlarmsStorageTypeKey),
                    DocumentDbConnString = configData.GetString(AlarmsDocDbConnStringKey),
                    DocumentDbDatabase = configData.GetString(AlarmsDocDbDatabaseKey),
                    DocumentDbCollection = configData.GetString(AlarmsDocDbCollectionKey),
                    DocumentDbRUs = configData.GetInt(AlarmsDocDbRUsKey)
                }
            };

	    CheckConfiguration(IoTHubConfig);
	}

        private void CheckConfiguration(IIoTHubConfig ioTHubConfig)
        {
            if (string.IsNullOrWhiteSpace(ioTHubConfig.ConnectionConfig.HubName))
            {
                throw new InvalidConfigurationException("Azure IoT Hub name not found in the configuration.");
            }

            if (string.IsNullOrWhiteSpace(ioTHubConfig.ConnectionConfig.HubEndpoint))
            {
                throw new InvalidConfigurationException("Azure IoT Hub endpoint not found in the configuration.");
            }

            if (string.IsNullOrWhiteSpace(ioTHubConfig.CheckpointingConfig.StorageConfig.Namespace))
            {
                throw new InvalidConfigurationException("Checkpointing namespace not found in the configuration.");
            }

            if (ioTHubConfig.CheckpointingConfig.StorageConfig.BackendType != "AzureBlob")
            {
                throw new InvalidConfigurationException("Checkpointing backend storage must be Azure Blob.");
            }
        }

	private string parseIoTHubEndpoint(string hubEndpoint)
	{
            var match = Regex.Match(hubEndpoint, "^Endpoint=(?<endpoint>.*/)");
            if (match.Success)
            {
                hubEndpoint = match.Groups["endpoint"].Value;
            }

            if (!Uri.TryCreate(hubEndpoint, UriKind.Absolute, out Uri endpoint))
            {
	    	if (!Uri.TryCreate($"sb://{hubEndpoint}/", UriKind.Absolute, out endpoint))
	    	{
		    {
                        throw new InvalidConfigurationException($"Invalid IoTHub endpoint {hubEndpoint}");
		    }
	        }
            }

            return hubEndpoint;
	}
    }
}
