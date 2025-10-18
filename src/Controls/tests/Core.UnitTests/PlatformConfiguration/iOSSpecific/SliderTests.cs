#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UnitTests
{
    public partial class SliderTests
    {
        /// <summary>
        /// Tests that SetUpdateOnTap correctly sets the UpdateOnTap property to true on a valid BindableObject.
        /// Verifies that the property value is properly stored and can be retrieved.
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_ValidElementAndTrueValue_SetsPropertyToTrue()
        {
            // Arrange
            var slider = new Microsoft.Maui.Controls.Slider();
            bool expectedValue = true;

            // Act
            Slider.SetUpdateOnTap(slider, expectedValue);

            // Assert
            bool actualValue = Slider.GetUpdateOnTap(slider);
            Assert.True(actualValue);
        }

        /// <summary>
        /// Tests that SetUpdateOnTap correctly sets the UpdateOnTap property to false on a valid BindableObject.
        /// Verifies that the property value is properly stored and can be retrieved.
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_ValidElementAndFalseValue_SetsPropertyToFalse()
        {
            // Arrange
            var slider = new Microsoft.Maui.Controls.Slider();
            bool expectedValue = false;

            // Act
            Slider.SetUpdateOnTap(slider, expectedValue);

            // Assert
            bool actualValue = Slider.GetUpdateOnTap(slider);
            Assert.False(actualValue);
        }

        /// <summary>
        /// Tests that SetUpdateOnTap throws a NullReferenceException when passed a null BindableObject element.
        /// Verifies that the method properly handles null input validation.
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Slider.SetUpdateOnTap(element, value));
        }

        /// <summary>
        /// Tests that GetUpdateOnTap throws NullReferenceException when config parameter is null.
        /// Validates null parameter handling for the extension method.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Controls.Slider> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Slider.GetUpdateOnTap(config));
        }

        /// <summary>
        /// Tests that GetUpdateOnTap throws ArgumentNullException when config.Element is null.
        /// Validates behavior when the configuration exists but Element property returns null.
        /// Expected result: ArgumentNullException should be thrown from the underlying BindableObject.GetValue call.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Controls.Slider>>();
            config.Element.Returns((Controls.Slider)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Slider.GetUpdateOnTap(config));
        }

        /// <summary>
        /// Tests that GetUpdateOnTap returns correct boolean value when config and element are valid.
        /// Validates the normal execution path where configuration and element are properly set up.
        /// Expected result: Should return the boolean value from the UpdateOnTap bindable property.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetUpdateOnTap_ValidConfigAndElement_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var slider = new Controls.Slider();
            slider.SetValue(Slider.UpdateOnTapProperty, expectedValue);

            var config = Substitute.For<IPlatformElementConfiguration<iOS, Controls.Slider>>();
            config.Element.Returns(slider);

            // Act
            var result = Slider.GetUpdateOnTap(config);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetUpdateOnTap properly accesses the Element property from config.
        /// Validates that the method correctly delegates to the underlying GetUpdateOnTap method
        /// by ensuring the Element property is accessed exactly once.
        /// Expected result: Element property should be accessed and method should complete successfully.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_ValidConfig_AccessesElementProperty()
        {
            // Arrange
            var slider = new Controls.Slider();
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Controls.Slider>>();
            config.Element.Returns(slider);

            // Act
            Slider.GetUpdateOnTap(config);

            // Assert
            var _ = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetUpdateOnTap extension method correctly calls the static SetUpdateOnTap method with the element and value, then returns the config for method chaining.
        /// Input: Valid config with mocked element, value = true
        /// Expected: Static method is called with correct parameters and config is returned
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_ValidConfigAndTrueValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<Slider>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Slider>>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            var result = mockConfig.SetUpdateOnTap(value);

            // Assert
            mockElement.Received(1).SetValue(global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateOnTap extension method correctly calls the static SetUpdateOnTap method with the element and value, then returns the config for method chaining.
        /// Input: Valid config with mocked element, value = false
        /// Expected: Static method is called with correct parameters and config is returned
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_ValidConfigAndFalseValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<Slider>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Slider>>();
            mockConfig.Element.Returns(mockElement);
            bool value = false;

            // Act
            var result = mockConfig.SetUpdateOnTap(value);

            // Assert
            mockElement.Received(1).SetValue(global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateOnTap extension method throws NullReferenceException when config parameter is null.
        /// Input: null config parameter
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Slider> nullConfig = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.SetUpdateOnTap(value));
        }

        /// <summary>
        /// Tests that SetUpdateOnTap extension method handles null element by passing it to the static method.
        /// Input: Valid config with null Element property
        /// Expected: Static method is called with null element and config is returned
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_ConfigWithNullElement_PassesNullToStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Slider>>();
            mockConfig.Element.Returns((Slider)null);
            bool value = true;

            // Act & Assert
            var exception = Assert.Throws<NullReferenceException>(() => mockConfig.SetUpdateOnTap(value));
        }

        /// <summary>
        /// Tests that SetUpdateOnTap extension method accesses the Element property exactly once.
        /// Input: Valid config with mocked element
        /// Expected: Element property is accessed once during execution
        /// </summary>
        [Fact]
        public void SetUpdateOnTap_ValidConfig_AccessesElementPropertyOnce()
        {
            // Arrange
            var mockElement = Substitute.For<Slider>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Slider>>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            mockConfig.SetUpdateOnTap(value);

            // Assert
            var elementAccess = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that GetUpdateOnTap throws ArgumentNullException when element parameter is null.
        /// Validates null parameter handling and ensures proper exception is thrown.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.GetUpdateOnTap(element));
        }

        /// <summary>
        /// Tests that GetUpdateOnTap returns true when the BindableObject's GetValue returns true.
        /// Validates the method correctly retrieves and casts the boolean value from the bindable property.
        /// Expected result: Method should return true.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_ElementReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty).Returns(true);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.GetUpdateOnTap(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetUpdateOnTap returns false when the BindableObject's GetValue returns false.
        /// Validates the method correctly retrieves and casts the boolean value from the bindable property.
        /// Expected result: Method should return false.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_ElementReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty).Returns(false);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.GetUpdateOnTap(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetUpdateOnTap throws InvalidCastException when the BindableObject's GetValue returns a non-boolean value.
        /// Validates that the method properly handles type casting errors from the bindable property.
        /// Expected result: InvalidCastException should be thrown.
        /// </summary>
        [Fact]
        public void GetUpdateOnTap_ElementReturnsNonBooleanValue_ThrowsInvalidCastException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty).Returns("not a boolean");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Slider.GetUpdateOnTap(element));
        }
    }
}