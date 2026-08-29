using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;
using Xunit;
using AndroidLocation = Android.Locations.Location;
using AndroidLocationManager = Android.Locations.LocationManager;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Geolocation")]
	public class Android_Geolocation_Tests
	{
		public static IEnumerable<object[]> AccuracyDistances =>
			new[]
			{
				new object[] { GeolocationAccuracy.Lowest, 500f },
				new object[] { GeolocationAccuracy.Low, 500f },
				new object[] { GeolocationAccuracy.Default, 250f },
				new object[] { GeolocationAccuracy.Medium, 250f },
				new object[] { GeolocationAccuracy.High, 100f },
				new object[] { GeolocationAccuracy.Best, 50f },
			};

		public static IEnumerable<object[]> StandardProviderPreferences =>
			new[]
			{
				new object[] { GeolocationAccuracy.Lowest, AndroidLocationManager.NetworkProvider },
				new object[] { GeolocationAccuracy.Low, AndroidLocationManager.NetworkProvider },
				new object[] { GeolocationAccuracy.Default, AndroidLocationManager.NetworkProvider },
				new object[] { GeolocationAccuracy.Medium, AndroidLocationManager.NetworkProvider },
				new object[] { GeolocationAccuracy.High, AndroidLocationManager.GpsProvider },
				new object[] { GeolocationAccuracy.Best, AndroidLocationManager.GpsProvider },
			};

		[Theory]
		[MemberData(nameof(AccuracyDistances))]
		public void GetAccuracyDistance_ReturnsExpectedDistance(GeolocationAccuracy accuracy, float expectedDistance)
		{
			Assert.Equal(expectedDistance, GeolocationImplementation.GetAccuracyDistance(accuracy));
		}

		[Theory]
		[MemberData(nameof(StandardProviderPreferences))]
		public void SelectProvider_UsesAccuracyPreferenceWhenFusedIsUnavailable(GeolocationAccuracy accuracy, string expectedProvider)
		{
			var enabledProviders = new[] { AndroidLocationManager.GpsProvider, AndroidLocationManager.NetworkProvider };

			var provider = GeolocationImplementation.SelectProvider(enabledProviders, accuracy);

			Assert.Equal(expectedProvider, provider);
		}

		[Fact]
		public void SelectProvider_PrefersFusedProvider()
		{
			var enabledProviders = new[] { AndroidLocationManager.GpsProvider, AndroidLocationManager.NetworkProvider, AndroidLocationManager.FusedProvider };

			var provider = GeolocationImplementation.SelectProvider(enabledProviders, GeolocationAccuracy.Best);

			Assert.Equal(AndroidLocationManager.FusedProvider, provider);
		}

		[Fact]
		public void SelectProvider_FallsBackToFirstNonPassiveProvider()
		{
			var enabledProviders = new[] { AndroidLocationManager.PassiveProvider, "local_database", "custom" };

			var provider = GeolocationImplementation.SelectProvider(enabledProviders, GeolocationAccuracy.Default);

			Assert.Equal("custom", provider);
		}

		[Fact]
		public void SelectFallbackProvider_IgnoresPassiveAndLocalDatabaseProviders()
		{
			var enabledProviders = new[] { AndroidLocationManager.PassiveProvider, "local_database", "custom" };

			var provider = GeolocationImplementation.SelectFallbackProvider(enabledProviders);

			Assert.Equal("custom", provider);
		}

		[Theory]
		[InlineData(GeolocationAccuracy.Best, AndroidLocationManager.NetworkProvider)]
		[InlineData(GeolocationAccuracy.Lowest, AndroidLocationManager.GpsProvider)]
		public void SelectProvider_UsesAlternateStandardProvider(GeolocationAccuracy accuracy, string enabledProvider)
		{
			var provider = GeolocationImplementation.SelectProvider(new[] { enabledProvider }, accuracy);

			Assert.Equal(enabledProvider, provider);
		}

		[Fact]
		public void SelectProvider_ReturnsNullWhenOnlyIgnoredProvidersAreEnabled()
		{
			var enabledProviders = new[] { AndroidLocationManager.PassiveProvider, "local_database" };

			var provider = GeolocationImplementation.SelectProvider(enabledProviders, GeolocationAccuracy.Default);

			Assert.Null(provider);
		}

		[Fact]
		public void SelectProvider_ReturnsNullWhenProvidersAreUnavailable()
		{
			var provider = GeolocationImplementation.SelectProvider(null, GeolocationAccuracy.Default);

			Assert.Null(provider);
		}

		[Fact]
		public void GetProviders_UsesGpsAndNetworkForLegacySelection()
		{
			var allProviders = new[] { "custom", AndroidLocationManager.NetworkProvider, AndroidLocationManager.GpsProvider };

			var providers = GeolocationImplementation.GetProviders(allProviders, "custom", includeSelectedProvider: false);

			Assert.Equal(new[] { AndroidLocationManager.GpsProvider, AndroidLocationManager.NetworkProvider }, providers);
		}

		[Fact]
		public void GetProviders_IncludesExplicitProviderAndEnabledFallbacks()
		{
			var enabledProviders = new[] { AndroidLocationManager.FusedProvider, AndroidLocationManager.NetworkProvider, AndroidLocationManager.GpsProvider };

			var providers = GeolocationImplementation.GetProviders(enabledProviders, AndroidLocationManager.FusedProvider, includeSelectedProvider: true);

			Assert.Equal(
				new[] { AndroidLocationManager.FusedProvider, AndroidLocationManager.GpsProvider, AndroidLocationManager.NetworkProvider },
				providers);
		}

		[Fact]
		public void GetProviders_UsesLegacyFallbackWhenStandardProvidersAreUnavailable()
		{
			var providers = GeolocationImplementation.GetProviders(new[] { "custom" }, "custom", includeSelectedProvider: false);

			Assert.Equal(new[] { "custom" }, providers);
		}

		[Theory]
		[InlineData(AndroidLocationManager.FusedProvider)]
		[InlineData("custom")]
		public void GetProviders_UsesFallbackWhenStandardProvidersAreUnavailable(string fallbackProvider)
		{
			var providers = GeolocationImplementation.GetProviders(new[] { fallbackProvider }, fallbackProvider, includeSelectedProvider: true);

			Assert.Equal(new[] { fallbackProvider }, providers);
		}

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

		[Fact]
		public void LocationCopyConstructor_PreservesAltitudeReferenceSystem()
		{
			var original = new Location(51.5, -0.1)
			{
				Altitude = 100.0,
				AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid,
				VerticalAccuracy = 2.5
			};

			var copy = new Location(original);

			Assert.Equal(original.Altitude, copy.Altitude);
			Assert.Equal(original.AltitudeReferenceSystem, copy.AltitudeReferenceSystem);
			Assert.Equal(original.VerticalAccuracy, copy.VerticalAccuracy);
		}
	}
}
