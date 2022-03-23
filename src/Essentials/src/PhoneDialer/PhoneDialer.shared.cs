#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	public interface IPhoneDialer
	{
		bool IsSupported { get; }

		void Open(string number);
	}

	public static class PhoneDialer
	{
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
