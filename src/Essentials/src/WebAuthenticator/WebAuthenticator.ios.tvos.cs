using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
#if __IOS__
using SafariServices;
#endif
using ObjCRuntime;
using UIKit;
using WebKit;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.ApplicationModel;
using System.Threading;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
#if __IOS__
		const int asWebAuthenticationSessionErrorCodeCanceledLogin = 1;
		const string asWebAuthenticationSessionErrorDomain = "com.apple.AuthenticationServices.WebAuthenticationSession";

		const int sfAuthenticationErrorCanceledLogin = 1;
		const string sfAuthenticationErrorDomain = "com.apple.SafariServices.Authentication";
#endif

		TaskCompletionSource<WebAuthenticatorResult> tcsResponse;
		UIViewController currentViewController;
		Uri redirectUri;
		WebAuthenticatorOptions currentOptions;

#if __IOS__
		ASWebAuthenticationSession was;
		SFAuthenticationSession sf;
#endif

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> await AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
		{
			currentOptions = webAuthenticatorOptions;
			var url = webAuthenticatorOptions?.Url;
			var callbackUrl = webAuthenticatorOptions?.CallbackUrl;
			var prefersEphemeralWebBrowserSession = webAuthenticatorOptions?.PrefersEphemeralWebBrowserSession ?? false;

			if (!VerifyHasUrlSchemeOrDoesntRequire(callbackUrl.Scheme))
				throw new InvalidOperationException("You must register your URL Scheme handler in your app's Info.plist.");

			// Cancel any previous task that's still pending
			if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
				tcsResponse.TrySetCanceled();

			tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
			redirectUri = callbackUrl;
			var scheme = redirectUri.Scheme;

			// Use the CancellationToken to cancel the operation
			using (cancellationToken.Register(() => tcsResponse.TrySetCanceled()))
			{
#if __IOS__
				void AuthSessionCallback(NSUrl cbUrl, NSError error)
				{
					if (error == null)
						OpenUrlCallback(cbUrl);
					else if (error.Domain == asWebAuthenticationSessionErrorDomain && error.Code == asWebAuthenticationSessionErrorCodeCanceledLogin)
						tcsResponse.TrySetCanceled();
					else if (error.Domain == sfAuthenticationErrorDomain && error.Code == sfAuthenticationErrorCanceledLogin)
						tcsResponse.TrySetCanceled();
					else
						tcsResponse.TrySetException(new NSErrorException(error));

					was = null;
					sf = null;
				}

				if (OperatingSystem.IsIOSVersionAtLeast(12))
				{
#if IOS17_4_OR_GREATER || MACCATALYST17_4_OR_GREATER
					if (OperatingSystem.IsIOSVersionAtLeast(17, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(17, 4))
					{
						var callback = ASWebAuthenticationSessionCallback.Create(scheme);
						was = new ASWebAuthenticationSession(WebUtils.GetNativeUrl(url), callback, AuthSessionCallback);
					}
					else
#endif
					{
						was = new ASWebAuthenticationSession(WebUtils.GetNativeUrl(url), scheme, AuthSessionCallback);
					}

					if (OperatingSystem.IsIOSVersionAtLeast(13))
					{
						var ctx = new ContextProvider(WindowStateManager.Default.GetCurrentUIWindow());
						was.PresentationContextProvider = ctx;
						was.PrefersEphemeralWebBrowserSession = prefersEphemeralWebBrowserSession;
					}
					else if (prefersEphemeralWebBrowserSession)
					{
						ClearCookies();
					}

					using (was)
					{
#pragma warning disable CA1416
						was.Start();
#pragma warning restore CA1416
						return await tcsResponse.Task;
					}
				}

				if (prefersEphemeralWebBrowserSession)
					ClearCookies();

#pragma warning disable CA1422
				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					sf = new SFAuthenticationSession(WebUtils.GetNativeUrl(url), scheme, AuthSessionCallback);
					using (sf)
					{
						sf.Start();
						return await tcsResponse.Task;
					}
				}
#pragma warning restore CA1422

				var controller = new SFSafariViewController(WebUtils.GetNativeUrl(url), false)
				{
					Delegate = new NativeSFSafariViewControllerDelegate
					{
						DidFinishHandler = (svc) =>
						{
							if (!(tcsResponse?.Task?.IsCompleted ?? true))
								tcsResponse.TrySetCanceled();
						}
					},
				};

				currentViewController = controller;
				await WindowStateManager.Default.GetCurrentUIViewController().PresentViewControllerAsync(controller, true);
#else
        var opened = UIApplication.SharedApplication.OpenUrl(url);
        if (!opened)
            tcsResponse.TrySetException(new Exception("Error opening Safari"));
#endif
				return await tcsResponse.Task;
			}
		}


		void ClearCookies()
		{
			NSUrlCache.SharedCache.RemoveAllCachedResponses();

#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				WKWebsiteDataStore.DefaultDataStore.HttpCookieStore.GetAllCookies((cookies) =>
				{
					foreach (var cookie in cookies)
					{
#pragma warning disable CA1416 // Known false positive with lambda, here we can also assert the version
						WKWebsiteDataStore.DefaultDataStore.HttpCookieStore.DeleteCookie(cookie, null);
#pragma warning restore CA1416
					}
				});
			}
#endif
		}

		public bool OpenUrlCallback(Uri uri)
		{
			// If we aren't waiting on a task, don't handle the url
			if (tcsResponse?.Task?.IsCompleted ?? true)
				return false;

			try
			{
				// If we can't handle the url, don't
				if (!WebUtils.CanHandleCallback(redirectUri, uri))
					return false;

				currentViewController?.DismissViewControllerAsync(true);
				currentViewController = null;

				tcsResponse.TrySetResult(new WebAuthenticatorResult(uri, currentOptions?.ResponseDecoder));
				return true;
			}
			catch (Exception ex)
			{
				// TODO change this to ILogger?
				Console.WriteLine(ex);
			}
			return false;
		}

		static bool VerifyHasUrlSchemeOrDoesntRequire(string scheme)
		{
			// iOS11+ uses sfAuthenticationSession which handles its own url routing
			if (OperatingSystem.IsIOSVersionAtLeast(11, 0) || OperatingSystem.IsTvOSVersionAtLeast(11, 0))
				return true;

			return AppInfoImplementation.VerifyHasUrlScheme(scheme);
		}

#if __IOS__
		class NativeSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
		{
			public Action<SFSafariViewController> DidFinishHandler { get; set; }

			public override void DidFinish(SFSafariViewController controller) =>
				DidFinishHandler?.Invoke(controller);
		}

		class ContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
		{
			public ContextProvider(UIWindow window) =>
				Window = window;

			public readonly UIWindow Window;

			[Export("presentationAnchorForWebAuthenticationSession:")]
			public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
				=> Window;
		}
#endif
	}
}
