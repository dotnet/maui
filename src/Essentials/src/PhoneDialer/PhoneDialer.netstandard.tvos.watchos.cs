namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class PhoneDialerImplementation : IPhoneDialer
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Open(string number) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
