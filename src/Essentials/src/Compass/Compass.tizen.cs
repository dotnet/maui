using Tizen.Sensor;
using TizenCompass = Tizen.Sensor.OrientationSensor;

namespace Microsoft.Maui.Essentials
{
	public static partial class Compass
	{
		internal static TizenCompass DefaultSensor =>
			(TizenCompass)Platform.GetDefaultSensor(SensorType.Compass);

		internal static bool IsSupported =>
			TizenCompass.IsSupported;

		internal static void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			DefaultSensor.Interval = sensorSpeed.ToPlatform();
			DefaultSensor.DataUpdated += DataUpdated;
			DefaultSensor.Start();
		}

		internal static void PlatformStop()
		{
			DefaultSensor.DataUpdated -= DataUpdated;
			DefaultSensor.Stop();
		}

		static void DataUpdated(object sender, OrientationSensorDataUpdatedEventArgs e)
		{
			OnChanged(new CompassData(e.Azimuth));
		}
	}
}
