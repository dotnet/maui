using System;
using Microsoft.Maui.ApplicationModel;
using Tizen.Sensor;
using TizenBarometerSensor = Tizen.Sensor.PressureSensor;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class BarometerImplementation : IBarometer
	{
		public bool IsSupported
			=> TizenBarometerSensor.IsSupported;

		TizenBarometerSensor DefaultSensor
			=> (TizenBarometerSensor)PlatformUtils.GetDefaultSensor(SensorType.Barometer);

		TizenBarometerSensor sensor = null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			sensor = DefaultSensor;
			sensor.Interval = sensorSpeed.ToPlatform();
			sensor.DataUpdated += DataUpdated;
			sensor.Start();
		}

		void DataUpdated(object sender, PressureSensorDataUpdatedEventArgs e)
			=> RaiseReadingChanged(new BarometerData(e.Pressure));

		void PlatformStop()
		{
			sensor.DataUpdated -= DataUpdated;
			sensor.Stop();
			sensor = null;
		}
	}
}
