using AppKit;
using Foundation;

namespace Microsoft.Maui.Essentials
{
	public static partial class PhoneDialer
	{
		internal static bool IsSupported =>
			MainThread.InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString($"tel:0000000000")) != null);

		static void PlatformOpen(string number)
		{
			ValidateOpen(number);

			var nsurl = NSUrl.FromString($"tel:{number}");
			NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
		}
	}
}
