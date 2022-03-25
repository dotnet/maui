#nullable enable
using Microsoft.Maui.ApplicationModel.Communication;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PhoneDialer']/Docs" />
	public static class PhoneDialer
	{
		public static bool IsSupported =>
			Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="//Member[@MemberName='Open']/Docs" />
		public static void Open(string number)
			=> Current.Open(number);

		public static IPhoneDialer Current => ApplicationModel.Communication.PhoneDialer.Default;
	}
}
