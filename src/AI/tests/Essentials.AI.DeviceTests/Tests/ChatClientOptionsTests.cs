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

public class ChatClientOptionsTests
{
	[Fact]
	public async Task GetResponseAsync_AcceptsNullOptions()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		await client.GetResponseAsync(messages, options: null);
	}

	[Fact]
	public async Task GetResponseAsync_WithChatOptionsAcceptsValidOptions()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			Temperature = 0.7f,
			MaxOutputTokens = 100,
			TopK = 10,
			Seed = 42
		};

		await client.GetResponseAsync(messages, options);
	}

	[Fact]
	public async Task GetResponseAsync_WithResponseFormatAcceptsJsonFormat()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		await client.GetResponseAsync(messages, options);
	}

	[Fact]
	public async Task GetResponseAsync_WithExtremeTemperatureHandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		var options = new ChatOptions
		{
			Temperature = 2.0f
		};

		await client.GetResponseAsync(messages, options);
	}

	[Fact]
	public async Task GetResponseAsync_WithZeroMaxTokensHandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			MaxOutputTokens = 0
		};

		await client.GetResponseAsync(messages, options);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_AcceptsNullOptions()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages, options: null))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithChatOptionsAcceptsValidOptions()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			Temperature = 0.7f,
			MaxOutputTokens = 100,
			TopK = 10,
			Seed = 42
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithResponseFormatAcceptsJsonFormat()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			// Process updates
		}
	}
}
