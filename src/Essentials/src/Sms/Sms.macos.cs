using System;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class SmsImplementation : ISms
	{
		public bool IsComposeSupported =>
			MainThread.InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString("sms:")) != null);

		Task PlatformComposeAsync(SmsMessage message)
		{
			var recipients = string.Join(",", message.Recipients.Select(r => Uri.EscapeDataString(r)));

			var uri = $"sms:/open?addresses={recipients}";

			if (!string.IsNullOrEmpty(message?.Body))
				uri += "&body=" + Uri.EscapeDataString(message.Body);

			var nsurl = NSUrl.FromString(uri);
			NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
			return Task.CompletedTask;
		}
	}
}
