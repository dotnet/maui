using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class PointCollectionConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when the destination type is string.
        /// Verifies the converter can convert PointCollection to string representation.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when the destination type is null.
        /// Verifies proper handling of null destination type parameter.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            Type? destinationType = null;

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Verifies the converter only supports conversion to string type.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Point))]
        [InlineData(typeof(Point[]))]
        [InlineData(typeof(PointCollectionConverter))]
        [InlineData(typeof(TypeConverter))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo behavior is consistent regardless of the context parameter.
        /// Verifies that the context parameter does not affect the conversion capability check.
        /// </summary>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        public void CanConvertTo_WithNonNullContext_BehaviorConsistent(Type destinationType, bool expected)
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly converts a Point array to PointCollection.
        /// </summary>
        [Fact]
        public void ConvertFrom_PointArray_ReturnsPointCollection()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointArray = new Point[] { new Point(10, 20), new Point(30, 40) };

            // Act
            var result = converter.ConvertFrom(null, null, pointArray);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Equal(2, pointCollection.Count);
            Assert.Equal(new Point(10, 20), pointCollection[0]);
            Assert.Equal(new Point(30, 40), pointCollection[1]);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly converts an empty Point array to empty PointCollection.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyPointArray_ReturnsEmptyPointCollection()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointArray = new Point[0];

            // Act
            var result = converter.ConvertFrom(null, null, pointArray);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Empty(pointCollection);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly converts null Point array to empty PointCollection.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullPointArray_ReturnsEmptyPointCollection()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            Point[] pointArray = null;

            // Act
            var result = converter.ConvertFrom(null, null, pointArray);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Empty(pointCollection);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses valid coordinate pairs from a string.
        /// </summary>
        [Theory]
        [InlineData("10,20 30,40", 2, 10.0, 20.0, 30.0, 40.0)]
        [InlineData("10 20 30 40", 2, 10.0, 20.0, 30.0, 40.0)]
        [InlineData("10,20,30,40", 2, 10.0, 20.0, 30.0, 40.0)]
        [InlineData("0,0", 1, 0.0, 0.0, 0.0, 0.0)]
        [InlineData("-10.5,20.5", 1, -10.5, 20.5, 0.0, 0.0)]
        public void ConvertFrom_ValidStringCoordinates_ReturnsPointCollection(string input, int expectedCount, double x1, double y1, double x2, double y2)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Equal(expectedCount, pointCollection.Count);
            Assert.Equal(new Point(x1, y1), pointCollection[0]);
            if (expectedCount > 1)
            {
                Assert.Equal(new Point(x2, y2), pointCollection[1]);
            }
        }

        /// <summary>
        /// Tests that ConvertFrom handles empty and whitespace-only strings correctly.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void ConvertFrom_EmptyOrWhitespaceString_ReturnsEmptyPointCollection(string input)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Empty(pointCollection);
        }

        /// <summary>
        /// Tests that ConvertFrom handles null input correctly.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullInput_ReturnsEmptyPointCollection()
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, null);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Empty(pointCollection);
        }

        /// <summary>
        /// Tests that ConvertFrom handles strings with extra whitespace correctly.
        /// </summary>
        [Fact]
        public void ConvertFrom_StringWithExtraWhitespace_ReturnsPointCollection()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var input = "  10,20   30,40  ";

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Equal(2, pointCollection.Count);
            Assert.Equal(new Point(10, 20), pointCollection[0]);
            Assert.Equal(new Point(30, 40), pointCollection[1]);
        }

        /// <summary>
        /// Tests that ConvertFrom handles extreme numeric values correctly.
        /// </summary>
        [Theory]
        [InlineData("1.7976931348623157E+308,1.7976931348623157E+308", double.MaxValue, double.MaxValue)]
        [InlineData("-1.7976931348623157E+308,-1.7976931348623157E+308", double.MinValue, double.MinValue)]
        [InlineData("4.94065645841247E-324,4.94065645841247E-324", double.Epsilon, double.Epsilon)]
        public void ConvertFrom_ExtremeNumericValues_ReturnsPointCollection(string input, double expectedX, double expectedY)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Single(pointCollection);
            Assert.Equal(new Point(expectedX, expectedY), pointCollection[0]);
        }

        /// <summary>
        /// Tests that ConvertFrom handles special double values correctly.
        /// </summary>
        [Theory]
        [InlineData("Infinity,NaN")]
        [InlineData("-Infinity,PositiveInfinity")]
        public void ConvertFrom_SpecialDoubleValues_ReturnsPointCollection(string input)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Single(pointCollection);
            // We don't assert exact values for special doubles as they may vary
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when encountering invalid number format.
        /// </summary>
        [Theory]
        [InlineData("abc,20")]
        [InlineData("10,xyz")]
        [InlineData("not_a_number")]
        [InlineData("10,20,invalid")]
        public void ConvertFrom_InvalidNumberFormat_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into", exception.Message);
            Assert.Contains(typeof(double).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when there's an odd number of coordinates.
        /// </summary>
        [Theory]
        [InlineData("10")]
        [InlineData("10,20,30")]
        [InlineData("10 20 30 40 50")]
        public void ConvertFrom_OddNumberOfCoordinates_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Equal("Cannot convert string into PointCollection", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly converts non-string objects by calling ToString().
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObject_CallsToStringAndConverts()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var customObject = new TestObject("10,20 30,40");

            // Act
            var result = converter.ConvertFrom(null, null, customObject);

            // Assert
            var pointCollection = Assert.IsType<PointCollection>(result);
            Assert.Equal(2, pointCollection.Count);
            Assert.Equal(new Point(10, 20), pointCollection[0]);
            Assert.Equal(new Point(30, 40), pointCollection[1]);
        }

        /// <summary>
        /// Helper class for testing ToString() conversion.
        /// </summary>
        private class TestObject
        {
            private readonly string _value;

            public TestObject(string value)
            {
                _value = value;
            }

            public override string ToString() => _value;
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// This tests the condition where value is not a PointCollection.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            object value = null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a PointCollection.
        /// This tests various non-PointCollection types including string, int, and object.
        /// Expected result: NotSupportedException is thrown for all non-PointCollection values.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_NonPointCollectionValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is a custom object.
        /// This ensures any object that is not PointCollection causes the expected exception.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_CustomObjectValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var value = new object();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns empty string when PointCollection is empty.
        /// This tests the string.Join operation with an empty collection.
        /// Expected result: Empty string is returned.
        /// </summary>
        [Fact]
        public void ConvertTo_EmptyPointCollection_ReturnsEmptyString()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection();
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, pointCollection, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string representation for single point.
        /// This tests the PointTypeConverter integration and string.Join with one element.
        /// Expected result: Single point formatted as "X, Y" string.
        /// </summary>
        [Fact]
        public void ConvertTo_SinglePoint_ReturnsFormattedString()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection { new Point(1.5, 2.7) };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, pointCollection, destinationType);

            // Assert
            Assert.Equal("1.5, 2.7", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string representation for multiple points.
        /// This tests the complete string.Join operation with multiple points.
        /// Expected result: Comma-separated string of all points.
        /// </summary>
        [Fact]
        public void ConvertTo_MultiplePoints_ReturnsCommaSeparatedString()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection
            {
                new Point(1, 2),
                new Point(3.5, 4.7),
                new Point(0, 0)
            };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, pointCollection, destinationType);

            // Assert
            Assert.Equal("1, 2, 3.5, 4.7, 0, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles edge case point values correctly.
        /// This tests special numeric values like negative, zero, and boundary values.
        /// Expected result: All special values are properly converted to strings.
        /// </summary>
        [Fact]
        public void ConvertTo_EdgeCasePointValues_ReturnsCorrectString()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection
            {
                new Point(-10.5, -20.7),
                new Point(0, 0),
                new Point(double.MaxValue, double.MinValue)
            };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, pointCollection, destinationType);

            // Assert
            Assert.Equal($"-10.5, -20.7, 0, 0, {double.MaxValue}, {double.MinValue}", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles special double values like NaN and Infinity.
        /// This tests the PointTypeConverter's handling of special floating-point values.
        /// Expected result: Special values are converted using invariant culture formatting.
        /// </summary>
        [Fact]
        public void ConvertTo_SpecialDoubleValues_ReturnsCorrectString()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection
            {
                new Point(double.NaN, double.PositiveInfinity),
                new Point(double.NegativeInfinity, 0)
            };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, pointCollection, destinationType);

            // Assert
            Assert.Equal("NaN, ∞, -∞, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of the context parameter value.
        /// This verifies that the context parameter doesn't affect the conversion logic.
        /// Expected result: Same conversion result regardless of context value.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentContexts_ReturnsConsistentResults()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection { new Point(1, 2) };
            var destinationType = typeof(string);

            // Act
            var resultWithNull = converter.ConvertTo(null, null, pointCollection, destinationType);
            var resultWithContext = converter.ConvertTo(NSubstitute.Substitute.For<ITypeDescriptorContext>(), null, pointCollection, destinationType);

            // Assert
            Assert.Equal("1, 2", resultWithNull);
            Assert.Equal("1, 2", resultWithContext);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of the culture parameter value.
        /// This verifies that the culture parameter doesn't affect the conversion logic.
        /// Expected result: Same conversion result regardless of culture value.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentCultures_ReturnsConsistentResults()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection { new Point(1.5, 2.7) };
            var destinationType = typeof(string);

            // Act
            var resultWithNull = converter.ConvertTo(null, null, pointCollection, destinationType);
            var resultWithCulture = converter.ConvertTo(null, new CultureInfo("de-DE"), pointCollection, destinationType);

            // Assert
            Assert.Equal("1.5, 2.7", resultWithNull);
            Assert.Equal("1.5, 2.7", resultWithCulture);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of the destination type parameter.
        /// This verifies that the destinationType parameter doesn't affect the conversion logic.
        /// Expected result: Same conversion result regardless of destination type.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_WithDifferentDestinationTypes_ReturnsString(Type destinationType)
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var pointCollection = new PointCollection { new Point(1, 2) };

            // Act
            var result = converter.ConvertTo(null, null, pointCollection, destinationType);

            // Assert
            Assert.Equal("1, 2", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringSourceType_ReturnsTrue()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is Point array.
        /// </summary>
        [Fact]
        public void CanConvertFrom_PointArraySourceType_ReturnsTrue()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var sourceType = typeof(Point[]);

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various unsupported source types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Point))]
        [InlineData(typeof(float[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(System.Collections.Generic.List<Point>))]
        public void CanConvertFrom_UnsupportedSourceTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new PointCollectionConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with a valid ITypeDescriptorContext.
        /// The context parameter should not affect the result for string source type.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithValidContext_StringSourceType_ReturnsTrue()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with a valid ITypeDescriptorContext.
        /// The context parameter should not affect the result for Point array source type.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithValidContext_PointArraySourceType_ReturnsTrue()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(Point[]);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with a valid ITypeDescriptorContext.
        /// The context parameter should not affect the result for unsupported source types.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithValidContext_UnsupportedSourceType_ReturnsFalse()
        {
            // Arrange
            var converter = new PointCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(int);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}