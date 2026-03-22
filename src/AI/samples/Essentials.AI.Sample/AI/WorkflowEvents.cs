using Microsoft.Agents.AI.Workflows;

namespace Maui.Controls.Sample.AI;

/// <summary>
/// Custom workflow event for status updates from executors.
/// Emits a single message for display in a fading status trail.
/// </summary>
public sealed class ExecutorStatusEvent(string message) : WorkflowEvent(message)
{
	public string StatusMessage { get; } = message;
}

/// <summary>
/// Custom workflow event for streaming text content from an executor.
/// Only emits text content, not function calls or other content types.
/// </summary>
public sealed class ItineraryTextChunkEvent(string executorId, string textChunk) : ExecutorEvent(executorId, textChunk)
{
	/// <summary>
	/// The text chunk to stream.
	/// </summary>
	public string TextChunk { get; } = textChunk;
}
