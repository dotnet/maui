using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient streaming tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientStreamingTestsBase<T>
	where T : class, IChatClient, new()
{
	[Fact]
	public async Task GetStreamingResponseAsync_ReturnsStreamingUpdates()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		bool receivedUpdate = false;
		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			receivedUpdate = true;
			Assert.NotNull(update);
		}

		Assert.True(receivedUpdate, "Should receive at least one streaming update");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_UpdatesHaveContents()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Tell me a short story")
		};

		var updates = new List<ChatResponseUpdate>();
		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			updates.Add(update);
		}

		Assert.True(updates.Count > 0, "Should receive streaming updates");
		Assert.True(updates.Any(u => u.Contents.Count > 0), "At least one update should have contents");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_CanBuildCompleteResponseFromUpdates()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Say hello")
		};

		var textBuilder = new System.Text.StringBuilder();
		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			foreach (var content in update.Contents)
			{
				if (content is TextContent textContent)
				{
					textBuilder.Append(textContent.Text);
				}
			}
		}

		var completeText = textBuilder.ToString();
		Assert.False(string.IsNullOrEmpty(completeText), "Should build complete response from streaming updates");
	}
}
