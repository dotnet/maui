using System;
using System.Collections.Generic;
using System.Linq;
using AndroidAddress = Android.Locations.Address;
using AndroidLocation = Android.Locations.Location;

namespace Microsoft.Maui.Devices.Sensors
{
	static partial class LocationExtensions
	{
		internal static Location ToLocation(this AndroidAddress address) =>
			new Location
			{
				Latitude = address.Latitude,
				Longitude = address.Longitude,
				Timestamp = DateTimeOffset.UtcNow
			};

		internal static IEnumerable<Location> ToLocations(this IEnumerable<AndroidAddress> addresses) =>
			addresses?.Select(a => a.ToLocation());

		internal static Location ToLocation(this AndroidLocation location) =>
			new Location
			{
				Latitude = location.Latitude,
				Longitude = location.Longitude,
				Altitude = location.HasAltitude ? location.Altitude : default(double?),
				Timestamp = location.GetTimestamp().ToUniversalTime(),
				Accuracy = location.HasAccuracy ? location.Accuracy : default(float?),
				VerticalAccuracy =
					OperatingSystem.IsAndroidVersionAtLeast(26) && location.HasVerticalAccuracy
						? location.VerticalAccuracyMeters
						: null,
				ReducedAccuracy = false,
				Course = location.HasBearing ? location.Bearing : default(double?),
				Speed = location.HasSpeed ? location.Speed : default(double?),
				IsFromMockProvider =
					OperatingSystem.IsAndroidVersionAtLeast(31)
						? location.Mock
#pragma warning disable CS0618 // Type or member is obsolete
						: location.IsFromMockProvider,
#pragma warning restore CS0618 // Type or member is obsolete
				AltitudeReferenceSystem = AltitudeReferenceSystem.Ellipsoid
			};

		static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		internal static DateTimeOffset GetTimestamp(this AndroidLocation location)
		{
			try
			{
				return new DateTimeOffset(epoch.AddMilliseconds(location.Time));
			}
			catch (Exception)
			{
				return new DateTimeOffset(epoch);
			}
		}
	}
}
