using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class Location_Tests
	{
		[Theory]
		[InlineData(AltitudeReferenceSystem.Geoid, 100.0, 2.5)]
		[InlineData(AltitudeReferenceSystem.Ellipsoid, 123.45, 5.0)]
		[InlineData(AltitudeReferenceSystem.Unspecified, null, null)]
		public void LocationCopyConstructor_PreservesAltitudeReferenceSystem(
			AltitudeReferenceSystem referenceSystem, double? altitude, double? verticalAccuracy)
		{
			var original = new Location(51.5, -0.1)
			{
				Altitude = altitude,
				AltitudeReferenceSystem = referenceSystem,
				VerticalAccuracy = verticalAccuracy
			};

			var copy = new Location(original);

			Assert.Equal(original.Latitude, copy.Latitude);
			Assert.Equal(original.Longitude, copy.Longitude);
			Assert.Equal(original.Altitude, copy.Altitude);
			Assert.Equal(original.AltitudeReferenceSystem, copy.AltitudeReferenceSystem);
			Assert.Equal(original.VerticalAccuracy, copy.VerticalAccuracy);
		}
	}
}
