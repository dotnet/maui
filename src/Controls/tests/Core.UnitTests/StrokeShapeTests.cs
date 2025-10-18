using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class StrokeShapeTests : BaseTestFixture
    {
        StrokeShapeTypeConverter _strokeShapeTypeConverter;

        public StrokeShapeTests()
        {
            _strokeShapeTypeConverter = new StrokeShapeTypeConverter();
        }

        [Theory]
        [InlineData("rectangle")]
        [InlineData("Rectangle")]
        public void TestRectangleConstructor(string value)
        {
            Rectangle rectangle = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Rectangle;

            Assert.NotNull(rectangle);
        }

        [Theory]
        [InlineData("roundRectangle")]
        [InlineData("RoundRectangle")]
        [InlineData("roundRectangle 12")]
        [InlineData("roundRectangle 12, 6, 24, 36")]
        [InlineData("roundRectangle 12, 12, 24, 12")]
        [InlineData("RoundRectangle 12")]
        [InlineData("RoundRectangle 12, 6, 24, 36")]
        [InlineData("RoundRectangle 12, 12, 24, 12")]
        public void TestRoundRectangleConstructor(string value)
        {
            RoundRectangle roundRectangle = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as RoundRectangle;

            Assert.NotNull(roundRectangle);

            if (!string.Equals("roundRectangle", value, StringComparison.OrdinalIgnoreCase))
            {
                Assert.NotEqual(0, roundRectangle.CornerRadius.TopLeft);
                Assert.NotEqual(0, roundRectangle.CornerRadius.TopRight);
                Assert.NotEqual(0, roundRectangle.CornerRadius.BottomLeft);
                Assert.NotEqual(0, roundRectangle.CornerRadius.BottomRight);
            }
        }

        [Theory]
        [InlineData("path")]
        [InlineData("Path")]
        [InlineData("path M8.4580019,25.5C8.4580019,26.747002 10.050002,27.758995 12.013003,27.758995 13.977001,27.758995 15.569004,26.747002 15.569004,25.5z M19.000005,10C16.861005,9.9469986 14.527004,12.903999 14.822002,22.133995 14.822002,22.133995 26.036002,15.072998 20.689,10.681999 20.183003,10.265999 19.599004,10.014999 19.000005,10z M4.2539991,10C3.6549998,10.014999 3.0710002,10.265999 2.5649996,10.681999 -2.7820019,15.072998 8.4320009,22.133995 8.4320009,22.133995 8.7270001,12.903999 6.3929995,9.9469986 4.2539991,10z M11.643,0C18.073003,0 23.286002,5.8619995 23.286002,13.091995 23.286002,20.321999 18.684003,32 12.254,32 5.8239992,32 1.8224728E-07,20.321999 0,13.091995 1.8224728E-07,5.8619995 5.2129987,0 11.643,0z")]
        [InlineData("path M16.484421,0.73799322C20.831404,0.7379931 24.353395,1.1259904 24.353395,1.6049905 24.353395,2.0839829 20.831404,2.4719803 16.484421,2.47198 12.138443,2.4719803 8.6154527,2.0839829 8.6154527,1.6049905 8.6154527,1.1259904 12.138443,0.7379931 16.484421,0.73799322z M1.9454784,0.061995983C2.7564723,5.2449602 12.246436,11.341911 12.246436,11.341911 13.248431,19.240842 9.6454477,17.915854 9.6454477,17.915854 7.9604563,18.897849 6.5314603,17.171859 6.5314603,17.171859 4.1084647,18.29585 3.279473,15.359877 3.2794733,15.359877 0.82348057,15.291876 1.2804796,11.362907 1.2804799,11.362907 -1.573514,10.239915 1.2344746,6.3909473 1.2344746,6.3909473 -1.3255138,4.9869594 1.9454782,0.061996057 1.9454784,0.061995983z M30.054371,0C30.054371,9.8700468E-08 33.325355,4.9249634 30.765367,6.3289513 30.765367,6.3289513 33.574364,10.177919 30.71837,11.30191 30.71837,11.30191 31.175369,15.22988 28.721384,15.297872 28.721384,15.297872 27.892376,18.232854 25.468389,17.110862 25.468389,17.110862 24.040392,18.835847 22.355402,17.853852 22.355402,17.853852 18.752417,19.178845 19.753414,11.279907 19.753414,11.279907 29.243385,5.1829566 30.054371,0z")]
        [InlineData("Path M8.4580019,25.5C8.4580019,26.747002 10.050002,27.758995 12.013003,27.758995 13.977001,27.758995 15.569004,26.747002 15.569004,25.5z M19.000005,10C16.861005,9.9469986 14.527004,12.903999 14.822002,22.133995 14.822002,22.133995 26.036002,15.072998 20.689,10.681999 20.183003,10.265999 19.599004,10.014999 19.000005,10z M4.2539991,10C3.6549998,10.014999 3.0710002,10.265999 2.5649996,10.681999 -2.7820019,15.072998 8.4320009,22.133995 8.4320009,22.133995 8.7270001,12.903999 6.3929995,9.9469986 4.2539991,10z M11.643,0C18.073003,0 23.286002,5.8619995 23.286002,13.091995 23.286002,20.321999 18.684003,32 12.254,32 5.8239992,32 1.8224728E-07,20.321999 0,13.091995 1.8224728E-07,5.8619995 5.2129987,0 11.643,0z")]
        [InlineData("Path M16.484421,0.73799322C20.831404,0.7379931 24.353395,1.1259904 24.353395,1.6049905 24.353395,2.0839829 20.831404,2.4719803 16.484421,2.47198 12.138443,2.4719803 8.6154527,2.0839829 8.6154527,1.6049905 8.6154527,1.1259904 12.138443,0.7379931 16.484421,0.73799322z M1.9454784,0.061995983C2.7564723,5.2449602 12.246436,11.341911 12.246436,11.341911 13.248431,19.240842 9.6454477,17.915854 9.6454477,17.915854 7.9604563,18.897849 6.5314603,17.171859 6.5314603,17.171859 4.1084647,18.29585 3.279473,15.359877 3.2794733,15.359877 0.82348057,15.291876 1.2804796,11.362907 1.2804799,11.362907 -1.573514,10.239915 1.2344746,6.3909473 1.2344746,6.3909473 -1.3255138,4.9869594 1.9454782,0.061996057 1.9454784,0.061995983z M30.054371,0C30.054371,9.8700468E-08 33.325355,4.9249634 30.765367,6.3289513 30.765367,6.3289513 33.574364,10.177919 30.71837,11.30191 30.71837,11.30191 31.175369,15.22988 28.721384,15.297872 28.721384,15.297872 27.892376,18.232854 25.468389,17.110862 25.468389,17.110862 24.040392,18.835847 22.355402,17.853852 22.355402,17.853852 18.752417,19.178845 19.753414,11.279907 19.753414,11.279907 29.243385,5.1829566 30.054371,0z")]
        public void TestPathConstructor(string value)
        {
            Path path = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Path;

            Assert.NotNull(path);
            if (!string.Equals("path", value, StringComparison.OrdinalIgnoreCase))
            {
                Assert.NotNull(path.Data);
            }
        }

        [Theory]
        [InlineData("polygon")]
        [InlineData("Polygon")]
        [InlineData("polygon 10,110 60,10 110,110")]
        [InlineData("polygon 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
        [InlineData("Polygon 10,110 60,10 110,110")]
        [InlineData("Polygon 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
        public void TestPolygonConstructor(string value)
        {
            Polygon polygon = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Polygon;

            Assert.NotNull(polygon);
            if (!string.Equals("polygon", value, StringComparison.OrdinalIgnoreCase))
            {
                Assert.NotEmpty(polygon.Points);
            }
        }

        [Theory]
        [InlineData("line")]
        [InlineData("Line")]
        [InlineData("line 1 2")]
        [InlineData("Line 1 2 3 4")]
        public void TestLineConstructor(string value)
        {
            Line line = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Line;

            Assert.NotNull(line);
            if (!string.Equals("line", value, StringComparison.OrdinalIgnoreCase))
            {
                Assert.True(line.X1 != 0);
                Assert.True(line.Y1 != 0);
            }
        }

        [Theory]
        [InlineData("polyline")]
        [InlineData("Polyline")]
        [InlineData("polyline 10,110 60,10 110,110")]
        [InlineData("polyline 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
        [InlineData("Polyline 10,110 60,10 110,110")]
        [InlineData("Polyline 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
        public void TestPolylineConstructor(string value)
        {
            Polyline polyline = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Polyline;

            Assert.NotNull(polyline);
            if (!string.Equals("polyline", value, StringComparison.OrdinalIgnoreCase))
            {
                Assert.NotEmpty(polyline.Points);
            }
        }

        [Theory]
        [InlineData("ellipse")]
        [InlineData("Ellipse")]
        public void TestEllipseConstructor(string value)
        {
            Ellipse ellipse = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Ellipse;

            Assert.NotNull(ellipse);
        }

        [Theory]
        [InlineData("20")]
        public void TestRoundRectangleSingleValue(string value)
        {
            RoundRectangle roundRectangle = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as RoundRectangle;

            Assert.NotNull(roundRectangle);
            Assert.NotEqual(0, roundRectangle.CornerRadius.TopLeft);
            Assert.NotEqual(0, roundRectangle.CornerRadius.TopRight);
            Assert.NotEqual(0, roundRectangle.CornerRadius.BottomLeft);
            Assert.NotEqual(0, roundRectangle.CornerRadius.BottomRight);
        }
    }

    /// <summary>
    /// Unit tests for the StrokeDashArray property of the Shape class.
    /// </summary>
    public partial class ShapeTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the StrokeDashArray property setter works correctly with null values.
        /// The null value should be implicitly converted to an empty DoubleCollection.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetNull_ReturnsEmptyCollection()
        {
            // Arrange
            var shape = new Rectangle();

            // Act
            shape.StrokeDashArray = null;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Empty(shape.StrokeDashArray);
        }

        /// <summary>
        /// Tests that the StrokeDashArray property setter and getter work correctly with various DoubleCollection values.
        /// Verifies that the setter (uncovered line 112) is properly exercised and values are stored correctly.
        /// </summary>
        [Theory]
        [InlineData(new double[] { })]
        [InlineData(new double[] { 1.0 })]
        [InlineData(new double[] { 0.0 })]
        [InlineData(new double[] { -1.0 })]
        [InlineData(new double[] { 1.0, 2.0, 3.0 })]
        [InlineData(new double[] { 5.5, 10.0, 2.5, 8.0 })]
        public void StrokeDashArray_SetValidDoubleArray_ReturnsExpectedCollection(double[] values)
        {
            // Arrange
            var shape = new Rectangle();
            var expected = new DoubleCollection(values);

            // Act
            shape.StrokeDashArray = expected;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Equal(expected.Count, shape.StrokeDashArray.Count);
            Assert.True(expected.SequenceEqual(shape.StrokeDashArray));
        }

        /// <summary>
        /// Tests that the StrokeDashArray property works with boundary double values including infinity and NaN.
        /// This tests edge cases that could cause issues with the setter implementation.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void StrokeDashArray_SetBoundaryValues_ReturnsExpectedValue(double value)
        {
            // Arrange
            var shape = new Rectangle();
            var collection = new DoubleCollection(new[] { value });

            // Act
            shape.StrokeDashArray = collection;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Single(shape.StrokeDashArray);
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(shape.StrokeDashArray[0]));
            }
            else
            {
                Assert.Equal(value, shape.StrokeDashArray[0]);
            }
        }

        /// <summary>
        /// Tests that the StrokeDashArray property works with implicit conversion from double arrays.
        /// This verifies the setter handles the implicit operator correctly.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetDoubleArray_ImplicitConversionWorks()
        {
            // Arrange
            var shape = new Rectangle();
            double[] values = { 1.0, 2.0, 3.0, 4.0 };

            // Act
            shape.StrokeDashArray = values;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Equal(4, shape.StrokeDashArray.Count);
            Assert.True(values.SequenceEqual(shape.StrokeDashArray));
        }

        /// <summary>
        /// Tests that the StrokeDashArray property works with implicit conversion from float arrays.
        /// This verifies the setter handles the implicit operator from float[] correctly.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetFloatArray_ImplicitConversionWorks()
        {
            // Arrange
            var shape = new Rectangle();
            float[] values = { 1.5f, 2.5f, 3.5f };
            var expectedDoubles = values.Select(f => (double)f).ToArray();

            // Act
            shape.StrokeDashArray = values;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Equal(3, shape.StrokeDashArray.Count);
            Assert.True(expectedDoubles.SequenceEqual(shape.StrokeDashArray));
        }

        /// <summary>
        /// Tests that the StrokeDashArray property returns the default value when first accessed.
        /// Based on the bindable property definition, it should create a new DoubleCollection by default.
        /// </summary>
        [Fact]
        public void StrokeDashArray_DefaultValue_ReturnsEmptyCollection()
        {
            // Arrange
            var shape = new Rectangle();

            // Act
            var result = shape.StrokeDashArray;

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that setting multiple different DoubleCollections works correctly.
        /// This ensures the setter properly replaces previous values.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var shape = new Rectangle();
            var firstCollection = new DoubleCollection(new[] { 1.0, 2.0 });
            var secondCollection = new DoubleCollection(new[] { 3.0, 4.0, 5.0 });

            // Act
            shape.StrokeDashArray = firstCollection;
            shape.StrokeDashArray = secondCollection;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Equal(3, shape.StrokeDashArray.Count);
            Assert.True(secondCollection.SequenceEqual(shape.StrokeDashArray));
        }

        /// <summary>
        /// Tests that the StrokeDashArray property works with a large collection.
        /// This tests potential performance or capacity edge cases in the setter.
        /// </summary>
        [Fact]
        public void StrokeDashArray_SetLargeCollection_WorksCorrectly()
        {
            // Arrange
            var shape = new Rectangle();
            var largeArray = Enumerable.Range(0, 1000).Select(i => (double)i).ToArray();
            var largeCollection = new DoubleCollection(largeArray);

            // Act
            shape.StrokeDashArray = largeCollection;

            // Assert
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Equal(1000, shape.StrokeDashArray.Count);
            Assert.True(largeArray.SequenceEqual(shape.StrokeDashArray));
        }

        /// <summary>
        /// Test implementation of abstract Shape class for testing purposes.
        /// </summary>
        private class TestShape : Shape
        {
            public override PathF GetPath()
            {
                return new PathF();
            }
        }

        /// <summary>
        /// Tests that StrokeDashOffset setter properly stores valid positive values.
        /// Exercises the uncovered setter line by setting various positive double values.
        /// Expected: The value should be stored and retrievable via the getter.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(5.5)]
        [InlineData(100.0)]
        [InlineData(1000.5)]
        public void StrokeDashOffset_SetPositiveValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeDashOffset = value;

            // Assert
            Assert.Equal(value, shape.StrokeDashOffset);
        }

        /// <summary>
        /// Tests that StrokeDashOffset setter properly stores negative values.
        /// Exercises the uncovered setter line by setting negative double values.
        /// Expected: The value should be stored and retrievable via the getter.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-5.5)]
        [InlineData(-100.0)]
        [InlineData(-1000.5)]
        public void StrokeDashOffset_SetNegativeValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeDashOffset = value;

            // Assert
            Assert.Equal(value, shape.StrokeDashOffset);
        }

        /// <summary>
        /// Tests that StrokeDashOffset setter handles extreme double boundary values.
        /// Exercises the uncovered setter line with double min/max values.
        /// Expected: The extreme values should be stored and retrievable via the getter.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void StrokeDashOffset_SetBoundaryValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeDashOffset = value;

            // Assert
            Assert.Equal(value, shape.StrokeDashOffset);
        }

        /// <summary>
        /// Tests that StrokeDashOffset setter handles special double values like NaN and infinities.
        /// Exercises the uncovered setter line with special floating-point values.
        /// Expected: The special values should be stored and retrievable via the getter.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void StrokeDashOffset_SetSpecialValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeDashOffset = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(shape.StrokeDashOffset));
            }
            else
            {
                Assert.Equal(value, shape.StrokeDashOffset);
            }
        }

        /// <summary>
        /// Tests that StrokeDashOffset has the expected default value of 0.0.
        /// Verifies the default value matches the bindable property definition.
        /// Expected: Default value should be 0.0 as defined in StrokeDashOffsetProperty.
        /// </summary>
        [Fact]
        public void StrokeDashOffset_DefaultValue_ReturnsZero()
        {
            // Arrange
            var shape = new TestShape();

            // Act & Assert
            Assert.Equal(0.0, shape.StrokeDashOffset);
        }

        /// <summary>
        /// Tests that multiple consecutive StrokeDashOffset setter calls work correctly.
        /// Exercises the uncovered setter line multiple times with different values.
        /// Expected: Each set operation should properly store and make the value retrievable.
        /// </summary>
        [Fact]
        public void StrokeDashOffset_MultipleSetOperations_EachValueStoredCorrectly()
        {
            // Arrange
            var shape = new TestShape();
            var testValues = new double[] { 1.5, -2.3, 0.0, 100.7, -50.1 };

            foreach (var value in testValues)
            {
                // Act
                shape.StrokeDashOffset = value;

                // Assert
                Assert.Equal(value, shape.StrokeDashOffset);
            }
        }

        /// <summary>
        /// Tests that StrokeLineCap property returns the default value of Flat when not explicitly set.
        /// </summary>
        [Fact]
        public void StrokeLineCap_DefaultValue_ReturnsFlat()
        {
            // Arrange
            var shape = new TestShape();

            // Act
            var result = shape.StrokeLineCap;

            // Assert
            Assert.Equal(PenLineCap.Flat, result);
        }

        /// <summary>
        /// Tests that StrokeLineCap property can be set and retrieved correctly for all valid enum values.
        /// </summary>
        /// <param name="lineCap">The PenLineCap value to test</param>
        [Theory]
        [InlineData(PenLineCap.Flat)]
        [InlineData(PenLineCap.Square)]
        [InlineData(PenLineCap.Round)]
        public void StrokeLineCap_SetValidValue_ReturnsSetValue(PenLineCap lineCap)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeLineCap = lineCap;
            var result = shape.StrokeLineCap;

            // Assert
            Assert.Equal(lineCap, result);
        }

        /// <summary>
        /// Tests that StrokeLineCap property can handle enum values cast from integers, including out-of-range values.
        /// </summary>
        /// <param name="intValue">The integer value to cast to PenLineCap</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)] // Flat
        [InlineData(1)] // Square
        [InlineData(2)] // Round
        [InlineData(3)]
        [InlineData(999)]
        public void StrokeLineCap_SetCastFromInteger_HandlesAllValues(int intValue)
        {
            // Arrange
            var shape = new TestShape();
            var castedValue = (PenLineCap)intValue;

            // Act
            shape.StrokeLineCap = castedValue;
            var result = shape.StrokeLineCap;

            // Assert
            Assert.Equal(castedValue, result);
        }

        /// <summary>
        /// Tests that setting StrokeLineCap multiple times correctly updates the stored value.
        /// </summary>
        [Fact]
        public void StrokeLineCap_SetMultipleTimes_UpdatesCorrectly()
        {
            // Arrange
            var shape = new TestShape();

            // Act & Assert
            shape.StrokeLineCap = PenLineCap.Square;
            Assert.Equal(PenLineCap.Square, shape.StrokeLineCap);

            shape.StrokeLineCap = PenLineCap.Round;
            Assert.Equal(PenLineCap.Round, shape.StrokeLineCap);

            shape.StrokeLineCap = PenLineCap.Flat;
            Assert.Equal(PenLineCap.Flat, shape.StrokeLineCap);
        }

        /// <summary>
        /// Tests that setting valid PenLineJoin enum values correctly assigns the value to the StrokeLineJoin property.
        /// Verifies that all defined enum values can be set and retrieved properly.
        /// </summary>
        /// <param name="expectedValue">The PenLineJoin enum value to test setting.</param>
        [Theory]
        [InlineData(PenLineJoin.Miter)]
        [InlineData(PenLineJoin.Bevel)]
        [InlineData(PenLineJoin.Round)]
        public void StrokeLineJoin_SetValidEnumValue_SetsAndReturnsCorrectValue(PenLineJoin expectedValue)
        {
            // Arrange
            var shape = Substitute.ForPartsOf<Shape>();

            // Act
            shape.StrokeLineJoin = expectedValue;
            var actualValue = shape.StrokeLineJoin;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that setting invalid PenLineJoin enum values (outside defined range) still works correctly.
        /// Verifies that the property can handle enum values cast from integers outside the normal range.
        /// </summary>
        /// <param name="invalidEnumValue">The integer value to cast to PenLineJoin for testing.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void StrokeLineJoin_SetInvalidEnumValue_SetsAndReturnsValue(int invalidEnumValue)
        {
            // Arrange
            var shape = Substitute.ForPartsOf<Shape>();
            var expectedValue = (PenLineJoin)invalidEnumValue;

            // Act
            shape.StrokeLineJoin = expectedValue;
            var actualValue = shape.StrokeLineJoin;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests the default value of StrokeLineJoin property.
        /// Verifies that the property returns the expected default value when not explicitly set.
        /// </summary>
        [Fact]
        public void StrokeLineJoin_DefaultValue_ReturnsMiter()
        {
            // Arrange
            var shape = Substitute.ForPartsOf<Shape>();

            // Act
            var actualValue = shape.StrokeLineJoin;

            // Assert
            Assert.Equal(PenLineJoin.Miter, actualValue);
        }

        /// <summary>
        /// Tests multiple consecutive assignments to StrokeLineJoin property.
        /// Verifies that the property correctly updates when set multiple times in sequence.
        /// </summary>
        [Fact]
        public void StrokeLineJoin_MultipleAssignments_UpdatesCorrectly()
        {
            // Arrange
            var shape = Substitute.ForPartsOf<Shape>();

            // Act & Assert
            shape.StrokeLineJoin = PenLineJoin.Bevel;
            Assert.Equal(PenLineJoin.Bevel, shape.StrokeLineJoin);

            shape.StrokeLineJoin = PenLineJoin.Round;
            Assert.Equal(PenLineJoin.Round, shape.StrokeLineJoin);

            shape.StrokeLineJoin = PenLineJoin.Miter;
            Assert.Equal(PenLineJoin.Miter, shape.StrokeLineJoin);
        }
#if !(NETSTANDARD || !PLATFORM)
		/// <summary>
		/// Tests TransformPathForBounds with null path parameter.
		/// Should throw ArgumentNullException when path is null.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_NullPath_ThrowsArgumentNullException()
		{
			// Arrange
			var shape = new TestableShape();
			var viewBounds = new Rect(0, 0, 100, 100);

			// Act & Assert
			Assert.Throws<NullReferenceException>(() => shape.TransformPathForBounds(null, viewBounds));
		}

		/// <summary>
		/// Tests TransformPathForBounds with Aspect.None and no position adjustment needed.
		/// Should not apply any transformation when path is already within bounds.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_AspectNone_NoAdjustmentNeeded_NoTransformation()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.None, StrokeThickness = 2.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(10, 10, 50, 50);
			var viewBounds = new Rect(5, 5, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert
			mockPath.DidNotReceive().Transform(Arg.Any<Matrix3x2>());
		}

		/// <summary>
		/// Tests TransformPathForBounds with Aspect.None requiring position adjustment.
		/// Should apply translation when viewBounds position is greater than path bounds.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_AspectNone_RequiresAdjustment_AppliesTranslation()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.None, StrokeThickness = 2.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 50);
			var viewBounds = new Rect(20, 30, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert
			mockPath.Received(1).Transform(Arg.Is<Matrix3x2>(m => 
				m.M31 == 21.0f && m.M32 == 31.0f)); // Translation by (21, 31)
		}

		/// <summary>
		/// Tests TransformPathForBounds with Aspect.Fill.
		/// Should scale path to fill entire view bounds, potentially distorting aspect ratio.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_AspectFill_AppliesScaleAndTranslation()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 25);
			var viewBounds = new Rect(10, 20, 100, 80);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert
			mockPath.Received(1).Transform(Arg.Is<Matrix3x2>(m => 
				m.M11 == 2.0f && m.M22 == 3.2f)); // Scale by (2.0, 3.2)
		}

		/// <summary>
		/// Tests TransformPathForBounds with Aspect.Uniform.
		/// Should scale uniformly maintaining aspect ratio and center the result.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_AspectUniform_AppliesUniformScaleAndCentering()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Uniform, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 25);
			var viewBounds = new Rect(0, 0, 100, 80);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert
			mockPath.Received(1).Transform(Arg.Is<Matrix3x2>(m => 
				Math.Abs(m.M11 - 2.0f) < 0.001f && Math.Abs(m.M22 - 2.0f) < 0.001f)); // Uniform scale by min(2.0, 3.2) = 2.0
		}

		/// <summary>
		/// Tests TransformPathForBounds with Aspect.UniformToFill.
		/// Should scale uniformly to fill bounds, potentially clipping content.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_AspectUniformToFill_AppliesMaxScale()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.UniformToFill, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 25);
			var viewBounds = new Rect(0, 0, 100, 80);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert
			mockPath.Received(1).Transform(Arg.Is<Matrix3x2>(m => 
				Math.Abs(m.M11 - 3.2f) < 0.001f && Math.Abs(m.M22 - 3.2f) < 0.001f)); // Uniform scale by max(2.0, 3.2) = 3.2
		}

		/// <summary>
		/// Tests TransformPathForBounds with stroke thickness adjustment.
		/// Should adjust viewBounds by subtracting stroke thickness before calculations.
		/// </summary>
		[Theory]
		[InlineData(0.0)]
		[InlineData(5.0)]
		[InlineData(20.0)]
		public void TransformPathForBounds_StrokeThickness_AdjustsViewBounds(double strokeThickness)
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = strokeThickness };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 50);
			var viewBounds = new Rect(0, 0, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert - Verify scale accounts for stroke thickness adjustment
			var expectedWidth = 100.0 - strokeThickness;
			var expectedHeight = 100.0 - strokeThickness;
			var expectedScaleX = (float)(expectedWidth / 50.0);
			var expectedScaleY = (float)(expectedHeight / 50.0);

			mockPath.Received(1).Transform(Arg.Is<Matrix3x2>(m => 
				Math.Abs(m.M11 - expectedScaleX) < 0.001f && 
				Math.Abs(m.M22 - expectedScaleY) < 0.001f));
		}

		/// <summary>
		/// Tests TransformPathForBounds with zero or negative view bounds dimensions.
		/// Should handle edge cases gracefully when viewBounds has zero or negative dimensions.
		/// </summary>
		[Theory]
		[InlineData(0, 0)]
		[InlineData(-10, -10)]
		[InlineData(0, 50)]
		[InlineData(50, 0)]
		public void TransformPathForBounds_ZeroOrNegativeViewBounds_HandlesGracefully(double width, double height)
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 50);
			var viewBounds = new Rect(0, 0, width, height);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act & Assert - Should not throw exception
			shape.TransformPathForBounds(mockPath, viewBounds);
		}

		/// <summary>
		/// Tests TransformPathForBounds with zero path bounds.
		/// Should handle cases where path has zero width or height gracefully.
		/// </summary>
		[Theory]
		[InlineData(0, 0)]
		[InlineData(0, 50)]
		[InlineData(50, 0)]
		public void TransformPathForBounds_ZeroPathBounds_HandlesGracefully(float pathWidth, float pathHeight)
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, pathWidth, pathHeight);
			var viewBounds = new Rect(0, 0, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act & Assert - Should not throw exception
			shape.TransformPathForBounds(mockPath, viewBounds);
		}

		/// <summary>
		/// Tests TransformPathForBounds with NaN and infinity in calculations.
		/// Should handle edge cases where calculations result in NaN or infinity values.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_NanAndInfinityCalculations_HandlesGracefully()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 0, 0); // Zero bounds will cause division by zero
			var viewBounds = new Rect(0, 0, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act & Assert - Should not throw exception
			shape.TransformPathForBounds(mockPath, viewBounds);
		}

		/// <summary>
		/// Tests TransformPathForBounds with negative stroke thickness.
		/// Should handle negative stroke thickness values appropriately.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_NegativeStrokeThickness_HandlesGracefully()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = -10.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 50);
			var viewBounds = new Rect(0, 0, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act & Assert - Should not throw exception
			shape.TransformPathForBounds(mockPath, viewBounds);
		}

		/// <summary>
		/// Tests TransformPathForBounds with very large stroke thickness.
		/// Should handle cases where stroke thickness exceeds view bounds dimensions.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_VeryLargeStrokeThickness_HandlesGracefully()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.Fill, StrokeThickness = 1000.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 50);
			var viewBounds = new Rect(0, 0, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act & Assert - Should not throw exception
			shape.TransformPathForBounds(mockPath, viewBounds);
		}

		/// <summary>
		/// Tests TransformPathForBounds with identity matrix scenario.
		/// Should not call Transform when the calculated transformation is identity.
		/// </summary>
		[Fact]
		public void TransformPathForBounds_IdentityMatrix_NoTransformCalled()
		{
			// Arrange
			var shape = new TestableShape { Aspect = Stretch.None, StrokeThickness = 0.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(0, 0, 50, 50);
			var viewBounds = new Rect(0, 0, 100, 100);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act
			shape.TransformPathForBounds(mockPath, viewBounds);

			// Assert - No transformation should be applied when matrix is identity
			mockPath.DidNotReceive().Transform(Arg.Any<Matrix3x2>());
		}

		/// <summary>
		/// Tests TransformPathForBounds with all Stretch enum values.
		/// Should handle all valid Stretch enum values without throwing exceptions.
		/// </summary>
		[Theory]
		[InlineData(Stretch.None)]
		[InlineData(Stretch.Fill)]
		[InlineData(Stretch.Uniform)]
		[InlineData(Stretch.UniformToFill)]
		public void TransformPathForBounds_AllStretchValues_HandlesCorrectly(Stretch aspectValue)
		{
			// Arrange
			var shape = new TestableShape { Aspect = aspectValue, StrokeThickness = 5.0 };
			var mockPath = Substitute.For<PathF>();
			var pathBounds = new RectF(10, 10, 40, 30);
			var viewBounds = new Rect(0, 0, 100, 80);

			mockPath.GetBoundsByFlattening(1).Returns(pathBounds);

			// Act & Assert - Should not throw exception for any valid enum value
			shape.TransformPathForBounds(mockPath, viewBounds);
		}

		/// <summary>
		/// Testable concrete implementation of the abstract Shape class for unit testing.
		/// Exposes the internal TransformPathForBounds method for testing purposes.
		/// </summary>
		private class TestableShape : Shape
		{
			/// <summary>
			/// Required implementation of abstract GetPath method.
			/// Returns a simple rectangular path for testing purposes.
			/// </summary>
			public override PathF GetPath()
			{
				var path = new PathF();
				path.AppendRectangle(0, 0, 100, 100);
				return path;
			}

			/// <summary>
			/// Exposes the internal TransformPathForBounds method for testing.
			/// Allows unit tests to verify the transformation logic directly.
			/// </summary>
			public new void TransformPathForBounds(PathF path, Rect viewBounds)
			{
				base.TransformPathForBounds(path, viewBounds);
			}
		}
#endif

        /// <summary>
        /// Tests that the StrokeMiterLimit property returns the default value when no value has been set.
        /// Input: Default initialization.
        /// Expected: Returns 10.0 (the default value defined in StrokeMiterLimitProperty).
        /// </summary>
        [Fact]
        public void StrokeMiterLimit_DefaultValue_Returns10()
        {
            // Arrange
            var shape = new TestShape();

            // Act
            var result = shape.StrokeMiterLimit;

            // Assert
            Assert.Equal(10.0, result);
        }

        /// <summary>
        /// Tests that the StrokeMiterLimit property correctly sets and gets various valid double values.
        /// Input: Various valid double values including positive, negative, zero, and boundary values.
        /// Expected: Property returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(5.5)]
        [InlineData(-5.5)]
        [InlineData(100.0)]
        [InlineData(-100.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void StrokeMiterLimit_SetValidValues_ReturnsSetValue(double value)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeMiterLimit = value;
            var result = shape.StrokeMiterLimit;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the StrokeMiterLimit property correctly handles special double values.
        /// Input: Special double values like NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected: Property returns the exact special value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void StrokeMiterLimit_SetSpecialDoubleValues_ReturnsSetValue(double value)
        {
            // Arrange
            var shape = new TestShape();

            // Act
            shape.StrokeMiterLimit = value;
            var result = shape.StrokeMiterLimit;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that multiple set operations on StrokeMiterLimit property work correctly.
        /// Input: Sequence of different double values set consecutively.
        /// Expected: Property always returns the most recently set value.
        /// </summary>
        [Fact]
        public void StrokeMiterLimit_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var shape = new TestShape();

            // Act & Assert
            shape.StrokeMiterLimit = 5.0;
            Assert.Equal(5.0, shape.StrokeMiterLimit);

            shape.StrokeMiterLimit = 15.0;
            Assert.Equal(15.0, shape.StrokeMiterLimit);

            shape.StrokeMiterLimit = 0.0;
            Assert.Equal(0.0, shape.StrokeMiterLimit);

            shape.StrokeMiterLimit = -10.0;
            Assert.Equal(-10.0, shape.StrokeMiterLimit);
        }

        /// <summary>
        /// Tests that MeasureOverride returns early when base measurement provides non-zero size.
        /// Verifies the early return path at lines 215-217.
        /// </summary>
        [Fact]
        public void MeasureOverride_WhenBaseMeasureReturnsNonZeroSize_ReturnsEarly()
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = new Size(100, 50);

            // Act
            var result = shape.CallMeasureOverride(200, 300);

            // Assert
            Assert.Equal(100, result.Width);
            Assert.Equal(50, result.Height);
            Assert.False(shape.GetPathCalled);
        }

        /// <summary>
        /// Tests MeasureOverride with Stretch.None aspect when base measurement returns zero size.
        /// Verifies path bounds offset addition at lines 239-241.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 10, 20, 5, 3)]
        [InlineData(-5, -10, 15, 25, 2, 0)]
        [InlineData(100, 200, 50, 30, 1, 4)]
        public void MeasureOverride_WithAspectNone_AddsPathBoundsOffset(
            float pathX, float pathY, float pathWidth, float pathHeight,
            double strokeThickness, double expectedWidthAddition)
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = strokeThickness;
            shape.Aspect = Stretch.None;

            var pathBounds = new RectF(pathX, pathY, pathWidth, pathHeight);
            shape.PathBounds = pathBounds;

            // Act
            var result = shape.CallMeasureOverride(double.PositiveInfinity, double.PositiveInfinity);

            // Assert
            var expectedWidth = pathWidth + pathX + strokeThickness;
            var expectedHeight = pathHeight + pathY + strokeThickness;
            Assert.Equal(expectedWidth, result.Width);
            Assert.Equal(expectedHeight, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with Stretch.Fill aspect using HeightRequest and WidthRequest.
        /// Verifies the Fill logic at lines 244-253.
        /// </summary>
        [Theory]
        [InlineData(100, 200, -1, -1, 100, 200)] // Use constraints
        [InlineData(double.PositiveInfinity, 200, 150, -1, 150, 200)] // Use WidthRequest
        [InlineData(100, double.PositiveInfinity, -1, 250, 100, 250)] // Use HeightRequest
        [InlineData(100, 200, 80, 120, 80, 120)] // Use both requests
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, 75, 90, 75, 90)] // Infinite constraints, use requests
        public void MeasureOverride_WithAspectFill_UsesRequestsOrConstraints(
            double widthConstraint, double heightConstraint,
            double widthRequest, double heightRequest,
            double expectedWidth, double expectedHeight)
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = 2;
            shape.Aspect = Stretch.Fill;
            shape.WidthRequest = widthRequest;
            shape.HeightRequest = heightRequest;
            shape.PathBounds = new RectF(0, 0, 50, 30);

            // Act
            var result = shape.CallMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedWidth + shape.StrokeThickness, result.Width);
            Assert.Equal(expectedHeight + shape.StrokeThickness, result.Height);
        }

        /// <summary>
        /// Tests MeasureOverride with Stretch.Uniform aspect maintaining aspect ratio.
        /// Verifies the Uniform scaling logic at lines 256-262.
        /// </summary>
        [Theory]
        [InlineData(100, 80, 50, 40, 2, 50, 40)] // Width constraint is limiting
        [InlineData(60, 100, 50, 40, 2, 48, 38.4)] // Height constraint is limiting
        [InlineData(200, 200, 50, 40, 0, 200, 160)] // Equal scaling
        public void MeasureOverride_WithAspectUniform_MaintainsAspectRatio(
            double widthConstraint, double heightConstraint,
            float pathWidth, float pathHeight, double strokeThickness,
            double expectedWidth, double expectedHeight)
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = strokeThickness;
            shape.Aspect = Stretch.Uniform;
            shape.PathBounds = new RectF(0, 0, pathWidth, pathHeight);

            // Act
            var result = shape.CallMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedWidth + strokeThickness, result.Width, 1);
            Assert.Equal(expectedHeight + strokeThickness, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride with Stretch.UniformToFill aspect using maximum scale.
        /// Verifies the UniformToFill scaling logic at lines 265-274.
        /// </summary>
        [Theory]
        [InlineData(100, 80, 50, 40, 1, 100, 80)] // Height constraint is limiting
        [InlineData(60, 100, 50, 40, 2, 58, 46.4)] // Width constraint is limiting
        public void MeasureOverride_WithAspectUniformToFill_UsesMaxScale(
            double widthConstraint, double heightConstraint,
            float pathWidth, float pathHeight, double strokeThickness,
            double expectedWidth, double expectedHeight)
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = strokeThickness;
            shape.Aspect = Stretch.UniformToFill;
            shape.PathBounds = new RectF(0, 0, pathWidth, pathHeight);

            // Act
            var result = shape.CallMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedWidth + strokeThickness, result.Width, 1);
            Assert.Equal(expectedHeight + strokeThickness, result.Height, 1);
        }

        /// <summary>
        /// Tests MeasureOverride with infinite and NaN constraints.
        /// Verifies scale calculation handling at lines 233-234.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 100, Stretch.Uniform)]
        [InlineData(100, double.PositiveInfinity, Stretch.Uniform)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, Stretch.Uniform)]
        [InlineData(double.NaN, 100, Stretch.Uniform)]
        [InlineData(100, double.NaN, Stretch.Uniform)]
        public void MeasureOverride_WithInfiniteOrNaNConstraints_HandlesGracefully(
            double widthConstraint, double heightConstraint, Stretch aspect)
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = 2;
            shape.Aspect = aspect;
            shape.PathBounds = new RectF(0, 0, 50, 40);

            // Act
            var result = shape.CallMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
            Assert.False(double.IsNaN(result.Width));
            Assert.False(double.IsNaN(result.Height));
        }

        /// <summary>
        /// Tests MeasureOverride with zero and negative constraints.
        /// Verifies scale calculation and boundary handling.
        /// </summary>
        [Theory]
        [InlineData(0, 100, Stretch.None)]
        [InlineData(100, 0, Stretch.None)]
        [InlineData(-50, 100, Stretch.Fill)]
        [InlineData(100, -50, Stretch.Fill)]
        public void MeasureOverride_WithZeroOrNegativeConstraints_HandlesGracefully(
            double widthConstraint, double heightConstraint, Stretch aspect)
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = 1;
            shape.Aspect = aspect;
            shape.PathBounds = new RectF(0, 0, 30, 25);

            // Act
            var result = shape.CallMeasureOverride(widthConstraint, heightConstraint);

            // Assert
            Assert.True(result.Width >= 0);
            Assert.True(result.Height >= 0);
        }

        /// <summary>
        /// Tests that path bounds are calculated when base measure returns zero.
        /// Verifies path calculation at lines 222-226.
        /// </summary>
        [Fact]
        public void MeasureOverride_WhenBaseMeasureReturnsZero_CalculatesPathBounds()
        {
            // Arrange
            var shape = new TestShape();
            shape.BaseMeasureResult = Size.Zero;
            shape.StrokeThickness = 3;
            shape.Aspect = Stretch.None;
            shape.PathBounds = new RectF(10, 15, 40, 35);

            // Act
            var result = shape.CallMeasureOverride(200, 150);

            // Assert
            Assert.True(shape.GetPathCalled);
            Assert.Equal(53, result.Width); // 40 + 10 + 3
            Assert.Equal(53, result.Height); // 35 + 15 + 3
        }

        /// <summary>
        /// Tests WidthForPathComputation property when Width is -1 (not arranged) and should return fallback width.
        /// Tests various fallback width values including zero, positive, negative, and special double values.
        /// Verifies that the conditional logic correctly returns _fallbackWidth when Width == -1.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(100.0)]
        [InlineData(-50.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void WidthForPathComputation_WidthIsMinusOne_ReturnsFallbackWidth(double fallbackWidth)
        {
            // Arrange
            var shape = new TestableShape();
            shape.SetFallbackWidth(fallbackWidth);
            shape.SetMockWidth(-1);

            // Act
            var result = shape.WidthForPathComputation;

            // Assert
            Assert.Equal(fallbackWidth, result);
        }

        /// <summary>
        /// Tests WidthForPathComputation property when Width is NaN and should return fallback width.
        /// Tests NaN fallback width value.
        /// Verifies that the conditional logic correctly returns _fallbackWidth when Width == -1.
        /// </summary>
        [Fact]
        public void WidthForPathComputation_WidthIsMinusOneAndFallbackWidthIsNaN_ReturnsNaN()
        {
            // Arrange
            var shape = new TestableShape();
            shape.SetFallbackWidth(double.NaN);
            shape.SetMockWidth(-1);

            // Act
            var result = shape.WidthForPathComputation;

            // Assert
            Assert.True(double.IsNaN(result));
        }

        /// <summary>
        /// Tests WidthForPathComputation property when Width is not -1 and should return Width value.
        /// Tests various Width values including zero, positive, negative, and special double values.
        /// Verifies that the conditional logic correctly returns Width when Width != -1.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(50.0)]
        [InlineData(-100.0)]
        [InlineData(-2.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void WidthForPathComputation_WidthIsNotMinusOne_ReturnsWidth(double width)
        {
            // Arrange
            var shape = new TestableShape();
            shape.SetFallbackWidth(999.0); // Set different fallback value to ensure Width is returned
            shape.SetMockWidth(width);

            // Act
            var result = shape.WidthForPathComputation;

            // Assert
            Assert.Equal(width, result);
        }

        /// <summary>
        /// Tests WidthForPathComputation property when Width is NaN and should return Width value.
        /// Verifies that NaN Width is correctly returned when Width != -1.
        /// </summary>
        [Fact]
        public void WidthForPathComputation_WidthIsNaN_ReturnsNaN()
        {
            // Arrange
            var shape = new TestableShape();
            shape.SetFallbackWidth(100.0);
            shape.SetMockWidth(double.NaN);

            // Act
            var result = shape.WidthForPathComputation;

            // Assert
            Assert.True(double.IsNaN(result));
        }

        /// <summary>
        /// Tests WidthForPathComputation property boundary condition at exactly -1.
        /// Verifies that the exact value -1.0 triggers fallback width behavior.
        /// </summary>
        [Fact]
        public void WidthForPathComputation_WidthIsExactlyMinusOne_ReturnsFallbackWidth()
        {
            // Arrange
            var shape = new TestableShape();
            var fallbackWidth = 42.5;
            shape.SetFallbackWidth(fallbackWidth);
            shape.SetMockWidth(-1.0);

            // Act
            var result = shape.WidthForPathComputation;

            // Assert
            Assert.Equal(fallbackWidth, result);
        }

        /// <summary>
        /// Tests WidthForPathComputation property with values very close to but not exactly -1.
        /// Verifies that values close to -1 but not exactly -1 return Width instead of fallback.
        /// </summary>
        [Theory]
        [InlineData(-0.999999999)]
        [InlineData(-1.000000001)]
        public void WidthForPathComputation_WidthIsCloseToMinusOneButNotExact_ReturnsWidth(double width)
        {
            // Arrange
            var shape = new TestableShape();
            shape.SetFallbackWidth(100.0);
            shape.SetMockWidth(width);

            // Act
            var result = shape.WidthForPathComputation;

            // Assert
            Assert.Equal(width, result);
        }

        /// <summary>
        /// Testable concrete implementation of the abstract Shape class.
        /// Provides access to protected/private members for testing purposes.
        /// </summary>
        private class TestableShape : Shape
        {
            private double _mockWidth = -1;

            public override PathF GetPath()
            {
                return new PathF();
            }

            public new double Width => _mockWidth;

            public void SetFallbackWidth(double value)
            {
                _fallbackWidth = value;
            }

            public void SetMockWidth(double value)
            {
                _mockWidth = value;
            }
        }

        /// <summary>
        /// Tests the HeightForPathComputation property when Height is not -1.
        /// Verifies that the property returns the actual Height value when the shape has been arranged.
        /// </summary>
        /// <param name="heightValue">The height value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(100.0)]
        [InlineData(1000.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void HeightForPathComputation_WhenHeightIsNotMinusOne_ReturnsActualHeight(double heightValue)
        {
            // Arrange
            var testShape = new TestShape();
            testShape.HeightRequest = heightValue;

            // Act
            double result = testShape.HeightForPathComputation;

            // Assert
            Assert.Equal(heightValue, result);
        }

        /// <summary>
        /// Tests the HeightForPathComputation property when Height is -1.
        /// Verifies that the property returns the fallback height value when the shape has never been arranged.
        /// </summary>
        /// <param name="fallbackHeight">The fallback height value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(50.0)]
        [InlineData(100.0)]
        [InlineData(-10.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void HeightForPathComputation_WhenHeightIsMinusOne_ReturnsFallbackHeight(double fallbackHeight)
        {
            // Arrange
            var testShape = new TestShape();
            testShape.SetFallbackHeight(fallbackHeight);
            testShape.SetMockHeight(-1);

            // Act
            double result = testShape.HeightForPathComputation;

            // Assert
            Assert.Equal(fallbackHeight, result);
        }

        /// <summary>
        /// Tests the HeightForPathComputation property with the boundary condition of Height exactly equal to -1.
        /// Verifies the conditional logic switches correctly at the boundary value.
        /// </summary>
        [Fact]
        public void HeightForPathComputation_WhenHeightIsExactlyMinusOne_ReturnsFallbackHeight()
        {
            // Arrange
            var testShape = new TestShape();
            const double expectedFallbackHeight = 42.5;
            testShape.SetFallbackHeight(expectedFallbackHeight);
            testShape.SetMockHeight(-1.0);

            // Act
            double result = testShape.HeightForPathComputation;

            // Assert
            Assert.Equal(expectedFallbackHeight, result);
        }

        /// <summary>
        /// Tests the HeightForPathComputation property with Height values near -1 but not exactly -1.
        /// Verifies that the conditional logic correctly handles values close to but not equal to -1.
        /// </summary>
        /// <param name="heightValue">The height value close to but not equal to -1.</param>
        [Theory]
        [InlineData(-0.9999999999999999)]
        [InlineData(-1.0000000000000001)]
        [InlineData(-0.9)]
        [InlineData(-1.1)]
        public void HeightForPathComputation_WhenHeightIsNearMinusOne_ReturnsActualHeight(double heightValue)
        {
            // Arrange
            var testShape = new TestShape();
            const double fallbackHeight = 100.0;
            testShape.SetFallbackHeight(fallbackHeight);
            testShape.SetMockHeight(heightValue);

            // Act
            double result = testShape.HeightForPathComputation;

            // Assert
            Assert.Equal(heightValue, result);
            Assert.NotEqual(fallbackHeight, result);
        }

        /// <summary>
        /// Tests that Unsubscribe properly removes PropertyChanged event handler for regular Brush.
        /// Verifies that when TryGetSource returns a regular Brush (not GradientBrush),
        /// only the PropertyChanged event is unsubscribed and base.Unsubscribe is called.
        /// </summary>
        [Fact]
        public void WeakBrushChangedProxy_Unsubscribe_WithRegularBrush_UnsubscribesFromPropertyChanged()
        {
            // Arrange
            var brush = Substitute.For<Brush>();
            var eventHandler = Substitute.For<EventHandler>();
            var proxy = new TestableWeakBrushChangedProxy();

            // Subscribe to set up the event handlers
            proxy.Subscribe(brush, eventHandler);

            // Clear any received calls from Subscribe to focus on Unsubscribe
            brush.ClearReceivedCalls();

            // Act
            proxy.Unsubscribe();

            // Assert
            brush.Received(1).PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Tests that Unsubscribe properly removes both PropertyChanged and InvalidateGradientBrushRequested event handlers for GradientBrush.
        /// Verifies that when TryGetSource returns a GradientBrush,
        /// both PropertyChanged and InvalidateGradientBrushRequested events are unsubscribed and base.Unsubscribe is called.
        /// </summary>
        [Fact]
        public void WeakBrushChangedProxy_Unsubscribe_WithGradientBrush_UnsubscribesFromBothEvents()
        {
            // Arrange
            var gradientBrush = Substitute.For<GradientBrush>();
            var eventHandler = Substitute.For<EventHandler>();
            var proxy = new TestableWeakBrushChangedProxy();

            // Subscribe to set up the event handlers
            proxy.Subscribe(gradientBrush, eventHandler);

            // Clear any received calls from Subscribe to focus on Unsubscribe
            gradientBrush.ClearReceivedCalls();

            // Act
            proxy.Unsubscribe();

            // Assert
            gradientBrush.Received(1).PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            gradientBrush.Received(1).InvalidateGradientBrushRequested -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests that Unsubscribe handles null source gracefully.
        /// Verifies that when TryGetSource returns false (no source available),
        /// the method completes without throwing and still calls base.Unsubscribe.
        /// </summary>
        [Fact]
        public void WeakBrushChangedProxy_Unsubscribe_WithNullSource_DoesNotThrow()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            // Do not subscribe, so TryGetSource will return false

            // Act & Assert
            var exception = Record.Exception(() => proxy.Unsubscribe());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the Shape constructor properly initializes the object with correct default values.
        /// Verifies that all bindable properties are set to their expected default values
        /// and that the object implements the required interfaces.
        /// </summary>
        [Fact]
        public void Constructor_WhenCalled_InitializesWithCorrectDefaultValues()
        {
            // Arrange & Act
            var shape = new TestShape();

            // Assert
            Assert.NotNull(shape);
            Assert.IsAssignableFrom<View>(shape);
            Assert.IsAssignableFrom<IShapeView>(shape);
            Assert.IsAssignableFrom<IShape>(shape);

            // Verify default property values from BindableProperty definitions
            Assert.Null(shape.Fill);
            Assert.Null(shape.Stroke);
            Assert.Equal(1.0, shape.StrokeThickness);
            Assert.NotNull(shape.StrokeDashArray);
            Assert.Empty(shape.StrokeDashArray);
            Assert.Equal(0.0, shape.StrokeDashOffset);
            Assert.Equal(PenLineCap.Flat, shape.StrokeLineCap);
            Assert.Equal(PenLineJoin.Miter, shape.StrokeLineJoin);
            Assert.Equal(10.0, shape.StrokeMiterLimit);
            Assert.Equal(Stretch.None, shape.Aspect);
        }

        /// <summary>
        /// Tests that the Shape constructor properly initializes interface properties.
        /// Verifies that interface implementations return expected values.
        /// </summary>
        [Fact]
        public void Constructor_WhenCalled_InitializesInterfacePropertiesCorrectly()
        {
            // Arrange & Act
            var shape = new TestShape();
            var shapeView = (IShapeView)shape;
            var stroke = (IStroke)shape;
            var graphicsShape = (IShape)shape;

            // Assert
            Assert.Same(shape, shapeView.Shape);
            Assert.Equal(PathAspect.None, shapeView.Aspect);
            Assert.Same(shape.Fill, shapeView.Fill);
            Assert.Same(shape.Stroke, stroke.Stroke);
            Assert.Equal(LineCap.Flat, stroke.StrokeLineCap);
            Assert.Equal(LineJoin.Miter, stroke.StrokeLineJoin);
            Assert.Equal(0.0f, stroke.StrokeDashOffset);
            Assert.Equal(10.0f, stroke.StrokeMiterLimit);
            Assert.Empty(shape.StrokeDashPattern);
        }

    }


    /// <summary>
    /// Tests for the Subscribe method of WeakBrushChangedProxy class.
    /// </summary>
    public partial class WeakBrushChangedProxyTests : BaseTestFixture
    {
        /// <summary>
        /// Tests Subscribe method with no existing source and regular Brush.
        /// Verifies that PropertyChanged event is subscribed without unsubscribing from previous source.
        /// </summary>
        [Fact]
        public void Subscribe_WithNoExistingSource_RegularBrush_SubscribesToPropertyChanged()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            var brush = Substitute.For<Brush>();
            var handler = Substitute.For<EventHandler>();

            proxy.SetMockExistingSource(null, false);

            // Act
            proxy.Subscribe(brush, handler);

            // Assert
            brush.Received(1).PropertyChanged += Arg.Any<EventHandler>();
            brush.DidNotReceive().PropertyChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with no existing source and GradientBrush.
        /// Verifies that both PropertyChanged and InvalidateGradientBrushRequested events are subscribed.
        /// </summary>
        [Fact]
        public void Subscribe_WithNoExistingSource_GradientBrush_SubscribesToBothEvents()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            var gradientBrush = Substitute.For<GradientBrush>();
            var handler = Substitute.For<EventHandler>();

            proxy.SetMockExistingSource(null, false);

            // Act
            proxy.Subscribe(gradientBrush, handler);

            // Assert
            gradientBrush.Received(1).PropertyChanged += Arg.Any<EventHandler>();
            gradientBrush.Received(1).InvalidateGradientBrushRequested += Arg.Any<EventHandler>();
            gradientBrush.DidNotReceive().PropertyChanged -= Arg.Any<EventHandler>();
            gradientBrush.DidNotReceive().InvalidateGradientBrushRequested -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with existing regular Brush source and new regular Brush.
        /// Verifies that previous PropertyChanged is unsubscribed and new one is subscribed.
        /// </summary>
        [Fact]
        public void Subscribe_WithExistingRegularBrush_NewRegularBrush_UnsubscribesAndSubscribes()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            var existingBrush = Substitute.For<Brush>();
            var newBrush = Substitute.For<Brush>();
            var handler = Substitute.For<EventHandler>();

            proxy.SetMockExistingSource(existingBrush, true);

            // Act
            proxy.Subscribe(newBrush, handler);

            // Assert
            existingBrush.Received(1).PropertyChanged -= Arg.Any<EventHandler>();
            existingBrush.DidNotReceive().PropertyChanged += Arg.Any<EventHandler>();
            newBrush.Received(1).PropertyChanged += Arg.Any<EventHandler>();
            newBrush.DidNotReceive().PropertyChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with existing GradientBrush source and new regular Brush.
        /// Verifies that previous GradientBrush events are unsubscribed and new Brush PropertyChanged is subscribed.
        /// </summary>
        [Fact]
        public void Subscribe_WithExistingGradientBrush_NewRegularBrush_UnsubscribesGradientAndSubscribesBrush()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            var existingGradientBrush = Substitute.For<GradientBrush>();
            var newBrush = Substitute.For<Brush>();
            var handler = Substitute.For<EventHandler>();

            proxy.SetMockExistingSource(existingGradientBrush, true);

            // Act
            proxy.Subscribe(newBrush, handler);

            // Assert
            existingGradientBrush.Received(1).PropertyChanged -= Arg.Any<EventHandler>();
            existingGradientBrush.Received(1).InvalidateGradientBrushRequested -= Arg.Any<EventHandler>();
            existingGradientBrush.DidNotReceive().PropertyChanged += Arg.Any<EventHandler>();
            existingGradientBrush.DidNotReceive().InvalidateGradientBrushRequested += Arg.Any<EventHandler>();
            newBrush.Received(1).PropertyChanged += Arg.Any<EventHandler>();
            newBrush.DidNotReceive().PropertyChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with existing regular Brush source and new GradientBrush.
        /// Verifies that previous Brush PropertyChanged is unsubscribed and new GradientBrush events are subscribed.
        /// </summary>
        [Fact]
        public void Subscribe_WithExistingRegularBrush_NewGradientBrush_UnsubscribesBrushAndSubscribesGradient()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            var existingBrush = Substitute.For<Brush>();
            var newGradientBrush = Substitute.For<GradientBrush>();
            var handler = Substitute.For<EventHandler>();

            proxy.SetMockExistingSource(existingBrush, true);

            // Act
            proxy.Subscribe(newGradientBrush, handler);

            // Assert
            existingBrush.Received(1).PropertyChanged -= Arg.Any<EventHandler>();
            existingBrush.DidNotReceive().PropertyChanged += Arg.Any<EventHandler>();
            newGradientBrush.Received(1).PropertyChanged += Arg.Any<EventHandler>();
            newGradientBrush.Received(1).InvalidateGradientBrushRequested += Arg.Any<EventHandler>();
            newGradientBrush.DidNotReceive().PropertyChanged -= Arg.Any<EventHandler>();
            newGradientBrush.DidNotReceive().InvalidateGradientBrushRequested -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with existing GradientBrush source and new GradientBrush.
        /// Verifies that previous GradientBrush events are unsubscribed and new GradientBrush events are subscribed.
        /// </summary>
        [Fact]
        public void Subscribe_WithExistingGradientBrush_NewGradientBrush_UnsubscribesAndSubscribesGradientEvents()
        {
            // Arrange
            var proxy = new TestableWeakBrushChangedProxy();
            var existingGradientBrush = Substitute.For<GradientBrush>();
            var newGradientBrush = Substitute.For<GradientBrush>();
            var handler = Substitute.For<EventHandler>();

            proxy.SetMockExistingSource(existingGradientBrush, true);

            // Act
            proxy.Subscribe(newGradientBrush, handler);

            // Assert
            existingGradientBrush.Received(1).PropertyChanged -= Arg.Any<EventHandler>();
            existingGradientBrush.Received(1).InvalidateGradientBrushRequested -= Arg.Any<EventHandler>();
            existingGradientBrush.DidNotReceive().PropertyChanged += Arg.Any<EventHandler>();
            existingGradientBrush.DidNotReceive().InvalidateGradientBrushRequested += Arg.Any<EventHandler>();
            newGradientBrush.Received(1).PropertyChanged += Arg.Any<EventHandler>();
            newGradientBrush.Received(1).InvalidateGradientBrushRequested += Arg.Any<EventHandler>();
            newGradientBrush.DidNotReceive().PropertyChanged -= Arg.Any<EventHandler>();
            newGradientBrush.DidNotReceive().InvalidateGradientBrushRequested -= Arg.Any<EventHandler>();
        }
    }
}