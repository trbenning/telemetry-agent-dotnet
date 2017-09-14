// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class RawMessage
    {
        public const string MessageIdKey = "id";
        public const string DeviceIdKey = "device.id";
        public const string MessageSchemaKey = "device.msg.schema";
        public const string MessageCreatedKey = "device.msg.created";
        public const string MessageReceivedKey = "device.msg.received";

        public const string MsgPropertiesPrefix = "metadata.";
        public const string MsgPayloadPrefix = "data.";

        private readonly Dictionary<string, object> reservedData = new Dictionary<string, object>();

        // RESERVED FIELDS
        public string Id
        {
            get { return reservedData[MessageIdKey] as string; }
            set { reservedData.Add(MessageIdKey, value); }
        }

        public string DeviceId
        {
            get { return reservedData[DeviceIdKey] as string; }
            set { reservedData.Add(DeviceIdKey, value); }
        }

        public string Schema
        {
            get { return reservedData[MessageSchemaKey] as string; }
            set { reservedData.Add(MessageSchemaKey, value); }
        }

        public long CreateTime
        {
            get { return (long)reservedData[MessageCreatedKey]; }
            set { reservedData.Add(MessageCreatedKey, value); }
        }

        public long ReceivedTime
        {
            get { return (long)reservedData[MessageReceivedKey]; }
            set { reservedData.Add(MessageReceivedKey, value); }
        }

        // MESSAGE PROPERTIES FIELDS
        public Dictionary<string, object> PropsData { get; } = new Dictionary<string, object>();

        // MESSAGE PAYLOAD FIELDS
        public Dictionary<string, object> PayloadData { get; } = new Dictionary<string, object>();
    }
}
