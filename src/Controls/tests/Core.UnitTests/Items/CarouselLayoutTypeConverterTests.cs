using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for CarouselLayoutTypeConverter class.
    /// </summary>
    public class CarouselLayoutTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

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
            var converter = new CarouselLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(CarouselLayoutTypeConverter))]
        [InlineData(typeof(Type))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for string type regardless of context parameter value.
        /// </summary>
        [Fact]
        public void CanConvertTo_ContextIsNull_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            ITypeDescriptorContext context = null;
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for non-string type regardless of context parameter value.
        /// </summary>
        [Fact]
        public void CanConvertTo_ContextIsNull_NonStringDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            ITypeDescriptorContext context = null;
            var destinationType = typeof(int);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns the correct LinearItemsLayout for valid horizontal list input.
        /// Input: "HorizontalList" string value
        /// Expected: Returns LinearItemsLayout.CarouselDefault
        /// </summary>
        [Fact]
        public void ConvertFrom_HorizontalListString_ReturnsCarouselDefault()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            const string input = "HorizontalList";

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Same(LinearItemsLayout.CarouselDefault, result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns the correct LinearItemsLayout for valid vertical list input.
        /// Input: "VerticalList" string value
        /// Expected: Returns LinearItemsLayout.CarouselVertical
        /// </summary>
        [Fact]
        public void ConvertFrom_VerticalListString_ReturnsCarouselVertical()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            const string input = "VerticalList";

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Same(LinearItemsLayout.CarouselVertical, result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for invalid string inputs.
        /// Input: Various invalid string values
        /// Expected: Throws InvalidOperationException with appropriate message
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("horizontallist")]
        [InlineData("HORIZONTALLIST")]
        [InlineData("HorizontalList ")]
        [InlineData(" HorizontalList")]
        [InlineData("verticallist")]
        [InlineData("VERTICALLIST")]
        [InlineData("VerticalList ")]
        [InlineData(" VerticalList")]
        [InlineData("Invalid")]
        [InlineData("SomeOtherValue")]
        [InlineData("Grid")]
        [InlineData("0")]
        [InlineData("true")]
        public void ConvertFrom_InvalidStringValues_ThrowsInvalidOperationException(string invalidValue)
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, invalidValue));
            Assert.Contains($"Cannot convert \"{invalidValue}\" into {typeof(LinearItemsLayout)}", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when value is null.
        /// Input: null object value
        /// Expected: Throws InvalidOperationException with null in message
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, null));
            Assert.Contains($"Cannot convert \"\" into {typeof(LinearItemsLayout)}", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for non-string object inputs.
        /// Input: Various non-string object values
        /// Expected: Throws InvalidOperationException with ToString() representation in message
        /// </summary>
        [Theory]
        [InlineData(123)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(45.67)]
        public void ConvertFrom_NonStringObjects_ThrowsInvalidOperationException(object nonStringValue)
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            var expectedStringValue = nonStringValue.ToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, nonStringValue));
            Assert.Contains($"Cannot convert \"{expectedStringValue}\" into {typeof(LinearItemsLayout)}", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles object with custom ToString() method correctly.
        /// Input: Object with custom ToString() returning invalid value
        /// Expected: Throws InvalidOperationException with custom ToString() value in message
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithCustomToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            var customObject = new CustomToStringObject();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, customObject));
            Assert.Contains($"Cannot convert \"CustomValue\" into {typeof(LinearItemsLayout)}", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly with valid inputs regardless of context and culture parameters.
        /// Input: Valid "HorizontalList" with non-null context and culture
        /// Expected: Returns LinearItemsLayout.CarouselDefault (parameters should not affect result)
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_ReturnsCorrectResult()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            var culture = CultureInfo.InvariantCulture;
            const string input = "HorizontalList";

            // Act
            var result = converter.ConvertFrom(null, culture, input);

            // Assert
            Assert.Same(LinearItemsLayout.CarouselDefault, result);
        }

        private class CustomToStringObject
        {
            public override string ToString() => "CustomValue";
        }
        private readonly CarouselLayoutTypeConverter _converter;

        public CarouselLayoutTypeConverterTests()
        {
            _converter = new CarouselLayoutTypeConverter();
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
        /// Tests that ConvertTo throws NotSupportedException when value is not a LinearItemsLayout.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_NonLinearItemsLayoutValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns "HorizontalList" when value is CarouselDefault.
        /// </summary>
        [Fact]
        public void ConvertTo_CarouselDefaultValue_ReturnsHorizontalList()
        {
            // Arrange
            var value = LinearItemsLayout.CarouselDefault;
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, value, destinationType);

            // Assert
            Assert.Equal("HorizontalList", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns "VerticalList" when value is CarouselVertical.
        /// </summary>
        [Fact]
        public void ConvertTo_CarouselVerticalValue_ReturnsVerticalList()
        {
            // Arrange
            var value = LinearItemsLayout.CarouselVertical;
            Type destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(null, null, value, destinationType);

            // Assert
            Assert.Equal("VerticalList", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is a LinearItemsLayout that is not CarouselDefault or CarouselVertical.
        /// </summary>
        [Fact]
        public void ConvertTo_OtherLinearItemsLayoutValue_ThrowsNotSupportedException()
        {
            // Arrange
            var value = LinearItemsLayout.Vertical;
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is a new LinearItemsLayout instance.
        /// </summary>
        [Fact]
        public void ConvertTo_NewLinearItemsLayoutInstance_ThrowsNotSupportedException()
        {
            // Arrange
            var value = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo works with various context and culture parameters.
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "en-US")]
        public void ConvertTo_VariousContextAndCulture_ReturnsExpectedResult(ITypeDescriptorContext context, string cultureName)
        {
            // Arrange
            var value = LinearItemsLayout.CarouselDefault;
            Type destinationType = typeof(string);
            CultureInfo culture = cultureName != null ? new CultureInfo(cultureName) : null;

            // Act
            var result = _converter.ConvertTo(context, culture, value, destinationType);

            // Assert
            Assert.Equal("HorizontalList", result);
        }

        /// <summary>
        /// Tests that ConvertTo works with different destination types (though the method doesn't seem to use this parameter).
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        public void ConvertTo_DifferentDestinationTypes_ReturnsExpectedResult(Type destinationType)
        {
            // Arrange
            var value = LinearItemsLayout.CarouselVertical;

            // Act
            var result = _converter.ConvertTo(null, null, value, destinationType);

            // Assert
            Assert.Equal("VerticalList", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when the source type is string,
        /// regardless of the context parameter value.
        /// </summary>
        /// <param name="context">The type descriptor context (null or mocked instance)</param>
        [Theory]
        [InlineData(null)]
        [InlineData("mock")]
        public void CanConvertFrom_WithStringType_ReturnsTrue(string contextType)
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            ITypeDescriptorContext context = contextType == "mock" ? Substitute.For<ITypeDescriptorContext>() : null;
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when the source type is not string,
        /// testing various type scenarios including value types, reference types, generic types,
        /// interfaces, and abstract types.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(CarouselLayoutTypeConverter))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, int>))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(TypeConverter))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        public void CanConvertFrom_WithNonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for non-string types even when
        /// a valid context is provided, ensuring the context parameter doesn't
        /// affect the result.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNonStringTypeAndValidContext_ReturnsFalse()
        {
            // Arrange
            var converter = new CarouselLayoutTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(int);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}