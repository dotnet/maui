using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public sealed class ImageSourceConverterTests
    {
        [Fact]
        public void ConvertFrom_WithValidAbsoluteUri_ReturnsImageSourceFromUri()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var uri = new Uri("https://example.com/image.png");

            // Act
            var result = converter.ConvertFrom(null, null, uri);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(uri, uriImageSource.Uri);
        }

        [Fact]
        public void ConvertFrom_WithNullUri_ReturnsNull()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            Uri nullUri = null;

            // Act
            var result = converter.ConvertFrom(null, null, nullUri);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ConvertFrom_WithRelativeUri_ThrowsArgumentException()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var relativeUri = new Uri("/relative/path", UriKind.Relative);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => converter.ConvertFrom(null, null, relativeUri));
            Assert.Equal("uri is relative", exception.Message);
        }

        [Theory]
        [InlineData("https://example.com/image.png")]
        [InlineData("http://example.com/image.jpg")]
        [InlineData("ftp://example.com/image.gif")]
        public void ConvertFrom_WithValidAbsoluteUriString_ReturnsUriImageSource(string uriString)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, uriString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(uriString, uriImageSource.Uri.ToString());
        }

        [Theory]
        [InlineData("file:///path/to/image.png")]
        [InlineData("file://localhost/path/to/image.jpg")]
        public void ConvertFrom_WithFileUriString_ReturnsFileImageSource(string fileUriString)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, fileUriString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(fileUriString, fileImageSource.File);
        }

        [Theory]
        [InlineData("image.png")]
        [InlineData("/path/to/image.jpg")]
        [InlineData("../relative/image.gif")]
        [InlineData("C:\\path\\to\\image.bmp")]
        public void ConvertFrom_WithFilePathString_ReturnsFileImageSource(string filePath)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, filePath);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(filePath, fileImageSource.File);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void ConvertFrom_WithEmptyOrWhitespaceString_ReturnsFileImageSource(string input)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(input, fileImageSource.File);
        }

        [Theory]
        [InlineData("not a valid uri")]
        [InlineData("://invalid")]
        [InlineData("http://")]
        public void ConvertFrom_WithInvalidUriString_ReturnsFileImageSource(string invalidUri)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, invalidUri);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(invalidUri, fileImageSource.File);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertFrom_WithNonStringNonUriObject_ReturnsFileImageSourceWithToStringValue(object value)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(value.ToString(), fileImageSource.File);
        }

        [Fact]
        public void ConvertFrom_WithCustomObjectReturningNullFromToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var customObject = new CustomObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, customObject));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("into Microsoft.Maui.Controls.ImageSource", exception.Message);
        }

        [Fact]
        public void ConvertFrom_WithContextAndCulture_IgnoresContextAndCulture()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var uri = new Uri("https://example.com/image.png");

            // Act
            var result = converter.ConvertFrom(context, culture, uri);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UriImageSource>(result);
        }

        [Theory]
        [InlineData("https://example.com/very/long/path/to/image/file/with/many/segments/that/creates/a/very/long/uri/string/image.png")]
        public void ConvertFrom_WithVeryLongUriString_ReturnsUriImageSource(string longUriString)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, longUriString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(longUriString, uriImageSource.Uri.ToString());
        }

        [Theory]
        [InlineData("https://example.com/image with spaces.png")]
        [InlineData("https://example.com/image%20with%20encoded%20spaces.png")]
        [InlineData("https://example.com/image?query=value&other=param")]
        [InlineData("https://example.com/image#fragment")]
        public void ConvertFrom_WithSpecialCharactersInUriString_ReturnsUriImageSource(string uriWithSpecialChars)
        {
            // Arrange
            var converter = new ImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, uriWithSpecialChars);

            // Assert
            Assert.NotNull(result);
            if (Uri.TryCreate(uriWithSpecialChars, UriKind.Absolute, out var uri) && uri.Scheme != "file")
            {
                Assert.IsType<UriImageSource>(result);
            }
            else
            {
                Assert.IsType<FileImageSource>(result);
            }
        }

        /// <summary>
        /// Custom object that returns null from ToString() to test the exception path.
        /// This violates .NET conventions but is used to test the edge case.
        /// </summary>
        private sealed class CustomObjectWithNullToString
        {
            public override string ToString() => null;
        }

        /// <summary>
        /// Tests that ConvertTo returns the File property value when given a FileImageSource with a valid file path.
        /// </summary>
        [Fact]
        public void ConvertTo_WithValidFileImageSource_ReturnsFileProperty()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var fileImageSource = new FileImageSource { File = "test.png" };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Equal("test.png", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns null when given a FileImageSource with a null File property.
        /// </summary>
        [Fact]
        public void ConvertTo_WithFileImageSourceNullFile_ReturnsNull()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var fileImageSource = new FileImageSource(); // File property defaults to null
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertTo returns the Uri.ToString() value when given a UriImageSource with a valid URI.
        /// </summary>
        [Fact]
        public void ConvertTo_WithValidUriImageSource_ReturnsUriToString()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var uri = new Uri("https://example.com/image.png");
            var uriImageSource = new UriImageSource { Uri = uri };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, uriImageSource, destinationType);

            // Assert
            Assert.Equal("https://example.com/image.png", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NullReferenceException when given a UriImageSource with null Uri property.
        /// </summary>
        [Fact]
        public void ConvertTo_WithUriImageSourceNullUri_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var uriImageSource = new UriImageSource(); // Uri property defaults to null
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                converter.ConvertTo(null, null, uriImageSource, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given null value.
        /// </summary>
        [Fact]
        public void ConvertTo_WithNullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given unsupported value types.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(123)]
        [InlineData(45.67)]
        [InlineData(true)]
        public void ConvertTo_WithUnsupportedValueTypes_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different context and culture parameters.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentContextAndCulture_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var fileImageSource = new FileImageSource { File = "image.jpg" };
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, fileImageSource, destinationType);

            // Assert
            Assert.Equal("image.jpg", result);
        }

        /// <summary>
        /// Tests ConvertTo with UriImageSource containing a complex URI with query parameters and fragments.
        /// </summary>
        [Fact]
        public void ConvertTo_WithComplexUri_ReturnsFullUriString()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var complexUri = new Uri("https://example.com/images/photo.jpg?size=large&format=png#section1");
            var uriImageSource = new UriImageSource { Uri = complexUri };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, uriImageSource, destinationType);

            // Assert
            Assert.Equal("https://example.com/images/photo.jpg?size=large&format=png#section1", result);
        }

        /// <summary>
        /// Tests ConvertTo with FileImageSource containing an empty string file path.
        /// </summary>
        [Fact]
        public void ConvertTo_WithEmptyStringFile_ReturnsEmptyString()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var fileImageSource = new FileImageSource { File = string.Empty };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is string type.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is string and context is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsStringAndContextIsNull_ReturnsTrue()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var destinationType = typeof(string);

            // Act
            bool result = converter.CanConvertTo(null, destinationType);

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
            var converter = new ImageSourceConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertTo(context, null);

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
        [InlineData(typeof(Uri))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo ignores the context parameter and only considers destinationType.
        /// </summary>
        [Fact]
        public void CanConvertTo_ContextParameterIgnored_BehaviorConsistent()
        {
            // Arrange
            var converter = new ImageSourceConverter();
            var context1 = Substitute.For<ITypeDescriptorContext>();
            var context2 = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            bool resultWithContext1 = converter.CanConvertTo(context1, destinationType);
            bool resultWithContext2 = converter.CanConvertTo(context2, destinationType);
            bool resultWithNullContext = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(resultWithContext1);
            Assert.True(resultWithContext2);
            Assert.True(resultWithNullContext);
            Assert.Equal(resultWithContext1, resultWithContext2);
            Assert.Equal(resultWithContext1, resultWithNullContext);
        }
    }
}