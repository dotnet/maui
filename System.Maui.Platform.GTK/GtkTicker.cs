using System.Maui.Internals;

namespace System.Maui.Platform.GTK
{
	public class GtkTicker : Ticker
	{
		private uint _timerId;

		protected override void DisableTimer()
		{
			GLib.Source.Remove(_timerId);
		}

		protected override void EnableTimer()
		{
			_timerId = GLib.Timeout.Add(15, new GLib.TimeoutHandler(OnSendSignals));
		}

		private bool OnSendSignals()
		{
			SendSignals();

			return true;
		}
	}
}
