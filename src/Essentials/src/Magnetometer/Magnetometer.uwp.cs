using System;
using Windows.Devices.Sensors;
using WindowsMagnetometer = Windows.Devices.Sensors.Magnetometer;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class MagnetometerImplementation : IMagnetometer
	{
		// keep around a reference so we can stop this same instance
		static WindowsMagnetometer sensor;

		internal static WindowsMagnetometer DefaultSensor =>
			WindowsMagnetometer.GetDefault();

		public bool IsSupported =>
			DefaultSensor != null;

		public void Start(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += DataUpdated;
		}

		static void DataUpdated(object sender, MagnetometerReadingChangedEventArgs e)
		{
			var reading = e.Reading;
			var data = new MagnetometerData(reading.MagneticFieldX, reading.MagneticFieldY, reading.MagneticFieldZ);
			OnChanged(data);
		}

		public void Stop()
		{
			sensor.ReadingChanged -= DataUpdated;
			sensor.ReportInterval = 0;
		}
	}
}
