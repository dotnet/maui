using Tizen.Sensor;
using TizenCompass = Tizen.Sensor.OrientationSensor;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class CompassImplementation : ICompass
	{
		public TizenCompass DefaultSensor =>
			(TizenCompass)Platform.GetDefaultSensor(SensorType.Compass);

		public bool IsSupported =>
			TizenCompass.IsSupported;

		public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
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

		void DataUpdated(object sender, OrientationSensorDataUpdatedEventArgs e)
		{
			OnChanged(new CompassData(e.Azimuth));
		}
	}
}
