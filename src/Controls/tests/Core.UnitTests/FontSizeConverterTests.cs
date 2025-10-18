using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class FontSizeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string), regardless of context value.
        /// </summary>
        /// <param name="context">The type descriptor context (null or mocked)</param>
        [Theory]
        [InlineData(null)]
        [InlineData("mock")]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue(string contextType)
        {
            // Arrange
            var converter = new FontSizeConverter();
            ITypeDescriptorContext context = contextType == "mock" ? Substitute.For<ITypeDescriptorContext>() : null;
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null, regardless of context value.
        /// </summary>
        /// <param name="context">The type descriptor context (null or mocked)</param>
        [Theory]
        [InlineData(null)]
        [InlineData("mock")]
        public void CanConvertTo_DestinationTypeIsNull_ReturnsFalse(string contextType)
        {
            // Arrange
            var converter = new FontSizeConverter();
            ITypeDescriptorContext context = contextType == "mock" ? Substitute.For<ITypeDescriptorContext>() : null;
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is any type other than string, regardless of context value.
        /// </summary>
        /// <param name="destinationType">The destination type to test</param>
        /// <param name="contextType">The type descriptor context (null or mocked)</param>
        [Theory]
        [InlineData(typeof(int), null)]
        [InlineData(typeof(int), "mock")]
        [InlineData(typeof(double), null)]
        [InlineData(typeof(double), "mock")]
        [InlineData(typeof(object), null)]
        [InlineData(typeof(object), "mock")]
        [InlineData(typeof(bool), null)]
        [InlineData(typeof(bool), "mock")]
        [InlineData(typeof(DateTime), null)]
        [InlineData(typeof(DateTime), "mock")]
        [InlineData(typeof(FontSizeConverter), null)]
        [InlineData(typeof(FontSizeConverter), "mock")]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType, string contextType)
        {
            // Arrange
            var converter = new FontSizeConverter();
            ITypeDescriptorContext context = contextType == "mock" ? Substitute.For<ITypeDescriptorContext>() : null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns a parsed double value when given a valid numeric string.
        /// </summary>
        /// <param name="input">The numeric string input to convert</param>
        /// <param name="expected">The expected double result</param>
        [Theory]
        [InlineData("12.5", 12.5)]
        [InlineData("0", 0.0)]
        [InlineData("-1", -1.0)]
        [InlineData("999999", 999999.0)]
        [InlineData("0.0001", 0.0001)]
        [InlineData("1.7976931348623157E+308", double.MaxValue)]
        [InlineData("-1.7976931348623157E+308", double.MinValue)]
        public void ConvertFrom_ValidNumericString_ReturnsDoubleValue(string input, double expected)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns a parsed double value when given a numeric string with whitespace.
        /// This tests the trimming behavior on line 32.
        /// </summary>
        /// <param name="input">The numeric string input with whitespace to convert</param>
        /// <param name="expected">The expected double result</param>
        [Theory]
        [InlineData(" 12.5 ", 12.5)]
        [InlineData("\t10\n", 10.0)]
        [InlineData("  0  ", 0.0)]
        [InlineData(" -5.25\r\n", -5.25)]
        [InlineData("\n\r 100 \t", 100.0)]
        public void ConvertFrom_NumericStringWithWhitespace_ReturnsDoubleValue(string input, double expected)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom converts non-string objects using ToString() method.
        /// </summary>
        /// <param name="input">The non-string input to convert</param>
        /// <param name="expected">The expected double result</param>
        [Theory]
        [InlineData(42, 42.0)]
        [InlineData(3.14, 3.14)]
        [InlineData(-7, -7.0)]
        public void ConvertFrom_NonStringNumericObject_ReturnsDoubleValue(object input, double expected)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when given null input.
        /// This tests the exception path on line 59.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullInput_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, null));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into System.Double", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when given an empty string.
        /// This tests the exception path on line 59.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, ""));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into System.Double", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when given whitespace-only strings.
        /// This tests the trimming behavior on line 32 and exception path on line 59.
        /// </summary>
        /// <param name="input">The whitespace-only input string</param>
        [Theory]
        [InlineData("   ")]
        [InlineData("\t\t")]
        [InlineData("\n\r")]
        [InlineData(" \t\n\r ")]
        public void ConvertFrom_WhitespaceOnlyString_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, input));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into System.Double", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when given invalid string inputs.
        /// This tests the exception path on line 59.
        /// </summary>
        /// <param name="input">The invalid string input</param>
        [Theory]
        [InlineData("abc")]
        [InlineData("not-a-number")]
        [InlineData("12.5.6")]
        [InlineData("1e999")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData("12x")]
        [InlineData("x12")]
        public void ConvertFrom_InvalidString_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, input));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into System.Double", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when given an object that converts to invalid string.
        /// This tests the exception path on line 59.
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithInvalidToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FontSizeConverter();
            var invalidObject = new InvalidToStringObject();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, invalidObject));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into System.Double", exception.Message);
        }

        /// <summary>
        /// PARTIAL TEST: Tests named size string conversion.
        /// 
        /// NOTE: This test cannot be completed automatically because it requires mocking the static Device.GetNamedSize method
        /// which calls DependencyService.Get&lt;IFontNamedSizeService&gt;(). To make this test work:
        /// 
        /// 1. Set up a mock IFontNamedSizeService in your test setup
        /// 2. Register it with DependencyService before running the test
        /// 3. Configure the mock to return expected font size values
        /// 
        /// This test covers the named size string matching logic on lines 35-54.
        /// </summary>
        /// <param name="namedSizeString">The named size string to test</param>
        /// <param name="expectedNamedSize">The expected NamedSize enum value</param>
        [Theory(Skip = "Requires IFontNamedSizeService dependency setup - see test comments for instructions")]
        [InlineData("Default", NamedSize.Default)]
        [InlineData("Micro", NamedSize.Micro)]
        [InlineData("Small", NamedSize.Small)]
        [InlineData("Medium", NamedSize.Medium)]
        [InlineData("Large", NamedSize.Large)]
        [InlineData("Body", NamedSize.Body)]
        [InlineData("Caption", NamedSize.Caption)]
        [InlineData("Header", NamedSize.Header)]
        [InlineData("Subtitle", NamedSize.Subtitle)]
        [InlineData("Title", NamedSize.Title)]
        public void ConvertFrom_NamedSizeString_ReturnsNamedSizeValue(string namedSizeString, NamedSize expectedNamedSize)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // TODO: Set up IFontNamedSizeService mock here
            // var mockService = Substitute.For<IFontNamedSizeService>();
            // mockService.GetNamedSize(expectedNamedSize, typeof(Label), false).Returns(expectedFontSize);
            // DependencyService.Register<IFontNamedSizeService>(mockService);

            // Act
            var result = converter.ConvertFrom(null, null, namedSizeString);

            // Assert
            // Assert.IsType<double>(result);
            // Assert.Equal(expectedFontSize, result);
        }

        /// <summary>
        /// PARTIAL TEST: Tests named size string conversion with whitespace.
        /// 
        /// NOTE: This test cannot be completed automatically because it requires mocking the static Device.GetNamedSize method.
        /// See the ConvertFrom_NamedSizeString_ReturnsNamedSizeValue test comments for setup instructions.
        /// 
        /// This test covers the trimming behavior on line 32 combined with named size matching on lines 35-54.
        /// </summary>
        /// <param name="namedSizeStringWithWhitespace">The named size string with whitespace to test</param>
        /// <param name="expectedNamedSize">The expected NamedSize enum value</param>
        [Theory(Skip = "Requires IFontNamedSizeService dependency setup - see test comments for instructions")]
        [InlineData(" Default ", NamedSize.Default)]
        [InlineData("\tMicro\n", NamedSize.Micro)]
        [InlineData("  Small  ", NamedSize.Small)]
        public void ConvertFrom_NamedSizeStringWithWhitespace_ReturnsNamedSizeValue(string namedSizeStringWithWhitespace, NamedSize expectedNamedSize)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // TODO: Set up IFontNamedSizeService mock here - see other partial test for instructions

            // Act
            var result = converter.ConvertFrom(null, null, namedSizeStringWithWhitespace);

            // Assert
            // Assert.IsType<double>(result);
        }

        /// <summary>
        /// PARTIAL TEST: Tests enum parsing fallback for NamedSize values.
        /// 
        /// NOTE: This test cannot be completed automatically because it requires mocking the static Device.GetNamedSize method.
        /// See the ConvertFrom_NamedSizeString_ReturnsNamedSizeValue test comments for setup instructions.
        /// 
        /// This test covers the Enum.TryParse fallback logic on lines 55-56.
        /// </summary>
        /// <param name="enumValue">The enum value as string to test</param>
        /// <param name="expectedNamedSize">The expected NamedSize enum value</param>
        [Theory(Skip = "Requires IFontNamedSizeService dependency setup - see test comments for instructions")]
        [InlineData("0", NamedSize.Default)]
        [InlineData("1", NamedSize.Micro)]
        [InlineData("9", NamedSize.Caption)]
        public void ConvertFrom_EnumValueString_ReturnsNamedSizeValue(string enumValue, NamedSize expectedNamedSize)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // TODO: Set up IFontNamedSizeService mock here - see other partial test for instructions

            // Act
            var result = converter.ConvertFrom(null, null, enumValue);

            // Assert
            // Assert.IsType<double>(result);
        }

        /// <summary>
        /// Helper class for testing objects with invalid ToString() results.
        /// </summary>
        private class InvalidToStringObject
        {
            public override string ToString() => "invalid-font-size";
        }

        /// <summary>
        /// Tests that ConvertTo successfully converts double values to their string representation using InvariantCulture.
        /// </summary>
        /// <param name="value">The double value to convert</param>
        /// <param name="expectedResult">The expected string result</param>
        [Theory]
        [InlineData(0.0, "0")]
        [InlineData(12.5, "12.5")]
        [InlineData(-5.75, "-5.75")]
        [InlineData(100.0, "100")]
        [InlineData(double.MaxValue, "1.7976931348623157E+308")]
        [InlineData(double.MinValue, "-1.7976931348623157E+308")]
        [InlineData(double.PositiveInfinity, "Infinity")]
        [InlineData(double.NegativeInfinity, "-Infinity")]
        [InlineData(double.NaN, "NaN")]
        public void ConvertTo_WithDoubleValue_ReturnsStringRepresentation(double value, string expectedResult)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act
            var result = converter.ConvertTo(null, null, value, typeof(string));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when the value parameter is not a double.
        /// </summary>
        /// <param name="value">The non-double value to test</param>
        [Theory]
        [InlineData(null)]
        [InlineData("12.5")]
        [InlineData(42)]
        [InlineData(42f)]
        [InlineData(true)]
        [InlineData('c')]
        public void ConvertTo_WithNonDoubleValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new FontSizeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo ignores the context parameter and still functions correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNonNullContext_IgnoresContextAndConvertsSuccessfully()
        {
            // Arrange
            var converter = new FontSizeConverter();
            var mockContext = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            double testValue = 25.5;

            // Act
            var result = converter.ConvertTo(mockContext, null, testValue, typeof(string));

            // Assert
            Assert.Equal("25.5", result);
        }

        /// <summary>
        /// Tests that ConvertTo ignores the culture parameter and always uses InvariantCulture.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentCulture_IgnoresCultureAndUsesInvariantCulture()
        {
            // Arrange
            var converter = new FontSizeConverter();
            var germanCulture = CultureInfo.GetCultureInfo("de-DE");
            double testValue = 12.5;

            // Act
            var result = converter.ConvertTo(null, germanCulture, testValue, typeof(string));

            // Assert
            Assert.Equal("12.5", result); // Should use dot, not German comma
        }

        /// <summary>
        /// Tests that ConvertTo ignores the destinationType parameter and still functions correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentDestinationType_IgnoresDestinationTypeAndConverts()
        {
            // Arrange
            var converter = new FontSizeConverter();
            double testValue = 15.0;

            // Act
            var result = converter.ConvertTo(null, null, testValue, typeof(object));

            // Assert
            Assert.Equal("15", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new FontSizeConverter();
            var sourceType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string source types.
        /// </summary>
        /// <param name="sourceType">The source type to test</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(FontSizeConverter))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new FontSizeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string) and context is a mock object.
        /// Verifies that the context parameter does not affect the result.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ContextIsMocked_SourceTypeStringReturnsTrue()
        {
            // Arrange
            var converter = new FontSizeConverter();
            var sourceType = typeof(string);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is not string and context is a mock object.
        /// Verifies that the context parameter does not affect the result.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ContextIsMocked_SourceTypeNotStringReturnsFalse()
        {
            // Arrange
            var converter = new FontSizeConverter();
            var sourceType = typeof(int);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new FontSizeConverter();
            Type sourceType = null;
            ITypeDescriptorContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(context, sourceType));
        }
    }
}