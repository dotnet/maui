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

		internal static Location ToLocation(this AndroidLocation location)
		{
			var (altitude, altitudeReference) = GetAltitude(location);

			return new Location
			{
				Latitude = location.Latitude,
				Longitude = location.Longitude,
				Altitude = altitude,
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
				AltitudeReferenceSystem = altitudeReference
			};
		}

		// Prefer mean sea level altitude (Android API 34+) when available so altitude
		// values are consistent across platforms without manual geoid correction.
		static (double? Altitude, AltitudeReferenceSystem Reference) GetAltitude(AndroidLocation location)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(34) && location.HasMslAltitude)
				return (location.MslAltitudeMeters, AltitudeReferenceSystem.Geoid);

			if (location.HasAltitude)
				return (location.Altitude, AltitudeReferenceSystem.Ellipsoid);

			return (null, AltitudeReferenceSystem.Unspecified);
		}

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
