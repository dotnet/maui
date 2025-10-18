#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.UnitTests
{
    /// <summary>
    /// Unit tests for the Switch class in TizenSpecific platform configuration.
    /// </summary>
    public class SwitchTests
    {
        /// <summary>
        /// Tests that SetColor method calls SetValue on the element with correct parameters.
        /// Verifies that the ColorProperty and color value are passed correctly to SetValue.
        /// </summary>
        [Fact]
        public void SetColor_ValidElementAndColor_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var color = Colors.Red;

            // Act
            Switch.SetColor(mockElement, color);

            // Assert
            mockElement.Received(1).SetValue(Switch.ColorProperty, color);
        }

        /// <summary>
        /// Tests that SetColor method throws NullReferenceException when element is null.
        /// Verifies proper error handling for null element parameter.
        /// </summary>
        [Fact]
        public void SetColor_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;
            var color = Colors.Blue;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Switch.SetColor(nullElement, color));
        }

        /// <summary>
        /// Tests SetColor method with various color values to ensure proper delegation.
        /// Verifies that different color values are correctly passed to SetValue.
        /// </summary>
        [Theory]
        [InlineData(1.0, 0.0, 0.0, 1.0)] // Red
        [InlineData(0.0, 1.0, 0.0, 1.0)] // Green  
        [InlineData(0.0, 0.0, 1.0, 1.0)] // Blue
        [InlineData(0.0, 0.0, 0.0, 0.0)] // Transparent
        [InlineData(1.0, 1.0, 1.0, 1.0)] // White
        [InlineData(0.0, 0.0, 0.0, 1.0)] // Black
        public void SetColor_VariousColorValues_CallsSetValueWithCorrectColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var color = new Color(red, green, blue, alpha);

            // Act
            Switch.SetColor(mockElement, color);

            // Assert
            mockElement.Received(1).SetValue(Switch.ColorProperty, color);
        }

        /// <summary>
        /// Tests that GetColor extension method throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void GetColor_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, Switch> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.GetColor());
        }

        /// <summary>
        /// Tests that GetColor extension method returns the color from the element when config is valid.
        /// Input: valid config with element that has a color set
        /// Expected: returns the color from the element via GetColor(BindableObject)
        /// </summary>
        [Fact]
        public void GetColor_ValidConfigWithElement_ReturnsExpectedColor()
        {
            // Arrange
            var element = new Switch();
            var expectedColor = Colors.Red;
            TizenSpecific.Switch.SetColor(element, expectedColor);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Switch>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetColor();

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that GetColor extension method throws NullReferenceException when config.Element is null.
        /// Input: valid config but with null Element property
        /// Expected: NullReferenceException is thrown when trying to access Element.GetValue()
        /// </summary>
        [Fact]
        public void GetColor_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Switch>>();
            config.Element.Returns((Switch)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.GetColor());
        }

        /// <summary>
        /// Tests that SetColor throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void SetColor_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Switch> config = null;
            var color = Colors.Red;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Switch.SetColor(config, color));
        }

        /// <summary>
        /// Tests that SetColor calls the static SetColor method with correct parameters and returns the config.
        /// Verifies proper delegation to the underlying SetColor method and fluent interface behavior.
        /// </summary>
        [Fact]
        public void SetColor_ValidConfigAndColor_CallsSetColorAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Switch>>();
            var mockSwitch = Substitute.For<Microsoft.Maui.Controls.Switch>();
            mockConfig.Element.Returns(mockSwitch);
            var color = Colors.Blue;

            // Act
            var result = Switch.SetColor(mockConfig, color);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests SetColor with a custom color to ensure Color struct parameter handling.
        /// Verifies proper handling of custom color values.
        /// </summary>
        [Fact]
        public void SetColor_CustomColor_ReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Switch>>();
            var mockSwitch = Substitute.For<Microsoft.Maui.Controls.Switch>();
            mockConfig.Element.Returns(mockSwitch);
            var customColor = Color.FromRgba(128, 64, 192, 128);

            // Act
            var result = Switch.SetColor(mockConfig, customColor);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetColor throws NullReferenceException when element parameter is null.
        /// Verifies proper null parameter handling.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetColor_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Switch.GetColor(element));
        }

        /// <summary>
        /// Tests that GetColor returns the correct Color value when element is valid.
        /// Verifies that the method properly retrieves and casts the color value from the bindable property.
        /// Expected result: Returns the Color value stored in the ColorProperty.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        public void GetColor_ValidElement_ReturnsExpectedColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var expectedColor = new Color(red, green, blue, alpha);
            var element = Substitute.For<BindableObject>();
            element.GetValue(Switch.ColorProperty).Returns(expectedColor);

            // Act
            var result = Switch.GetColor(element);

            // Assert
            Assert.Equal(expectedColor, result);
            element.Received(1).GetValue(Switch.ColorProperty);
        }

        /// <summary>
        /// Tests that GetColor properly handles default Color value.
        /// Verifies that the method works when the property contains a default Color instance.
        /// Expected result: Returns the default Color value.
        /// </summary>
        [Fact]
        public void GetColor_DefaultColor_ReturnsDefaultColor()
        {
            // Arrange
            var defaultColor = new Color(); // Default constructor creates black color
            var element = Substitute.For<BindableObject>();
            element.GetValue(Switch.ColorProperty).Returns(defaultColor);

            // Act
            var result = Switch.GetColor(element);

            // Assert
            Assert.Equal(defaultColor, result);
            Assert.Equal(0.0f, result.Red);
            Assert.Equal(0.0f, result.Green);
            Assert.Equal(0.0f, result.Blue);
            Assert.Equal(1.0f, result.Alpha);
            element.Received(1).GetValue(Switch.ColorProperty);
        }
    }
}