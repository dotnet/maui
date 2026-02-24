using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Tests for <see cref="NonStreamingResponseHandler"/> — validates TCS, completion,
/// and error handling logic without requiring the native Apple Intelligence layer.
/// </summary>
public class NonStreamingResponseHandlerTests
{
	[Fact]
	public async Task Complete_WithValidResponse_ReturnsMessages()
	{
		var handler = new NonStreamingResponseHandler();

		var nativeResponse = new ChatResponseNative(messages:
		[
			new ChatMessageNative
			{
				Role = ChatRoleNative.Assistant,
				Contents = [new TextContentNative("Hello world")]
			}
		]);

		handler.Complete(nativeResponse);

		var result = await handler.Task;
		Assert.NotNull(result);
		Assert.Single(result.Messages);
		Assert.Equal(ChatRole.Assistant, result.Messages[0].Role);
		Assert.Equal("Hello world", result.Messages[0].Text);
	}

	[Fact]
	public async Task Complete_WithNullResponse_ReturnsFallback()
	{
		var handler = new NonStreamingResponseHandler();

		handler.Complete(null);

		var result = await handler.Task;
		Assert.NotNull(result);
		Assert.Single(result.Messages);
		Assert.Equal(ChatRole.Assistant, result.Messages[0].Role);
		Assert.Equal("", result.Messages[0].Text);
	}

	[Fact]
	public async Task CompleteWithError_SurfacesException()
	{
		var handler = new NonStreamingResponseHandler();

		handler.CompleteWithError(new InvalidOperationException("test error"));

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Task);
		Assert.Equal("test error", ex.Message);
	}

	[Fact]
	public async Task CompleteCancelled_SurfacesCancellation()
	{
		var handler = new NonStreamingResponseHandler();
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		handler.CompleteCancelled(cts.Token);

		await Assert.ThrowsAsync<TaskCanceledException>(() => handler.Task);
	}

	[Fact]
	public void DoubleComplete_DoesNotThrow()
	{
		var handler = new NonStreamingResponseHandler();

		handler.Complete(null);
		handler.CompleteWithError(new InvalidOperationException("should be ignored"));
		handler.CompleteCancelled(CancellationToken.None);
		// No exception — TrySet* methods return false on subsequent calls
	}

	[Fact]
	public async Task Complete_WithMultipleMessages_ReturnsAll()
	{
		var handler = new NonStreamingResponseHandler();

		var nativeResponse = new ChatResponseNative(messages:
		[
			new ChatMessageNative
			{
				Role = ChatRoleNative.User,
				Contents = [new TextContentNative("What's the weather?")]
			},
			new ChatMessageNative
			{
				Role = ChatRoleNative.Assistant,
				Contents = [new TextContentNative("It's sunny in Boston")]
			}
		]);

		handler.Complete(nativeResponse);

		var result = await handler.Task;
		Assert.Equal(2, result.Messages.Count);
		Assert.Equal(ChatRole.User, result.Messages[0].Role);
		Assert.Equal(ChatRole.Assistant, result.Messages[1].Role);
	}
}
