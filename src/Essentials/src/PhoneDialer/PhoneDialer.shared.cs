#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <summary>
	/// The PhoneDialer API enables an application to open a phone number in the dialer.
	/// </summary>
	public interface IPhoneDialer
	{
		/// <summary>
		/// Gets a value indicating whether using the phone dialer is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Open the phone dialer to a specific phone number.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="ArgumentNullException"/> if <paramref name="number"/> is not valid.
		/// Will throw <see cref="FeatureNotSupportedException"/> if making phone calls is not supported on the device.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="number"/> is not valid.</exception>
		/// <exception cref="FeatureNotSupportedException">Thrown if making phone calls is not supported on the device.</exception>
		/// <param name="number">Phone number to initialize the dialer with.</param>
		void Open(string number);
	}

	/// <summary>
	/// The PhoneDialer API enables an application to open a phone number in the dialer.
	/// </summary>
	public static class PhoneDialer
	{
		/// <summary>
		/// Gets a value indicating whether using the phone dialer is supported on this device.
		/// </summary>
		public static bool IsSupported =>
			Default.IsSupported;

		/// <summary>
		/// Open the phone dialer to a specific phone number.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="ArgumentNullException"/> if <paramref name="number"/> is not valid.
		/// Will throw <see cref="FeatureNotSupportedException"/> if making phone calls is not supported on the device.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="number"/> is not valid.</exception>
		/// <exception cref="FeatureNotSupportedException">Thrown if making phone calls is not supported on the device.</exception>
		/// <param name="number">Phone number to initialize the dialer with.</param>
		public static void Open(string number)
			=> Default.Open(number);

		static IPhoneDialer? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
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
