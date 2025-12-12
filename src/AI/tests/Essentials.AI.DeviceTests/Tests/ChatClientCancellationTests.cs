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

public class ChatClientCancellationTests
{
	[Fact]
	public async Task GetResponseAsync_AcceptsCancellationToken()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();

		await client.GetResponseAsync(messages, cancellationToken: cts.Token);
	}

	[Fact]
	public async Task GetResponseAsync_WithCanceledToken_ThrowsOrCompletesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();
		cts.Cancel();

		try
		{
			await client.GetResponseAsync(messages, cancellationToken: cts.Token);
		}
		catch (OperationCanceledException)
		{
			// Expected behavior
		}
	}

	[Fact]
	public async Task GetResponseAsync_CancelAfterStart_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Tell me a long story")
		};

		using var cts = new CancellationTokenSource();
		
		var task = client.GetResponseAsync(messages, cancellationToken: cts.Token);
		
		await Task.Delay(50);
		cts.Cancel();

		try
		{
			await task;
		}
		catch (OperationCanceledException)
		{
			// Expected behavior
		}
	}

	[Fact]
	public async Task GetResponseAsync_WithTimeout_CompletesOrThrows()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

		try
		{
			await client.GetResponseAsync(messages, cancellationToken: cts.Token);
		}
		catch (OperationCanceledException)
		{
			// Expected if operation times out
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_AcceptsCancellationToken()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();

		await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
		{
			// Process updates
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithCanceledToken_ThrowsOrCompletesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();
		cts.Cancel();

		try
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
			{
				// Should not reach here
			}
		}
		catch (OperationCanceledException)
		{
			// Expected behavior
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_CancelDuringStreaming_HandlesGracefully()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Count to 100")
		};

		using var cts = new CancellationTokenSource();
		
		var updateCount = 0;
		try
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
			{
				updateCount++;
				if (updateCount >= 2)
				{
					cts.Cancel();
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected behavior
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithTimeout_CompletesOrThrows()
	{
		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Count to 100")
		};

		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

		try
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
			{
				// Process updates until timeout
			}
		}
		catch (OperationCanceledException)
		{
			// Expected if operation times out
		}
	}
}
