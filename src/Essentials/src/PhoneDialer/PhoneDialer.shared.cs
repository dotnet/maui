#nullable enable
using System;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IPhoneDialer
	{
		bool IsSupported { get; }

		void Open(string number);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PhoneDialer']/Docs" />
	public static class PhoneDialer
	{
		public static bool IsSupported => Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/PhoneDialer.xml" path="//Member[@MemberName='Open']/Docs" />
		public static void Open(string number)
			=> Current.Open(number);

		static IPhoneDialer? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPhoneDialer Current =>
			currentImplementation ??= new PhoneDialerImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IPhoneDialer? implementation) =>
			currentImplementation = implementation;
	}
}

namespace Microsoft.Maui.Essentials.Implementations
{
	partial class PhoneDialerImplementation
	{
		internal void ValidateOpen(string number)
		{
			if (string.IsNullOrWhiteSpace(number))
				throw new ArgumentNullException(nameof(number));

			if (!IsSupported)
				throw new FeatureNotSupportedException();
		}
	}
}
