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
    public class TextAlignmentConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenDestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not typeof(string).
        /// Validates various type scenarios including primitive types, object types, and framework types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(TextAlignment))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertTo_WhenDestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo works correctly when context parameter is null.
        /// The context parameter is not used in the implementation, so null should be acceptable.
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenContextIsNull_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;

            // Act & Assert - should return true for string type
            var resultForString = converter.CanConvertTo(context, typeof(string));
            Assert.True(resultForString);

            // Act & Assert - should return false for non-string type
            var resultForInt = converter.CanConvertTo(context, typeof(int));
            Assert.False(resultForInt);
        }

        /// <summary>
        /// Tests that CanConvertTo throws ArgumentNullException when destinationType is null.
        /// This tests the behavior when the equality comparison is performed on a null Type.
        /// </summary>
        [Fact]
        public void CanConvertTo_WhenDestinationTypeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertTo(context, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo successfully converts valid TextAlignment enum values to their string representations.
        /// Verifies that Start, Center, and End enum values return the correct nameof strings.
        /// </summary>
        /// <param name="textAlignment">The TextAlignment enum value to convert.</param>
        /// <param name="expectedResult">The expected string result.</param>
        [Theory]
        [InlineData(TextAlignment.Start, "Start")]
        [InlineData(TextAlignment.Center, "Center")]
        [InlineData(TextAlignment.End, "End")]
        public void ConvertTo_ValidTextAlignmentValues_ReturnsCorrectStringName(TextAlignment textAlignment, string expectedResult)
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            Type destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, textAlignment, destinationType);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given a TextAlignment.Justify value.
        /// The method only handles Start, Center, and End values, so Justify should throw an exception.
        /// </summary>
        [Fact]
        public void ConvertTo_TextAlignmentJustify_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, TextAlignment.Justify, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given invalid TextAlignment enum values.
        /// Values outside the defined enum range should be rejected and throw an exception.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid TextAlignment enum value cast from an integer.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ConvertTo_InvalidTextAlignmentEnumValues_ThrowsNotSupportedException(int invalidEnumValue)
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            Type destinationType = typeof(string);
            var invalidTextAlignment = (TextAlignment)invalidEnumValue;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, invalidTextAlignment, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given non-TextAlignment values.
        /// The method should reject any value that is not a TextAlignment enum and throw an exception.
        /// </summary>
        /// <param name="invalidValue">A non-TextAlignment value to test.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("Start")]
        [InlineData("")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(true)]
        [InlineData(3.14)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ConvertTo_NonTextAlignmentValues_ThrowsNotSupportedException(object invalidValue)
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, invalidValue, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different parameter combinations for context, culture, and destinationType.
        /// Since these parameters are not used in the logic, they should not affect the conversion result.
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentParameterCombinations_WorksCorrectly()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var mockContext = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(object);

            // Act
            var result1 = converter.ConvertTo(mockContext, culture, TextAlignment.Start, destinationType);
            var result2 = converter.ConvertTo(null, CultureInfo.CurrentCulture, TextAlignment.Center, typeof(string));
            var result3 = converter.ConvertTo(null, null, TextAlignment.End, null);

            // Assert
            Assert.Equal("Start", result1);
            Assert.Equal("Center", result2);
            Assert.Equal("End", result3);
        }

        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(TextAlignment))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanConvertFrom_ContextIsNull_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanConvertFrom_ContextIsNull_SourceTypeIsNotString_ReturnsFalse()
        {
            // Arrange
            var converter = new TextAlignmentConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(int);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}