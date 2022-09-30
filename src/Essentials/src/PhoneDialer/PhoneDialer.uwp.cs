using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported =>
			true;

		public async void Open(string number)
		{
			ValidateOpen(number);

			if (ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager"))
				PhoneCallManager.ShowPhoneCallUI(number, string.Empty);
			else
				await Launcher.OpenAsync($"tel:{number}");
		}
	}
}
