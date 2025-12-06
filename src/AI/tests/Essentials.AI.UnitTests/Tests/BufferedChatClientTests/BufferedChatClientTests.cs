using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Tests for the BufferedChatClient which buffers streaming text content
/// until a minimum size threshold is reached before yielding updates.
/// </summary>
public partial class BufferedChatClientTests
{
	/// <summary>
	/// Mock chat client for testing that allows adding predefined responses.
	/// </summary>
	internal class MockChatClient : IChatClient
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
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			var response = new ChatResponse(
				new ChatMessage(ChatRole.Assistant, _nonStreamingResponse ?? "Default response")
			);
			return Task.FromResult(response);
		}

		public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages,
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
