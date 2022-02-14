using Windows.Devices.Sensors;
using WindowsOrientationSensor = Windows.Devices.Sensors.OrientationSensor;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class OrientationSensorImplementation : IOrientationSensor
	{
		// keep around a reference so we can stop this same instance
		static WindowsOrientationSensor sensor;

		internal static WindowsOrientationSensor DefaultSensor =>
		  WindowsOrientationSensor.GetDefault();

		public bool IsSupported =>
			DefaultSensor != null;

		public void Start(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;

			var interval = sensorSpeed.ToPlatform();

			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += DataUpdated;
		}

		static void DataUpdated(object sender, OrientationSensorReadingChangedEventArgs e)
		{
			var reading = e.Reading;
			var data = new OrientationSensorData(reading.Quaternion.X, reading.Quaternion.Y, reading.Quaternion.Z, reading.Quaternion.W);
			OnChanged(data);
		}

		public void Stop()
		{
			sensor.ReadingChanged -= DataUpdated;
			sensor.ReportInterval = 0;
		}
	}
}
