// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models
{
    public class DeviceListApiModel
    {
        public IEnumerable<DeviceApiModel> Items { get; set; }

        public string ContinuationToken { get; set; }
    }
}
