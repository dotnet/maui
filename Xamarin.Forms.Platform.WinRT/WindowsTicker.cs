using System;
using Windows.UI.Xaml;
using Xamarin.Forms.Internals;

#if WINDOWS_UWP
namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal class WindowsTicker : Ticker
	{
		readonly DispatcherTimer _timer;

		public WindowsTicker()
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