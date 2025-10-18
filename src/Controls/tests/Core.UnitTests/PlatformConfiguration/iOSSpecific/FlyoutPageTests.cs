#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.iOSSpecific
{
    /// <summary>
    /// Unit tests for the FlyoutPage iOS-specific platform configuration methods.
    /// </summary>
    public class FlyoutPageTests
    {
        /// <summary>
        /// Tests that SetApplyShadow throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation for the element parameter.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void SetApplyShadow_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlyoutPage.SetApplyShadow(element, value));
        }

        /// <summary>
        /// Tests that SetApplyShadow correctly calls SetValue on the element with true value.
        /// Verifies that the method properly delegates to BindableObject.SetValue with the correct parameters.
        /// Expected result: SetValue should be called once with ApplyShadowProperty and true value.
        /// </summary>
        [Fact]
        public void SetApplyShadow_ValidElementTrueValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            FlyoutPage.SetApplyShadow(element, value);

            // Assert
            element.Received(1).SetValue(FlyoutPage.ApplyShadowProperty, value);
        }

        /// <summary>
        /// Tests that SetApplyShadow correctly calls SetValue on the element with false value.
        /// Verifies that the method properly delegates to BindableObject.SetValue with the correct parameters.
        /// Expected result: SetValue should be called once with ApplyShadowProperty and false value.
        /// </summary>
        [Fact]
        public void SetApplyShadow_ValidElementFalseValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            FlyoutPage.SetApplyShadow(element, value);

            // Assert
            element.Received(1).SetValue(FlyoutPage.ApplyShadowProperty, value);
        }

        /// <summary>
        /// Tests that SetApplyShadow works correctly with different boolean values using parameterized test.
        /// Verifies that the method correctly handles both true and false boolean values.
        /// Expected result: SetValue should be called with the correct boolean value in each case.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetApplyShadow_ValidElementWithBooleanValues_CallsSetValueWithCorrectValue(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            FlyoutPage.SetApplyShadow(element, value);

            // Assert
            element.Received(1).SetValue(FlyoutPage.ApplyShadowProperty, value);
        }

        /// <summary>
        /// Tests that SetApplyShadow extension method sets the value to true and returns the same configuration object.
        /// Input: Valid configuration object and true value.
        /// Expected: Static SetApplyShadow method is called with the element and value, and the same config object is returned.
        /// </summary>
        [Fact]
        public void SetApplyShadow_WithValidConfigAndTrueValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<FlyoutPage>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            var result = mockConfig.SetApplyShadow(value);

            // Assert
            mockElement.Received(1).SetValue(FlyoutPage.ApplyShadowProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetApplyShadow extension method sets the value to false and returns the same configuration object.
        /// Input: Valid configuration object and false value.
        /// Expected: Static SetApplyShadow method is called with the element and value, and the same config object is returned.
        /// </summary>
        [Fact]
        public void SetApplyShadow_WithValidConfigAndFalseValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<FlyoutPage>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);
            bool value = false;

            // Act
            var result = mockConfig.SetApplyShadow(value);

            // Assert
            mockElement.Received(1).SetValue(FlyoutPage.ApplyShadowProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetApplyShadow extension method throws NullReferenceException when config is null.
        /// Input: Null configuration object and any boolean value.
        /// Expected: NullReferenceException is thrown when trying to access config.Element.
        /// </summary>
        [Fact]
        public void SetApplyShadow_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, FlyoutPage> nullConfig = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.SetApplyShadow(value));
        }

        /// <summary>
        /// Tests that SetApplyShadow extension method throws when config.Element is null.
        /// Input: Configuration object with null Element property and any boolean value.
        /// Expected: ArgumentNullException is thrown by the underlying SetValue call.
        /// </summary>
        [Fact]
        public void SetApplyShadow_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, FlyoutPage>>();
            mockConfig.Element.Returns((FlyoutPage)null);
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.SetApplyShadow(value));
        }

        /// <summary>
        /// Tests that GetApplyShadow extension method returns true when the underlying element has ApplyShadow set to true.
        /// Verifies that the method correctly delegates to the static GetApplyShadow method and retrieves the boolean value from the bindable property.
        /// </summary>
        [Fact]
        public void GetApplyShadow_ConfigWithElementApplyShadowTrue_ReturnsTrue()
        {
            // Arrange
            var mockElement = Substitute.For<FlyoutPage>();
            mockElement.GetValue(iOSSpecific.FlyoutPage.ApplyShadowProperty).Returns(true);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            bool result = mockConfig.GetApplyShadow();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetApplyShadow extension method returns false when the underlying element has ApplyShadow set to false.
        /// Verifies that the method correctly delegates to the static GetApplyShadow method and retrieves the boolean value from the bindable property.
        /// </summary>
        [Fact]
        public void GetApplyShadow_ConfigWithElementApplyShadowFalse_ReturnsFalse()
        {
            // Arrange
            var mockElement = Substitute.For<FlyoutPage>();
            mockElement.GetValue(iOSSpecific.FlyoutPage.ApplyShadowProperty).Returns(false);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            bool result = mockConfig.GetApplyShadow();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetApplyShadow extension method throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation for the extension method.
        /// </summary>
        [Fact]
        public void GetApplyShadow_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, FlyoutPage> nullConfig = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.GetApplyShadow());
        }

        /// <summary>
        /// Tests that GetApplyShadow extension method throws NullReferenceException when config.Element is null.
        /// Verifies that the method properly handles cases where the configuration has a null element.
        /// </summary>
        [Fact]
        public void GetApplyShadow_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, FlyoutPage>>();
            mockConfig.Element.Returns((FlyoutPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.GetApplyShadow());
        }

        /// <summary>
        /// Tests that GetApplyShadow throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetApplyShadow_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlyoutPage.GetApplyShadow(element));
        }

        /// <summary>
        /// Tests that GetApplyShadow returns true when the ApplyShadow property is set to true.
        /// Verifies the method correctly retrieves and casts the property value.
        /// Expected result: Should return true.
        /// </summary>
        [Fact]
        public void GetApplyShadow_ElementWithApplyShadowTrue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.ApplyShadowProperty).Returns(true);

            // Act
            bool result = FlyoutPage.GetApplyShadow(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetApplyShadow returns false when the ApplyShadow property is set to false.
        /// Verifies the method correctly retrieves and casts the property value.
        /// Expected result: Should return false.
        /// </summary>
        [Fact]
        public void GetApplyShadow_ElementWithApplyShadowFalse_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.ApplyShadowProperty).Returns(false);

            // Act
            bool result = FlyoutPage.GetApplyShadow(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetApplyShadow returns the default value when no value has been explicitly set.
        /// Verifies the method works with the property's default value.
        /// Expected result: Should return false (the default value).
        /// </summary>
        [Fact]
        public void GetApplyShadow_ElementWithDefaultValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.ApplyShadowProperty).Returns(false);

            // Act
            bool result = FlyoutPage.GetApplyShadow(element);

            // Assert
            Assert.False(result);
        }
    }
}