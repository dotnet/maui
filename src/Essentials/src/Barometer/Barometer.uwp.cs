using System;
using Windows.Devices.Sensors;
using WinBarometer = Windows.Devices.Sensors.Barometer;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		static WinBarometer sensor;

		static WinBarometer DefaultBarometer => WinBarometer.GetDefault();

		public bool IsMonitoring { get; set; }

		public bool IsSupported =>
			DefaultBarometer != null;

		public void Start(SensorSpeed sensorSpeed)
		{
			sensor = DefaultBarometer;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += BarometerReportedInterval;
		}

		internal void BarometerReportedInterval(object sender, BarometerReadingChangedEventArgs e)
			=> OnChanged(new BarometerData(e.Reading.StationPressureInHectopascals));

		public void Stop()
		{
			sensor.ReadingChanged -= BarometerReportedInterval;
			sensor.ReportInterval = 0;
			sensor = null;
		}
	}
}
