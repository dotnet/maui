using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Java.Net;
using Java.Util;
using Uri = Android.Net.Uri;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class PhoneDialerImplementation : IPhoneDialer
	{
		const string intentCheck = "00000000000";

		public bool IsSupported
		{
			get
			{
				var dialIntent = ResolveDialIntent(intentCheck);
				return Platform.IsIntentSupported(dialIntent);
			}
		}

		public void Open(string number)
		{
			ValidateOpen(number);

			var phoneNumber = string.Empty;
			if (Platform.HasApiLevelN)
				phoneNumber = PhoneNumberUtils.FormatNumber(number, Java.Util.Locale.GetDefault(Java.Util.Locale.Category.Format).Country) ?? phoneNumber;
			else
				phoneNumber = PhoneNumberUtils.FormatNumber(number, Java.Util.Locale.Default.Country) ?? phoneNumber;

			// if we are an extension then we need to encode
			if (phoneNumber.Contains(',') || phoneNumber.Contains(';'))
				phoneNumber = URLEncoder.Encode(phoneNumber, "UTF-8") ?? phoneNumber;

			var dialIntent = ResolveDialIntent(phoneNumber);

			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			if (Platform.HasApiLevelN)
				flags |= ActivityFlags.LaunchAdjacent;
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
