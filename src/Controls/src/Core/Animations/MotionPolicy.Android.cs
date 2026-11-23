#nullable enable
using System;
using Android.Animation;
using Android.OS;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class PlatformMotionPolicy
	{
		DurationScaleListener? _listener;

		// Never Settings.Global.animator_duration_scale: under a battery saver that disables animation it still
		// reads 1 while the effective scale is 0.
		partial void ReadReducedMotionCore(ref bool reduced)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
				reduced = !ValueAnimator.AreAnimatorsEnabled();
		}

		partial void ReadDurationScaleCore(ref double scale)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(33))
			{
				var value = ValueAnimator.DurationScale;
				if (value > 0)
					scale = value;
			}
		}

		partial void StartObserving()
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(33) || _listener is not null)
				return;

			_listener = new DurationScaleListener(this);
			ValueAnimator.RegisterDurationScaleChangeListener(_listener);
		}

		partial void StopObserving()
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(33) || _listener is null)
				return;

			ValueAnimator.UnregisterDurationScaleChangeListener(_listener);
			_listener = null;
		}

		sealed class DurationScaleListener : Java.Lang.Object, ValueAnimator.IDurationScaleChangeListener
		{
			readonly PlatformMotionPolicy _policy;
			readonly Handler _main = new(Looper.MainLooper!);

			public DurationScaleListener(PlatformMotionPolicy policy) => _policy = policy;

			// Called on a binder thread; runs are mutated on the UI thread only.
			public void OnChanged(float scale) => _main.Post(_policy.RaiseChanged);
		}
	}
}
