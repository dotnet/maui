#nullable disable

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DisplayInfoExtensionsTests
    {
        /// <summary>
        /// Tests that GetScaledScreenSize returns Size.Zero when DisplayInfo has zero density.
        /// This covers the edge case where density equals zero, preventing division by zero.
        /// </summary>
        [Fact]
        public void GetScaledScreenSize_DensityIsZero_ReturnsSizeZero()
        {
            // Arrange
            var displayInfo = new DisplayInfo(1920, 1080, 0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with various density values including edge cases.
        /// Verifies that the scaling calculation works correctly for different density scenarios.
        /// Expected result is width/density and height/density for each test case.
        /// </summary>
        [Theory]
        [InlineData(1920, 1080, 1.0, 1920, 1080)]
        [InlineData(1920, 1080, 2.0, 960, 540)]
        [InlineData(1920, 1080, 3.0, 640, 360)]
        [InlineData(1920, 1080, 0.5, 3840, 2160)]
        [InlineData(1920, 1080, 2.5, 768, 432)]
        [InlineData(100, 200, 1.0, 100, 200)]
        [InlineData(100, 200, 4.0, 25, 50)]
        public void GetScaledScreenSize_ValidDensity_ReturnsCorrectScaledSize(double width, double height, double density, double expectedWidth, double expectedHeight)
        {
            // Arrange
            var displayInfo = new DisplayInfo(width, height, density, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(expectedWidth, result.Width, 10);
            Assert.Equal(expectedHeight, result.Height, 10);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with negative density value.
        /// Verifies behavior when density is negative, which would result in negative scaled dimensions.
        /// </summary>
        [Fact]
        public void GetScaledScreenSize_NegativeDensity_ReturnsNegativeScaledSize()
        {
            // Arrange
            var displayInfo = new DisplayInfo(1920, 1080, -2.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(-960, result.Width, 10);
            Assert.Equal(-540, result.Height, 10);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with zero width and height values.
        /// Verifies that zero dimensions result in zero scaled dimensions regardless of density.
        /// </summary>
        [Fact]
        public void GetScaledScreenSize_ZeroWidthAndHeight_ReturnsZeroSize()
        {
            // Arrange
            var displayInfo = new DisplayInfo(0, 0, 2.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(0, result.Width);
            Assert.Equal(0, result.Height);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with negative width and height values.
        /// Verifies that negative dimensions are scaled correctly maintaining their sign.
        /// </summary>
        [Fact]
        public void GetScaledScreenSize_NegativeWidthAndHeight_ReturnsNegativeScaledSize()
        {
            // Arrange
            var displayInfo = new DisplayInfo(-1920, -1080, 2.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(-960, result.Width, 10);
            Assert.Equal(-540, result.Height, 10);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with very small density value close to zero.
        /// Verifies that very small density values produce very large scaled dimensions without causing overflow.
        /// </summary>
        [Fact]
        public void GetScaledScreenSize_VerySmallDensity_ReturnsVeryLargeSize()
        {
            // Arrange
            var displayInfo = new DisplayInfo(100, 50, 0.001, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(100000, result.Width, 1);
            Assert.Equal(50000, result.Height, 1);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with very large density value.
        /// Verifies that large density values produce very small scaled dimensions.
        /// </summary>
        [Fact]
        public void GetScaledScreenSize_VeryLargeDensity_ReturnsVerySmallSize()
        {
            // Arrange
            var displayInfo = new DisplayInfo(1920, 1080, 1000.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            Assert.Equal(1.92, result.Width, 2);
            Assert.Equal(1.08, result.Height, 2);
        }

        /// <summary>
        /// Tests GetScaledScreenSize with special double values for density.
        /// Verifies behavior when density contains special floating-point values like NaN and infinities.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void GetScaledScreenSize_SpecialDensityValues_ReturnsSpecialValues(double specialDensity)
        {
            // Arrange
            var displayInfo = new DisplayInfo(1920, 1080, specialDensity, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            if (double.IsNaN(specialDensity))
            {
                Assert.True(double.IsNaN(result.Width));
                Assert.True(double.IsNaN(result.Height));
            }
            else if (double.IsPositiveInfinity(specialDensity))
            {
                Assert.Equal(0, result.Width);
                Assert.Equal(0, result.Height);
            }
            else if (double.IsNegativeInfinity(specialDensity))
            {
                Assert.Equal(0, result.Width);
                Assert.Equal(0, result.Height);
            }
        }

        /// <summary>
        /// Tests GetScaledScreenSize with special double values for width and height.
        /// Verifies behavior when width or height contain special floating-point values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1080)]
        [InlineData(1920, double.NaN)]
        [InlineData(double.PositiveInfinity, 1080)]
        [InlineData(1920, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 1080)]
        [InlineData(1920, double.NegativeInfinity)]
        public void GetScaledScreenSize_SpecialWidthHeightValues_ReturnsSpecialValues(double width, double height)
        {
            // Arrange
            var displayInfo = new DisplayInfo(width, height, 2.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            if (double.IsNaN(width))
            {
                Assert.True(double.IsNaN(result.Width));
            }
            else if (double.IsInfinity(width))
            {
                Assert.True(double.IsInfinity(result.Width));
            }

            if (double.IsNaN(height))
            {
                Assert.True(double.IsNaN(result.Height));
            }
            else if (double.IsInfinity(height))
            {
                Assert.True(double.IsInfinity(result.Height));
            }
        }

        /// <summary>
        /// Tests GetScaledScreenSize with maximum and minimum double values.
        /// Verifies behavior at the extreme boundaries of double precision.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, 1.0)]
        [InlineData(double.MinValue, double.MinValue, 1.0)]
        [InlineData(1920, 1080, double.MaxValue)]
        [InlineData(1920, 1080, double.MinValue)]
        public void GetScaledScreenSize_ExtremeDoubleValues_HandlesCorrectly(double width, double height, double density)
        {
            // Arrange
            var displayInfo = new DisplayInfo(width, height, density, DisplayOrientation.Portrait, DisplayRotation.Rotation0);

            // Act
            var result = displayInfo.GetScaledScreenSize();

            // Assert
            // For extreme values, we just verify the operation doesn't throw and produces a result
            Assert.NotNull(result);

            if (density == 0)
            {
                Assert.Equal(Size.Zero, result);
            }
            else
            {
                // For extreme values, the result may be infinity or very large/small numbers
                // We just verify the calculation was performed (width/density, height/density)
                Assert.Equal(width / density, result.Width);
                Assert.Equal(height / density, result.Height);
            }
        }
    }
}
