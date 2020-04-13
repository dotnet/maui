using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        internal static bool IsComposeSupported
        {
            get
            {
                var can = false;
                var url = NSUrl.FromString("mailto:");
                NSRunLoop.Main.InvokeOnMainThread(() => can = NSWorkspace.SharedWorkspace.UrlForApplication(url) != null);
                return can;
            }
        }

        static Task PlatformComposeAsync(EmailMessage message)
        {
            var url = GetMailToUri(message);

            var nsurl = NSUrl.FromString(url);
            NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
            return Task.CompletedTask;
        }
    }
}
