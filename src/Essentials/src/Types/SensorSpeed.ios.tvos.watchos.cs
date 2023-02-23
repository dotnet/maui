namespace Microsoft.Maui.Devices.Sensors
{
	internal static partial class SensorSpeedExtensions
	{
		internal static double ToPlatform(this SensorSpeed sensorSpeed)
		{
			switch (sensorSpeed)
			{
				case SensorSpeed.Fastest:
					return sensorIntervalFastest / 1000.0;
				case SensorSpeed.Game:
					return sensorIntervalGame / 1000.0;
				case SensorSpeed.UI:
					return sensorIntervalUI / 1000.0;
			}

			return sensorIntervalDefault / 1000.0;
		}
	}
}
