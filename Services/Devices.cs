// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public interface IDevices
    {
        Task<IEnumerable<string>> GetListAsync(IEnumerable<DeviceGroupConditionApiModel> conditions);
    }

    public class Devices : IDevices
    {
        private readonly ILogger logger;
        private readonly IHttpClientWrapper httpClient;
        private readonly string baseUrl;

        public Devices(
            ILogger logger,
            IHttpClientWrapper httpClient,
            IServicesConfig config)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.baseUrl = $"{config.DevicesUrl}/devices";
        }

        public async Task<IEnumerable<string>> GetListAsync(IEnumerable<DeviceGroupConditionApiModel> conditions)
        {
            try
            {
                var query = JsonConvert.SerializeObject(conditions);

                var deviceList = await httpClient.GetAsync<DeviceListApiModel>($"{baseUrl}?query={query}", $"devices by query {query}");
                return deviceList.Items.Select(d => d.Id);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get list of devices", () => new { e });
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}
