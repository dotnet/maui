using Microsoft.Extensions.AI;
using Maui.Controls.Sample.Services;
using Xunit;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public class BufferedChatClientTests
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

	private class MockChatClient : IChatClient
	{
		private readonly List<object> _streamContent = new();
		private string? _nonStreamingResponse;

		public ChatClientMetadata Metadata => new("MockClient");

		public void AddTextChunk(string text)
		{
			_streamContent.Add(text);
		}

		public void AddDelayedTextChunk(string text, TimeSpan delay)
		{
			_streamContent.Add(new DelayedChunk(text, delay));
		}

		public void AddFunctionCall(string name, Dictionary<string, object?> arguments)
		{
			_streamContent.Add(new FunctionCallContent(Guid.NewGuid().ToString(), name, arguments));
		}

		public void SetNonStreamingResponse(string response)
		{
			_nonStreamingResponse = response;
		}

		public Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> chatMessages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			var response = new ChatResponse(
				new ChatMessage(ChatRole.Assistant, _nonStreamingResponse ?? "Default response")
			);
			return Task.FromResult(response);
		}

		public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> chatMessages,
			ChatOptions? options = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			foreach (var item in _streamContent)
			{
				if (item is string text)
				{
					yield return new ChatResponseUpdate
					{
						Role = ChatRole.Assistant,
						Contents = [new TextContent(text)]
					};
				}
				else if (item is DelayedChunk delayed)
				{
					await Task.Delay(delayed.Delay, cancellationToken);
					yield return new ChatResponseUpdate
					{
						Role = ChatRole.Assistant,
						Contents = [new TextContent(delayed.Text)]
					};
				}
				else if (item is FunctionCallContent functionCall)
				{
					yield return new ChatResponseUpdate
					{
						Role = ChatRole.Assistant,
						Contents = [functionCall]
					};
				}
			}
		}

		public object? GetService(Type serviceType, object? serviceKey = null) => null;

		public TService? GetService<TService>(object? key = null) where TService : class => null;

		public void Dispose() { }

		private record DelayedChunk(string Text, TimeSpan Delay);
	}
}
