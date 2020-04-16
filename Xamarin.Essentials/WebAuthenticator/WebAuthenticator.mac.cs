using System;
using System.Threading.Tasks;
using AppKit;
using AuthenticationServices;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class WebAuthenticator
    {
        static readonly CallBackHelper callbackHelper = new CallBackHelper();

        static TaskCompletionSource<WebAuthenticatorResult> tcsResponse;
        static Uri redirectUri;

        static WebAuthenticator()
        {
            callbackHelper.Register();
        }

        internal static async Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
        {
            // Cancel any previous task that's still pending
            if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
                tcsResponse.TrySetCanceled();

            tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
            redirectUri = callbackUrl;

            try
            {
                var scheme = redirectUri.Scheme;

                if (!AppInfo.VerifyHasUrlScheme(scheme))
                {
                    tcsResponse.TrySetException(new InvalidOperationException("You must register your URL Scheme handler in your app's Info.plist!"));
                    return await tcsResponse.Task;
                }

                if (DeviceInfo.Version >= new Version(10, 15))
                {
                    static void AuthSessionCallback(NSUrl cbUrl, NSError error)
                    {
                        if (error == null)
                            OpenUrl(cbUrl);
                        else
                            tcsResponse.TrySetException(new NSErrorException(error));
                    }

                    var was = new ASWebAuthenticationSession(new NSUrl(url.OriginalString), scheme, AuthSessionCallback);

                    var ctx = new ContextProvider(Platform.GetCurrentWindow());
                    was.PresentationContextProvider = ctx;

                    was.Start();
                    return await tcsResponse.Task;
                }

                var opened = NSWorkspace.SharedWorkspace.OpenUrl(url);
                if (!opened)
                    tcsResponse.TrySetException(new EssentialsException("Error opening Safari"));
            }
            catch (Exception ex)
            {
                tcsResponse.TrySetException(ex);
            }

            return await tcsResponse.Task;
        }

        static bool OpenUrl(NSUrl uri)
        {
            // If we aren't waiting on a task, don't handle the url
            if (tcsResponse?.Task?.IsCompleted ?? true)
                return false;

            try
            {
                // If we can't handle the url, don't
                if (!WebUtils.CanHandleCallback(redirectUri, uri))
                    return false;

                tcsResponse.TrySetResult(new WebAuthenticatorResult(uri));
                return true;
            }
            catch (Exception ex)
            {
                tcsResponse.TrySetException(ex);
                return false;
            }
        }

        class ContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            public ContextProvider(NSWindow window) =>
                Window = window;

            public NSWindow Window { get; }

            public NSWindow GetPresentationAnchor(ASWebAuthenticationSession session)
                => Window;
        }

        class CallBackHelper : NSObject
        {
            public void Register()
            {
                NSAppleEventManager.SharedAppleEventManager.SetEventHandler(
                    this,
                    new ObjCRuntime.Selector("handleAppleEvent:withReplyEvent:"),
                    AEEventClass.Internet,
                    AEEventID.GetUrl);
            }

            [Export("handleAppleEvent:withReplyEvent:")]
            public void HandleAppleEvent(NSAppleEventDescriptor evt, NSAppleEventDescriptor replyEvt)
            {
                var url = evt.ParamDescriptorForKeyword(DirectObject).StringValue;
                OpenUrl(new NSUrl(url));
            }

            static uint GetDescriptor(string s) =>
                (uint)(s[0] << 24 | s[1] << 16 | s[2] << 8 | s[3]);

            static uint DirectObject => GetDescriptor("----");
        }
    }
}
