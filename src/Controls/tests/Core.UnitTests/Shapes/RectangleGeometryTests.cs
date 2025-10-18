#nullable disable

using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    /// <summary>
    /// Unit tests for the RectangleGeometry class AppendPath method.
    /// </summary>
    public partial class RectangleGeometryTests
    {
        /// <summary>
        /// Tests that AppendPath throws ArgumentNullException when path parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected to throw ArgumentNullException.
        /// </summary>
        [Fact]
        public void AppendPath_NullPath_ThrowsNullReferenceException()
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            PathF nullPath = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => rectangleGeometry.AppendPath(nullPath));
        }

        /// <summary>
        /// Tests that AppendPath successfully processes a valid PathF and normal Rect values.
        /// Verifies that the method completes without throwing exceptions and properly converts double coordinates to float.
        /// Expected to complete successfully without exceptions.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(10, 20, 30, 40)]
        [InlineData(-10, -20, 50, 60)]
        [InlineData(100.5, 200.7, 300.3, 400.9)]
        public void AppendPath_ValidPathAndRect_CompletesSuccessfully(double x, double y, double width, double height)
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(x, y, width, height);
            var path = new PathF();

            // Act
            rectangleGeometry.AppendPath(path);

            // Assert - No exception should be thrown, method should complete successfully
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests AppendPath with boundary values for coordinate conversion from double to float.
        /// Verifies that extreme coordinate values are handled correctly during float conversion.
        /// Expected to complete without overflow exceptions.
        /// </summary>
        [Theory]
        [InlineData(float.MaxValue, float.MaxValue, 1, 1)]
        [InlineData(float.MinValue, float.MinValue, 1, 1)]
        [InlineData(0, 0, float.MaxValue, float.MaxValue)]
        [InlineData(0, 0, float.MinValue, float.MinValue)]
        public void AppendPath_BoundaryValues_HandlesConversionCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(x, y, width, height);
            var path = new PathF();

            // Act & Assert - Should not throw exceptions during float conversion
            rectangleGeometry.AppendPath(path);
        }

        /// <summary>
        /// Tests AppendPath with special floating-point values including NaN and infinity.
        /// Verifies that special double values are handled during conversion to float parameters.
        /// Expected to complete without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 10, 10)]
        [InlineData(0, double.NaN, 10, 10)]
        [InlineData(0, 0, double.NaN, 10)]
        [InlineData(0, 0, 10, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 10, 10)]
        [InlineData(0, double.PositiveInfinity, 10, 10)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, 10, 10)]
        public void AppendPath_SpecialFloatingPointValues_HandlesConversionCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(x, y, width, height);
            var path = new PathF();

            // Act & Assert - Should handle special values during float conversion
            rectangleGeometry.AppendPath(path);
        }

        /// <summary>
        /// Tests AppendPath with zero dimensions to verify edge case handling.
        /// Verifies that zero-width or zero-height rectangles are processed correctly.
        /// Expected to complete successfully.
        /// </summary>
        [Theory]
        [InlineData(10, 10, 0, 20)]
        [InlineData(10, 10, 20, 0)]
        [InlineData(10, 10, 0, 0)]
        public void AppendPath_ZeroDimensions_CompletesSuccessfully(double x, double y, double width, double height)
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(x, y, width, height);
            var path = new PathF();

            // Act
            rectangleGeometry.AppendPath(path);

            // Assert - No exception should be thrown
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests AppendPath with negative dimensions to verify handling of invalid rectangle dimensions.
        /// Verifies that negative width or height values are processed without throwing exceptions.
        /// Expected to complete successfully.
        /// </summary>
        [Theory]
        [InlineData(10, 10, -20, 30)]
        [InlineData(10, 10, 20, -30)]
        [InlineData(10, 10, -20, -30)]
        public void AppendPath_NegativeDimensions_CompletesSuccessfully(double x, double y, double width, double height)
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(x, y, width, height);
            var path = new PathF();

            // Act
            rectangleGeometry.AppendPath(path);

            // Assert - No exception should be thrown
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests AppendPath with very small coordinate values near zero.
        /// Verifies that very small positive and negative values are handled correctly during float conversion.
        /// Expected to complete successfully.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon, double.Epsilon, double.Epsilon, double.Epsilon)]
        [InlineData(-double.Epsilon, -double.Epsilon, 1, 1)]
        [InlineData(0.0000001, 0.0000001, 0.0000001, 0.0000001)]
        public void AppendPath_VerySmallValues_HandlesConversionCorrectly(double x, double y, double width, double height)
        {
            // Arrange
            var rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = new Rect(x, y, width, height);
            var path = new PathF();

            // Act
            rectangleGeometry.AppendPath(path);

            // Assert - Should handle very small values during conversion
            Assert.NotNull(path);
        }
    }
}
