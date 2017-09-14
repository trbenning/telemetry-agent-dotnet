// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class ConditionApiModel
    {
        [JsonProperty("Field")]
        public string Field { get; set; }

        [JsonProperty("Operator")]
        public string Operator { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
