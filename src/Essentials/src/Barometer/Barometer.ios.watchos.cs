using System;
using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		public bool IsSupported
			=> CMAltimeter.IsRelativeAltitudeAvailable;

		public event EventHandler<BarometerChangedEventArgs> ReadingChanged;

		static CMAltimeter altitudeManager;

		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public SensorSpeed SensorSpeed { get; private set; } = SensorSpeed.Default;

		public bool IsMonitoring { get; private set; }

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Barometer has already been started.");

			IsMonitoring = true;
			SensorSpeed = sensorSpeed;

			try
			{
				altitudeManager = new CMAltimeter();
				altitudeManager.StartRelativeAltitudeUpdates(Platform.GetCurrentQueue(), LocationManagerUpdatedHeading);

				void LocationManagerUpdatedHeading(CMAltitudeData e, NSError error)
				{
					var reading = new BarometerData(UnitConverters.KilopascalsToHectopascals(e.Pressure.DoubleValue));

					if (UseSyncContext)
						MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(this, new BarometerChangedEventArgs(reading)));
					else
						ReadingChanged?.Invoke(this, new BarometerChangedEventArgs(reading));
				}
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		public void Stop()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (!IsMonitoring)
				return;

			IsMonitoring = false;

			if (altitudeManager == null)
				return;

			try
			{
				altitudeManager.StopRelativeAltitudeUpdates();
				altitudeManager.Dispose();
				altitudeManager = null;
			}
			catch
			{
				IsMonitoring = true;
				throw;
			}
		}
	}
}
