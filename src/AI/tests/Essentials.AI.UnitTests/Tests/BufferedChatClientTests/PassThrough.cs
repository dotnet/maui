using Microsoft.Extensions.AI;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class BufferedChatClientTests
{
	public class PassThrough
	{
		[Fact]
		public async Task GetResponseAsyncPassesThrough()
		{
			// Arrange
			var mockClient = new MockChatClient();
			mockClient.SetNonStreamingResponse("Test response");
			var bufferedClient = new BufferedChatClient(mockClient);
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var response = await bufferedClient.GetResponseAsync(messages);
			
			// Assert
			Assert.NotNull(response);
			Assert.Single(response.Messages);
			var text = response.Messages[0].Contents.OfType<TextContent>().FirstOrDefault()?.Text;
			Assert.Equal("Test response", text);
		}

		[Fact]
		public async Task PreservesMetadataInBufferedUpdates()
		{
			// Arrange
			var mockClient = new MockChatClient();
			var bufferedClient = new BufferedChatClient(mockClient, minBufferSize: 100, bufferDelay: TimeSpan.FromMilliseconds(250));
			
			mockClient.AddTextChunk("This is a long enough message that will trigger the buffer to flush when the conditions are met.");
			
			var messages = new List<ChatMessage> { new(ChatRole.User, "Test") };
			
			// Act
			var updates = new List<ChatResponseUpdate>();
			await foreach (var update in bufferedClient.GetStreamingResponseAsync(messages))
			{
				updates.Add(update);
			}
			
			// Assert
			Assert.NotEmpty(updates);
			var firstUpdate = updates[0];
			Assert.Equal(ChatRole.Assistant, firstUpdate.Role);
		}
	}
}
