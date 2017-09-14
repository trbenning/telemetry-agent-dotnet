// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class DeviceGroupsTest
    {
        private readonly Random rand = new Random();
        private readonly ILogger logger = new Logger("UnitTest", LogLevel.Debug);
        private readonly Mock<IHttpClientWrapper> mockHttpClient = new Mock<IHttpClientWrapper>();
        private readonly Mock<IDevices> mockDevices = new Mock<IDevices>();

        private readonly ServicesConfig config = new ServicesConfig
        {
            DeviceGroupsUrl = "http://deviceGroupService"
        };

        private readonly DeviceGroups deviceGroups;

        public DeviceGroupsTest()
        {
            deviceGroups = new DeviceGroups(
                logger,
                mockHttpClient.Object,
                mockDevices.Object,
                config);
        }

        [Fact]
        public async Task NonexistGroupTest()
        {
            var groupId = rand.NextString();

            mockHttpClient
                .Setup(x => x.GetAsync<DeviceGroupApiModel>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((DeviceGroupApiModel)null);

            var result = await deviceGroups.GetDevicesAsync(groupId);
            Assert.False(result.Any());

            mockHttpClient
                .Verify(x => x.GetAsync<DeviceGroupApiModel>(
                    It.Is<string>(s => s == $"{config.DeviceGroupsUrl}/devicegroups/{groupId}"),
                    It.IsAny<string>(),
                    It.Is<bool>(b => b)),
                    Times.Once);
        }

        [Fact]
        public async Task GetDevicesAsyncTest()
        {
            var groupId = rand.NextString();
            var conditions = new DeviceGroupConditionApiModel[] { };
            var group = new DeviceGroupApiModel
            {
                Conditions = conditions
            };
            var deviceIds = new string[] { };

            mockHttpClient
                .Setup(x => x.GetAsync<DeviceGroupApiModel>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(group);

            mockDevices
                .Setup(x => x.GetListAsync(It.IsAny<IEnumerable<DeviceGroupConditionApiModel>>()))
                .ReturnsAsync(deviceIds);

            var result = await deviceGroups.GetDevicesAsync(groupId);
            Assert.Equal(result, deviceIds);

            mockHttpClient
                .Verify(x => x.GetAsync<DeviceGroupApiModel>(
                    It.Is<string>(s => s == $"{config.DeviceGroupsUrl}/devicegroups/{groupId}"),
                    It.IsAny<string>(),
                    It.Is<bool>(b => b)),
                    Times.Once);

            mockDevices
                .Verify(x => x.GetListAsync(
                    It.Is<IEnumerable<DeviceGroupConditionApiModel>>(c => c == conditions)),
                    Times.Once);
        }
    }
}
