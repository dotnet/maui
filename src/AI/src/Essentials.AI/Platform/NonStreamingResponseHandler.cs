using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Handles the TaskCompletionSource and completion for non-streaming responses.
/// Extracted from the platform-specific chat client for testability.
/// </summary>
internal sealed class NonStreamingResponseHandler
{
	private readonly TaskCompletionSource<ChatResponse> _tcs =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	/// <summary>
	/// The task that completes when the response is ready.
	/// </summary>
	public Task<ChatResponse> Task => _tcs.Task;

	/// <summary>
	/// Completes with a successful response.
	/// </summary>
	public void Complete(ChatResponse response)
	{
		_tcs.TrySetResult(response);
	}

	/// <summary>
	/// Completes with an error. Safe to call multiple times.
	/// </summary>
	public void CompleteWithError(Exception exception)
	{
		_tcs.TrySetException(exception);
	}

	/// <summary>
	/// Completes with a cancellation.
	/// </summary>
	public void CompleteCancelled(CancellationToken cancellationToken)
	{
		_tcs.TrySetCanceled(cancellationToken);
	}
}
