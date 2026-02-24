using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Decoupled representation of a streaming update, allowing the handler to be tested
/// without constructing native <c>ResponseUpdateNative</c> objects (which have no public C# constructor).
/// </summary>
internal readonly record struct StreamUpdate(
	ResponseUpdateTypeNative UpdateType,
	string? Text = null,
	string? ToolCallId = null,
	string? ToolCallName = null,
	string? ToolCallArguments = null,
	string? ToolCallResult = null)
{
	/// <summary>Creates a <see cref="StreamUpdate"/> from a native update.</summary>
	public static StreamUpdate FromNative(ResponseUpdateNative native) =>
		new(native.UpdateType, native.Text, native.ToolCallId, native.ToolCallName, native.ToolCallArguments, native.ToolCallResult);
}

/// <summary>
/// Handles the channel, chunker, and update processing for streaming responses.
/// Extracted from <see cref="AppleIntelligenceChatClient"/> for testability.
/// </summary>
internal sealed class StreamingResponseHandler
{
	private readonly Channel<ChatResponseUpdate> _channel;
	private readonly StreamChunkerBase _chunker;

	public StreamingResponseHandler(bool useJsonChunker)
	{
		_channel = Channel.CreateUnbounded<ChatResponseUpdate>(
			new UnboundedChannelOptions { SingleReader = true });
		_chunker = useJsonChunker
			? new JsonStreamChunker()
			: new PlainTextStreamChunker();
	}

	/// <summary>
	/// Processes a single streaming update into channel messages.
	/// </summary>
	public void ProcessUpdate(StreamUpdate update)
	{
		switch (update.UpdateType)
		{
			case ResponseUpdateTypeNative.Content:
				if (update.Text is not null)
				{
					var delta = _chunker.Process(update.Text);
					if (!string.IsNullOrEmpty(delta))
					{
						_channel.Writer.TryWrite(new ChatResponseUpdate
						{
							Role = ChatRole.Assistant,
							Contents = { new TextContent(delta) }
						});
					}
				}
				break;

			case ResponseUpdateTypeNative.ToolCall:
				// Flush any pending content before resetting for tool call
				var pendingContent = _chunker.Flush();
				if (!string.IsNullOrEmpty(pendingContent))
				{
					_channel.Writer.TryWrite(new ChatResponseUpdate
					{
						Role = ChatRole.Assistant,
						Contents = { new TextContent(pendingContent) }
					});
				}
				_chunker.Reset();

				var args = update.ToolCallArguments is null
					? null
#pragma warning disable IL3050, IL2026 // DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled
					: JsonSerializer.Deserialize<AIFunctionArguments>(update.ToolCallArguments, AIJsonUtilities.DefaultOptions);
#pragma warning restore IL3050, IL2026

				_channel.Writer.TryWrite(new ChatResponseUpdate
				{
					Role = ChatRole.Assistant,
					Contents = { new FunctionCallContent(update.ToolCallId!, update.ToolCallName!, args) }
				});
				break;

			case ResponseUpdateTypeNative.ToolResult:
				_channel.Writer.TryWrite(new ChatResponseUpdate
				{
					Role = ChatRole.Tool,
					Contents = { new FunctionResultContent(update.ToolCallId!, update.ToolCallResult!) }
				});
				break;
		}
	}

	/// <summary>
	/// Flushes remaining chunker content and completes the channel successfully.
	/// </summary>
	public void Complete()
	{
		var finalChunk = _chunker.Flush();
		if (!string.IsNullOrEmpty(finalChunk))
		{
			_channel.Writer.TryWrite(new ChatResponseUpdate
			{
				Role = ChatRole.Assistant,
				Contents = { new TextContent(finalChunk) }
			});
		}

		_channel.Writer.TryComplete();
	}

	/// <summary>
	/// Completes the channel with an error. Safe to call multiple times.
	/// </summary>
	public void CompleteWithError(Exception exception)
	{
		_channel.Writer.TryComplete(exception);
	}

	/// <summary>
	/// Returns an async enumerable that reads all updates from the channel.
	/// </summary>
	public IAsyncEnumerable<ChatResponseUpdate> ReadAllAsync(CancellationToken cancellationToken)
		=> _channel.Reader.ReadAllAsync(cancellationToken);
}
