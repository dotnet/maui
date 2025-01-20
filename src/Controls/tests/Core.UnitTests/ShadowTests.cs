using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShadowTests
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
		public void ShadowTypeConverter_Invalid(string value)
		{
			ShadowTypeConverter converter = new ShadowTypeConverter();
			bool actual = converter.IsValid(value);
			Assert.False(actual);
		}
	}
}