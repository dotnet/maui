using Microsoft.Maui.ApplicationModel;
using Tizen.Sensor;
using TizenCompass = Tizen.Sensor.OrientationSensor;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class CompassImplementation : ICompass
	{
		static TizenCompass DefaultSensor =>
			(TizenCompass)PlatformUtils.GetDefaultSensor(SensorType.Compass);

		bool PlatformIsSupported =>
			TizenCompass.IsSupported;

		TizenCompass sensor;

		void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
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

		void DataUpdated(object sender, OrientationSensorDataUpdatedEventArgs e)
		{
			RaiseReadingChanged(new CompassData(e.Azimuth));
		}
	}
}
