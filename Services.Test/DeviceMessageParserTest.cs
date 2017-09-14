// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class DeviceMessageParserTest
    {
        private readonly Random rand = new Random();
        private readonly ILogger logger = new Logger("UnitTest", LogLevel.Debug);
        private readonly DeviceMessageParser parser;

        public DeviceMessageParserTest()
        {
            parser = new DeviceMessageParser(logger);
        }

        [Fact]
        public void MessageToRawMessageTest()
        {
            var schema = rand.NextString();
            var contentType = "JSON";
            var deviceId = rand.NextString();
            var createTime = rand.NextDateTimeOffset().UtcDateTime;

            var properties = Enumerable.Range(0, rand.Next(3, 10))
                .Select(i => new KeyValuePair<string, string>(rand.NextString(), rand.NextString()))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var payloadItems = Enumerable.Range(0, rand.Next(3, 10))
                .Select(i => new KeyValuePair<string, object>(rand.NextString(), rand.NextRandomTypeObject()))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var content = JsonConvert.SerializeObject(payloadItems);

            var @event = new EventData(Encoding.UTF8.GetBytes(content))
            {
                Properties =
                {
                    [DeviceMessageParser.ContentTypeKey] = contentType,
                    [DeviceMessageParser.MessageSchemaKey] = schema,
                    [DeviceMessageParser.DeviceIdKey] = deviceId,
                    [DeviceMessageParser.CreationTimeUtcKey] = createTime
                }
            };

            foreach (var pair in properties)
            {
                @event.Properties[pair.Key] = pair.Value;
            }

            var result = parser.MessageToRawMessage(@event);

            Assert.True(result.Id.StartsWith($"{deviceId};"));
            Assert.Equal(result.DeviceId, deviceId);
            Assert.Equal(result.Schema, schema);
            Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(result.CreateTime), createTime);

            Assert.Equal(result.PropsData.Count, properties.Count);
            foreach (var pair in properties)
            {
                Assert.Equal(pair.Value, result.PropsData[pair.Key]);
            }

            Assert.Equal(result.PayloadData.Count, payloadItems.Count);
            foreach (var pair in payloadItems)
            {
                Assert.Equal(pair.Value.ToString(), result.PayloadData[pair.Key].ToString());
            }
        }

        [Fact]
        public void PlainMessageTest()
        {
            // Repeat times for broader coverage
            foreach (var unused in Enumerable.Range(0, 10))
            {
                var schema = rand.NextString();
                var contentType = "JSON";
                var deviceId = rand.NextString();
                var createTime = rand.NextDateTimeOffset().UtcDateTime;

                var payload = rand.NextRandomTypeObject();
                var content = JsonConvert.SerializeObject(payload);

                var @event = new EventData(Encoding.UTF8.GetBytes(content))
                {
                    Properties =
                    {
                        [DeviceMessageParser.ContentTypeKey] = contentType,
                        [DeviceMessageParser.MessageSchemaKey] = schema,
                        [DeviceMessageParser.DeviceIdKey] = deviceId,
                        [DeviceMessageParser.CreationTimeUtcKey] = createTime
                    }
                };

                var result = parser.MessageToRawMessage(@event);

                Assert.True(result.Id.StartsWith($"{deviceId};"));
                Assert.Equal(result.DeviceId, deviceId);
                Assert.Equal(result.Schema, schema);
                Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(result.CreateTime), createTime);

                Assert.Equal(result.PayloadData["value"].ToString(), payload.ToString());
            }
        }
    }
}
