#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class VisualTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertTo_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string types.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(IVisual))]
        [InlineData(typeof(VisualTypeConverter))]
        [InlineData(typeof(Type))]
        public void CanConvertTo_NonStringTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns correct result regardless of context value.
        /// The context parameter should not affect the conversion capability.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanConvertTo_ContextVariations_ReturnsExpectedResult(bool useNullContext)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = useNullContext ? null : Substitute.For<ITypeDescriptorContext>();

            // Act & Assert - String type should return true regardless of context
            var stringResult = converter.CanConvertTo(context, typeof(string));
            Assert.True(stringResult);

            // Act & Assert - Non-string type should return false regardless of context
            var intResult = converter.CanConvertTo(context, typeof(int));
            Assert.False(intResult);
        }

        /// <summary>
        /// Tests that CanConvertTo handles generic types correctly.
        /// </summary>
        [Theory]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, int>))]
        [InlineData(typeof(IEnumerable<object>))]
        public void CanConvertTo_GenericTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo handles array types correctly.
        /// </summary>
        [Theory]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(object[]))]
        public void CanConvertTo_ArrayTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws XamlParseException when value is null.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsXamlParseException()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, null));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(IVisual).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws XamlParseException when value converts to null string.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValueWithNullToString_ThrowsXamlParseException()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var objectWithNullToString = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                converter.ConvertFrom(null, CultureInfo.InvariantCulture, objectWithNullToString));

            Assert.Contains("Cannot convert", exception.Message);
            Assert.Contains(typeof(IVisual).ToString(), exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom returns VisualMarker.Default for empty string.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsVisualMarkerDefault()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, string.Empty);

            // Assert
            Assert.Same(VisualMarker.Default, result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns VisualMarker.Default for whitespace-only string.
        /// </summary>
        [Fact]
        public void ConvertFrom_WhitespaceString_ReturnsVisualMarkerDefault()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "   ");

            // Assert
            Assert.Same(VisualMarker.Default, result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns VisualMarker.Default for invalid visual name.
        /// </summary>
        [Fact]
        public void ConvertFrom_InvalidVisualName_ReturnsVisualMarkerDefault()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "NonExistentVisual");

            // Assert
            Assert.Same(VisualMarker.Default, result);
        }

        /// <summary>
        /// Tests that ConvertFrom handles string values correctly and initializes mappings on first call.
        /// </summary>
        [Theory]
        [InlineData("Default")]
        [InlineData("DefaultVisual")]
        public void ConvertFrom_KnownVisualNames_ReturnsCorrectVisual(string visualName)
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, visualName);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IVisual>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom handles non-string objects by calling ToString().
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObject_UsesToStringConversion()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var objectWithToString = new ObjectWithToString("Default");

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, objectWithToString);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IVisual>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom works with different culture info values.
        /// </summary>
        [Fact]
        public void ConvertFrom_DifferentCultureInfo_WorksCorrectly()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var turkishCulture = new CultureInfo("tr-TR");

            // Act
            var result = converter.ConvertFrom(null, turkishCulture, "Default");

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IVisual>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom works with different context values including null.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullContext_WorksCorrectly()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Default");

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IVisual>(result);
        }

        /// <summary>
        /// Tests ConvertFrom with very long string values.
        /// </summary>
        [Fact]
        public void ConvertFrom_VeryLongString_ReturnsVisualMarkerDefault()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var longString = new string('A', 10000);

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, longString);

            // Assert
            Assert.Same(VisualMarker.Default, result);
        }

        /// <summary>
        /// Tests ConvertFrom with strings containing special characters.
        /// </summary>
        [Theory]
        [InlineData("Visual\0WithNull")]
        [InlineData("Visual\nWithNewline")]
        [InlineData("Visual\tWithTab")]
        [InlineData("Visual\r\nWithCRLF")]
        [InlineData("Visual🚀WithEmoji")]
        public void ConvertFrom_StringWithSpecialCharacters_ReturnsVisualMarkerDefault(string specialString)
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, specialString);

            // Assert
            Assert.Same(VisualMarker.Default, result);
        }

        /// <summary>
        /// Helper class for testing objects with custom ToString() returning null.
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString() => null;
        }

        /// <summary>
        /// Helper class for testing objects with custom ToString() returning specific string.
        /// </summary>
        private class ObjectWithToString
        {
            private readonly string _value;

            public ObjectWithToString(string value)
            {
                _value = value;
            }

            public override string ToString() => _value;
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// Input: null value
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsNull_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, null, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not an IVisual.
        /// Input: various non-IVisual types (string, int, object)
        /// Expected: NotSupportedException for all
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        public void ConvertTo_ValueIsNotIVisual_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo returns "default" when value is VisualMarker.Default.
        /// Input: VisualMarker.Default
        /// Expected: "default" string
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsDefaultVisual_ReturnsDefaultString()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var defaultVisual = VisualMarker.Default;

            // Act
            var result = converter.ConvertTo(context, culture, defaultVisual, destinationType);

            // Assert
            Assert.Equal("default", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when IVisual is not in mappings.
        /// Input: custom IVisual implementation not in mappings
        /// Expected: NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsIVisualNotInMappings_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);
            var customVisual = new CustomVisual();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, customVisual, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo behavior is not affected by context, culture, or destinationType parameters.
        /// Input: various parameter combinations with VisualMarker.Default
        /// Expected: "default" string regardless of parameter values
        /// </summary>
        [Theory]
        [InlineData(null, null, null)]
        [InlineData(null, "en-US", typeof(string))]
        [InlineData(null, "fr-FR", typeof(object))]
        public void ConvertTo_ParametersDoNotAffectBehavior_ReturnsExpectedResult(
            ITypeDescriptorContext context,
            string cultureName,
            Type destinationType)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var culture = cultureName != null ? new CultureInfo(cultureName) : null;
            var defaultVisual = VisualMarker.Default;

            // Act
            var result = converter.ConvertTo(context, culture, defaultVisual, destinationType);

            // Assert
            Assert.Equal("default", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles extreme values for parameters without affecting core logic.
        /// Input: VisualMarker.Default with extreme parameter values
        /// Expected: "default" string
        /// </summary>
        [Fact]
        public void ConvertTo_ExtremeParameterValues_ReturnsExpectedResult()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var defaultVisual = VisualMarker.Default;

            // Act
            var result = converter.ConvertTo(null, null, defaultVisual, null);

            // Assert
            Assert.Equal("default", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for non-IVisual objects with various parameter combinations.
        /// Input: non-IVisual value with different parameter combinations
        /// Expected: NotSupportedException
        /// </summary>
        [Theory]
        [InlineData("test", null, null, null)]
        [InlineData(123, null, "en-US", typeof(string))]
        [InlineData(false, null, "ja-JP", typeof(object))]
        public void ConvertTo_NonIVisualWithVariousParameters_ThrowsNotSupportedException(
            object value,
            ITypeDescriptorContext context,
            string cultureName,
            Type destinationType)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var culture = cultureName != null ? new CultureInfo(cultureName) : null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Custom IVisual implementation for testing scenarios where visual is not in mappings.
        /// </summary>
        private class CustomVisual : IVisual
        {
        }

        /// <summary>
        /// Tests that GetStandardValuesExclusive returns false when provided with a valid context.
        /// This verifies the method returns the expected constant value with normal input conditions.
        /// Expected result: The method should return false.
        /// </summary>
        [Fact]
        public void GetStandardValuesExclusive_WithValidContext_ReturnsFalse()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.GetStandardValuesExclusive(context);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesExclusive returns false when provided with a null context.
        /// This verifies the method handles null input gracefully and returns the expected constant value.
        /// Expected result: The method should return false without throwing an exception.
        /// </summary>
        [Fact]
        public void GetStandardValuesExclusive_WithNullContext_ReturnsFalse()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.GetStandardValuesExclusive(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesSupported returns true when context is null.
        /// This validates the method handles null input gracefully and returns the expected value.
        /// </summary>
        [Fact]
        public void GetStandardValuesSupported_WithNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.GetStandardValuesSupported(context);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesSupported returns true when provided with a valid context.
        /// This validates the method returns the expected value regardless of context content.
        /// </summary>
        [Fact]
        public void GetStandardValuesSupported_WithValidContext_ReturnsTrue()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.GetStandardValuesSupported(context);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetStandardValuesSupported consistently returns true across multiple calls.
        /// This validates the method behavior is deterministic and stable.
        /// </summary>
        [Fact]
        public void GetStandardValuesSupported_MultipleCalls_ConsistentlyReturnsTrue()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context1 = Substitute.For<ITypeDescriptorContext>();
            var context2 = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result1 = converter.GetStandardValuesSupported(context1);
            var result2 = converter.GetStandardValuesSupported(context2);
            var result3 = converter.GetStandardValuesSupported(null);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
        }

        /// <summary>
        /// Tests that GetStandardValues returns a non-null StandardValuesCollection with expected values when context is null.
        /// Verifies the collection contains the "Default" string value from VisualMarker.Default.
        /// </summary>
        [Fact]
        public void GetStandardValues_NullContext_ReturnsCollectionWithDefaultValue()
        {
            // Arrange
            var converter = new VisualTypeConverter();

            // Act
            var result = converter.GetStandardValues(null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("Default", result.Cast<string>());
        }

        /// <summary>
        /// Tests that GetStandardValues returns a non-null StandardValuesCollection with expected values when context is provided.
        /// Verifies the collection contains the "Default" string value from VisualMarker.Default.
        /// </summary>
        [Fact]
        public void GetStandardValues_ValidContext_ReturnsCollectionWithDefaultValue()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.GetStandardValues(context);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("Default", result.Cast<string>());
        }

        /// <summary>
        /// Tests that GetStandardValues returns a collection with exactly one element and verifies the content.
        /// Ensures the returned collection has the correct count and contains only the expected "Default" value.
        /// </summary>
        [Fact]
        public void GetStandardValues_AnyContext_ReturnsCollectionWithSingleDefaultElement()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.GetStandardValues(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            var values = result.Cast<string>().ToArray();
            Assert.Equal("Default", values[0]);
        }

        /// <summary>
        /// Tests that GetStandardValues returns the same collection content regardless of context parameter.
        /// Verifies that the context parameter does not affect the returned values.
        /// </summary>
        [Fact]
        public void GetStandardValues_DifferentContexts_ReturnsSameValues()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context1 = Substitute.For<ITypeDescriptorContext>();
            var context2 = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result1 = converter.GetStandardValues(null);
            var result2 = converter.GetStandardValues(context1);
            var result3 = converter.GetStandardValues(context2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);

            var values1 = result1.Cast<string>().ToArray();
            var values2 = result2.Cast<string>().ToArray();
            var values3 = result3.Cast<string>().ToArray();

            Assert.Equal(values1, values2);
            Assert.Equal(values2, values3);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// </summary>
        /// <param name="sourceType">The source type to test.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(IVisual))]
        [InlineData(typeof(VisualTypeConverter))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly regardless of the context parameter value.
        /// </summary>
        /// <param name="contextIsNull">Whether the context parameter should be null.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanConvertFrom_ContextParameter_DoesNotAffectResult(bool contextIsNull)
        {
            // Arrange
            var converter = new VisualTypeConverter();
            var context = contextIsNull ? null : Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }
    }
}