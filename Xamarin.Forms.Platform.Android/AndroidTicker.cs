using System;
using Android.Animation;
using Android.Content;
using Android.OS;
using Xamarin.Forms.Internals;
using GlobalSettings = Android.Provider.Settings.Global;
using SystemSettings = Android.Provider.Settings.System;

namespace Xamarin.Forms.Platform.Android
{
	internal class AndroidTicker : Ticker, IDisposable
	{
		ValueAnimator _val;
		bool _animatorEnabled;

		public AndroidTicker()
		{
			_val = new ValueAnimator();
			_val.SetIntValues(0, 100); // avoid crash
			_val.RepeatCount = ValueAnimator.Infinite;
			_val.Update += OnValOnUpdate;
			_animatorEnabled = IsAnimatorEnabled();
			CheckAnimationEnabledStatus();
		}

		public override bool SystemEnabled => _animatorEnabled;

		internal void CheckAnimationEnabledStatus()
		{
			var animatorEnabled = IsAnimatorEnabled();

			if (animatorEnabled != _animatorEnabled)
			{
				_animatorEnabled = animatorEnabled;

				// Notify the ticker that this value has changed, so it can manage animations in progress
				OnSystemEnabledChanged();
			}
		}

		static bool IsAnimatorEnabled()
		{
			if (Forms.IsOreoOrNewer)
			{
				// For more recent API levels, we can just check this method and be done with it
				return ValueAnimator.AreAnimatorsEnabled();
			}

			if (Forms.IsLollipopOrNewer)
			{
				// For API levels which support power saving but not AreAnimatorsEnabled, we can check the
				// power save mode; for these API levels, power saving == ON will mean that animations are disabled
				var powerManager = (PowerManager)Forms.ApplicationContext.GetSystemService(Context.PowerService);
				if (powerManager.IsPowerSaveMode)
				{
					return false;
				}
			}

			// If we're not in power save mode (or don't support it), we still need to check the AnimatorDurationScale;
			// animations might be disabled by developer mode

			var resolver = global::Android.App.Application.Context?.ContentResolver;
			if (resolver == null)
			{
				return false;
			}

			float animationScale;

			if (Forms.IsJellyBeanMr1OrNewer)
			{
				animationScale = GlobalSettings.GetFloat(resolver, GlobalSettings.AnimatorDurationScale, 1);
			}
			else
			{
#pragma warning disable 0618
				animationScale = SystemSettings.GetFloat(resolver, SystemSettings.AnimatorDurationScale, 1);
#pragma warning restore 0618
			}

			return animationScale > 0;
		}

		public void Dispose()
		{
			if (_val != null)
			{
				_val.Update -= OnValOnUpdate;
				_val.Dispose();
			}
			_val = null;
		}

		protected override void DisableTimer()
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(new Action(() =>
				{
					_val?.Cancel();
				}));
			}
			else
			{
				_val?.Cancel();
			}
		}

		protected override void EnableTimer()
		{
			_val?.Start();
		}

		void OnValOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
		{
			SendSignals();
		}
	}
}