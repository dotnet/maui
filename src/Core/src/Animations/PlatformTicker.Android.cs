using System;
using Android.Animation;
using Java.Interop;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker, IDisposable, IEnergySaverListener
	{
		readonly IEnergySaverListenerManager _manager;
		readonly ValueAnimator _val;
		bool _disposedValue;
		readonly DurationScaleListener? _durationScaleListener;

		/// <summary>
		/// Creates a new Android <see cref="PlatformTicker"/> object. 
		/// </summary>
		/// <param name="manager">Reference to an <see cref="IEnergySaverListenerManager"/> object to determine the energy saving status of the device.</param>
		public PlatformTicker(IEnergySaverListenerManager manager)
		{
			_manager = manager;
			_val = new ValueAnimator();
			_val.SetIntValues(0, 100); // avoid crash
			_val.RepeatCount = ValueAnimator.Infinite;
			_val.Update += (s, e) => Fire?.Invoke();

			CheckAnimationEnabledStatus();

			_manager.Add(this);

			if (OperatingSystem.IsAndroidVersionAtLeast(33))
			{
				_durationScaleListener = new DurationScaleListener(CheckAnimationEnabledStatus);
				ValueAnimator.RegisterDurationScaleChangeListener(_durationScaleListener);
			}
		}

		/// <inheritdoc/>
		public override bool IsRunning => _val.IsStarted;

		/// <inheritdoc/>
		public override void Start() => _val.Start();

		/// <inheritdoc/>
		public override void Stop() => _val.Cancel();

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_manager.Remove(this);

					if (OperatingSystem.IsAndroidVersionAtLeast(33) && _durationScaleListener != null)
					{
						ValueAnimator.UnregisterDurationScaleChangeListener(_durationScaleListener);
					}
				}

				_disposedValue = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void IEnergySaverListener.OnStatusUpdated(bool energySaverEnabled)
		{
			// Moving in and out of power saving mode may enable/disable animations, depending on
			// some other settings; we should check 
			SystemEnabled = AreAnimationsEnabled();
		}

		internal void CheckAnimationEnabledStatus()
		{
			SystemEnabled = AreAnimationsEnabled();
		}

		static bool AreAnimationsEnabled()
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				// For more recent API levels, we can just check this method and be done with it
				// https://developer.android.com/reference/android/animation/ValueAnimator#areAnimatorsEnabled()
				return ValueAnimator.AreAnimatorsEnabled();
			}

			if (OperatingSystem.IsAndroidVersionAtLeast(21))
			{
				// For API levels which support power saving but not AreAnimatorsEnabled, we can check the
				// power save mode; for these API levels, power saving == ON will mean that animations are disabled

				return Devices.Battery.EnergySaverStatus switch
				{
					Devices.EnergySaverStatus.On => false,
					_ => true,
				};
			}

			// We don't support anything below 21
			return false;
		}

		class DurationScaleListener : Java.Lang.Object, ValueAnimator.IDurationScaleChangeListener
		{
			readonly Action _check;

			public DurationScaleListener(Action check) 
			{
				_check = check;
			}

			public void OnChanged(float scale)
			{
				_check.Invoke();
			}
		}
	}
}