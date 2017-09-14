// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Extensions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public interface IDeviceMessageParser
    {
        RawMessage MessageToRawMessage(EventData m);
    }

    public class DeviceMessageParser : IDeviceMessageParser
    {
        private readonly ILogger logger;

        public const string MessageSchemaKey = "$$MessageSchema";
        public const string ContentTypeKey = "$$ContentType";
        public const string CreationTimeUtcKey = "$$CreationTimeUtc";
        public const string DeviceIdKey = "iothub-connection-device-id";

        private readonly HashSet<string> builtInProperties = new HashSet<string>
        {
            DeviceIdKey,
            "iothub-connection-auth-method",
            "iothub-connection-auth-generation-id",
            "iothub-enqueuedtime",
            "iothub-message-source",
            "x-opt-sequence-number",
            "x-opt-offset",
            "x-opt-enqueued-time",
            MessageSchemaKey,
            ContentTypeKey,
            CreationTimeUtcKey
        };

        public DeviceMessageParser(ILogger logger)
        {
            this.logger = logger;
        }

        public RawMessage MessageToRawMessage(EventData m)
        {
            var schema = m.Properties[MessageSchemaKey].ToString();
            var contentType = m.Properties[ContentTypeKey].ToString();
            var deviceId = m.Properties[DeviceIdKey].ToString();
            var content = Encoding.UTF8.GetString(m.Body.Array);
            var now = DateTime.UtcNow;
            var enqueueTime = DateTime.Parse(m.Properties[CreationTimeUtcKey].ToString());

            var result = new RawMessage
            {
                Id = $"{deviceId};{now.ToEpochMilli()}",
                DeviceId = deviceId,
                Schema = schema,
                CreateTime = enqueueTime.ToEpochMilli(),
                ReceivedTime = now.ToEpochMilli()
            };

            // Save all the message properties from the message header
            foreach (var pair in m.Properties.Where(p => !builtInProperties.Contains(p.Key)))
            {
                result.PropsData.Add(pair.Key, pair.Value.ToString());
            }

            // Save all the message properties from the message payload
            if (!contentType.ToLowerInvariant().Contains("json") && !schema.ToLowerInvariant().Contains("json") && (!contentType.IsNullOrWhiteSpace() || !content.Contains("{")))
            {
                goto Done;
            }

            object obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject(content);
            }
            catch (JsonReaderException e)
            {
                // ToDo: retrieve partition ID and message ID
                // Log as an error, including stream information
                logger.Error($"Invalid JSON: {e.Message}, partition:{"N/A"}, offset:{m.SystemProperties.Offset}, msgId:{"N/A"}, device:{deviceId}, msgTime:{enqueueTime}, msg:{content}", () => { });
            }

            if (obj == null)
            {
                goto Done;
            }

            var root = obj as JToken;
            if (root != null)
            {
                foreach (var field in root.Children<JProperty>())
                {
                    var value = field.Value as JValue;
                    if (value != null)
                    {
                        result.PayloadData.Add(field.Name, value.Value);
                    }
                }
            }
            else
            {
                result.PayloadData.Add("value", obj);
            }

            Done:
            return result;
        }
    }
}