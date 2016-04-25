using System;
using Android.Animation;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class AndroidTicker : Ticker, IDisposable
	{
		ValueAnimator _val;

		public AndroidTicker()
		{
			_val = new ValueAnimator();
			_val.SetIntValues(0, 100); // avoid crash
			_val.RepeatCount = ValueAnimator.Infinite;
			_val.Update += OnValOnUpdate;
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
			_val?.Cancel();
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