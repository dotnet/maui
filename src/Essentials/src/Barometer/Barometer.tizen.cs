using Tizen.Sensor;
using TizenBarometerSensor = Tizen.Sensor.PressureSensor;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		static TizenBarometerSensor DefaultSensor
			=> (TizenBarometerSensor)Platform.GetDefaultSensor(SensorType.Barometer);

		public bool IsSupported
			=> TizenBarometerSensor.IsSupported;

		public bool IsMonitoring { get; set; }

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

		public void DataUpdated(object sender, PressureSensorDataUpdatedEventArgs e)
		{
			OnChanged(new BarometerData(e.Pressure));
		}
	}
}
