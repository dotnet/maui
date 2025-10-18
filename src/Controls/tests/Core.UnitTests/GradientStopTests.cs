#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class GradientStopTests
    {
        /// <summary>
        /// Tests that Equals returns false when passed a null object.
        /// </summary>
        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var gradientStop = new GradientStop();

            // Act
            var result = gradientStop.Equals(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when passed an object of a different type.
        /// </summary>
        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var gradientStop = new GradientStop();
            var differentObject = "not a gradient stop";

            // Act
            var result = gradientStop.Equals(differentObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing an object to itself.
        /// </summary>
        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var gradientStop = new GradientStop(Colors.Red, 0.5f);

            // Act
            var result = gradientStop.Equals(gradientStop);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests Equals with various color and offset combinations.
        /// Input conditions: Different combinations of colors and offsets.
        /// Expected result: True when both color and offset match (within tolerance), false otherwise.
        /// </summary>
        [Theory]
        [InlineData(true, 0.0f, 0.0f, true)]  // Same default values
        [InlineData(true, 0.5f, 0.5f, true)]  // Same non-default values
        [InlineData(true, 0.0f, 0.5f, false)] // Same color, different offset
        [InlineData(false, 0.5f, 0.5f, false)] // Different color, same offset
        [InlineData(false, 0.0f, 0.5f, false)] // Different color and offset
        public void Equals_ColorAndOffsetCombinations_ReturnsExpectedResult(bool sameColor, float offset1, float offset2, bool expected)
        {
            // Arrange
            var color1 = sameColor ? Colors.Red : Colors.Red;
            var color2 = sameColor ? Colors.Red : Colors.Blue;
            var gradientStop1 = new GradientStop(color1, offset1);
            var gradientStop2 = new GradientStop(color2, offset2);

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Equals with offsets that are within the tolerance threshold.
        /// Input conditions: Offsets that differ by less than 0.00001.
        /// Expected result: True since the difference is within tolerance.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.000009f)]
        [InlineData(1.0f, 1.000009f)]
        [InlineData(-1.0f, -1.000009f)]
        public void Equals_OffsetsWithinTolerance_ReturnsTrue(float offset1, float offset2)
        {
            // Arrange
            var gradientStop1 = new GradientStop(Colors.Red, offset1);
            var gradientStop2 = new GradientStop(Colors.Red, offset2);

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests Equals with offsets that are exactly at or outside the tolerance threshold.
        /// Input conditions: Offsets that differ by exactly 0.00001 or more.
        /// Expected result: False since the difference is at or outside tolerance.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.00001f)]  // Exactly at tolerance
        [InlineData(0.0f, 0.00002f)]  // Just outside tolerance
        [InlineData(1.0f, 1.1f)]      // Well outside tolerance
        [InlineData(-1.0f, -0.9f)]    // Well outside tolerance, negative
        public void Equals_OffsetsOutsideTolerance_ReturnsFalse(float offset1, float offset2)
        {
            // Arrange
            var gradientStop1 = new GradientStop(Colors.Red, offset1);
            var gradientStop2 = new GradientStop(Colors.Red, offset2);

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Equals with special float values like NaN, infinity, and extreme values.
        /// Input conditions: Special float values that may cause unexpected behavior.
        /// Expected result: Correct comparison based on the specific float values.
        /// </summary>
        [Theory]
        [InlineData(float.NaN, float.NaN, false)]  // NaN != NaN
        [InlineData(float.PositiveInfinity, float.PositiveInfinity, true)]
        [InlineData(float.NegativeInfinity, float.NegativeInfinity, true)]
        [InlineData(float.MinValue, float.MinValue, true)]
        [InlineData(float.MaxValue, float.MaxValue, true)]
        [InlineData(float.PositiveInfinity, float.NegativeInfinity, false)]
        [InlineData(0.0f, float.NaN, false)]
        public void Equals_SpecialFloatValues_ReturnsExpectedResult(float offset1, float offset2, bool expected)
        {
            // Arrange
            var gradientStop1 = new GradientStop(Colors.Red, offset1);
            var gradientStop2 = new GradientStop(Colors.Red, offset2);

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Equals with null colors on both gradient stops.
        /// Input conditions: Both gradient stops have null colors.
        /// Expected result: True if offsets are also equal within tolerance.
        /// </summary>
        [Fact]
        public void Equals_BothNullColors_ReturnsTrue()
        {
            // Arrange
            var gradientStop1 = new GradientStop();
            var gradientStop2 = new GradientStop();
            // Both should have null color as default

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests Equals when one gradient stop has null color and the other doesn't.
        /// Input conditions: One gradient stop has null color, other has a valid color.
        /// Expected result: False since colors don't match.
        /// </summary>
        [Fact]
        public void Equals_OneNullOneValidColor_ReturnsFalse()
        {
            // Arrange
            var gradientStop1 = new GradientStop(); // null color by default
            var gradientStop2 = new GradientStop(Colors.Red, 0.0f);

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Equals with various color combinations.
        /// Input conditions: Different color values.
        /// Expected result: True only when colors are exactly equal.
        /// </summary>
        [Theory]
        [InlineData(true)]  // Same colors
        [InlineData(false)] // Different colors
        public void Equals_DifferentColors_ReturnsExpectedResult(bool sameColors)
        {
            // Arrange
            var color1 = Colors.Red;
            var color2 = sameColors ? Colors.Red : Colors.Blue;
            var gradientStop1 = new GradientStop(color1, 0.5f);
            var gradientStop2 = new GradientStop(color2, 0.5f);

            // Act
            var result = gradientStop1.Equals(gradientStop2);

            // Assert
            Assert.Equal(sameColors, result);
        }
    }
}
