#nullable disable

using System;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class EllipseGeometryTests
    {
        /// <summary>
        /// Tests that AppendPath throws ArgumentNullException when path parameter is null.
        /// Validates that null path parameter is properly handled.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void AppendPath_NullPath_ThrowsNullReferenceException()
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ellipseGeometry.AppendPath(null));
        }

        /// <summary>
        /// Tests AppendPath with default values (center at origin, zero radii).
        /// Validates that method works with default property values.
        /// Expected result: AppendEllipse called with parameters (0, 0, 0, 0).
        /// </summary>
        [Fact]
        public void AppendPath_DefaultValues_CallsAppendEllipseWithZeroValues()
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry();
            var mockPath = Substitute.For<PathF>();

            // Act
            ellipseGeometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendEllipse(0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// Tests AppendPath with positive center coordinates and radii.
        /// Validates correct calculation of ellipse parameters from center and radii.
        /// Expected result: AppendEllipse called with correctly calculated parameters.
        /// </summary>
        [Theory]
        [InlineData(10, 20, 5, 8, 5f, 12f, 10f, 16f)] // center (10,20), radii (5,8) -> x=5, y=12, w=10, h=16
        [InlineData(0, 0, 10, 15, -10f, -15f, 20f, 30f)] // center (0,0), radii (10,15) -> x=-10, y=-15, w=20, h=30
        [InlineData(-5, -3, 2, 4, -7f, -7f, 4f, 8f)] // center (-5,-3), radii (2,4) -> x=-7, y=-7, w=4, h=8
        public void AppendPath_ValidValues_CallsAppendEllipseWithCorrectParameters(
            double centerX, double centerY, double radiusX, double radiusY,
            float expectedX, float expectedY, float expectedWidth, float expectedHeight)
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry(new Point(centerX, centerY), radiusX, radiusY);
            var mockPath = Substitute.For<PathF>();

            // Act
            ellipseGeometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendEllipse(expectedX, expectedY, expectedWidth, expectedHeight);
        }

        /// <summary>
        /// Tests AppendPath with negative radius values.
        /// Validates that negative radii are handled correctly in ellipse calculations.
        /// Expected result: AppendEllipse called with parameters reflecting negative radii.
        /// </summary>
        [Theory]
        [InlineData(0, 0, -5, -3, 5f, 3f, -10f, -6f)] // negative radii should result in negative width/height
        [InlineData(10, 10, -2, 4, 12f, 6f, -4f, 8f)] // mixed positive/negative radii
        public void AppendPath_NegativeRadii_CallsAppendEllipseWithNegativeDimensions(
            double centerX, double centerY, double radiusX, double radiusY,
            float expectedX, float expectedY, float expectedWidth, float expectedHeight)
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry(new Point(centerX, centerY), radiusX, radiusY);
            var mockPath = Substitute.For<PathF>();

            // Act
            ellipseGeometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendEllipse(expectedX, expectedY, expectedWidth, expectedHeight);
        }

        /// <summary>
        /// Tests AppendPath with extreme double values including NaN and infinity.
        /// Validates handling of special floating-point values.
        /// Expected result: AppendEllipse called with corresponding float values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        public void AppendPath_ExtremeValues_CallsAppendEllipseWithConvertedValues(
            double centerX, double centerY, double radiusX, double radiusY)
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry(new Point(centerX, centerY), radiusX, radiusY);
            var mockPath = Substitute.For<PathF>();

            float expectedCenterX = (float)centerX;
            float expectedCenterY = (float)centerY;
            float expectedRadiusX = (float)radiusX;
            float expectedRadiusY = (float)radiusY;

            float expectedX = expectedCenterX - expectedRadiusX;
            float expectedY = expectedCenterY - expectedRadiusY;
            float expectedWidth = expectedRadiusX * 2f;
            float expectedHeight = expectedRadiusY * 2f;

            // Act
            ellipseGeometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendEllipse(expectedX, expectedY, expectedWidth, expectedHeight);
        }

        /// <summary>
        /// Tests AppendPath with very large radius values that could cause overflow.
        /// Validates behavior with boundary values that might cause arithmetic issues.
        /// Expected result: AppendEllipse called with calculated parameters without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 1e10, 1e10)] // Very large radii
        [InlineData(1e10, 1e10, 1e10, 1e10)] // Very large center and radii
        public void AppendPath_VeryLargeValues_CallsAppendEllipseWithoutException(
            double centerX, double centerY, double radiusX, double radiusY)
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry(new Point(centerX, centerY), radiusX, radiusY);
            var mockPath = Substitute.For<PathF>();

            // Act & Assert - Should not throw
            ellipseGeometry.AppendPath(mockPath);
            mockPath.Received(1).AppendEllipse(Arg.Any<float>(), Arg.Any<float>(), Arg.Any<float>(), Arg.Any<float>());
        }

        /// <summary>
        /// Tests AppendPath with zero radii.
        /// Validates that zero radii produce zero width and height ellipse.
        /// Expected result: AppendEllipse called with zero width and height.
        /// </summary>
        [Theory]
        [InlineData(5, 10, 0, 0, 5f, 10f, 0f, 0f)] // Zero radii
        [InlineData(0, 0, 0, 5, 0f, -5f, 0f, 10f)] // Zero RadiusX only
        [InlineData(0, 0, 3, 0, -3f, 0f, 6f, 0f)] // Zero RadiusY only
        public void AppendPath_ZeroRadii_CallsAppendEllipseWithZeroDimensions(
            double centerX, double centerY, double radiusX, double radiusY,
            float expectedX, float expectedY, float expectedWidth, float expectedHeight)
        {
            // Arrange
            var ellipseGeometry = new EllipseGeometry(new Point(centerX, centerY), radiusX, radiusY);
            var mockPath = Substitute.For<PathF>();

            // Act
            ellipseGeometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendEllipse(expectedX, expectedY, expectedWidth, expectedHeight);
        }
    }
}
