using AppKit;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported =>
			MainThread.InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString($"tel:0000000000")) != null);

		public void PlatformOpen(string number)
		{
			ValidateOpen(number);

			var nsurl = NSUrl.FromString($"tel:{number}");
			NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
		}
	}
}
