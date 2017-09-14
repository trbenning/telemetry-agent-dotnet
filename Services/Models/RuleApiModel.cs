// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class RuleApiModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("GroupId")]
        public string GroupId { get; set; }

        [JsonProperty("Severity")]
        public string Severity { get; set; }

        [JsonProperty("Conditions")]
        public IEnumerable<ConditionApiModel> Conditions { get; set; }
    }
}
