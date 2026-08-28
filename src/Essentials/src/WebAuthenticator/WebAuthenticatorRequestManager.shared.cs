#nullable enable
using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Maui.Authentication
{
	static class WebAuthenticatorRequestManager
	{
		static readonly object locker = new();

		static WebAuthenticatorRequest? activeRequest;
		static long nextRequestId;

		internal static WebAuthenticatorRequest Begin(
			Uri url,
			Uri callbackUrl,
			IWebAuthenticatorResponseDecoder? responseDecoder,
			bool prefersEphemeralWebBrowserSession,
			CancellationToken cancellationToken,
			Action? beforeCallbackCompletion = null)
		{
			if (url is null)
				throw new ArgumentNullException(nameof(url));
			if (callbackUrl is null)
				throw new ArgumentNullException(nameof(callbackUrl));
			if (!url.IsAbsoluteUri)
				throw new ArgumentException("The authentication URL must be absolute.", nameof(url));
			if (!callbackUrl.IsAbsoluteUri)
				throw new ArgumentException("The callback URL must be absolute.", nameof(callbackUrl));

			cancellationToken.ThrowIfCancellationRequested();

			lock (locker)
			{
				if (activeRequest is not null)
					throw new InvalidOperationException("Another WebAuthenticator operation is already in progress.");

				nextRequestId = nextRequestId == long.MaxValue ? 1 : nextRequestId + 1;

				activeRequest = new WebAuthenticatorRequest(
					nextRequestId,
					url,
					callbackUrl,
					responseDecoder,
					prefersEphemeralWebBrowserSession,
					cancellationToken,
					beforeCallbackCompletion);

				return activeRequest;
			}
		}

		internal static bool TryHandleCallback(Uri? callbackUri)
		{
			WebAuthenticatorRequest request;

			lock (locker)
			{
				request = activeRequest!;
				if (request is null || callbackUri is null || !callbackUri.IsAbsoluteUri)
					return false;

				if (!WebUtils.CanHandleCallback(request.CallbackUrl, callbackUri))
					return false;

				// Protect duplicates while the platform is still cleaning up and before End removes the request.
				if (request.CompletionClaimed)
					return true;

				request.CompletionClaimed = true;
			}

			CompleteCallback(request, callbackUri);
			return true;
		}

		internal static bool TryHandleTerminalCallback(WebAuthenticatorRequest request, Uri? callbackUri)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			if (!TryReserve(request))
				return false;

			CompleteTerminalCallback(request, callbackUri);
			return true;
		}

		internal static bool TryHandleTerminalCallback(long requestId, Uri? callbackUri)
		{
			if (!TryReserve(requestId, out var request))
				return false;

			CompleteTerminalCallback(request, callbackUri);
			return true;
		}

		internal static bool TryCancelFromCaller(WebAuthenticatorRequest request)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			if (!TryReserve(request))
				return false;

			request.TrySetCanceled(request.CancellationToken);
			return true;
		}

		internal static bool TryCancelFromPlatform(WebAuthenticatorRequest request)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			if (!TryReserve(request))
				return false;

			request.TrySetCanceled();
			return true;
		}

		internal static bool TryCancelFromPlatform(long requestId)
		{
			if (!TryReserve(requestId, out var request))
				return false;

			request.TrySetCanceled();
			return true;
		}

		internal static bool TryFail(WebAuthenticatorRequest request, Exception exception)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));
			if (exception is null)
				throw new ArgumentNullException(nameof(exception));

			if (!TryReserve(request))
				return false;

			request.TrySetException(exception);
			return true;
		}

		internal static bool TryFail(long requestId, Exception exception)
		{
			if (exception is null)
				throw new ArgumentNullException(nameof(exception));

			if (!TryReserve(requestId, out var request))
				return false;

			request.TrySetException(exception);
			return true;
		}

		internal static bool End(WebAuthenticatorRequest request)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			lock (locker)
			{
				if (!ReferenceEquals(activeRequest, request))
					return false;

				activeRequest = null;
				return true;
			}
		}

		internal static bool IsActive(long requestId)
		{
			lock (locker)
				return activeRequest?.Id == requestId;
		}

		static bool TryReserve(WebAuthenticatorRequest request)
		{
			lock (locker)
			{
				if (!ReferenceEquals(activeRequest, request) || request.CompletionClaimed)
					return false;

				request.CompletionClaimed = true;
				return true;
			}
		}

		static bool TryReserve(long requestId, out WebAuthenticatorRequest request)
		{
			lock (locker)
			{
				request = activeRequest!;
				if (request is null || request.Id != requestId || request.CompletionClaimed)
					return false;

				request.CompletionClaimed = true;
				return true;
			}
		}

		static void CompleteTerminalCallback(WebAuthenticatorRequest request, Uri? callbackUri)
		{
			if (callbackUri is null || !callbackUri.IsAbsoluteUri)
			{
				request.TrySetException(new InvalidOperationException("The native authentication result did not include an absolute callback URI."));
				return;
			}

			if (!WebUtils.CanHandleCallback(request.CallbackUrl, callbackUri))
			{
				request.TrySetException(new InvalidOperationException("The native authentication result did not match the expected callback route."));
				return;
			}

			CompleteCallback(request, callbackUri);
		}

		static void CompleteCallback(WebAuthenticatorRequest request, Uri callbackUri)
		{
			try
			{
				request.BeforeCallbackCompletion?.Invoke();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"WebAuthenticator pre-completion callback failed ({ex.GetType().Name}).");
			}

			try
			{
				request.TrySetResult(new WebAuthenticatorResult(callbackUri, request.ResponseDecoder));
			}
			catch (Exception ex)
			{
				// Decoder exceptions are delivered to the caller without logging their contents.
				request.TrySetException(ex);
			}
		}
	}
}
