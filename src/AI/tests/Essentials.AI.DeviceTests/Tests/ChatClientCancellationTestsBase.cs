using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient cancellation tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientCancellationTestsBase<T>
	where T : class, IChatClient, new()
{
	[Fact]
	public async Task GetResponseAsync_AcceptsCancellationToken()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();

		await client.GetResponseAsync(messages, cancellationToken: cts.Token);
	}

	[Fact]
	public async Task GetResponseAsync_WithCanceledToken_ThrowsOperationCanceledException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetResponseAsync(messages, cancellationToken: cts.Token));
	}

	[Fact]
	public async Task GetResponseAsync_CancelAfterStart_ThrowsOperationCanceledException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Tell me a long story")
		};

		using var cts = new CancellationTokenSource();

		var task = client.GetResponseAsync(messages, cancellationToken: cts.Token);

		await Task.Delay(50);
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
	}

	[Fact]
	public async Task GetResponseAsync_WithTimeout_ThrowsOperationCanceledException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetResponseAsync(messages, cancellationToken: cts.Token));
	}

	[Fact]
	public async Task GetStreamingResponseAsync_AcceptsCancellationToken()
	{
		var client = new T();
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
	public async Task GetStreamingResponseAsync_WithCanceledToken_ThrowsOperationCanceledException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};

		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
			{
				// Should not reach here
			}
		});
	}

	[Fact]
	public async Task GetStreamingResponseAsync_CancelDuringStreaming_ThrowsOperationCanceledException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Count to 100")
		};

		using var cts = new CancellationTokenSource();

		var updateCount = 0;
		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
			{
				updateCount++;
				if (updateCount >= 2)
				{
					cts.Cancel();
				}
			}
		});
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithTimeout_ThrowsOperationCanceledException()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Count to 100")
		};

		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, cancellationToken: cts.Token))
			{
				// Process updates until timeout
			}
		});
	}
}
