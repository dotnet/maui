using Windows.Devices.Sensors;
using WindowsGyro = Windows.Devices.Sensors.Gyrometer;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GyroscopeImplementation : IGyroscope
	{
		// keep around a reference so we can stop this same instance
		WindowsGyro sensor;

		static WindowsGyro DefaultSensor =>
			WindowsGyro.GetDefault();

		bool PlatformIsSupported =>
			DefaultSensor != null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += DataUpdated;
		}

		void DataUpdated(object sender, GyrometerReadingChangedEventArgs e)
		{
			var reading = e.Reading;
			var data = new GyroscopeData(reading.AngularVelocityX, reading.AngularVelocityY, reading.AngularVelocityZ);
			RaiseReadingChanged(data);
		}

		void PlatformStop()
		{
			sensor.ReadingChanged -= DataUpdated;
			sensor.ReportInterval = 0;
		}
	}
}
