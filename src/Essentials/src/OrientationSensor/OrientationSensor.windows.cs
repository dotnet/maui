using Windows.Devices.Sensors;
using WindowsOrientationSensor = Windows.Devices.Sensors.OrientationSensor;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class OrientationSensorImplementation : IOrientationSensor
	{
		// keep around a reference so we can stop this same instance
		WindowsOrientationSensor sensor;

		static WindowsOrientationSensor DefaultSensor =>
			WindowsOrientationSensor.GetDefault();

		bool PlatformIsSupported =>
			DefaultSensor != null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;

			var interval = sensorSpeed.ToPlatform();

			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;
			sensor.ReadingChanged += DataUpdated;
		}

		void DataUpdated(object sender, OrientationSensorReadingChangedEventArgs e)
		{
			var reading = e.Reading;
			var data = new OrientationSensorData(reading.Quaternion.X, reading.Quaternion.Y, reading.Quaternion.Z, reading.Quaternion.W);
			RaiseReadingChanged(data);
		}

		void PlatformStop()
		{
			sensor.ReadingChanged -= DataUpdated;
			sensor.ReportInterval = 0;
		}
	}
}
