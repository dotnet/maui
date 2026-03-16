using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class NonStreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for successful completion paths.
	/// </summary>
	public class CompletionTests
	{
		[Fact]
		public async Task Complete_WithResponse_ReturnsIt()
		{
			var handler = new NonStreamingResponseHandler();
			var expected = new ChatResponse([new ChatMessage(ChatRole.Assistant, "Hello world")]);

			handler.Complete(expected);

			var result = await handler.Task;
			Assert.Same(expected, result);
		}

		[Fact]
		public async Task Complete_WithMultipleMessages_ReturnsAll()
		{
			var handler = new NonStreamingResponseHandler();
			var expected = new ChatResponse([
				new ChatMessage(ChatRole.User, "What's the weather?"),
				new ChatMessage(ChatRole.Assistant, "It's sunny in Boston")
			]);

			handler.Complete(expected);

			var result = await handler.Task;
			Assert.Equal(2, result.Messages.Count);
			Assert.Equal(ChatRole.User, result.Messages[0].Role);
			Assert.Equal(ChatRole.Assistant, result.Messages[1].Role);
		}

		[Fact]
		public async Task Complete_WithEmptyFallback_ReturnsIt()
		{
			var handler = new NonStreamingResponseHandler();
			var fallback = new ChatResponse([new ChatMessage(ChatRole.Assistant, "")]);

			handler.Complete(fallback);

			var result = await handler.Task;
			Assert.Single(result.Messages);
			Assert.Equal("", result.Messages[0].Text);
		}
	}
}
