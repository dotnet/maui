using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient options tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientOptionsTestsBase<T>
	where T : class, IChatClient, new()
{
	[Fact]
	public async Task GetResponseAsync_AcceptsNullOptions()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		await client.GetResponseAsync(messages, options: null);
	}

	[Fact]
	public async Task GetResponseAsync_WithChatOptions_AcceptsValidOptions()
	{
		var client = new T();
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
	public virtual async Task GetResponseAsync_WithResponseFormat_AcceptsJsonFormat()
	{
		var client = new T();
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
	public async Task GetResponseAsync_WithExtremeTemperature_HandlesGracefully()
	{
		var client = new T();
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
	public async Task GetResponseAsync_WithZeroMaxTokens_ThrowsException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			MaxOutputTokens = 0
		};

		await Assert.ThrowsAnyAsync<Exception>(() => client.GetResponseAsync(messages, options));
	}

	[Fact]
	public async Task GetStreamingResponseAsync_AcceptsNullOptions()
	{
		var client = new T();
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
	public async Task GetStreamingResponseAsync_WithChatOptions_AcceptsValidOptions()
	{
		var client = new T();
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
	public virtual async Task GetStreamingResponseAsync_WithResponseFormat_AcceptsJsonFormat()
	{
		var client = new T();
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
