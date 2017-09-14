// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class RulesEvaluationTest
    {
        private readonly Random rand = new Random();
        private readonly ILogger logger = new Logger("UnitTest", LogLevel.Debug);
        private readonly Mock<IDeviceGroups> mockDeviceGroups = new Mock<IDeviceGroups>();

        [Fact]
        public void UnmatchDeviceGroupTest()
        {
            var evaluation = new RulesEvaluation(
                logger,
                mockDeviceGroups.Object);

            mockDeviceGroups
                .Setup(x => x.GetDevicesAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(new string[] { });

            var rule = new RuleApiModel
            {
                GroupId = rand.NextString()
            };

            var message = rand.NextRawMessage();

            var result = evaluation.Evaluate(rule, message);

            Assert.False(result.Match);
        }

        [Fact]
        public void UnmatchRuleTest()
        {
            var evaluation = new RulesEvaluation(
                logger,
                mockDeviceGroups.Object);

            // Repeat times for broader coverage
            foreach (var unused in Enumerable.Range(0, 10))
            {
                var totalConditions = rand.Next(3, 10);
                var failedConditions = rand.Next(1, totalConditions);

                var message = rand.NextRawMessage();

                var rule = new RuleApiModel
                {
                    GroupId = rand.NextString(),
                    Conditions = Enumerable.Range(0, totalConditions).Select(i => GenerateCondition(message, i != failedConditions)).ToList()
                };

                mockDeviceGroups
                    .Setup(x => x.GetDevicesAsync(
                        It.IsAny<string>()))
                    .ReturnsAsync(new[] { message.DeviceId });

                var result = evaluation.Evaluate(rule, message);

                Assert.False(result.Match);
            }
        }

        [Fact]
        public void EvaluateTest()
        {
            var evaluation = new RulesEvaluation(
                logger,
                mockDeviceGroups.Object);

            // Repeat times for broader coverage
            foreach (var unused in Enumerable.Range(0, 10))
            {
                var totalConditions = rand.Next(3, 10);

                var message = rand.NextRawMessage();

                var rule = new RuleApiModel
                {
                    GroupId = rand.NextString(),
                    Conditions = Enumerable.Range(0, totalConditions).Select(i => GenerateCondition(message, true)).ToList()
                };

                mockDeviceGroups
                    .Setup(x => x.GetDevicesAsync(
                        It.IsAny<string>()))
                    .ReturnsAsync(new[] { message.DeviceId });

                var result = evaluation.Evaluate(rule, message);

                mockDeviceGroups
                    .Verify(x => x.GetDevicesAsync(
                            It.Is<string>(s => s == rule.GroupId)),
                        Times.Once);

                Assert.True(result.Match);
            }
        }

        private ConditionApiModel GenerateCondition(RawMessage message, bool match)
        {
            while (true)
            {
                var field = rand.NextString(message.PayloadData.Keys);
                var value = message.PayloadData[field];

                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return GenerateCondition(field, Convert.ToDouble(value), match);

                    case TypeCode.String:
                        return GenerateCondition(field, value.ToString(), match);

                    default:
                        // Unknown value type, try other fields
                        break;
                }
            }
        }

        private ConditionApiModel GenerateCondition(string field, double value, bool match)
        {
            var condition = new ConditionApiModel()
            {
                Field = field,
                Operator = rand.NextString(RulesEvaluation.OperatorLogicLookup.Keys)
            };

            switch (condition.Operator)
            {
                case ">":
                case "greaterthan":
                case ">=":
                case "greaterthanorequal":
                    condition.Value = match ? $"{value - 1}" : $"{value + 1}";
                    break;

                case "<":
                case "lessthan":
                case "<=":
                case "lessthanorequal":
                    condition.Value = match ? $"{value + 1}" : $"{value - 1}";
                    break;

                case "=":
                case "==":
                case "equal":
                case "equals":
                    condition.Value = match ? $"{value}" : $"{value - 1}";
                    break;

                case "!=":
                case "<>":
                case "notequal":
                case "notequals":
                    condition.Value = match ? $"{value - 1}" : $"{value}";
                    break;
            }

            return condition;
        }

        private ConditionApiModel GenerateCondition(string field, string value, bool match)
        {
            var condition = new ConditionApiModel
            {
                Field = field,
                Operator = rand.NextString(RulesEvaluation.OperatorLogicLookup.Keys)
            };

            switch (condition.Operator)
            {
                case ">":
                case "greaterthan":
                case ">=":
                case "greaterthanorequal":
                    condition.Value = match ? $"{value.First()}" : $"{value}a";
                    break;

                case "<":
                case "lessthan":
                case "<=":
                case "lessthanorequal":
                    condition.Value = match ? $"{value}a" : $"{value.First()}";
                    break;

                case "=":
                case "==":
                case "equal":
                case "equals":
                    condition.Value = match ? value : $"{value}a";
                    break;

                case "!=":
                case "<>":
                case "notequal":
                case "notequals":
                    condition.Value = match ? $"{value}a" : value;
                    break;
            }

            return condition;
        }
    }
}
