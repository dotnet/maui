using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Essentials
{
    public static partial class PhoneDialer
    {
        internal static bool IsSupported =>
             ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager");

        static void PlatformOpen(string number)
        {
            ValidateOpen(number);

            PhoneCallManager.ShowPhoneCallUI(number, string.Empty);
        }
    }
}
