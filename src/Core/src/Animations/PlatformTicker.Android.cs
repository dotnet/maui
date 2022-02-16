using System;
using Android.Animation;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Animations
{
	public class PlatformTicker : Ticker, IDisposable, IEnergySaverListener
	{
		readonly IEnergySaverListenerManager _manager;
		readonly ValueAnimator _val;

		bool _systemEnabled;
		bool _disposedValue;

		public PlatformTicker(IEnergySaverListenerManager manager)
		{
			_manager = manager;
			_val = new ValueAnimator();
			_val.SetIntValues(0, 100); // avoid crash
			_val.RepeatCount = ValueAnimator.Infinite;
			_val.Update += (s, e) => Fire?.Invoke();

			_manager.Add(this);
		}

		public override bool IsRunning => _val.IsStarted;

		public override bool SystemEnabled => _systemEnabled;

		public override void Start() => _val.Start();

		public override void Stop() => _val.Cancel();

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
					_manager.Remove(this);

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void IEnergySaverListener.OnStatusUpdated(bool energySaverEnabled) =>
			_systemEnabled = !energySaverEnabled;
	}
}