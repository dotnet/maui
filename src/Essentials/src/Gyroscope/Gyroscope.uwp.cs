using Windows.Devices.Sensors;
using WindowsGyro = Windows.Devices.Sensors.Gyrometer;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		// keep around a reference so we can stop this same instance
		static WindowsGyro sensor;

		internal static WindowsGyro DefaultSensor =>
			WindowsGyro.GetDefault();

		public bool IsSupported =>
			DefaultSensor != null;

		public void Start(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;

			var interval = sensorSpeed.ToPlatform();
			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += DataUpdated;
		}

		static void DataUpdated(object sender, GyrometerReadingChangedEventArgs e)
		{
			var reading = e.Reading;
			var data = new GyroscopeData(reading.AngularVelocityX, reading.AngularVelocityY, reading.AngularVelocityZ);
			Gyroscope.OnChanged(data);
		}

		public void Stop()
		{
			sensor.ReadingChanged -= DataUpdated;
			sensor.ReportInterval = 0;
		}
	}
}
