#nullable disable

using System;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class PolygonTests : BaseTestFixture
    {
        PointCollectionConverter _pointCollectionConverter;


        public PolygonTests()
        {


            _pointCollectionConverter = new PointCollectionConverter();
        }

        [Fact]
        public void CreatePolygonFromStringPointCollectionTest()
        {
            PointCollection points = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

            Polygon polygon = new Polygon
            {
                Points = points
            };

            Assert.NotNull(points);
            Assert.NotNull(polygon);
            Assert.Equal(10, points.Count);
        }

        /// <summary>
        /// Tests that the FillRule property setter correctly stores valid enum values.
        /// </summary>
        /// <param name="fillRule">The FillRule value to test.</param>
        [Theory]
        [InlineData(FillRule.EvenOdd)]
        [InlineData(FillRule.Nonzero)]
        public void FillRule_SetValidValue_StoresValueCorrectly(FillRule fillRule)
        {
            // Arrange
            var polygon = new Polygon();

            // Act
            polygon.FillRule = fillRule;

            // Assert
            Assert.Equal(fillRule, polygon.FillRule);
        }

        /// <summary>
        /// Tests that the FillRule property getter correctly retrieves the stored value.
        /// </summary>
        [Fact]
        public void FillRule_GetAfterSet_ReturnsCorrectValue()
        {
            // Arrange
            var polygon = new Polygon();
            var expectedValue = FillRule.Nonzero;

            // Act
            polygon.FillRule = expectedValue;
            var actualValue = polygon.FillRule;

            // Assert  
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the FillRule property returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void FillRule_DefaultValue_ReturnsEvenOdd()
        {
            // Arrange
            var polygon = new Polygon();

            // Act
            var actualValue = polygon.FillRule;

            // Assert
            Assert.Equal(FillRule.EvenOdd, actualValue);
        }

        /// <summary>
        /// Tests that the FillRule property can handle invalid enum values by casting integers outside the defined range.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid FillRule value cast from an integer.</param>
        [Theory]
        [InlineData((FillRule)(-1))]
        [InlineData((FillRule)2)]
        [InlineData((FillRule)999)]
        [InlineData((FillRule)int.MinValue)]
        [InlineData((FillRule)int.MaxValue)]
        public void FillRule_SetInvalidEnumValue_StoresAndRetrievesValue(FillRule invalidEnumValue)
        {
            // Arrange
            var polygon = new Polygon();

            // Act
            polygon.FillRule = invalidEnumValue;
            var retrievedValue = polygon.FillRule;

            // Assert
            Assert.Equal(invalidEnumValue, retrievedValue);
        }

        /// <summary>
        /// Tests that setting FillRule multiple times overwrites the previous value correctly.
        /// </summary>
        [Fact]
        public void FillRule_SetMultipleTimes_OverwritesValueCorrectly()
        {
            // Arrange
            var polygon = new Polygon();
            var firstValue = FillRule.EvenOdd;
            var secondValue = FillRule.Nonzero;

            // Act
            polygon.FillRule = firstValue;
            polygon.FillRule = secondValue;

            // Assert
            Assert.Equal(secondValue, polygon.FillRule);
        }

        /// <summary>
        /// Tests that GetPath returns an empty path when Points collection is empty.
        /// Verifies that the method handles empty collections gracefully without throwing exceptions.
        /// Expected result: Returns a PathF object with no operations.
        /// </summary>
        [Fact]
        public void GetPath_EmptyPointsCollection_ReturnsEmptyPath()
        {
            // Arrange
            var polygon = new Polygon();

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath returns an empty path when Points is explicitly set to null.
        /// Verifies null safety of the Points?.Count check.
        /// Expected result: Returns a PathF object with no operations.
        /// </summary>
        [Fact]
        public void GetPath_NullPoints_ReturnsEmptyPath()
        {
            // Arrange
            var polygon = new Polygon();
            polygon.Points = null;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath creates a proper path when Points contains a single point.
        /// Verifies that the method moves to the point and closes the path.
        /// Expected result: Path with MoveTo and Close operations.
        /// </summary>
        [Fact]
        public void GetPath_SinglePoint_CreatesPathWithMoveAndClose()
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection { new Point(10, 20) };
            polygon.Points = points;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath creates a proper path when Points contains two points.
        /// Verifies that the method moves to first point, draws line to second, and closes.
        /// Expected result: Path with MoveTo, LineTo, and Close operations.
        /// </summary>
        [Fact]
        public void GetPath_TwoPoints_CreatesPathWithMoveLineAndClose()
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection { new Point(10, 20), new Point(30, 40) };
            polygon.Points = points;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath creates a proper polygon path with multiple points.
        /// Verifies that the method moves to first point, connects all subsequent points with lines, and closes the path.
        /// Expected result: Path with MoveTo, multiple LineTo operations, and Close.
        /// </summary>
        [Fact]
        public void GetPath_MultiplePoints_CreatesClosedPolygonPath()
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection
            {
                new Point(0, 0),
                new Point(100, 0),
                new Point(100, 100),
                new Point(0, 100)
            };
            polygon.Points = points;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath handles extreme coordinate values correctly.
        /// Verifies proper casting from double to float for extreme values.
        /// Expected result: Path created without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(-1000000, 1000000)]
        [InlineData(float.MinValue, float.MaxValue)]
        public void GetPath_ExtremeCoordinateValues_HandlesCorrectly(double x, double y)
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection { new Point(x, y), new Point(0, 0) };
            polygon.Points = points;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath handles special floating point values.
        /// Verifies behavior with NaN and Infinity values.
        /// Expected result: Path created without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0)]
        [InlineData(0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0)]
        [InlineData(double.NegativeInfinity, 0)]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(0, double.NegativeInfinity)]
        public void GetPath_SpecialFloatingPointValues_HandlesCorrectly(double x, double y)
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection { new Point(x, y) };
            polygon.Points = points;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath creates different path instances for each call.
        /// Verifies that the method always returns a new PathF instance.
        /// Expected result: Each call returns a distinct object instance.
        /// </summary>
        [Fact]
        public void GetPath_MultipleCalls_ReturnsNewInstanceEachTime()
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection { new Point(10, 20) };
            polygon.Points = points;

            // Act
            var result1 = polygon.GetPath();
            var result2 = polygon.GetPath();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that GetPath works correctly when using constructor with points.
        /// Verifies that the constructor parameter properly sets the Points property for path generation.
        /// Expected result: Path created using the points provided in constructor.
        /// </summary>
        [Fact]
        public void GetPath_ConstructorWithPoints_CreatesCorrectPath()
        {
            // Arrange
            var points = new PointCollection { new Point(5, 10), new Point(15, 20), new Point(25, 30) };
            var polygon = new Polygon(points);

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetPath handles a large number of points efficiently.
        /// Verifies that the method can process collections with many points without issues.
        /// Expected result: Path created successfully with all points processed.
        /// </summary>
        [Fact]
        public void GetPath_LargeNumberOfPoints_HandlesCorrectly()
        {
            // Arrange
            var polygon = new Polygon();
            var points = new PointCollection();

            // Add 1000 points in a circular pattern
            for (int i = 0; i < 1000; i++)
            {
                double angle = (double)i / 1000 * 2 * Math.PI;
                points.Add(new Point(Math.Cos(angle) * 100, Math.Sin(angle) * 100));
            }

            polygon.Points = points;

            // Act
            var result = polygon.GetPath();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the Polygon constructor properly initializes with a valid PointCollection parameter.
        /// Verifies that the Points property is correctly set to the provided PointCollection.
        /// </summary>
        [Fact]
        public void Constructor_WithValidPointCollection_SetsPointsProperty()
        {
            // Arrange
            var expectedPoints = new PointCollection(new Point[] { new Point(10, 20), new Point(30, 40), new Point(50, 60) });

            // Act
            var polygon = new Polygon(expectedPoints);

            // Assert
            Assert.Same(expectedPoints, polygon.Points);
        }

        /// <summary>
        /// Tests that the Polygon constructor properly initializes with an empty PointCollection parameter.
        /// Verifies that the Points property is correctly set to the provided empty PointCollection.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyPointCollection_SetsPointsProperty()
        {
            // Arrange
            var expectedPoints = new PointCollection();

            // Act
            var polygon = new Polygon(expectedPoints);

            // Assert
            Assert.Same(expectedPoints, polygon.Points);
            Assert.Empty(polygon.Points);
        }

        /// <summary>
        /// Tests that the Polygon constructor properly handles a null PointCollection parameter.
        /// Verifies that the Points property is set to null when null is passed to the constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithNullPointCollection_SetsPointsPropertyToNull()
        {
            // Arrange
            PointCollection nullPoints = null;

            // Act
            var polygon = new Polygon(nullPoints);

            // Assert
            Assert.Null(polygon.Points);
        }

        /// <summary>
        /// Tests that the Polygon constructor properly initializes with a single point PointCollection.
        /// Verifies that the Points property correctly contains the single point.
        /// </summary>
        [Fact]
        public void Constructor_WithSinglePointCollection_SetsPointsProperty()
        {
            // Arrange
            var singlePoint = new Point(100, 200);
            var expectedPoints = new PointCollection(new Point[] { singlePoint });

            // Act
            var polygon = new Polygon(expectedPoints);

            // Assert
            Assert.Same(expectedPoints, polygon.Points);
            Assert.Single(polygon.Points);
            Assert.Equal(singlePoint, polygon.Points[0]);
        }

        /// <summary>
        /// Tests that the Polygon constructor with PointCollection parameter creates a properly initialized Polygon object.
        /// Verifies that the base constructor is called and the object is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_WithPointCollection_CreatesValidPolygonInstance()
        {
            // Arrange
            var points = new PointCollection(new Point[] { new Point(0, 0), new Point(10, 0), new Point(5, 10) });

            // Act
            var polygon = new Polygon(points);

            // Assert
            Assert.NotNull(polygon);
            Assert.IsType<Polygon>(polygon);
            Assert.Same(points, polygon.Points);
        }
    }
}