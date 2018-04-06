using System;
using CoreTelephony;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        const string noNetworkProviderCode = "65535";

        internal static bool IsSupported
        {
            get
            {
                var isDialerInstalled = UIApplication.SharedApplication.CanOpenUrl(CreateNsUrl(new string('0', 10)));
                if (!isDialerInstalled)
                {
                    return false;
                }

                using (var netInfo = new CTTelephonyNetworkInfo())
                {
                    var mnc = netInfo.SubscriberCellularProvider?.MobileNetworkCode;
                    return !string.IsNullOrEmpty(mnc) && mnc != noNetworkProviderCode;
                }
            }
        }

        public static void Open(string number)
        {
            ValidateOpen(number);

            var nsUrl = CreateNsUrl(number);
            UIApplication.SharedApplication.OpenUrl(nsUrl);
        }

        static NSUrl CreateNsUrl(string number) => new NSUrl(new Uri($"tel:{number}").AbsoluteUri);
    }
}
