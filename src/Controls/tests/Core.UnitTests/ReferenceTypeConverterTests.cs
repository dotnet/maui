using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for ReferenceTypeConverter class
    /// </summary>
    public sealed class ReferenceTypeConverterTests
    {
        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is string type
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            ITypeDescriptorContext context = null;
            Type destinationType = typeof(string);

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is null
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            ITypeDescriptorContext context = null;
            Type destinationType = null;

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string types
        /// </summary>
        /// <param name="destinationType">The destination type to test</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertTo_DestinationTypeIsNotString_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo ignores the context parameter and returns true for string type
        /// </summary>
        [Fact]
        public void CanConvertTo_WithMockedContext_DestinationTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = typeof(string);

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo ignores the context parameter and returns false for non-string type
        /// </summary>
        [Fact]
        public void CanConvertTo_WithMockedContext_DestinationTypeIsNotString_ReturnsFalse()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = typeof(int);

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom method always throws NotImplementedException regardless of input parameters.
        /// Verifies that the method consistently throws the expected exception for various input combinations
        /// including null and non-null parameters.
        /// </summary>
        /// <param name="context">The type descriptor context to test with.</param>
        /// <param name="culture">The culture info to test with.</param>
        /// <param name="value">The value object to test with.</param>
        [Theory]
        [InlineData(null, null, "test")]
        [InlineData(null, null, 42)]
        [InlineData(null, null, true)]
        public void ConvertFrom_AnyParameters_ThrowsNotImplementedException(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with valid non-null context and culture.
        /// Verifies that even when provided with valid context and culture instances, the method
        /// still throws NotImplementedException as expected.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithValidContextAndCulture_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = "test value";

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with edge case values.
        /// Verifies behavior with boundary and special case input values including empty strings,
        /// numeric extremes, and complex objects.
        /// </summary>
        /// <param name="value">The edge case value to test with.</param>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ConvertFrom_WithEdgeCaseValues_ThrowsNotImplementedException(object value)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(null, null, value));
        }

        /// <summary>
        /// Tests that ConvertTo always throws NotSupportedException regardless of input parameters.
        /// </summary>
        /// <param name="context">The type descriptor context parameter</param>
        /// <param name="culture">The culture info parameter</param>
        /// <param name="value">The value to convert parameter</param>
        /// <param name="destinationType">The destination type parameter</param>
        [Theory]
        [InlineData(null, null, null, typeof(string))]
        [InlineData(null, null, "test", typeof(string))]
        [InlineData(null, null, 123, typeof(int))]
        [InlineData(null, null, true, typeof(bool))]
        [InlineData(null, null, null, typeof(object))]
        public void ConvertTo_AnyParameters_ThrowsNotSupportedException(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with specific culture values.
        /// </summary>
        [Fact]
        public void ConvertTo_WithSpecificCultureInfo_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, culture, "test", typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with current culture.
        /// </summary>
        [Fact]
        public void ConvertTo_WithCurrentCulture_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var culture = CultureInfo.CurrentCulture;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, culture, 42, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with complex object values.
        /// </summary>
        [Fact]
        public void ConvertTo_WithComplexObjects_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var complexObject = new { Name = "Test", Value = 123 };

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, complexObject, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with various destination types.
        /// </summary>
        /// <param name="destinationType">The destination type to test</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        public void ConvertTo_WithVariousDestinationTypes_ThrowsNotSupportedException(Type destinationType)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, "value", destinationType));
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string and context is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsStringAndContextIsNull_ReturnsTrue()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string source types.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(ReferenceTypeConverter))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(void))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws NullReferenceException when sourceType is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for array types including string arrays.
        /// </summary>
        /// <param name="sourceType">The array source type to test conversion from.</param>
        [Theory]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(object[]))]
        public void CanConvertFrom_SourceTypeIsArray_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new ReferenceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}