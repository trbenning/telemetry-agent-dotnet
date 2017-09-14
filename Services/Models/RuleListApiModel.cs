// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class RuleListApiModel
    {
        [JsonProperty("Items")]
        public IEnumerable<RuleApiModel> Items { get; set; }
    }
}
