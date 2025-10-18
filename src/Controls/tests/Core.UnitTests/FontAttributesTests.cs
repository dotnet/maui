#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class FontAttributesConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when the destination type is string.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Verifies the method correctly identifies unsupported conversion types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(FontAttributes))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(Type))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo works correctly when context is null.
        /// Verifies that the context parameter is not required for the method to function.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContext_WorksCorrectly()
        {
            // Arrange
            var converter = new FontAttributesConverter();
            ITypeDescriptorContext context = null;
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo throws NullReferenceException when destination type is null.
        /// Verifies that the method does not handle null destination type gracefully.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertTo(context, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not FontAttributes.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(123)]
        [InlineData(true)]
        [InlineData(12.34)]
        public void ConvertTo_NonFontAttributesValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo returns empty string when FontAttributes is None.
        /// </summary>
        [Fact]
        public void ConvertTo_FontAttributesNone_ReturnsEmptyString()
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act
            var result = converter.ConvertTo(null, null, FontAttributes.None, typeof(string));

            // Assert
            Assert.Equal("", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string representation for FontAttributes values.
        /// </summary>
        [Theory]
        [InlineData(FontAttributes.Bold, "Bold")]
        [InlineData(FontAttributes.Italic, "Italic")]
        [InlineData(FontAttributes.Bold | FontAttributes.Italic, "Bold' Italic")]
        public void ConvertTo_ValidFontAttributes_ReturnsCorrectString(FontAttributes fontAttributes, string expected)
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act
            var result = converter.ConvertTo(null, null, fontAttributes, typeof(string));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo works with different context values.
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentContextValues_WorksCorrectly()
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var mockContext = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var resultWithNull = converter.ConvertTo(null, null, FontAttributes.Bold, typeof(string));
            var resultWithContext = converter.ConvertTo(mockContext, null, FontAttributes.Bold, typeof(string));

            // Assert
            Assert.Equal("Bold", resultWithNull);
            Assert.Equal("Bold", resultWithContext);
        }

        /// <summary>
        /// Tests that ConvertTo works with different culture values.
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentCultureValues_WorksCorrectly()
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var culture = new CultureInfo("en-US");

            // Act
            var resultWithNull = converter.ConvertTo(null, null, FontAttributes.Italic, typeof(string));
            var resultWithCulture = converter.ConvertTo(null, culture, FontAttributes.Italic, typeof(string));

            // Assert
            Assert.Equal("Italic", resultWithNull);
            Assert.Equal("Italic", resultWithCulture);
        }

        /// <summary>
        /// Tests that ConvertTo works with different destination type values.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        public void ConvertTo_DifferentDestinationTypes_WorksCorrectly(Type destinationType)
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act
            var result = converter.ConvertTo(null, null, FontAttributes.Bold, destinationType);

            // Assert
            Assert.Equal("Bold", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles invalid enum values by casting integers.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(999)]
        public void ConvertTo_InvalidEnumValues_WorksWithCastValues(int invalidValue)
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var fontAttributes = (FontAttributes)invalidValue;

            // Act
            var result = converter.ConvertTo(null, null, fontAttributes, typeof(string));

            // Assert
            // For invalid enum values, the method should still work and return based on flag matching
            Assert.IsType<string>(result);
        }

        /// <summary>
        /// Tests that ConvertTo with null destination type works correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_NullDestinationType_WorksCorrectly()
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act
            var result = converter.ConvertTo(null, null, FontAttributes.Bold, null);

            // Assert
            Assert.Equal("Bold", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string) and false for all other types.
        /// Verifies the method correctly identifies string as the only supported source type for conversion.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion support for</param>
        /// <param name="expected">The expected result indicating whether conversion is supported</param>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(double), false)]
        [InlineData(typeof(FontAttributes), false)]
        [InlineData(typeof(char), false)]
        [InlineData(typeof(byte), false)]
        [InlineData(typeof(DateTime), false)]
        [InlineData(typeof(Guid), false)]
        [InlineData(typeof(string[]), false)]
        [InlineData(typeof(List<string>), false)]
        [InlineData(typeof(IEnumerable), false)]
        [InlineData(typeof(ITypeDescriptorContext), false)]
        public void CanConvertFrom_WithVariousSourceTypes_ReturnsExpectedResult(Type sourceType, bool expected)
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with null context parameter.
        /// Verifies that the context parameter is ignored and doesn't affect the conversion support determination.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion support for</param>
        /// <param name="expected">The expected result indicating whether conversion is supported</param>
        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(object), false)]
        public void CanConvertFrom_WithNullContext_ReturnsExpectedResult(Type sourceType, bool expected)
        {
            // Arrange
            var converter = new FontAttributesConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// Verifies that the method properly validates the sourceType parameter.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNullSourceType_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new FontAttributesConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(context, null));
        }
    }
}