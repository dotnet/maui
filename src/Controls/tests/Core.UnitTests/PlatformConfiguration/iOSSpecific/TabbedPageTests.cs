#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.iOSSpecific
{
    public partial class TabbedPageTests
    {
        /// <summary>
        /// Tests that SetTranslucencyMode throws NullReferenceException when the element parameter is null.
        /// Verifies proper null parameter validation by expecting an exception when trying to call SetValue on null.
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;
            var validTranslucencyMode = TranslucencyMode.Default;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.SetTranslucencyMode(nullElement, validTranslucencyMode));
        }

        /// <summary>
        /// Tests that SetTranslucencyMode handles invalid enum values by still calling SetValue with the provided value.
        /// Verifies that the method acts as a pass-through and doesn't validate enum ranges.
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_InvalidEnumValue_CallsSetValueWithInvalidValue()
        {
            // Arrange
            var mockBindableObject = Substitute.For<BindableObject>();
            var invalidTranslucencyMode = (TranslucencyMode)999;

            // Act
            TabbedPage.SetTranslucencyMode(mockBindableObject, invalidTranslucencyMode);

            // Assert
            mockBindableObject.Received(1).SetValue(TabbedPage.TranslucencyModeProperty, invalidTranslucencyMode);
        }

        /// <summary>
        /// Tests that SetTranslucencyMode uses the correct BindableProperty when calling SetValue.
        /// Verifies that the method uses TabbedPage.TranslucencyModeProperty as the property parameter.
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_ValidInputs_UsesCorrectBindableProperty()
        {
            // Arrange
            var mockBindableObject = Substitute.For<BindableObject>();
            var translucencyMode = TranslucencyMode.Translucent;

            // Act
            TabbedPage.SetTranslucencyMode(mockBindableObject, translucencyMode);

            // Assert
            mockBindableObject.Received(1).SetValue(
                Arg.Is<BindableProperty>(prop => prop == TabbedPage.TranslucencyModeProperty),
                Arg.Is<TranslucencyMode>(mode => mode == translucencyMode));
        }

        /// <summary>
        /// Tests that GetTranslucencyMode throws NullReferenceException when config parameter is null.
        /// Validates that the method properly handles null input by attempting to access the Element property.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, TabbedPage> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.GetTranslucencyMode(config));
        }

        /// <summary>
        /// Tests that GetTranslucencyMode handles config with null Element property.
        /// Validates that the method properly handles the case where config.Element returns null.
        /// Expected result: NullReferenceException should be thrown when trying to call GetValue on null element.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, TabbedPage>>();
            config.Element.Returns((TabbedPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.GetTranslucencyMode(config));
        }

        /// <summary>
        /// Tests that SetTranslucencyMode calls the static method with correct parameters and returns the config object when given Default value.
        /// Input: Valid config with mocked Element and TranslucencyMode.Default
        /// Expected: Static SetTranslucencyMode is called with config.Element and value, and the same config is returned
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_WithDefaultValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var value = TranslucencyMode.Default;

            // Act
            var result = TabbedPage.SetTranslucencyMode(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            mockElement.Received(1).SetValue(TabbedPage.TranslucencyModeProperty, value);
        }

        /// <summary>
        /// Tests that SetTranslucencyMode calls the static method with correct parameters and returns the config object when given Translucent value.
        /// Input: Valid config with mocked Element and TranslucencyMode.Translucent
        /// Expected: Static SetTranslucencyMode is called with config.Element and value, and the same config is returned
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_WithTranslucentValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var value = TranslucencyMode.Translucent;

            // Act
            var result = TabbedPage.SetTranslucencyMode(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            mockElement.Received(1).SetValue(TabbedPage.TranslucencyModeProperty, value);
        }

        /// <summary>
        /// Tests that SetTranslucencyMode calls the static method with correct parameters and returns the config object when given Opaque value.
        /// Input: Valid config with mocked Element and TranslucencyMode.Opaque
        /// Expected: Static SetTranslucencyMode is called with config.Element and value, and the same config is returned
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_WithOpaqueValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var value = TranslucencyMode.Opaque;

            // Act
            var result = TabbedPage.SetTranslucencyMode(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            mockElement.Received(1).SetValue(TabbedPage.TranslucencyModeProperty, value);
        }

        /// <summary>
        /// Tests that SetTranslucencyMode throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter and valid TranslucencyMode value
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, TabbedPage> config = null;
            var value = TranslucencyMode.Default;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TabbedPage.SetTranslucencyMode(config, value));
        }

        /// <summary>
        /// Tests that SetTranslucencyMode handles out-of-range enum values correctly.
        /// Input: Valid config and an invalid enum value (cast from integer)
        /// Expected: The invalid enum value is passed through to the static method and config is returned
        /// </summary>
        [Fact]
        public void SetTranslucencyMode_WithInvalidEnumValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var invalidValue = (TranslucencyMode)999;

            // Act
            var result = TabbedPage.SetTranslucencyMode(mockConfig, invalidValue);

            // Assert
            Assert.Same(mockConfig, result);
            mockElement.Received(1).SetValue(TabbedPage.TranslucencyModeProperty, invalidValue);
        }

        /// <summary>
        /// Tests that GetTranslucencyMode throws ArgumentNullException when element parameter is null.
        /// This verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TabbedPage.GetTranslucencyMode(element));
        }

        /// <summary>
        /// Tests that GetTranslucencyMode returns the correct TranslucencyMode value from the BindableObject.
        /// This verifies the method correctly retrieves and casts the TranslucencyMode value for Default.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_ValidElement_ReturnsDefaultTranslucencyMode()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TranslucencyModeProperty).Returns(TranslucencyMode.Default);

            // Act
            var result = TabbedPage.GetTranslucencyMode(element);

            // Assert
            Assert.Equal(TranslucencyMode.Default, result);
        }

        /// <summary>
        /// Tests that GetTranslucencyMode returns the correct TranslucencyMode value from the BindableObject.
        /// This verifies the method correctly retrieves and casts the TranslucencyMode value for Translucent.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_ValidElement_ReturnsTranslucentTranslucencyMode()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TranslucencyModeProperty).Returns(TranslucencyMode.Translucent);

            // Act
            var result = TabbedPage.GetTranslucencyMode(element);

            // Assert
            Assert.Equal(TranslucencyMode.Translucent, result);
        }

        /// <summary>
        /// Tests that GetTranslucencyMode returns the correct TranslucencyMode value from the BindableObject.
        /// This verifies the method correctly retrieves and casts the TranslucencyMode value for Opaque.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_ValidElement_ReturnsOpaqueTranslucencyMode()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TranslucencyModeProperty).Returns(TranslucencyMode.Opaque);

            // Act
            var result = TabbedPage.GetTranslucencyMode(element);

            // Assert
            Assert.Equal(TranslucencyMode.Opaque, result);
        }

        /// <summary>
        /// Tests that GetTranslucencyMode calls GetValue with the correct TranslucencyModeProperty.
        /// This verifies the method uses the correct bindable property for value retrieval.
        /// </summary>
        [Fact]
        public void GetTranslucencyMode_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TranslucencyModeProperty).Returns(TranslucencyMode.Default);

            // Act
            TabbedPage.GetTranslucencyMode(element);

            // Assert
            element.Received(1).GetValue(TabbedPage.TranslucencyModeProperty);
        }
    }
}