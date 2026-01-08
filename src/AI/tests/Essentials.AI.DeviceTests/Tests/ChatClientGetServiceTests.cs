#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using Xunit;

using PlatformChatClient = Microsoft.Maui.Essentials.AI.AppleIntelligenceChatClient;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ChatClientGetServiceTests
{
	[Fact]
	public void GetService_ReturnsMetadataWithCorrectProviderAndModel()
	{
		var client = new PlatformChatClient();
		var metadata = client.GetService<ChatClientMetadata>();

		Assert.NotNull(metadata);

		Assert.Equal("apple", metadata.ProviderName);
		Assert.Equal("apple-intelligence", metadata.DefaultModelId);
	}

	[Fact]
	public void GetService_ReturnsNullForUnknownService()
	{
		IChatClient client = new PlatformChatClient();
		var unknownService = client.GetService(typeof(string));
		Assert.Null(unknownService);
	}

	[Fact]
	public void GetService_ReturnsNullForKeyedServices()
	{
		IChatClient client = new PlatformChatClient();
		var keyedService = client.GetService(typeof(ChatClientMetadata), "some-key");
		Assert.Null(keyedService);
	}

	[Fact]
	public void GetService_ReturnsItselfForMatchingType()
	{
		IChatClient client = new PlatformChatClient();
		var self = client.GetService(typeof(PlatformChatClient));

		Assert.NotNull(self);
		Assert.Same(client, self);
	}

	[Fact]
	public void GetService_ReturnsItselfForIChatClientType()
	{
		IChatClient client = new PlatformChatClient();
		var self = client.GetService(typeof(IChatClient));
		Assert.NotNull(self);
		Assert.Same(client, self);
	}
}
#endif
