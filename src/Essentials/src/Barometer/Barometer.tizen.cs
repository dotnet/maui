using System;
using Tizen.Sensor;
using TizenBarometerSensor = Tizen.Sensor.PressureSensor;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		static TizenBarometerSensor DefaultSensor
			=> (TizenBarometerSensor)Platform.GetDefaultSensor(SensorType.Barometer);

		bool PlatformIsSupported
			=> TizenBarometerSensor.IsSupported;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			DefaultSensor.Interval = sensorSpeed.ToPlatform();
			DefaultSensor.DataUpdated += DataUpdated;
			DefaultSensor.Start();
		}

		void DataUpdated(object sender, PressureSensorDataUpdatedEventArgs e)
			=> RaiseDataChanged(new BarometerData(e.Pressure));

		void PlatformStop()
		{
			DefaultSensor.DataUpdated -= DataUpdated;
			DefaultSensor.Stop();
		}
	}
}
