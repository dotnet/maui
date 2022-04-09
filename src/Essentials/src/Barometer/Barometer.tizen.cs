using System;
using Tizen.Sensor;
using TizenBarometerSensor = Tizen.Sensor.PressureSensor;

namespace Microsoft.Maui.Devices.Sensors
{
	class BarometerImplementation : IBarometer
	{
		public bool IsSupported
			=> TizenBarometerSensor.IsSupported;

		TizenBarometerSensor DefaultSensor
			=> (TizenBarometerSensor)Platform.GetDefaultSensor(SensorType.Barometer);

		TizenBarometerSensor sensor = null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;
			sensor.Interval = sensorSpeed.ToPlatform();
			sensor.DataUpdated += DataUpdated;
			sensor.Start();
		}

		void DataUpdated(object sender, PressureSensorDataUpdatedEventArgs e)
			=> RaiseDataChanged(new BarometerData(e.Pressure));

		void PlatformStop()
		{
			sensor.DataUpdated -= DataUpdated;
			sensor.Stop();
			sensor = null;
		}
	}
}
