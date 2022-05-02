using Microsoft.Maui.ApplicationModel;
using Tizen.Sensor;
using TizenAccelerometer = Tizen.Sensor.Accelerometer;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class AccelerometerImplementation
	{
		internal static TizenAccelerometer DefaultSensor =>
			(TizenAccelerometer)PlatformUtils.GetDefaultSensor(SensorType.Accelerometer);

		public bool IsSupported =>
			TizenAccelerometer.IsSupported;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			DefaultSensor.Interval = sensorSpeed.ToPlatform();
			DefaultSensor.DataUpdated += DataUpdated;
			DefaultSensor.Start();
		}

		void PlatformStop()
		{
			DefaultSensor.DataUpdated -= DataUpdated;
			DefaultSensor.Stop();
		}

		void DataUpdated(object sender, AccelerometerDataUpdatedEventArgs e)
		{
			OnChanged(new AccelerometerData(e.X, e.Y, e.Z));
		}
	}
}
