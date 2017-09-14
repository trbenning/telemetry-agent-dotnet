// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class Alarm
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public long MessageReceivedTime { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public string RuleId { get; set; }
        public string RuleServerity { get; set; }
        public string RuleDescription { get; set; }
    }
}
