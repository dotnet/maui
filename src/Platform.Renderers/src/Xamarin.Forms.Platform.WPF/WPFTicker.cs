using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	internal class WPFTicker : Ticker
	{
		readonly DispatcherTimer _timer;

		public WPFTicker()
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
