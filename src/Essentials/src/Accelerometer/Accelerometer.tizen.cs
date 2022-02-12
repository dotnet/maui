using Tizen.Sensor;
using TizenAccelerometer = Tizen.Sensor.Accelerometer;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class AccelerometerImplementation  : IAccelerometer
	{
		internal static TizenAccelerometer DefaultSensor =>
			(TizenAccelerometer)Platform.GetDefaultSensor(SensorType.Accelerometer);

		internal static bool IsSupported =>
			TizenAccelerometer.IsSupported;

		static void Start(SensorSpeed sensorSpeed)
		{
			DefaultSensor.Interval = sensorSpeed.ToPlatform();
			DefaultSensor.DataUpdated += DataUpdated;
			DefaultSensor.Start();
		}

		static void Stop()
		{
			DefaultSensor.DataUpdated -= DataUpdated;
			DefaultSensor.Stop();
		}

		static void DataUpdated(object sender, AccelerometerDataUpdatedEventArgs e)
		{
			OnChanged(new AccelerometerData(e.X, e.Y, e.Z));
		}
	}
}
