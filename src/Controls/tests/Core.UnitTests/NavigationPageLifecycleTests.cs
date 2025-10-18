#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class NavigationPageLifecycleTests : BaseTestFixture
    {
    }

    /// <summary>
    /// Unit tests for the NavigationPage iOS-specific platform configuration methods.
    /// </summary>
    public class NavigationPageiOSSpecificTests
    {
        /// <summary>
        /// Tests that GetStatusBarTextColorMode extension method returns the correct value when called with a valid configuration
        /// containing an element with the StatusBarTextColorMode property set to MatchNavigationBarTextLuminosity.
        /// </summary>
        [Fact]
        public void GetStatusBarTextColorMode_ValidConfigWithMatchNavigationBarTextLuminosity_ReturnsMatchNavigationBarTextLuminosity()
        {
            // Arrange
            var navigationPage = new NavigationPage();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetStatusBarTextColorMode(navigationPage, StatusBarTextColorMode.MatchNavigationBarTextLuminosity);

            var config = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, NavigationPage>>();
            config.Element.Returns(navigationPage);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetStatusBarTextColorMode(config);

            // Assert
            Assert.Equal(StatusBarTextColorMode.MatchNavigationBarTextLuminosity, result);
        }

        /// <summary>
        /// Tests that GetStatusBarTextColorMode extension method returns the correct value when called with a valid configuration
        /// containing an element with the StatusBarTextColorMode property set to DoNotAdjust.
        /// </summary>
        [Fact]
        public void GetStatusBarTextColorMode_ValidConfigWithDoNotAdjust_ReturnsDoNotAdjust()
        {
            // Arrange
            var navigationPage = new NavigationPage();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetStatusBarTextColorMode(navigationPage, StatusBarTextColorMode.DoNotAdjust);

            var config = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, NavigationPage>>();
            config.Element.Returns(navigationPage);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetStatusBarTextColorMode(config);

            // Assert
            Assert.Equal(StatusBarTextColorMode.DoNotAdjust, result);
        }

        /// <summary>
        /// Tests that GetStatusBarTextColorMode extension method returns the default value when called with a valid configuration
        /// containing an element that has not had the StatusBarTextColorMode property explicitly set.
        /// </summary>
        [Fact]
        public void GetStatusBarTextColorMode_ValidConfigWithDefaultValue_ReturnsMatchNavigationBarTextLuminosity()
        {
            // Arrange
            var navigationPage = new NavigationPage();

            var config = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, NavigationPage>>();
            config.Element.Returns(navigationPage);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetStatusBarTextColorMode(config);

            // Assert
            Assert.Equal(StatusBarTextColorMode.MatchNavigationBarTextLuminosity, result);
        }

        /// <summary>
        /// Tests that GetStatusBarTextColorMode extension method throws ArgumentNullException when called with a null configuration parameter.
        /// </summary>
        [Fact]
        public void GetStatusBarTextColorMode_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, NavigationPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetStatusBarTextColorMode(config));
        }

        /// <summary>
        /// Tests that GetStatusBarTextColorMode extension method throws ArgumentNullException when called with a configuration
        /// that has a null Element property.
        /// </summary>
        [Fact]
        public void GetStatusBarTextColorMode_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.iOS, NavigationPage>>();
            config.Element.Returns((NavigationPage)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.GetStatusBarTextColorMode(config));
        }

        /// <summary>
        /// Tests SetStatusBarTextColorMode with an invalid enum value cast from integer to ensure it handles out-of-range values.
        /// </summary>
        [Fact]
        public void SetStatusBarTextColorMode_InvalidEnumValue_ReturnsConfigAndCallsStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);
            var invalidEnumValue = (StatusBarTextColorMode)999;

            // Act
            var result = NavigationPage.SetStatusBarTextColorMode(mockConfig, invalidEnumValue);

            // Assert
            Assert.Same(mockConfig, result);
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests SetStatusBarTextColorMode with null config parameter to ensure ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetStatusBarTextColorMode_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, NavigationPage> nullConfig = null;
            var colorMode = StatusBarTextColorMode.MatchNavigationBarTextLuminosity;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NavigationPage.SetStatusBarTextColorMode(nullConfig, colorMode));
        }

        /// <summary>
        /// Tests SetStatusBarTextColorMode to verify that config.Element is accessed exactly once during method execution.
        /// </summary>
        [Fact]
        public void SetStatusBarTextColorMode_ValidInput_AccessesElementPropertyOnce()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);
            var colorMode = StatusBarTextColorMode.DoNotAdjust;

            // Act
            NavigationPage.SetStatusBarTextColorMode(mockConfig, colorMode);

            // Assert
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests SetStatusBarTextColorMode with minimum enum value to ensure boundary value handling.
        /// </summary>
        [Fact]
        public void SetStatusBarTextColorMode_MinimumEnumValue_ReturnsConfigAndCallsStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);
            var minEnumValue = (StatusBarTextColorMode)0;

            // Act
            var result = NavigationPage.SetStatusBarTextColorMode(mockConfig, minEnumValue);

            // Assert
            Assert.Same(mockConfig, result);
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests SetStatusBarTextColorMode with negative enum value to ensure it handles negative cast values.
        /// </summary>
        [Fact]
        public void SetStatusBarTextColorMode_NegativeEnumValue_ReturnsConfigAndCallsStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);
            var negativeEnumValue = (StatusBarTextColorMode)(-1);

            // Act
            var result = NavigationPage.SetStatusBarTextColorMode(mockConfig, negativeEnumValue);

            // Assert
            Assert.Same(mockConfig, result);
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that HideNavigationBarSeparator throws NullReferenceException when config parameter is null.
        /// Verifies that the method properly accesses config.Element and throws when config is null.
        /// </summary>
        [Fact]
        public void HideNavigationBarSeparator_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.NavigationPage> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparator(config));
        }

        /// <summary>
        /// Tests that HideNavigationBarSeparator returns false when the HideNavigationBarSeparator property is set to false.
        /// Verifies that the method correctly retrieves the property value through GetHideNavigationBarSeparator.
        /// </summary>
        [Fact]
        public void HideNavigationBarSeparator_ValidConfigWithFalseProperty_ReturnsFalse()
        {
            // Arrange
            var navigationPage = new Microsoft.Maui.Controls.NavigationPage();
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.NavigationPage>>();
            config.Element.Returns(navigationPage);

            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(navigationPage, false);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparator(config);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HideNavigationBarSeparator returns true when the HideNavigationBarSeparator property is set to true.
        /// Verifies that the method correctly retrieves the property value through GetHideNavigationBarSeparator.
        /// </summary>
        [Fact]
        public void HideNavigationBarSeparator_ValidConfigWithTrueProperty_ReturnsTrue()
        {
            // Arrange
            var navigationPage = new Microsoft.Maui.Controls.NavigationPage();
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.NavigationPage>>();
            config.Element.Returns(navigationPage);

            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(navigationPage, true);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparator(config);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HideNavigationBarSeparator returns the default false value when the property has not been explicitly set.
        /// Verifies that the method returns the default property value as defined in HideNavigationBarSeparatorProperty.
        /// </summary>
        [Fact]
        public void HideNavigationBarSeparator_ValidConfigWithDefaultProperty_ReturnsFalse()
        {
            // Arrange
            var navigationPage = new Microsoft.Maui.Controls.NavigationPage();
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.NavigationPage>>();
            config.Element.Returns(navigationPage);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparator(config);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HideNavigationBarSeparator throws NullReferenceException when config.Element is null.
        /// Verifies that the method properly handles the case where the config object exists but its Element property is null.
        /// </summary>
        [Fact]
        public void HideNavigationBarSeparator_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.NavigationPage>>();
            config.Element.Returns((Microsoft.Maui.Controls.NavigationPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparator(config));
        }

        /// <summary>
        /// Tests that SetHideNavigationBarSeparator extension method sets the value to true and returns the same config object.
        /// </summary>
        [Fact]
        public void SetHideNavigationBarSeparator_WithTrueValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.SetHideNavigationBarSeparator(true);

            // Assert
            Assert.Same(mockConfig, result);
            mockElement.Received(1).SetValue(NavigationPage.HideNavigationBarSeparatorProperty, true);
        }

        /// <summary>
        /// Tests that SetHideNavigationBarSeparator extension method sets the value to false and returns the same config object.
        /// </summary>
        [Fact]
        public void SetHideNavigationBarSeparator_WithFalseValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.SetHideNavigationBarSeparator(false);

            // Assert
            Assert.Same(mockConfig, result);
            mockElement.Received(1).SetValue(NavigationPage.HideNavigationBarSeparatorProperty, false);
        }

        /// <summary>
        /// Tests that SetHideNavigationBarSeparator throws when config parameter is null.
        /// </summary>
        [Fact]
        public void SetHideNavigationBarSeparator_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, NavigationPage> nullConfig = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.SetHideNavigationBarSeparator(true));
        }

        /// <summary>
        /// Tests that SetHideNavigationBarSeparator throws when config.Element is null.
        /// </summary>
        [Fact]
        public void SetHideNavigationBarSeparator_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.SetHideNavigationBarSeparator(true));
        }

        /// <summary>
        /// Tests that SetHideNavigationBarSeparator can be chained with multiple calls and maintains the same config object.
        /// </summary>
        [Fact]
        public void SetHideNavigationBarSeparator_MethodChaining_ReturnsSameConfigObject()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, NavigationPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result1 = mockConfig.SetHideNavigationBarSeparator(true);
            var result2 = result1.SetHideNavigationBarSeparator(false);

            // Assert
            Assert.Same(mockConfig, result1);
            Assert.Same(mockConfig, result2);
            Assert.Same(result1, result2);
            mockElement.Received(1).SetValue(NavigationPage.HideNavigationBarSeparatorProperty, true);
            mockElement.Received(1).SetValue(NavigationPage.HideNavigationBarSeparatorProperty, false);
        }
    }
}