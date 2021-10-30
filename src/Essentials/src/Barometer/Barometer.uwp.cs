using System;
using Windows.Devices.Sensors;
using WinBarometer = Windows.Devices.Sensors.Barometer;

namespace Microsoft.Maui.Essentials
{
	public static partial class Barometer
	{
		static WinBarometer sensor;

		static WinBarometer DefaultBarometer => WinBarometer.GetDefault();

		internal static bool IsSupported =>
			DefaultBarometer != null;

		static void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultBarometer;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += BarometerReportedInterval;
		}

		static void BarometerReportedInterval(object sender, BarometerReadingChangedEventArgs e)
			=> OnChanged(new BarometerData(e.Reading.StationPressureInHectopascals));

		static void PlatformStop()
		{
			sensor.ReadingChanged -= BarometerReportedInterval;
			sensor.ReportInterval = 0;
			sensor = null;
		}
	}
}
