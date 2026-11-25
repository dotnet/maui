using System;
using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported =>
			true;

		public async void Open(string number)
		{
			ValidateOpen(number);

			// Preserve telephone URI syntax such as + and ;ext= while escaping URI delimiters.
			var escapedNumber = number
				.Replace("%", "%25", StringComparison.Ordinal)
				.Replace("#", "%23", StringComparison.Ordinal)
				.Replace("?", "%3F", StringComparison.Ordinal);

			// PhoneCallManager can be present without opening a dialer on desktop Windows.
			if (!await Launcher.OpenAsync($"tel:{escapedNumber}"))
			{
				Trace.WriteLine("Unable to open the phone dialer using the registered tel: handler.");
			}
		}
	}
}
