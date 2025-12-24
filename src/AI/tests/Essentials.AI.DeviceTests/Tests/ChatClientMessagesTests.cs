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

public class ChatClientMessagesTests
{
	[Fact]
	public async Task GetResponseAsync_WithEmptyMessages_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>();

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetResponseAsync_WithSystemMessage_AcceptsSystemRole()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, "You are a helpful assistant"),
			new(ChatRole.User, "Hello")
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetResponseAsync_WithConversationHistory_AcceptsMultipleMessages()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's 2 + 2?"),
			new(ChatRole.Assistant, "4"),
			new(ChatRole.User, "And times 3?")
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetResponseAsync_WithLongMessage_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var longText = string.Join(" ", Enumerable.Repeat("This is a test message.", 100));
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, longText)
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetResponseAsync_WithEmptyMessageContent_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "")
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetResponseAsync_WithSpecialCharacters_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Test with special chars: 你好 🌍 émojis & symbols!")
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithEmptyMessages_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>();

		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithSystemMessage_AcceptsSystemRole()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, "You are a helpful assistant"),
			new(ChatRole.User, "Hello")
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithConversationHistory_AcceptsMultipleMessages()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's 2 + 2?"),
			new(ChatRole.Assistant, "4"),
			new(ChatRole.User, "And times 3?")
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithLongMessage_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var longText = string.Join(" ", Enumerable.Repeat("This is a test message.", 100));
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, longText)
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithEmptyMessageContent_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "")
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithSpecialCharacters_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Test with special chars: 你好 🌍 émojis & symbols!")
		};

		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			// Process updates
		}
	}
}
