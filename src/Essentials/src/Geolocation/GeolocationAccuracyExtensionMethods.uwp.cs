#nullable enable

namespace Microsoft.Maui.Devices.Sensors
{
	static class GeolocationAccuracyExtensionMethods
	{
		internal static uint PlatformGetDesiredAccuracy(this GeolocationAccuracy desiredAccuracy)
		{
			switch (desiredAccuracy)
			{
				case GeolocationAccuracy.Lowest:
					return 3000;
				case GeolocationAccuracy.Low:
					return 1000;
				case GeolocationAccuracy.Default:
				case GeolocationAccuracy.Medium:
					return 100;
				case GeolocationAccuracy.High:
					return 10; // Equivalent to PositionAccuracy.High
				case GeolocationAccuracy.Best:
					return 1;
				default:
					return 500; // Equivalent to PositionAccuracy.Default
			}
		}
	}
}
