using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class PhoneDialerImplementation : IPhoneDialer
	{
		const string noNetworkProviderCode = "65535";

		public bool IsSupported => UIApplication.SharedApplication.CanOpenUrl(CreateNsUrl(new string('0', 10)));

		public async void Open(string number)
		{
			ValidateOpen(number);

			var nsUrl = CreateNsUrl(number);
			await Launcher.PlatformOpenAsync(nsUrl);
		}

		static NSUrl CreateNsUrl(string number) => new NSUrl(new Uri($"tel:{number}").AbsoluteUri);
	}
}
