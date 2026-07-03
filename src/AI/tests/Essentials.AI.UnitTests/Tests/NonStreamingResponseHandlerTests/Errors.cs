using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class NonStreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for error and cancellation paths.
	/// </summary>
	public class ErrorTests
	{
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
		public async Task DoubleComplete_PreservesFirstResult()
		{
			var handler = new NonStreamingResponseHandler();
			var response = new Microsoft.Extensions.AI.ChatResponse(
				[new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, "first")]);

			handler.Complete(response);
			handler.CompleteWithError(new InvalidOperationException("should be ignored"));
			handler.CompleteCancelled(CancellationToken.None);

			var result = await handler.Task;
			Assert.Same(response, result);
		}
	}
}
