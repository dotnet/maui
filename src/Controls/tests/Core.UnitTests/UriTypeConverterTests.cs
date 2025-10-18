using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class UriTypeConverterTests
    {
        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new UriTypeConverter();
            object value = null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is a string.
        /// </summary>
        [Fact]
        public void ConvertTo_StringValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new UriTypeConverter();
            object value = "http://example.com";
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is an integer.
        /// </summary>
        [Fact]
        public void ConvertTo_IntValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new UriTypeConverter();
            object value = 42;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is a custom object.
        /// </summary>
        [Fact]
        public void ConvertTo_CustomObjectValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new UriTypeConverter();
            object value = new object();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns URI string when value is a valid absolute URI.
        /// </summary>
        [Fact]
        public void ConvertTo_AbsoluteUri_ReturnsUriString()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var uri = new Uri("http://example.com", UriKind.Absolute);
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, uri, destinationType);

            // Assert
            Assert.Equal("http://example.com/", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns URI string when value is a valid relative URI.
        /// </summary>
        [Fact]
        public void ConvertTo_RelativeUri_ReturnsUriString()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var uri = new Uri("path/to/resource", UriKind.Relative);
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, uri, destinationType);

            // Assert
            Assert.Equal("path/to/resource", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns URI string when value is a URI with query parameters.
        /// </summary>
        [Fact]
        public void ConvertTo_UriWithQuery_ReturnsUriString()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var uri = new Uri("https://example.com/api?param=value", UriKind.Absolute);
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, uri, destinationType);

            // Assert
            Assert.Equal("https://example.com/api?param=value", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with non-null context and culture parameters.
        /// </summary>
        [Fact]
        public void ConvertTo_WithContextAndCulture_ReturnsUriString()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var uri = new Uri("http://example.com", UriKind.Absolute);
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, uri, destinationType);

            // Assert
            Assert.Equal("http://example.com/", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different destination types (parameter is ignored).
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentDestinationType_ReturnsUriString()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var uri = new Uri("http://example.com", UriKind.Absolute);
            var destinationType = typeof(object);

            // Act
            var result = converter.ConvertTo(null, null, uri, destinationType);

            // Assert
            Assert.Equal("http://example.com/", result);
        }

        /// <summary>
        /// Tests ConvertFrom with null value parameter.
        /// Should return null when value is null.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ReturnsNull()
        {
            // Arrange
            var converter = new UriTypeConverter();
            object value = null;

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertFrom with empty string value.
        /// Should return null when value ToString results in empty string.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsNull()
        {
            // Arrange
            var converter = new UriTypeConverter();
            object value = "";

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertFrom with whitespace-only string value.
        /// Should return null when value ToString results in whitespace-only string.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData(" \t \n ")]
        public void ConvertFrom_WhitespaceOnlyString_ReturnsNull(string whitespaceValue)
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, whitespaceValue);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertFrom with valid absolute URI string.
        /// Should return Uri object when value ToString results in valid absolute URI.
        /// </summary>
        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://localhost:8080/path")]
        [InlineData("ftp://ftp.example.com/file.txt")]
        [InlineData("file:///C:/temp/file.txt")]
        public void ConvertFrom_ValidAbsoluteUri_ReturnsUri(string uriString)
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, uriString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Uri>(result);
            var uri = (Uri)result;
            Assert.Equal(uriString, uri.ToString());
            Assert.True(uri.IsAbsoluteUri);
        }

        /// <summary>
        /// Tests ConvertFrom with valid relative URI string.
        /// Should return Uri object when value ToString results in valid relative URI.
        /// </summary>
        [Theory]
        [InlineData("path/to/file")]
        [InlineData("../parent/file.txt")]
        [InlineData("./current/file")]
        [InlineData("file.html")]
        public void ConvertFrom_ValidRelativeUri_ReturnsUri(string uriString)
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, uriString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Uri>(result);
            var uri = (Uri)result;
            Assert.Equal(uriString, uri.ToString());
            Assert.False(uri.IsAbsoluteUri);
        }

        /// <summary>
        /// Tests ConvertFrom with invalid URI string.
        /// Should throw UriFormatException when value ToString results in invalid URI format.
        /// </summary>
        [Theory]
        [InlineData("ht tp://invalid space.com")]
        [InlineData("://missing-scheme")]
        [InlineData("http://[invalid-ipv6")]
        public void ConvertFrom_InvalidUriString_ThrowsUriFormatException(string invalidUriString)
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act & Assert
            Assert.Throws<UriFormatException>(() => converter.ConvertFrom(null, null, invalidUriString));
        }

        /// <summary>
        /// Tests ConvertFrom with object that has valid URI ToString result.
        /// Should return Uri object when value ToString results in valid URI string.
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithValidUriToString_ReturnsUri()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var customObject = new CustomObjectWithToString("https://example.com");

            // Act
            var result = converter.ConvertFrom(null, null, customObject);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Uri>(result);
            var uri = (Uri)result;
            Assert.Equal("https://example.com", uri.ToString());
        }

        /// <summary>
        /// Tests ConvertFrom with object that has empty ToString result.
        /// Should return null when value ToString results in empty string.
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithEmptyToString_ReturnsNull()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var customObject = new CustomObjectWithToString("");

            // Act
            var result = converter.ConvertFrom(null, null, customObject);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertFrom with object that has null ToString result.
        /// Should return null when value ToString returns null.
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithNullToString_ReturnsNull()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var customObject = new CustomObjectWithToString(null);

            // Act
            var result = converter.ConvertFrom(null, null, customObject);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertFrom with various context and culture parameters.
        /// Should work correctly regardless of context and culture values since they are not used.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_WorksCorrectly()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var context = new MockTypeDescriptorContext();
            var culture = new CultureInfo("en-US");
            var uriString = "https://example.com";

            // Act
            var result = converter.ConvertFrom(context, culture, uriString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Uri>(result);
            var uri = (Uri)result;
            Assert.Equal(uriString, uri.ToString());
        }

        /// <summary>
        /// Helper class for testing objects with custom ToString behavior.
        /// </summary>
        private class CustomObjectWithToString
        {
            private readonly string _value;

            public CustomObjectWithToString(string value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when the destination type is string.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when the destination type is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Validates the method's behavior with different type parameters.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(char))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(long))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(Array))]
        [InlineData(typeof(Type))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new UriTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo works correctly when context is null.
        /// Verifies the method handles null context parameter properly.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContext_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo works correctly when context is null and destination type is not string.
        /// Verifies the method handles null context parameter properly with non-string types.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContext_NonStringDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new UriTypeConverter();
            var destinationType = typeof(int);

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string and false for all other types.
        /// Verifies that the context parameter does not affect the result.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from.</param>
        /// <param name="expected">The expected result of CanConvertFrom.</param>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(Uri), false)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(double), false)]
        [InlineData(typeof(char), false)]
        [InlineData(typeof(byte[]), false)]
        [InlineData(typeof(ITypeDescriptorContext), false)]
        [InlineData(typeof(Type), false)]
        public void CanConvertFrom_VariousSourceTypes_ReturnsExpectedResult(Type sourceType, bool expected)
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns the same result regardless of the context parameter value.
        /// Verifies that providing a non-null context does not change the behavior.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from.</param>
        /// <param name="expected">The expected result of CanConvertFrom.</param>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(Uri), false)]
        public void CanConvertFrom_WithNonNullContext_ReturnsExpectedResult(Type sourceType, bool expected)
        {
            // Arrange
            var converter = new UriTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom specifically returns true only for string type.
        /// Validates the core functionality that only string sources are supported for conversion.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for non-string types.
        /// Ensures that types other than string are not supported for conversion.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NonStringType_ReturnsFalse()
        {
            // Arrange
            var converter = new UriTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, typeof(int));

            // Assert
            Assert.False(result);
        }
    }
}