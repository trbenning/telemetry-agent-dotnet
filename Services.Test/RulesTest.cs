// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Models;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;
using Moq;
using Xunit;

namespace Services.Test
{
    public class RulesTest
    {
        private readonly Mock<IHttpClientWrapper> mockHttpClient = new Mock<IHttpClientWrapper>();
        private readonly ServicesConfig config = new ServicesConfig
        {
            MonitoringRulesUrl = "http://ruleService"
        };

        private readonly Rules rules;

        public RulesTest()
        {
            rules = new Rules(
                mockHttpClient.Object,
                config);
        }

        [Fact]
        public async Task GetAllAsyncTest()
        {
            var ruleList = new RuleApiModel[] { };
            var model = new RuleListApiModel
            {
                Items = ruleList
            };

            mockHttpClient
                .Setup(x => x.GetAsync<RuleListApiModel>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(model);

            var result = await rules.GetAllAsync();
            Assert.Equal(result, ruleList);

            mockHttpClient
                .Verify(x => x.GetAsync<RuleListApiModel>(
                    It.Is<string>(s => s == $"{config.MonitoringRulesUrl}/rules"),
                    It.IsAny<string>(),
                    It.Is<bool>(b => !b)),
                    Times.Once);
        }
    }
}
