using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class MapSpanTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var span = new MapSpan(new Location(0, 0), 1, 1);

			Assert.Equal(new Location(0, 0), span.Center);
			Assert.Equal(1, span.LatitudeDegrees);
			Assert.Equal(1, span.LongitudeDegrees);
			Assert.True(span.Radius.Kilometers > 54 && span.Radius.Kilometers < 56);
		}

		[Fact]
		public void EqualsTest()
		{
			Assert.True(new MapSpan(new Location(1, 2), 3, 4) == new MapSpan(new Location(1, 2), 3, 4));
			Assert.True(new MapSpan(new Location(1, 2), 3, 4) != new MapSpan(new Location(2, 3), 4, 5));
			Assert.True(new MapSpan(new Location(1, 2), 3, 4).Equals(new MapSpan(new Location(1, 2), 3, 4)));
			Assert.False(new MapSpan(new Location(1, 2), 3, 4).Equals("MapSpan"));
			Assert.False(new MapSpan(new Location(1, 2), 3, 4).Equals(null));
		}

		[Fact]
		public void HashCode()
		{
			Assert.Equal(new MapSpan(new Location(1, 2), 3, 4).GetHashCode(), new MapSpan(new Location(1, 2), 3, 4).GetHashCode());
		}

		[Fact]
		public void RangeClamping()
		{
			var span = new MapSpan(new Location(0, 0), -1, -2);
			Assert.True(span.LatitudeDegrees > 0);
			Assert.True(span.LongitudeDegrees > 0);
		}

		[Fact]
		public void FromLocationsThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => MapSpan.FromLocations(null!));
		}

		[Fact]
		public void FromLocationsThrowsOnEmpty()
		{
			Assert.Throws<ArgumentException>(() => MapSpan.FromLocations(Array.Empty<Location>()));
		}

		[Fact]
		public void FromLocationsSingleUsesOneKmRadius()
		{
			var loc = new Location(47.6, -122.3);
			var span = MapSpan.FromLocations(new[] { loc });

			Assert.Equal(47.6, span.Center.Latitude);
			Assert.Equal(-122.3, span.Center.Longitude);
			Assert.True(span.Radius.Kilometers > 0.9 && span.Radius.Kilometers < 1.1);
		}

		[Fact]
		public void FromLocationsMultipleComputesBoundingBox()
		{
			var locations = new[]
			{
				new Location(40.0, -74.0), // New York area
				new Location(42.0, -72.0), // Boston area
			};

			var span = MapSpan.FromLocations(locations);

			Assert.Equal(41.0, span.Center.Latitude, 1);
			Assert.Equal(-73.0, span.Center.Longitude, 1);
			// With 10% padding: lat span = 2.0 * 1.1 = 2.2, lon span = 2.0 * 1.1 = 2.2
			Assert.Equal(2.2, span.LatitudeDegrees, 1);
			Assert.Equal(2.2, span.LongitudeDegrees, 1);
		}

		[Fact]
		public void FromLocationsEncompassesAllPoints()
		{
			var locations = new List<Location>
			{
				new Location(10, 20),
				new Location(30, 40),
				new Location(20, 30),
			};

			var span = MapSpan.FromLocations(locations);

			// Center should be midpoint
			Assert.Equal(20.0, span.Center.Latitude);
			Assert.Equal(30.0, span.Center.Longitude);
		}
	}
}
