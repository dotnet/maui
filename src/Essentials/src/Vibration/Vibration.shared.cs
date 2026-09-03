#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// The Vibration API provides an easy way to make the device vibrate.
	/// </summary>
	public interface IVibration
	{
		/// <summary>
		/// Gets a value indicating whether vibration is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Vibrates the device for 500ms.
		/// </summary>
		void Vibrate();

		/// <summary>
		/// Vibrates the device for the specified time in the range [0, 5000]ms.
		/// </summary>
		/// <remarks>On iOS, the device will only vibrate for 500ms, regardless of the value specified.</remarks>
		/// <param name="duration">The time to vibrate for. This value will be ignored on iOS as it only supports a vibration of 500ms.</param>
		void Vibrate(TimeSpan duration);

		/// <summary>
		/// Cancel any current vibrations.
		/// </summary>
		void Cancel();
	}

	/// <summary>
	/// The Vibration API provides an easy way to make the device vibrate.
	/// </summary>
	/// <remarks>On Android make sure that the <c>android.permission.VIBRATE</c> permission is declared in the <c>AndroidManifest.xml</c> file.</remarks>
	public static class Vibration
	{
		/// <summary>
		/// Vibrates the device for 500ms.
		/// </summary>
		public static void Vibrate() =>
			Current.Vibrate();

		/// <summary>
		/// Vibrates the device for the specified number of milliseconds in the range [0, 5000]ms.
		/// </summary>
		/// <remarks>On iOS, the device will only vibrate for 500ms, regardless of the value specified.</remarks>
		/// <param name="duration">The number of milliseconds to vibrate for. This value will be ignored on iOS as it only supports a vibration of 500ms.</param>
		public static void Vibrate(double duration) =>
			Current.Vibrate(duration);

		/// <summary>
		/// Vibrates the device for the specified time in the range [0, 5000]ms.
		/// </summary>
		/// <remarks>On iOS, the device will only vibrate for 500ms, regardless of the value specified.</remarks>
		/// <param name="duration">The time to vibrate for. This value will be ignored on iOS as it only supports a vibration of 500ms.</param>
		public static void Vibrate(TimeSpan duration) =>
			Current.Vibrate(duration);

		/// <summary>
		/// Cancel any current vibrations.
		/// </summary>
		public static void Cancel() =>
			Current.Cancel();

		/// <summary>
		/// Gets a value indicating whether vibration is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		static IVibration Current => Devices.Vibration.Default;

		static IVibration? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IVibration Default =>
			defaultImplementation ??= new VibrationImplementation();

		internal static void SetDefault(IVibration? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="IVibration"/> APIs.
	/// </summary>
	public static class VibrationExtensions
	{
		/// <summary>
		/// Vibrates the device for the specified time in the range [0, 5000]ms.
		/// </summary>
		/// <param name="vibration">The object this method is invoked on.</param>
		/// <param name="duration">The time to vibrate for. This value will be ignored on iOS as it only supports a vibration of 500ms.</param>
		public static void Vibrate(this IVibration vibration, double duration) =>
			vibration.Vibrate(TimeSpan.FromMilliseconds(duration));
	}

	partial class VibrationImplementation : IVibration
	{
		public void Vibrate()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			PlatformVibrate();
		}

		public void Vibrate(TimeSpan duration)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (duration.TotalMilliseconds < 0)
				duration = TimeSpan.Zero;
			else if (duration.TotalSeconds > 5)
				duration = TimeSpan.FromSeconds(5);

			PlatformVibrate(duration);
		}

		public void Cancel()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			PlatformCancel();
		}
	}
}
