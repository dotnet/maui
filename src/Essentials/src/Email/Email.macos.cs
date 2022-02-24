using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
			MainThread.InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString("mailto:")) != null);

		public Task ComposeAsync(EmailMessage message)
		{
			var url = GetMailToUri(message);

			using var nsurl = NSUrl.FromString(url);
			NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
			return Task.CompletedTask;
		}
	}
}
