#nullable enable
using System;
using Android.Animation;
using Android.OS;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class PlatformMotionPolicy
	{
		DurationScaleListener? _listener;
		Handler? _pollHandler;
		Java.Lang.IRunnable? _poll;
		bool _lastReduced;

		// Below API 33 Android has no duration-scale callback, so while runs observe the policy it is read twice a
		// second and Changed raised when it flips ("Remove animations", battery saver).
		const int PollMilliseconds = 500;

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
			if (OperatingSystem.IsAndroidVersionAtLeast(33))
			{
				if (_listener is not null)
					return;

				_listener = new DurationScaleListener(this);
				ValueAnimator.RegisterDurationScaleChangeListener(_listener);
				return;
			}

			if (!OperatingSystem.IsAndroidVersionAtLeast(26) || _poll is not null)
				return;

			_lastReduced = !ValueAnimator.AreAnimatorsEnabled();
			_pollHandler ??= new Handler(Looper.MainLooper!);
			_poll = new Java.Lang.Runnable(Poll);
			_pollHandler.PostDelayed(_poll, PollMilliseconds);
		}

		partial void StopObserving()
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(33))
			{
				if (_listener is null)
					return;

				ValueAnimator.UnregisterDurationScaleChangeListener(_listener);
				_listener = null;
				return;
			}

			if (_poll is null)
				return;

			_pollHandler?.RemoveCallbacks(_poll);
			_poll = null;
		}

		void Poll()
		{
			if (_poll is null || !OperatingSystem.IsAndroidVersionAtLeast(26))
				return;

			var reduced = !ValueAnimator.AreAnimatorsEnabled();
			if (reduced != _lastReduced)
			{
				_lastReduced = reduced;
				RaiseChanged();
			}

			if (_poll is not null)
				_pollHandler?.PostDelayed(_poll, PollMilliseconds);
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
