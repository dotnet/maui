using System;

using Microsoft.Maui.Primitives;
using Xunit;

namespace Core.UnitTests
{
    public static class DimensionTests
    {
        /// <summary>
        /// Tests that IsMinimumSet returns false when the input value is NaN (unset).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public static void IsMinimumSet_WithNaNValue_ReturnsFalse()
        {
            // Arrange
            double value = double.NaN;

            // Act
            bool result = Dimension.IsMinimumSet(value);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsMinimumSet returns true for various non-NaN numeric values.
        /// Input conditions include normal numbers, infinities, and boundary values.
        /// Expected result is true for all non-NaN values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(42.5)]
        [InlineData(-42.5)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        [InlineData(1e-100)]
        [InlineData(1e100)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public static void IsMinimumSet_WithNonNaNValues_ReturnsTrue(double value)
        {
            // Arrange
            // (value provided by theory data)

            // Act
            bool result = Dimension.IsMinimumSet(value);

            // Assert
            Assert.True(result);
        }

    }
}