using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ShadowTypeConverterTests
    {
        /// <summary>
        /// Tests that ConvertTo throws ArgumentNullException when value parameter is null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new ShadowTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                converter.ConvertTo(null, null, null, typeof(string)));

            Assert.Equal("value", exception.ParamName);
        }

        /// <summary>
        /// Tests that ConvertTo returns properly formatted string when given a valid Shadow with SolidColorBrush.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidShadowWithSolidColorBrush_ReturnsFormattedString()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var shadow = new Shadow
            {
                Offset = new Point(1.5f, 2.5f),
                Radius = 10.0f,
                Opacity = 0.8f,
                Brush = new SolidColorBrush(Colors.Red)
            };

            // Act
            var result = converter.ConvertTo(null, null, shadow, typeof(string));

            // Assert
            Assert.Equal("1.5 2.5 10 #FF0000 0.8", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles various numeric values correctly using InvariantCulture formatting.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 0f, "#000000", "0 0 0 #000000 0")]
        [InlineData(-1.5f, -2.5f, 5.25f, 1.0f, "#FFFFFF", "-1.5 -2.5 5.25 #FFFFFF 1")]
        [InlineData(100.123f, 200.456f, 15.789f, 0.5f, "#FF00FF", "100.123 200.456 15.789 #FF00FF 0.5")]
        public void ConvertTo_VariousNumericValues_ReturnsCorrectlyFormattedString(
            float offsetX, float offsetY, float radius, float opacity, string colorHex, string expected)
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var color = Color.FromArgb(colorHex);
            var shadow = new Shadow
            {
                Offset = new Point(offsetX, offsetY),
                Radius = radius,
                Opacity = opacity,
                Brush = new SolidColorBrush(color)
            };

            // Act
            var result = converter.ConvertTo(null, null, shadow, typeof(string));

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo throws InvalidOperationException when Shadow has SolidColorBrush with null Color.
        /// </summary>
        [Fact]
        public void ConvertTo_ShadowWithSolidColorBrushNullColor_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var shadow = new Shadow
            {
                Offset = new Point(1.0f, 1.0f),
                Radius = 5.0f,
                Opacity = 1.0f,
                Brush = new SolidColorBrush() // Default constructor leaves Color as null
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertTo(null, null, shadow, typeof(string)));

            Assert.Contains("Cannot convert Shadow to string: Brush is not a valid SolidColorBrush or has no Color", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertTo throws InvalidOperationException when Shadow has non-SolidColorBrush.
        /// </summary>
        [Fact]
        public void ConvertTo_ShadowWithNonSolidColorBrush_ThrowsInvalidOperationException()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var mockBrush = Substitute.For<Brush>();
            var shadow = new Shadow
            {
                Offset = new Point(1.0f, 1.0f),
                Radius = 5.0f,
                Opacity = 1.0f,
                Brush = mockBrush
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertTo(null, null, shadow, typeof(string)));

            Assert.Contains("Cannot convert Shadow to string: Brush is not a valid SolidColorBrush or has no Color", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertTo throws InvalidOperationException when value is not a Shadow.
        /// </summary>
        [Theory]
        [InlineData("not a shadow")]
        [InlineData(42)]
        [InlineData(true)]
        public void ConvertTo_NonShadowObject_ThrowsInvalidOperationException(object nonShadowValue)
        {
            // Arrange
            var converter = new ShadowTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                converter.ConvertTo(null, null, nonShadowValue, typeof(string)));

            Assert.Equal($"Cannot convert \"{nonShadowValue}\" into string.", exception.Message);
        }

        /// <summary>
        /// Tests that ConvertTo handles extreme float values correctly.
        /// </summary>
        [Theory]
        [InlineData(float.MinValue, float.MinValue, float.MinValue, float.MinValue)]
        [InlineData(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue)]
        [InlineData(0f, 0f, 0f, 0f)]
        public void ConvertTo_ExtremeFloatValues_ReturnsFormattedString(
            float offsetX, float offsetY, float radius, float opacity)
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var shadow = new Shadow
            {
                Offset = new Point(offsetX, offsetY),
                Radius = radius,
                Opacity = opacity,
                Brush = new SolidColorBrush(Colors.Black)
            };

            // Act
            var result = converter.ConvertTo(null, null, shadow, typeof(string));

            // Assert
            var expectedOffsetX = offsetX.ToString(CultureInfo.InvariantCulture);
            var expectedOffsetY = offsetY.ToString(CultureInfo.InvariantCulture);
            var expectedRadius = radius.ToString(CultureInfo.InvariantCulture);
            var expectedOpacity = opacity.ToString(CultureInfo.InvariantCulture);
            var expected = $"{expectedOffsetX} {expectedOffsetY} {expectedRadius} #000000 {expectedOpacity}";

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo handles special float values (NaN, Infinity) correctly.
        /// </summary>
        [Theory]
        [InlineData(float.NaN, 1f, 1f, 1f)]
        [InlineData(1f, float.NaN, 1f, 1f)]
        [InlineData(1f, 1f, float.NaN, 1f)]
        [InlineData(1f, 1f, 1f, float.NaN)]
        [InlineData(float.PositiveInfinity, 1f, 1f, 1f)]
        [InlineData(float.NegativeInfinity, 1f, 1f, 1f)]
        public void ConvertTo_SpecialFloatValues_ReturnsFormattedString(
            float offsetX, float offsetY, float radius, float opacity)
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var shadow = new Shadow
            {
                Offset = new Point(offsetX, offsetY),
                Radius = radius,
                Opacity = opacity,
                Brush = new SolidColorBrush(Colors.White)
            };

            // Act
            var result = converter.ConvertTo(null, null, shadow, typeof(string));

            // Assert
            var expectedOffsetX = offsetX.ToString(CultureInfo.InvariantCulture);
            var expectedOffsetY = offsetY.ToString(CultureInfo.InvariantCulture);
            var expectedRadius = radius.ToString(CultureInfo.InvariantCulture);
            var expectedOpacity = opacity.ToString(CultureInfo.InvariantCulture);
            var expected = $"{expectedOffsetX} {expectedOffsetY} {expectedRadius} #FFFFFF {expectedOpacity}";

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ConvertTo ignores context and culture parameters and uses InvariantCulture.
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentCultureAndContext_UsesInvariantCulture()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var mockContext = Substitute.For<ITypeDescriptorContext>();
            var germanCulture = new CultureInfo("de-DE"); // Uses comma as decimal separator
            var shadow = new Shadow
            {
                Offset = new Point(1.5f, 2.5f),
                Radius = 10.5f,
                Opacity = 0.75f,
                Brush = new SolidColorBrush(Colors.Blue)
            };

            // Act
            var result = converter.ConvertTo(mockContext, germanCulture, shadow, typeof(string));

            // Assert
            // Should use period as decimal separator (InvariantCulture) regardless of passed culture
            Assert.Equal("1.5 2.5 10.5 #0000FF 0.75", result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is Shadow type.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsShadow_ReturnsTrue()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(Shadow);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

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
            var converter = new ShadowTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type destinationType = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not Shadow type.
        /// Tests various common types to ensure only Shadow type returns true.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Color))]
        [InlineData(typeof(Brush))]
        public void CanConvertTo_DestinationTypeIsNotShadow_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is Shadow type and context is null.
        /// Verifies that null context does not affect the result.
        /// </summary>
        [Fact]
        public void CanConvertTo_ContextIsNullAndDestinationTypeIsShadow_ReturnsTrue()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            ITypeDescriptorContext context = null;
            var destinationType = typeof(Shadow);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not Shadow type and context is null.
        /// Verifies that null context does not affect the result for non-Shadow types.
        /// </summary>
        [Fact]
        public void CanConvertTo_ContextIsNullAndDestinationTypeIsNotShadow_ReturnsFalse()
        {
            // Arrange
            var converter = new ShadowTypeConverter();
            ITypeDescriptorContext context = null;
            var destinationType = typeof(string);

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }
    }
}