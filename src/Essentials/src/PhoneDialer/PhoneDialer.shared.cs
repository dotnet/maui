using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PhoneDialer']/Docs" />
	public static partial class PhoneDialer
	{
		internal static void ValidateOpen(string number)
		{
			if (string.IsNullOrWhiteSpace(number))
				throw new ArgumentNullException(nameof(number));

			if (!IsSupported)
				throw new FeatureNotSupportedException();
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="//Member[@MemberName='Open']/Docs" />
		public static void Open(string number)
			=> PlatformOpen(number);
	}
}
