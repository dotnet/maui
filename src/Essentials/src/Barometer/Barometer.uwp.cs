using System;
using Windows.Devices.Sensors;
using WinBarometer = Windows.Devices.Sensors.Barometer;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class BarometerImplementation : IBarometer
	{
		WinBarometer sensor;

		WinBarometer DefaultBarometer => WinBarometer.GetDefault();

		bool PlatformIsSupported =>
			DefaultBarometer != null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultBarometer;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += BarometerReportedInterval;
		}

		internal void BarometerReportedInterval(object sender, BarometerReadingChangedEventArgs e)
			=> RaiseReadingChanged(new BarometerData(e.Reading.StationPressureInHectopascals));

		void PlatformStop()
		{
			if (sensor == null)
				return;

			sensor.ReadingChanged -= BarometerReportedInterval;
			sensor.ReportInterval = 0;
			sensor = null;
		}
	}
}
