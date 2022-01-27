namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PhoneDialer']/Docs" />
	public static partial class PhoneDialer
	{
		internal static bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformOpen(string number) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
