// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Extensions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public interface IAlarms
    {
        Task ProcessAsync(RawMessage message);
        Task ReloadRulesAsync();
    }

    public class Alarms : IAlarms
    {
        private readonly ILogger logger;
        private readonly IRules rules;
        private readonly IRulesEvaluation rulesEvaluation;
        private readonly IDocumentWriter documentWriter;

        private IEnumerable<RuleApiModel> monitoringRules;

        public const string DocSchemaKey = "doc.schema";
        public const string DocSchemaValue = "alarm";

        public const string DocSchemaVersionKey = "doc.schemaVersion";
        public const int DocSchemaVersionValue = 1;

        public const string CreatedKey = "created";
        public const string ModifiedKey = "modified";
        public const string DescriptionKey = "description";
        public const string StatusKey = "status";
        public const string DeviceIdKey = "device.id";
        public const string MessageReceivedTimeKey = "device.msg.received";

        public const string RuleIdKey = "rule.id";
        public const string RuleSeverityKey = "rule.severity";
        public const string RuleDescriptionKey = "rule.description";

        // TODO: https://github.com/Azure/telemetry-agent-java/issues/34
        public const string NewAlarmStatus = "open";

        public const string LogicKey = "logic";
        public const string LogicValue = "1Rule-1Device-1Message";

        public Alarms(
            ILogger logger,
            IServicesConfig config,
            IRules rules,
            IRulesEvaluation rulesEvaluation,
            IDocumentWriter documentWriter)
        {
            this.logger = logger;
            this.rules = rules;
            this.rulesEvaluation = rulesEvaluation;
            this.documentWriter = documentWriter;
            this.documentWriter.OpenAsync(config.AlarmsStorageServiceConfig).Wait();
            LoadAllRulesAsync().Wait();
        }

        public async Task ProcessAsync(RawMessage message)
        {
            foreach (var rule in monitoringRules.ToList())
            {
                logger.Debug($"Evaluating rule {rule.Description} for device {message.DeviceId}", () => { });
                var result = rulesEvaluation.Evaluate(rule, message);
                if (!result.Match)
                {
                    continue;
                }

                logger.Info("Alarm!", () => new { result.Message });
                await CreateAlarmAsync(rule, message, result.Message);
            }
        }

        private async Task CreateAlarmAsync(
            RuleApiModel rule,
            RawMessage deviceMessage,
            string alarmDescription)
        {
            var created = DateTime.UtcNow;

            var alarm = new Alarm
            {
                Id = Guid.NewGuid().ToString(),
                DateCreated = created,
                DateModified = created,
                MessageReceivedTime = deviceMessage.ReceivedTime,
                Description = alarmDescription,
                DeviceId = deviceMessage.DeviceId,
                Status = NewAlarmStatus,
                RuleId = rule.Id,
                RuleServerity = rule.Severity,
                RuleDescription = rule.Description
            };

            await documentWriter.WriteAsync(AlarmToDocument(alarm));
        }

        private async Task LoadAllRulesAsync()
        {
            logger.Debug("Loading rules...", () => { });
            IEnumerable<RuleApiModel> result = null;

            try
            {
                result = (await rules.GetAllAsync()).Where(r => r.Enabled);
                logger.Info($"Monitoring rules loaded: {result.Count()} rules", () => { });
            }
            catch
            {
                logger.Error("Unable to load monitoring rules", () => { });
            }

            monitoringRules = result;
        }

        private static Document AlarmToDocument(Alarm alarm)
        {
            // TODO: make inserts idempotent, e.g. gen Id from msg details
            var document = new Document
            {
                Id = Guid.NewGuid().ToString()
            };

            document.SetPropertyValue(DocSchemaKey, DocSchemaValue);
            document.SetPropertyValue(DocSchemaVersionKey, DocSchemaVersionValue);
            document.SetPropertyValue(CreatedKey, alarm.DateCreated.ToEpochMilli());
            document.SetPropertyValue(ModifiedKey, alarm.DateModified.ToEpochMilli());
            document.SetPropertyValue(StatusKey, alarm.Status);
            document.SetPropertyValue(DescriptionKey, alarm.Description);
            document.SetPropertyValue(RuleSeverityKey, alarm.RuleServerity);
            document.SetPropertyValue(RuleDescriptionKey, alarm.RuleDescription);

            // The logic used to generate the alarm (future proofing for ML)
            document.SetPropertyValue(LogicKey, LogicValue);
            document.SetPropertyValue(RuleIdKey, alarm.RuleId);
            document.SetPropertyValue(DeviceIdKey, alarm.DeviceId);
            document.SetPropertyValue(MessageReceivedTimeKey, alarm.MessageReceivedTime);

            return document;
        }

        public async Task ReloadRulesAsync()
        {
            await LoadAllRulesAsync();
        }
    }
}
