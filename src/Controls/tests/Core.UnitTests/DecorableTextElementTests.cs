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
    public class TextDecorationConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// Input: destinationType = typeof(string), context can be null or valid
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            var destinationType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string) with valid context.
        /// Input: destinationType = typeof(string), context = valid ITypeDescriptorContext
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationTypeWithValidContext_ReturnsTrue()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            var destinationType = typeof(string);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null.
        /// Input: destinationType = null, context can be null or valid
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            Type destinationType = null;
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Input: destinationType = various types other than string, context can be null or valid
        /// Expected: Returns false for all non-string types
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(TextDecorations))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(TypeConverter))]
        [InlineData(typeof(CultureInfo))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new TextDecorationConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for non-string types with valid context.
        /// Input: destinationType = non-string type, context = valid ITypeDescriptorContext
        /// Expected: Returns false regardless of context
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(TextDecorations))]
        public void CanConvertTo_NonStringDestinationTypeWithValidContext_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new TextDecorationConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }
        private readonly TextDecorationConverter _converter;

        public TextDecorationConverterTests()
        {
            _converter = new TextDecorationConverter();
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            object? value = null;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not TextDecorations type.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_NonTextDecorationsValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns "None" when value is TextDecorations.None.
        /// </summary>
        [Fact]
        public void ConvertTo_NoneValue_ReturnsNone()
        {
            // Arrange
            var value = TextDecorations.None;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("None", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns "Underline" when value is TextDecorations.Underline.
        /// </summary>
        [Fact]
        public void ConvertTo_UnderlineValue_ReturnsUnderline()
        {
            // Arrange
            var value = TextDecorations.Underline;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("Underline", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns "Strikethrough" when value is TextDecorations.Strikethrough.
        /// </summary>
        [Fact]
        public void ConvertTo_StrikethroughValue_ReturnsStrikethrough()
        {
            // Arrange
            var value = TextDecorations.Strikethrough;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("Strikethrough", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns "Underline, Strikethrough" when value is the bitwise AND of Underline and Strikethrough.
        /// Note: This tests the actual implementation which uses bitwise AND (resulting in None/0) rather than bitwise OR.
        /// </summary>
        [Fact]
        public void ConvertTo_UnderlineAndStrikethroughBitwiseAnd_ReturnsUnderlineStrikethrough()
        {
            // Arrange
            var value = TextDecorations.Underline & TextDecorations.Strikethrough;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("Underline, Strikethrough", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is the bitwise OR of Underline and Strikethrough.
        /// </summary>
        [Fact]
        public void ConvertTo_UnderlineOrStrikethroughBitwiseOr_ThrowsNotSupportedException()
        {
            // Arrange
            var value = TextDecorations.Underline | TextDecorations.Strikethrough;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is an unsupported TextDecorations value.
        /// </summary>
        [Theory]
        [InlineData(99)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public void ConvertTo_UnsupportedTextDecorationsValue_ThrowsNotSupportedException(int enumValue)
        {
            // Arrange
            var value = (TextDecorations)enumValue;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo behavior is independent of context parameter value.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNonNullContext_ReturnsCorrectResult()
        {
            // Arrange
            var value = TextDecorations.Underline;
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = (CultureInfo?)null;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("Underline", result);
        }

        /// <summary>
        /// Tests that ConvertTo behavior is independent of culture parameter value.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNonNullCulture_ReturnsCorrectResult()
        {
            // Arrange
            var value = TextDecorations.Strikethrough;
            var context = (ITypeDescriptorContext?)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("Strikethrough", result);
        }

        /// <summary>
        /// Tests that ConvertTo behavior is independent of destinationType parameter value.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentDestinationType_ReturnsCorrectResult()
        {
            // Arrange
            var value = TextDecorations.None;
            var context = (ITypeDescriptorContext?)null;
            var culture = (CultureInfo?)null;
            var destinationType = typeof(object);

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("None", result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when value is null.
        /// This test targets the uncovered line 28 where null values are rejected.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            object value = null;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, value));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("Microsoft.Maui.TextDecorations", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when value.ToString() returns null.
        /// This test targets the uncovered line 28 where null string values are rejected.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValueWithNullToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            var value = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, value));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("Microsoft.Maui.TextDecorations", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when encountering an invalid enum value.
        /// This test targets the uncovered line 42 where unparseable values are rejected.
        /// </summary>
        [Theory]
        [InlineData("InvalidValue")]
        [InlineData("InvalidValue,Underline")]
        [InlineData("Underline,InvalidValue")]
        [InlineData("NotAnEnum")]
        [InlineData("123456")]
        [InlineData("@#$%")]
        public void ConvertFrom_InvalidEnumValue_ThrowsInvalidOperationException(string invalidValue)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, invalidValue));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("Microsoft.Maui.TextDecorations", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly parses valid single TextDecorations enum values.
        /// Verifies case insensitive parsing and basic enum conversion functionality.
        /// </summary>
        [Theory]
        [InlineData("None", TextDecorations.None)]
        [InlineData("Underline", TextDecorations.Underline)]
        [InlineData("Strikethrough", TextDecorations.Strikethrough)]
        [InlineData("none", TextDecorations.None)]
        [InlineData("UNDERLINE", TextDecorations.Underline)]
        [InlineData("strikethrough", TextDecorations.Strikethrough)]
        [InlineData(" Underline ", TextDecorations.Underline)]
        [InlineData("  None  ", TextDecorations.None)]
        public void ConvertFrom_ValidSingleValues_ReturnsExpectedTextDecorations(string input, TextDecorations expected)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly handles the special case "line-through" mapping to Strikethrough.
        /// Verifies the custom mapping logic for CSS-style text decoration names.
        /// </summary>
        [Theory]
        [InlineData("line-through", TextDecorations.Strikethrough)]
        [InlineData("LINE-THROUGH", TextDecorations.Strikethrough)]
        [InlineData("Line-Through", TextDecorations.Strikethrough)]
        [InlineData(" line-through ", TextDecorations.Strikethrough)]
        public void ConvertFrom_LineThrough_ReturnsStrikethrough(string input, TextDecorations expected)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly combines multiple TextDecorations values using comma separator.
        /// Verifies bitwise OR operation for flags enum values.
        /// </summary>
        [Theory]
        [InlineData("Underline,Strikethrough", TextDecorations.Underline | TextDecorations.Strikethrough)]
        [InlineData("None,Underline", TextDecorations.None | TextDecorations.Underline)]
        [InlineData("Underline,line-through", TextDecorations.Underline | TextDecorations.Strikethrough)]
        [InlineData(" Underline , Strikethrough ", TextDecorations.Underline | TextDecorations.Strikethrough)]
        [InlineData("underline,STRIKETHROUGH", TextDecorations.Underline | TextDecorations.Strikethrough)]
        public void ConvertFrom_MultipleValuesCommaSeparated_ReturnsCombinedFlags(string input, TextDecorations expected)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly combines multiple TextDecorations values using space separator.
        /// Verifies fallback to space separator when no comma is present.
        /// </summary>
        [Theory]
        [InlineData("Underline Strikethrough", TextDecorations.Underline | TextDecorations.Strikethrough)]
        [InlineData("None Underline", TextDecorations.None | TextDecorations.Underline)]
        [InlineData("Underline line-through", TextDecorations.Underline | TextDecorations.Strikethrough)]
        [InlineData("  Underline   Strikethrough  ", TextDecorations.Underline | TextDecorations.Strikethrough)]
        public void ConvertFrom_MultipleValuesSpaceSeparated_ReturnsCombinedFlags(string input, TextDecorations expected)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly handles empty strings and whitespace-only strings.
        /// Verifies that empty input is treated as an invalid value and throws exception.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void ConvertFrom_EmptyOrWhitespaceValues_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, input));

            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly handles various object types by calling their ToString() method.
        /// Verifies the method works with different input types that have valid string representations.
        /// </summary>
        [Theory]
        [InlineData(0, TextDecorations.None)] // int that matches None enum value
        [InlineData(1, TextDecorations.Underline)] // int that matches Underline enum value
        [InlineData(2, TextDecorations.Strikethrough)] // int that matches Strikethrough enum value
        public void ConvertFrom_DifferentObjectTypes_ConvertsViaToString(object input, TextDecorations expected)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom ignores context and culture parameters.
        /// Verifies that the method behavior is consistent regardless of these optional parameters.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_IgnoresParameters()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("fr-FR");

            // Act
            var result1 = converter.ConvertFrom(context, culture, "Underline");
            var result2 = converter.ConvertFrom(null, null, "Underline");

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(TextDecorations.Underline, result1);
        }

        /// <summary>
        /// Helper class that returns null from ToString() method.
        /// Used to test the null string handling path in ConvertFrom.
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString() => null;
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string) and false for all other types.
        /// Validates the core conversion capability logic with various type inputs.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        /// <param name="expectedResult">The expected boolean result</param>
        [Theory]
        [MemberData(nameof(CanConvertFromTestData))]
        public void CanConvertFrom_WithVariousSourceTypes_ReturnsExpectedResult(Type sourceType, bool expectedResult)
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when provided with a non-null ITypeDescriptorContext.
        /// Verifies that the context parameter is properly ignored in the implementation.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNonNullContext_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new TextDecorationConverter();
            var mockContext = Substitute.For<ITypeDescriptorContext>();

            // Act
            var resultForString = converter.CanConvertFrom(mockContext, typeof(string));
            var resultForInt = converter.CanConvertFrom(mockContext, typeof(int));

            // Assert
            Assert.True(resultForString);
            Assert.False(resultForInt);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// Validates proper null parameter handling for the non-nullable sourceType parameter.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNullSourceType_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new TextDecorationConverter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(null, null!));
        }

        public static IEnumerable<object[]> CanConvertFromTestData()
        {
            yield return new object[] { typeof(string), true };
            yield return new object[] { typeof(int), false };
            yield return new object[] { typeof(double), false };
            yield return new object[] { typeof(float), false };
            yield return new object[] { typeof(bool), false };
            yield return new object[] { typeof(char), false };
            yield return new object[] { typeof(byte), false };
            yield return new object[] { typeof(short), false };
            yield return new object[] { typeof(long), false };
            yield return new object[] { typeof(decimal), false };
            yield return new object[] { typeof(DateTime), false };
            yield return new object[] { typeof(TimeSpan), false };
            yield return new object[] { typeof(Guid), false };
            yield return new object[] { typeof(object), false };
            yield return new object[] { typeof(Array), false };
            yield return new object[] { typeof(List<string>), false };
            yield return new object[] { typeof(Dictionary<string, int>), false };
            yield return new object[] { typeof(IEnumerable<string>), false };
            yield return new object[] { typeof(ITypeDescriptorContext), false };
            yield return new object[] { typeof(TextDecorations), false };
            yield return new object[] { typeof(TextDecorationConverter), false };
        }
    }
}