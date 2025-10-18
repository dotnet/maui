#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the TizenSpecific NavigationPage static class.
    /// </summary>
    public partial class NavigationPageTests
    {
        /// <summary>
        /// Tests that GetHasBreadCrumbsBar throws NullReferenceException when element parameter is null.
        /// Input: null element
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void GetHasBreadCrumbsBar_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.GetHasBreadCrumbsBar(element));
        }

        /// <summary>
        /// Tests that GetHasBreadCrumbsBar returns false when the element has the default value.
        /// Input: BindableObject with default HasBreadCrumbsBar value
        /// Expected: false is returned
        /// </summary>
        [Fact]
        public void GetHasBreadCrumbsBar_ElementWithDefaultValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(NavigationPage.HasBreadCrumbsBarProperty).Returns(false);

            // Act
            bool result = NavigationPage.GetHasBreadCrumbsBar(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetHasBreadCrumbsBar returns true when the element's HasBreadCrumbsBar property is set to true.
        /// Input: BindableObject with HasBreadCrumbsBar set to true
        /// Expected: true is returned
        /// </summary>
        [Fact]
        public void GetHasBreadCrumbsBar_ElementWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(NavigationPage.HasBreadCrumbsBarProperty).Returns(true);

            // Act
            bool result = NavigationPage.GetHasBreadCrumbsBar(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetHasBreadCrumbsBar returns false when the element's HasBreadCrumbsBar property is explicitly set to false.
        /// Input: BindableObject with HasBreadCrumbsBar explicitly set to false
        /// Expected: false is returned
        /// </summary>
        [Fact]
        public void GetHasBreadCrumbsBar_ElementWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(NavigationPage.HasBreadCrumbsBarProperty).Returns(false);

            // Act
            bool result = NavigationPage.GetHasBreadCrumbsBar(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetHasBreadCrumbsBar correctly casts non-boolean object values returned from GetValue.
        /// Input: BindableObject that returns object value that can be cast to bool
        /// Expected: correct boolean value is returned after casting
        /// </summary>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void GetHasBreadCrumbsBar_ElementReturnsObjectValue_ReturnsCorrectBooleanAfterCasting(object objectValue, bool expectedResult)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(NavigationPage.HasBreadCrumbsBarProperty).Returns(objectValue);

            // Act
            bool result = NavigationPage.GetHasBreadCrumbsBar(element);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that GetHasBreadCrumbsBar calls GetValue with the correct HasBreadCrumbsBarProperty parameter.
        /// Input: valid BindableObject
        /// Expected: GetValue is called with HasBreadCrumbsBarProperty
        /// </summary>
        [Fact]
        public void GetHasBreadCrumbsBar_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Arg.Any<BindableProperty>()).Returns(false);

            // Act
            NavigationPage.GetHasBreadCrumbsBar(element);

            // Assert
            element.Received(1).GetValue(NavigationPage.HasBreadCrumbsBarProperty);
        }

        /// <summary>
        /// Tests that HasBreadCrumbsBar returns the correct boolean value when called with a valid configuration.
        /// </summary>
        /// <param name="expectedValue">The boolean value that should be returned by the underlying GetValue method.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasBreadCrumbsBar_ValidConfig_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Tizen, NavigationPage>>();
            var mockNavigationPage = Substitute.For<NavigationPage>();

            mockConfig.Element.Returns(mockNavigationPage);
            mockNavigationPage.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.NavigationPage.HasBreadCrumbsBarProperty).Returns(expectedValue);

            // Act
            var result = mockConfig.HasBreadCrumbsBar();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that HasBreadCrumbsBar throws ArgumentNullException when called with null configuration.
        /// </summary>
        [Fact]
        public void HasBreadCrumbsBar_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Tizen, NavigationPage> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.HasBreadCrumbsBar());
        }

        /// <summary>
        /// Tests that HasBreadCrumbsBar throws NullReferenceException when the configuration has a null Element.
        /// </summary>
        [Fact]
        public void HasBreadCrumbsBar_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Tizen, NavigationPage>>();
            mockConfig.Element.Returns((NavigationPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.HasBreadCrumbsBar());
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar extension method sets the value and returns the same config instance when value is true.
        /// </summary>
        [Fact]
        public void SetHasBreadCrumbsBar_WithTrueValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var navigationPage = new NavigationPage();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, NavigationPage>>();
            config.Element.Returns(navigationPage);
            const bool value = true;

            // Act
            var result = config.SetHasBreadCrumbsBar(value);

            // Assert
            Assert.Same(config, result);
            Assert.True(navigationPage.GetValue(TizenSpecific.NavigationPage.HasBreadCrumbsBarProperty));
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar extension method sets the value and returns the same config instance when value is false.
        /// </summary>
        [Fact]
        public void SetHasBreadCrumbsBar_WithFalseValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var navigationPage = new NavigationPage();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, NavigationPage>>();
            config.Element.Returns(navigationPage);
            const bool value = false;

            // Act
            var result = config.SetHasBreadCrumbsBar(value);

            // Assert
            Assert.Same(config, result);
            Assert.False(navigationPage.GetValue(TizenSpecific.NavigationPage.HasBreadCrumbsBarProperty));
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar extension method throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetHasBreadCrumbsBar_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, NavigationPage> config = null;
            const bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetHasBreadCrumbsBar(value));
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar extension method works correctly with parameterized boolean values.
        /// </summary>
        /// <param name="value">The boolean value to set for HasBreadCrumbsBar property.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetHasBreadCrumbsBar_WithVariousValues_SetsCorrectValueAndReturnsConfig(bool value)
        {
            // Arrange
            var navigationPage = new NavigationPage();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, NavigationPage>>();
            config.Element.Returns(navigationPage);

            // Act
            var result = config.SetHasBreadCrumbsBar(value);

            // Assert
            Assert.Same(config, result);
            Assert.Equal(value, navigationPage.GetValue(TizenSpecific.NavigationPage.HasBreadCrumbsBarProperty));
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar throws NullReferenceException when element parameter is null.
        /// Input: null element and any boolean value.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetHasBreadCrumbsBar_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => NavigationPage.SetHasBreadCrumbsBar(element, value));
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar calls SetValue with correct parameters when value is true.
        /// Input: valid BindableObject element and true value.
        /// Expected: SetValue is called with HasBreadCrumbsBarProperty and true value.
        /// </summary>
        [Fact]
        public void SetHasBreadCrumbsBar_ValidElementWithTrueValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            NavigationPage.SetHasBreadCrumbsBar(element, value);

            // Assert
            element.Received(1).SetValue(NavigationPage.HasBreadCrumbsBarProperty, value);
        }

        /// <summary>
        /// Tests that SetHasBreadCrumbsBar calls SetValue with correct parameters when value is false.
        /// Input: valid BindableObject element and false value.
        /// Expected: SetValue is called with HasBreadCrumbsBarProperty and false value.
        /// </summary>
        [Fact]
        public void SetHasBreadCrumbsBar_ValidElementWithFalseValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            NavigationPage.SetHasBreadCrumbsBar(element, value);

            // Assert
            element.Received(1).SetValue(NavigationPage.HasBreadCrumbsBarProperty, value);
        }
    }
}