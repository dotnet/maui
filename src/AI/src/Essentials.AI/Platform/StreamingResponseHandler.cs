using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Handles the channel, chunker, and update processing for streaming responses.
/// Extracted from the platform-specific chat client for testability.
/// </summary>
/// <remarks>
/// When a <see cref="StreamChunkerBase"/> is provided, <see cref="ProcessContent"/> expects
/// cumulative snapshots and the chunker computes deltas (used by Apple Intelligence).
/// When no chunker is provided, <see cref="ProcessContent"/> expects pre-chunked deltas
/// and passes them through directly (used by Windows Phi Silica).
/// </remarks>
internal sealed class StreamingResponseHandler
{
	private readonly Channel<ChatResponseUpdate> _channel;
	private readonly StreamChunkerBase? _chunker;

	/// <summary>
	/// Creates a handler that passes content through directly (no chunking).
	/// Use when the AI model already provides incremental deltas.
	/// </summary>
	public StreamingResponseHandler()
	{
		_channel = Channel.CreateUnbounded<ChatResponseUpdate>(
			new UnboundedChannelOptions { SingleReader = true });
	}

	/// <summary>
	/// Creates a handler with a chunker for computing deltas from cumulative snapshots.
	/// Use when the AI model provides progressively longer complete responses.
	/// </summary>
	public StreamingResponseHandler(StreamChunkerBase chunker) : this()
	{
		_chunker = chunker;
	}

	/// <summary>
	/// Processes a content (text) streaming update.
	/// If a chunker is configured, <paramref name="text"/> should be the cumulative response.
	/// If no chunker, <paramref name="text"/> should be the incremental delta.
	/// </summary>
	public void ProcessContent(string? text)
	{
		if (text is null)
			return;

		var delta = _chunker is not null ? _chunker.Process(text) : text;
		if (!string.IsNullOrEmpty(delta))
		{
			_channel.Writer.TryWrite(new ChatResponseUpdate
			{
				Role = ChatRole.Assistant,
				Contents = { new TextContent(delta) }
			});
		}
	}

	/// <summary>
	/// Processes a tool call update. Flushes any pending content first.
	/// </summary>
	public void ProcessToolCall(string? toolCallId, string? toolCallName, string? toolCallArguments)
	{
		if (_chunker is not null)
		{
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
		}

		var args = toolCallArguments is null
			? null
#pragma warning disable IL3050, IL2026 // DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled
			: JsonSerializer.Deserialize<AIFunctionArguments>(toolCallArguments, AIJsonUtilities.DefaultOptions);
#pragma warning restore IL3050, IL2026

		_channel.Writer.TryWrite(new ChatResponseUpdate
		{
			Role = ChatRole.Assistant,
			Contents = { new FunctionCallContent(toolCallId!, toolCallName!, args) { InformationalOnly = true } }
		});
	}

	/// <summary>
	/// Processes a tool result update.
	/// </summary>
	public void ProcessToolResult(string? toolCallId, string? toolCallResult)
	{
		_channel.Writer.TryWrite(new ChatResponseUpdate
		{
			Role = ChatRole.Tool,
			Contents = { new FunctionResultContent(toolCallId!, toolCallResult!) }
		});
	}

	/// <summary>
	/// Flushes remaining chunker content and completes the channel successfully.
	/// </summary>
	public void Complete()
	{
		if (_chunker is not null)
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
