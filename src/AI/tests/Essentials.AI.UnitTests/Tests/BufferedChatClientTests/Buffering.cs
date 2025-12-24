using Microsoft.Extensions.AI;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class BufferedChatClientTests
{
	public class Buffering
	{
		[Fact]
		public async Task BuffersTextContentUntilThresholdReached()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 100, bufferDelay: TimeSpan.FromMilliseconds(250));
			
			// Add text chunks that are smaller than buffer size
			mockClient.AddTextChunk("Hello ");
			mockClient.AddTextChunk("world ");
			mockClient.AddTextChunk("this is a test message that will eventually exceed the minimum buffer size requirement!!");
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert - should have buffered and then yielded once threshold was met
			Assert.NotEmpty(updates);
			var firstUpdate = updates[0];
			var text = string.Concat(firstUpdate.Contents.OfType<TextContent>().Select(t => t.Text));
			Assert.True(text.Length >= 100, $"Expected buffered text to be at least 100 chars, got {text.Length}");
		}

		[Fact]
		public async Task FlushesRemainingBufferAtEnd()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 100, bufferDelay: TimeSpan.FromMilliseconds(250));
			
			// Add text chunks that total less than buffer size
			mockClient.AddTextChunk("Short ");
			mockClient.AddTextChunk("message.");
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert - should flush the remaining buffer at the end
			Assert.Single(updates);
			var text = string.Concat(updates[0].Contents.OfType<TextContent>().Select(t => t.Text));
			Assert.Equal("Short message.", text);
		}

		[Fact]
		public async Task RespectsBufferDelay()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 10, bufferDelay: TimeSpan.FromMilliseconds(100));
			
			// Add chunks that exceed buffer size but test delay
			mockClient.AddTextChunk("First chunk that is long enough");
			mockClient.AddDelayedTextChunk("Second chunk", TimeSpan.FromMilliseconds(150));
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert - should have yielded when both size and delay conditions were met
			Assert.NotEmpty(updates);
		}

		[Fact]
		public async Task HandlesMultipleFlushCycles()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 50, bufferDelay: TimeSpan.FromMilliseconds(100));
			
			// Add enough chunks to trigger multiple flushes
			mockClient.AddTextChunk("First batch of text that exceeds the minimum size");
			mockClient.AddDelayedTextChunk("Second batch of text that also exceeds the minimum", TimeSpan.FromMilliseconds(150));
			mockClient.AddDelayedTextChunk("Final small batch", TimeSpan.FromMilliseconds(150));
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert - should have multiple updates
			Assert.True(updates.Count >= 2, $"Expected at least 2 updates, got {updates.Count}");
		}
	}
}
