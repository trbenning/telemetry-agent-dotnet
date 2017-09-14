// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public interface IMessages
    {
        Task ProcessAsync(EventData m);
        Task RefreshLogicAsync();
    }

    public class Messages : IMessages
    {
        private readonly ILogger logger;
        private readonly IDeviceMessageParser messageParser;
        private readonly IAlarms alarms;
        private readonly IDocumentWriter documentWriter;

        public const string DocSchemaKey = "doc.schema";
        public const string DocSchemaValue = "d2cmessage";
        public const string DocSchemaVersionKey = "doc.schemaVersion";
        public const int DocSchemaVersionVersion = 1;

        public Messages(
            ILogger logger,
            IServicesConfig config,
            IDeviceMessageParser messageParser,
            IAlarms alarms,
            IDocumentWriter documentWriter)
        {
            this.logger = logger;
            this.messageParser = messageParser;
            this.alarms = alarms;
            this.documentWriter = documentWriter;
            this.documentWriter.OpenAsync(config.MessagesStorageServiceConfig).Wait();
        }

        public async Task ProcessAsync(EventData m)
        {
            var message = messageParser.MessageToRawMessage(m);

            logger.Debug("Saving message...", () => { });
            await documentWriter.WriteAsync(MessageToDocument(message));

            logger.Debug("Analyzing message...", () => { });
            await alarms.ProcessAsync(message);
        }

        private static Document MessageToDocument(RawMessage message)
        {
            var document = new Document
            {
                Id = message.Id
            };

            document.SetPropertyValue(DocSchemaKey, DocSchemaValue);
            document.SetPropertyValue(DocSchemaVersionKey, DocSchemaVersionVersion);

            document.SetPropertyValue(RawMessage.DeviceIdKey, message.DeviceId);
            document.SetPropertyValue(RawMessage.MessageSchemaKey, message.Schema);
            document.SetPropertyValue(RawMessage.MessageCreatedKey, message.CreateTime);
            document.SetPropertyValue(RawMessage.MessageReceivedKey, message.ReceivedTime);

            // Message properties
            foreach (var pair in message.PropsData)
            {
                document.SetPropertyValue($"{RawMessage.MsgPropertiesPrefix}{pair.Key}", pair.Value);
            }

            // Message payload
            foreach (var pair in message.PayloadData)
            {
                document.SetPropertyValue($"{RawMessage.MsgPayloadPrefix}{pair.Key}", pair.Value);
            }

            return document;
        }

        public async Task RefreshLogicAsync()
        {
            await alarms.ReloadRulesAsync();
        }
    }
}
