using System;
using System.ComponentModel;
using System.Globalization;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    public class PathGeometryConverterTests : BaseTestFixture
    {
        private readonly PathGeometryConverter _converter = new();

        [Fact]
        public void ConvertNullTest()
        {
            var result = _converter.ConvertFromInvariantString(null);
            var pathGeometry = Assert.IsType<PathGeometry>(result);
            Assert.Empty(pathGeometry.Figures);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for null destination type.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            ITypeDescriptorContext context = null;
            Type destinationType = null;

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for null context and null destination type.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContextAndNullDestinationType_ReturnsFalse()
        {
            // Arrange
            ITypeDescriptorContext context = null;
            Type destinationType = null;

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various destination types with null context.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to.</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(PathGeometry))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        public void CanConvertTo_NullContextVariousDestinationTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            ITypeDescriptorContext context = null;

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when provided with a mocked context.
        /// </summary>
        [Fact]
        public void CanConvertTo_WithMockedContext_ReturnsFalse()
        {
            // Arrange
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = typeof(string);

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various destination types with mocked context.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to.</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(PathGeometry))]
        [InlineData(typeof(object))]
        public void CanConvertTo_MockedContextVariousDestinationTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            bool result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertTo always throws NotSupportedException regardless of input parameters.
        /// Verifies the method correctly rejects conversion attempts with various parameter combinations.
        /// </summary>
        /// <param name="context">The type descriptor context to test with</param>
        /// <param name="culture">The culture info to test with</param>
        /// <param name="value">The value to test conversion from</param>
        /// <param name="destinationType">The destination type to test conversion to</param>
        [Theory]
        [InlineData(null, null, null, typeof(string))]
        [InlineData(null, null, "test", typeof(string))]
        [InlineData(null, null, 42, typeof(int))]
        [InlineData(null, null, true, typeof(bool))]
        [InlineData(null, null, 3.14, typeof(double))]
        public void ConvertTo_WithVariousInputs_ThrowsNotSupportedException(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with valid context and culture instances.
        /// Verifies behavior when non-null context and culture are provided.
        /// </summary>
        [Fact]
        public void ConvertTo_WithValidContextAndCulture_ThrowsNotSupportedException()
        {
            // Arrange
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = "test value";
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when converting to PathGeometry type.
        /// Verifies the method rejects conversion even to its own related type.
        /// </summary>
        [Fact]
        public void ConvertTo_WithPathGeometryDestination_ThrowsNotSupportedException()
        {
            // Arrange
            var pathGeometry = new PathGeometry();
            var destinationType = typeof(PathGeometry);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, pathGeometry, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with extreme numeric values.
        /// Verifies behavior with boundary value inputs.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, typeof(string))]
        [InlineData(int.MaxValue, typeof(int))]
        [InlineData(double.NaN, typeof(double))]
        [InlineData(double.PositiveInfinity, typeof(string))]
        [InlineData(double.NegativeInfinity, typeof(string))]
        public void ConvertTo_WithExtremeValues_ThrowsNotSupportedException(
            object value,
            Type destinationType)
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with various string inputs.
        /// Verifies behavior with different string edge cases.
        /// </summary>
        [Theory]
        [InlineData("", typeof(string))]
        [InlineData("   ", typeof(string))]
        [InlineData("M 10,10 L 20,20", typeof(string))]
        public void ConvertTo_WithStringValues_ThrowsNotSupportedException(
            string value,
            Type destinationType)
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// This verifies the positive case where string conversion is supported.
        /// Expected result: true.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string) and context is null.
        /// This verifies that the context parameter is ignored and null context works correctly.
        /// Expected result: true.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringTypeWithNullContext_ReturnsTrue()
        {
            // Arrange
            ITypeDescriptorContext context = null;
            var sourceType = typeof(string);

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// This verifies the negative cases where conversion is not supported for other types.
        /// Expected result: false for all non-string types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(PathGeometry))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(CultureInfo))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// This verifies proper null parameter handling for the required sourceType parameter.
        /// Expected result: ArgumentNullException or NullReferenceException.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ThrowsException()
        {
            // Arrange
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _converter.CanConvertFrom(context, sourceType));
        }
    }
}