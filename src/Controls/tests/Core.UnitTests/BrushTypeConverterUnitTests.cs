using System;
using System.ComponentModel;
using System.Globalization;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class BrushTypeConverterUnitTests : BaseTestFixture
    {
        private readonly BrushTypeConverter _converter = new();

        [Fact]
        public void ConvertNullTest()
        {
            var result = _converter.ConvertFromInvariantString(null);
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Null(brush.Color);
        }

        [Theory]
        [InlineData("rgb(6, 201, 198)")]
        [InlineData("rgba(6, 201, 188, 0.2)")]
        [InlineData("hsl(6, 20%, 45%)")]
        [InlineData("hsla(6, 20%, 45%,0.75)")]
        [InlineData("rgb(100%, 32%, 64%)")]
        [InlineData("rgba(100%, 32%, 64%,0.27)")]
        public void TestBrushTypeConverterWithColorDefinition(string colorDefinition)
        {
            Assert.True(_converter.CanConvertFrom(typeof(string)));
            Assert.NotNull(_converter.ConvertFromInvariantString(colorDefinition));
        }

        [Theory]
        [InlineData("#ff00ff")]
        [InlineData("#00FF33")]
        [InlineData("#00FFff 40%")]
        public void TestBrushTypeConverterWithColorHex(string colorHex)
        {
            Assert.True(_converter.CanConvertFrom(typeof(string)));
            Assert.NotNull(_converter.ConvertFromInvariantString(colorHex));
        }

        [Theory]
        [InlineData("linear-gradient(90deg, rgb(255, 0, 0),rgb(255, 153, 51))")]
        [InlineData("radial-gradient(circle, rgb(255, 0, 0) 25%, rgb(0, 255, 0) 50%, rgb(0, 0, 255) 75%)")]
        public void TestBrushTypeConverterWithBrush(string brush)
        {
            Assert.True(_converter.CanConvertFrom(typeof(string)));
            Assert.NotNull(_converter.ConvertFromInvariantString(brush));
        }

        [Fact]
        public void TestBindingContextPropagation()
        {
            var context = new object();
            var linearGradientBrush = new LinearGradientBrush();

            var firstStop = new GradientStop { Offset = 0.1f, Color = Colors.Red };
            var secondStop = new GradientStop { Offset = 1.0f, Color = Colors.Blue };

            linearGradientBrush.GradientStops.Add(firstStop);
            linearGradientBrush.GradientStops.Add(secondStop);

            linearGradientBrush.BindingContext = context;

            Assert.Same(context, firstStop.BindingContext);
            Assert.Same(context, secondStop.BindingContext);
        }

        [Fact]
        public void TestBrushBindingContext()
        {
            var context = new object();

            var parent = new Grid
            {
                BindingContext = context
            };

            var linearGradientBrush = new LinearGradientBrush();

            var firstStop = new GradientStop { Offset = 0.1f, Color = Colors.Red };
            var secondStop = new GradientStop { Offset = 1.0f, Color = Colors.Blue };

            linearGradientBrush.GradientStops.Add(firstStop);
            linearGradientBrush.GradientStops.Add(secondStop);

            parent.Background = linearGradientBrush;

            Assert.Same(context, parent.Background.BindingContext);
        }

        [Fact]
        public void TestGetGradientStopHashCode()
        {
            var gradientStop = new GradientStop();
            _ = gradientStop.GetHashCode();
            // This test is just validating that calling `GetHashCode` doesn't throw
        }

        [Fact]
        public void ImmutableBrushDoesntSetParent()
        {
            var grid = new Grid();
            grid.Background = SolidColorBrush.Green;
            Assert.Null(SolidColorBrush.Green.Parent);
        }

        [Fact]
        public void InvalidOperationExceptionWhenSettingParentOnImmutableBrush()
        {
            Assert.Throws<InvalidOperationException>(() => SolidColorBrush.Green.Parent = new Grid());
        }
    }

    public partial class BrushTypeConverterTests : BaseTestFixture
    {
        readonly BrushTypeConverter _converter = new();

        /// <summary>
        /// Tests ConvertFrom with a Color input parameter.
        /// Should convert the Color to a Brush using implicit conversion.
        /// </summary>
        [Fact]
        public void ConvertFrom_ColorInput_ReturnsBrush()
        {
            // Arrange
            var color = Colors.Red;

            // Act
            var result = _converter.ConvertFrom(null, null, color);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(color, brush.Color);
        }

        /// <summary>
        /// Tests ConvertFrom with a Paint input parameter.
        /// Should convert the Paint to a Brush using implicit conversion.
        /// </summary>
        [Fact]
        public void ConvertFrom_PaintInput_ReturnsBrush()
        {
            // Arrange
            var paint = new SolidPaint(Colors.Blue);

            // Act
            var result = _converter.ConvertFrom(null, null, paint);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Brush>(result);
        }

        /// <summary>
        /// Tests ConvertFrom with null input.
        /// Should return a SolidColorBrush with null color as fallback.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullInput_ReturnsSolidColorBrushWithNullColor()
        {
            // Act
            var result = _converter.ConvertFrom(null, null, null);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Null(brush.Color);
        }

        /// <summary>
        /// Tests ConvertFrom with various valid string inputs that should parse successfully.
        /// Covers RGB, RGBA, HSL, HSLA color functions and color names.
        /// </summary>
        [Theory]
        [InlineData("rgb(255, 0, 0)")]
        [InlineData("rgba(255, 0, 0, 0.5)")]
        [InlineData("hsl(0, 100%, 50%)")]
        [InlineData("hsla(0, 100%, 50%, 0.8)")]
        [InlineData("Red")]
        [InlineData("Color.Red")]
        [InlineData("#FF0000")]
        public void ConvertFrom_ValidColorStrings_ReturnsSolidColorBrush(string colorString)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, colorString);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SolidColorBrush>(result);
        }

        /// <summary>
        /// Tests ConvertFrom with valid gradient strings.
        /// Should return appropriate gradient brush types.
        /// </summary>
        [Theory]
        [InlineData("linear-gradient(90deg, red, blue)")]
        [InlineData("radial-gradient(circle, red 25%, blue 75%)")]
        public void ConvertFrom_ValidGradientStrings_ReturnsGradientBrush(string gradientString)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, gradientString);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<GradientBrush>(result);
        }

        /// <summary>
        /// Tests ConvertFrom with invalid gradient strings that cannot be parsed.
        /// Should fall through to the fallback SolidColorBrush with null color.
        /// </summary>
        [Theory]
        [InlineData("linear-gradient()")]
        [InlineData("radial-gradient(invalid)")]
        [InlineData("linear-gradient(invalid-syntax")]
        public void ConvertFrom_InvalidGradientStrings_ReturnsSolidColorBrushWithNullColor(string invalidGradient)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, invalidGradient);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Null(brush.Color);
        }

        /// <summary>
        /// Tests ConvertFrom with strings that don't match any expected patterns.
        /// Should return SolidColorBrush with null color as fallback.
        /// </summary>
        [Theory]
        [InlineData("invalid.color.name")]
        [InlineData("not.a.color")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("completely invalid string")]
        public void ConvertFrom_InvalidStrings_ReturnsSolidColorBrushWithNullColor(string invalidString)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, invalidString);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Null(brush.Color);
        }

        /// <summary>
        /// Tests ConvertFrom with non-string, non-Color, non-Paint input types.
        /// Should return SolidColorBrush with null color as fallback.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertFrom_NonSupportedTypes_ReturnsSolidColorBrushWithNullColor(object input)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, input);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Null(brush.Color);
        }

        /// <summary>
        /// Tests ConvertFrom with strings containing only whitespace.
        /// Should be trimmed and fall through to fallback behavior.
        /// </summary>
        [Theory]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void ConvertFrom_WhitespaceStrings_ReturnsSolidColorBrushWithNullColor(string whitespaceString)
        {
            // Act
            var result = _converter.ConvertFrom(null, null, whitespaceString);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Null(brush.Color);
        }

        /// <summary>
        /// Tests ConvertFrom with edge case Color values.
        /// Should properly convert edge case colors to brushes.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)] // Transparent
        [InlineData(1, 1, 1, 1)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        public void ConvertFrom_EdgeCaseColors_ReturnsBrush(float r, float g, float b, float a)
        {
            // Arrange
            var color = new Color(r, g, b, a);

            // Act
            var result = _converter.ConvertFrom(null, null, color);

            // Assert
            Assert.NotNull(result);
            var brush = Assert.IsType<SolidColorBrush>(result);
            Assert.Equal(color, brush.Color);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when destinationType is Paint type.
        /// </summary>
        [Fact]
        public void CanConvertTo_DestinationTypeIsPaint_ReturnsTrue()
        {
            // Arrange
            var converter = new BrushTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var destinationType = typeof(Paint);

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
            var converter = new BrushTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false when destinationType is not Paint type.
        /// Verifies behavior with various common types that are not Paint.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(BrushTypeConverter))]
        public void CanConvertTo_DestinationTypeIsNotPaint_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = new BrushTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo behavior is not affected by context parameter value.
        /// Verifies that both null context and valid context produce the same result.
        /// </summary>
        [Theory]
        [InlineData(true)]  // Paint type
        [InlineData(false)] // Non-Paint type
        public void CanConvertTo_ContextParameterDoesNotAffectResult(bool usePaintType)
        {
            // Arrange
            var converter = new BrushTypeConverter();
            var mockedContext = Substitute.For<ITypeDescriptorContext>();
            var destinationType = usePaintType ? typeof(Paint) : typeof(string);

            // Act
            var resultWithNullContext = converter.CanConvertTo(null, destinationType);
            var resultWithMockedContext = converter.CanConvertTo(mockedContext, destinationType);

            // Assert
            Assert.Equal(resultWithNullContext, resultWithMockedContext);
            Assert.Equal(usePaintType, resultWithNullContext);
        }
    }


    public class BrushTypeConverterConvertToTests
    {
        private readonly BrushTypeConverter _converter = new();

        /// <summary>
        /// Tests that ConvertTo successfully converts a Brush to Paint when destinationType is typeof(Paint).
        /// Input: SolidColorBrush and destinationType typeof(Paint).
        /// Expected: Returns Paint object without throwing exception.
        /// </summary>
        [Fact]
        public void ConvertTo_BrushToPaint_ReturnsConvertedPaint()
        {
            // Arrange
            var brush = SolidColorBrush.Green;
            var destinationType = typeof(Paint);

            // Act
            var result = _converter.ConvertTo(null, null, brush, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Paint>(result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// Input: null value with destinationType typeof(Paint).
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            object value = null;
            var destinationType = typeof(Paint);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a Brush.
        /// Input: Various non-Brush values with destinationType typeof(Paint).
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_NonBrushValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var destinationType = typeof(Paint);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when destinationType is not typeof(Paint).
        /// Input: SolidColorBrush with various non-Paint destination types.
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(Brush))]
        [InlineData(typeof(object))]
        public void ConvertTo_WrongDestinationType_ThrowsNotSupportedException(Type destinationType)
        {
            // Arrange
            var brush = SolidColorBrush.Green;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, brush, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when destinationType is null.
        /// Input: SolidColorBrush with null destinationType.
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertTo_NullDestinationType_ThrowsNotSupportedException()
        {
            // Arrange
            var brush = SolidColorBrush.Green;
            Type destinationType = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, brush, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with provided context and culture parameters.
        /// Input: SolidColorBrush with non-null context and culture, destinationType typeof(Paint).
        /// Expected: Returns Paint object, context and culture parameters don't affect behavior.
        /// </summary>
        [Fact]
        public void ConvertTo_WithContextAndCulture_ReturnsConvertedPaint()
        {
            // Arrange
            var brush = SolidColorBrush.Green;
            var destinationType = typeof(Paint);
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.ConvertTo(context, culture, brush, destinationType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Paint>(result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException for edge case combinations.
        /// Input: null value with null destinationType.
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValueAndDestinationType_ThrowsNotSupportedException()
        {
            // Arrange
            object value = null;
            Type destinationType = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertTo(null, null, value, destinationType));
        }
    }
}