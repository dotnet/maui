#nullable disable

using System;
using System.Collections.ObjectModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the RoundRectangleGeometry class.
    /// </summary>
    public class RoundRectangleGeometryTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates a valid RoundRectangleGeometry instance.
        /// Verifies that the instance is properly initialized and inherits from GeometryGroup.
        /// Expected result: A valid RoundRectangleGeometry instance is created.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Act
            var geometry = new RoundRectangleGeometry();

            // Assert
            Assert.NotNull(geometry);
            Assert.IsType<RoundRectangleGeometry>(geometry);
            Assert.IsAssignableFrom<GeometryGroup>(geometry);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes properties with correct default values.
        /// Verifies that Rect property defaults to an empty rectangle and CornerRadius defaults to zero radius.
        /// Expected result: Properties are initialized with default values.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesPropertiesWithDefaultValues()
        {
            // Act
            var geometry = new RoundRectangleGeometry();

            // Assert
            Assert.Equal(new Rect(), geometry.Rect);
            Assert.Equal(new CornerRadius(), geometry.CornerRadius);
        }

        /// <summary>
        /// Tests that the parameterless constructor properly initializes bindable properties.
        /// Verifies that the bindable properties RectProperty and CornerRadiusProperty are accessible and have default values.
        /// Expected result: Bindable properties are properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesBindableProperties()
        {
            // Act
            var geometry = new RoundRectangleGeometry();

            // Assert
            Assert.Equal(new Rect(), geometry.GetValue(RoundRectangleGeometry.RectProperty));
            Assert.Equal(new CornerRadius(), geometry.GetValue(RoundRectangleGeometry.CornerRadiusProperty));
        }

        /// <summary>
        /// Tests that AppendPath throws ArgumentNullException when path parameter is null.
        /// </summary>
        [Fact]
        public void AppendPath_WithNullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => geometry.AppendPath(null));
        }

        /// <summary>
        /// Tests that AppendPath correctly extracts rectangle coordinates and corner radii and calls AppendRoundedRectangle with proper float conversions.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(10.5, 20.7, 30.2, 40.8, 5.1, 6.2, 7.3, 8.4)]
        [InlineData(-10, -20, 50, 60, 0, 0, 0, 0)]
        [InlineData(100.999, 200.001, 300.5, 400.5, 10.25, 15.75, 20.125, 25.875)]
        public void AppendPath_WithValidInputs_CallsAppendRoundedRectangleWithCorrectParameters(
            double rectX, double rectY, double rectWidth, double rectHeight,
            double topLeft, double topRight, double bottomLeft, double bottomRight)
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();
            var mockPath = Substitute.For<PathF>();
            var rect = new Rect(rectX, rectY, rectWidth, rectHeight);
            var cornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);

            geometry.Rect = rect;
            geometry.CornerRadius = cornerRadius;

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(
                (float)rectX,
                (float)rectY,
                (float)rectWidth,
                (float)rectHeight,
                (float)topLeft,
                (float)topRight,
                (float)bottomLeft,
                (float)bottomRight);
        }

        /// <summary>
        /// Tests that AppendPath handles extreme double values by properly casting to float.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(1.7976931348623157E+308, 1.7976931348623157E+308, 1.7976931348623157E+308, 1.7976931348623157E+308)]
        public void AppendPath_WithExtremeDoubleValues_CastsToFloatCorrectly(
            double rectValue, double cornerValue1, double cornerValue2, double cornerValue3)
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();
            var mockPath = Substitute.For<PathF>();
            var rect = new Rect(rectValue, rectValue, rectValue, rectValue);
            var cornerRadius = new CornerRadius(cornerValue1, cornerValue2, cornerValue3, cornerValue3);

            geometry.Rect = rect;
            geometry.CornerRadius = cornerRadius;

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(
                (float)rectValue,
                (float)rectValue,
                (float)rectValue,
                (float)rectValue,
                (float)cornerValue1,
                (float)cornerValue2,
                (float)cornerValue3,
                (float)cornerValue3);
        }

        /// <summary>
        /// Tests that AppendPath handles special floating point values like NaN and Infinity.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0, 0)]
        [InlineData(0, double.NaN, 0, 0)]
        [InlineData(0, 0, double.PositiveInfinity, 0)]
        [InlineData(0, 0, 0, double.NegativeInfinity)]
        public void AppendPath_WithSpecialFloatingPointValues_HandlesCorrectly(
            double rectX, double rectY, double cornerTopLeft, double cornerTopRight)
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();
            var mockPath = Substitute.For<PathF>();
            var rect = new Rect(rectX, rectY, 100, 100);
            var cornerRadius = new CornerRadius(cornerTopLeft, cornerTopRight, 0, 0);

            geometry.Rect = rect;
            geometry.CornerRadius = cornerRadius;

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(
                (float)rectX,
                (float)rectY,
                100f,
                100f,
                (float)cornerTopLeft,
                (float)cornerTopRight,
                0f,
                0f);
        }

        /// <summary>
        /// Tests that AppendPath works correctly with default constructor values.
        /// </summary>
        [Fact]
        public void AppendPath_WithDefaultValues_CallsAppendRoundedRectangleWithZeroValues()
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();
            var mockPath = Substitute.For<PathF>();

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// Tests that AppendPath works correctly with parameterized constructor values.
        /// </summary>
        [Fact]
        public void AppendPath_WithParameterizedConstructorValues_CallsAppendRoundedRectangleCorrectly()
        {
            // Arrange
            var rect = new Rect(15.5, 25.3, 35.7, 45.9);
            var cornerRadius = new CornerRadius(12.1, 13.2, 14.3, 15.4);
            var geometry = new RoundRectangleGeometry(cornerRadius, rect);
            var mockPath = Substitute.For<PathF>();

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(15.5f, 25.3f, 35.7f, 45.9f, 12.1f, 13.2f, 14.3f, 15.4f);
        }

        /// <summary>
        /// Tests that AppendPath handles negative rectangle dimensions.
        /// </summary>
        [Theory]
        [InlineData(-100, 50, -50, 100)]
        [InlineData(100, -50, 50, -100)]
        [InlineData(-100, -50, -50, -100)]
        public void AppendPath_WithNegativeRectangleDimensions_PassesValuesCorrectly(
            double width, double height, double x, double y)
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();
            var mockPath = Substitute.For<PathF>();
            var rect = new Rect(x, y, width, height);
            var cornerRadius = new CornerRadius(5, 10, 15, 20);

            geometry.Rect = rect;
            geometry.CornerRadius = cornerRadius;

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(
                (float)x,
                (float)y,
                (float)width,
                (float)height,
                5f,
                10f,
                15f,
                20f);
        }

        /// <summary>
        /// Tests that AppendPath handles negative corner radius values.
        /// </summary>
        [Theory]
        [InlineData(-5, -10, -15, -20)]
        [InlineData(-1.5, 2.5, -3.7, 4.8)]
        public void AppendPath_WithNegativeCornerRadius_PassesValuesCorrectly(
            double topLeft, double topRight, double bottomLeft, double bottomRight)
        {
            // Arrange
            var geometry = new RoundRectangleGeometry();
            var mockPath = Substitute.For<PathF>();
            var rect = new Rect(10, 20, 100, 150);
            var cornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);

            geometry.Rect = rect;
            geometry.CornerRadius = cornerRadius;

            // Act
            geometry.AppendPath(mockPath);

            // Assert
            mockPath.Received(1).AppendRoundedRectangle(
                10f,
                20f,
                100f,
                150f,
                (float)topLeft,
                (float)topRight,
                (float)bottomLeft,
                (float)bottomRight);
        }
    }
}