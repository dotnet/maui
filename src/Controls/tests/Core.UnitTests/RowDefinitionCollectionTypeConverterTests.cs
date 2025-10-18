using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RowDefinitionCollectionTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is string type.
        /// Verifies the core conversion capability to string.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertTo(context, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null.
        /// Verifies proper handling of null destination type.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertTo(context, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Verifies that only string type conversion is supported.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion capability for.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(RowDefinitionCollection))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo behavior is consistent regardless of context parameter value.
        /// Verifies that the context parameter does not affect the conversion capability determination.
        /// </summary>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        public void CanConvertTo_WithNullContext_BehaviorConsistent(Type destinationType, bool expected)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            bool result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom with null value throws InvalidOperationException.
        /// Input: null value
        /// Expected: InvalidOperationException with appropriate message
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, null));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(RowDefinitionCollection).Name, exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom with object that has null ToString() throws InvalidOperationException.
        /// Input: object with null ToString() return value
        /// Expected: InvalidOperationException with appropriate message
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithNullToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var objectWithNullToString = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, objectWithNullToString));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(RowDefinitionCollection).Name, exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom with empty string returns empty RowDefinitionCollection.
        /// Input: empty string
        /// Expected: Empty RowDefinitionCollection
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsEmptyCollection()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, "");

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom with valid single GridLength string creates collection with one definition.
        /// Input: various single valid GridLength strings
        /// Expected: RowDefinitionCollection with one RowDefinition
        /// </summary>
        [Theory]
        [InlineData("100")]
        [InlineData("auto")]
        [InlineData("Auto")]
        [InlineData("AUTO")]
        [InlineData("*")]
        [InlineData("2*")]
        [InlineData("0.5*")]
        [InlineData("50.25")]
        public void ConvertFrom_SingleValidGridLength_ReturnsCollectionWithOneDefinition(string gridLengthString)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, gridLengthString);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.Single(collection);
        }

        /// <summary>
        /// Tests that ConvertFrom with multiple valid GridLength strings creates collection with multiple definitions.
        /// Input: comma-separated valid GridLength strings
        /// Expected: RowDefinitionCollection with corresponding number of RowDefinitions
        /// </summary>
        [Theory]
        [InlineData("100,200", 2)]
        [InlineData("auto,*,100", 3)]
        [InlineData("50,2*,auto,150", 4)]
        [InlineData("*,*,*", 3)]
        [InlineData("100,auto", 2)]
        public void ConvertFrom_MultipleValidGridLengths_ReturnsCollectionWithMultipleDefinitions(string gridLengthString, int expectedCount)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, gridLengthString);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.Equal(expectedCount, collection.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom with invalid GridLength format throws FormatException.
        /// Input: string with invalid GridLength format
        /// Expected: FormatException from GridLengthTypeConverter.ParseStringToGridLength
        /// </summary>
        [Theory]
        [InlineData("invalid")]
        [InlineData("100px")]
        [InlineData("abc")]
        [InlineData("**")]
        [InlineData("auto*")]
        [InlineData("-*")]
        public void ConvertFrom_InvalidGridLengthFormat_ThrowsFormatException(string invalidGridLength)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act & Assert
            Assert.Throws<FormatException>(() =>
                converter.ConvertFrom(null, null, invalidGridLength));
        }

        /// <summary>
        /// Tests that ConvertFrom with mixed valid and invalid GridLength formats throws FormatException.
        /// Input: comma-separated string with at least one invalid GridLength
        /// Expected: FormatException from GridLengthTypeConverter.ParseStringToGridLength
        /// </summary>
        [Theory]
        [InlineData("100,invalid")]
        [InlineData("auto,abc,*")]
        [InlineData("*,100px")]
        public void ConvertFrom_MixedValidInvalidGridLengths_ThrowsFormatException(string mixedGridLengths)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act & Assert
            Assert.Throws<FormatException>(() =>
                converter.ConvertFrom(null, null, mixedGridLengths));
        }

        /// <summary>
        /// Tests that ConvertFrom with non-string object that has valid ToString() works correctly.
        /// Input: non-string object with valid ToString() method
        /// Expected: RowDefinitionCollection created from ToString() result
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObjectWithValidToString_ReturnsCollection()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var objectWithToString = new ObjectWithToString("100,auto");

            // Act
            var result = converter.ConvertFrom(null, null, objectWithToString);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.Equal(2, collection.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom handles whitespace in GridLength strings correctly.
        /// Input: GridLength strings with various whitespace patterns
        /// Expected: RowDefinitionCollection created successfully (whitespace handled by GridLengthTypeConverter)
        /// </summary>
        [Theory]
        [InlineData(" 100 ")]
        [InlineData("100 , auto")]
        [InlineData(" * , 200 , auto ")]
        public void ConvertFrom_GridLengthsWithWhitespace_ReturnsCollection(string gridLengthWithWhitespace)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, gridLengthWithWhitespace);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.True(collection.Count > 0);
        }

        /// <summary>
        /// Tests that ConvertFrom with context and culture parameters works correctly.
        /// Input: valid GridLength string with non-null context and culture
        /// Expected: RowDefinitionCollection created successfully (parameters are ignored in implementation)
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_ReturnsCollection()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var culture = CultureInfo.InvariantCulture;
            // Note: Cannot easily mock ITypeDescriptorContext, but the implementation ignores it anyway

            // Act
            var result = converter.ConvertFrom(null, culture, "100,*");

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.Equal(2, collection.Count);
        }

        /// <summary>
        /// Tests that ConvertFrom with extreme numeric values works correctly.
        /// Input: GridLength strings with extreme numeric values
        /// Expected: RowDefinitionCollection created successfully or FormatException for invalid values
        /// </summary>
        [Theory]
        [InlineData("0")]
        [InlineData("0.0")]
        [InlineData("1.7976931348623157E+308")] // double.MaxValue
        [InlineData("4.9406564584124654E-324")] // double.Epsilon
        public void ConvertFrom_ExtremeNumericValues_HandlesCorrectly(string extremeValue)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, extremeValue);

            // Assert
            Assert.NotNull(result);
            var collection = Assert.IsType<RowDefinitionCollection>(result);
            Assert.Single(collection);
        }

        // Helper classes for testing
        private class ObjectWithNullToString
        {
            public override string ToString() => null;
        }

        private class ObjectWithToString
        {
            private readonly string _value;

            public ObjectWithToString(string value)
            {
                _value = value;
            }

            public override string ToString() => _value;
        }
        private readonly RowDefinitionCollectionTypeConverter _converter;

        public RowDefinitionCollectionTypeConverterTests()
        {
            _converter = new RowDefinitionCollectionTypeConverter();
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
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
        /// Tests that ConvertTo throws NotSupportedException when value is not a RowDefinitionCollection.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(new object[] { })]
        public void ConvertTo_InvalidValueType_ThrowsNotSupportedException(object value)
        {
            // Arrange
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns empty string for an empty RowDefinitionCollection.
        /// </summary>
        [Fact]
        public void ConvertTo_EmptyCollection_ReturnsEmptyString()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string representation for a single RowDefinition with Auto height.
        /// </summary>
        [Fact]
        public void ConvertTo_SingleRowDefinitionWithAutoHeight_ReturnsAutoString()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(GridLength.Auto));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("auto", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string representation for a single RowDefinition with Star height.
        /// </summary>
        [Fact]
        public void ConvertTo_SingleRowDefinitionWithStarHeight_ReturnsStarString()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(GridLength.Star));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("1*", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string representation for a single RowDefinition with absolute height.
        /// </summary>
        [Fact]
        public void ConvertTo_SingleRowDefinitionWithAbsoluteHeight_ReturnsAbsoluteString()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(new GridLength(100)));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("100", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct comma-separated string for multiple RowDefinitions with various heights.
        /// </summary>
        [Fact]
        public void ConvertTo_MultipleRowDefinitions_ReturnsCommaSeparatedString()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(GridLength.Auto));
            definitions.Add(new RowDefinition(new GridLength(50)));
            definitions.Add(new RowDefinition(new GridLength(2, GridUnitType.Star)));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("auto, 50, 2*", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles multiple RowDefinitions with same height types correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_MultipleRowDefinitionsSameType_ReturnsCommaSeparatedString()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(new GridLength(100)));
            definitions.Add(new RowDefinition(new GridLength(200)));
            definitions.Add(new RowDefinition(new GridLength(300)));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("100, 200, 300", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with null context parameter.
        /// </summary>
        [Fact]
        public void ConvertTo_NullContext_WorksCorrectly()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(GridLength.Auto));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("auto", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with null culture parameter.
        /// </summary>
        [Fact]
        public void ConvertTo_NullCulture_WorksCorrectly()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(GridLength.Auto));
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("auto", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with custom culture parameter.
        /// </summary>
        [Fact]
        public void ConvertTo_CustomCulture_WorksCorrectly()
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(new GridLength(100.5)));
            var culture = new CultureInfo("de-DE");
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, culture, definitions, destinationType);

            // Assert
            // The result should still use InvariantCulture internally
            Assert.Equal("100.5", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of destinationType parameter value.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_DifferentDestinationTypes_WorksCorrectly(Type destinationType)
        {
            // Arrange
            var definitions = new RowDefinitionCollection();
            definitions.Add(new RowDefinition(GridLength.Auto));

            // Act
            var result = _converter.ConvertTo(null, null, definitions, destinationType);

            // Assert
            Assert.Equal("auto", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string type.
        /// Input: sourceType = typeof(string), context = null
        /// Expected: true
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string type with non-null context.
        /// Input: sourceType = typeof(string), context = mock context
        /// Expected: true
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringTypeWithContext_ReturnsTrue()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// Input: various non-string types
        /// Expected: false for all non-string types
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(long))]
        [InlineData(typeof(float))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for complex types including arrays, generics, and interfaces.
        /// Input: various complex non-string types
        /// Expected: false for all complex non-string types
        /// </summary>
        [Theory]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, int>))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(RowDefinitionCollectionTypeConverter))]
        public void CanConvertFrom_ComplexNonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for non-string types with non-null context.
        /// Input: non-string type with mock context
        /// Expected: false
        /// </summary>
        [Fact]
        public void CanConvertFrom_NonStringTypeWithContext_ReturnsFalse()
        {
            // Arrange
            var converter = new RowDefinitionCollectionTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(int);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}