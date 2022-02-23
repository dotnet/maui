using Tizen.Sensor;
using TizenGyroscope = Tizen.Sensor.Gyroscope;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		static TizenGyroscope DefaultSensor =>
			(TizenGyroscope)Platform.GetDefaultSensor(SensorType.Gyroscope);

		bool PlatformIsSupported =>
			TizenGyroscope.IsSupported;

		TizenGyroscope sensor;

		public void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;
			sensor.Interval = sensorSpeed.ToPlatform();
			sensor.DataUpdated += DataUpdated;
			sensor.Start();
		}

		public void PlatformStop()
		{
			sensor.DataUpdated -= DataUpdated;
			sensor.Stop();
			sensor = null;
		}

		void DataUpdated(object sender, GyroscopeDataUpdatedEventArgs e)
		{
			RaiseReadingChanged(new GyroscopeData(e.X, e.Y, e.Z));
		}
	}
}
