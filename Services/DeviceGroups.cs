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

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services
{
    public interface IDeviceGroups
    {
        Task<IEnumerable<string>> GetDevicesAsync(string groupId);
    }

    public class DeviceGroups : IDeviceGroups
    {
        private readonly ILogger logger;
        private readonly IHttpClientWrapper httpClient;
        private readonly IDevices devices;
        private readonly string baseUrl;

        public DeviceGroups(
            ILogger logger,
            IHttpClientWrapper httpClient,
            IDevices devices,
            IServicesConfig config)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.devices = devices;
            baseUrl = $"{config.DeviceGroupsUrl}/devicegroups";
        }

        public async Task<IEnumerable<string>> GetDevicesAsync(string groupId)
        {
            try
            {
                // Accept error "NotFound" since it is possible if the rule was not updated after the group was removed
                var group = await httpClient.GetAsync<DeviceGroupApiModel>($"{baseUrl}/{groupId}", $"device group {groupId}", true);
                if (group == null)
                {
                    return new string[] { };
                }

                var list = await devices.GetListAsync(group.Conditions);
                logger.Debug($"Group {groupId} loaded, {list.Count()} devices found", () => { });
                return list;
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get list of devices", () => new { e });
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}
