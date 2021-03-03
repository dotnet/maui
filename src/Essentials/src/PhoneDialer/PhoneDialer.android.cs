using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Java.Net;
using Java.Util;
using Uri = Android.Net.Uri;

namespace Microsoft.Maui.Essentials
{
	public static partial class PhoneDialer
	{
		const string intentCheck = "00000000000";

		internal static bool IsSupported
		{
			get
			{
				var dialIntent = ResolveDialIntent(intentCheck);
				return Platform.IsIntentSupported(dialIntent);
			}
		}

		static void PlatformOpen(string number)
		{
			ValidateOpen(number);

			var phoneNumber = string.Empty;
#if __ANDROID_24__
            if (Platform.HasApiLevelN)
                phoneNumber = PhoneNumberUtils.FormatNumber(number, Java.Util.Locale.GetDefault(Java.Util.Locale.Category.Format).Country);
            else if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
#else
			if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
#endif

				phoneNumber = PhoneNumberUtils.FormatNumber(number, Java.Util.Locale.Default.Country);
			else
#pragma warning disable CS0618
				phoneNumber = PhoneNumberUtils.FormatNumber(number);
#pragma warning restore CS0618

			// if we are an extension then we need to encode
			if (phoneNumber.Contains(',') || phoneNumber.Contains(';'))
				phoneNumber = URLEncoder.Encode(phoneNumber, "UTF-8");

			var dialIntent = ResolveDialIntent(phoneNumber);

			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
            if (Platform.HasApiLevelN)
                flags |= ActivityFlags.LaunchAdjacent;
#endif
			dialIntent.SetFlags(flags);

			Platform.AppContext.StartActivity(dialIntent);
		}

		static Intent ResolveDialIntent(string number)
		{
			var telUri = Uri.Parse($"tel:{number}");
			return new Intent(Intent.ActionDial, telUri);
		}
	}
}
