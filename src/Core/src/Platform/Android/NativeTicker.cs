using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Content;
using Android.OS;
using Microsoft.Maui;
using Microsoft.Maui.Internal;

namespace Microsoft.Maui.Animations
{
	public class NativeTicker : Ticker
	{
		static WeakList<NativeTicker> tickers = new WeakList<NativeTicker>();
		public static void CheckPowerSaveModeStatus()
		{
			tickers.ForEach((t) => t.checkPowerSaveModeStatus());
		}
		readonly IMauiContext MauiContext;
		ValueAnimator _val;
		bool _systemEnabled;
		public NativeTicker(IMauiContext mauiContext)
		{
			MauiContext = mauiContext;
			_val = new ValueAnimator();
			_val.SetIntValues(0, 100); // avoid crash
			_val.RepeatCount = ValueAnimator.Infinite;
			_val.Update += (s, e) => Fire?.Invoke();
			checkPowerSaveModeStatus();
			tickers.Add(this);
		}
		~NativeTicker()
		{
			tickers.Remove(this);
		}

		internal void checkPowerSaveModeStatus()
		{
			// Android disables animations when it's in power save mode
			// So we need to keep track of whether we're in that mode and handle animations accordingly
			// We can't just check ValueAnimator.AreAnimationsEnabled() because there's no event for that, and it's
			// only supported on API >= 26
			var context = MauiContext.Context!;
			var powerManager = (PowerManager)context.GetSystemService(Context.PowerService)!;

			var powerSaveOn = powerManager.IsPowerSaveMode;

			// If power saver is active, then animations will not run
			_systemEnabled = !powerSaveOn;

		}

		public override bool IsRunning => _val.IsStarted;
		public override bool SystemEnabled { get => _systemEnabled; }
		public override void Start() =>  _val.Start();
		public override void Stop() =>  _val?.Cancel();
	}
}
