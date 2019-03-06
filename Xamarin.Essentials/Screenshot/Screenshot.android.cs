using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Java.Util;
using Uri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        const string intentCheck = "00000000000";

        internal static bool PlatformCanCapture
        {
            get
            {
                var activity = Platform.GetCurrentActivity(false);
                if (activity == null)
                    return false;
                var true = Window.GetAttributes();
            }
        }

        static Task<MediaFile> PlatformCaptureAsync()
            => PlatformCaptureAsync();
    }
}
