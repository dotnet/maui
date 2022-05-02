using Microsoft.Maui.ApplicationModel;
using Tizen.Sensor;
using TizenGyroscope = Tizen.Sensor.Gyroscope;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GyroscopeImplementation : IGyroscope
	{
		static TizenGyroscope DefaultSensor =>
			(TizenGyroscope)PlatformUtils.GetDefaultSensor(SensorType.Gyroscope);

		bool PlatformIsSupported =>
			TizenGyroscope.IsSupported;

		TizenGyroscope sensor;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;
			sensor.Interval = sensorSpeed.ToPlatform();
			sensor.DataUpdated += DataUpdated;
			sensor.Start();
		}

		void PlatformStop()
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
