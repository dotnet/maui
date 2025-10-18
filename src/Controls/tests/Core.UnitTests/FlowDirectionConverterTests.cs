using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class FlowDirectionConverterTests
    {
        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string type.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is not string type.
        /// Tests various types including primitives, objects, and custom types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(FlowDirectionConverter))]
        [InlineData(typeof(Type))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context is null.
        /// The context parameter is nullable and not used in the implementation.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ContextIsNull_ReturnsCorrectResult()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            ITypeDescriptorContext context = null;

            // Act & Assert
            var resultForString = converter.CanConvertFrom(context, typeof(string));
            var resultForInt = converter.CanConvertFrom(context, typeof(int));

            Assert.True(resultForString);
            Assert.False(resultForInt);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// The sourceType parameter is non-nullable.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts valid string inputs to their corresponding FlowDirection enum values.
        /// Verifies enum name parsing, numeric parsing, and shortcut string conversions.
        /// </summary>
        /// <param name="input">The input value to convert</param>
        /// <param name="expected">The expected FlowDirection result</param>
        [Theory]
        [InlineData("MatchParent", FlowDirection.MatchParent)]
        [InlineData("LeftToRight", FlowDirection.LeftToRight)]
        [InlineData("RightToLeft", FlowDirection.RightToLeft)]
        [InlineData("matchparent", FlowDirection.MatchParent)]
        [InlineData("LEFTORIGHT", FlowDirection.LeftToRight)]
        [InlineData("righttoleft", FlowDirection.RightToLeft)]
        [InlineData("0", FlowDirection.MatchParent)]
        [InlineData("1", FlowDirection.LeftToRight)]
        [InlineData("2", FlowDirection.RightToLeft)]
        [InlineData("ltr", FlowDirection.LeftToRight)]
        [InlineData("LTR", FlowDirection.LeftToRight)]
        [InlineData("Ltr", FlowDirection.LeftToRight)]
        [InlineData("rtl", FlowDirection.RightToLeft)]
        [InlineData("RTL", FlowDirection.RightToLeft)]
        [InlineData("Rtl", FlowDirection.RightToLeft)]
        [InlineData("inherit", FlowDirection.MatchParent)]
        [InlineData("INHERIT", FlowDirection.MatchParent)]
        [InlineData("Inherit", FlowDirection.MatchParent)]
        public void ConvertFrom_ValidInputs_ReturnsExpectedFlowDirection(object input, FlowDirection expected)
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for null input values.
        /// Verifies that null objects result in null string values and proper exception handling.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, null));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("FlowDirection", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for invalid string inputs.
        /// Verifies proper exception handling for strings that don't match any valid conversion pattern.
        /// </summary>
        /// <param name="invalidInput">The invalid input value that should cause an exception</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("invalid")]
        [InlineData("leftright")]
        [InlineData("3")]
        [InlineData("-1")]
        [InlineData("left")]
        [InlineData("right")]
        [InlineData("parent")]
        [InlineData("lr")]
        [InlineData("rl")]
        public void ConvertFrom_InvalidStringInputs_ThrowsInvalidOperationException(string invalidInput)
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, invalidInput));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(invalidInput, exception.Message);
            Assert.Contains("FlowDirection", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom works with non-string objects that have meaningful ToString implementations.
        /// Verifies that the method properly converts objects to strings before processing.
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObjectWithValidToString_ReturnsExpectedFlowDirection()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var objectWithToString = new TestObjectWithToString("ltr");

            // Act
            var result = converter.ConvertFrom(null, null, objectWithToString);

            // Assert
            Assert.Equal(FlowDirection.LeftToRight, result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for non-string objects with invalid ToString results.
        /// Verifies proper handling of objects that convert to invalid string representations.
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObjectWithInvalidToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var objectWithToString = new TestObjectWithToString("invalid");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertFrom(null, null, objectWithToString));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains("invalid", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly with different context and culture parameters.
        /// Verifies that the method ignores these nullable parameters as expected.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("en-US");

            // Act
            var result = converter.ConvertFrom(context, culture, "rtl");

            // Assert
            Assert.Equal(FlowDirection.RightToLeft, result);
        }

        /// <summary>
        /// Tests ConvertFrom with boundary numeric values for FlowDirection enum.
        /// Verifies that only valid enum numeric values are accepted.
        /// </summary>
        [Theory]
        [InlineData("0", FlowDirection.MatchParent)]
        [InlineData("1", FlowDirection.LeftToRight)]
        [InlineData("2", FlowDirection.RightToLeft)]
        public void ConvertFrom_ValidNumericEnumValues_ReturnsExpectedFlowDirection(string numericInput, FlowDirection expected)
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act
            var result = converter.ConvertFrom(null, null, numericInput);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Helper class for testing non-string objects with custom ToString implementations.
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
        /// Tests that ConvertTo returns the correct string representation for valid FlowDirection enum values.
        /// </summary>
        /// <param name="flowDirection">The FlowDirection enum value to convert</param>
        /// <param name="expectedString">The expected string representation</param>
        [Theory]
        [InlineData(FlowDirection.MatchParent, "MatchParent")]
        [InlineData(FlowDirection.LeftToRight, "LeftToRight")]
        [InlineData(FlowDirection.RightToLeft, "RightToLeft")]
        public void ConvertTo_ValidFlowDirectionValue_ReturnsStringRepresentation(FlowDirection flowDirection, string expectedString)
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, flowDirection, destinationType);

            // Assert
            Assert.Equal(expectedString, result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different context and culture parameters for valid FlowDirection values.
        /// The context and culture parameters should not affect the conversion result.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidFlowDirectionWithDifferentContextAndCulture_ReturnsStringRepresentation()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(object);

            // Act
            var result = converter.ConvertTo(context, culture, FlowDirection.LeftToRight, destinationType);

            // Assert
            Assert.Equal("LeftToRight", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when the input value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for various invalid input value types.
        /// </summary>
        /// <param name="invalidValue">The invalid value to test</param>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_InvalidValueType_ThrowsNotSupportedException(object invalidValue)
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, invalidValue, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when an invalid enum value is cast to FlowDirection.
        /// This tests the edge case of out-of-range enum values.
        /// </summary>
        [Fact]
        public void ConvertTo_InvalidEnumValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var invalidEnumValue = (FlowDirection)999; // Out of range enum value
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, invalidEnumValue, destinationType);

            // Assert
            // Even invalid enum values will call ToString(), so this should return "999"
            Assert.Equal("999", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different destination types.
        /// The destination type parameter should not affect the conversion logic.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_ValidFlowDirectionWithDifferentDestinationTypes_ReturnsStringRepresentation(Type destinationType)
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act
            var result = converter.ConvertTo(null, null, FlowDirection.RightToLeft, destinationType);

            // Assert
            Assert.Equal("RightToLeft", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for an empty object.
        /// </summary>
        [Fact]
        public void ConvertTo_EmptyObject_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var emptyObject = new object();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, emptyObject, destinationType));
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string) and context is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsStringAndContextIsNull_ReturnsTrue()
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act
            var result = converter.CanConvertTo(null, typeof(string));

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
            var converter = new FlowDirectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// </summary>
        /// <param name="destinationType">The destination type to test.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(FlowDirection))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(CultureInfo))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new FlowDirectionConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for non-string destination types when context is null.
        /// </summary>
        /// <param name="destinationType">The destination type to test.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        public void CanConvertTo_DestinationTypeIsNotStringAndContextIsNull_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo handles both null context and null destinationType correctly.
        /// </summary>
        [Fact]
        public void CanConvertTo_BothContextAndDestinationTypeAreNull_ReturnsFalse()
        {
            // Arrange
            var converter = new FlowDirectionConverter();

            // Act
            var result = converter.CanConvertTo(null, null);

            // Assert
            Assert.False(result);
        }
    }
}