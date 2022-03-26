using System;
using Windows.Devices.Sensors;
using WindowsMagnetometer = Windows.Devices.Sensors.Magnetometer;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class MagnetometerImplementation : IMagnetometer
	{
		// keep around a reference so we can stop this same instance
		WindowsMagnetometer sensor;

		static WindowsMagnetometer DefaultSensor =>
			WindowsMagnetometer.GetDefault();

		bool PlatformIsSupported =>
			DefaultSensor != null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += DataUpdated;
		}

		void DataUpdated(object sender, MagnetometerReadingChangedEventArgs e)
		{
			var reading = e.Reading;
			var data = new MagnetometerData(reading.MagneticFieldX, reading.MagneticFieldY, reading.MagneticFieldZ);
			RaiseReadingChanged(data);
		}

		void PlatformStop()
		{
			sensor.ReadingChanged -= DataUpdated;
			sensor.ReportInterval = 0;
			sensor = null;
		}
	}
}
