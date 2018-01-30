// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.StreamingAgent.Runtime;
using Moq;
using Xunit;

namespace StreamingAgent.Test
{
	public class ConfigTest
	{
		public ConfigTest()
		{
		}

		[Fact]
		public async Task HubEndpointWithEndpointPrefix()
		{
			// Arrange
			Mock<IConfigData> configData = new Mock<IConfigData>();
			configData.Setup(x => x.GetString("telemetryagent:iothub:connection:hubName")).Returns("iothub123");
			configData.Setup(x => x.GetString("telemetryagent:iothub:checkpointing:storage:namespace")).Returns("iothub123");
			configData.Setup(x => x.GetString("telemetryagent:iothub:checkpointing:storage:backendType")).Returns("AzureBlob");
			configData.Setup(x => x.GetString("telemetryagent:iothub:connection:hubEndpoint"))
				.Returns("Endpoint=sb://iothub-ns-iothub-123-1232-123.servicebus.windows.net/;SharedAccessKeyName=iothubowner;");

			// Act
			Config config = new Config(configData.Object);

			// Assert
			Assert.Equal(config.IoTHubConfig.ConnectionConfig.HubEndpoint, "sb://iothub-ns-iothub-123-1232-123.servicebus.windows.net/");
		}

		[Fact]
		public async Task HubEndpointWithoutEndpointPrefix()
		{
			// Arrange
			var hubEndpoint = "sb://iothub-ns-iothub-123-1232-123.servicebus.windows.net/";
			Mock<IConfigData> configData = new Mock<IConfigData>();
			configData.Setup(x => x.GetString("telemetryagent:iothub:connection:hubName")).Returns("iothub123");
			configData.Setup(x => x.GetString("telemetryagent:iothub:checkpointing:storage:namespace")).Returns("iothub123");
			configData.Setup(x => x.GetString("telemetryagent:iothub:checkpointing:storage:backendType")).Returns("AzureBlob");
			configData.Setup(x => x.GetString("telemetryagent:iothub:connection:hubEndpoint"))
				.Returns(hubEndpoint);

			// Act
			Config config = new Config(configData.Object);

			// Assert
			Assert.Equal(config.IoTHubConfig.ConnectionConfig.HubEndpoint, hubEndpoint);
		}
	}
}
