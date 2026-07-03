using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for timeout and slow operation scenarios.
	/// Verifies that the handler behaves correctly when operations are slow
	/// or when the handler is never completed (simulating hung tools).
	/// </summary>
	public class TimeoutTests
	{
		[Fact]
		public async Task ReadAllAsync_NeverCompleted_CanBeCancelledViaTimeout()
		{
			// Simulates a hung tool that never returns — the handler is never completed.
			// The reader should be cancellable via CancellationToken timeout.
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

			await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
			{
				await foreach (var _ in handler.ReadAllAsync(cts.Token))
				{
				}
			});
		}

		[Fact]
		public async Task ReadAllAsync_SlowProducer_ReadsAllUpdatesBeforeTimeout()
		{
			// Simulates a slow producer that writes content with delays.
			// The reader should still get all updates before the channel is completed.
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			var writeTask = Task.Run(async () =>
			{
				handler.ProcessContent("Hello");
				await Task.Delay(50);
				handler.ProcessContent("Hello World");
				await Task.Delay(50);
				handler.Complete();
			});

			// Use a generous timeout — we expect completion before timeout
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

			var updates = new List<Microsoft.Extensions.AI.ChatResponseUpdate>();
			await foreach (var update in handler.ReadAllAsync(cts.Token))
			{
				updates.Add(update);
			}

			await writeTask;

			// Should have received text updates
			Assert.NotEmpty(updates);
			var allText = string.Concat(updates
				.SelectMany(u => u.Contents.OfType<Microsoft.Extensions.AI.TextContent>())
				.Select(tc => tc.Text));
			Assert.Contains("World", allText, StringComparison.Ordinal);
		}

		[Fact]
		public async Task ReadAllAsync_SlowToolExecution_ReceivesUpdatesBeforeAndAfterTool()
		{
			// Simulates a tool that takes time to execute — content before, tool call,
			// slow tool result, content after.
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			var writeTask = Task.Run(async () =>
			{
				handler.ProcessContent("Let me check the weather...");
				await Task.Delay(30);
				handler.ProcessToolCall("call-1", "GetWeather", "{\"location\":\"Boston\"}");
				// Simulate slow tool execution
				await Task.Delay(200);
				handler.ProcessToolResult("call-1", "Sunny, 72°F");
				await Task.Delay(30);
				handler.ProcessContent("The weather in Boston is sunny and 72°F.");
				handler.Complete();
			});

			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

			var updates = new List<Microsoft.Extensions.AI.ChatResponseUpdate>();
			await foreach (var update in handler.ReadAllAsync(cts.Token))
			{
				updates.Add(update);
			}

			await writeTask;

			// Should have content, tool call, tool result, and final content
			Assert.True(updates.Count >= 4, $"Expected at least 4 updates, got {updates.Count}");

			var hasToolCall = updates.Any(u => u.Contents.OfType<Microsoft.Extensions.AI.FunctionCallContent>().Any());
			var hasToolResult = updates.Any(u => u.Contents.OfType<Microsoft.Extensions.AI.FunctionResultContent>().Any());
			Assert.True(hasToolCall, "Expected a tool call update");
			Assert.True(hasToolResult, "Expected a tool result update");
		}

		[Fact]
		public async Task NonStreamingHandler_NeverCompleted_TaskDoesNotCompleteWithinTimeout()
		{
			// Verifies that a NonStreamingResponseHandler that is never completed
			// will not resolve its Task — and can be observed via a timeout.
			var handler = new NonStreamingResponseHandler();

			var completed = await Task.WhenAny(handler.Task, Task.Delay(200));

			Assert.NotSame(handler.Task, completed);
			Assert.False(handler.Task.IsCompleted, "Handler task should not complete when nothing calls Complete/CompleteWithError/CompleteCancelled");
		}

		[Fact]
		public async Task NonStreamingHandler_SlowCompletion_TaskCompletesEventually()
		{
			// Verifies that a NonStreamingResponseHandler completes when Complete() is eventually called.
			var handler = new NonStreamingResponseHandler();

			_ = Task.Run(async () =>
			{
				await Task.Delay(200);
				handler.Complete(new Microsoft.Extensions.AI.ChatResponse(
					[new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, "Done")]));
			});

			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			var result = await handler.Task.WaitAsync(cts.Token);

			Assert.Equal("Done", result.Messages.First().Text);
		}
	}
}
