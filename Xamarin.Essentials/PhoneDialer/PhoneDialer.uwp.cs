using System;
using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        internal static bool IsSupported =>
             ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager");

        public static void Open(string number)
        {
            ValidateOpen(number);

            PhoneCallManager.ShowPhoneCallUI(number, string.Empty);
        }
    }
}
