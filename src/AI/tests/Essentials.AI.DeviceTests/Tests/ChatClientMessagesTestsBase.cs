using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient message handling tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientMessagesTestsBase<T>
	where T : class, IChatClient, new()
{
	[Fact]
	public async Task GetResponseAsync_WithEmptyMessages_ThrowsException()
	{
		var client = new T();
		var messages = new List<ChatMessage>();

		await Assert.ThrowsAsync<ArgumentException>(() => client.GetResponseAsync(messages));
	}

	[Fact]
	public async Task GetResponseAsync_WithSystemMessage_AcceptsSystemRole()
	{
		var client = new T();
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
		var client = new T();
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
		var client = new T();
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
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "")
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetResponseAsync_WithSpecialCharacters_HandlesGracefully()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Test with special chars: 你好 🌍 émojis & symbols!")
		};

		await client.GetResponseAsync(messages);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithEmptyMessages_ThrowsException()
	{
		var client = new T();
		var messages = new List<ChatMessage>();

		await Assert.ThrowsAnyAsync<Exception>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages))
			{
				// Process updates
			}
		});
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithSystemMessage_AcceptsSystemRole()
	{
		var client = new T();
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
		var client = new T();
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
		var client = new T();
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
		var client = new T();
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
		var client = new T();
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
