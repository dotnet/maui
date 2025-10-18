#nullable disable

using System;
using System.Globalization;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ContentConverterTests
    {
        /// <summary>
        /// Tests that ConvertBack method always throws NotImplementedException regardless of input parameters.
        /// Tests various combinations of null, valid, and edge case parameter values.
        /// Expected result: NotImplementedException is always thrown.
        /// </summary>
        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData("test", typeof(string), "parameter", null)]
        [InlineData(42, typeof(int), null, null)]
        [InlineData(null, typeof(object), "param", null)]
        [InlineData("", typeof(string), "", null)]
        [InlineData(double.NaN, typeof(double), double.PositiveInfinity, null)]
        [InlineData(int.MaxValue, typeof(int), int.MinValue, null)]
        public void ConvertBack_AnyParameters_ThrowsNotImplementedException(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Arrange
            var converter = new ContentConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(value, targetType, parameter, culture));
        }

        /// <summary>
        /// Tests ConvertBack method with various CultureInfo values.
        /// Expected result: NotImplementedException is always thrown regardless of culture.
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("")]
        public void ConvertBack_WithDifferentCultures_ThrowsNotImplementedException(string cultureName)
        {
            // Arrange
            var converter = new ContentConverter();
            var culture = new CultureInfo(cultureName);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack("value", typeof(string), "param", culture));
        }

        /// <summary>
        /// Tests ConvertBack method with InvariantCulture.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertBack_WithInvariantCulture_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ContentConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack("test", typeof(string), null, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Tests ConvertBack method with complex object types as parameters.
        /// Expected result: NotImplementedException is thrown regardless of complex parameter types.
        /// </summary>
        [Fact]
        public void ConvertBack_WithComplexTypes_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ContentConverter();
            var complexValue = new { Name = "Test", Value = 42 };
            var complexParameter = new string[] { "item1", "item2" };

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(complexValue, typeof(object), complexParameter, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Tests ConvertBack method with extreme string values.
        /// Expected result: NotImplementedException is thrown for any string input.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Very long string with special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        public void ConvertBack_WithStringEdgeCases_ThrowsNotImplementedException(string value)
        {
            // Arrange
            var converter = new ContentConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(value, typeof(string), null, null));
        }

        /// <summary>
        /// Tests ConvertBack method with numeric edge cases.
        /// Expected result: NotImplementedException is thrown for any numeric input.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void ConvertBack_WithNumericEdgeCases_ThrowsNotImplementedException(int value)
        {
            // Arrange
            var converter = new ContentConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(value, typeof(int), null, null));
        }

        /// <summary>
        /// Tests ConvertBack method with floating point edge cases.
        /// Expected result: NotImplementedException is thrown for any floating point input.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void ConvertBack_WithFloatingPointEdgeCases_ThrowsNotImplementedException(double value)
        {
            // Arrange
            var converter = new ContentConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(value, typeof(double), null, null));
        }

        /// <summary>
        /// Tests that Convert returns null when the input value is null.
        /// </summary>
        [Fact]
        public void Convert_NullValue_ReturnsNull()
        {
            // Arrange
            var converter = new ContentConverter();
            var targetType = typeof(object);
            var parameter = Substitute.For<ContentPresenter>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(null, targetType, parameter, culture);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Convert returns the same view when the input value is a View.
        /// </summary>
        [Fact]
        public void Convert_ViewValue_ReturnsConfiguredView()
        {
            // Arrange
            var converter = new ContentConverter();
            var view = Substitute.For<View>();
            var targetType = typeof(object);
            var parameter = Substitute.For<ContentPresenter>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(view, targetType, parameter, culture);

            // Assert
            Assert.Same(view, result);
        }

        /// <summary>
        /// Tests that Convert returns a Label with the specified text when the input value is a string.
        /// This test covers the previously uncovered string conversion path.
        /// </summary>
        /// <param name="textContent">The string content to convert</param>
        [Theory]
        [InlineData("test content")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Very long string content that tests the behavior with longer text input to ensure proper handling")]
        [InlineData("\n\r\t")]
        [InlineData("Special chars: !@#$%^&*()")]
        public void Convert_StringValue_ReturnsLabelWithText(string textContent)
        {
            // Arrange
            var converter = new ContentConverter();
            var targetType = typeof(object);
            var parameter = Substitute.For<ContentPresenter>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(textContent, targetType, parameter, culture);

            // Assert
            Assert.IsType<Label>(result);
            var label = (Label)result;
            Assert.Equal(textContent, label.Text);
        }

        /// <summary>
        /// Tests that Convert returns the original value when the input is neither a View nor a string.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        [InlineData(false)]
        public void Convert_NonViewNonStringValue_ReturnsOriginalValue(object value)
        {
            // Arrange
            var converter = new ContentConverter();
            var targetType = typeof(object);
            var parameter = Substitute.For<ContentPresenter>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.Same(value, result);
        }

        /// <summary>
        /// Tests that Convert works correctly when parameter is null.
        /// </summary>
        [Fact]
        public void Convert_WithNullParameter_StringValue_ReturnsLabel()
        {
            // Arrange
            var converter = new ContentConverter();
            var textContent = "test with null parameter";
            var targetType = typeof(object);
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(textContent, targetType, null, culture);

            // Assert
            Assert.IsType<Label>(result);
            var label = (Label)result;
            Assert.Equal(textContent, label.Text);
        }

        /// <summary>
        /// Tests that Convert works correctly when parameter is not a ContentPresenter.
        /// </summary>
        [Fact]
        public void Convert_WithNonContentPresenterParameter_StringValue_ReturnsLabel()
        {
            // Arrange
            var converter = new ContentConverter();
            var textContent = "test with non-ContentPresenter parameter";
            var targetType = typeof(object);
            var parameter = "not a ContentPresenter";
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(textContent, targetType, parameter, culture);

            // Assert
            Assert.IsType<Label>(result);
            var label = (Label)result;
            Assert.Equal(textContent, label.Text);
        }

        /// <summary>
        /// Tests that Convert works correctly with different target types.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(View))]
        [InlineData(typeof(Label))]
        [InlineData(typeof(object))]
        public void Convert_StringValue_WithDifferentTargetTypes_ReturnsLabel(Type targetType)
        {
            // Arrange
            var converter = new ContentConverter();
            var textContent = "test content";
            var parameter = Substitute.For<ContentPresenter>();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.Convert(textContent, targetType, parameter, culture);

            // Assert
            Assert.IsType<Label>(result);
            var label = (Label)result;
            Assert.Equal(textContent, label.Text);
        }

        /// <summary>
        /// Tests that Convert works correctly with different culture values.
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("de-DE")]
        public void Convert_StringValue_WithDifferentCultures_ReturnsLabel(string cultureName)
        {
            // Arrange
            var converter = new ContentConverter();
            var textContent = "test content";
            var targetType = typeof(object);
            var parameter = Substitute.For<ContentPresenter>();
            var culture = new CultureInfo(cultureName);

            // Act
            var result = converter.Convert(textContent, targetType, parameter, culture);

            // Assert
            Assert.IsType<Label>(result);
            var label = (Label)result;
            Assert.Equal(textContent, label.Text);
        }

        /// <summary>
        /// Tests that Convert works correctly with null culture.
        /// </summary>
        [Fact]
        public void Convert_StringValue_WithNullCulture_ReturnsLabel()
        {
            // Arrange
            var converter = new ContentConverter();
            var textContent = "test content";
            var targetType = typeof(object);
            var parameter = Substitute.For<ContentPresenter>();

            // Act
            var result = converter.Convert(textContent, targetType, parameter, null);

            // Assert
            Assert.IsType<Label>(result);
            var label = (Label)result;
            Assert.Equal(textContent, label.Text);
        }
    }
}