using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DoubleCollectionConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true only when the destination type is string.
        /// Verifies the method correctly identifies string as the only supported conversion target.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion support for</param>
        /// <param name="expected">The expected result of the conversion support check</param>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(double), false)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(Guid), false)]
        [InlineData(typeof(decimal), false)]
        [InlineData(typeof(float), false)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(char), false)]
        public void CanConvertTo_WithVariousDestinationTypes_ReturnsExpectedResult(Type destinationType, bool expected)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when the destination type is null.
        /// Verifies the method handles null destination type gracefully.
        /// </summary>
        [Fact]
        public void CanConvertTo_WithNullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns the same result regardless of the context parameter value.
        /// Verifies that the context parameter does not affect the conversion support determination.
        /// </summary>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        public void CanConvertTo_WithNullContext_ReturnsExpectedResult(Type destinationType, bool expected)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertTo correctly handles array and generic types.
        /// Verifies that only string type returns true, not string arrays or generic string types.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion support for</param>
        /// <param name="expected">The expected result of the conversion support check</param>
        [Theory]
        [InlineData(typeof(string[]), false)]
        [InlineData(typeof(System.Collections.Generic.List<string>), false)]
        [InlineData(typeof(System.Collections.Generic.IEnumerable<string>), false)]
        public void CanConvertTo_WithComplexStringTypes_ReturnsFalse(Type destinationType, bool expected)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }
        private readonly DoubleCollectionConverter _converter;

        public DoubleCollectionConverterTests()
        {
            _converter = new DoubleCollectionConverter();
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a double array to DoubleCollection.
        /// Verifies the double array conversion path and implicit operator usage.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithDoubleArray_ReturnsDoubleCollection()
        {
            // Arrange
            var doubleArray = new double[] { 1.0, 2.5, 3.14159 };

            // Act
            var result = _converter.ConvertFrom(null, null, doubleArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Equal(3, collection.Count);
            Assert.Equal(1.0, collection[0]);
            Assert.Equal(2.5, collection[1]);
            Assert.Equal(3.14159, collection[2]);
        }

        /// <summary>
        /// Tests that ConvertFrom handles null double array by creating empty DoubleCollection.
        /// Verifies the null double array handling in implicit operator.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithNullDoubleArray_ReturnsEmptyDoubleCollection()
        {
            // Arrange
            double[] doubleArray = null;

            // Act
            var result = _converter.ConvertFrom(null, null, doubleArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom handles empty double array correctly.
        /// Verifies empty array conversion behavior.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithEmptyDoubleArray_ReturnsEmptyDoubleCollection()
        {
            // Arrange
            var doubleArray = new double[] { };

            // Act
            var result = _converter.ConvertFrom(null, null, doubleArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom handles double arrays with special values (NaN, Infinity).
        /// Verifies special double value handling in array conversion.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithDoubleArrayContainingSpecialValues_ReturnsDoubleCollection()
        {
            // Arrange
            var doubleArray = new double[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue, double.MinValue };

            // Act
            var result = _converter.ConvertFrom(null, null, doubleArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Equal(5, collection.Count);
            Assert.True(double.IsNaN(collection[0]));
            Assert.True(double.IsPositiveInfinity(collection[1]));
            Assert.True(double.IsNegativeInfinity(collection[2]));
            Assert.Equal(double.MaxValue, collection[3]);
            Assert.Equal(double.MinValue, collection[4]);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a float array to DoubleCollection.
        /// Verifies the float array conversion path and implicit operator usage.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithFloatArray_ReturnsDoubleCollection()
        {
            // Arrange
            var floatArray = new float[] { 1.0f, 2.5f, 3.14f };

            // Act
            var result = _converter.ConvertFrom(null, null, floatArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Equal(3, collection.Count);
            Assert.Equal(1.0, collection[0]);
            Assert.Equal(2.5, collection[1]);
            Assert.Equal(3.14000010490417, collection[2], 10); // Float precision
        }

        /// <summary>
        /// Tests that ConvertFrom handles null float array by creating empty DoubleCollection.
        /// Verifies the null float array handling in implicit operator.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithNullFloatArray_ReturnsEmptyDoubleCollection()
        {
            // Arrange
            float[] floatArray = null;

            // Act
            var result = _converter.ConvertFrom(null, null, floatArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom handles empty float array correctly.
        /// Verifies empty float array conversion behavior.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithEmptyFloatArray_ReturnsEmptyDoubleCollection()
        {
            // Arrange
            var floatArray = new float[] { };

            // Act
            var result = _converter.ConvertFrom(null, null, floatArray);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when object's ToString returns null.
        /// Verifies the null string value exception path.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithObjectReturningNullFromToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var objectWithNullToString = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(null, null, objectWithNullToString));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into Microsoft.Maui.Controls.DoubleCollection", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when string contains invalid double values.
        /// Verifies the invalid double parsing exception path.
        /// </summary>
        [Theory]
        [InlineData("invalid")]
        [InlineData("1.0,invalid,3.0")]
        [InlineData("abc def")]
        [InlineData("1.0 abc 3.0")]
        public void ConvertFrom_WithInvalidDoubleInString_ThrowsInvalidOperationException(string invalidValue)
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(null, null, invalidValue));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into System.Double", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully parses valid string with comma and space separators.
        /// Verifies successful string parsing with multiple separators.
        /// </summary>
        [Theory]
        [InlineData("1.0,2.0,3.0", new double[] { 1.0, 2.0, 3.0 })]
        [InlineData("1.0 2.0 3.0", new double[] { 1.0, 2.0, 3.0 })]
        [InlineData("1.0, 2.0, 3.0", new double[] { 1.0, 2.0, 3.0 })]
        [InlineData("1.0 , 2.0 , 3.0", new double[] { 1.0, 2.0, 3.0 })]
        [InlineData("1.5", new double[] { 1.5 })]
        [InlineData("", new double[] { })]
        [InlineData("   ", new double[] { })]
        [InlineData(",,,", new double[] { })]
        public void ConvertFrom_WithValidStringValues_ReturnsDoubleCollection(string input, double[] expectedValues)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Equal(expectedValues.Length, collection.Count);

            for (int i = 0; i < expectedValues.Length; i++)
            {
                Assert.Equal(expectedValues[i], collection[i]);
            }
        }

        /// <summary>
        /// Tests that ConvertFrom handles special double values in string format.
        /// Verifies parsing of special double values from strings.
        /// </summary>
        [Theory]
        [InlineData("NaN", double.NaN)]
        [InlineData("Infinity", double.PositiveInfinity)]
        [InlineData("-Infinity", double.NegativeInfinity)]
        public void ConvertFrom_WithSpecialDoubleStringValues_ReturnsDoubleCollection(string input, double expectedValue)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Single(collection);

            if (double.IsNaN(expectedValue))
                Assert.True(double.IsNaN(collection[0]));
            else
                Assert.Equal(expectedValue, collection[0]);
        }

        /// <summary>
        /// Tests that ConvertFrom handles extreme double values.
        /// Verifies parsing of boundary double values from strings.
        /// </summary>
        [Theory]
        [InlineData("1.7976931348623157E+308", double.MaxValue)]
        [InlineData("-1.7976931348623157E+308", double.MinValue)]
        [InlineData("0", 0.0)]
        [InlineData("-0", 0.0)]
        public void ConvertFrom_WithExtremeDoubleValues_ReturnsDoubleCollection(string input, double expectedValue)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Single(collection);
            Assert.Equal(expectedValue, collection[0]);
        }

        /// <summary>
        /// Tests that ConvertFrom handles various object types by using their ToString method.
        /// Verifies conversion of different object types through string representation.
        /// </summary>
        [Theory]
        [InlineData(42, new double[] { 42.0 })]
        [InlineData(3.14, new double[] { 3.14 })]
        [InlineData(true, new double[] { })] // "True" is not a valid double, should result in empty collection due to exception
        public void ConvertFrom_WithDifferentObjectTypes_ParsesStringRepresentation(object input, double[] expectedValues)
        {
            if (input.Equals(true)) // Special case for boolean which will throw
            {
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() =>
                    _converter.ConvertFrom(null, null, input));
            }
            else
            {
                // Act
                var result = _converter.ConvertFrom(null, null, input);

                // Assert
                Assert.IsType<DoubleCollection>(result);
                var collection = (DoubleCollection)result;
                Assert.Equal(expectedValues.Length, collection.Count);

                for (int i = 0; i < expectedValues.Length; i++)
                {
                    Assert.Equal(expectedValues[i], collection[i]);
                }
            }
        }

        /// <summary>
        /// Tests that ConvertFrom ignores context and culture parameters and uses InvariantCulture.
        /// Verifies that context and culture parameters don't affect the conversion behavior.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_IgnoresParameters()
        {
            // Arrange
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("de-DE"); // Uses comma as decimal separator
            var input = "1.5,2.5"; // Should be parsed as two values, not one German decimal

            // Act
            var result = _converter.ConvertFrom(context, culture, input);

            // Assert
            Assert.IsType<DoubleCollection>(result);
            var collection = (DoubleCollection)result;
            Assert.Equal(2, collection.Count);
            Assert.Equal(1.5, collection[0]);
            Assert.Equal(2.5, collection[1]);
        }

        /// <summary>
        /// Helper class that returns null from ToString() method.
        /// Used to test the null string handling path.
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString()
            {
                return null;
            }
        }

        /// <summary>
        /// Tests that ConvertTo returns a comma-separated string when given a valid DoubleCollection with multiple values.
        /// Input: Valid DoubleCollection with multiple double values.
        /// Expected: Comma-separated string representation of the values.
        /// </summary>
        [Fact]
        public void ConvertTo_DoubleCollectionWithMultipleValues_ReturnsCommaSeparatedString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([1.0, 2.5, -3.14]);
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal("1, 2.5, -3.14", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns an empty string when given an empty DoubleCollection.
        /// Input: Empty DoubleCollection.
        /// Expected: Empty string.
        /// </summary>
        [Fact]
        public void ConvertTo_EmptyDoubleCollection_ReturnsEmptyString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns a single value string when given a DoubleCollection with one value.
        /// Input: DoubleCollection with a single double value.
        /// Expected: String representation of the single value.
        /// </summary>
        [Fact]
        public void ConvertTo_SingleValueDoubleCollection_ReturnsSingleValueString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([42.0]);
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal("42", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles special double values correctly.
        /// Input: DoubleCollection containing NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected: String representation of special values separated by commas.
        /// </summary>
        [Fact]
        public void ConvertTo_SpecialDoubleValues_ReturnsSpecialValuesString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([double.NaN, double.PositiveInfinity, double.NegativeInfinity]);
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal("NaN, Infinity, -Infinity", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given a null value.
        /// Input: Null value.
        /// Expected: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, null, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given a non-DoubleCollection object.
        /// Input: Various non-DoubleCollection values including string, int, and other collection types.
        /// Expected: NotSupportedException is thrown for all non-DoubleCollection inputs.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_NonDoubleCollectionValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with null context parameter.
        /// Input: Valid DoubleCollection with null context.
        /// Expected: Comma-separated string representation regardless of null context.
        /// </summary>
        [Fact]
        public void ConvertTo_NullContext_ReturnsCommaSeparatedString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([1.0, 2.0]);
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal("1, 2", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with null culture parameter.
        /// Input: Valid DoubleCollection with null culture.
        /// Expected: Comma-separated string representation regardless of null culture.
        /// </summary>
        [Fact]
        public void ConvertTo_NullCulture_ReturnsCommaSeparatedString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([1.0, 2.0]);
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, null, doubleCollection, destinationType);

            // Assert
            Assert.Equal("1, 2", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles boundary double values correctly.
        /// Input: DoubleCollection containing MaxValue, MinValue, and zero.
        /// Expected: String representation of boundary values separated by commas.
        /// </summary>
        [Fact]
        public void ConvertTo_BoundaryDoubleValues_ReturnsBoundaryValuesString()
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([double.MaxValue, double.MinValue, 0.0]);
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal($"{double.MaxValue}, {double.MinValue}, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo works with different destination types.
        /// Input: Valid DoubleCollection with various destination types.
        /// Expected: Comma-separated string representation regardless of destination type.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_DifferentDestinationTypes_ReturnsCommaSeparatedString(Type destinationType)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var doubleCollection = new DoubleCollection([1.0, 2.0]);
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.ConvertTo(context, culture, doubleCollection, destinationType);

            // Assert
            Assert.Equal("1, 2", result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(double[]))]
        [InlineData(typeof(float[]))]
        public void CanConvertFrom_SupportedTypes_ReturnsTrue(Type sourceType)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            ITypeDescriptorContext context = null;

            // Act
            bool result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<double>))]
        [InlineData(typeof(IEnumerable<double>))]
        public void CanConvertFrom_UnsupportedTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            ITypeDescriptorContext context = null;

            // Act
            bool result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(double[]))]
        [InlineData(typeof(float[]))]
        public void CanConvertFrom_SupportedTypesWithMockContext_ReturnsTrue(Type sourceType)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        public void CanConvertFrom_UnsupportedTypesWithMockContext_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new DoubleCollectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}