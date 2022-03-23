#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	public interface IVibration
	{
		bool IsSupported { get; }

		void Vibrate();

		void Vibrate(TimeSpan duration);

		void Cancel();
	}

	public static class Vibration
	{
		static IVibration? defaultImplementation;

		public static IVibration Default =>
			defaultImplementation ??= new VibrationImplementation();

		internal static void SetDefault(IVibration? implementation) =>
			defaultImplementation = implementation;
	}

	public static class VibrationExtensions
	{
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
