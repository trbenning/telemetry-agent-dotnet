// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public interface IRules
    {
        Task<IEnumerable<RuleApiModel>> GetAllAsync();
    }

    public class Rules : IRules
    {
        private readonly IHttpClientWrapper httpClient;
        private readonly string uri;

        public Rules(
            IHttpClientWrapper httpClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            this.uri = $"{config.MonitoringRulesUrl}/rules";
        }

        public async Task<IEnumerable<RuleApiModel>> GetAllAsync()
        {
            var list = await httpClient.GetAsync<RuleListApiModel>(uri, "monitoring rules");
            return list.Items;
        }
    }
}
