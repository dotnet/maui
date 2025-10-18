#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for BindablePropertyConverter class.
    /// </summary>
    public sealed class BindablePropertyConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when both context and destinationType are null.
        /// This verifies the method handles null inputs without throwing exceptions.
        /// Expected result: Returns true.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContextAndNullDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for various combinations of context and destination types.
        /// This verifies the method consistently returns true regardless of input parameters.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(null, typeof(string))]
        [InlineData(null, typeof(int))]
        [InlineData(null, typeof(object))]
        [InlineData(null, typeof(BindableProperty))]
        public void CanConvertTo_NullContextWithVariousDestinationTypes_ReturnsTrue(ITypeDescriptorContext context, Type destinationType)
        {
            // Arrange
            var converter = new BindablePropertyConverter();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when context is provided and destinationType is null.
        /// This verifies the method handles null destination type without throwing exceptions.
        /// Expected result: Returns true.
        /// </summary>
        [Fact]
        public void CanConvertTo_ValidContextAndNullDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for valid context with various destination types.
        /// This verifies the method consistently returns true with valid inputs.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(BindableProperty))]
        [InlineData(typeof(Type))]
        public void CanConvertTo_ValidContextWithVariousDestinationTypes_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for complex and generic types.
        /// This verifies the method handles complex type scenarios consistently.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, object>))]
        [InlineData(typeof(System.Nullable<int>))]
        [InlineData(typeof(System.Action<string>))]
        public void CanConvertTo_ComplexAndGenericTypes_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns null when value parameter is null
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ReturnsNull()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = null;

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns null when value converts to empty string
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsNull()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = "";

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns null when value converts to whitespace-only string
        /// </summary>
        [Theory]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData(" \t \n ")]
        public void ConvertFrom_WhitespaceString_ReturnsNull(string whitespaceValue)
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = whitespaceValue;

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns null when string contains colon (namespace prefix)
        /// </summary>
        [Theory]
        [InlineData("x:Name")]
        [InlineData("xmlns:x")]
        [InlineData("prefix:PropertyName")]
        [InlineData("a:b:c")]
        public void ConvertFrom_StringWithColon_ReturnsNull(string valueWithColon)
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = valueWithColon;

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns null when string has incorrect number of parts after splitting by dot
        /// </summary>
        [Theory]
        [InlineData("SinglePart")]
        [InlineData("One.Two.Three")]
        [InlineData("One.Two.Three.Four")]
        [InlineData("")]
        public void ConvertFrom_IncorrectPartCount_ReturnsNull(string incorrectValue)
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = incorrectValue;

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertFrom handles non-string objects by converting them to string
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObject_ConvertsToString()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = 123; // This will be converted to "123" which has only 1 part

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.Null(result); // Should return null because "123" doesn't have 2 parts
        }

        /// <summary>
        /// Tests that ConvertFrom processes valid format but with invalid type name
        /// This should result in GetControlType returning null and ConvertFrom method being called with null type
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidFormatInvalidType_ThrowsException()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = "NonExistentType.SomeProperty";

            // Act & Assert
            // GetControlType will return null for invalid type name
            // Then ConvertFrom(null, "SomeProperty", null) will be called which should throw
            var exception = Assert.Throws<NullReferenceException>(() =>
                converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests ConvertFrom with object that returns null from ToString()
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithNullToString_ReturnsNull()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var objectWithNullToString = new ObjectWithNullToString();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, objectWithNullToString);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Helper class that returns null from ToString method
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString()
            {
                return null;
            }
        }

        /// <summary>
        /// Tests that ConvertTo returns the correct formatted string when given a valid BindableProperty.
        /// Input: Valid BindableProperty with known DeclaringType and PropertyName
        /// Expected: String in format "{DeclaringType.Name}.{PropertyName}"
        /// </summary>
        [Fact]
        public void ConvertTo_ValidBindableProperty_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var bindableProperty = BindableProperty.Create("TestProperty", typeof(string), typeof(BindablePropertyConverterTests), "defaultValue");
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, bindableProperty, destinationType);

            // Assert
            Assert.Equal("BindablePropertyConverterTests.TestProperty", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given null value.
        /// Input: null value
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            object value = null;
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given a string value.
        /// Input: string value instead of BindableProperty
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_StringValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            object value = "not a bindable property";
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given an integer value.
        /// Input: integer value instead of BindableProperty
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_IntegerValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            object value = 42;
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given an arbitrary object.
        /// Input: object instance instead of BindableProperty
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_ObjectValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            object value = new object();
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns correct format for BindableProperty with different declaring type.
        /// Input: BindableProperty with different DeclaringType (int)
        /// Expected: String with correct type name
        /// </summary>
        [Fact]
        public void ConvertTo_BindablePropertyWithDifferentDeclaringType_ReturnsCorrectFormat()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var bindableProperty = BindableProperty.Create("IntProperty", typeof(int), typeof(int), 0);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, bindableProperty, destinationType);

            // Assert
            Assert.Equal("Int32.IntProperty", result);
        }

        /// <summary>
        /// Tests that ConvertTo works when context is null.
        /// Input: Valid BindableProperty with null context
        /// Expected: Correct formatted string (context parameter is unused)
        /// </summary>
        [Fact]
        public void ConvertTo_NullContext_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var bindableProperty = BindableProperty.Create("ContextTestProperty", typeof(bool), typeof(bool), false);
            ITypeDescriptorContext context = null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, bindableProperty, destinationType);

            // Assert
            Assert.Equal("Boolean.ContextTestProperty", result);
        }

        /// <summary>
        /// Tests that ConvertTo works when culture is null.
        /// Input: Valid BindableProperty with null culture
        /// Expected: Correct formatted string (culture parameter is unused)
        /// </summary>
        [Fact]
        public void ConvertTo_NullCulture_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var bindableProperty = BindableProperty.Create("CultureTestProperty", typeof(double), typeof(double), 0.0);
            var context = (ITypeDescriptorContext)null;
            CultureInfo culture = null;
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, bindableProperty, destinationType);

            // Assert
            Assert.Equal("Double.CultureTestProperty", result);
        }

        /// <summary>
        /// Tests that ConvertTo works regardless of destination type.
        /// Input: Valid BindableProperty with different destination type (int instead of string)
        /// Expected: Correct formatted string (destinationType parameter is unused)
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentDestinationType_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var bindableProperty = BindableProperty.Create("DestTypeProperty", typeof(float), typeof(float), 0.0f);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(int);

            // Act
            var result = converter.ConvertTo(context, culture, bindableProperty, destinationType);

            // Assert
            Assert.Equal("Single.DestTypeProperty", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string type.
        /// This verifies the converter can handle string inputs as expected.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is null.
        /// This ensures null safety in type comparison.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// This validates that only string type conversions are supported.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(BindableProperty))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when context is null.
        /// This verifies that the context parameter is not required for the operation.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ContextIsNull_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly when both context is null and sourceType is not string.
        /// This ensures proper behavior with multiple null/invalid inputs.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ContextIsNullAndSourceTypeIsNotString_ReturnsFalse()
        {
            // Arrange
            var converter = new BindablePropertyConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(int);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}