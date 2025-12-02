using Microsoft.Extensions.AI;
using Xunit;

#if IOS || MACCATALYST
using PlatformChatClient = Microsoft.Maui.Essentials.AI.AppleIntelligenceChatClient;
#elif ANDROID
using PlatformChatClient = Microsoft.Maui.Essentials.AI.MLKitGenAIChatClient;
#elif WINDOWS
using PlatformChatClient = Microsoft.Maui.Essentials.AI.WindowsAIChatClient;
#endif

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public partial class ChatClientGetServiceTests
{
	[Fact]
	public void GetService_ReturnsMetadataWithCorrectProviderAndModel()
	{
		var client = new PlatformChatClient();
		var metadata = client.GetService<ChatClientMetadata>();

		Assert.NotNull(metadata);

#if IOS || MACCATALYST
		Assert.Equal("apple", metadata.ProviderName);
		Assert.Equal("apple-intelligence", metadata.DefaultModelId);
#elif ANDROID
		Assert.Equal("ml-kit", metadata.ProviderName);
		Assert.Equal("gemini-nano", metadata.DefaultModelId);
#elif WINDOWS
		Assert.Equal("windows-ai", metadata.ProviderName);
		Assert.Equal("phi-silica", metadata.DefaultModelId);
#endif
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
