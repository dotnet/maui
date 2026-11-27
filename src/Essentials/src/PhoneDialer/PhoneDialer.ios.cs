using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
#pragma warning disable CA1422 // CanOpenUrl is obsolete on Mac Catalyst 27+, but IsSupported must answer without opening a URL
		public bool IsSupported => UIApplication.SharedApplication.CanOpenUrl(CreateNsUrl(new string('0', 10)));
#pragma warning restore CA1422

		public void Open(string number)
		{
			ValidateOpen(number);

			var nsUrl = CreateNsUrl(number);
			Launcher.Default.OpenAsync(nsUrl);
		}

		static NSUrl CreateNsUrl(string number) => new NSUrl(new Uri($"tel:{number}").AbsoluteUri);
	}
}
