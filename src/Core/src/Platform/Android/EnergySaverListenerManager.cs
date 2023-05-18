using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Platform
{
	public class EnergySaverListenerManager : IEnergySaverListenerManager, IDisposable
	{
		readonly WeakList<IEnergySaverListener> _listeners = new();
		bool _disposedValue;

		public EnergySaverListenerManager()
		{
			Battery.EnergySaverStatusChanged += OnEnergySaverStatusChanged;
		}

		private void OnEnergySaverStatusChanged(object? sender, EnergySaverStatusChangedEventArgs e)
		{
			if (_disposedValue)
				return;

			_listeners.ForEach(l => l.OnStatusUpdated(e.EnergySaverStatus == EnergySaverStatus.On));
		}

		public void Add(IEnergySaverListener listener)
		{
			if (_disposedValue)
				throw new ObjectDisposedException(null);

			_listeners.Add(listener);
			listener.OnStatusUpdated(Battery.EnergySaverStatus == EnergySaverStatus.On);
		}

		public void Remove(IEnergySaverListener listener)
		{
			if (_disposedValue)
				throw new ObjectDisposedException(null);

			_listeners.Remove(listener);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Battery.EnergySaverStatusChanged -= OnEnergySaverStatusChanged;
					_listeners.Clear();
				}

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}