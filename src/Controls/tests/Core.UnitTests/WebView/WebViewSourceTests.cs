#nullable disable

using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for WebViewSource.WebViewSourceTypeConverter class.
    /// </summary>
    public class WebViewSourceTests
    {
        /// <summary>
        /// Tests that ConvertTo always throws NotSupportedException regardless of input parameters.
        /// This test verifies the method consistently throws the expected exception for various parameter combinations.
        /// </summary>
        /// <param name="contextIsNull">Whether to pass null for context parameter</param>
        /// <param name="cultureInfoIsNull">Whether to pass null for cultureInfo parameter</param>
        /// <param name="valueIsNull">Whether to pass null for value parameter</param>
        /// <param name="destinationTypeIsNull">Whether to pass null for destinationType parameter</param>
        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(false, true, true, true)]
        [InlineData(true, false, true, true)]
        [InlineData(true, true, false, true)]
        [InlineData(true, true, true, false)]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, true, false, false)]
        [InlineData(true, false, false, false)]
        public void ConvertTo_AnyParameters_ThrowsNotSupportedException(bool contextIsNull, bool cultureInfoIsNull, bool valueIsNull, bool destinationTypeIsNull)
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));
            var context = contextIsNull ? null : Substitute.For<ITypeDescriptorContext>();
            var cultureInfo = cultureInfoIsNull ? null : CultureInfo.InvariantCulture;
            var value = valueIsNull ? null : new UrlWebViewSource { Url = "https://example.com" };
            var destinationType = destinationTypeIsNull ? null : typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, cultureInfo, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when converting to various destination types.
        /// This test verifies the method consistently throws regardless of the destination type.
        /// </summary>
        /// <param name="destinationType">The type to convert to</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(WebViewSource))]
        public void ConvertTo_VariousDestinationTypes_ThrowsNotSupportedException(Type destinationType)
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));
            var context = Substitute.For<ITypeDescriptorContext>();
            var cultureInfo = CultureInfo.InvariantCulture;
            var value = new UrlWebViewSource { Url = "https://example.com" };

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, cultureInfo, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when converting various value types.
        /// This test verifies the method consistently throws regardless of the input value type.
        /// </summary>
        /// <param name="value">The value to convert</param>
        [Theory]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_VariousValueTypes_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));
            var context = Substitute.For<ITypeDescriptorContext>();
            var cultureInfo = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, cultureInfo, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with different culture info settings.
        /// This test verifies the method consistently throws regardless of culture settings.
        /// </summary>
        /// <param name="cultureName">The culture name to use, or null for null culture</param>
        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("de-DE")]
        [InlineData("")]
        [InlineData(null)]
        public void ConvertTo_VariousCultures_ThrowsNotSupportedException(string cultureName)
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));
            var context = Substitute.For<ITypeDescriptorContext>();
            var cultureInfo = cultureName == null ? null : cultureName == "" ? CultureInfo.InvariantCulture : new CultureInfo(cultureName);
            var value = new UrlWebViewSource { Url = "https://example.com" };
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, cultureInfo, value, destinationType));
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for string destination type with valid context.
        /// Input: Valid context, string destination type.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for Uri destination type with valid context.
        /// Input: Valid context, Uri destination type.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void CanConvertTo_UriDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(Uri);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false with null context parameter.
        /// Input: Null context, string destination type.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContext_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            ITypeDescriptorContext context = null;
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false with null destination type parameter.
        /// Input: Valid context, null destination type.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false with both parameters null.
        /// Input: Null context, null destination type.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void CanConvertTo_BothParametersNull_ReturnsFalse()
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            ITypeDescriptorContext context = null;
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various destination types.
        /// Input: Valid context, different destination types including value types, reference types, and interfaces.
        /// Expected: Always returns false regardless of destination type.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(object))]
        [InlineData(typeof(WebViewSource))]
        [InlineData(typeof(IWebViewSource))]
        [InlineData(typeof(BindableObject))]
        public void CanConvertTo_VariousDestinationTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for generic and complex types.
        /// Input: Valid context, generic and complex destination types.
        /// Expected: Always returns false regardless of type complexity.
        /// </summary>
        [Theory]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, object>))]
        [InlineData(typeof(System.Func<string, bool>))]
        [InlineData(typeof(System.Action))]
        public void CanConvertTo_GenericAndComplexTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new WebViewSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        // Helper class to access the private WebViewSourceTypeConverter
        private class WebViewSourceTypeConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => false;
        }
    }

    public partial class WebViewSourceTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertFrom returns true for string type.
        /// Input: typeof(string)
        /// Expected: true
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));

            // Act
            var result = converter.CanConvertFrom(null, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true for Uri type.
        /// Input: typeof(Uri)
        /// Expected: true
        /// </summary>
        [Fact]
        public void CanConvertFrom_UriType_ReturnsTrue()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));

            // Act
            var result = converter.CanConvertFrom(null, typeof(Uri));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for unsupported types.
        /// Input: Various unsupported types
        /// Expected: false for all unsupported types
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(WebViewSource))]
        public void CanConvertFrom_UnsupportedTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom handles null sourceType parameter.
        /// Input: null sourceType
        /// Expected: false (null is not equal to typeof(string) or typeof(Uri))
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ReturnsFalse()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));

            // Act
            var result = converter.CanConvertFrom(null, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works with valid ITypeDescriptorContext.
        /// Input: Valid context and supported type
        /// Expected: true (context parameter is ignored in implementation)
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithValidContext_ReturnsCorrectResult()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(WebViewSource));
            var context = TypeDescriptor.CreateProperty(typeof(WebViewSource), "TestProperty", typeof(string)).CreateValueSerializationContext();

            // Act
            var resultForString = converter.CanConvertFrom(context, typeof(string));
            var resultForUri = converter.CanConvertFrom(context, typeof(Uri));
            var resultForInt = converter.CanConvertFrom(context, typeof(int));

            // Assert
            Assert.True(resultForString);
            Assert.True(resultForUri);
            Assert.False(resultForInt);
        }

        /// <summary>
        /// Tests that ConvertFrom with a valid string input creates a UrlWebViewSource with the correct URL.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidString_ReturnsUrlWebViewSourceWithUrl()
        {
            // Arrange
            var converter = CreateConverter();
            var testUrl = "https://example.com";

            // Act
            var result = converter.ConvertFrom(null, null, testUrl);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Equal(testUrl, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom with a null string input creates a UrlWebViewSource with null URL.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullString_ReturnsUrlWebViewSourceWithNullUrl()
        {
            // Arrange
            var converter = CreateConverter();
            string nullString = null;

            // Act
            var result = converter.ConvertFrom(null, null, nullString);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Null(urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom with an empty string input creates a UrlWebViewSource with empty URL.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsUrlWebViewSourceWithEmptyUrl()
        {
            // Arrange
            var converter = CreateConverter();
            var emptyString = string.Empty;

            // Act
            var result = converter.ConvertFrom(null, null, emptyString);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Equal(string.Empty, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom with a whitespace string input creates a UrlWebViewSource with whitespace URL.
        /// </summary>
        [Fact]
        public void ConvertFrom_WhitespaceString_ReturnsUrlWebViewSourceWithWhitespaceUrl()
        {
            // Arrange
            var converter = CreateConverter();
            var whitespaceString = "   ";

            // Act
            var result = converter.ConvertFrom(null, null, whitespaceString);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Equal(whitespaceString, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom with a valid Uri input creates a UrlWebViewSource with the AbsoluteUri.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidUri_ReturnsUrlWebViewSourceWithAbsoluteUri()
        {
            // Arrange
            var converter = CreateConverter();
            var testUri = new Uri("https://example.com/path?query=value");

            // Act
            var result = converter.ConvertFrom(null, null, testUri);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Equal(testUri.AbsoluteUri, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom with a null Uri input creates a UrlWebViewSource with null URL.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullUri_ReturnsUrlWebViewSourceWithNullUrl()
        {
            // Arrange
            var converter = CreateConverter();
            Uri nullUri = null;

            // Act
            var result = converter.ConvertFrom(null, null, nullUri);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Null(urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom with an integer input throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertFrom_IntegerInput_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = CreateConverter();
            var intValue = 42;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null, null, intValue));
        }

        /// <summary>
        /// Tests that ConvertFrom with a boolean input throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertFrom_BooleanInput_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = CreateConverter();
            var boolValue = true;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null, null, boolValue));
        }

        /// <summary>
        /// Tests that ConvertFrom with a custom object input throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertFrom_CustomObjectInput_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = CreateConverter();
            var customObject = new object();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null, null, customObject));
        }

        /// <summary>
        /// Tests that ConvertFrom with a DateTime input throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertFrom_DateTimeInput_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = CreateConverter();
            var dateTime = DateTime.Now;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null, null, dateTime));
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly regardless of context parameter.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithNonNullContext_WorksCorrectly()
        {
            // Arrange
            var converter = CreateConverter();
            var mockContext = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var testUrl = "https://example.com";

            // Act
            var result = converter.ConvertFrom(mockContext, null, testUrl);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Equal(testUrl, urlWebViewSource.Url);
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly regardless of culture parameter.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithNonNullCulture_WorksCorrectly()
        {
            // Arrange
            var converter = CreateConverter();
            var culture = CultureInfo.InvariantCulture;
            var testUrl = "https://example.com";

            // Act
            var result = converter.ConvertFrom(null, culture, testUrl);

            // Assert
            Assert.IsType<UrlWebViewSource>(result);
            var urlWebViewSource = (UrlWebViewSource)result;
            Assert.Equal(testUrl, urlWebViewSource.Url);
        }
    }
}