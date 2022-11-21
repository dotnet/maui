#nullable enable
using System;
using Android.App;
using Android.Content;
#if __ANDROID_26__
using Android.OS;
using Microsoft.Maui.ApplicationModel;
#endif

namespace Microsoft.Maui.Devices
{
	partial class VibrationImplementation : IVibration
	{
		static VibratorManager? VibratorManager =>
			OperatingSystem.IsAndroidVersionAtLeast(31)
				? Application.Context.GetSystemService(Context.VibratorManagerService) as VibratorManager
				: null;

		static Vibrator? VibratorManagerVibrator =>
			OperatingSystem.IsAndroidVersionAtLeast(31)
				? VibratorManager?.DefaultVibrator
				: null;

		static Vibrator? VibratorServiceVibrator =>
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CA1422 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
			Application.Context.GetSystemService(Context.VibratorService) as Vibrator;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS0618 // Type or member is obsolete

		static Vibrator? vibrator;

		static Vibrator? Vibrator =>
			vibrator ??= (VibratorManagerVibrator ?? VibratorServiceVibrator);

		public bool IsSupported => true;

		void PlatformVibrate() =>
			PlatformVibrate(TimeSpan.FromMilliseconds(500));

		void PlatformVibrate(TimeSpan duration)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			var time = (long)duration.TotalMilliseconds;
#if __ANDROID_26__
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				Vibrator?.Vibrate(VibrationEffect.CreateOneShot(time, VibrationEffect.DefaultAmplitude));
			}
			else
#endif
			{
#pragma warning disable CS0618 // Type or member is obsolete
				Vibrator?.Vibrate(time);
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		void PlatformCancel()
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			Vibrator?.Cancel();
		}
	}
}
