#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.iOSSpecific
{
    public class CellTests
    {
        /// <summary>
        /// Tests that SetDefaultBackgroundColor calls SetValue on the element with the correct property and value.
        /// Input: Valid BindableObject and Color value.
        /// Expected: SetValue is called with DefaultBackgroundColorProperty and the provided color value.
        /// </summary>
        [Fact]
        public void SetDefaultBackgroundColor_ValidElementAndValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var color = Colors.Red;

            // Act
            Cell.SetDefaultBackgroundColor(element, color);

            // Assert
            element.Received(1).SetValue(Cell.DefaultBackgroundColorProperty, color);
        }

        /// <summary>
        /// Tests that SetDefaultBackgroundColor throws ArgumentNullException when element is null.
        /// Input: Null element and valid Color value.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetDefaultBackgroundColor_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var color = Colors.Blue;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Cell.SetDefaultBackgroundColor(element, color));
        }

        /// <summary>
        /// Tests that SetDefaultBackgroundColor works with different color values.
        /// Input: Valid BindableObject and various Color values including default, transparent, and specific colors.
        /// Expected: SetValue is called with DefaultBackgroundColorProperty and the provided color value for each case.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)] // Transparent
        [InlineData(1, 0, 0, 1)] // Red
        [InlineData(0, 1, 0, 1)] // Green  
        [InlineData(0, 0, 1, 1)] // Blue
        [InlineData(1, 1, 1, 1)] // White
        public void SetDefaultBackgroundColor_VariousColorValues_CallsSetValueWithCorrectColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var color = new Color(red, green, blue, alpha);

            // Act
            Cell.SetDefaultBackgroundColor(element, color);

            // Assert
            element.Received(1).SetValue(Cell.DefaultBackgroundColorProperty, color);
        }

        /// <summary>
        /// Tests that SetDefaultBackgroundColor works with default Color value.
        /// Input: Valid BindableObject and default Color value.
        /// Expected: SetValue is called with DefaultBackgroundColorProperty and the default color value.
        /// </summary>
        [Fact]
        public void SetDefaultBackgroundColor_DefaultColorValue_CallsSetValueWithDefaultColor()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var color = default(Color);

            // Act
            Cell.SetDefaultBackgroundColor(element, color);

            // Assert
            element.Received(1).SetValue(Cell.DefaultBackgroundColorProperty, color);
        }

        /// <summary>
        /// Tests that DefaultBackgroundColor throws ArgumentNullException when config parameter is null.
        /// Verifies that the method properly validates its input parameter.
        /// </summary>
        [Fact]
        public void DefaultBackgroundColor_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.iOS, Cell> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.DefaultBackgroundColor());
        }

        /// <summary>
        /// Tests that DefaultBackgroundColor throws NullReferenceException when config.Element is null.
        /// Verifies that the method fails appropriately when the underlying element is not available.
        /// </summary>
        [Fact]
        public void DefaultBackgroundColor_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Cell>>();
            config.Element.Returns((Cell)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.DefaultBackgroundColor());
        }

        /// <summary>
        /// Tests that DefaultBackgroundColor returns the default color value when config has a valid element.
        /// Verifies that the method correctly retrieves the default background color from the bindable property system.
        /// </summary>
        [Fact]
        public void DefaultBackgroundColor_ValidConfigWithValidElement_ReturnsDefaultColor()
        {
            // Arrange
            var cell = new TestCell();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Cell>>();
            config.Element.Returns(cell);

            // Act
            var result = config.DefaultBackgroundColor();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Color>(result);
        }

        /// <summary>
        /// Tests that DefaultBackgroundColor returns the correct color when a custom color has been set.
        /// Verifies that the method retrieves the actual value from the bindable property system.
        /// </summary>
        [Fact]
        public void DefaultBackgroundColor_ValidConfigWithCustomColor_ReturnsSetColor()
        {
            // Arrange
            var cell = new TestCell();
            var expectedColor = Colors.Red;
            Cell.SetDefaultBackgroundColor(cell, expectedColor);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Cell>>();
            config.Element.Returns(cell);

            // Act
            var result = config.DefaultBackgroundColor();

            // Assert
            Assert.Equal(expectedColor, result);
        }

        private class TestCell : Cell
        {
        }
    }
}