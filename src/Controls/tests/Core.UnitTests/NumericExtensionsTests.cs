using System;

using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class NumericExtensionsTests
    {
        [Fact]
        public void InRange()
        {
            Assert.Equal(5, 5.Clamp(0, 10));
        }

        [Fact]
        public void BelowMin()
        {
            Assert.Equal(5, 0.Clamp(5, 10));
        }

        [Fact]
        public void AboveMax()
        {
            Assert.Equal(5, 10.Clamp(0, 5));
        }

        [Fact]
        public void MinMaxWrong()
        {
            Assert.Equal(0, 10.Clamp(5, 0));
            Assert.Equal(0, 5.Clamp(10, 0));
            Assert.Equal(5, 0.Clamp(10, 5));
        }

        /// <summary>
        /// Tests that IsCloseTo returns true when the absolute difference between two doubles is exactly at the tolerance boundary (0.001).
        /// </summary>
        [Fact]
        public void IsCloseTo_DifferenceExactlyAtTolerance_ReturnsTrue()
        {
            // Arrange
            double sizeA = 1.0;
            double sizeB = 1.001;

            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsCloseTo returns false when the absolute difference between two doubles is greater than the tolerance (0.001).
        /// </summary>
        [Theory]
        [InlineData(1.0, 1.002)]
        [InlineData(0.0, 0.002)]
        [InlineData(5.5, 5.502)]
        [InlineData(-1.0, -0.998)]
        [InlineData(100.0, 100.01)]
        public void IsCloseTo_DifferenceGreaterThanTolerance_ReturnsFalse(double sizeA, double sizeB)
        {
            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsCloseTo returns true when the absolute difference between two doubles is less than the tolerance (0.001).
        /// </summary>
        [Theory]
        [InlineData(1.0, 1.0005)]
        [InlineData(0.0, 0.0005)]
        [InlineData(5.5, 5.5009)]
        [InlineData(-1.0, -0.9995)]
        [InlineData(2.5, 2.4999)]
        public void IsCloseTo_DifferenceLessThanTolerance_ReturnsTrue(double sizeA, double sizeB)
        {
            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsCloseTo returns true when both values are identical.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void IsCloseTo_IdenticalValues_ReturnsTrue(double sizeA, double sizeB)
        {
            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsCloseTo handles special double values correctly, including NaN and infinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.NaN, 1.0)]
        [InlineData(1.0, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void IsCloseTo_SpecialDoubleValues_ReturnsTrue(double sizeA, double sizeB)
        {
            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsCloseTo returns false when comparing finite values with infinity values.
        /// </summary>
        [Theory]
        [InlineData(1.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, 1.0)]
        [InlineData(1.0, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, 1.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        public void IsCloseTo_FiniteVsInfinityValues_ReturnsFalse(double sizeA, double sizeB)
        {
            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsCloseTo handles extreme boundary values correctly, including maximum and minimum double values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue - 1)]
        [InlineData(double.MinValue, double.MinValue + 1)]
        public void IsCloseTo_ExtremeValues_ReturnsFalse(double sizeA, double sizeB)
        {
            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsCloseTo returns true when the difference is just under the tolerance boundary.
        /// </summary>
        [Fact]
        public void IsCloseTo_DifferenceJustUnderTolerance_ReturnsTrue()
        {
            // Arrange
            double sizeA = 1.0;
            double sizeB = 1.0009; // 0.0009 difference, which is less than 0.001

            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsCloseTo returns false when the difference is just over the tolerance boundary.
        /// </summary>
        [Fact]
        public void IsCloseTo_DifferenceJustOverTolerance_ReturnsFalse()
        {
            // Arrange
            double sizeA = 1.0;
            double sizeB = 1.0011; // 0.0011 difference, which is greater than 0.001

            // Act
            bool result = sizeA.IsCloseTo(sizeB);

            // Assert
            Assert.False(result);
        }
    }
}