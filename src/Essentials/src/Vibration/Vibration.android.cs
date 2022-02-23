using System;
#if __ANDROID_26__
using Android;
using Android.OS;
#endif

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class VibrationImplementation : IVibration
	{
		public bool IsSupported => true;

		public void Vibrate() 
			=> Vibrate(TimeSpan.FromMilliseconds(500));

		public void Vibrate(double duration) 
			=> Vibrate(TimeSpan.FromMilliseconds(duration));

		public void Vibrate(TimeSpan duration)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			var time = (long)duration.TotalMilliseconds;
#if __ANDROID_26__
			if (Platform.HasApiLevelO)
			{
				Platform.Vibrator.Vibrate(VibrationEffect.CreateOneShot(time, VibrationEffect.DefaultAmplitude));
				return;
			}
#endif

#pragma warning disable CS0618 // Type or member is obsolete
			Platform.Vibrator.Vibrate(time);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public void Cancel()
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			Platform.Vibrator.Cancel();
		}
	}
}
