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

public class ChatClientStreamingTests
{
	[Fact]
	public async Task GetStreamingResponseAsync_ReturnsStreamingUpdates()
	{
		var client = new PlatformChatClient();
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
		var client = new PlatformChatClient();
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
		var client = new PlatformChatClient();
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
