using System.Maui.Platform;
using Android.Animation;
using Android.Content;
using Android.OS;

namespace System.Maui.Internals
{
	internal class NativeTicker : Ticker, IDisposable
	{
		ValueAnimator _val;
		bool _energySaveModeDisabled;
		readonly bool _animatorEnabled;

		public NativeTicker()
		{
			_val = new ValueAnimator();
			_val.SetIntValues(0, 100); // avoid crash
			_val.RepeatCount = ValueAnimator.Infinite;
			_val.Update += OnValOnUpdate;
			_animatorEnabled = IsAnimatorEnabled();
			CheckPowerSaveModeStatus();
		}

		public override bool SystemEnabled => _energySaveModeDisabled && _animatorEnabled;

		internal void CheckPowerSaveModeStatus()
		{
			// Android disables animations when it's in power save mode
			// So we need to keep track of whether we're in that mode and handle animations accordingly
			// We can't just check ValueAnimator.AreAnimationsEnabled() because there's no event for that, and it's
			// only supported on API >= 26
			
			if (!NativeVersion.Supports(NativeApis.PowerSaveMode))
			{
				_energySaveModeDisabled = true;
				return;
			}

			// TODO Figure out what Forms.ApplicationContext will look like in this brave new world
			//var powerManager = (PowerManager)Forms.ApplicationContext.GetSystemService(Context.PowerService);

			//var powerSaveOn = powerManager.IsPowerSaveMode;

			//// If power saver is active, then animations will not run
			//_energySaveModeDisabled = !powerSaveOn;

			//// Notify the ticker that this value has changed, so it can manage animations in progress
			//OnSystemEnabledChanged();
		}

		static bool IsAnimatorEnabled()
		{
			var resolver = global::Android.App.Application.Context?.ContentResolver;
			if (resolver == null)
			{
				return false;
			}

			float animationScale;

			if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
			{
				animationScale = global::Android.Provider.Settings.Global.GetFloat(resolver, global::Android.Provider.Settings.Global.AnimatorDurationScale, 1);
			}
			else
			{
#pragma warning disable 0618
				animationScale = global::Android.Provider.Settings.System.GetFloat(resolver, global::Android.Provider.Settings.System.AnimatorDurationScale, 1);
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
			//TODO:Bring back
			//if (Device.IsInvokeRequired)
			//{
			//	Device.BeginInvokeOnMainThread(new Action(() =>
			//	{
			//		_val?.Cancel();
			//	}));
			//}
			//else
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
