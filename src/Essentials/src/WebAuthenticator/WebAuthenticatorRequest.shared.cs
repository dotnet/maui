#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	sealed class WebAuthenticatorRequest
	{
		readonly TaskCompletionSource<WebAuthenticatorResult> responseSource =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		internal WebAuthenticatorRequest(
			long id,
			Uri url,
			Uri callbackUrl,
			IWebAuthenticatorResponseDecoder? responseDecoder,
			bool prefersEphemeralWebBrowserSession,
			CancellationToken cancellationToken,
			Action? beforeCallbackCompletion)
		{
			Id = id;
			Url = url;
			CallbackUrl = callbackUrl;
			ResponseDecoder = responseDecoder;
			PrefersEphemeralWebBrowserSession = prefersEphemeralWebBrowserSession;
			CancellationToken = cancellationToken;
			BeforeCallbackCompletion = beforeCallbackCompletion;
		}

		internal long Id { get; }

		internal Uri Url { get; }

		internal Uri CallbackUrl { get; }

		internal IWebAuthenticatorResponseDecoder? ResponseDecoder { get; }

		internal bool PrefersEphemeralWebBrowserSession { get; }

		internal CancellationToken CancellationToken { get; }

		internal Task<WebAuthenticatorResult> Task => responseSource.Task;

		internal Action? BeforeCallbackCompletion { get; }

		// Only WebAuthenticatorRequestManager may read or write this field, and only while holding its process-wide lock.
		internal bool CompletionClaimed;

		internal bool TrySetResult(WebAuthenticatorResult result) =>
			responseSource.TrySetResult(result);

		internal bool TrySetCanceled() =>
			responseSource.TrySetCanceled();

		internal bool TrySetCanceled(CancellationToken cancellationToken) =>
			responseSource.TrySetCanceled(cancellationToken);

		internal bool TrySetException(Exception exception) =>
			responseSource.TrySetException(exception);
	}
}
