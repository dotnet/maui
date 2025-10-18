using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TransformTypeConverterTests
    {
        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when value parameter is null.
        /// The method calls value?.ToString() which returns null, and CreateMatrix throws for null input.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(context, culture, value));
            Assert.Equal("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when value.ToString() returns null.
        /// This tests the null-conditional operator behavior in value?.ToString().
        /// </summary>
        [Fact]
        public void ConvertFrom_ObjectWithNullToString_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var objectWithNullToString = new ObjectWithNullToString();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, null, objectWithNullToString));
            Assert.Equal("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when value.ToString() returns empty string.
        /// CreateMatrix throws for empty string input.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, null, value));
            Assert.Equal("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when value.ToString() returns whitespace-only string.
        /// CreateMatrix considers whitespace-only strings as empty after trimming.
        /// </summary>
        [Fact]
        public void ConvertFrom_WhitespaceString_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = "   ";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, null, value));
            Assert.Equal("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully creates MatrixTransform when given valid matrix string.
        /// Input string contains exactly 6 numeric values that can be parsed as doubles.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidMatrixString_ReturnsMatrixTransformWithCorrectMatrix()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = "1,0,0,1,10,20";

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MatrixTransform>(result);
            var matrixTransform = (MatrixTransform)result;
            var matrix = matrixTransform.Matrix;
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(10.0, matrix.OffsetX);
            Assert.Equal(20.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when string contains less than 6 values.
        /// CreateMatrix requires exactly 6 numeric values to construct a matrix.
        /// </summary>
        [Fact]
        public void ConvertFrom_StringWithLessThan6Values_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = "1,0,0,1,10";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, null, value));
            Assert.Equal("Argument must have six numbers", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when string contains more than 6 values.
        /// CreateMatrix requires exactly 6 numeric values to construct a matrix.
        /// </summary>
        [Fact]
        public void ConvertFrom_StringWithMoreThan6Values_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = "1,0,0,1,10,20,30";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, null, value));
            Assert.Equal("Argument must have six numbers", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws ArgumentException when string contains non-numeric values.
        /// CreateMatrix requires all 6 values to be parseable as doubles.
        /// </summary>
        [Fact]
        public void ConvertFrom_StringWithNonNumericValues_ThrowsArgumentException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = "1,0,abc,1,10,20";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                converter.ConvertFrom(null, null, value));
            Assert.Equal("Argument must be numeric values", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom works with different object types by calling their ToString() method.
        /// Uses a custom object that returns a valid matrix string from ToString().
        /// </summary>
        [Fact]
        public void ConvertFrom_CustomObjectWithValidToString_ReturnsMatrixTransform()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var customObject = new ObjectWithCustomToString("1,0,0,1,5,15");

            // Act
            var result = converter.ConvertFrom(null, null, customObject);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MatrixTransform>(result);
            var matrixTransform = (MatrixTransform)result;
            var matrix = matrixTransform.Matrix;
            Assert.Equal(5.0, matrix.OffsetX);
            Assert.Equal(15.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that ConvertFrom handles extreme double values correctly.
        /// Uses very large and very small double values that are still valid.
        /// </summary>
        [Fact]
        public void ConvertFrom_ExtremeDoubleValues_ReturnsMatrixTransform()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var value = $"{double.MaxValue},{double.MinValue},0,1,{double.PositiveInfinity},{double.NegativeInfinity}";

            // Act
            var result = converter.ConvertFrom(null, null, value);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MatrixTransform>(result);
            var matrixTransform = (MatrixTransform)result;
            var matrix = matrixTransform.Matrix;
            Assert.Equal(double.MaxValue, matrix.M11);
            Assert.Equal(double.MinValue, matrix.M12);
            Assert.Equal(double.PositiveInfinity, matrix.OffsetX);
            Assert.Equal(double.NegativeInfinity, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that ConvertFrom ignores context and culture parameters.
        /// The method implementation doesn't use these parameters, so they should have no effect.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithNonNullContextAndCulture_ReturnsMatrixTransform()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = "1,0,0,1,0,0";

            // Act
            var result = converter.ConvertFrom(context, culture, value);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MatrixTransform>(result);
        }

        /// <summary>
        /// Helper class that returns null from ToString() to test null-conditional operator behavior.
        /// </summary>
        private class ObjectWithNullToString
        {
            public override string ToString() => null;
        }

        /// <summary>
        /// Helper class that returns a custom string from ToString() for testing object conversion.
        /// </summary>
        private class ObjectWithCustomToString
        {
            private readonly string _value;

            public ObjectWithCustomToString(string value)
            {
                _value = value;
            }

            public override string ToString() => _value;
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsNull_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            object? value = null;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a MatrixTransform.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_ValueIsNotMatrixTransform_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo succeeds when value is a MatrixTransform with default matrix.
        /// Expected result is the string representation of the identity matrix values.
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsMatrixTransformWithDefaultMatrix_ReturnsFormattedString()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var matrixTransform = new MatrixTransform();
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, matrixTransform, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            // Default Matrix should be identity matrix (1, 0, 0, 1, 0, 0)
            Assert.Equal("1, 0, 0, 1, 0, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo succeeds when value is a MatrixTransform with custom matrix values.
        /// Expected result is the string representation of the custom matrix values.
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsMatrixTransformWithCustomMatrix_ReturnsFormattedString()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var customMatrix = new Matrix(2.5, 1.2, 0.8, 3.7, 10.5, 20.3);
            var matrixTransform = new MatrixTransform { Matrix = customMatrix };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, matrixTransform, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal("2.5, 1.2, 0.8, 3.7, 10.5, 20.3", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different destination types.
        /// The destinationType parameter is not used in the implementation, so behavior should be consistent.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        public void ConvertTo_DifferentDestinationTypes_ReturnsConsistentResult(Type destinationType)
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var matrixTransform = new MatrixTransform();

            // Act
            var result = converter.ConvertTo(null, null, matrixTransform, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1, 0, 0, 1, 0, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles extreme matrix values correctly.
        /// Tests boundary conditions with very large, very small, and special double values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue, 0.0, -0.0, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
        [InlineData(-1.5, -2.5, -3.5, -4.5, -5.5, -6.5)]
        public void ConvertTo_ExtremeMatrixValues_ReturnsFormattedString(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);
            var matrixTransform = new MatrixTransform { Matrix = matrix };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, matrixTransform, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            var expected = $"{m11.ToString(CultureInfo.InvariantCulture)}, {m12.ToString(CultureInfo.InvariantCulture)}, {m21.ToString(CultureInfo.InvariantCulture)}, {m22.ToString(CultureInfo.InvariantCulture)}, {offsetX.ToString(CultureInfo.InvariantCulture)}, {offsetY.ToString(CultureInfo.InvariantCulture)}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo handles NaN values in matrix correctly.
        /// Expected result should include "NaN" in the formatted string.
        /// </summary>
        [Fact]
        public void ConvertTo_MatrixWithNaNValues_ReturnsFormattedStringWithNaN()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var matrix = new Matrix(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
            var matrixTransform = new MatrixTransform { Matrix = matrix };
            var destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(null, null, matrixTransform, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal("NaN, NaN, NaN, NaN, NaN, NaN", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string, regardless of context value.
        /// </summary>
        /// <param name="context">The context parameter (nullable)</param>
        [Theory]
        [InlineData(null)]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue(ITypeDescriptorContext context)
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is string with a valid context.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsStringWithValidContext_ReturnsTrue()
        {
            // Arrange
            var converter = new TransformTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(string);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is not string.
        /// </summary>
        /// <param name="sourceType">The source type to test</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Transform))]
        [InlineData(typeof(TransformTypeConverter))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new TransformTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }
    }
}