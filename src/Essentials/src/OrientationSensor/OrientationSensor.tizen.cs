using Tizen.Sensor;
using TizenRotationVectorSensor = Tizen.Sensor.RotationVectorSensor;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class OrientationSensorImplementation : IOrientationSensor
	{
		static TizenRotationVectorSensor DefaultSensor
			=> (TizenRotationVectorSensor)Platform.GetDefaultSensor(SensorType.OrientationSensor);

		public bool IsSupported
			=> TizenRotationVectorSensor.IsSupported;

		public void Start(SensorSpeed sensorSpeed)
		{
			DefaultSensor.Interval = sensorSpeed.ToPlatform();
			DefaultSensor.DataUpdated += DataUpdated;
			DefaultSensor.Start();
		}

		public void Stop()
		{
			DefaultSensor.DataUpdated -= DataUpdated;
			DefaultSensor.Stop();
		}

		static void DataUpdated(object sender, RotationVectorSensorDataUpdatedEventArgs e)
		{
			OnChanged(new OrientationSensorData(e.X, e.Y, e.Z, e.W));
		}
	}
}
