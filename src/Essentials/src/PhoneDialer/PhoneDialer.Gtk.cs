namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported => false;

		public void Open(string number) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}