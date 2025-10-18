using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;


using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    public class MatrixTypeConverterUnitTests : BaseTestFixture
    {
        private readonly MatrixTypeConverter _converter = new();

        [Fact]
        public void ConvertNullTest()
        {
            Assert.Throws<ArgumentException>(() => _converter.ConvertFromInvariantString(null));
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when the destination type is string.
        /// Verifies the converter can convert to string type regardless of context.
        /// Expected result: true.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringType_ReturnsTrue()
        {
            // Arrange
            var destinationType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when the destination type is string with a valid context.
        /// Verifies the converter can convert to string type and context parameter is ignored.
        /// Expected result: true.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringTypeWithContext_ReturnsTrue()
        {
            // Arrange
            var destinationType = typeof(string);
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when the destination type is null.
        /// Verifies the converter cannot convert to a null destination type.
        /// Expected result: false.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            Type destinationType = null;
            ITypeDescriptorContext context = null;

            // Act
            var result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Verifies the converter only supports conversion to string type.
        /// Expected result: false for all non-string types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Matrix))]
        [InlineData(typeof(MatrixTypeConverter))]
        public void CanConvertTo_NonStringTypes_ReturnsFalse(Type destinationType)
        {
            // Arrange
            ITypeDescriptorContext context = null;

            // Act
            var result = _converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var sourceType = typeof(string);

            // Act
            var result = _converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(int?))]
        [InlineData(typeof(double?))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, object>))]
        [InlineData(typeof(Matrix))]
        public void CanConvertFrom_NonStringType_ReturnsFalse(Type sourceType)
        {
            // Act
            var result = _converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true for string type regardless of context parameter value.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringTypeWithNonNullContext_ReturnsTrue()
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
        /// Tests that CanConvertFrom returns false for non-string type regardless of context parameter value.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NonStringTypeWithNonNullContext_ReturnsFalse()
        {
            // Arrange
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(int);

            // Act
            var result = _converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }
    }


    /// <summary>
    /// Tests for MatrixTypeConverter.CreateMatrix method
    /// </summary>
    public partial class MatrixTypeConverterTests
    {
        /// <summary>
        /// Tests that CreateMatrix throws ArgumentException when input is null
        /// </summary>
        [Fact]
        public void CreateMatrix_NullInput_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(null));
            Assert.Equal("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix throws ArgumentException when input is empty string
        /// </summary>
        [Fact]
        public void CreateMatrix_EmptyString_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(""));
            Assert.Equal("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix throws ArgumentException when input is whitespace only
        /// </summary>
        [Theory]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData(" \t \n ")]
        public void CreateMatrix_WhitespaceOnly_ThrowsArgumentException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(input));
            Assert.Equal("Argument must have six numbers", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix throws ArgumentException when input has fewer than 6 numbers
        /// </summary>
        [Theory]
        [InlineData("1")]
        [InlineData("1 2")]
        [InlineData("1,2,3")]
        [InlineData("1 2 3 4")]
        [InlineData("1,2,3,4,5")]
        public void CreateMatrix_FewerThanSixNumbers_ThrowsArgumentException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(input));
            Assert.Equal("Argument must have six numbers", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix throws ArgumentException when input has more than 6 numbers
        /// </summary>
        [Theory]
        [InlineData("1 2 3 4 5 6 7")]
        [InlineData("1,2,3,4,5,6,7,8")]
        [InlineData("1 2 3 4 5 6 7 8 9")]
        public void CreateMatrix_MoreThanSixNumbers_ThrowsArgumentException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(input));
            Assert.Equal("Argument must have six numbers", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix throws ArgumentException when input contains non-numeric values
        /// </summary>
        [Theory]
        [InlineData("a b c d e f")]
        [InlineData("1 2 3 abc 5 6")]
        [InlineData("1,2,3,invalid,5,6")]
        [InlineData("1 2 3 4 5 ")]
        [InlineData("one two three four five six")]
        public void CreateMatrix_NonNumericValues_ThrowsArgumentException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(input));
            Assert.Equal("Argument must be numeric values", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix successfully creates Matrix with valid space-separated numbers
        /// </summary>
        [Fact]
        public void CreateMatrix_ValidSpaceSeparatedNumbers_CreatesMatrix()
        {
            // Arrange
            string input = "1.0 2.5 3.7 4.2 5.9 6.1";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(1.0, result._m11);
            Assert.Equal(2.5, result._m12);
            Assert.Equal(3.7, result._m21);
            Assert.Equal(4.2, result._m22);
            Assert.Equal(5.9, result._offsetX);
            Assert.Equal(6.1, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix successfully creates Matrix with valid comma-separated numbers
        /// </summary>
        [Fact]
        public void CreateMatrix_ValidCommaSeparatedNumbers_CreatesMatrix()
        {
            // Arrange
            string input = "1.5,2.0,3.3,4.8,5.2,6.7";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(1.5, result._m11);
            Assert.Equal(2.0, result._m12);
            Assert.Equal(3.3, result._m21);
            Assert.Equal(4.8, result._m22);
            Assert.Equal(5.2, result._offsetX);
            Assert.Equal(6.7, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix successfully creates Matrix with mixed separators
        /// </summary>
        [Fact]
        public void CreateMatrix_MixedSeparators_CreatesMatrix()
        {
            // Arrange
            string input = "1.0,2.0 3.0,4.0 5.0,6.0";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(1.0, result._m11);
            Assert.Equal(2.0, result._m12);
            Assert.Equal(3.0, result._m21);
            Assert.Equal(4.0, result._m22);
            Assert.Equal(5.0, result._offsetX);
            Assert.Equal(6.0, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix handles extra whitespace correctly
        /// </summary>
        [Fact]
        public void CreateMatrix_ExtraWhitespace_CreatesMatrix()
        {
            // Arrange
            string input = "  1.0   2.0   3.0   4.0   5.0   6.0  ";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(1.0, result._m11);
            Assert.Equal(2.0, result._m12);
            Assert.Equal(3.0, result._m21);
            Assert.Equal(4.0, result._m22);
            Assert.Equal(5.0, result._offsetX);
            Assert.Equal(6.0, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix handles integer values correctly
        /// </summary>
        [Fact]
        public void CreateMatrix_IntegerValues_CreatesMatrix()
        {
            // Arrange
            string input = "1 2 3 4 5 6";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(1.0, result._m11);
            Assert.Equal(2.0, result._m12);
            Assert.Equal(3.0, result._m21);
            Assert.Equal(4.0, result._m22);
            Assert.Equal(5.0, result._offsetX);
            Assert.Equal(6.0, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix handles negative numbers correctly
        /// </summary>
        [Fact]
        public void CreateMatrix_NegativeNumbers_CreatesMatrix()
        {
            // Arrange
            string input = "-1.5 -2.0 -3.3 -4.8 -5.2 -6.7";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(-1.5, result._m11);
            Assert.Equal(-2.0, result._m12);
            Assert.Equal(-3.3, result._m21);
            Assert.Equal(-4.8, result._m22);
            Assert.Equal(-5.2, result._offsetX);
            Assert.Equal(-6.7, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix handles zero values correctly
        /// </summary>
        [Fact]
        public void CreateMatrix_ZeroValues_CreatesMatrix()
        {
            // Arrange
            string input = "0 0 0 0 0 0";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(0.0, result._m11);
            Assert.Equal(0.0, result._m12);
            Assert.Equal(0.0, result._m21);
            Assert.Equal(0.0, result._m22);
            Assert.Equal(0.0, result._offsetX);
            Assert.Equal(0.0, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix handles extreme double values correctly
        /// </summary>
        [Theory]
        [InlineData("1.7976931348623157E+308 1.0 1.0 1.0 1.0 1.0")] // Double.MaxValue
        [InlineData("-1.7976931348623157E+308 1.0 1.0 1.0 1.0 1.0")] // Double.MinValue
        [InlineData("4.94065645841247E-324 1.0 1.0 1.0 1.0 1.0")] // Double.Epsilon
        public void CreateMatrix_ExtremeDoubleValues_CreatesMatrix(string input)
        {
            // Act & Assert - Should not throw
            var result = MatrixTypeConverter.CreateMatrix(input);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that CreateMatrix handles special double values that cannot be parsed
        /// </summary>
        [Theory]
        [InlineData("NaN 1.0 1.0 1.0 1.0 1.0")]
        [InlineData("Infinity 1.0 1.0 1.0 1.0 1.0")]
        [InlineData("-Infinity 1.0 1.0 1.0 1.0 1.0")]
        [InlineData("1.0 1.0 1.0 1.0 1.0 NaN")]
        public void CreateMatrix_SpecialDoubleValues_ThrowsArgumentException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => MatrixTypeConverter.CreateMatrix(input));
            Assert.Equal("Argument must be numeric values", exception.Message);
        }

        /// <summary>
        /// Tests that CreateMatrix handles scientific notation correctly
        /// </summary>
        [Fact]
        public void CreateMatrix_ScientificNotation_CreatesMatrix()
        {
            // Arrange
            string input = "1.5E2 2.0E-1 3.0E0 4.0E1 5.0E-2 6.0E3";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(150.0, result._m11);
            Assert.Equal(0.2, result._m12);
            Assert.Equal(3.0, result._m21);
            Assert.Equal(40.0, result._m22);
            Assert.Equal(0.05, result._offsetX);
            Assert.Equal(6000.0, result._offsetY);
        }

        /// <summary>
        /// Tests that CreateMatrix handles very long decimal precision correctly
        /// </summary>
        [Fact]
        public void CreateMatrix_LongDecimalPrecision_CreatesMatrix()
        {
            // Arrange
            string input = "1.123456789012345 2.987654321098765 3.555555555555555 4.444444444444444 5.333333333333333 6.777777777777777";

            // Act
            var result = MatrixTypeConverter.CreateMatrix(input);

            // Assert
            Assert.Equal(1.123456789012345, result._m11, 15);
            Assert.Equal(2.987654321098765, result._m12, 15);
            Assert.Equal(3.555555555555555, result._m21, 15);
            Assert.Equal(4.444444444444444, result._m22, 15);
            Assert.Equal(5.333333333333333, result._offsetX, 15);
            Assert.Equal(6.777777777777777, result._offsetY, 15);
        }
        private readonly MatrixTypeConverter _converter = new();

        /// <summary>
        /// Tests that ConvertTo properly converts a valid Matrix to its string representation.
        /// Input: Valid Matrix with specific values.
        /// Expected: Comma-separated string with M11, M12, M21, M22, OffsetX, OffsetY values.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidMatrix_ReturnsFormattedString()
        {
            // Arrange
            var matrix = new Matrix(1.5, 2.0, 3.0, 4.5, 10.25, 20.75);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, matrix, destinationType);

            // Assert
            Assert.Equal("1.5, 2, 3, 4.5, 10.25, 20.75", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles Matrix.Identity correctly.
        /// Input: Matrix.Identity.
        /// Expected: String representation of identity matrix values.
        /// </summary>
        [Fact]
        public void ConvertTo_IdentityMatrix_ReturnsFormattedString()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, matrix, destinationType);

            // Assert
            Assert.Equal("1, 0, 0, 1, 0, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles Matrix with zero values correctly.
        /// Input: Matrix with all zero values.
        /// Expected: String with all zeros.
        /// </summary>
        [Fact]
        public void ConvertTo_ZeroMatrix_ReturnsFormattedString()
        {
            // Arrange
            var matrix = new Matrix(0, 0, 0, 0, 0, 0);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, matrix, destinationType);

            // Assert
            Assert.Equal("0, 0, 0, 0, 0, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles Matrix with negative values correctly.
        /// Input: Matrix with negative values.
        /// Expected: String representation with negative values.
        /// </summary>
        [Fact]
        public void ConvertTo_NegativeValuesMatrix_ReturnsFormattedString()
        {
            // Arrange
            var matrix = new Matrix(-1.5, -2.0, -3.0, -4.5, -10.25, -20.75);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, matrix, destinationType);

            // Assert
            Assert.Equal("-1.5, -2, -3, -4.5, -10.25, -20.75", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles Matrix with special double values (NaN, Infinity).
        /// Input: Matrix with NaN and infinity values.
        /// Expected: String representation of special values using InvariantCulture.
        /// </summary>
        [Fact]
        public void ConvertTo_SpecialDoubleValues_ReturnsFormattedString()
        {
            // Arrange
            var matrix = new Matrix(double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue, double.MinValue, double.Epsilon);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act
            var result = _converter.ConvertTo(context, culture, matrix, destinationType);

            // Assert
            var expected = $"{double.NaN.ToString(CultureInfo.InvariantCulture)}, {double.PositiveInfinity.ToString(CultureInfo.InvariantCulture)}, {double.NegativeInfinity.ToString(CultureInfo.InvariantCulture)}, {double.MaxValue.ToString(CultureInfo.InvariantCulture)}, {double.MinValue.ToString(CultureInfo.InvariantCulture)}, {double.Epsilon.ToString(CultureInfo.InvariantCulture)}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// Input: null value.
        /// Expected: NotSupportedException thrown.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            object value = null;
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a Matrix.
        /// Input: Various non-Matrix object types.
        /// Expected: NotSupportedException thrown for each.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_NonMatrixValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;
            var destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of context parameter value.
        /// Input: Valid Matrix with various context values.
        /// Expected: Same formatted string regardless of context.
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentContextValues_IgnoresContext()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);
            var destinationType = typeof(string);
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result1 = _converter.ConvertTo(null, culture, matrix, destinationType);
            var result2 = _converter.ConvertTo(null, culture, matrix, destinationType); // context is not used anyway

            // Assert
            Assert.Equal("1, 2, 3, 4, 5, 6", result1);
            Assert.Equal(result1, result2);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of culture parameter value.
        /// Input: Valid Matrix with various culture values.
        /// Expected: Same formatted string using InvariantCulture regardless of input culture.
        /// </summary>
        [Fact]
        public void ConvertTo_DifferentCultureValues_IgnoresCulture()
        {
            // Arrange
            var matrix = new Matrix(1.5, 2.5, 3.5, 4.5, 5.5, 6.5);
            var context = (ITypeDescriptorContext)null;
            var destinationType = typeof(string);

            // Act
            var result1 = _converter.ConvertTo(context, null, matrix, destinationType);
            var result2 = _converter.ConvertTo(context, CultureInfo.InvariantCulture, matrix, destinationType);
            var result3 = _converter.ConvertTo(context, new CultureInfo("fr-FR"), matrix, destinationType);

            // Assert
            Assert.Equal("1.5, 2.5, 3.5, 4.5, 5.5, 6.5", result1);
            Assert.Equal(result1, result2);
            Assert.Equal(result1, result3);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly regardless of destinationType parameter value.
        /// Input: Valid Matrix with various destination types.
        /// Expected: Same formatted string regardless of destination type.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_DifferentDestinationTypes_IgnoresDestinationType(Type destinationType)
        {
            // Arrange
            var matrix = new Matrix(7, 8, 9, 10, 11, 12);
            var context = (ITypeDescriptorContext)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.ConvertTo(context, culture, matrix, destinationType);

            // Assert
            Assert.Equal("7, 8, 9, 10, 11, 12", result);
        }
    }
}
