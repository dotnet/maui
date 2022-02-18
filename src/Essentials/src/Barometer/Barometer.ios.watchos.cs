using System;
using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		public bool IsSupported
			=> CMAltimeter.IsRelativeAltitudeAvailable;

		static CMAltimeter altitudeManager;

		public bool IsMonitoring { get; set; }

		public void Start(SensorSpeed sensorSpeed)
		{
			altitudeManager = new CMAltimeter();
			altitudeManager.StartRelativeAltitudeUpdates(Platform.GetCurrentQueue(), LocationManagerUpdatedHeading);

			void LocationManagerUpdatedHeading(CMAltitudeData e, NSError error) =>
				OnChanged(new BarometerData(UnitConverters.KilopascalsToHectopascals(e.Pressure.DoubleValue)));
		}

		public void Stop()
		{
			if (altitudeManager == null)
				return;
			altitudeManager.StopRelativeAltitudeUpdates();
			altitudeManager.Dispose();
			altitudeManager = null;
		}

		public event EventHandler<BarometerChangedEventArgs> ReadingChanged;

		internal void OnChanged(BarometerData reading) =>
			OnChanged(new BarometerChangedEventArgs(reading));

		internal void OnChanged(BarometerChangedEventArgs e)
		{
			if ( ! MainThread.IsMainThread)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);
		}
	}
}
