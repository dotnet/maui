#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	public interface IPhoneDialer
	{
		bool IsSupported { get; }

		void Open(string number);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PhoneDialer']/Docs" />
	public static class PhoneDialer
	{
		public static bool IsSupported =>
			Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="//Member[@MemberName='Open']/Docs" />
		public static void Open(string number)
			=> Current.Open(number);

		public static IPhoneDialer Current => ApplicationModel.Communication.PhoneDialer.Default;

		static IPhoneDialer? defaultImplementation;

		public static IPhoneDialer Default =>
			defaultImplementation ??= new PhoneDialerImplementation();

		internal static void SetDefault(IPhoneDialer? implementation) =>
			defaultImplementation = implementation;
	}

	partial class PhoneDialerImplementation : IPhoneDialer
	{
		void ValidateOpen(string number)
		{
			if (string.IsNullOrWhiteSpace(number))
				throw new ArgumentNullException(nameof(number));

			if (!IsSupported)
				throw new FeatureNotSupportedException();
		}
	}
}
