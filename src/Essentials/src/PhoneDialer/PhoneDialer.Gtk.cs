namespace Microsoft.Maui.Essentials
{
	public static partial class PhoneDialer
	{
		internal static bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformOpen(string number) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
