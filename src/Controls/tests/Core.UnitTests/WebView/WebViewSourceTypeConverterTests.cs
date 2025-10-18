using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class WebViewSourceTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// </summary>
        /// <param name="destinationType">The destination type to test</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(WebViewSource))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string) regardless of context parameter.
        /// </summary>
        [Fact]
        public void CanConvertTo_WithNonNullContext_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not string regardless of context parameter.
        /// </summary>
        [Fact]
        public void CanConvertTo_WithNonNullContext_DestinationTypeIsNotString_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(int);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a non-null string value to UrlWebViewSource.
        /// Input: Valid string value.
        /// Expected: Returns UrlWebViewSource with the string as Url property.
        /// </summary>
        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://test.com")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("relative/path")]
        [InlineData("file:///local/path")]
        public void ConvertFrom_WithValidStringValue_ReturnsUrlWebViewSourceWithCorrectUrl(string urlValue)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, urlValue);

            // Assert
            Assert.NotNull(result);
            var urlWebViewSource = Assert.IsType<UrlWebViewSource>(result);
            Assert.Equal(urlValue, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts an object with non-null ToString() result to UrlWebViewSource.
        /// Input: Object with ToString() returning a non-null string.
        /// Expected: Returns UrlWebViewSource with ToString() result as Url property.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithObjectHavingToStringResult_ReturnsUrlWebViewSourceWithToStringValue()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var testObject = new TestObjectWithToString("custom-url");

            // Act
            var result = converter.ConvertFrom(null, null, testObject);

            // Assert
            Assert.NotNull(result);
            var urlWebViewSource = Assert.IsType<UrlWebViewSource>(result);
            Assert.Equal("custom-url", urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when value is null.
        /// Input: null value.
        /// Expected: Throws InvalidOperationException with specific message.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithNullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, null));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into Microsoft.Maui.Controls.UrlWebViewSource", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when object's ToString() returns null.
        /// Input: Object with ToString() returning null.
        /// Expected: Throws InvalidOperationException with specific message.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithObjectReturningNullFromToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var testObject = new TestObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, testObject));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into Microsoft.Maui.Controls.UrlWebViewSource", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly with various context and culture parameters.
        /// Input: Valid string with different context and culture combinations.
        /// Expected: Returns UrlWebViewSource regardless of context and culture values.
        /// </summary>
        [Theory]
        [InlineData("test-url", true, true)]
        [InlineData("test-url", true, false)]
        [InlineData("test-url", false, true)]
        [InlineData("test-url", false, false)]
        public void ConvertFrom_WithVariousContextAndCultureParameters_ReturnsCorrectResult(
            string urlValue, bool hasContext, bool hasCulture)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            ITypeDescriptorContext context = hasContext ? NSubstitute.Substitute.For<ITypeDescriptorContext>() : null;
            CultureInfo culture = hasCulture ? CultureInfo.InvariantCulture : null;

            // Act
            var result = converter.ConvertFrom(context, culture, urlValue);

            // Assert
            Assert.NotNull(result);
            var urlWebViewSource = Assert.IsType<UrlWebViewSource>(result);
            Assert.Equal(urlValue, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom handles numeric values by converting them to string via ToString().
        /// Input: Various numeric types.
        /// Expected: Returns UrlWebViewSource with string representation of the number.
        /// </summary>
        [Theory]
        [InlineData(123)]
        [InlineData(123.456)]
        [InlineData(0)]
        [InlineData(-42)]
        public void ConvertFrom_WithNumericValues_ReturnsUrlWebViewSourceWithStringRepresentation(object numericValue)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var expectedUrl = numericValue.ToString();

            // Act
            var result = converter.ConvertFrom(null, null, numericValue);

            // Assert
            Assert.NotNull(result);
            var urlWebViewSource = Assert.IsType<UrlWebViewSource>(result);
            Assert.Equal(expectedUrl, urlWebViewSource.Url);
        }

        /// <summary>
        /// Helper class for testing objects with custom ToString() implementation.
        /// </summary>
        private class TestObjectWithToString
        {
            private readonly string _value;

            public TestObjectWithToString(string value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        /// <summary>
        /// Helper class for testing objects with ToString() returning null.
        /// </summary>
        private class TestObjectWithNullToString
        {
            public override string ToString()
            {
                return null;
            }
        }

        /// <summary>
        /// Tests that ConvertTo returns the URL when provided with a valid UrlWebViewSource.
        /// Input: UrlWebViewSource with a URL string.
        /// Expected: Returns the URL string from the UrlWebViewSource.
        /// </summary>
        [Fact]
        public void ConvertTo_WithValidUrlWebViewSource_ReturnsUrl()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var urlWebViewSource = new UrlWebViewSource { Url = "https://example.com" };
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, urlWebViewSource, destinationType);

            // Assert
            Assert.Equal("https://example.com", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns the URL when UrlWebViewSource has an empty URL.
        /// Input: UrlWebViewSource with empty URL string.
        /// Expected: Returns empty string.
        /// </summary>
        [Fact]
        public void ConvertTo_WithUrlWebViewSourceEmptyUrl_ReturnsEmptyString()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var urlWebViewSource = new UrlWebViewSource { Url = string.Empty };

            // Act
            var result = converter.ConvertTo(null, null, urlWebViewSource, typeof(string));

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns null when UrlWebViewSource has null URL.
        /// Input: UrlWebViewSource with null URL.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void ConvertTo_WithUrlWebViewSourceNullUrl_ReturnsNull()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var urlWebViewSource = new UrlWebViewSource { Url = null };

            // Act
            var result = converter.ConvertTo(null, null, urlWebViewSource, typeof(string));

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// Input: Null value parameter.
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for non-UrlWebViewSource values.
        /// Input: Various object types that are not UrlWebViewSource.
        /// Expected: Throws NotSupportedException for all cases.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_WithNonUrlWebViewSourceValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo behavior is consistent regardless of context parameter values.
        /// Input: UrlWebViewSource with various context values (null and mocked).
        /// Expected: Returns URL consistently regardless of context.
        /// </summary>
        [Theory]
        [InlineData(null)]
        public void ConvertTo_WithDifferentContextValues_BehavesConsistently(ITypeDescriptorContext context)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var urlWebViewSource = new UrlWebViewSource { Url = "https://test.com" };

            // Act
            var result1 = converter.ConvertTo(context, null, urlWebViewSource, typeof(string));
            var result2 = converter.ConvertTo(Substitute.For<ITypeDescriptorContext>(), null, urlWebViewSource, typeof(string));

            // Assert
            Assert.Equal("https://test.com", result1);
            Assert.Equal("https://test.com", result2);
        }

        /// <summary>
        /// Tests that ConvertTo behavior is consistent regardless of culture parameter values.
        /// Input: UrlWebViewSource with various culture values (null and specific cultures).
        /// Expected: Returns URL consistently regardless of culture.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentCultureValues_BehavesConsistently()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var urlWebViewSource = new UrlWebViewSource { Url = "https://culture-test.com" };

            // Act
            var resultWithNull = converter.ConvertTo(null, null, urlWebViewSource, typeof(string));
            var resultWithInvariant = converter.ConvertTo(null, CultureInfo.InvariantCulture, urlWebViewSource, typeof(string));
            var resultWithSpecific = converter.ConvertTo(null, new CultureInfo("en-US"), urlWebViewSource, typeof(string));

            // Assert
            Assert.Equal("https://culture-test.com", resultWithNull);
            Assert.Equal("https://culture-test.com", resultWithInvariant);
            Assert.Equal("https://culture-test.com", resultWithSpecific);
        }

        /// <summary>
        /// Tests that ConvertTo behavior is consistent regardless of destinationType parameter values.
        /// Input: UrlWebViewSource with various destination type values.
        /// Expected: Returns URL consistently regardless of destination type.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_WithDifferentDestinationTypes_BehavesConsistently(Type destinationType)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var urlWebViewSource = new UrlWebViewSource { Url = "https://type-test.com" };

            // Act
            var result = converter.ConvertTo(null, null, urlWebViewSource, destinationType);

            // Assert
            Assert.Equal("https://type-test.com", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles UrlWebViewSource with very long URL.
        /// Input: UrlWebViewSource with extremely long URL string.
        /// Expected: Returns the complete long URL string.
        /// </summary>
        [Fact]
        public void ConvertTo_WithVeryLongUrl_ReturnsCompleteUrl()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var longUrl = "https://example.com/" + new string('a', 2000);
            var urlWebViewSource = new UrlWebViewSource { Url = longUrl };

            // Act
            var result = converter.ConvertTo(null, null, urlWebViewSource, typeof(string));

            // Assert
            Assert.Equal(longUrl, result);
        }

        /// <summary>
        /// Tests that ConvertTo handles UrlWebViewSource with URL containing special characters.
        /// Input: UrlWebViewSource with URL containing various special characters.
        /// Expected: Returns the URL with all special characters intact.
        /// </summary>
        [Fact]
        public void ConvertTo_WithSpecialCharactersInUrl_ReturnsUrlWithSpecialCharacters()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var specialUrl = "https://example.com/path?query=value&param=测试#fragment";
            var urlWebViewSource = new UrlWebViewSource { Url = specialUrl };

            // Act
            var result = converter.ConvertTo(null, null, urlWebViewSource, typeof(string));

            // Assert
            Assert.Equal(specialUrl, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when the source type is string.
        /// This validates the core functionality of the converter which should only accept string inputs.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var sourceType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// This ensures the converter properly rejects unsupported source types.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(char))]
        [InlineData(typeof(long))]
        [InlineData(typeof(short))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(sbyte))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context parameter is provided.
        /// This verifies that the context parameter is properly ignored in the implementation.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithMockedContext_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act & Assert - Test with string type (should return true)
            var resultForString = converter.CanConvertFrom(context, typeof(string));
            Assert.True(resultForString);

            // Act & Assert - Test with non-string type (should return false)
            var resultForInt = converter.CanConvertFrom(context, typeof(int));
            Assert.False(resultForInt);
        }

        /// <summary>
        /// Tests that CanConvertFrom handles complex and custom types correctly.
        /// This ensures the converter rejects complex types including arrays, generics, and custom classes.
        /// </summary>
        /// <param name="sourceType">The complex source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(WebViewSourceTypeConverter))]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, object>))]
        public void CanConvertFrom_ComplexSourceTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}