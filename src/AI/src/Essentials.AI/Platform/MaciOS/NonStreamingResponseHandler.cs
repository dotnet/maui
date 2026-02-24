using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Handles the TaskCompletionSource and response conversion for non-streaming responses.
/// Extracted from <see cref="AppleIntelligenceChatClient"/> for testability.
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
	/// Completes with a successful response, converting from native format.
	/// </summary>
	public void Complete(ChatResponseNative? response)
	{
		try
		{
			var chatResponse = AppleIntelligenceChatClient.FromNativeChatResponse(response);
			_tcs.TrySetResult(chatResponse);
		}
		catch (Exception ex)
		{
			_tcs.TrySetException(ex);
		}
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
