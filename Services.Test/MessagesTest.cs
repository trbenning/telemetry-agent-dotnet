// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class MessagesTest
    {
        private readonly Random rand = new Random();
        private readonly ILogger logger = new Logger("UnitTest", LogLevel.Debug);
        private readonly Mock<IDeviceMessageParser> mockMessageParser = new Mock<IDeviceMessageParser>();
        private readonly Mock<IAlarms> mockAlarms = new Mock<IAlarms>();
        private readonly Mock<IDocumentWriter> mockWriter = new Mock<IDocumentWriter>();

        [Fact]
        public void ConstructorTest()
        {
            mockWriter
                .Setup(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()))
                .Returns(Task.FromResult(0));

            var unused = new Messages(
                logger,
                new ServicesConfig { MessagesStorageServiceConfig = null },
                mockMessageParser.Object,
                mockAlarms.Object,
                mockWriter.Object);

            mockWriter
                .Verify(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()), Times.Once);
        }

        [Fact]
        public async Task ProcessTest()
        {
            mockWriter
                .Setup(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()))
                .Returns(Task.FromResult(0));

            var messages = new Messages(
                logger,
                new ServicesConfig { MessagesStorageServiceConfig = null },
                mockMessageParser.Object,
                mockAlarms.Object,
                mockWriter.Object);

            // Empty input event
            var @event = new EventData(new byte[] { });

            // Mock parsed message
            var message = rand.NextRawMessage();

            // Expected output document
            var document = new Document
            {
                Id = message.Id
            };
            document.SetPropertyValue(Messages.DocSchemaKey, Messages.DocSchemaValue);
            document.SetPropertyValue(Messages.DocSchemaVersionKey, Messages.DocSchemaVersionVersion);
            document.SetPropertyValue(RawMessage.DeviceIdKey, message.DeviceId);
            document.SetPropertyValue(RawMessage.MessageSchemaKey, message.Schema);
            document.SetPropertyValue(RawMessage.MessageCreatedKey, message.CreateTime);
            document.SetPropertyValue(RawMessage.MessageReceivedKey, message.ReceivedTime);

            foreach (var pair in message.PropsData)
            {
                document.SetPropertyValue($"{RawMessage.MsgPropertiesPrefix}{pair.Key}", pair.Value);
            }

            foreach (var pair in message.PayloadData)
            {
                document.SetPropertyValue($"{RawMessage.MsgPayloadPrefix}{pair.Key}", pair.Value);
            }

            mockMessageParser
                .Setup(x => x.MessageToRawMessage(It.IsAny<EventData>()))
                .Returns(message);

            mockWriter
                .Setup(x => x.WriteAsync(It.IsAny<Document>()))
                .Returns(Task.FromResult(0));

            mockAlarms
                .Setup(x => x.ProcessAsync(It.IsAny<RawMessage>()))
                .Returns(Task.FromResult(0));

            await messages.ProcessAsync(@event);

            mockMessageParser
                .Verify(x => x.MessageToRawMessage(
                    It.Is<EventData>(e => e == @event)),
                    Times.Once);

            mockWriter
                .Verify(x => x.WriteAsync(
                    It.Is<Document>(d => d.ToString() == document.ToString())),
                    Times.Once);

            mockAlarms
                .Verify(x => x.ProcessAsync(
                    It.Is<RawMessage>(m => m == message)),
                    Times.Once);
        }
    }
}
