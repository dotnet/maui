namespace Microsoft.Maui.Devices.Sensors
{
	internal static partial class SensorSpeedExtensions
	{
		internal static uint ToPlatform(this SensorSpeed sensorSpeed)
		{
			switch (sensorSpeed)
			{
				case SensorSpeed.Fastest:
					return sensorIntervalFastest;
				case SensorSpeed.Game:
					return sensorIntervalGame;
				case SensorSpeed.UI:
					return sensorIntervalUI;
			}

			return sensorIntervalDefault;
		}
	}
}
