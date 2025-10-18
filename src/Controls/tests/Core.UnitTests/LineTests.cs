#nullable disable

using System;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class LineTests : BaseTestFixture
    {
        [Fact]
        public void XPointCanBeSetFromStyle()
        {
            var line = new Line();

            Assert.Equal(0.0, line.X1);
            line.SetValue(Line.X1Property, 1.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
            Assert.Equal(1.0, line.X1);

            Assert.Equal(0.0, line.X2);
            line.SetValue(Line.X2Property, 100.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
            Assert.Equal(100.0, line.X2);
        }

        [Fact]
        public void YPointCanBeSetFromStyle()
        {
            var line = new Line();

            Assert.Equal(0.0, line.Y1);
            line.SetValue(Line.Y1Property, 1.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
            Assert.Equal(1.0, line.Y1);

            Assert.Equal(0.0, line.Y2);
            line.SetValue(Line.Y2Property, 10.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
            Assert.Equal(10.0, line.Y2);
        }

        /// <summary>
        /// Tests that GetPath returns a non-null PathF object with correct path operations and coordinates.
        /// Input: Line with standard coordinate values.
        /// Expected: PathF with MoveTo and LineTo operations at specified coordinates.
        /// </summary>
        [Fact]
        public void GetPath_WithNormalCoordinates_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = 10.5,
                Y1 = 20.5,
                X2 = 30.5,
                Y2 = 40.5
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var operations = path.SegmentTypes.ToArray();
            Assert.Equal(2, operations.Length);
            Assert.Equal(PathOperation.Move, operations[0]);
            Assert.Equal(PathOperation.Line, operations[1]);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.Equal(new PointF(10.5f, 20.5f), points[0]);
            Assert.Equal(new PointF(30.5f, 40.5f), points[1]);

            Assert.Equal(new PointF(10.5f, 20.5f), path.FirstPoint);
            Assert.Equal(new PointF(30.5f, 40.5f), path.LastPoint);
        }

        /// <summary>
        /// Tests that GetPath handles zero coordinates correctly.
        /// Input: Line with all coordinates set to zero.
        /// Expected: PathF with MoveTo and LineTo operations at origin.
        /// </summary>
        [Fact]
        public void GetPath_WithZeroCoordinates_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = 0.0,
                Y1 = 0.0,
                X2 = 0.0,
                Y2 = 0.0
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var operations = path.SegmentTypes.ToArray();
            Assert.Equal(2, operations.Length);
            Assert.Equal(PathOperation.Move, operations[0]);
            Assert.Equal(PathOperation.Line, operations[1]);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.Equal(new PointF(0.0f, 0.0f), points[0]);
            Assert.Equal(new PointF(0.0f, 0.0f), points[1]);
        }

        /// <summary>
        /// Tests that GetPath handles negative coordinates correctly.
        /// Input: Line with negative coordinate values.
        /// Expected: PathF with MoveTo and LineTo operations at specified negative coordinates.
        /// </summary>
        [Fact]
        public void GetPath_WithNegativeCoordinates_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = -50.0,
                Y1 = -25.0,
                X2 = -10.0,
                Y2 = -5.0
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.Equal(new PointF(-50.0f, -25.0f), points[0]);
            Assert.Equal(new PointF(-10.0f, -5.0f), points[1]);
        }

        /// <summary>
        /// Tests that GetPath handles extreme double values correctly when cast to float.
        /// Input: Line with Double.MinValue and Double.MaxValue coordinates.
        /// Expected: PathF with coordinates cast to float equivalents.
        /// </summary>
        [Fact]
        public void GetPath_WithExtremeValues_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = double.MinValue,
                Y1 = double.MaxValue,
                X2 = double.MinValue,
                Y2 = double.MaxValue
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.Equal(new PointF((float)double.MinValue, (float)double.MaxValue), points[0]);
            Assert.Equal(new PointF((float)double.MinValue, (float)double.MaxValue), points[1]);
        }

        /// <summary>
        /// Tests that GetPath handles NaN values correctly when cast to float.
        /// Input: Line with Double.NaN coordinates.
        /// Expected: PathF with NaN coordinates cast to float.
        /// </summary>
        [Fact]
        public void GetPath_WithNaNValues_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = double.NaN,
                Y1 = double.NaN,
                X2 = double.NaN,
                Y2 = double.NaN
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.True(float.IsNaN(points[0].X));
            Assert.True(float.IsNaN(points[0].Y));
            Assert.True(float.IsNaN(points[1].X));
            Assert.True(float.IsNaN(points[1].Y));
        }

        /// <summary>
        /// Tests that GetPath handles infinity values correctly when cast to float.
        /// Input: Line with positive and negative infinity coordinates.
        /// Expected: PathF with infinity coordinates cast to float.
        /// </summary>
        [Fact]
        public void GetPath_WithInfinityValues_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = double.PositiveInfinity,
                Y1 = double.NegativeInfinity,
                X2 = double.NegativeInfinity,
                Y2 = double.PositiveInfinity
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.True(float.IsPositiveInfinity(points[0].X));
            Assert.True(float.IsNegativeInfinity(points[0].Y));
            Assert.True(float.IsNegativeInfinity(points[1].X));
            Assert.True(float.IsPositiveInfinity(points[1].Y));
        }

        /// <summary>
        /// Tests that GetPath handles precision loss when casting from double to float.
        /// Input: Line with high-precision double values that cannot be represented exactly as float.
        /// Expected: PathF with coordinates cast to their float representation.
        /// </summary>
        [Fact]
        public void GetPath_WithHighPrecisionValues_ReturnsCorrectPath()
        {
            // Arrange
            var line = new Line
            {
                X1 = 1.23456789012345,
                Y1 = 9.87654321098765,
                X2 = 123456789012345.0,
                Y2 = 0.123456789012345
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.Equal(new PointF((float)1.23456789012345, (float)9.87654321098765), points[0]);
            Assert.Equal(new PointF((float)123456789012345.0, (float)0.123456789012345), points[1]);
        }

        /// <summary>
        /// Tests that GetPath returns correct path structure regardless of coordinate values.
        /// Input: Various coordinate combinations.
        /// Expected: PathF always contains exactly one MoveTo followed by one LineTo operation.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 100.0, 100.0)]
        [InlineData(-50.0, -50.0, 50.0, 50.0)]
        [InlineData(1.0, 2.0, 3.0, 4.0)]
        [InlineData(100.5, 200.5, 300.5, 400.5)]
        public void GetPath_WithVariousCoordinates_ReturnsCorrectPathStructure(double x1, double y1, double x2, double y2)
        {
            // Arrange
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            // Act
            var path = line.GetPath();

            // Assert
            Assert.NotNull(path);

            var operations = path.SegmentTypes.ToArray();
            Assert.Equal(2, operations.Length);
            Assert.Equal(PathOperation.Move, operations[0]);
            Assert.Equal(PathOperation.Line, operations[1]);

            var points = path.Points.ToArray();
            Assert.Equal(2, points.Length);
            Assert.Equal(new PointF((float)x1, (float)y1), points[0]);
            Assert.Equal(new PointF((float)x2, (float)y2), points[1]);
        }

        /// <summary>
        /// Tests the Line constructor with four double parameters to ensure all coordinate properties are correctly set.
        /// Verifies that X1, Y1, X2, Y2 properties are assigned the exact values passed to the constructor.
        /// </summary>
        /// <param name="x1">X coordinate of the line start point</param>
        /// <param name="y1">Y coordinate of the line start point</param>
        /// <param name="x2">X coordinate of the line end point</param>
        /// <param name="y2">Y coordinate of the line end point</param>
        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(1.0, 2.0, 3.0, 4.0)]
        [InlineData(-1.0, -2.0, -3.0, -4.0)]
        [InlineData(100.5, 200.5, 300.5, 400.5)]
        [InlineData(double.MinValue, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(-0.0, 0.0, 1.5, -1.5)]
        public void Constructor_WithCoordinateParameters_SetsAllPropertiesCorrectly(double x1, double y1, double x2, double y2)
        {
            // Arrange & Act
            var line = new Line(x1, y1, x2, y2);

            // Assert
            Assert.Equal(x1, line.X1);
            Assert.Equal(y1, line.Y1);
            Assert.Equal(x2, line.X2);
            Assert.Equal(y2, line.Y2);
        }

        /// <summary>
        /// Tests the Line constructor with extreme double values to ensure proper handling of boundary conditions.
        /// Verifies that the constructor can handle the full range of double values including special cases.
        /// </summary>
        [Fact]
        public void Constructor_WithExtremeValues_HandlesAllDoubleRanges()
        {
            // Arrange & Act
            var line = new Line(double.MaxValue, double.MinValue, double.PositiveInfinity, double.NegativeInfinity);

            // Assert
            Assert.Equal(double.MaxValue, line.X1);
            Assert.Equal(double.MinValue, line.Y1);
            Assert.Equal(double.PositiveInfinity, line.X2);
            Assert.Equal(double.NegativeInfinity, line.Y2);
        }

        /// <summary>
        /// Tests the Line constructor with NaN values to ensure proper handling of invalid numeric values.
        /// Verifies that NaN values are correctly stored and retrieved from properties.
        /// </summary>
        [Fact]
        public void Constructor_WithNaNValues_PreservesNaNInProperties()
        {
            // Arrange & Act
            var line = new Line(double.NaN, double.NaN, double.NaN, double.NaN);

            // Assert
            Assert.True(double.IsNaN(line.X1));
            Assert.True(double.IsNaN(line.Y1));
            Assert.True(double.IsNaN(line.X2));
            Assert.True(double.IsNaN(line.Y2));
        }

        /// <summary>
        /// Tests the Line constructor with mixed positive and negative zero values.
        /// Verifies that both positive and negative zero are handled correctly by the properties.
        /// </summary>
        [Fact]
        public void Constructor_WithPositiveAndNegativeZero_PreservesZeroValues()
        {
            // Arrange & Act
            var line = new Line(0.0, -0.0, 0.0, -0.0);

            // Assert
            Assert.Equal(0.0, line.X1);
            Assert.Equal(-0.0, line.Y1);
            Assert.Equal(0.0, line.X2);
            Assert.Equal(-0.0, line.Y2);
        }
    }
}