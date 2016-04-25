using System;
using System.Windows.Threading;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	internal class WinPhoneTicker : Ticker
	{
		readonly DispatcherTimer _timer;

		public WinPhoneTicker()
		{
			_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
			_timer.Tick += (sender, args) => SendSignals();
		}

		protected override void DisableTimer()
		{
			_timer.Stop();
		}

		protected override void EnableTimer()
		{
			_timer.Start();
		}
	}
}