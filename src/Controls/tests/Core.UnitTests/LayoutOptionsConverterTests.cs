using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the LayoutOptionsConverter class, specifically testing the ConvertFrom method.
    /// </summary>
    public sealed class LayoutOptionsConverterTests
    {
        /// <summary>
        /// Tests that ConvertFrom correctly converts valid LayoutOptions string values to their corresponding LayoutOptions objects.
        /// Covers all standard LayoutOptions values to ensure proper conversion.
        /// </summary>
        /// <param name="input">The string representation of the LayoutOptions value to convert.</param>
        /// <param name="expected">The expected LayoutOptions result after conversion.</param>
        [Theory]
        [InlineData("Start", typeof(LayoutOptions))]
        [InlineData("Center", typeof(LayoutOptions))]
        [InlineData("End", typeof(LayoutOptions))]
        [InlineData("Fill", typeof(LayoutOptions))]
        [InlineData("StartAndExpand", typeof(LayoutOptions))]
        [InlineData("CenterAndExpand", typeof(LayoutOptions))]
        [InlineData("EndAndExpand", typeof(LayoutOptions))]
        [InlineData("FillAndExpand", typeof(LayoutOptions))]
        public void ConvertFrom_ValidLayoutOptionsString_ReturnsCorrectLayoutOptions(string input, Type expectedType)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var expectedValue = GetLayoutOptionsValue(input);

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType(expectedType, result);
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that ConvertFrom correctly converts qualified LayoutOptions string values (prefixed with "LayoutOptions.") 
        /// to their corresponding LayoutOptions objects.
        /// </summary>
        /// <param name="input">The qualified string representation of the LayoutOptions value to convert.</param>
        [Theory]
        [InlineData("LayoutOptions.Start")]
        [InlineData("LayoutOptions.Center")]
        [InlineData("LayoutOptions.End")]
        [InlineData("LayoutOptions.Fill")]
        [InlineData("LayoutOptions.StartAndExpand")]
        [InlineData("LayoutOptions.CenterAndExpand")]
        [InlineData("LayoutOptions.EndAndExpand")]
        [InlineData("LayoutOptions.FillAndExpand")]
        public void ConvertFrom_QualifiedLayoutOptionsString_ReturnsCorrectLayoutOptions(string input)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var optionName = input.Split('.')[1];
            var expectedValue = GetLayoutOptionsValue(optionName);

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when provided with null input.
        /// Validates proper error handling for null values.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, null));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(LayoutOptions).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when provided with empty string input.
        /// Validates proper error handling for empty string values.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, ""));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(LayoutOptions).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when provided with whitespace-only string input.
        /// Validates proper error handling for whitespace values.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void ConvertFrom_WhitespaceString_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(LayoutOptions).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when provided with invalid LayoutOptions string values.
        /// Validates proper error handling for unrecognized option names.
        /// </summary>
        /// <param name="input">The invalid string representation to test.</param>
        [Theory]
        [InlineData("Invalid")]
        [InlineData("UnknownOption")]
        [InlineData("start")] // Case sensitivity test
        [InlineData("CENTER")] // Case sensitivity test
        [InlineData("123")]
        [InlineData("!@#$%")]
        public void ConvertFrom_InvalidLayoutOptionsString_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(input, exception.Message);
            Assert.Contains(typeof(LayoutOptions).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when provided with incorrectly qualified string values.
        /// Validates proper error handling for malformed qualified names.
        /// </summary>
        /// <param name="input">The incorrectly qualified string representation to test.</param>
        [Theory]
        [InlineData("SomeOther.Center")]
        [InlineData("LayoutOptions.Invalid.Center")]
        [InlineData("Multiple.Parts.Here.Center")]
        [InlineData("LayoutOptions.")]
        [InlineData(".Center")]
        public void ConvertFrom_IncorrectlyQualifiedString_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(input, exception.Message);
            Assert.Contains(typeof(LayoutOptions).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles non-string objects by calling their ToString method.
        /// Validates conversion of objects that have meaningful string representations.
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObject_UsesToStringAndConverts()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var objectWithToString = new ObjectWithToString("Center");

            // Act
            var result = converter.ConvertFrom(null, null, objectWithToString);

            // Assert
            Assert.Equal(LayoutOptions.Center, result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException when provided with an object whose ToString returns null.
        /// Validates proper error handling for objects with null string representations.
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithNullToString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var objectWithNullToString = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, objectWithNullToString));
            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(LayoutOptions).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom accepts null context and culture parameters without issues.
        /// Validates that these parameters are properly handled when null.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullContextAndCulture_ConvertsSuccessfully()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result = converter.ConvertFrom(null, null, "Center");

            // Assert
            Assert.Equal(LayoutOptions.Center, result);
        }

        /// <summary>
        /// Tests that ConvertFrom works with provided context and culture parameters.
        /// Validates that the method handles non-null context and culture appropriately.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_ConvertsSuccessfully()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.ConvertFrom(context, culture, "Fill");

            // Assert
            Assert.Equal(LayoutOptions.Fill, result);
        }

        /// <summary>
        /// Helper method to get the expected LayoutOptions value for a given string name.
        /// Uses reflection to retrieve the static field value to ensure test accuracy.
        /// </summary>
        /// <param name="optionName">The name of the LayoutOptions static field.</param>
        /// <returns>The LayoutOptions value corresponding to the given name.</returns>
        private static LayoutOptions GetLayoutOptionsValue(string optionName)
        {
            var field = typeof(LayoutOptions).GetField(optionName, BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                throw new ArgumentException($"LayoutOptions field '{optionName}' not found.");

            var value = field.GetValue(null);
            if (value is not LayoutOptions layoutOptions)
                throw new InvalidOperationException($"Field '{optionName}' is not a LayoutOptions.");

            return layoutOptions;
        }

        /// <summary>
        /// Helper class for testing ConvertFrom with objects that have a meaningful ToString implementation.
        /// </summary>
        private class ObjectWithToString
        {
            private readonly string _value;

            public ObjectWithToString(string value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        /// <summary>
        /// Helper class for testing ConvertFrom with objects that return null from ToString.
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString()
            {
                return null!;
            }
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsNull_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            object value = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not LayoutOptions.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertTo_ValueIsNotLayoutOptions_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for Start alignment without expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_StartAlignmentNoExpand_ReturnsStart()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Start, false);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("Start", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for Start alignment with expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_StartAlignmentWithExpand_ReturnsStartAndExpand()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Start, true);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("StartAndExpand", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for Center alignment without expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_CenterAlignmentNoExpand_ReturnsCenter()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Center, false);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("Center", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for Center alignment with expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_CenterAlignmentWithExpand_ReturnsCenterAndExpand()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Center, true);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("CenterAndExpand", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for End alignment without expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_EndAlignmentNoExpand_ReturnsEnd()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.End, false);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("End", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for End alignment with expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_EndAlignmentWithExpand_ReturnsEndAndExpand()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.End, true);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("EndAndExpand", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for Fill alignment without expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_FillAlignmentNoExpand_ReturnsFill()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Fill, false);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("Fill", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correct string for Fill alignment with expansion.
        /// </summary>
        [Fact]
        public void ConvertTo_FillAlignmentWithExpand_ReturnsFillAndExpand()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Fill, true);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("FillAndExpand", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for invalid alignment values.
        /// </summary>
        [Fact]
        public void ConvertTo_InvalidAlignment_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            // Create LayoutOptions with invalid alignment by manipulating the Alignment property
            var layoutOptions = new LayoutOptions(LayoutAlignment.Start, false);
            layoutOptions.Alignment = (LayoutAlignment)999; // Invalid alignment value

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, layoutOptions, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo works regardless of context parameter value.
        /// </summary>
        [Fact]
        public void ConvertTo_ContextParameter_DoesNotAffectResult()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Start, false);
            var mockContext = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.ConvertTo(mockContext, null, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("Start", result);
        }

        /// <summary>
        /// Tests that ConvertTo works regardless of culture parameter value.
        /// </summary>
        [Fact]
        public void ConvertTo_CultureParameter_DoesNotAffectResult()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Center, true);
            var culture = new CultureInfo("fr-FR");

            // Act
            var result = converter.ConvertTo(null, culture, layoutOptions, typeof(string));

            // Assert
            Assert.Equal("CenterAndExpand", result);
        }

        /// <summary>
        /// Tests that ConvertTo works regardless of destinationType parameter value.
        /// </summary>
        [Fact]
        public void ConvertTo_DestinationTypeParameter_DoesNotAffectResult()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var layoutOptions = new LayoutOptions(LayoutAlignment.Fill, false);

            // Act
            var result = converter.ConvertTo(null, null, layoutOptions, typeof(object));

            // Assert
            Assert.Equal("Fill", result);
        }

        /// <summary>
        /// Tests that GetStandardValuesSupported returns true when context is null.
        /// This verifies the converter supports providing standard values regardless of context.
        /// </summary>
        [Fact]
        public void GetStandardValuesSupported_WithNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            bool result = converter.GetStandardValuesSupported(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesSupported returns true when context is provided.
        /// This verifies the converter supports providing standard values with a valid context.
        /// </summary>
        [Fact]
        public void GetStandardValuesSupported_WithValidContext_ReturnsTrue()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = converter.GetStandardValuesSupported(context);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests GetStandardValues with null context parameter.
        /// Verifies that the method returns the expected StandardValuesCollection with all layout option strings.
        /// </summary>
        [Fact]
        public void GetStandardValues_NullContext_ReturnsExpectedCollection()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result = converter.GetStandardValues(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.Count);

            var values = result.Cast<string>().ToArray();
            Assert.Equal("Start", values[0]);
            Assert.Equal("Center", values[1]);
            Assert.Equal("End", values[2]);
            Assert.Equal("Fill", values[3]);
            Assert.Equal("StartAndExpand", values[4]);
            Assert.Equal("CenterAndExpand", values[5]);
            Assert.Equal("EndAndExpand", values[6]);
            Assert.Equal("FillAndExpand", values[7]);
        }

        /// <summary>
        /// Tests GetStandardValues with a mocked context parameter.
        /// Verifies that the context parameter does not affect the returned values.
        /// </summary>
        [Fact]
        public void GetStandardValues_WithContext_ReturnsExpectedCollection()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var mockContext = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.GetStandardValues(mockContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.Count);

            var values = result.Cast<string>().ToArray();
            Assert.Equal("Start", values[0]);
            Assert.Equal("Center", values[1]);
            Assert.Equal("End", values[2]);
            Assert.Equal("Fill", values[3]);
            Assert.Equal("StartAndExpand", values[4]);
            Assert.Equal("CenterAndExpand", values[5]);
            Assert.Equal("EndAndExpand", values[6]);
            Assert.Equal("FillAndExpand", values[7]);
        }

        /// <summary>
        /// Tests that GetStandardValues returns consistent results across multiple calls.
        /// Verifies that the method behavior is deterministic.
        /// </summary>
        [Fact]
        public void GetStandardValues_MultipleCalls_ReturnsConsistentResults()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result1 = converter.GetStandardValues(null);
            var result2 = converter.GetStandardValues(null);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(result1.Count, result2.Count);

            var values1 = result1.Cast<string>().ToArray();
            var values2 = result2.Cast<string>().ToArray();

            Assert.True(values1.SequenceEqual(values2));
        }

        /// <summary>
        /// Tests that GetStandardValues returns a StandardValuesCollection containing only string values.
        /// Verifies that all items in the collection are of the expected type.
        /// </summary>
        [Fact]
        public void GetStandardValues_ReturnsCollectionOfStrings()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result = converter.GetStandardValues(null);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Cast<object>(), item => Assert.IsType<string>(item));
        }

        /// <summary>
        /// Tests that GetStandardValues returns a collection containing all expected layout option values.
        /// Verifies the complete set of standard values without regard to order.
        /// </summary>
        [Fact]
        public void GetStandardValues_ContainsAllExpectedValues()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var expectedValues = new[]
            {
                "Start", "Center", "End", "Fill",
                "StartAndExpand", "CenterAndExpand", "EndAndExpand", "FillAndExpand"
            };

            // Act
            var result = converter.GetStandardValues(null);

            // Assert
            Assert.NotNull(result);
            var actualValues = result.Cast<string>().ToArray();

            foreach (var expectedValue in expectedValues)
            {
                Assert.Contains(expectedValue, actualValues);
            }
        }

        /// <summary>
        /// Tests that GetStandardValues returns the correct type of collection.
        /// Verifies that the return type is StandardValuesCollection as expected by the framework.
        /// </summary>
        [Fact]
        public void GetStandardValues_ReturnsStandardValuesCollection()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result = converter.GetStandardValues(null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TypeConverter.StandardValuesCollection>(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesExclusive returns false when context is null,
        /// indicating that values other than standard ones are also valid.
        /// </summary>
        [Fact]
        public void GetStandardValuesExclusive_WithNullContext_ReturnsFalse()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.GetStandardValuesExclusive(context);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesExclusive returns false when context is provided,
        /// indicating that values other than standard ones are also valid.
        /// </summary>
        [Fact]
        public void GetStandardValuesExclusive_WithValidContext_ReturnsFalse()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.GetStandardValuesExclusive(context);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when both context and destinationType are null.
        /// Verifies the method handles null parameters correctly.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContextAndNullDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result = converter.CanConvertTo(null, null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when context is null and destinationType is a valid type.
        /// Verifies the method handles null context with various destination types.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(LayoutOptions))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        public void CanConvertTo_NullContextWithValidDestinationType_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when context is provided and destinationType is null.
        /// Verifies the method handles valid context with null destination type.
        /// </summary>
        [Fact]
        public void CanConvertTo_ValidContextWithNullDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when both context and destinationType are provided.
        /// Verifies the method handles various combinations of valid parameters.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(LayoutOptions))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(char))]
        [InlineData(typeof(decimal))]
        public void CanConvertTo_ValidContextWithValidDestinationType_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new LayoutOptionsConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }
    }
}