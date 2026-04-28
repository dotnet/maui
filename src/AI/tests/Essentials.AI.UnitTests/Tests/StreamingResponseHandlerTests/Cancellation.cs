using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for cancellation scenarios during streaming reads.
	/// </summary>
	public class CancellationTests
	{
		[Fact]
		public async Task ReadAllAsync_WithPreCancelledToken_ThrowsOperationCanceled()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());
			handler.ProcessContent("Hello");
			handler.Complete();

			using var cts = new CancellationTokenSource();
			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
			{
				await foreach (var _ in handler.ReadAllAsync(cts.Token))
				{
				}
			});
		}

		[Fact]
		public async Task ReadAllAsync_CancelledDuringRead_ThrowsOperationCanceled()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			// Write one update but don't complete — the reader will block waiting for more
			handler.ProcessContent("Hello");

			using var cts = new CancellationTokenSource();
			var readTask = Task.Run(async () =>
			{
				var updates = new List<Microsoft.Extensions.AI.ChatResponseUpdate>();
				await foreach (var update in handler.ReadAllAsync(cts.Token))
				{
					updates.Add(update);
				}
				return updates;
			});

			// Give the reader time to start consuming, then cancel
			await Task.Delay(50);
			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => readTask);
		}

		[Fact]
		public async Task ReadAllAsync_CancelledAfterToolCall_ThrowsOperationCanceled()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessContent("Checking weather...");
			handler.ProcessToolCall("call-1", "GetWeather", "{\"location\":\"Boston\"}");
			// Don't send tool result or complete — simulates cancellation during tool execution

			using var cts = new CancellationTokenSource();
			var readTask = Task.Run(async () =>
			{
				var updates = new List<Microsoft.Extensions.AI.ChatResponseUpdate>();
				await foreach (var update in handler.ReadAllAsync(cts.Token))
				{
					updates.Add(update);
				}
				return updates;
			});

			// Let reader consume the queued items, then cancel while waiting for more
			await Task.Delay(50);
			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => readTask);
		}

		[Fact]
		public async Task ReadAllAsync_WithTimeout_ThrowsWhenNeverCompleted()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());
			// Handler is never completed — simulates a hung tool or stalled response

			using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

			await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
			{
				await foreach (var _ in handler.ReadAllAsync(cts.Token))
				{
				}
			});
		}
	}
}
