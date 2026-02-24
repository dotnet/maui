using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for <see cref="StreamingResponseHandler.ProcessContent"/>.
	/// </summary>
	public class ContentTests
	{
		[Fact]
		public async Task ProcessContent_EmitsTextDelta()
		{
			var handler = new StreamingResponseHandler(useJsonChunker: false);

			handler.ProcessContent("Hello");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal(ChatRole.Assistant, updates[0].Role);
			Assert.Equal("Hello", updates[0].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task ProcessContent_ProgressiveUpdates_EmitDeltas()
		{
			var handler = new StreamingResponseHandler(useJsonChunker: false);

			handler.ProcessContent("Hello");
			handler.ProcessContent("Hello world");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Equal(2, updates.Count);
			Assert.Equal("Hello", updates[0].Contents.OfType<TextContent>().Single().Text);
			Assert.Equal(" world", updates[1].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task ProcessContent_EmptyText_ProducesNoOutput()
		{
			var handler = new StreamingResponseHandler(useJsonChunker: false);

			handler.ProcessContent("");
			handler.Complete();

			var updates = await ReadAll(handler);
			Assert.Empty(updates);
		}

		[Fact]
		public async Task ProcessContent_NullText_ProducesNoOutput()
		{
			var handler = new StreamingResponseHandler(useJsonChunker: false);

			handler.ProcessContent(null);
			handler.Complete();

			var updates = await ReadAll(handler);
			Assert.Empty(updates);
		}
	}
}
