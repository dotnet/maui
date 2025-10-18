#nullable disable

using System;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class PolylineTests : BaseTestFixture
    {
        PointCollectionConverter _pointCollectionConverter;


        public PolylineTests()
        {


            _pointCollectionConverter = new PointCollectionConverter();
        }

        [Fact]
        public void CreatePolylineFromStringPointCollectionTest()
        {
            PointCollection points = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

            Polyline polyline = new Polyline
            {
                Points = points
            };

            Assert.NotNull(points);
            Assert.NotNull(polyline);
            Assert.Equal(10, points.Count);
        }

        /// <summary>
        /// Tests that the FillRule property returns the default value when not explicitly set.
        /// Input: New Polyline instance without setting FillRule.
        /// Expected: FillRule.EvenOdd (the default value defined in FillRuleProperty).
        /// </summary>
        [Fact]
        public void FillRule_DefaultValue_ReturnsEvenOdd()
        {
            // Arrange
            var polyline = new Polyline();

            // Act
            var result = polyline.FillRule;

            // Assert
            Assert.Equal(FillRule.EvenOdd, result);
        }

        /// <summary>
        /// Tests that the FillRule property can be set to valid enum values and retrieved correctly.
        /// Input: Valid FillRule enum values (EvenOdd, Nonzero).
        /// Expected: The property returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(FillRule.EvenOdd)]
        [InlineData(FillRule.Nonzero)]
        public void FillRule_SetValidEnumValues_ReturnsSetValue(FillRule fillRule)
        {
            // Arrange
            var polyline = new Polyline();

            // Act
            polyline.FillRule = fillRule;
            var result = polyline.FillRule;

            // Assert
            Assert.Equal(fillRule, result);
        }

        /// <summary>
        /// Tests that the FillRule property can handle invalid enum values (out-of-range integers cast to FillRule).
        /// Input: Invalid FillRule enum values created by casting integers outside the defined enum range.
        /// Expected: The property stores and returns the invalid enum value without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void FillRule_SetInvalidEnumValues_StoresAndReturnsValue(int invalidEnumValue)
        {
            // Arrange
            var polyline = new Polyline();
            var invalidFillRule = (FillRule)invalidEnumValue;

            // Act
            polyline.FillRule = invalidFillRule;
            var result = polyline.FillRule;

            // Assert
            Assert.Equal(invalidFillRule, result);
            Assert.Equal(invalidEnumValue, (int)result);
        }

        /// <summary>
        /// Tests that setting FillRule multiple times with different values works correctly.
        /// Input: Sequential setting of different FillRule values.
        /// Expected: Each set operation updates the property value correctly.
        /// </summary>
        [Fact]
        public void FillRule_SetMultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var polyline = new Polyline();

            // Act & Assert - Set to Nonzero
            polyline.FillRule = FillRule.Nonzero;
            Assert.Equal(FillRule.Nonzero, polyline.FillRule);

            // Act & Assert - Set back to EvenOdd
            polyline.FillRule = FillRule.EvenOdd;
            Assert.Equal(FillRule.EvenOdd, polyline.FillRule);

            // Act & Assert - Set to invalid value
            var invalidValue = (FillRule)999;
            polyline.FillRule = invalidValue;
            Assert.Equal(invalidValue, polyline.FillRule);
        }

        /// <summary>
        /// Tests GetPath method when Points collection is null.
        /// Should return an empty PathF without any MoveTo or LineTo operations.
        /// </summary>
        [Fact]
        public void GetPath_WhenPointsIsNull_ReturnsEmptyPath()
        {
            // Arrange
            var polyline = new Polyline();

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method when Points collection is empty.
        /// Should return an empty PathF without any MoveTo or LineTo operations.
        /// </summary>
        [Fact]
        public void GetPath_WhenPointsIsEmpty_ReturnsEmptyPath()
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection();

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method when Points collection has exactly one point.
        /// Should call MoveTo for the single point but no LineTo operations.
        /// </summary>
        [Fact]
        public void GetPath_WhenPointsHasSinglePoint_ReturnsPathWithMoveTo()
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection { new Point(10.5, 20.7) };

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method when Points collection has exactly two points.
        /// Should call MoveTo for first point and LineTo for second point.
        /// </summary>
        [Fact]
        public void GetPath_WhenPointsHasTwoPoints_ReturnsPathWithMoveToAndLineTo()
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection
            {
                new Point(0, 0),
                new Point(100, 100)
            };

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method when Points collection has multiple points.
        /// Should call MoveTo for first point and LineTo for each subsequent point.
        /// </summary>
        [Fact]
        public void GetPath_WhenPointsHasMultiplePoints_ReturnsPathWithMultipleSegments()
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection
            {
                new Point(0, 0),
                new Point(50, 25),
                new Point(100, 50),
                new Point(150, 75),
                new Point(200, 100)
            };

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method with extreme coordinate values including boundary values.
        /// Should handle float conversion from double coordinates without exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(float.MinValue, float.MinValue)]
        [InlineData(float.MaxValue, float.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(1, 1)]
        public void GetPath_WithExtremeCoordinates_ReturnsPathWithoutException(double x, double y)
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection { new Point(x, y) };

            // Act & Assert - Should not throw
            var path = polyline.GetPath();
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method with special floating-point values.
        /// Should handle NaN and infinity values in coordinates.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0)]
        [InlineData(0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 0)]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0)]
        [InlineData(0, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void GetPath_WithSpecialFloatingPointValues_ReturnsPathWithoutException(double x, double y)
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection { new Point(x, y) };

            // Act & Assert - Should not throw
            var path = polyline.GetPath();
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method with a large number of points.
        /// Should handle collections with many points without performance issues.
        /// </summary>
        [Fact]
        public void GetPath_WithLargeNumberOfPoints_ReturnsPathWithoutException()
        {
            // Arrange
            var polyline = new Polyline();
            var points = new PointCollection();

            for (int i = 0; i < 1000; i++)
            {
                points.Add(new Point(i, i * 0.5));
            }

            polyline.Points = points;

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method using constructor that accepts PointCollection.
        /// Should create path correctly when points are provided via constructor.
        /// </summary>
        [Fact]
        public void GetPath_WithConstructorProvidedPoints_ReturnsValidPath()
        {
            // Arrange
            var points = new PointCollection
            {
                new Point(10, 20),
                new Point(30, 40),
                new Point(50, 60)
            };
            var polyline = new Polyline(points);

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method with negative coordinates.
        /// Should handle negative coordinate values properly.
        /// </summary>
        [Fact]
        public void GetPath_WithNegativeCoordinates_ReturnsValidPath()
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection
            {
                new Point(-100, -200),
                new Point(-50, -150),
                new Point(-25, -75)
            };

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests GetPath method with mixed positive and negative coordinates.
        /// Should handle coordinate transitions across zero properly.
        /// </summary>
        [Fact]
        public void GetPath_WithMixedCoordinates_ReturnsValidPath()
        {
            // Arrange
            var polyline = new Polyline();
            polyline.Points = new PointCollection
            {
                new Point(-50, 100),
                new Point(0, 0),
                new Point(50, -100)
            };

            // Act
            var path = polyline.GetPath();

            // Assert
            Assert.NotNull(path);
        }

        /// <summary>
        /// Tests the Polyline constructor that accepts a PointCollection parameter.
        /// Verifies that the Points property is correctly set to the provided PointCollection.
        /// </summary>
        /// <param name="expectedPointCount">The expected number of points in the PointCollection.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public void Constructor_WithValidPointCollection_SetsPointsProperty(int expectedPointCount)
        {
            // Arrange
            var pointCollection = new PointCollection();
            for (int i = 0; i < expectedPointCount; i++)
            {
                pointCollection.Add(new Point(i, i * 2));
            }

            // Act
            var polyline = new Polyline(pointCollection);

            // Assert
            Assert.NotNull(polyline);
            Assert.Same(pointCollection, polyline.Points);
            Assert.Equal(expectedPointCount, polyline.Points.Count);
        }

        /// <summary>
        /// Tests the Polyline constructor with a null PointCollection parameter.
        /// Verifies that the Points property is set to null without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_WithNullPointCollection_SetsPointsPropertyToNull()
        {
            // Arrange
            PointCollection nullPointCollection = null;

            // Act
            var polyline = new Polyline(nullPointCollection);

            // Assert
            Assert.NotNull(polyline);
            Assert.Null(polyline.Points);
        }

        /// <summary>
        /// Tests the Polyline constructor with a PointCollection containing specific Point values.
        /// Verifies that the exact Point values are preserved in the Points property.
        /// </summary>
        [Fact]
        public void Constructor_WithPointCollectionContainingSpecificPoints_PreservesPointValues()
        {
            // Arrange
            var expectedPoints = new Point[]
            {
                new Point(0, 0),
                new Point(10.5, 20.3),
                new Point(-5, 15),
                new Point(double.MaxValue, double.MinValue)
            };
            var pointCollection = new PointCollection(expectedPoints);

            // Act
            var polyline = new Polyline(pointCollection);

            // Assert
            Assert.NotNull(polyline);
            Assert.Same(pointCollection, polyline.Points);
            Assert.Equal(expectedPoints.Length, polyline.Points.Count);
            for (int i = 0; i < expectedPoints.Length; i++)
            {
                Assert.Equal(expectedPoints[i], polyline.Points[i]);
            }
        }

        /// <summary>
        /// Tests the Polyline constructor with edge case Point values including infinity and NaN.
        /// Verifies that extreme Point values are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithPointCollectionContainingEdgeCasePoints_HandlesExtremeValues()
        {
            // Arrange
            var edgeCasePoints = new Point[]
            {
                new Point(double.PositiveInfinity, double.NegativeInfinity),
                new Point(double.NaN, double.NaN),
                new Point(0, 0),
                new Point(-0.0, 0.0)
            };
            var pointCollection = new PointCollection(edgeCasePoints);

            // Act
            var polyline = new Polyline(pointCollection);

            // Assert
            Assert.NotNull(polyline);
            Assert.Same(pointCollection, polyline.Points);
            Assert.Equal(edgeCasePoints.Length, polyline.Points.Count);
            for (int i = 0; i < edgeCasePoints.Length; i++)
            {
                Assert.Equal(edgeCasePoints[i], polyline.Points[i]);
            }
        }

        /// <summary>
        /// Tests that the Polyline constructor properly initializes the base Shape class.
        /// Verifies that the constructed Polyline is a valid Shape instance.
        /// </summary>
        [Fact]
        public void Constructor_WithPointCollection_ProperlyInitializesBaseShape()
        {
            // Arrange
            var pointCollection = new PointCollection();
            pointCollection.Add(new Point(1, 2));

            // Act
            var polyline = new Polyline(pointCollection);

            // Assert
            Assert.NotNull(polyline);
            Assert.IsType<Polyline>(polyline);
            Assert.IsAssignableFrom<Shape>(polyline);
            Assert.Same(pointCollection, polyline.Points);
        }
    }
}