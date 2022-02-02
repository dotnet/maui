using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported =>
			 ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager");

		public void PlatformOpen(string number)
		{
			PhoneDialer.ValidateOpen(number);

			PhoneCallManager.ShowPhoneCallUI(number, string.Empty);
		}
	}
}
