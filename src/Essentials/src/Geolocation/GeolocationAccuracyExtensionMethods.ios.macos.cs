#nullable enable
using CoreLocation;

namespace Microsoft.Maui.Devices.Sensors
{
	static class GeolocationAccuracyExtensionMethods
	{
		internal static double PlatformDesiredAccuracy(this GeolocationAccuracy desiredAccuracy)
		{
			switch (desiredAccuracy)
			{
				case GeolocationAccuracy.Lowest:
					return CLLocation.AccuracyThreeKilometers;
				case GeolocationAccuracy.Low:
					return CLLocation.AccuracyKilometer;
				case GeolocationAccuracy.Default:
				case GeolocationAccuracy.Medium:
					return CLLocation.AccuracyHundredMeters;
				case GeolocationAccuracy.High:
					return CLLocation.AccuracyNearestTenMeters;
				case GeolocationAccuracy.Best:
					return CLLocation.AccuracyBestForNavigation;
				default:
					return CLLocation.AccuracyHundredMeters;
			}
		}
	}
}
