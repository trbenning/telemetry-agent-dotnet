// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public class RulesEvaluationResult
    {
        public bool Match { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }

    public interface IRulesEvaluation
    {
        RulesEvaluationResult Evaluate(RuleApiModel rule, RawMessage message);
    }

    public class RulesEvaluation : IRulesEvaluation
    {
        private class DeviceGroupCacheItem
        {
            public HashSet<string> DeviceIds { get; set; }
            public DateTime Expiration { get; set; }
        }

        private readonly ILogger logger;

        // For each group, reload the device IDs once every 5 minutes
        private const int GroupsCacheTtlSeconds = 300;

        // Groups cache: <Group ID, <list of devices, cache expiration>>
        private readonly Dictionary<string, DeviceGroupCacheItem> deviceGroupsCache = new Dictionary<string, DeviceGroupCacheItem>();

        private readonly IDeviceGroups deviceGroups;

        public class OperatorLogic
        {
            public string[] Operator { set; get; }
            public Func<string, string, bool> StringTestFunc { set; get; }
            public Func<double, double, bool> DoubleTestFunc { set; get; }
            public string Message { set; get; }
        }

        // Important! keep the literal values lowercase
        public static OperatorLogic[] OperatorLogics => new[]
        {
            new OperatorLogic
            {
                Operator = new[] { "greaterthan", ">" },
                StringTestFunc = (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) > 0,
                DoubleTestFunc = (v1, v2) => v1 > v2,
                Message = "`{0}` value `{1}` is greater than `{2}`"
            },
            new OperatorLogic
            {
                Operator = new[] { "greaterthanorequal", ">=" },
                StringTestFunc = (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) >= 0,
                DoubleTestFunc = (v1, v2) => v1 >= v2,
                Message = "`{0}` value `{1}` is greater than or equal to `{2}`"
            },
            new OperatorLogic
            {
                Operator = new[] { "lessthan", "<" },
                StringTestFunc = (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) < 0,
                DoubleTestFunc = (v1, v2) => v1 < v2,
                Message = "`{0}` value `{1}` is less than `{2}`"
            },
            new OperatorLogic
            {
                Operator = new[] { "lessthanorequal", "<=" },
                StringTestFunc = (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) <= 0,
                DoubleTestFunc = (v1, v2) => v1 <= v2,
                Message = "`{0}` value `{1}` is less than or equal to `{2}`"
            },
            new OperatorLogic
            {
                Operator = new[] { "equals", "equal", "=", "==" },
                StringTestFunc = (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) == 0,
                DoubleTestFunc = (v1, v2) => v1 == v2,
                Message = "`{0}` value `{1}` is equal to `{2}`"
            },
            new OperatorLogic
            {
                Operator = new[] { "notequal", "notequals", "!=", "<>" },
                StringTestFunc = (s1, s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) != 0,
                DoubleTestFunc = (v1, v2) => v1 != v2,
                Message = "`{0}` value `{1}` is not equal to `{2}`"
            }
        };

        /// <summary>
        /// Lookup table to improve performance
        /// </summary>
        public static Dictionary<string, OperatorLogic> OperatorLogicLookup { get; }

        static RulesEvaluation()
        {
            OperatorLogicLookup = new Dictionary<string, OperatorLogic>();

            foreach (var logic in OperatorLogics)
            {
                foreach (var @operator in logic.Operator)
                {
                    OperatorLogicLookup.Add(@operator, logic);
                }
            }
        }

        public RulesEvaluation(
            ILogger logger,
            IDeviceGroups deviceGroups)
        {
            this.logger = logger;
            this.deviceGroups = deviceGroups;
        }

        public RulesEvaluationResult Evaluate(RuleApiModel rule, RawMessage message)
        {
            var result = new RulesEvaluationResult();

            if (GroupContainsDevice(message.DeviceId, rule.GroupId))
            {
                logger.Debug($"Evaluating rule {rule.Description} for device {message.DeviceId} with {rule.Conditions.Count()} conditions", () => { });

                var descriptions = new List<string>();
                foreach (var c in rule.Conditions)
                {
                    var eval = EvaluateCondition(c, message);
                    // perf: all conditions must match, break as soon as one doesn't
                    if (!eval.Match)
                    {
                        return result;
                    }
                    descriptions.Add(eval.Message);
                }

                result.Match = true;
                result.Message = string.Join("; ", descriptions);
            }
            else
            {
                logger.Debug($"Skipping rule {rule.Description} because device {message.DeviceId} doesn't belong to group {rule.GroupId}", () => { });
            }

            return result;
        }

        private bool GroupContainsDevice(string deviceId, string groupId)
        {
            lock (deviceGroupsCache)
            {
                // Check if the cache is expired
                if (deviceGroupsCache.ContainsKey(groupId))
                {
                    if (deviceGroupsCache[groupId].Expiration < DateTime.UtcNow)
                    {
                        logger.Debug($"Cache for group {groupId} expired", () => { });
                        deviceGroupsCache.Remove(groupId);
                    }
                }

                // If the group information is in not available, retrieve and cache
                if (!deviceGroupsCache.ContainsKey(groupId))
                {
                    logger.Debug($"Preparing cache for group {groupId}", () => { });

                    var deviceIds = deviceGroups.GetDevicesAsync(groupId).Result;
                    deviceGroupsCache.Add(
                        groupId,
                        new DeviceGroupCacheItem
                        {
                            DeviceIds = new HashSet<string>(deviceIds),
                            Expiration = DateTime.UtcNow + TimeSpan.FromSeconds(GroupsCacheTtlSeconds)
                        });
                }
            }

            // Check if the group contains the device ID
            return deviceGroupsCache[groupId].DeviceIds.Contains(deviceId);
        }

        private RulesEvaluationResult EvaluateCondition(
            ConditionApiModel condition,
            RawMessage message)
        {
            var r = new RulesEvaluationResult();

            var field = condition.Field;
            object value;
            if (!message.PayloadData.TryGetValue(field, out value))
            {
                logger.Debug($"Message payload doesn't contain field {field}", () => { });
                return r;
            }

            OperatorLogic logic;
            if (!OperatorLogicLookup.TryGetValue(condition.Operator.ToLowerInvariant(), out logic))
            {
                logger.Error("Unknown operator", () => new { condition });
                return r;
            }

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
                    {
                        var actualValue = Convert.ToDouble(value);
                        var threshold = Convert.ToDouble(condition.Value);

                        logger.Debug($"Field {field}, Value {actualValue}, Operator {condition.Operator}, Threshold {threshold}", () => { });
                        if (logic.DoubleTestFunc(actualValue, threshold))
                        {
                            r.Match = true;
                            r.Message = string.Format(logic.Message, field, actualValue, threshold);
                        }
                    }
                    break;

                case TypeCode.String:
                    {
                        var actualValue = value.ToString();
                        var threshold = condition.Value;

                        logger.Debug($"Field {field}, Value '{actualValue}', Operator {condition.Operator}, Threshold '{threshold}'", () => { });
                        if (logic.StringTestFunc(actualValue, threshold))
                        {
                            r.Match = true;
                            r.Message = string.Format(logic.Message, field, actualValue, threshold);
                        }
                    }
                    break;

                default:
                    logger.Error($"Unknown type for `{field}` value sent by device {message.DeviceId}", () => { });
                    break;
            }

            return r;
        }
    }
}
