using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	internal static partial class SensorSpeedExtensions
	{
		internal static SensorDelay ToPlatform(this SensorSpeed sensorSpeed)
		{
			switch (sensorSpeed)
			{
				case SensorSpeed.Fastest:
					return SensorDelay.Fastest;
				case SensorSpeed.Game:
					return SensorDelay.Game;
				case SensorSpeed.UI:
					return SensorDelay.Ui;
			}

			return SensorDelay.Normal;
		}
	}
}
