using System;
using System.Globalization;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Tests
{
	public class LocationTypeConverter_Tests
	{
		[Fact]
		public void ConvertFromValidString()
		{
			var converter = new LocationTypeConverter();
			var result = (Location)converter.ConvertFrom(null, CultureInfo.InvariantCulture, "36.9628066,-122.0194722")!;

			Assert.Equal(36.9628066, result.Latitude, 7);
			Assert.Equal(-122.0194722, result.Longitude, 7);
		}

		[Fact]
		public void ConvertFromWithSpaces()
		{
			var converter = new LocationTypeConverter();
			var result = (Location)converter.ConvertFrom(null, CultureInfo.InvariantCulture, " 36.9628066 , -122.0194722 ")!;

			Assert.Equal(36.9628066, result.Latitude, 7);
			Assert.Equal(-122.0194722, result.Longitude, 7);
		}

		[Fact]
		public void ConvertFromNegativeValues()
		{
			var converter = new LocationTypeConverter();
			var result = (Location)converter.ConvertFrom(null, CultureInfo.InvariantCulture, "-33.8688,151.2093")!;

			Assert.Equal(-33.8688, result.Latitude, 4);
			Assert.Equal(151.2093, result.Longitude, 4);
		}

		[Fact]
		public void ConvertFromZero()
		{
			var converter = new LocationTypeConverter();
			var result = (Location)converter.ConvertFrom(null, CultureInfo.InvariantCulture, "0,0")!;

			Assert.Equal(0, result.Latitude);
			Assert.Equal(0, result.Longitude);
		}

		[Fact]
		public void ConvertToString()
		{
			var converter = new LocationTypeConverter();
			var location = new Location(36.9628066, -122.0194722);
			var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, location, typeof(string));

			Assert.Equal("36.9628066,-122.0194722", result);
		}

		[Fact]
		public void ConvertFromInvalidStringThrows()
		{
			var converter = new LocationTypeConverter();
			Assert.Throws<InvalidOperationException>(() =>
				converter.ConvertFrom(null, CultureInfo.InvariantCulture, "invalid"));
		}

		[Fact]
		public void ConvertFromEmptyStringThrows()
		{
			var converter = new LocationTypeConverter();
			Assert.Throws<InvalidOperationException>(() =>
				converter.ConvertFrom(null, CultureInfo.InvariantCulture, ""));
		}

		[Fact]
		public void ConvertFromTooManyPartsThrows()
		{
			var converter = new LocationTypeConverter();
			Assert.Throws<InvalidOperationException>(() =>
				converter.ConvertFrom(null, CultureInfo.InvariantCulture, "1,2,3"));
		}

		[Fact]
		public void CanConvertFromString()
		{
			var converter = new LocationTypeConverter();
			Assert.True(converter.CanConvertFrom(null, typeof(string)));
			Assert.False(converter.CanConvertFrom(null, typeof(int)));
		}

		[Fact]
		public void CanConvertToString()
		{
			var converter = new LocationTypeConverter();
			Assert.True(converter.CanConvertTo(null, typeof(string)));
			Assert.False(converter.CanConvertTo(null, typeof(int)));
		}

		[Fact]
		public void RoundTrip()
		{
			var converter = new LocationTypeConverter();
			var original = new Location(47.6062, -122.3321);
			var str = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, original, typeof(string))!;
			var result = (Location)converter.ConvertFrom(null, CultureInfo.InvariantCulture, str)!;

			Assert.Equal(original.Latitude, result.Latitude, 10);
			Assert.Equal(original.Longitude, result.Longitude, 10);
		}
	}
}
