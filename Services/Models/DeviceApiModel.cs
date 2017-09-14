// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class DeviceApiModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
    }
}
