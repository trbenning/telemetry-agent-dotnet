// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
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
    public class AlarmsTest
    {
        private readonly Random rand = new Random();
        private readonly ILogger logger = new Logger("UnitTest", LogLevel.Debug);
        private readonly Mock<IRules> mockRules = new Mock<IRules>();
        private readonly Mock<IRulesEvaluation> mockRulesEvaluation = new Mock<IRulesEvaluation>();
        private readonly Mock<IDocumentWriter> mockWriter = new Mock<IDocumentWriter>();

        [Fact]
        public void ConstructorTest()
        {
            var rules = new RuleApiModel[] { };

            mockRules
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(rules);

            mockWriter
                .Setup(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()))
                .Returns(Task.FromResult(0));

            var unused = new Alarms(
                logger,
                new ServicesConfig { AlarmsStorageServiceConfig = null },
                mockRules.Object,
                mockRulesEvaluation.Object,
                mockWriter.Object);

            mockRules
                .Verify(x => x.GetAllAsync(), Times.Once);

            mockWriter
                .Verify(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()), Times.Once);
        }

        [Fact]
        public async Task DisabledRuleTest()
        {
            var rules = Enumerable.Range(0, rand.Next(3, 10)).Select(i => new RuleApiModel
            {
                Enabled = false
            }).ToList();

            mockRules
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(rules);

            mockWriter
                .Setup(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()))
                .Returns(Task.FromResult(0));

            var alarms = new Alarms(
                logger,
                new ServicesConfig { AlarmsStorageServiceConfig = null },
                mockRules.Object,
                mockRulesEvaluation.Object,
                mockWriter.Object);

            mockRulesEvaluation
                .Setup(x => x.Evaluate(
                    It.IsAny<RuleApiModel>(),
                    It.IsAny<RawMessage>()))
                .Returns(new RulesEvaluationResult { Match = false });

            var message = new RawMessage();
            await alarms.ProcessAsync(message);

            mockRulesEvaluation
                .Verify(x => x.Evaluate(
                    It.IsAny<RuleApiModel>(),
                    It.IsAny<RawMessage>()),
                    Times.Never);
        }

        [Fact]
        public async Task ProcessTest()
        {
            // Mock rule
            var ruleId = rand.NextString();
            var ruleDescription = rand.NextString();
            var ruleSeverity = rand.NextString();
            var ruleResultMessage = rand.NextString();

            var rules = new[] {
                new RuleApiModel
                {
                    Id = ruleId,
                    Name = rand.NextString(),
                    Enabled = true,
                    Description = ruleDescription,
                    GroupId = rand.NextString(),
                    Severity = ruleSeverity,
                    Conditions = new ConditionApiModel[] { }
                }
            };

            // Mock messages
            var matchMessage = rand.NextRawMessage();
            var unmatchMessage = rand.NextRawMessage();

            mockRules
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(rules);

            mockWriter
                .Setup(x => x.OpenAsync(It.IsAny<IStorageServiceConfig>()))
                .Returns(Task.FromResult(0));

            var alarms = new Alarms(
                logger,
                new ServicesConfig { AlarmsStorageServiceConfig = null },
                mockRules.Object,
                mockRulesEvaluation.Object,
                mockWriter.Object);

            mockRulesEvaluation
                .Setup(x => x.Evaluate(
                    It.IsAny<RuleApiModel>(),
                    It.Is<RawMessage>(m => m.DeviceId == matchMessage.DeviceId)))
                .Returns(new RulesEvaluationResult { Match = true, Message = ruleResultMessage });

            mockRulesEvaluation
                .Setup(x => x.Evaluate(
                    It.IsAny<RuleApiModel>(),
                    It.Is<RawMessage>(m => m.DeviceId == unmatchMessage.DeviceId)))
                .Returns(new RulesEvaluationResult { Match = false });

            mockWriter
                .Setup(x => x.WriteAsync(It.IsAny<Document>()))
                .Returns(Task.FromResult(0));

            await alarms.ProcessAsync(matchMessage);
            await alarms.ProcessAsync(unmatchMessage);

            mockRulesEvaluation
                .Verify(x => x.Evaluate(
                    It.IsAny<RuleApiModel>(),
                    It.IsAny<RawMessage>()),
                    Times.Exactly(2));

            mockWriter
                .Verify(x => x.WriteAsync(
                    It.Is<Document>(doc =>
                        doc.GetPropertyValue<string>(Alarms.DeviceIdKey) == matchMessage.DeviceId
                        && doc.GetPropertyValue<string>(Alarms.DocSchemaKey) == Alarms.DocSchemaValue
                        && doc.GetPropertyValue<int>(Alarms.DocSchemaVersionKey) == Alarms.DocSchemaVersionValue
                        && doc.GetPropertyValue<string>(Alarms.StatusKey) == Alarms.NewAlarmStatus
                        && doc.GetPropertyValue<string>(Alarms.DescriptionKey) == ruleResultMessage
                        && doc.GetPropertyValue<string>(Alarms.RuleSeverityKey) == ruleSeverity
                        && doc.GetPropertyValue<string>(Alarms.RuleDescriptionKey) == ruleDescription
                        && doc.GetPropertyValue<string>(Alarms.LogicKey) == Alarms.LogicValue
                        && doc.GetPropertyValue<string>(Alarms.RuleIdKey) == ruleId
                        && doc.GetPropertyValue<long>(Alarms.MessageReceivedTimeKey) == matchMessage.ReceivedTime)),
                    Times.Once);
        }
    }
}
