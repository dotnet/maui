using System;
using System.ComponentModel;
using System.Globalization;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class GridLengthTypeConverterTests : BaseTestFixture
    {
        [Fact]
        public void TestAbsolute()
        {
            var converter = new GridLengthTypeConverter();

            Assert.Equal(new GridLength(42), converter.ConvertFromInvariantString("42"));
            Assert.Equal(new GridLength(42.2), converter.ConvertFromInvariantString("42.2"));

            Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("foo"));
        }

        [Fact]
        public void TestAuto()
        {
            var converter = new GridLengthTypeConverter();

            Assert.Equal(GridLength.Auto, converter.ConvertFromInvariantString("auto"));
            Assert.Equal(GridLength.Auto, converter.ConvertFromInvariantString(" AuTo "));
        }

        [Fact]
        public void TestStar()
        {
            var converter = new GridLengthTypeConverter();

            Assert.Equal(new GridLength(1, GridUnitType.Star), converter.ConvertFromInvariantString("*"));
            Assert.Equal(new GridLength(42, GridUnitType.Star), converter.ConvertFromInvariantString("42*"));

        }

        [Fact]
        public void TestValue()
        {
            var converter = new GridLengthTypeConverter();
            Assert.Equal(new GridLength(3.3), converter.ConvertFromInvariantString("3.3"));
        }

        [Fact]
        public void TestValueStar()
        {
            var converter = new GridLengthTypeConverter();
            Assert.Equal(new GridLength(32.3, GridUnitType.Star), converter.ConvertFromInvariantString("32.3*"));
        }

        /// <summary>
        /// Tests that CanConvertTo always returns true regardless of input parameters.
        /// This method should accept any combination of context and destination type parameters.
        /// </summary>
        /// <param name="hasContext">Whether to provide a non-null context parameter</param>
        /// <param name="hasDestinationType">Whether to provide a non-null destination type parameter</param>
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void CanConvertTo_AnyParameters_ReturnsTrue(bool hasContext, bool hasDestinationType)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            ITypeDescriptorContext context = hasContext ? Substitute.For<ITypeDescriptorContext>() : null;
            Type destinationType = hasDestinationType ? typeof(string) : null;

            // Act
            bool result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for various specific destination types.
        /// Validates that the method consistently returns true regardless of the target type.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion to</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(GridLength))]
        [InlineData(typeof(double))]
        public void CanConvertTo_VariousDestinationTypes_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act
            bool result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ConvertFrom returns null when the input value is null.
        /// This exercises the null check path that returns null when strValue is null.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ReturnsNull()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts valid string inputs to GridLength objects.
        /// Verifies the main conversion path through ParseStringToGridLength.
        /// </summary>
        [Theory]
        [InlineData("42", 42.0, GridUnitType.Absolute)]
        [InlineData("42.5", 42.5, GridUnitType.Absolute)]
        [InlineData("auto", 1.0, GridUnitType.Auto)]
        [InlineData("Auto", 1.0, GridUnitType.Auto)]
        [InlineData("AUTO", 1.0, GridUnitType.Auto)]
        [InlineData("*", 1.0, GridUnitType.Star)]
        [InlineData("2*", 2.0, GridUnitType.Star)]
        [InlineData("3.5*", 3.5, GridUnitType.Star)]
        public void ConvertFrom_ValidStringInput_ReturnsCorrectGridLength(string input, double expectedValue, GridUnitType expectedUnit)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<GridLength>(result);
            var gridLength = (GridLength)result;
            Assert.Equal(expectedValue, gridLength.Value);
            Assert.Equal(expectedUnit, gridLength.GridUnitType);
        }

        /// <summary>
        /// Tests that ConvertFrom handles non-string objects by calling their ToString() method.
        /// Verifies the value?.ToString() code path when input is not a string.
        /// </summary>
        [Theory]
        [InlineData(42, 42.0, GridUnitType.Absolute)]
        [InlineData(42.5, 42.5, GridUnitType.Absolute)]
        [InlineData(0, 0.0, GridUnitType.Absolute)]
        public void ConvertFrom_NonStringObject_CallsToStringAndConverts(object input, double expectedValue, GridUnitType expectedUnit)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<GridLength>(result);
            var gridLength = (GridLength)result;
            Assert.Equal(expectedValue, gridLength.Value);
            Assert.Equal(expectedUnit, gridLength.GridUnitType);
        }

        /// <summary>
        /// Tests that ConvertFrom throws FormatException for invalid string formats.
        /// Verifies error handling in ParseStringToGridLength method.
        /// </summary>
        [Theory]
        [InlineData("invalid")]
        [InlineData("foo")]
        [InlineData("123abc")]
        [InlineData("*123")]
        [InlineData("auto*")]
        [InlineData("**")]
        public void ConvertFrom_InvalidStringFormat_ThrowsFormatException(string input)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act & Assert
            Assert.Throws<FormatException>(() => converter.ConvertFrom(null, null, input));
        }

        /// <summary>
        /// Tests that ConvertFrom handles edge case string values.
        /// Verifies behavior with empty strings, whitespace, and boundary conditions.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("   ")]
        public void ConvertFrom_EdgeCaseStrings_ThrowsFormatException(string input)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act & Assert
            Assert.Throws<FormatException>(() => converter.ConvertFrom(null, null, input));
        }

        /// <summary>
        /// Tests that ConvertFrom handles strings with leading/trailing whitespace correctly.
        /// Verifies that ParseStringToGridLength properly trims input.
        /// </summary>
        [Theory]
        [InlineData("  42  ", 42.0, GridUnitType.Absolute)]
        [InlineData("\t auto \n", 1.0, GridUnitType.Auto)]
        [InlineData("  *  ", 1.0, GridUnitType.Star)]
        [InlineData(" 2* ", 2.0, GridUnitType.Star)]
        public void ConvertFrom_StringWithWhitespace_TrimsAndConverts(string input, double expectedValue, GridUnitType expectedUnit)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<GridLength>(result);
            var gridLength = (GridLength)result;
            Assert.Equal(expectedValue, gridLength.Value);
            Assert.Equal(expectedUnit, gridLength.GridUnitType);
        }

        /// <summary>
        /// Tests that ConvertFrom ignores the context and culture parameters.
        /// Verifies that different context and culture values don't affect the conversion.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_IgnoresParameters()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("de-DE");

            // Act
            var result = converter.ConvertFrom(context, culture, "42.5");

            // Assert
            Assert.IsType<GridLength>(result);
            var gridLength = (GridLength)result;
            Assert.Equal(42.5, gridLength.Value);
            Assert.Equal(GridUnitType.Absolute, gridLength.GridUnitType);
        }

        /// <summary>
        /// Tests ConvertTo method when value parameter is null.
        /// Should throw NotSupportedException because null is not a GridLength.
        /// </summary>
        [Fact]
        public void ConvertTo_ValueIsNull_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, typeof(string)));
        }

        /// <summary>
        /// Tests ConvertTo method when value parameter is not a GridLength type.
        /// Should throw NotSupportedException for non-GridLength types.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(42.5)]
        [InlineData(true)]
        public void ConvertTo_ValueIsNotGridLength_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));
        }

        /// <summary>
        /// Tests ConvertTo method with valid GridLength values.
        /// Should return the string representation of the GridLength.
        /// </summary>
        [Theory]
        [InlineData(42.0, GridUnitType.Absolute, "42")]
        [InlineData(0.0, GridUnitType.Absolute, "0")]
        [InlineData(1.5, GridUnitType.Absolute, "1.5")]
        [InlineData(2.0, GridUnitType.Star, "2*")]
        [InlineData(1.0, GridUnitType.Star, "1*")]
        [InlineData(0.5, GridUnitType.Star, "0.5*")]
        public void ConvertTo_ValidGridLength_ReturnsExpectedString(double value, GridUnitType unitType, string expected)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var gridLength = new GridLength(value, unitType);

            // Act
            var result = converter.ConvertTo(null, null, gridLength, typeof(string));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ConvertTo method with Auto GridLength.
        /// Should return "auto" string.
        /// </summary>
        [Fact]
        public void ConvertTo_AutoGridLength_ReturnsAutoString()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var gridLength = GridLength.Auto;

            // Act
            var result = converter.ConvertTo(null, null, gridLength, typeof(string));

            // Assert
            Assert.Equal("auto", result);
        }

        /// <summary>
        /// Tests ConvertTo method with Star GridLength using static property.
        /// Should return "1*" string.
        /// </summary>
        [Fact]
        public void ConvertTo_StarGridLength_ReturnsStarString()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var gridLength = GridLength.Star;

            // Act
            var result = converter.ConvertTo(null, null, gridLength, typeof(string));

            // Assert
            Assert.Equal("1*", result);
        }

        /// <summary>
        /// Tests ConvertTo method with extreme numeric values for GridLength.
        /// Should handle boundary values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, GridUnitType.Absolute)]
        [InlineData(double.Epsilon, GridUnitType.Absolute)]
        [InlineData(1000000.0, GridUnitType.Star)]
        public void ConvertTo_ExtremeValues_ReturnsValidString(double value, GridUnitType unitType)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var gridLength = new GridLength(value, unitType);

            // Act
            var result = converter.ConvertTo(null, null, gridLength, typeof(string));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        /// <summary>
        /// Tests ConvertTo method ignores context and culture parameters.
        /// Should produce same results regardless of context and culture values.
        /// </summary>
        [Fact]
        public void ConvertTo_ContextAndCultureIgnored_ProducesSameResult()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var gridLength = new GridLength(42.5, GridUnitType.Absolute);
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("fr-FR");

            // Act
            var resultWithNull = converter.ConvertTo(null, null, gridLength, typeof(string));
            var resultWithValues = converter.ConvertTo(context, culture, gridLength, typeof(string));

            // Assert
            Assert.Equal(resultWithNull, resultWithValues);
        }

        /// <summary>
        /// Tests ConvertTo method ignores destinationType parameter.
        /// Should work with any destination type since the method doesn't use it.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        public void ConvertTo_DestinationTypeIgnored_ReturnsString(Type destinationType)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var gridLength = new GridLength(10.0, GridUnitType.Absolute);

            // Act
            var result = converter.ConvertTo(null, null, gridLength, destinationType);

            // Assert
            Assert.Equal("10", result);
        }

        /// <summary>
        /// Tests that ConvertToString returns "auto" for GridLength.Auto.
        /// Verifies the auto type conversion path.
        /// </summary>
        [Fact]
        public void ConvertToString_AutoGridLength_ReturnsAuto()
        {
            // Arrange
            var autoGridLength = GridLength.Auto;

            // Act
            var result = GridLengthTypeConverter.ConvertToString(autoGridLength);

            // Assert
            Assert.Equal("auto", result);
        }

        /// <summary>
        /// Tests that ConvertToString returns the value followed by "*" for Star GridLength values.
        /// Verifies the star type conversion path with various numeric values.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0.5)]
        [InlineData(2.5)]
        [InlineData(10)]
        [InlineData(100.25)]
        [InlineData(999999.999)]
        public void ConvertToString_StarGridLength_ReturnsValueWithStar(double value)
        {
            // Arrange
            var starGridLength = new GridLength(value, GridUnitType.Star);

            // Act
            var result = GridLengthTypeConverter.ConvertToString(starGridLength);

            // Assert
            var expectedValue = value.ToString(CultureInfo.InvariantCulture);
            var expected = $"{expectedValue}*";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertToString returns the value as string for Absolute GridLength values.
        /// Verifies the absolute type conversion path with various numeric values.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0.5)]
        [InlineData(2.5)]
        [InlineData(10)]
        [InlineData(100.25)]
        [InlineData(999999.999)]
        [InlineData(0.000001)]
        public void ConvertToString_AbsoluteGridLength_ReturnsValue(double value)
        {
            // Arrange
            var absoluteGridLength = new GridLength(value);

            // Act
            var result = GridLengthTypeConverter.ConvertToString(absoluteGridLength);

            // Assert
            var expected = value.ToString(CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertToString returns "1*" for the static GridLength.Star instance.
        /// Verifies the default star GridLength conversion.
        /// </summary>
        [Fact]
        public void ConvertToString_DefaultStar_ReturnsOneStar()
        {
            // Arrange
            var starGridLength = GridLength.Star;

            // Act
            var result = GridLengthTypeConverter.ConvertToString(starGridLength);

            // Assert
            Assert.Equal("1*", result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// This verifies the converter can convert from string type.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var sourceType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// This verifies the converter only supports conversion from string type.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(char))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(long))]
        [InlineData(typeof(short))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(GridLength))]
        [InlineData(typeof(ITypeDescriptorContext))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// This verifies proper null parameter handling.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            Type sourceType = null;
            ITypeDescriptorContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with a non-null context parameter.
        /// This verifies that the context parameter doesn't affect the conversion logic.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNonNullContext_ReturnsCorrectResult()
        {
            // Arrange
            var converter = new GridLengthTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var stringSourceType = typeof(string);
            var intSourceType = typeof(int);

            // Act
            var stringResult = converter.CanConvertFrom(context, stringSourceType);
            var intResult = converter.CanConvertFrom(context, intSourceType);

            // Assert
            Assert.True(stringResult);
            Assert.False(intResult);
        }
    }


    public partial class GridLengthTypeConverterParseStringToGridLengthTests : BaseTestFixture
    {
        /// <summary>
        /// Tests parsing valid "auto" strings in various cases and with whitespace.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        [Theory]
        [InlineData("auto")]
        [InlineData("AUTO")]
        [InlineData("Auto")]
        [InlineData("AuTo")]
        [InlineData(" auto ")]
        [InlineData("\tauto\n")]
        public void ParseStringToGridLength_ValidAutoInput_ReturnsAutoGridLength(string input)
        {
            // Arrange & Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(GridLength.Auto, result);
        }

        /// <summary>
        /// Tests parsing single star character with optional whitespace.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        [Theory]
        [InlineData("*")]
        [InlineData(" * ")]
        [InlineData("\t*\n")]
        public void ParseStringToGridLength_SingleStar_ReturnsStarGridLength(string input)
        {
            // Arrange & Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(GridLength.Star, result);
        }

        /// <summary>
        /// Tests parsing valid star values with numeric prefixes.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        /// <param name="expectedValue">The expected numeric value</param>
        [Theory]
        [InlineData("2*", 2.0)]
        [InlineData("2.5*", 2.5)]
        [InlineData("0*", 0.0)]
        [InlineData("42.123*", 42.123)]
        [InlineData(" 3.14* ", 3.14)]
        [InlineData("1.0*", 1.0)]
        public void ParseStringToGridLength_ValidStarWithNumber_ReturnsStarGridLength(string input, double expectedValue)
        {
            // Arrange & Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(new GridLength(expectedValue, GridUnitType.Star), result);
        }

        /// <summary>
        /// Tests parsing valid absolute numeric values.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        /// <param name="expectedValue">The expected numeric value</param>
        [Theory]
        [InlineData("42", 42.0)]
        [InlineData("42.5", 42.5)]
        [InlineData("0", 0.0)]
        [InlineData("0.0", 0.0)]
        [InlineData(" 123.456 ", 123.456)]
        [InlineData("1", 1.0)]
        [InlineData("999999.999", 999999.999)]
        public void ParseStringToGridLength_ValidAbsoluteNumber_ReturnsAbsoluteGridLength(string input, double expectedValue)
        {
            // Arrange & Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(new GridLength(expectedValue), result);
        }

        /// <summary>
        /// Tests parsing empty and whitespace-only strings throws FormatException.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("   ")]
        [InlineData("\t\n\r ")]
        public void ParseStringToGridLength_EmptyOrWhitespaceInput_ThrowsFormatException(string input)
        {
            // Arrange & Act & Assert
#if NET6_0_OR_GREATER
            var exception = Assert.Throws<FormatException>(() => GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan()));
#else
            var exception = Assert.Throws<FormatException>(() => GridLengthTypeConverter.ParseStringToGridLength(input));
#endif

            Assert.Contains("Invalid GridLength format", exception.Message);
        }

        /// <summary>
        /// Tests parsing invalid star formats throws FormatException.
        /// This should exercise the uncovered code path where star parsing fails.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        [Theory]
        [InlineData("abc*")]
        [InlineData(".*")]
        [InlineData("+*")]
        [InlineData("**")]
        [InlineData("auto*")]
        [InlineData("text*")]
        [InlineData(" *")]
        public void ParseStringToGridLength_InvalidStarFormat_ThrowsFormatException(string input)
        {
            // Arrange & Act & Assert
#if NET6_0_OR_GREATER
            var exception = Assert.Throws<FormatException>(() => GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan()));
#else
            var exception = Assert.Throws<FormatException>(() => GridLengthTypeConverter.ParseStringToGridLength(input));
#endif

            Assert.Contains("Invalid GridLength format", exception.Message);
        }

        /// <summary>
        /// Tests parsing completely invalid strings throws FormatException.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        [Theory]
        [InlineData("invalid")]
        [InlineData("xyz")]
        [InlineData("123abc")]
        [InlineData("px")]
        [InlineData("50%")]
        [InlineData("*5")]
        [InlineData("5*5")]
        [InlineData("auto123")]
        [InlineData("123auto")]
        public void ParseStringToGridLength_InvalidFormat_ThrowsFormatException(string input)
        {
            // Arrange & Act & Assert
#if NET6_0_OR_GREATER
            var exception = Assert.Throws<FormatException>(() => GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan()));
#else
            var exception = Assert.Throws<FormatException>(() => GridLengthTypeConverter.ParseStringToGridLength(input));
#endif

            Assert.Contains("Invalid GridLength format", exception.Message);
        }

        /// <summary>
        /// Tests parsing special double values that should throw exceptions due to GridLength constructor constraints.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        [Theory]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData("-1")]
        [InlineData("-42.5")]
        public void ParseStringToGridLength_InvalidDoubleValues_ThrowsException(string input)
        {
            // Arrange & Act & Assert
            // These should either throw FormatException (for invalid parsing) or ArgumentException (from GridLength constructor)
#if NET6_0_OR_GREATER
            Assert.ThrowsAny<Exception>(() => GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan()));
#else
            Assert.ThrowsAny<Exception>(() => GridLengthTypeConverter.ParseStringToGridLength(input));
#endif
        }

        /// <summary>
        /// Tests parsing very large numbers that are valid doubles.
        /// </summary>
        [Fact]
        public void ParseStringToGridLength_VeryLargeNumber_ReturnsAbsoluteGridLength()
        {
            // Arrange
            var input = "1.7976931348623157E+308"; // Near double.MaxValue

            // Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(new GridLength(1.7976931348623157E+308), result);
        }

        /// <summary>
        /// Tests parsing very small positive numbers.
        /// </summary>
        [Fact]
        public void ParseStringToGridLength_VerySmallNumber_ReturnsAbsoluteGridLength()
        {
            // Arrange
            var input = "4.94065645841247E-324"; // Near double.Epsilon

            // Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(new GridLength(4.94065645841247E-324), result);
        }

        /// <summary>
        /// Tests parsing star values with very large numbers.
        /// </summary>
        [Fact]
        public void ParseStringToGridLength_VeryLargeStarNumber_ReturnsStarGridLength()
        {
            // Arrange
            var input = "1000000*";

            // Act
#if NET6_0_OR_GREATER
            var result = GridLengthTypeConverter.ParseStringToGridLength(input.AsSpan());
#else
            var result = GridLengthTypeConverter.ParseStringToGridLength(input);
#endif

            // Assert
            Assert.Equal(new GridLength(1000000, GridUnitType.Star), result);
        }
    }
}