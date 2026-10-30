using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShadowTests : BaseTestFixture
	{
		[Fact]
		public void ShadowInitializesCorrectly()
		{
			// Arrange
			const float expectedOpacity = 1.0f;
			const float expectedRadius = 10.0f;
			var expectedOffset = new Point(10, 10);

			// Act
			var shadow = new Shadow
			{
				Offset = expectedOffset,
				Opacity = expectedOpacity,
				Radius = expectedRadius
			};

			// Assert
			Assert.Equal(expectedOffset, shadow.Offset);
			Assert.Equal(expectedOpacity, shadow.Opacity);
			Assert.Equal(expectedRadius, shadow.Radius);
		}

		[Theory]
		[InlineData("#000000 4 4")]
		[InlineData("#123 4 4")]
		[InlineData("#1234 4 4")]
		[InlineData("#12345 4 4")]
		[InlineData("#1234567 4 4")]
		[InlineData("#12345678 4 4")]
		[InlineData("rgb(6, 201, 198) 4 4")]
		[InlineData("rgba(6, 201, 188, 0.2) 4 8")]
		[InlineData("hsl(6, 20%, 45%) 1 5")]
		[InlineData("hsla(6, 20%, 45%,0.75) 6 3")]
		[InlineData("fuchsia 4 4")]
		[InlineData("rgb(100%, 32%, 64%) 8 5")]
		[InlineData("rgba(100%, 32%, 64%,0.27) 16 5")]
		[InlineData("hsv(6, 20%, 45%) 1 5")]
		[InlineData("hsva(6, 20%, 45%,0.75) 6 3")]
		[InlineData("4 4 16 #FF00FF")]
		[InlineData("4 4 16 AliceBlue")]
		[InlineData("5 8 8 rgb(6, 201, 198)")]
		[InlineData("7 5 4 rgba(6, 201, 188, 0.2)")]
		[InlineData("9 4 6 hsl(6, 20%, 45%)")]
		[InlineData("8 1 5 hsla(6, 20%, 45%,0.75)")]
		[InlineData("5 2 8 rgb(100%, 32%, 64%)")]
		[InlineData("1 5 3 rgba(100%, 32%, 64%,0.27)")]
		[InlineData("4 4 16 #00FF00 0.5")]
		[InlineData("4 4 16 limegreen 0.5")]
		[InlineData("5 8 8 rgb(6, 201, 198) 0.5")]
		[InlineData("7 5 4 rgba(6, 201, 188, 0.2) 0.5")]
		[InlineData("9 4 6 hsl(6, 20%, 45%) 0.5")]
		[InlineData("8 1 5 hsla(6, 20%, 45%,0.75) 0.5")]
		[InlineData("9 4 6 hsv(6, 20%, 45%) 0.5")]
		[InlineData("8 1 5 hsva(6, 20%, 45%,0.75) 0.5")]
		[InlineData("5 2 8 rgb(100%, 32%, 64%) 0.5")]
		[InlineData("1 5 3 rgba(100%, 32%, 64%,0.27) 0.5")]
		public void ShadowTypeConverter_Valid(string value)
		{
			var converter = new ShadowTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			bool actual = converter.IsValid(value);
			Assert.True(actual);
		}

		[Theory]
		[InlineData("#000 4 8", 4, 8, 10, 1)]
		[InlineData("-4.5 8e-1 16 #FF00FF", -4.5, 0.8, 16, 1)]
		[InlineData("4 8 16 rgba(6, 201, 188, 0.2) 0.25", 4, 8, 16, 0.25)]
		[InlineData("\t4,\r\n8;16|#FF00FF/0.5!", 4, 8, 16, 0.5)]
		[InlineData("+4 +8 +16 #FF00FF +0.5", 4, 8, 16, 0.5)]
		[InlineData("4 8 16 #123456789", 4, 8, 16, 9)]
		[InlineData("4. 8 16 #000", 4, 8, 16, 1)]
		public void ShadowTypeConverter_ParsesValues(string value, double offsetX, double offsetY, double radius, double opacity)
		{
			var converter = new ShadowTypeConverter();

			var shadow = Assert.IsType<Shadow>(converter.ConvertFromInvariantString(value));

			Assert.Equal(offsetX, shadow.Offset.X, 5);
			Assert.Equal(offsetY, shadow.Offset.Y, 5);
			Assert.Equal(radius, shadow.Radius, 5);
			Assert.Equal(opacity, shadow.Opacity, 5);
			Assert.IsType<SolidColorBrush>(shadow.Brush);
		}

		[Theory]
		[InlineData("rgb(\t6,\r\n201, 198) 4 4")]
		[InlineData("rgba(100%,\u00A032%, 64%, 0.27) 4 4")]
		[InlineData("hsl(6, 20%, 45%)\n1\t5")]
		[InlineData("hsva(6, 20%, 45%, 0.75)\r\n6 3")]
		public void ShadowTypeConverter_AcceptsRegexWhitespace(string value)
		{
			var converter = new ShadowTypeConverter();

			Assert.IsType<Shadow>(converter.ConvertFromInvariantString(value));
		}

		[Theory]
		[InlineData("rgb(255, 0, 0) 1 2")]
		[InlineData("rgb(100%, 0%, 0%) 1 2")]
		[InlineData("rgba(255, 0, 0, 0.5) 1 2")]
		[InlineData("rgba(100%, 0%, 0%, 0.5) 1 2")]
		[InlineData("hsl(120, 100%, 50%) 1 2")]
		[InlineData("hsla(120, 100%, 50%, 0.5) 1 2")]
		[InlineData("hsv(120, 100%, 50%) 1 2")]
		[InlineData("hsva(120, 100%, 50%, 0.5) 1 2")]
		[InlineData("AliceBlue 1 2")]
		public void ShadowTypeConverter_AcceptsSupportedColorSyntax(string value)
		{
			var converter = new ShadowTypeConverter();

			var shadow = Assert.IsType<Shadow>(converter.ConvertFromInvariantString(value));

			Assert.IsType<SolidColorBrush>(shadow.Brush);
		}

		[Fact]
		public void ShadowTypeConverter_UsesInvariantCulture()
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
			var converter = new ShadowTypeConverter();

			var shadow = Assert.IsType<Shadow>(converter.ConvertFrom("4.5 8.25 16 #FF00FF 0.5"));

			Assert.Equal(4.5, shadow.Offset.X, 5);
			Assert.Equal(8.25, shadow.Offset.Y, 5);
			Assert.Equal(0.5, shadow.Opacity, 5);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("invalid")]
		[InlineData("#ZZZZZZ 4 4")]
		[InlineData("4 4 #000000")]
		[InlineData("4 4 dotnetpurple")]
		[InlineData("rgb(6, 14.5, 198) 4 4")]
		[InlineData("argb(0.2, 6, 201, 188) 4 8")]
		[InlineData("hsl(6, 20%, 45.8%) 1 5")]
		[InlineData("hsla(6.8, 20%, 45%,0.75) 6 3")]
		[InlineData("hsv(6, 20%, 45.8%) 1 5")]
		[InlineData("hsva(6.8, 20%, 45%,0.75) 6 3")]
		[InlineData("rgb(100%, 32.9%, 64%) 8 5")]
		[InlineData("argb(0.27, 100%, 32%, 64%) 16 5")]
		[InlineData("#12 4 4")]
		[InlineData("#123456789 4 4")]
		[InlineData("RGB(6, 201, 198) 4 4")]
		[InlineData("rgba(6, 201, 188, .2) 4 8")]
		[InlineData("rgba(6, 201, 188, 2.) 4 8")]
		[InlineData("rgb(100 %, 0%, 0%) 1 2")]
		[InlineData("hsl(6, 20 %, 45%) 1 5")]
		[InlineData("4e 4 4")]
		[InlineData("4 4 16 #000000 0.5 extra")]
		[InlineData("NaN 4 4 16 #000000")]
		[InlineData("Infinity 4 4 16 #000000")]
		public void ShadowTypeConverter_Invalid(string value)
		{
			ShadowTypeConverter converter = new ShadowTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}

		[Theory]
		[InlineData("4 4")]
		[InlineData("4 4 16")]
		[InlineData("4 4 16 #ZZZZZZ")]
		[InlineData("4 4 16 rgb(6, 14.5, 198)")]
		[InlineData("4 4 16 #000000 0.5 1")]
		public void ShadowTypeConverter_MalformedInputThrows(string value)
		{
			var converter = new ShadowTypeConverter();

			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(value));
		}
	}
}