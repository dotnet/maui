using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for <see cref="StreamingResponseHandler"/> without a chunker (passthrough mode).
	/// Used by models like Phi Silica that already provide incremental deltas.
	/// </summary>
	public class PassthroughTests
	{
		[Fact]
		public async Task ProcessContent_WithoutChunker_PassesDeltasDirectly()
		{
			var handler = new StreamingResponseHandler();

			handler.ProcessContent("Hello");
			handler.ProcessContent(" world");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Equal(2, updates.Count);
			Assert.Equal("Hello", updates[0].Contents.OfType<TextContent>().Single().Text);
			Assert.Equal(" world", updates[1].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task ProcessContent_WithoutChunker_DoesNotAccumulateOrDelta()
		{
			var handler = new StreamingResponseHandler();

			// In passthrough mode, each call is emitted as-is (no delta computation)
			handler.ProcessContent("abc");
			handler.ProcessContent("abc");
			handler.Complete();

			var updates = await ReadAll(handler);

			// Both "abc" values pass through (unlike chunker which would emit "" for the second)
			Assert.Equal(2, updates.Count);
			Assert.Equal("abc", updates[0].Contents.OfType<TextContent>().Single().Text);
			Assert.Equal("abc", updates[1].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task ProcessContent_WithoutChunker_NullIgnored()
		{
			var handler = new StreamingResponseHandler();

			handler.ProcessContent(null);
			handler.ProcessContent("Hello");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal("Hello", updates[0].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task ProcessContent_WithoutChunker_EmptyStringIgnored()
		{
			var handler = new StreamingResponseHandler();

			handler.ProcessContent("");
			handler.ProcessContent("data");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal("data", updates[0].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task Complete_WithoutChunker_NoExtraFlush()
		{
			var handler = new StreamingResponseHandler();

			handler.ProcessContent("token1");
			handler.ProcessContent("token2");
			handler.Complete();

			var updates = await ReadAll(handler);

			// Only the two tokens, no extra flush update
			Assert.Equal(2, updates.Count);
		}

		[Fact]
		public async Task ToolCall_WithoutChunker_NoFlushNeeded()
		{
			var handler = new StreamingResponseHandler();

			handler.ProcessContent("before");
			handler.ProcessToolCall("id1", "myTool", """{"arg": "val"}""");
			handler.ProcessContent("after");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Equal(3, updates.Count);
			Assert.Equal("before", updates[0].Contents.OfType<TextContent>().Single().Text);
			Assert.IsType<FunctionCallContent>(updates[1].Contents[0]);
			Assert.Equal("after", updates[2].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task Error_WithoutChunker_PropagatesException()
		{
			var handler = new StreamingResponseHandler();

			handler.ProcessContent("partial");
			handler.CompleteWithError(new InvalidOperationException("test error"));

			await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				await foreach (var _ in handler.ReadAllAsync(CancellationToken.None)) { }
			});
		}
	}
}
