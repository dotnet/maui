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
    /// Unit tests for the FileImageSourceConverter class.
    /// </summary>
    public sealed class FileImageSourceConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
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
            var converter = new FileImageSourceConverter();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// </summary>
        /// <param name="destinationType">The destination type to test.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(FileImageSource))]
        [InlineData(typeof(TypeConverter))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new FileImageSourceConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo behavior is not affected by the context parameter.
        /// </summary>
        /// <param name="destinationType">The destination type to test.</param>
        /// <param name="expected">The expected result.</param>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        public void CanConvertTo_WithNonNullContext_ReturnsExpectedResult(Type destinationType, bool expected)
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            object value = null;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, value));

            Assert.Equal("Cannot convert \"\" into Microsoft.Maui.Controls.FileImageSource", exception.Message);
        }

        [Theory]
        [InlineData("test.png")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("path/to/image.jpg")]
        [InlineData("C:\\Windows\\image.bmp")]
        [InlineData("/usr/local/image.gif")]
        [InlineData("image with spaces.png")]
        [InlineData("image@#$%^&*().png")]
        public void ConvertFrom_StringValue_ReturnsFileImageSourceWithCorrectFile(string fileName)
        {
            // Arrange
            var converter = new FileImageSourceConverter();

            // Act
            var result = converter.ConvertFrom(null, null, fileName);

            // Assert
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(fileName, fileImageSource.File);
        }

        [Fact]
        public void ConvertFrom_NonStringObject_ReturnsFileImageSourceWithToStringResult()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var value = 12345;

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal("12345", fileImageSource.File);
        }

        [Fact]
        public void ConvertFrom_ObjectWithToStringReturningNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var value = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, value));

            Assert.Equal("Cannot convert \"\" into Microsoft.Maui.Controls.FileImageSource", exception.Message);
        }

        [Fact]
        public void ConvertFrom_WithContextAndCulture_ReturnsFileImageSource()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var fileName = "test.png";

            // Act
            var result = converter.ConvertFrom(context, culture, fileName);

            // Assert
            Assert.IsType<FileImageSource>(result);
            var fileImageSource = (FileImageSource)result;
            Assert.Equal(fileName, fileImageSource.File);
        }

        /// <summary>
        /// Helper class that returns null from ToString() to test edge case
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString() => null;
        }

        [Fact]
        public void ConvertTo_ValidFileImageSourceWithFile_ReturnsFile()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var fileImageSource = new FileImageSource { File = "test.png" };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Equal("test.png", result);
        }

        [Fact]
        public void ConvertTo_ValidFileImageSourceWithNullFile_ReturnsNull()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var fileImageSource = new FileImageSource { File = null };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ConvertTo_ValidFileImageSourceWithEmptyFile_ReturnsEmpty()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var fileImageSource = new FileImageSource { File = string.Empty };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ConvertTo_ValidFileImageSourceWithWhitespaceFile_ReturnsWhitespace()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var fileImageSource = new FileImageSource { File = "   " };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Equal("   ", result);
        }

        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, destinationType));
        }

        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(true)]
        public void ConvertTo_WrongTypeValue_ThrowsNotSupportedException(object wrongTypeValue)
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, wrongTypeValue, destinationType));
        }

        [Fact]
        public void ConvertTo_WithNonNullContextAndCulture_ReturnsFileFromFileImageSource()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var fileImageSource = new FileImageSource { File = "context_test.jpg" };
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("en-US");
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, fileImageSource, destinationType);

            // Assert
            Assert.Equal("context_test.jpg", result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        public void ConvertTo_DifferentDestinationTypes_ReturnsFileFromFileImageSource(Type destinationType)
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var fileImageSource = new FileImageSource { File = "destination_test.gif" };

            // Act
            var result = converter.ConvertTo(null, null, fileImageSource, destinationType);

            // Assert
            Assert.Equal("destination_test.gif", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string type.
        /// This verifies the converter can convert from string to FileImageSource.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var sourceType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// This verifies the converter only supports conversion from string.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(FileImageSource))]
        [InlineData(typeof(ImageSource))]
        [InlineData(typeof(Type))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context is provided.
        /// This verifies that a non-null context doesn't affect the conversion logic.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithMockedContext_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context is provided.
        /// This verifies that a non-null context doesn't affect the conversion logic for non-string types.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithMockedContext_NonStringType_ReturnsFalse()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(int);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws NullReferenceException when sourceType is null.
        /// This verifies proper exception handling for invalid input.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = new FileImageSourceConverter();
            Type sourceType = null;
            ITypeDescriptorContext context = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(context, sourceType));
        }
    }
}