#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
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

        /// <summary>
        /// Tests that the Brush property returns the default value (Brush.Black) when no value has been explicitly set.
        /// This verifies the bindable property default value is correctly applied.
        /// </summary>
        [Fact]
        public void Brush_DefaultValue_ReturnsBrushBlack()
        {
            // Arrange & Act
            var shadow = new Shadow();

            // Assert
            Assert.Equal(Brush.Black, shadow.Brush);
        }

        /// <summary>
        /// Tests that the Brush property setter stores the provided value and the getter returns the same value.
        /// This verifies the basic get/set functionality of the bindable property.
        /// </summary>
        [Fact]
        public void Brush_SetValidBrush_ReturnsSetValue()
        {
            // Arrange
            var shadow = new Shadow();
            var mockBrush = Substitute.For<Brush>();

            // Act
            shadow.Brush = mockBrush;

            // Assert
            Assert.Equal(mockBrush, shadow.Brush);
        }

        /// <summary>
        /// Tests that the Brush property can be set to null and returns null when retrieved.
        /// This verifies null handling since nullable reference types are disabled.
        /// </summary>
        [Fact]
        public void Brush_SetToNull_ReturnsNull()
        {
            // Arrange
            var shadow = new Shadow();
            var mockBrush = Substitute.For<Brush>();
            shadow.Brush = mockBrush; // Set to a value first

            // Act
            shadow.Brush = null;

            // Assert
            Assert.Null(shadow.Brush);
        }

        /// <summary>
        /// Tests that the Brush property can be set to different Brush instances and correctly stores each value.
        /// This verifies the property handles multiple assignments properly.
        /// </summary>
        [Fact]
        public void Brush_SetMultipleBrushes_ReturnsCorrectValues()
        {
            // Arrange
            var shadow = new Shadow();
            var firstBrush = Substitute.For<Brush>();
            var secondBrush = Substitute.For<Brush>();

            // Act & Assert - First brush
            shadow.Brush = firstBrush;
            Assert.Equal(firstBrush, shadow.Brush);

            // Act & Assert - Second brush
            shadow.Brush = secondBrush;
            Assert.Equal(secondBrush, shadow.Brush);
        }

        /// <summary>
        /// Tests that the Brush property works with system-defined brushes like Brush.Black and Brush.Blue.
        /// This verifies compatibility with built-in brush instances.
        /// </summary>
        [Theory]
        [InlineData("Black")]
        [InlineData("Blue")]
        [InlineData("Red")]
        public void Brush_SetSystemBrush_ReturnsCorrectBrush(string brushColorName)
        {
            // Arrange
            var shadow = new Shadow();
            Brush systemBrush = brushColorName switch
            {
                "Black" => Brush.Black,
                "Blue" => Brush.Blue,
                "Red" => Brush.Red,
                _ => throw new ArgumentException($"Unknown brush color: {brushColorName}")
            };

            // Act
            shadow.Brush = systemBrush;

            // Assert
            Assert.Equal(systemBrush, shadow.Brush);
        }

        /// <summary>
        /// Tests that setting the Brush property to the same value multiple times works correctly.
        /// This verifies the property handles redundant assignments without issues.
        /// </summary>
        [Fact]
        public void Brush_SetSameValueMultipleTimes_ReturnsCorrectValue()
        {
            // Arrange
            var shadow = new Shadow();
            var mockBrush = Substitute.For<Brush>();

            // Act
            shadow.Brush = mockBrush;
            shadow.Brush = mockBrush;
            shadow.Brush = mockBrush;

            // Assert
            Assert.Equal(mockBrush, shadow.Brush);
        }
    }
}