using System;
using CoreTelephony;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        const string noNetworkProviderCode = "65535";

        internal static bool IsSupported => UIApplication.SharedApplication.CanOpenUrl(CreateNsUrl(new string('0', 10)));

        static async void PlatformOpen(string number)
        {
            ValidateOpen(number);

            var nsUrl = CreateNsUrl(number);
            await Launcher.PlatformOpenAsync(nsUrl);
        }

        static NSUrl CreateNsUrl(string number) => new NSUrl(new Uri($"tel:{number}").AbsoluteUri);
    }
}
