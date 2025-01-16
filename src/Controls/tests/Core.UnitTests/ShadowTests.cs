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

		[Fact]
		public void TestShadowTypeConverter()
		{
			var converter = new ShadowTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			// Test converting from string to Shadow (format 1)
			var shadow1 = (Shadow)converter.ConvertFromInvariantString("#000000 4 4");
			Assert.NotNull(shadow1);
			Assert.Equal(Color.FromArgb("#000000"), (shadow1.Brush as SolidColorBrush)?.Color);
			Assert.Equal(new Point(4, 4), shadow1.Offset);

			// Test converting from string to Shadow (format 2)
			var shadow2 = (Shadow)converter.ConvertFromInvariantString("4 4 16 #FF00FF");
			Assert.NotNull(shadow2);
			Assert.Equal(Color.FromArgb("#FF00FF"), (shadow2.Brush as SolidColorBrush)?.Color);
			Assert.Equal(new Point(4, 4), shadow2.Offset);
			Assert.Equal(16, shadow2.Radius);

			// Test converting from string to Shadow (format 3)
			var shadow3 = (Shadow)converter.ConvertFromInvariantString("4 4 16 #00FF00 0.5");
			Assert.NotNull(shadow3);
			Assert.Equal(Color.FromArgb("#00FF00"), (shadow3.Brush as SolidColorBrush)?.Color);
			Assert.Equal(new Point(4, 4), shadow3.Offset);
			Assert.Equal(16, shadow3.Radius);
			Assert.Equal(0.5f, shadow3.Opacity);

			// Test for converting Shadow to a string
			var shadow = new Shadow
			{
				Brush = new SolidColorBrush(Color.FromArgb("#123456")),
				Offset = new Point(10, 20),
				Radius = 30,
				Opacity = 0.8f
			};

			var shadowString = converter.ConvertToInvariantString(shadow);
			Assert.Equal("10 20 30 #123456 0.8", shadowString);

			// Test some problematic cases
			Assert.Throws<ArgumentNullException>(() => converter.ConvertFromInvariantString(null));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(""));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("invalid"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("#ZZZZZZ 4 4"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("4 4 #000000"));

			Assert.Throws<ArgumentNullException>(() => converter.ConvertToInvariantString(null));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertToInvariantString("invalid"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertToInvariantString(new { }));
		}
	}
}