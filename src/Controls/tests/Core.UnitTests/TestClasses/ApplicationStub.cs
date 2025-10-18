#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
}

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.AndroidSpecific
{
    /// <summary>
    /// Unit tests for the Application class in Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific namespace.
    /// </summary>
    public class ApplicationTests
    {
        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust calls SetValue on the element with the correct property and value for Pan mode.
        /// Input: Valid BindableObject and WindowSoftInputModeAdjust.Pan
        /// Expected: SetValue called once with WindowSoftInputModeAdjustProperty and Pan value
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_ValidElementAndPanValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = WindowSoftInputModeAdjust.Pan;

            // Act
            Application.SetWindowSoftInputModeAdjust(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Application.WindowSoftInputModeAdjustProperty, value);
        }

        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust calls SetValue on the element with the correct property and value for Resize mode.
        /// Input: Valid BindableObject and WindowSoftInputModeAdjust.Resize
        /// Expected: SetValue called once with WindowSoftInputModeAdjustProperty and Resize value
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_ValidElementAndResizeValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = WindowSoftInputModeAdjust.Resize;

            // Act
            Application.SetWindowSoftInputModeAdjust(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Application.WindowSoftInputModeAdjustProperty, value);
        }

        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust calls SetValue on the element with the correct property and value for Unspecified mode.
        /// Input: Valid BindableObject and WindowSoftInputModeAdjust.Unspecified
        /// Expected: SetValue called once with WindowSoftInputModeAdjustProperty and Unspecified value
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_ValidElementAndUnspecifiedValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = WindowSoftInputModeAdjust.Unspecified;

            // Act
            Application.SetWindowSoftInputModeAdjust(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Application.WindowSoftInputModeAdjustProperty, value);
        }

        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust throws ArgumentNullException when element parameter is null.
        /// Input: Null BindableObject and valid WindowSoftInputModeAdjust value
        /// Expected: ArgumentNullException thrown
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullElement = null;
            var value = WindowSoftInputModeAdjust.Pan;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Application.SetWindowSoftInputModeAdjust(nullElement, value));
        }

        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust works with invalid enum values by casting them and calling SetValue.
        /// Input: Valid BindableObject and invalid enum value (cast from integer)
        /// Expected: SetValue called once with WindowSoftInputModeAdjustProperty and invalid enum value
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_ValidElementAndInvalidEnumValue_CallsSetValueWithInvalidValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var invalidValue = (WindowSoftInputModeAdjust)999;

            // Act
            Application.SetWindowSoftInputModeAdjust(mockElement, invalidValue);

            // Assert
            mockElement.Received(1).SetValue(Application.WindowSoftInputModeAdjustProperty, invalidValue);
        }

        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust works with enum minimum boundary value.
        /// Input: Valid BindableObject and minimum enum value (Pan = 0)
        /// Expected: SetValue called once with WindowSoftInputModeAdjustProperty and Pan value
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_ValidElementAndMinimumEnumValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = (WindowSoftInputModeAdjust)0; // Pan

            // Act
            Application.SetWindowSoftInputModeAdjust(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Application.WindowSoftInputModeAdjustProperty, value);
        }

        /// <summary>
        /// Tests that SetWindowSoftInputModeAdjust works with enum maximum boundary value.
        /// Input: Valid BindableObject and maximum enum value (Unspecified = 2)
        /// Expected: SetValue called once with WindowSoftInputModeAdjustProperty and Unspecified value
        /// </summary>
        [Fact]
        public void SetWindowSoftInputModeAdjust_ValidElementAndMaximumEnumValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = (WindowSoftInputModeAdjust)2; // Unspecified

            // Act
            Application.SetWindowSoftInputModeAdjust(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(Application.WindowSoftInputModeAdjustProperty, value);
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust extension method returns Pan when the underlying element has Pan value.
        /// Verifies that the method correctly delegates to the static overload and returns the expected result.
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_ConfigWithElementHavingPanValue_ReturnsPan()
        {
            // Arrange
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns(mockApplication);

            // Set up the mock application to return Pan when GetValue is called
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty)
                .Returns(WindowSoftInputModeAdjust.Pan);

            // Act
            var result = mockConfig.GetWindowSoftInputModeAdjust();

            // Assert
            Assert.Equal(WindowSoftInputModeAdjust.Pan, result);
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust extension method returns Resize when the underlying element has Resize value.
        /// Verifies that the method correctly delegates to the static overload and returns the expected result.
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_ConfigWithElementHavingResizeValue_ReturnsResize()
        {
            // Arrange
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns(mockApplication);

            // Set up the mock application to return Resize when GetValue is called
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty)
                .Returns(WindowSoftInputModeAdjust.Resize);

            // Act
            var result = mockConfig.GetWindowSoftInputModeAdjust();

            // Assert
            Assert.Equal(WindowSoftInputModeAdjust.Resize, result);
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust extension method returns Unspecified when the underlying element has Unspecified value.
        /// Verifies that the method correctly delegates to the static overload and returns the expected result.
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_ConfigWithElementHavingUnspecifiedValue_ReturnsUnspecified()
        {
            // Arrange
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns(mockApplication);

            // Set up the mock application to return Unspecified when GetValue is called
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty)
                .Returns(WindowSoftInputModeAdjust.Unspecified);

            // Act
            var result = mockConfig.GetWindowSoftInputModeAdjust();

            // Assert
            Assert.Equal(WindowSoftInputModeAdjust.Unspecified, result);
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust extension method throws ArgumentNullException when config parameter is null.
        /// Verifies that the method properly handles null input and throws the expected exception.
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Microsoft.Maui.Controls.Application> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.GetWindowSoftInputModeAdjust());
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust extension method throws ArgumentNullException when config.Element is null.
        /// Verifies that the method properly handles null element and throws the expected exception.
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.Application)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.GetWindowSoftInputModeAdjust());
        }

        /// <summary>
        /// Tests that UseWindowSoftInputModeAdjust calls SetWindowSoftInputModeAdjust with the correct parameters
        /// and returns the same configuration instance, enabling fluent interface usage.
        /// Input: Valid configuration and Pan enum value.
        /// Expected: SetValue called on element with correct property and value, same config returned.
        /// </summary>
        [Fact]
        public void UseWindowSoftInputModeAdjust_ValidConfigAndPanValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            var value = WindowSoftInputModeAdjust.Pan;

            // Act
            var result = mockConfig.UseWindowSoftInputModeAdjust(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that UseWindowSoftInputModeAdjust works correctly with the Resize enum value.
        /// Input: Valid configuration and Resize enum value.
        /// Expected: SetValue called on element with Resize value, same config returned.
        /// </summary>
        [Fact]
        public void UseWindowSoftInputModeAdjust_ValidConfigAndResizeValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            var value = WindowSoftInputModeAdjust.Resize;

            // Act
            var result = mockConfig.UseWindowSoftInputModeAdjust(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that UseWindowSoftInputModeAdjust works correctly with the Unspecified enum value.
        /// Input: Valid configuration and Unspecified enum value.
        /// Expected: SetValue called on element with Unspecified value, same config returned.
        /// </summary>
        [Fact]
        public void UseWindowSoftInputModeAdjust_ValidConfigAndUnspecifiedValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            var value = WindowSoftInputModeAdjust.Unspecified;

            // Act
            var result = mockConfig.UseWindowSoftInputModeAdjust(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that UseWindowSoftInputModeAdjust handles invalid enum values by passing them through to SetValue.
        /// Input: Valid configuration and invalid enum value (cast from integer).
        /// Expected: SetValue called on element with invalid enum value, same config returned.
        /// </summary>
        [Fact]
        public void UseWindowSoftInputModeAdjust_ValidConfigAndInvalidEnumValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            var invalidValue = (WindowSoftInputModeAdjust)999;

            // Act
            var result = mockConfig.UseWindowSoftInputModeAdjust(invalidValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty, invalidValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that UseWindowSoftInputModeAdjust throws ArgumentNullException when config is null.
        /// Input: Null configuration parameter.
        /// Expected: ArgumentNullException thrown.
        /// </summary>
        [Fact]
        public void UseWindowSoftInputModeAdjust_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.Application> nullConfig = null;
            var value = WindowSoftInputModeAdjust.Pan;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.UseWindowSoftInputModeAdjust(value));
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust throws NullReferenceException when element parameter is null.
        /// Verifies that the method properly handles null input by attempting to call GetValue on a null object.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.GetWindowSoftInputModeAdjust(element));
        }

        /// <summary>
        /// Tests that GetWindowSoftInputModeAdjust handles the default property value correctly.
        /// Verifies that when no explicit value is set, the method returns the default enum value.
        /// Expected result: Returns WindowSoftInputModeAdjust.Pan (the default value).
        /// </summary>
        [Fact]
        public void GetWindowSoftInputModeAdjust_ValidElementWithDefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty)
                   .Returns(WindowSoftInputModeAdjust.Pan); // Default value based on property definition

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.GetWindowSoftInputModeAdjust(element);

            // Assert
            Assert.Equal(WindowSoftInputModeAdjust.Pan, result);
        }
    }
}