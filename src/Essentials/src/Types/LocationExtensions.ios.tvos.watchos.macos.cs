using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Foundation;

namespace Microsoft.Maui.Devices.Sensors
{
	static partial class LocationExtensions
	{
		[System.Runtime.InteropServices.DllImport(ObjCRuntime.Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		static extern CLAuthorizationStatus CLAuthorizationStatus_objc_msgSend(IntPtr receiver, IntPtr selector);

		internal static Location ToLocation(this CLPlacemark placemark) =>
			new Location
			{
				Latitude = placemark.Location.Coordinate.Latitude,
				Longitude = placemark.Location.Coordinate.Longitude,
				Altitude = placemark.Location.Altitude,
				AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid,
				Timestamp = DateTimeOffset.UtcNow,
				ReducedAccuracy = false,
			};

		internal static IEnumerable<Location> ToLocations(this IEnumerable<CLPlacemark> placemarks) =>
			placemarks?.Select(a => a.ToLocation());

		internal static Location ToLocation(this CLLocation location, bool reducedAccuracy) =>
			new Location
			{
				Latitude = location.Coordinate.Latitude,
				Longitude = location.Coordinate.Longitude,
				Altitude = location.VerticalAccuracy < 0 ? default(double?) : location.Altitude,
				Accuracy = location.HorizontalAccuracy,
				VerticalAccuracy = location.VerticalAccuracy,
				ReducedAccuracy = reducedAccuracy,
				Timestamp = location.Timestamp.ToDateTime(),
#if __IOS__ || __WATCHOS__
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
				Course = location.Course < 0 ? default(double?) : location.Course,
				Speed = location.Speed < 0 ? default(double?) : location.Speed,
#pragma warning restore CA1416
#endif
				IsFromMockProvider = DeviceInfo.DeviceType == DeviceType.Virtual,
				AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid
			};

		internal static DateTimeOffset ToDateTime(this NSDate timestamp)
		{
			try
			{
				return new DateTimeOffset((DateTime)timestamp);
			}
			catch
			{
				return DateTimeOffset.UtcNow;
			}
		}

		internal static CLAuthorizationStatus GetAuthorizationStatus(this CLLocationManager locationManager)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0) ||
				OperatingSystem.IsMacOSVersionAtLeast(11, 0) ||
				OperatingSystem.IsWatchOSVersionAtLeast(7, 0) ||
				OperatingSystem.IsTvOSVersionAtLeast(14, 0))
			{
				// return locationManager.AuthorizationStatus;

				var sel = ObjCRuntime.Selector.GetHandle("authorizationStatus");
				return CLAuthorizationStatus_objc_msgSend(locationManager.Handle, sel);
			}

			return CLLocationManager.Status;
		}
	}
}
