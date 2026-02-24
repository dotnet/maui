using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for completion and error handling.
	/// </summary>
	public class CompletionTests
	{
		[Fact]
		public async Task Complete_FlushesRemainingContent()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessContent("Hello");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal("Hello", updates[0].Contents.OfType<Microsoft.Extensions.AI.TextContent>().Single().Text);
		}

		[Fact]
		public async Task CompleteWithError_SurfacesExceptionToReader()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessContent("Hello");
			handler.CompleteWithError(new InvalidOperationException("test error"));

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await ReadAll(handler));
		}

		[Fact]
		public void DoubleComplete_DoesNotThrow()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.CompleteWithError(new InvalidOperationException("first error"));
			handler.Complete();
			handler.CompleteWithError(new InvalidOperationException("second error"));
		}
	}
}
