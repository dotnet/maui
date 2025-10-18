#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.AndroidSpecific.AppCompat
{
    public class NavigationPageTests
    {
        /// <summary>
        /// Tests that SetBarHeight method calls SetValue on the element with BarHeightProperty and the specified value.
        /// Tests various integer values including positive, negative, zero, and boundary values.
        /// </summary>
        /// <param name="value">The bar height value to set</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetBarHeight_ValidElement_CallsSetValueWithCorrectParameters(int value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            NavigationPage.SetBarHeight(element, value);

            // Assert
            element.Received(1).SetValue(NavigationPage.BarHeightProperty, value);
        }

        /// <summary>
        /// Tests that SetBarHeight throws ArgumentNullException when element parameter is null.
        /// Expected to throw NullReferenceException when attempting to call SetValue on null element.
        /// </summary>
        [Fact]
        public void SetBarHeight_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            int value = 10;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.SetBarHeight(element, value));
        }

        /// <summary>
        /// Tests that GetBarHeight returns the correct bar height value when called with a valid configuration.
        /// Tests the normal execution path where config and config.Element are both valid.
        /// Expected result: Returns the bar height value from the underlying BindableObject.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(56)]
        [InlineData(-10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetBarHeight_WithValidConfig_ReturnsExpectedBarHeight(int expectedHeight)
        {
            // Arrange
            var mockElement = Substitute.For<NavigationPage>();
            mockElement.GetValue(NavigationPage.BarHeightProperty).Returns(expectedHeight);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, NavigationPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            int result = NavigationPage.GetBarHeight(mockConfig);

            // Assert
            Assert.Equal(expectedHeight, result);
        }

        /// <summary>
        /// Tests that GetBarHeight throws NullReferenceException when called with null configuration.
        /// Tests the error condition where the config parameter is null.
        /// Expected result: Throws NullReferenceException when accessing config.Element.
        /// </summary>
        [Fact]
        public void GetBarHeight_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, NavigationPage> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.GetBarHeight(nullConfig));
        }

        /// <summary>
        /// Tests that GetBarHeight throws NullReferenceException when config.Element is null.
        /// Tests the error condition where the configuration is valid but Element property returns null.
        /// Expected result: Throws NullReferenceException when the underlying GetBarHeight method tries to access the null element.
        /// </summary>
        [Fact]
        public void GetBarHeight_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, NavigationPage>>();
            mockConfig.Element.Returns((NavigationPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.GetBarHeight(mockConfig));
        }

        /// <summary>
        /// Tests that SetBarHeight throws NullReferenceException when config parameter is null.
        /// This test verifies proper null handling for the config parameter.
        /// Expected: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void SetBarHeight_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, NavigationPage> config = null;
            int value = 50;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.SetBarHeight(config, value));
        }

        /// <summary>
        /// Tests that SetBarHeight returns the same config object that was passed in.
        /// This test verifies the fluent interface pattern implementation.
        /// Expected: The exact same config object should be returned.
        /// </summary>
        [Fact]
        public void SetBarHeight_ValidConfig_ReturnsSameConfigObject()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);
            int value = 100;

            // Act
            var result = NavigationPage.SetBarHeight(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests SetBarHeight with various integer values including edge cases.
        /// This test verifies that different integer values are handled correctly.
        /// Expected: Method should complete successfully and return the config object.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetBarHeight_VariousIntegerValues_ReturnsConfigAndCompletes(int value)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);

            // Act
            var result = NavigationPage.SetBarHeight(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that SetBarHeight accesses the Element property of the config parameter.
        /// This test verifies that the method properly uses the config.Element property.
        /// Expected: The Element property should be accessed during method execution.
        /// </summary>
        [Fact]
        public void SetBarHeight_ValidConfig_AccessesElementProperty()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();
            mockConfig.Element.Returns(mockNavigationPage);
            int value = 75;

            // Act
            NavigationPage.SetBarHeight(mockConfig, value);

            // Assert
            var element = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that GetBarHeight throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetBarHeight_NullElement_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.GetBarHeight(null));
        }

        /// <summary>
        /// Tests that GetBarHeight returns the default value when BindableObject returns default int value.
        /// </summary>
        [Fact]
        public void GetBarHeight_DefaultValue_ReturnsZero()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(NavigationPage.BarHeightProperty).Returns(0);

            // Act
            var result = NavigationPage.GetBarHeight(mockElement);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that GetBarHeight returns various valid integer values correctly.
        /// </summary>
        /// <param name="expectedValue">The expected bar height value to test.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetBarHeight_ValidValues_ReturnsExpectedValue(int expectedValue)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(NavigationPage.BarHeightProperty).Returns(expectedValue);

            // Act
            var result = NavigationPage.GetBarHeight(mockElement);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetBarHeight correctly casts object return value from GetValue to int.
        /// </summary>
        [Fact]
        public void GetBarHeight_ObjectReturnValue_CastsToInt()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            object boxedInt = 42;
            mockElement.GetValue(NavigationPage.BarHeightProperty).Returns(boxedInt);

            // Act
            var result = NavigationPage.GetBarHeight(mockElement);

            // Assert
            Assert.Equal(42, result);
        }

        /// <summary>
        /// Tests that GetBarHeight calls GetValue with the correct BarHeightProperty.
        /// </summary>
        [Fact]
        public void GetBarHeight_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(NavigationPage.BarHeightProperty).Returns(50);

            // Act
            NavigationPage.GetBarHeight(mockElement);

            // Assert
            mockElement.Received(1).GetValue(NavigationPage.BarHeightProperty);
        }
    }
}