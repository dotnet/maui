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
            var firstNumber = message.Recipients?.FirstOrDefault();
            var uri = $"sms:{firstNumber}";
            if (!string.IsNullOrEmpty(message?.Body))
                uri += "&body=" + Uri.EscapeDataString(message.Body);

            var nsurl = NSUrl.FromString(uri);
            NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
            return Task.CompletedTask;
        }
    }
}
