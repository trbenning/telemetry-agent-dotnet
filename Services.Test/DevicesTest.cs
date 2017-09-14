// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class DevicesTest
    {
        private readonly Random rand = new Random();
        private readonly ILogger logger = new Logger("UnitTest", LogLevel.Debug);
        private readonly Mock<IHttpClientWrapper> mockHttpClient = new Mock<IHttpClientWrapper>();
        private readonly ServicesConfig config = new ServicesConfig
        {
            DevicesUrl = "http://devicesService"
        };

        private readonly Devices devices;

        public DevicesTest()
        {
            devices = new Devices(
                logger,
                mockHttpClient.Object,
                config);
        }

        [Fact]
        public async Task GetListAsyncTest()
        {
            var conditions = Enumerable.Range(0, rand.Next(3, 10)).Select(i => new DeviceGroupConditionApiModel
            {
                Key = rand.NextString(),
                Operator = rand.NextString(),
                Value = rand.NextString()
            }).ToList();
            var query = JsonConvert.SerializeObject(conditions);

            var deviceList = Enumerable.Range(0, rand.Next(3, 10)).Select(i => new DeviceApiModel
            {
                Id = rand.NextString()
            }).ToList();
            var model = new DeviceListApiModel
            {
                Items = deviceList
            };

            mockHttpClient
                .Setup(x => x.GetAsync<DeviceListApiModel>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(model);

            var result = await devices.GetListAsync(conditions);
            Assert.True(result.OrderBy(id => id).SequenceEqual(deviceList.Select(d => d.Id).OrderBy(id => id)));

            mockHttpClient
                .Verify(x => x.GetAsync<DeviceListApiModel>(
                    It.Is<string>(s => s == $"{config.DevicesUrl}/devices?query={query}"),
                    It.IsAny<string>(),
                    It.Is<bool>(b => !b)),
                    Times.Once);
        }
    }
}
