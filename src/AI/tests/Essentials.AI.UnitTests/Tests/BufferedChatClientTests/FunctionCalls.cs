using Microsoft.Extensions.AI;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class BufferedChatClientTests
{
	public class FunctionCalls
	{
		[Fact]
		public async Task PassesThroughNonTextContentImmediately()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 100, bufferDelay: TimeSpan.FromMilliseconds(250));
			
			// Add some text, then a function call, then more text
			mockClient.AddTextChunk("Before function");
			mockClient.AddFunctionCall("testFunction", new Dictionary<string, object?> { ["arg1"] = "value1" });
			mockClient.AddTextChunk("After function");
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert - should have at least 3 updates: buffered text before, function call, final text
			Assert.True(updates.Count >= 2, $"Expected at least 2 updates, got {updates.Count}");
			
			// Find the function call update
			var functionCallUpdate = updates.FirstOrDefault(u => u.Contents.Any(c => c is FunctionCallContent));
			Assert.NotNull(functionCallUpdate);
		}

		[Fact]
		public async Task FlushesBufferBeforeFunctionCall()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 100, bufferDelay: TimeSpan.FromMilliseconds(250));
			
			// Add text that's less than threshold, then a function call
			mockClient.AddTextChunk("Small text");
			mockClient.AddFunctionCall("testFunction", new Dictionary<string, object?> { ["arg1"] = "value1" });
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert - should flush text before function call
			Assert.Equal(2, updates.Count);
			
			var firstUpdate = updates[0];
			Assert.All(firstUpdate.Contents, c => Assert.IsType<TextContent>(c));
			var text = string.Concat(firstUpdate.Contents.OfType<TextContent>().Select(t => t.Text));
			Assert.Equal("Small text", text);
			
			var secondUpdate = updates[1];
			Assert.Contains(secondUpdate.Contents, c => c is FunctionCallContent);
		}
	}
}
