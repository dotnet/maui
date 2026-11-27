#nullable enable
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#if IOS || MACCATALYST
using AuthenticationServices;
using CoreFoundation;
#endif
using Foundation;
using Microsoft.Maui.ApplicationModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
#if IOS || MACCATALYST
		const int asWebAuthenticationSessionErrorCodeCanceledLogin = 1;
		const string asWebAuthenticationSessionErrorDomain = "com.apple.AuthenticationServices.WebAuthenticationSession";
#endif

		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
			AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(
			WebAuthenticatorOptions webAuthenticatorOptions,
			CancellationToken cancellationToken)
		{
			if (webAuthenticatorOptions is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions));

			var url = webAuthenticatorOptions.Url;
			var callbackUrl = webAuthenticatorOptions.CallbackUrl;
			var responseDecoder = webAuthenticatorOptions.ResponseDecoder;
			var prefersEphemeral = webAuthenticatorOptions.PrefersEphemeralWebBrowserSession;

			if (url is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
			if (callbackUrl is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));
			if (!url.IsAbsoluteUri)
				throw new ArgumentException("The authentication URL must be absolute.", nameof(webAuthenticatorOptions.Url));
			if (!callbackUrl.IsAbsoluteUri)
				throw new ArgumentException("The callback URL must be absolute.", nameof(webAuthenticatorOptions.CallbackUrl));

#if IOS || MACCATALYST
			ValidateModernSessionUrls(url, callbackUrl);
#else
			if (!VerifyHasUrlSchemeOrDoesntRequire(callbackUrl.Scheme))
				throw new InvalidOperationException("You must register the callback URL scheme in the app Info.plist.");
#endif

			cancellationToken.ThrowIfCancellationRequested();

#if IOS || MACCATALYST
			ContextProvider contextProvider;
			try
			{
				contextProvider = await MainThread.InvokeOnMainThreadAsync(() =>
				{
					var window = WindowStateManager.Default.GetCurrentUIWindow();
					if (window is null)
						throw new InvalidOperationException();

					return new ContextProvider(window);
				});
			}
			catch (Exception)
			{
				throw new InvalidOperationException("Unable to identify a presentation window for web authentication.");
			}
#endif

			var request = WebAuthenticatorRequestManager.Begin(
				url,
				callbackUrl,
				responseDecoder,
				prefersEphemeral,
				cancellationToken);

			var registration = cancellationToken.Register(
				static state =>
				{
					var request = (WebAuthenticatorRequest)state!;
					WebAuthenticatorRequestManager.TryCancelFromCaller(request);
				},
				request);

#if IOS || MACCATALYST
			ASWebAuthenticationSession? session = null;
#endif

			try
			{
				if (!request.Task.IsCompleted)
				{
#if IOS || MACCATALYST
					try
					{
						await MainThread.InvokeOnMainThreadAsync(() =>
						{
							if (request.Task.IsCompleted)
								return;

							session = CreateSession(request, url, callbackUrl);
							session.PresentationContextProvider = contextProvider;
							session.PrefersEphemeralWebBrowserSession = prefersEphemeral;

							if (!session.Start())
							{
								WebAuthenticatorRequestManager.TryFail(
									request,
									new InvalidOperationException("The native web authentication session could not be started."));
							}
						});
					}
					catch (FeatureNotSupportedException)
					{
						throw;
					}
					catch (Exception ex)
					{
						WebAuthenticatorRequestManager.TryFail(
							request,
							new InvalidOperationException(
								"The native web authentication session could not be started.",
								ex));
					}
#else
					try
					{
						if (!UIApplication.SharedApplication.OpenUrl(WebUtils.GetNativeUrl(url)))
						{
							WebAuthenticatorRequestManager.TryFail(
								request,
								new InvalidOperationException("The system browser could not be opened for authentication."));
						}
					}
					catch (Exception)
					{
						WebAuthenticatorRequestManager.TryFail(
							request,
							new InvalidOperationException("The system browser could not be opened for authentication."));
					}
#endif
				}

				return await request.Task.ConfigureAwait(false);
			}
			finally
			{
#if IOS || MACCATALYST
				await CancelAndDisposeSessionAsync(session).ConfigureAwait(false);
				GC.KeepAlive(contextProvider);
				GC.KeepAlive(session);
#endif
				registration.Dispose();
				WebAuthenticatorRequestManager.End(request);
			}
		}

		public bool OpenUrlCallback(Uri uri) =>
			TryHandleBuiltInCallback(uri);

		internal static bool TryHandleBuiltInCallback(Uri uri) =>
			WebAuthenticatorRequestManager.TryHandleCallback(uri);

#if IOS || MACCATALYST
		internal static void ValidateModernSessionUrls(Uri url, Uri callbackUrl)
		{
			if (url.Scheme is not "http" and not "https")
			{
				throw new InvalidOperationException(
					"ASWebAuthenticationSession requires an HTTP or HTTPS authentication URL.");
			}

			if (callbackUrl.Scheme == "http")
				throw new InvalidOperationException("HTTP callback URLs are not supported by ASWebAuthenticationSession.");

			if (callbackUrl.Scheme != "https")
				return;

			if (!callbackUrl.IsDefaultPort)
			{
				throw new InvalidOperationException(
					"ASWebAuthenticationSession cannot preserve a non-default HTTPS callback port.");
			}

#if IOS
			if (!OperatingSystem.IsIOSVersionAtLeast(17, 4))
#elif MACCATALYST
			if (!OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
#endif
			{
				throw new FeatureNotSupportedException(
					"HTTPS WebAuthenticator callbacks require iOS or Mac Catalyst 17.4 or later and an Associated Domains configuration.");
			}
		}

		static ASWebAuthenticationSession CreateSession(
			WebAuthenticatorRequest request,
			Uri url,
			Uri callbackUrl)
		{
			var nativeUrl = WebUtils.GetNativeUrl(url);
			ASWebAuthenticationSessionCompletionHandler completionHandler = (callbackUrl, error) =>
				PostNativeCompletion(request, callbackUrl, error);

			if (callbackUrl.Scheme != "https")
				return new ASWebAuthenticationSession(nativeUrl, callbackUrl.Scheme, completionHandler);

#if IOS
			if (OperatingSystem.IsIOSVersionAtLeast(17, 4))
#elif MACCATALYST
			if (OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
#endif
			{
				var callback = ASWebAuthenticationSessionCallback.Create(callbackUrl.Host, callbackUrl.AbsolutePath);
				return new ASWebAuthenticationSession(nativeUrl, callback, completionHandler);
			}

			throw new FeatureNotSupportedException("HTTPS WebAuthenticator callbacks require iOS or Mac Catalyst 17.4 or later.");
		}

		internal static void PostNativeCompletion(
			WebAuthenticatorRequest request,
			NSUrl? callbackUrl,
			NSError? error)
		{
			string? absoluteCallbackUrl = null;
			var wasCanceled = false;
			Exception? nativeFailure = null;
			try
			{
				absoluteCallbackUrl = callbackUrl?.AbsoluteString;
				if (error is not null)
				{
					wasCanceled = error.Domain == asWebAuthenticationSessionErrorDomain &&
						error.Code == asWebAuthenticationSessionErrorCodeCanceledLogin;
					if (!wasCanceled)
						nativeFailure = new NSErrorException(error);
				}
			}
			catch (Exception ex)
			{
				nativeFailure = ex;
			}

			// DispatchQueue.DispatchAsync never runs inline, including when the native callback is already on main.
			DispatchQueue.MainQueue.DispatchAsync(() =>
				CompleteNativeSession(request, absoluteCallbackUrl, wasCanceled, nativeFailure));
		}

		internal static void CompleteNativeSession(
			WebAuthenticatorRequest request,
			string? absoluteCallbackUrl,
			bool wasCanceled,
			Exception? nativeFailure)
		{
			if (wasCanceled)
			{
				WebAuthenticatorRequestManager.TryCancelFromPlatform(request);
				return;
			}

			if (nativeFailure is not null)
			{
				WebAuthenticatorRequestManager.TryFail(
					request,
					new InvalidOperationException(
						"The native web authentication session failed.",
						nativeFailure));
				return;
			}

			if (absoluteCallbackUrl is null)
			{
				WebAuthenticatorRequestManager.TryHandleTerminalCallback(request, null);
				return;
			}

			try
			{
				WebAuthenticatorRequestManager.TryHandleTerminalCallback(request, new Uri(absoluteCallbackUrl));
			}
			catch (Exception)
			{
				WebAuthenticatorRequestManager.TryFail(
					request,
					new InvalidOperationException("The native authentication callback URI was invalid."));
			}
		}

		static Task CancelAndDisposeSessionAsync(ASWebAuthenticationSession? session)
		{
			if (session is null)
				return Task.CompletedTask;

			var cleanupSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			DispatchQueue.MainQueue.DispatchAsync(() =>
			{
				try
				{
					session.Cancel();
					session.Dispose();
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Unable to close the native WebAuthenticator session ({ex}).");
				}
				finally
				{
					cleanupSource.TrySetResult(null);
				}
			});

			return cleanupSource.Task;
		}

		sealed class ContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
		{
			internal ContextProvider(UIWindow window) =>
				Window = window;

			internal UIWindow Window { get; }

			[Export("presentationAnchorForWebAuthenticationSession:")]
			public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session) =>
				Window;
		}
#else
		static bool VerifyHasUrlSchemeOrDoesntRequire(string scheme)
		{
			if (OperatingSystem.IsTvOSVersionAtLeast(11))
				return true;

			return AppInfoImplementation.VerifyHasUrlScheme(scheme);
		}
#endif
	}
}
