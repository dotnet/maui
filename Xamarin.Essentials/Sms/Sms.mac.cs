using System;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Sms
    {
        internal static bool IsComposeSupported =>
            MainThread.InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString("sms:")) != null);

        static Task PlatformComposeAsync(SmsMessage message)
        {
            var recipients = string.Join(",", message.Recipients.Select(r => System.Net.WebUtility.UrlEncode(r)));

            var uri = $"sms:/open?addresses={recipients}";

            if (!string.IsNullOrEmpty(message?.Body))
                uri += "&body=" + System.Net.WebUtility.UrlEncode(message.Body);

            var nsurl = NSUrl.FromString(uri);
            NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
            return Task.CompletedTask;
        }
    }
}
