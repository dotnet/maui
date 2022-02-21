using Tizen.Sensor;
using TizenMagnetometer = Tizen.Sensor.Magnetometer;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class MagnetometerImplementation : IMagnetometer
	{
		public TizenMagnetometer DefaultSensor =>
			(TizenMagnetometer)Platform.GetDefaultSensor(SensorType.Magnetometer);

		internal static bool IsSupported =>
			TizenMagnetometer.IsSupported;

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

		static void DataUpdated(object sender, MagnetometerDataUpdatedEventArgs e)
		{
			Magnetometer.OnChanged(new MagnetometerData(e.X, e.Y, e.Z));
		}
	}
}
