using System;
using System.Globalization;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MapSpanTypeConverterTests : BaseTestFixture
	{
		[Fact]
		public void ConvertFromValidString()
		{
			var converter = new MapSpanTypeConverter();
			var result = (MapSpan)converter.ConvertFrom(null, CultureInfo.InvariantCulture, "36.9628,-122.0195,0.01,0.02")!;

			Assert.Equal(36.9628, result.Center.Latitude, 4);
			Assert.Equal(-122.0195, result.Center.Longitude, 4);
			Assert.Equal(0.01, result.LatitudeDegrees, 10);
			Assert.Equal(0.02, result.LongitudeDegrees, 10);
		}

		[Fact]
		public void ConvertFromWithSpaces()
		{
			var converter = new MapSpanTypeConverter();
			var result = (MapSpan)converter.ConvertFrom(null, CultureInfo.InvariantCulture, " 36.9628 , -122.0195 , 0.01 , 0.02 ")!;

			Assert.Equal(36.9628, result.Center.Latitude, 4);
			Assert.Equal(-122.0195, result.Center.Longitude, 4);
		}

		[Fact]
		public void ConvertToString()
		{
			var converter = new MapSpanTypeConverter();
			var span = new MapSpan(new Location(36.9628, -122.0195), 0.01, 0.02);
			var result = converter.ConvertTo(null, CultureInfo.InvariantCulture, span, typeof(string));

			Assert.Equal("36.9628,-122.0195,0.01,0.02", result);
		}

		[Fact]
		public void ConvertFromInvalidStringThrows()
		{
			var converter = new MapSpanTypeConverter();
			Assert.Throws<InvalidOperationException>(() =>
				converter.ConvertFrom(null, CultureInfo.InvariantCulture, "invalid"));
		}

		[Fact]
		public void ConvertFromTooFewPartsThrows()
		{
			var converter = new MapSpanTypeConverter();
			Assert.Throws<InvalidOperationException>(() =>
				converter.ConvertFrom(null, CultureInfo.InvariantCulture, "36.9628,-122.0195"));
		}

		[Fact]
		public void ConvertFromEmptyStringThrows()
		{
			var converter = new MapSpanTypeConverter();
			Assert.Throws<InvalidOperationException>(() =>
				converter.ConvertFrom(null, CultureInfo.InvariantCulture, ""));
		}

		[Fact]
		public void CanConvertFromString()
		{
			var converter = new MapSpanTypeConverter();
			Assert.True(converter.CanConvertFrom(null, typeof(string)));
			Assert.False(converter.CanConvertFrom(null, typeof(int)));
		}

		[Fact]
		public void RoundTrip()
		{
			var converter = new MapSpanTypeConverter();
			var original = new MapSpan(new Location(47.6062, -122.3321), 0.5, 0.5);
			var str = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, original, typeof(string))!;
			var result = (MapSpan)converter.ConvertFrom(null, CultureInfo.InvariantCulture, str)!;

			Assert.Equal(original.Center.Latitude, result.Center.Latitude, 10);
			Assert.Equal(original.Center.Longitude, result.Center.Longitude, 10);
			Assert.Equal(original.LatitudeDegrees, result.LatitudeDegrees, 10);
			Assert.Equal(original.LongitudeDegrees, result.LongitudeDegrees, 10);
		}
	}
}
