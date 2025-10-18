using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Converters;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ColumnDefinitionCollectionTypeConverterUnitTests : BaseTestFixture
    {
        private readonly ColumnDefinitionCollectionTypeConverter _converter = new();

        [Fact]
        public void ConvertNullTest()
        {
            Assert.Throws<InvalidOperationException>(() => _converter.ConvertFromInvariantString(null));
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destination type is string.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            Type destinationType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destination type is string with non-null context.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationTypeWithContext_ReturnsTrue()
        {
            // Arrange
            Type destinationType = typeof(string);
            ITypeDescriptorContext context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destination type is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            Type destinationType = null;
            ITypeDescriptorContext context = null;

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Verifies the method correctly identifies only string as a valid conversion target.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion capability for.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(long))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(Array))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(ColumnDefinitionCollection))]
        [InlineData(typeof(ColumnDefinition))]
        [InlineData(typeof(GridLength))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(CultureInfo))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            ITypeDescriptorContext context = null;

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types with non-null context.
        /// Verifies that the context parameter does not affect the conversion capability determination.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion capability for.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(ColumnDefinitionCollection))]
        public void CanConvertTo_NonStringDestinationTypeWithContext_ReturnsFalse(Type destinationType)
        {
            // Arrange
            ITypeDescriptorContext context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }
    }



    public partial class ColumnDefinitionCollectionTypeConverterTests : BaseTestFixture
    {
        private readonly ColumnDefinitionCollectionTypeConverter _converter = new();

        /// <summary>
        /// Tests that ConvertFrom returns an empty ColumnDefinitionCollection when given an empty string.
        /// This test covers the fast path for empty string input.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsEmptyCollection()
        {
            // Arrange
            string emptyString = "";

            // Act
            var result = _converter.ConvertFrom(null, null, emptyString);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<ColumnDefinitionCollection>(result);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses single valid column definition values.
        /// This test covers various valid GridLength formats including absolute, auto, and star values.
        /// </summary>
        [Theory]
        [InlineData("100", GridUnitType.Absolute, 100.0)]
        [InlineData("auto", GridUnitType.Auto, 1.0)]
        [InlineData("*", GridUnitType.Star, 1.0)]
        [InlineData("2*", GridUnitType.Star, 2.0)]
        [InlineData("0.5*", GridUnitType.Star, 0.5)]
        [InlineData("50.5", GridUnitType.Absolute, 50.5)]
        public void ConvertFrom_ValidSingleValue_ReturnsCorrectCollection(string input, GridUnitType expectedType, double expectedValue)
        {
            // Arrange & Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<ColumnDefinitionCollection>(result);
            Assert.Single(collection);
            Assert.Equal(expectedType, collection[0].Width.GridUnitType);
            Assert.Equal(expectedValue, collection[0].Width.Value, 10);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses multiple comma-separated column definition values.
        /// This test covers the parsing loop and collection creation with multiple definitions.
        /// </summary>
        [Theory]
        [InlineData("100,auto", 2)]
        [InlineData("*,2*,auto,100", 4)]
        [InlineData("50,auto,*", 3)]
        [InlineData("100", 1)]
        [InlineData("auto,auto,auto", 3)]
        public void ConvertFrom_ValidMultipleValues_ReturnsCorrectCollectionCount(string input, int expectedCount)
        {
            // Arrange & Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<ColumnDefinitionCollection>(result);
            Assert.Equal(expectedCount, collection.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses a complex multi-value string with various definition types.
        /// This test verifies the complete parsing pipeline for mixed column definitions.
        /// </summary>
        [Fact]
        public void ConvertFrom_MixedValidValues_ReturnsCorrectDefinitions()
        {
            // Arrange
            string input = "100,auto,*,2*";

            // Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<ColumnDefinitionCollection>(result);
            Assert.Equal(4, collection.Count);

            Assert.Equal(GridUnitType.Absolute, collection[0].Width.GridUnitType);
            Assert.Equal(100.0, collection[0].Width.Value, 10);

            Assert.Equal(GridUnitType.Auto, collection[1].Width.GridUnitType);

            Assert.Equal(GridUnitType.Star, collection[2].Width.GridUnitType);
            Assert.Equal(1.0, collection[2].Width.Value, 10);

            Assert.Equal(GridUnitType.Star, collection[3].Width.GridUnitType);
            Assert.Equal(2.0, collection[3].Width.Value, 10);
        }

        /// <summary>
        /// Tests that ConvertFrom throws FormatException when given invalid column definition values.
        /// This test covers error handling for malformed input that cannot be parsed.
        /// </summary>
        [Theory]
        [InlineData("invalid")]
        [InlineData("100x")]
        [InlineData("auto*")]
        [InlineData("**")]
        [InlineData("100,invalid")]
        [InlineData("auto,100x,*")]
        public void ConvertFrom_InvalidValues_ThrowsFormatException(string input)
        {
            // Arrange & Act & Assert
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, input));
        }

        /// <summary>
        /// Tests that ConvertFrom works with non-string objects that have a valid ToString method.
        /// This test covers the ToString conversion path for objects other than strings.
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObjectWithValidToString_ReturnsCorrectCollection()
        {
            // Arrange
            var stringBuilder = new StringBuilder("100,auto");

            // Act
            var result = _converter.ConvertFrom(null, null, stringBuilder);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<ColumnDefinitionCollection>(result);
            Assert.Equal(2, collection.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when given an object whose ToString returns null.
        /// This test covers the error handling for objects that cannot be converted to a valid string representation.
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithNullToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var objectWithNullToString = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _converter.ConvertFrom(null, null, objectWithNullToString));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(ColumnDefinitionCollection).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles whitespace-only strings by attempting to parse them.
        /// This test covers edge cases with whitespace input.
        /// </summary>
        [Fact]
        public void ConvertFrom_WhitespaceString_ThrowsFormatException()
        {
            // Arrange
            string whitespaceString = "   ";

            // Act & Assert
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, whitespaceString));
        }

        /// <summary>
        /// Tests that ConvertFrom works with culture and context parameters.
        /// This test verifies that the method accepts non-null context and culture parameters.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_ReturnsCorrectCollection()
        {
            // Arrange
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            string input = "100";

            // Act
            var result = _converter.ConvertFrom(context, culture, input);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<ColumnDefinitionCollection>(result);
            Assert.Single(collection);
        }

        private class ObjectWithNullToString
        {
        }
        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// Input: null value
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            object value = null;
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a ColumnDefinitionCollection.
        /// Input conditions: Various non-ColumnDefinitionCollection values
        /// Expected: NotSupportedException for each case
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_NonColumnDefinitionCollectionValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns empty string when ColumnDefinitionCollection is empty.
        /// Input: Empty ColumnDefinitionCollection
        /// Expected: Empty string
        /// </summary>
        [Fact]
        public void ConvertTo_EmptyCollection_ReturnsEmptyString()
        {
            // Arrange
            var emptyCollection = new ColumnDefinitionCollection();
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, emptyCollection, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns converted Width string for single item collection.
        /// Input conditions: ColumnDefinitionCollection with single ColumnDefinition of different GridLength types
        /// Expected: Converted Width string using GridLengthTypeConverter
        /// </summary>
        [Theory]
        [InlineData(GridUnitType.Auto, 0, "auto")]
        [InlineData(GridUnitType.Star, 1, "1*")]
        [InlineData(GridUnitType.Star, 2.5, "2.5*")]
        [InlineData(GridUnitType.Absolute, 100, "100")]
        [InlineData(GridUnitType.Absolute, 50.75, "50.75")]
        public void ConvertTo_SingleItemCollection_ReturnsConvertedWidthString(GridUnitType gridUnitType, double value, string expected)
        {
            // Arrange
            var gridLength = new GridLength(value, gridUnitType);
            var collection = new ColumnDefinitionCollection(new ColumnDefinition(gridLength));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, collection, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns comma-separated Width strings for multiple item collection.
        /// Input: ColumnDefinitionCollection with multiple ColumnDefinitions
        /// Expected: Comma-separated Width strings
        /// </summary>
        [Fact]
        public void ConvertTo_MultipleItemCollection_ReturnsCommaSeparatedWidthStrings()
        {
            // Arrange
            var collection = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(100, GridUnitType.Absolute))
            );
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, collection, destinationType);

            // Assert
            Assert.Equal("auto, 2*, 100", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles collections with many items efficiently using ArrayPool.
        /// Input: Large ColumnDefinitionCollection
        /// Expected: Comma-separated Width strings for all items
        /// </summary>
        [Fact]
        public void ConvertTo_LargeCollection_ReturnsCommaSeparatedWidthStrings()
        {
            // Arrange
            var definitions = new List<ColumnDefinition>();
            var expectedParts = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                definitions.Add(new ColumnDefinition(new GridLength(i + 1, GridUnitType.Star)));
                expectedParts.Add($"{i + 1}*");
            }

            var collection = new ColumnDefinitionCollection(definitions.ToArray());
            Type destinationType = typeof(string);
            var expectedResult = string.Join(", ", expectedParts);

            // Act
            var result = _converter.ConvertTo(null, null, collection, destinationType);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that ConvertTo works with different context and culture parameters.
        /// Input conditions: Various context and culture values (though not used in implementation)
        /// Expected: Correct conversion regardless of context and culture values
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentContextAndCulture_IgnoresParametersAndConvertsCorrectly()
        {
            // Arrange
            var collection = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Auto));
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("fr-FR");
            Type destinationType = typeof(string);

            // Act
            var result1 = _converter.ConvertTo(context, culture, collection, destinationType);
            var result2 = _converter.ConvertTo(null, null, collection, destinationType);

            // Assert
            Assert.Equal("auto", result1);
            Assert.Equal("auto", result2);
            Assert.Equal(result1, result2);
        }

        /// <summary>
        /// Tests that ConvertTo handles mixed GridLength types in multiple item collection.
        /// Input: ColumnDefinitionCollection with mixed Auto, Star, and Absolute GridLengths
        /// Expected: Comma-separated Width strings with correct formatting for each type
        /// </summary>
        [Fact]
        public void ConvertTo_MixedGridLengthTypes_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var collection = new ColumnDefinitionCollection(
                new ColumnDefinition(new GridLength(150, GridUnitType.Absolute)),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(0.5, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(25.25, GridUnitType.Absolute))
            );
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, collection, destinationType);

            // Assert
            Assert.Equal("150, auto, 0.5*, 3*, 25.25", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles edge case GridLength values correctly.
        /// Input conditions: GridLength with boundary values like 0, negative (if valid), very large numbers
        /// Expected: Correct string representation for boundary values
        /// </summary>
        [Theory]
        [InlineData(0, GridUnitType.Absolute, "0")]
        [InlineData(0, GridUnitType.Star, "0*")]
        [InlineData(double.MaxValue, GridUnitType.Absolute, "1.7976931348623157E+308")]
        [InlineData(1e-10, GridUnitType.Star, "1E-10*")]
        public void ConvertTo_EdgeCaseGridLengthValues_ReturnsCorrectString(double value, GridUnitType gridUnitType, string expected)
        {
            // Arrange
            var gridLength = new GridLength(value, gridUnitType);
            var collection = new ColumnDefinitionCollection(new ColumnDefinition(gridLength));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, collection, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when the source type is string.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var sourceType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// Input conditions: Different non-string types with null context.
        /// Expected result: False for all non-string types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            ITypeDescriptorContext context = null;

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for complex types like collections and arrays.
        /// Input conditions: Collection and array types with null context.
        /// Expected result: False for all complex types.
        /// </summary>
        [Theory]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, object>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(IEnumerable))]
        public void CanConvertFrom_ComplexTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            ITypeDescriptorContext context = null;

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context is provided (not null).
        /// Input conditions: String type with a mocked context.
        /// Expected result: True, context should not affect the result.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithContext_StringType_ReturnsTrue()
        {
            // Arrange
            var sourceType = typeof(string);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context is provided (not null) with non-string type.
        /// Input conditions: Non-string type with a mocked context.
        /// Expected result: False, context should not affect the result.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithContext_NonStringType_ReturnsFalse()
        {
            // Arrange
            var sourceType = typeof(int);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws NullReferenceException when sourceType is null.
        /// Input conditions: Null sourceType parameter.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ThrowsNullReferenceException()
        {
            // Arrange
            Type sourceType = null;
            ITypeDescriptorContext context = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _converter.CanConvertFrom(context, sourceType));
        }
    }
}
