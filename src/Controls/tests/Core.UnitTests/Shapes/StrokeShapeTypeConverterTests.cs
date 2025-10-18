#nullable disable

using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class StrokeShapeTypeConverterTests
    {
        private readonly StrokeShapeTypeConverter _converter;
        private readonly ITypeDescriptorContext _context;
        private readonly CultureInfo _culture;

        public StrokeShapeTypeConverterTests()
        {
            _converter = new StrokeShapeTypeConverter();
            _context = Substitute.For<ITypeDescriptorContext>();
            _culture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Tests that ConvertFrom handles null input correctly by throwing InvalidOperationException.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            object value = null;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(_context, _culture, value));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles empty string input correctly by throwing InvalidOperationException.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ThrowsInvalidOperationException()
        {
            // Arrange
            object value = "";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(_context, _culture, value));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles whitespace-only string input correctly by throwing InvalidOperationException.
        /// </summary>
        [Fact]
        public void ConvertFrom_WhitespaceOnlyString_ThrowsInvalidOperationException()
        {
            // Arrange
            object value = "   \t\n  ";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(_context, _culture, value));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles invalid string input that doesn't match any shape pattern.
        /// </summary>
        [Fact]
        public void ConvertFrom_InvalidShapeName_ThrowsInvalidOperationException()
        {
            // Arrange
            object value = "InvalidShape";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(_context, _culture, value));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom creates an Ellipse when input starts with "Ellipse".
        /// </summary>
        [Theory]
        [InlineData("Ellipse")]
        [InlineData("ellipse")]
        [InlineData("ELLIPSE")]
        [InlineData("Ellipse with extra text")]
        public void ConvertFrom_EllipseInput_ReturnsEllipse(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Ellipse>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Line with default properties when input is "Line" without parameters.
        /// </summary>
        [Theory]
        [InlineData("Line")]
        [InlineData("line")]
        [InlineData("LINE")]
        public void ConvertFrom_LineWithoutParameters_ReturnsDefaultLine(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Line>(result);
            var line = (Line)result;
            Assert.Equal(0, line.X1);
            Assert.Equal(0, line.Y1);
            Assert.Equal(0, line.X2);
            Assert.Equal(0, line.Y2);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Line with single point when input contains one valid point.
        /// </summary>
        [Fact]
        public void ConvertFrom_LineWithSinglePoint_ReturnsLineWithSinglePoint()
        {
            // Arrange
            string input = "Line 10,20";

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Line>(result);
            var line = (Line)result;
            Assert.Equal(10, line.X1);
            Assert.Equal(20, line.Y1);
            Assert.Equal(0, line.X2);
            Assert.Equal(0, line.Y2);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Line with two points when input contains two valid points.
        /// </summary>
        [Fact]
        public void ConvertFrom_LineWithTwoPoints_ReturnsLineWithBothPoints()
        {
            // Arrange
            string input = "Line 10,20 30,40";

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Line>(result);
            var line = (Line)result;
            Assert.Equal(10, line.X1);
            Assert.Equal(20, line.Y1);
            Assert.Equal(30, line.X2);
            Assert.Equal(40, line.Y2);
        }

        /// <summary>
        /// Tests that ConvertFrom handles Line with invalid points by returning default Line.
        /// This tests the uncovered line 49 where points is null or empty.
        /// </summary>
        [Theory]
        [InlineData("Line invalidpoints")]
        [InlineData("Line ")]
        [InlineData("Line ,")]
        public void ConvertFrom_LineWithInvalidPoints_ReturnsDefaultLine(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Line>(result);
            var line = (Line)result;
            Assert.Equal(0, line.X1);
            Assert.Equal(0, line.Y1);
            Assert.Equal(0, line.X2);
            Assert.Equal(0, line.Y2);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Path with default properties when input is "Path" without parameters.
        /// </summary>
        [Theory]
        [InlineData("Path")]
        [InlineData("path")]
        [InlineData("PATH")]
        public void ConvertFrom_PathWithoutParameters_ReturnsDefaultPath(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Path>(result);
            var path = (Path)result;
            Assert.Null(path.Data);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Path with geometry when input contains valid path data.
        /// </summary>
        [Fact]
        public void ConvertFrom_PathWithValidData_ReturnsPathWithGeometry()
        {
            // Arrange
            string input = "Path M 10,10 L 20,20";

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Path>(result);
            var path = (Path)result;
            Assert.NotNull(path.Data);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Polygon with default properties when input is "Polygon" without parameters.
        /// </summary>
        [Theory]
        [InlineData("Polygon")]
        [InlineData("polygon")]
        [InlineData("POLYGON")]
        public void ConvertFrom_PolygonWithoutParameters_ReturnsDefaultPolygon(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Polygon>(result);
            var polygon = (Polygon)result;
            Assert.Null(polygon.Points);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Polygon with points when input contains valid point data.
        /// </summary>
        [Fact]
        public void ConvertFrom_PolygonWithValidPoints_ReturnsPolygonWithPoints()
        {
            // Arrange
            string input = "Polygon 10,20 30,40 50,60";

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Polygon>(result);
            var polygon = (Polygon)result;
            Assert.NotNull(polygon.Points);
            Assert.Equal(3, polygon.Points.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom handles Polygon with invalid points by returning default Polygon.
        /// This tests the uncovered line 86 where points is null or empty.
        /// </summary>
        [Theory]
        [InlineData("Polygon invalidpoints")]
        [InlineData("Polygon ")]
        [InlineData("Polygon ,")]
        public void ConvertFrom_PolygonWithInvalidPoints_ReturnsDefaultPolygon(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Polygon>(result);
            var polygon = (Polygon)result;
            Assert.Null(polygon.Points);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Polyline with default properties when input is "Polyline" without parameters.
        /// </summary>
        [Theory]
        [InlineData("Polyline")]
        [InlineData("polyline")]
        [InlineData("POLYLINE")]
        public void ConvertFrom_PolylineWithoutParameters_ReturnsDefaultPolyline(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Polyline>(result);
            var polyline = (Polyline)result;
            Assert.Null(polyline.Points);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Polyline with points when input contains valid point data.
        /// </summary>
        [Fact]
        public void ConvertFrom_PolylineWithValidPoints_ReturnsPolylineWithPoints()
        {
            // Arrange
            string input = "Polyline 10,20 30,40 50,60";

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Polyline>(result);
            var polyline = (Polyline)result;
            Assert.NotNull(polyline.Points);
            Assert.Equal(3, polyline.Points.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom handles Polyline with invalid points by returning default Polyline.
        /// This tests the uncovered line 101 where points is null or empty.
        /// </summary>
        [Theory]
        [InlineData("Polyline invalidpoints")]
        [InlineData("Polyline ")]
        [InlineData("Polyline ,")]
        public void ConvertFrom_PolylineWithInvalidPoints_ReturnsDefaultPolyline(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Polyline>(result);
            var polyline = (Polyline)result;
            Assert.Null(polyline.Points);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a Rectangle when input starts with "Rectangle".
        /// </summary>
        [Theory]
        [InlineData("Rectangle")]
        [InlineData("rectangle")]
        [InlineData("RECTANGLE")]
        [InlineData("Rectangle with extra text")]
        public void ConvertFrom_RectangleInput_ReturnsRectangle(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<Rectangle>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a RoundRectangle with default corner radius when input is "RoundRectangle" without parameters.
        /// </summary>
        [Theory]
        [InlineData("RoundRectangle")]
        [InlineData("roundrectangle")]
        [InlineData("ROUNDRECTANGLE")]
        public void ConvertFrom_RoundRectangleWithoutParameters_ReturnsDefaultRoundRectangle(string input)
        {
            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<RoundRectangle>(result);
            var roundRect = (RoundRectangle)result;
            Assert.Equal(new CornerRadius(), roundRect.CornerRadius);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a RoundRectangle with corner radius when input contains valid corner radius data.
        /// </summary>
        [Fact]
        public void ConvertFrom_RoundRectangleWithValidCornerRadius_ReturnsRoundRectangleWithCornerRadius()
        {
            // Arrange
            string input = "RoundRectangle 5";

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<RoundRectangle>(result);
            var roundRect = (RoundRectangle)result;
            Assert.Equal(new CornerRadius(5), roundRect.CornerRadius);
        }

        /// <summary>
        /// Tests that ConvertFrom creates a RoundRectangle from valid double input.
        /// This tests the uncovered line 131 for double parsing fallback.
        /// </summary>
        [Theory]
        [InlineData("5.5")]
        [InlineData("10")]
        [InlineData("0")]
        [InlineData("100.75")]
        public void ConvertFrom_ValidDouble_ReturnsRoundRectangleWithCornerRadius(string input)
        {
            // Arrange
            double expectedRadius = double.Parse(input);

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<RoundRectangle>(result);
            var roundRect = (RoundRectangle)result;
            Assert.Equal(new CornerRadius(expectedRadius), roundRect.CornerRadius);
        }

        /// <summary>
        /// Tests that ConvertFrom handles special double values correctly.
        /// </summary>
        [Theory]
        [InlineData("0.0")]
        [InlineData("-5.5")]
        [InlineData("999999.99")]
        public void ConvertFrom_SpecialDoubleValues_ReturnsRoundRectangleWithCornerRadius(string input)
        {
            // Arrange
            double expectedRadius = double.Parse(input);

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<RoundRectangle>(result);
            var roundRect = (RoundRectangle)result;
            Assert.Equal(new CornerRadius(expectedRadius), roundRect.CornerRadius);
        }

        /// <summary>
        /// Tests that ConvertFrom handles different cultures correctly for case-insensitive matching.
        /// </summary>
        [Fact]
        public void ConvertFrom_DifferentCulture_HandlesCorrectly()
        {
            // Arrange
            var turkishCulture = new CultureInfo("tr-TR");
            string input = "ellipse";

            // Act
            var result = _converter.ConvertFrom(_context, turkishCulture, input);

            // Assert
            Assert.IsType<Ellipse>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom handles non-string objects correctly by converting to string first.
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObject_ConvertsToStringFirst()
        {
            // Arrange
            var customObject = new CustomToStringObject("Ellipse");

            // Act
            var result = _converter.ConvertFrom(_context, _culture, customObject);

            // Assert
            Assert.IsType<Ellipse>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom handles boundary cases for numeric inputs.
        /// </summary>
        [Theory]
        [InlineData("0")]
        [InlineData("1.7976931348623157E+308")] // Double.MaxValue
        [InlineData("-1.7976931348623157E+308")] // Double.MinValue
        public void ConvertFrom_BoundaryDoubleValues_ReturnsRoundRectangle(string input)
        {
            // Arrange
            double expectedRadius = double.Parse(input);

            // Act
            var result = _converter.ConvertFrom(_context, _culture, input);

            // Assert
            Assert.IsType<RoundRectangle>(result);
            var roundRect = (RoundRectangle)result;
            Assert.Equal(new CornerRadius(expectedRadius), roundRect.CornerRadius);
        }

        /// <summary>
        /// Helper class for testing ToString() conversion.
        /// </summary>
        private class CustomToStringObject
        {
            private readonly string _value;

            public CustomToStringObject(string value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new StrokeShapeTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new StrokeShapeTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// Verifies that only string type conversion is supported.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(long))]
        [InlineData(typeof(short))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(sbyte))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new StrokeShapeTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom behavior is not affected by the context parameter.
        /// Verifies that null context produces the same result as non-null context.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ContextIsNull_DoesNotAffectResult()
        {
            // Arrange
            var converter = new StrokeShapeTypeConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with array types.
        /// Verifies that even string array types return false (only string itself is supported).
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsStringArray_ReturnsFalse()
        {
            // Arrange
            var converter = new StrokeShapeTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string[]);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with generic types.
        /// Verifies that generic types containing string return false.
        /// </summary>
        [Theory]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        [InlineData(typeof(System.Collections.Generic.IEnumerable<string>))]
        [InlineData(typeof(System.Nullable<int>))]
        public void CanConvertFrom_SourceTypeIsGeneric_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new StrokeShapeTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}