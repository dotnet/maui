using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
			MainThread.InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString("mailto:")) != null);

		Task PlatformComposeAsync(EmailMessage message)
		{
			var url = GetMailToUri(message);

			using var nsurl = NSUrl.FromString(url);
			NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
			return Task.CompletedTask;
		}
	}
}
