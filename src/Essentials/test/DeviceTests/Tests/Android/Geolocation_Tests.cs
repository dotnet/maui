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
		public void ToLocation_HasAltitudeButNoMslAltitude_UsesEllipsoid()
		{
			// On every API level, a location that reports an ellipsoidal altitude but no
			// MSL altitude must resolve to Ellipsoid. On pre-34 devices that is the only
			// code path; on API 34+ devices this exercises the HasMslAltitude == false
			// fallback branch. Either way this assertion runs, so the test cannot silently
			// pass if the fallback regresses.
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
			var androidLocation = new AndroidLocation("test")
			{
				Altitude = 123.45,
			};

			// Baseline: without MSL altitude set, we must get Ellipsoid on any API level.
			// This guarantees an assertion runs even on pre-34 devices so the test cannot
			// silently pass if the API-34 branch is accidentally taken or the fallback
			// regresses.
			var baseline = androidLocation.ToLocation();
			Assert.Equal(123.45, baseline.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Ellipsoid, baseline.AltitudeReferenceSystem);

			// The remaining assertions exercise the API 34+ MSL path. MslAltitudeMeters is
			// only meaningful on API 34+, so below that we stop after validating the
			// fallback above.
			if (!OperatingSystem.IsAndroidVersionAtLeast(34))
				return;

			androidLocation.MslAltitudeMeters = 100.0;
			androidLocation.MslAltitudeAccuracyMeters = 2.5f;
			androidLocation.VerticalAccuracyMeters = 5.0f; // ellipsoidal accuracy, must NOT be surfaced

			var location = androidLocation.ToLocation();

			Assert.Equal(100.0, location.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Geoid, location.AltitudeReferenceSystem);
			// VerticalAccuracy must be paired with the chosen altitude reference system,
			// so the MSL accuracy is used rather than the ellipsoidal one.
			Assert.Equal(2.5, location.VerticalAccuracy);
		}

		[Fact]
		public void ToLocation_MslAltitudeWithoutMslAccuracy_ReportsNullVerticalAccuracy()
		{
			var androidLocation = new AndroidLocation("test");

			// Baseline: a location with no altitude reports Unspecified on any API level.
			// This keeps the test meaningful on pre-34 devices instead of silently passing.
			var baseline = androidLocation.ToLocation();
			Assert.Null(baseline.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Unspecified, baseline.AltitudeReferenceSystem);
			Assert.Null(baseline.VerticalAccuracy);

			if (!OperatingSystem.IsAndroidVersionAtLeast(34))
				return;

			// On API 34+, an MSL altitude without an MSL accuracy must NOT surface the
			// ellipsoidal VerticalAccuracyMeters — they describe a different reference system.
			androidLocation.MslAltitudeMeters = 100.0;
			androidLocation.VerticalAccuracyMeters = 5.0f;

			var location = androidLocation.ToLocation();

			Assert.Equal(100.0, location.Altitude);
			Assert.Equal(AltitudeReferenceSystem.Geoid, location.AltitudeReferenceSystem);
			Assert.Null(location.VerticalAccuracy);
		}
	}
}
