using System;
using Android.App;
using Android.Content;
using Android.Telephony;
using Java.Net;
using Uri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
		const string intentCheck = "00000000000";

		public bool IsSupported
		{
			get
			{
				var dialIntent = ResolveDialIntent(intentCheck);
				return PlatformUtils.IsIntentSupported(dialIntent);
			}
		}

		public void Open(string number)
		{
			ValidateOpen(number);

			var phoneNumber = string.Empty;
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				phoneNumber = PhoneNumberUtils.FormatNumber(number, Java.Util.Locale.GetDefault(Java.Util.Locale.Category.Format).Country) ?? phoneNumber;
			else
				phoneNumber = PhoneNumberUtils.FormatNumber(number, Java.Util.Locale.Default.Country) ?? phoneNumber;

			// if we are an extension then we need to encode
			if (phoneNumber.Contains(',', StringComparison.Ordinal) || phoneNumber.Contains(';', StringComparison.Ordinal) || phoneNumber.Contains('#', StringComparison.Ordinal))
				phoneNumber = URLEncoder.Encode(phoneNumber, "UTF-8") ?? phoneNumber;

			var dialIntent = ResolveDialIntent(phoneNumber);

			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			dialIntent.SetFlags(flags);

			Application.Context.StartActivity(dialIntent);
		}

		static Intent ResolveDialIntent(string number)
		{
			var telUri = Uri.Parse($"tel:{number}");
			return new Intent(Intent.ActionDial, telUri);
		}
	}
}
