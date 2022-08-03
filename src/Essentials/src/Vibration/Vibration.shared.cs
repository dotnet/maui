#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	public interface IVibration
	{
		bool IsSupported { get; }

		void Vibrate();

		void Vibrate(TimeSpan duration);

		void Cancel();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Vibration']/Docs" />
	public static class Vibration
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][1]/Docs" />
		public static void Vibrate() =>
			Current.Vibrate();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][2]/Docs" />
		public static void Vibrate(double duration) =>
			Current.Vibrate(duration);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][3]/Docs" />
		public static void Vibrate(TimeSpan duration) =>
			Current.Vibrate(duration);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public static void Cancel() =>
			Current.Cancel();

		static IVibration Current => Devices.Vibration.Default;

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
