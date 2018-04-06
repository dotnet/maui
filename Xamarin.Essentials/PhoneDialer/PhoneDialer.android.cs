using System;
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

        internal static bool IsSupported
        {
            get
            {
                var packageManager = Application.Context.PackageManager;
                var dialIntent = ResolveDialIntent(intentCheck);
                return dialIntent.ResolveActivity(packageManager) != null;
            }
        }

        public static void Open(string number)
        {
            ValidateOpen(number);

            var phoneNumber = string.Empty;
            if (Platform.HasApiLevel(BuildVersionCodes.N))
            {
                phoneNumber = PhoneNumberUtils.FormatNumber(number, Locale.GetDefault(Locale.Category.Format).Country);
            }
            else if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
            {
                phoneNumber = PhoneNumberUtils.FormatNumber(number, Locale.Default.Country);
            }
            else
            {
#pragma warning disable CS0618
                phoneNumber = PhoneNumberUtils.FormatNumber(number);
#pragma warning restore CS0618
            }

            var dialIntent = ResolveDialIntent(phoneNumber)
                .SetFlags(ActivityFlags.ClearTop)
                .SetFlags(ActivityFlags.NewTask);

            Application.Context.StartActivity(dialIntent);
        }

        static Intent ResolveDialIntent(string number)
        {
            var telUri = Uri.Parse($"tel:{number}");
            return new Intent(Intent.ActionDial, telUri);
        }
    }
}
