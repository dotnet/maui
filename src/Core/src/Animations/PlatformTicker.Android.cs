using System;
using Android.Animation;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker, IDisposable, IEnergySaverListener
	{
		readonly IEnergySaverListenerManager _manager;
		readonly ValueAnimator _val;

		bool _systemEnabled;
		bool _disposedValue;

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

			_manager.Add(this);
		}

		/// <inheritdoc/>
		public override bool IsRunning => _val.IsStarted;

		/// <inheritdoc/>
		public override bool SystemEnabled => _systemEnabled;

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
					_manager.Remove(this);

				_disposedValue = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void IEnergySaverListener.OnStatusUpdated(bool energySaverEnabled) =>
			_systemEnabled = !energySaverEnabled;
	}
}