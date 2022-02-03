using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported =>
			ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager");

		public void Open(string number)
		{
			ValidateOpen(number);

			PhoneCallManager.ShowPhoneCallUI(number, string.Empty);
		}
	}
}
