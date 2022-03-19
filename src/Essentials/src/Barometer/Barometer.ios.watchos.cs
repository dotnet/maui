using System;
using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class BarometerImplementation : IBarometer
	{
		CMAltimeter altitudeManager;

		public bool IsSupported
			=> CMAltimeter.IsRelativeAltitudeAvailable;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			altitudeManager = new CMAltimeter();
			altitudeManager.StartRelativeAltitudeUpdates(Platform.GetCurrentQueue(), LocationManagerUpdatedHeading);

			void LocationManagerUpdatedHeading(CMAltitudeData e, NSError error)
			{
				var reading = new BarometerData(UnitConverters.KilopascalsToHectopascals(e.Pressure.DoubleValue));

				RaiseReadingChanged(reading);
			}
		}

		void PlatformStop()
		{
			if (altitudeManager == null)
				return;

			altitudeManager.StopRelativeAltitudeUpdates();
			altitudeManager.Dispose();
			altitudeManager = null;
		}
	}
}
