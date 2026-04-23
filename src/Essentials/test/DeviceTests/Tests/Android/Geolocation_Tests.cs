using System;
using Microsoft.Maui.Devices.Sensors;
using Xunit;
using AndroidLocation = Android.Locations.Location;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("Android Geolocation")]
	public class Android_Geolocation_Tests
	{
		[Fact]
		public void ToLocation_NoAltitude_UsesUnspecifiedReferenceSystem()
		{
			var androidLocation = new AndroidLocation("test");

			var location = androidLocation.ToLocation();

			Assert.Null(location.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Unspecified, location.AltitudeReferenceSystem);
			Assert.Null(location.VerticalAccuracy);
		}

		[Fact]
		public void ToLocation_EllipsoidalAltitude_UsesEllipsoidReferenceSystem()
		{
			var androidLocation = new AndroidLocation("test")
			{
				Altitude = 123.45,
			};

			if (OperatingSystem.IsAndroidVersionAtLeast(26))
				androidLocation.VerticalAccuracyMeters = 5.0f;

			var location = androidLocation.ToLocation();

			Assert.Equal(123.45, location.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Ellipsoid, location.AltitudeReferenceSystem);

			if (OperatingSystem.IsAndroidVersionAtLeast(26))
				Assert.Equal(5.0, location.VerticalAccuracy);
			else
				Assert.Null(location.VerticalAccuracy);
		}

		[Fact]
		public void ToLocation_MslAltitude_UsesGeoidReferenceSystem()
		{
			// MSL altitude is only available on Android API 34+
			if (!OperatingSystem.IsAndroidVersionAtLeast(34))
				return;

			var androidLocation = new AndroidLocation("test")
			{
				Altitude = 123.45,              // ellipsoidal
				MslAltitudeMeters = 100.0,      // geoid
				MslAltitudeAccuracyMeters = 2.5f,
				VerticalAccuracyMeters = 5.0f,  // ellipsoidal accuracy
			};

			var location = androidLocation.ToLocation();

			Assert.Equal(100.0, location.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Geoid, location.AltitudeReferenceSystem);
			// VerticalAccuracy must be paired with the chosen altitude reference system,
			// so the MSL accuracy is preferred over the ellipsoidal one.
			Assert.Equal(2.5, location.VerticalAccuracy);
		}

		[Fact]
		public void ToLocation_MslAltitudeWithoutMslAccuracy_ReportsNullVerticalAccuracy()
		{
			// MSL altitude is only available on Android API 34+
			if (!OperatingSystem.IsAndroidVersionAtLeast(34))
				return;

			var androidLocation = new AndroidLocation("test")
			{
				MslAltitudeMeters = 100.0,
				VerticalAccuracyMeters = 5.0f,  // ellipsoidal accuracy, must NOT be surfaced alongside MSL altitude
			};

			var location = androidLocation.ToLocation();

			Assert.Equal(100.0, location.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Geoid, location.AltitudeReferenceSystem);
			Assert.Null(location.VerticalAccuracy);
		}
	}
}
