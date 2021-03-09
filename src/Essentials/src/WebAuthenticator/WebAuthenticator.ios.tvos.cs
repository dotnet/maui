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
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class WebAuthenticator
	{
#if __IOS__
        [System.Runtime.InteropServices.DllImport(ObjCRuntime.Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Required for iOS Export")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Required for iOS Export")]
        static extern void void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        const int asWebAuthenticationSessionErrorCodeCanceledLogin = 1;
        const string asWebAuthenticationSessionErrorDomain = "com.apple.AuthenticationServices.WebAuthenticationSession";

        const int sfAuthenticationErrorCanceledLogin = 1;
        const string sfAuthenticationErrorDomain = "com.apple.SafariServices.Authentication";
#endif

		static TaskCompletionSource<WebAuthenticatorResult> tcsResponse;
		static UIViewController currentViewController;
		static Uri redirectUri;

#if __IOS__
        static ASWebAuthenticationSession was;
        static SFAuthenticationSession sf;
#endif

		internal static async Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
		{
			if (!VerifyHasUrlSchemeOrDoesntRequire(callbackUrl.Scheme))
				throw new InvalidOperationException("You must register your URL Scheme handler in your app's Info.plist.");

			// Cancel any previous task that's still pending
			if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
				tcsResponse.TrySetCanceled();

			tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
			redirectUri = callbackUrl;
			var scheme = redirectUri.Scheme;

#if __IOS__
            static void AuthSessionCallback(NSUrl cbUrl, NSError error)
            {
                if (error == null)
                    OpenUrl(cbUrl);
                else if (error.Domain == asWebAuthenticationSessionErrorDomain && error.Code == asWebAuthenticationSessionErrorCodeCanceledLogin)
                    tcsResponse.TrySetCanceled();
                else if (error.Domain == sfAuthenticationErrorDomain && error.Code == sfAuthenticationErrorCanceledLogin)
                    tcsResponse.TrySetCanceled();
                else
                    tcsResponse.TrySetException(new NSErrorException(error));

                was = null;
                sf = null;
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
            {
                was = new ASWebAuthenticationSession(WebUtils.GetNativeUrl(url), scheme, AuthSessionCallback);

                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    var ctx = new ContextProvider(Platform.GetCurrentWindow());
                    void_objc_msgSend_IntPtr(was.Handle, ObjCRuntime.Selector.GetHandle("setPresentationContextProvider:"), ctx.Handle);
                }

                using (was)
                {
                    was.Start();
                    return await tcsResponse.Task;
                }
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                sf = new SFAuthenticationSession(WebUtils.GetNativeUrl(url), scheme, AuthSessionCallback);
                using (sf)
                {
                    sf.Start();
                    return await tcsResponse.Task;
                }
            }

            // This is only on iOS9+ but we only support 10+ in Essentials anyway
            var controller = new SFSafariViewController(WebUtils.GetNativeUrl(url), false)
            {
                Delegate = new NativeSFSafariViewControllerDelegate
                {
                    DidFinishHandler = (svc) =>
                    {
                        // Cancel our task if it wasn't already marked as completed
                        if (!(tcsResponse?.Task?.IsCompleted ?? true))
                            tcsResponse.TrySetCanceled();
                    }
                },
            };

            currentViewController = controller;
            await Platform.GetCurrentUIViewController().PresentViewControllerAsync(controller, true);
#else
			var opened = UIApplication.SharedApplication.OpenUrl(url);
			if (!opened)
				tcsResponse.TrySetException(new Exception("Error opening Safari"));
#endif

			return await tcsResponse.Task;
		}

		internal static bool OpenUrl(Uri uri)
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

				tcsResponse.TrySetResult(new WebAuthenticatorResult(uri));
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return false;
		}

		static bool VerifyHasUrlSchemeOrDoesntRequire(string scheme)
		{
			// iOS11+ uses sfAuthenticationSession which handles its own url routing
			if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
				return true;

			return AppInfo.VerifyHasUrlScheme(scheme);
		}

#if __IOS__
        class NativeSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
        {
            public Action<SFSafariViewController> DidFinishHandler { get; set; }

            public override void DidFinish(SFSafariViewController controller) =>
                DidFinishHandler?.Invoke(controller);
        }

        [ObjCRuntime.Adopts("ASWebAuthenticationPresentationContextProviding")]
        class ContextProvider : NSObject
        {
            public ContextProvider(UIWindow window) =>
                Window = window;

            public UIWindow Window { get; private set; }

            [Export("presentationAnchorForWebAuthenticationSession:")]
            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
                => Window;
        }
#endif
	}
}
