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

			await Launcher.OpenAsync($"tel:{number}");
		}
	}
}
