using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class BoundsTypeConverterTests
    {
        /// <summary>
        /// Tests that ConvertFrom successfully converts a valid two-component string (x,y) into a Rect with AutoSize dimensions.
        /// Input: Valid "x,y" format string.
        /// Expected: Returns Rect with parsed x,y coordinates and AutoSize for width and height.
        /// </summary>
        [Theory]
        [InlineData("10,20", 10.0, 20.0)]
        [InlineData("0,0", 0.0, 0.0)]
        [InlineData("-5.5,10.7", -5.5, 10.7)]
        [InlineData("1.23456789,9.87654321", 1.23456789, 9.87654321)]
        public void ConvertFrom_ValidTwoComponentString_ReturnsRectWithAutoSize(string input, double expectedX, double expectedY)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, rect.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, rect.Height);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a valid four-component string (x,y,w,h) into a Rect.
        /// Input: Valid "x,y,w,h" format string with numeric values.
        /// Expected: Returns Rect with all parsed values.
        /// </summary>
        [Theory]
        [InlineData("10,20,30,40", 10.0, 20.0, 30.0, 40.0)]
        [InlineData("0,0,100,200", 0.0, 0.0, 100.0, 200.0)]
        [InlineData("-10,-20,50,60", -10.0, -20.0, 50.0, 60.0)]
        [InlineData("1.5,2.5,3.5,4.5", 1.5, 2.5, 3.5, 4.5)]
        public void ConvertFrom_ValidFourComponentString_ReturnsRectWithAllValues(string input, double expectedX, double expectedY, double expectedW, double expectedH)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);
            Assert.Equal(expectedW, rect.Width);
            Assert.Equal(expectedH, rect.Height);
        }

        /// <summary>
        /// Tests that ConvertFrom handles AutoSize keyword for width component.
        /// Input: Four-component string with "AutoSize" as third component.
        /// Expected: Returns Rect with AutoSize value for width.
        /// </summary>
        [Theory]
        [InlineData("10,20,AutoSize,40", 10.0, 20.0, -1.0, 40.0)]
        [InlineData("5,15,autosize,25", 5.0, 15.0, -1.0, 25.0)]
        [InlineData("0,0, AutoSize ,100", 0.0, 0.0, -1.0, 100.0)]
        [InlineData("1,2,AUTOSIZE,3", 1.0, 2.0, -1.0, 3.0)]
        public void ConvertFrom_AutoSizeWidth_ReturnsRectWithAutoSizeWidth(string input, double expectedX, double expectedY, double expectedW, double expectedH)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);
            Assert.Equal(expectedW, rect.Width);
            Assert.Equal(expectedH, rect.Height);
        }

        /// <summary>
        /// Tests that ConvertFrom handles AutoSize keyword for height component.
        /// Input: Four-component string with "AutoSize" as fourth component.
        /// Expected: Returns Rect with AutoSize value for height.
        /// </summary>
        [Theory]
        [InlineData("10,20,30,AutoSize", 10.0, 20.0, 30.0, -1.0)]
        [InlineData("5,15,25,autosize", 5.0, 15.0, 25.0, -1.0)]
        [InlineData("0,0,100, AutoSize ", 0.0, 0.0, 100.0, -1.0)]
        [InlineData("1,2,3,AUTOSIZE", 1.0, 2.0, 3.0, -1.0)]
        public void ConvertFrom_AutoSizeHeight_ReturnsRectWithAutoSizeHeight(string input, double expectedX, double expectedY, double expectedW, double expectedH)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);
            Assert.Equal(expectedW, rect.Width);
            Assert.Equal(expectedH, rect.Height);
        }

        /// <summary>
        /// Tests that ConvertFrom handles both width and height as AutoSize.
        /// Input: Four-component string with "AutoSize" for both width and height.
        /// Expected: Returns Rect with AutoSize values for both dimensions.
        /// </summary>
        [Fact]
        public void ConvertFrom_BothAutoSize_ReturnsRectWithBothAutoSize()
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, "10,20,AutoSize,AutoSize");

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(10.0, rect.X);
            Assert.Equal(20.0, rect.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, rect.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, rect.Height);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for null input.
        /// Input: null value.
        /// Expected: Throws InvalidOperationException.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullInput_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, null));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for empty string input.
        /// Input: Empty string.
        /// Expected: Throws InvalidOperationException.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, ""));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for invalid component count.
        /// Input: Strings with 1, 3, or 5+ components.
        /// Expected: Throws InvalidOperationException.
        /// </summary>
        [Theory]
        [InlineData("10")]
        [InlineData("10,20,30")]
        [InlineData("10,20,30,40,50")]
        [InlineData("a")]
        [InlineData("a,b,c,d,e,f")]
        public void ConvertFrom_InvalidComponentCount_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for invalid numeric values.
        /// Input: Strings with non-numeric components where numbers are expected.
        /// Expected: Throws InvalidOperationException.
        /// </summary>
        [Theory]
        [InlineData("abc,def")]
        [InlineData("10,abc")]
        [InlineData("abc,20")]
        [InlineData("10,20,abc,40")]
        [InlineData("10,20,30,abc")]
        [InlineData("abc,20,30,40")]
        [InlineData("10,abc,30,40")]
        public void ConvertFrom_InvalidNumericValues_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for partially valid four-component strings.
        /// Input: Four-component strings where some components are invalid but not AutoSize.
        /// Expected: Throws InvalidOperationException.
        /// </summary>
        [Theory]
        [InlineData("10,20,invalid,40")]
        [InlineData("10,20,30,invalid")]
        [InlineData("10,20,NotAutoSize,40")]
        [InlineData("10,20,30,NotAutoSize")]
        public void ConvertFrom_PartiallyValidFourComponents_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles extreme numeric values correctly.
        /// Input: Strings with extreme double values like infinity, NaN, max/min values.
        /// Expected: Returns Rect with parsed extreme values or throws for invalid cases.
        /// </summary>
        [Theory]
        [InlineData("1.7976931348623157E+308,0", double.MaxValue, 0.0)] // double.MaxValue
        [InlineData("0,1.7976931348623157E+308", 0.0, double.MaxValue)]
        [InlineData("-1.7976931348623157E+308,0", double.MinValue, 0.0)] // double.MinValue
        [InlineData("0,-1.7976931348623157E+308", 0.0, double.MinValue)]
        public void ConvertFrom_ExtremeNumericValues_ReturnsRectWithExtremeValues(string input, double expectedX, double expectedY)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);
        }

        /// <summary>
        /// Tests that ConvertFrom throws InvalidOperationException for special double values that cannot be parsed.
        /// Input: Strings representing NaN, Infinity values.
        /// Expected: Throws InvalidOperationException since these don't parse as valid numbers with NumberStyles.Number.
        /// </summary>
        [Theory]
        [InlineData("NaN,0")]
        [InlineData("0,NaN")]
        [InlineData("Infinity,0")]
        [InlineData("0,Infinity")]
        [InlineData("-Infinity,0")]
        [InlineData("0,-Infinity")]
        public void ConvertFrom_SpecialDoubleValues_ThrowsInvalidOperationException(string input)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => converter.ConvertFrom(null, null, input));
            Assert.Contains("Cannot convert", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertFrom handles whitespace around components correctly.
        /// Input: Strings with extra whitespace around numeric values.
        /// Expected: Returns Rect with parsed values, ignoring whitespace.
        /// </summary>
        [Theory]
        [InlineData(" 10 , 20 ", 10.0, 20.0)]
        [InlineData("  10  ,  20  ,  30  ,  40  ", 10.0, 20.0, 30.0, 40.0)]
        public void ConvertFrom_WhitespaceAroundComponents_ReturnsRectWithParsedValues(string input, double expectedX, double expectedY, double expectedW = -1.0, double expectedH = -1.0)
        {
            // Arrange
            var converter = new BoundsTypeConverter();

            // Act
            var result = converter.ConvertFrom(null, null, input);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);

            if (input.Split(',').Length == 2)
            {
                Assert.Equal(AbsoluteLayout.AutoSize, rect.Width);
                Assert.Equal(AbsoluteLayout.AutoSize, rect.Height);
            }
            else
            {
                Assert.Equal(expectedW, rect.Width);
                Assert.Equal(expectedH, rect.Height);
            }
        }

        /// <summary>
        /// Tests that ConvertFrom handles non-string objects by calling ToString().
        /// Input: Non-string objects that have valid ToString() representations.
        /// Expected: Returns Rect by converting via ToString().
        /// </summary>
        [Fact]
        public void ConvertFrom_NonStringObject_ConvertsViaToString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var customObject = new CustomStringObject("10,20");

            // Act
            var result = converter.ConvertFrom(null, null, customObject);

            // Assert
            Assert.IsType<Rect>(result);
            var rect = (Rect)result;
            Assert.Equal(10.0, rect.X);
            Assert.Equal(20.0, rect.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, rect.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, rect.Height);
        }

        /// <summary>
        /// Helper class for testing non-string object conversion via ToString().
        /// </summary>
        private class CustomStringObject
        {
            private readonly string _value;

            public CustomStringObject(string value)
            {
                _value = value;
            }

            public override string ToString() => _value;
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            object value = null;
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a Rect.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_NonRectValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            Type destinationType = typeof(string);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo correctly formats a basic Rect with positive values.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidRect_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;
            var rect = new Rect(10, 20, 30, 40);
            Type destinationType = typeof(string);

            // Act
            var result = converter.ConvertTo(context, culture, rect, destinationType);

            // Assert
            Assert.Equal("10, 20, 30, 40", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly formats a Rect with zero values.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithZeroValues_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(0, 0, 0, 0);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal("0, 0, 0, 0", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly formats a Rect with negative values.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithNegativeValues_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(-10, -20, -30, -40);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal("-10, -20, -30, -40", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly handles AutoSize for width only.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithAutoSizeWidth_ReturnsFormattedStringWithAutoSize()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(10, 20, AbsoluteLayout.AutoSize, 40);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal("10, 20, AutoSize, 40", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly handles AutoSize for height only.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithAutoSizeHeight_ReturnsFormattedStringWithAutoSize()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(10, 20, 30, AbsoluteLayout.AutoSize);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal("10, 20, 30, AutoSize", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly handles AutoSize for both width and height.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithAutoSizeWidthAndHeight_ReturnsFormattedStringWithAutoSize()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(10, 20, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal("10, 20, AutoSize, AutoSize", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly formats a Rect with extreme double values.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithExtremeValues_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal($"{double.MaxValue.ToString(CultureInfo.InvariantCulture)}, {double.MinValue.ToString(CultureInfo.InvariantCulture)}, {double.MaxValue.ToString(CultureInfo.InvariantCulture)}, {double.MinValue.ToString(CultureInfo.InvariantCulture)}", result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly formats a Rect with special double values like NaN and Infinity.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.PositiveInfinity, double.NegativeInfinity, 0)]
        [InlineData(double.PositiveInfinity, double.NaN, 0, double.NegativeInfinity)]
        public void ConvertTo_RectWithSpecialDoubleValues_ReturnsFormattedString(double x, double y, double width, double height)
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(x, y, width, height);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            var expectedWidth = width == AbsoluteLayout.AutoSize ? "AutoSize" : width.ToString(CultureInfo.InvariantCulture);
            var expectedHeight = height == AbsoluteLayout.AutoSize ? "AutoSize" : height.ToString(CultureInfo.InvariantCulture);
            var expected = $"{x.ToString(CultureInfo.InvariantCulture)}, {y.ToString(CultureInfo.InvariantCulture)}, {expectedWidth}, {expectedHeight}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo correctly formats a Rect with decimal values.
        /// </summary>
        [Fact]
        public void ConvertTo_RectWithDecimalValues_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(1.5, 2.75, 3.125, 4.999);

            // Act
            var result = converter.ConvertTo(null, null, rect, typeof(string));

            // Assert
            Assert.Equal("1.5, 2.75, 3.125, 4.999", result);
        }

        /// <summary>
        /// Tests that ConvertTo uses InvariantCulture regardless of the culture parameter.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentCulture_UsesInvariantCulture()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(1.5, 2.5, 3.5, 4.5);
            var culture = new CultureInfo("de-DE"); // German culture uses comma as decimal separator

            // Act
            var result = converter.ConvertTo(null, culture, rect, typeof(string));

            // Assert
            // Should use dots as decimal separators regardless of German culture
            Assert.Equal("1.5, 2.5, 3.5, 4.5", result);
        }

        /// <summary>
        /// Tests that ConvertTo works with various destination types parameter (though behavior is the same).
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        public void ConvertTo_WithDifferentDestinationTypes_ReturnsString(Type destinationType)
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(1, 2, 3, 4);

            // Act
            var result = converter.ConvertTo(null, null, rect, destinationType);

            // Assert
            Assert.Equal("1, 2, 3, 4", result);
        }

        /// <summary>
        /// Tests that ConvertTo works with different context parameter (should not affect behavior).
        /// </summary>
        [Fact]
        public void ConvertTo_WithContext_ReturnsFormattedString()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var rect = new Rect(1, 2, 3, 4);
            ITypeDescriptorContext context = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.ConvertTo(context, null, rect, typeof(string));

            // Assert
            Assert.Equal("1, 2, 3, 4", result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destination type is string.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destination type is null.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Input conditions: Different Type objects representing various .NET types.
        /// Expected result: False for all non-string types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Rect))]
        [InlineData(typeof(BoundsTypeConverter))]
        [InlineData(typeof(void))]
        [InlineData(typeof(char))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(long))]
        [InlineData(typeof(short))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(sbyte))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo works correctly when context is null.
        /// Input conditions: Null context with string destination type.
        /// Expected result: True, as context should not affect the conversion capability.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContext_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo works correctly when context is null and destination type is not string.
        /// Input conditions: Null context with non-string destination type.
        /// Expected result: False, as only string conversions are supported.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullContext_NonStringDestinationType_ReturnsFalse()
        {
            // Arrange
            var converter = new BoundsTypeConverter();
            var destinationType = typeof(int);

            // Act
            var result = converter.CanConvertTo(null, destinationType);

            // Assert
            Assert.False(result);
        }
    }
}