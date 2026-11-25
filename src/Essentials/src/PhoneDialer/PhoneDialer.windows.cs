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

			// PhoneCallManager can be present without opening a dialer on desktop Windows.
			if (!await Launcher.OpenAsync($"tel:{number}"))
			{
				Debug.WriteLine("Unable to open the phone dialer using the registered tel: handler.");
			}
		}
	}
}
