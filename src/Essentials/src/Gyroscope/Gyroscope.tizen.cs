using Tizen.Sensor;
using TizenGyroscope = Tizen.Sensor.Gyroscope;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		internal static TizenGyroscope DefaultSensor =>
			(TizenGyroscope)Platform.GetDefaultSensor(SensorType.Gyroscope);

		public bool IsSupported =>
			TizenGyroscope.IsSupported;

		public void Start(SensorSpeed sensorSpeed)
		{
			DefaultSensor.Interval = sensorSpeed.ToPlatform();
			DefaultSensor.DataUpdated += DataUpdated;
			DefaultSensor.Start();
		}

		publicvoid Stop()
		{
			DefaultSensor.DataUpdated -= DataUpdated;
			DefaultSensor.Stop();
		}

		static void DataUpdated(object sender, GyroscopeDataUpdatedEventArgs e)
		{
			Gyroscope.OnChanged(new GyroscopeData(e.X, e.Y, e.Z));
		}
	}
}
