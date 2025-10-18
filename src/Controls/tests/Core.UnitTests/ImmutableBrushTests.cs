#nullable disable

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for ImmutableBrush class.
    /// </summary>
    public class ImmutableBrushTests
    {
        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with default black color.
        /// Input: Default Color (black: R=0, G=0, B=0, A=1)
        /// Expected: Color property should return the same black color
        /// </summary>
        [Fact]
        public void Color_WithDefaultBlackColor_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color();
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with RGB values.
        /// Input: RGB Color (red: R=1, G=0, B=0, A=1)
        /// Expected: Color property should return the same red color
        /// </summary>
        [Fact]
        public void Color_WithRgbRedColor_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(1.0f, 0.0f, 0.0f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with RGBA values including alpha.
        /// Input: RGBA Color with transparency (R=0.5, G=0.7, B=0.3, A=0.5)
        /// Expected: Color property should return the same color with alpha
        /// </summary>
        [Fact]
        public void Color_WithRgbaColorIncludingAlpha_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(0.5f, 0.7f, 0.3f, 0.5f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with byte RGB values.
        /// Input: Byte RGB Color (R=255, G=128, B=64)
        /// Expected: Color property should return the same color converted to float values
        /// </summary>
        [Fact]
        public void Color_WithByteRgbColor_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(255, 128, 64);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with byte RGBA values.
        /// Input: Byte RGBA Color (R=100, G=200, B=50, A=150)
        /// Expected: Color property should return the same color converted to float values
        /// </summary>
        [Fact]
        public void Color_WithByteRgbaColor_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(100, 200, 50, 150);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with grayscale value.
        /// Input: Grayscale Color (gray level 0.6)
        /// Expected: Color property should return the same grayscale color
        /// </summary>
        [Fact]
        public void Color_WithGrayscaleColor_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(0.6f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was set in the constructor with fully transparent color.
        /// Input: Fully transparent color (R=1, G=1, B=1, A=0)
        /// Expected: Color property should return the same fully transparent color
        /// </summary>
        [Fact]
        public void Color_WithFullyTransparentColor_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was created from static factory method.
        /// Input: Color created using Color.FromRgb static method
        /// Expected: Color property should return the same color
        /// </summary>
        [Fact]
        public void Color_WithColorFromStaticFactory_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = Color.FromRgb(0.8f, 0.2f, 0.9f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter returns the color that was created from RGBA factory method.
        /// Input: Color created using Color.FromRgba with specific alpha value
        /// Expected: Color property should return the same color with alpha
        /// </summary>
        [Fact]
        public void Color_WithColorFromRgbaFactory_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = Color.FromRgba(0.3f, 0.6f, 0.9f, 0.75f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter works correctly with boundary values (minimum values).
        /// Input: Color with all minimum float values (R=0, G=0, B=0, A=0)
        /// Expected: Color property should return the same minimum values
        /// </summary>
        [Fact]
        public void Color_WithMinimumValues_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the Color property getter works correctly with boundary values (maximum values).
        /// Input: Color with all maximum float values (R=1, G=1, B=1, A=1)
        /// Expected: Color property should return the same maximum values
        /// </summary>
        [Fact]
        public void Color_WithMaximumValues_ReturnsCorrectColor()
        {
            // Arrange
            var expectedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            var brush = new ImmutableBrush(expectedColor);

            // Act
            var actualColor = brush.Color;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red);
            Assert.Equal(expectedColor.Green, actualColor.Green);
            Assert.Equal(expectedColor.Blue, actualColor.Blue);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha);
        }
    }
}
