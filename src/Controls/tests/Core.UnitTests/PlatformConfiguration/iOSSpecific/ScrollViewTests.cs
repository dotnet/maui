#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ScrollView iOS-specific platform configuration extension methods.
    /// </summary>
    public partial class ScrollViewTests
    {
        /// <summary>
        /// Verifies that ShouldDelayContentTouches throws NullReferenceException when config parameter is null.
        /// Tests the case where a null configuration is passed to the extension method.
        /// Expected to throw NullReferenceException when trying to access config.Element.
        /// </summary>
        [Fact]
        public void ShouldDelayContentTouches_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ScrollView> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.ShouldDelayContentTouches());
        }

        /// <summary>
        /// Verifies that ShouldDelayContentTouches throws NullReferenceException when config.Element is null.
        /// Tests the case where the configuration has a null Element property.
        /// Expected to throw NullReferenceException when GetShouldDelayContentTouches tries to call GetValue on null element.
        /// </summary>
        [Fact]
        public void ShouldDelayContentTouches_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ScrollView>>();
            config.Element.Returns((Microsoft.Maui.Controls.ScrollView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.ShouldDelayContentTouches());
        }

        /// <summary>
        /// Verifies that ShouldDelayContentTouches returns true when the underlying element's property value is true.
        /// Tests the case where the ScrollView element has ShouldDelayContentTouches set to true.
        /// Expected to return true.
        /// </summary>
        [Fact]
        public void ShouldDelayContentTouches_ElementReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ScrollView>>();
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty).Returns(true);
            config.Element.Returns((Microsoft.Maui.Controls.ScrollView)element);

            // Act
            bool result = config.ShouldDelayContentTouches();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Verifies that ShouldDelayContentTouches returns false when the underlying element's property value is false.
        /// Tests the case where the ScrollView element has ShouldDelayContentTouches set to false.
        /// Expected to return false.
        /// </summary>
        [Fact]
        public void ShouldDelayContentTouches_ElementReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ScrollView>>();
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty).Returns(false);
            config.Element.Returns((Microsoft.Maui.Controls.ScrollView)element);

            // Act
            bool result = config.ShouldDelayContentTouches();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that SetShouldDelayContentTouches extension method calls the static method with correct parameters and returns the config object when value is true.
        /// </summary>
        [Fact]
        public void SetShouldDelayContentTouches_WithTrueValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ScrollView>>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            var result = mockConfig.SetShouldDelayContentTouches(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShouldDelayContentTouches extension method calls the static method with correct parameters and returns the config object when value is false.
        /// </summary>
        [Fact]
        public void SetShouldDelayContentTouches_WithFalseValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ScrollView>>();
            mockConfig.Element.Returns(mockElement);
            bool value = false;

            // Act
            var result = mockConfig.SetShouldDelayContentTouches(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetShouldDelayContentTouches throws ArgumentNullException when element parameter is null.
        /// Input: null BindableObject
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void GetShouldDelayContentTouches_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ScrollView.GetShouldDelayContentTouches(element));
        }

        /// <summary>
        /// Tests that GetShouldDelayContentTouches returns the correct boolean value from the BindableObject.
        /// Input: BindableObject with ShouldDelayContentTouches set to various boolean values
        /// Expected: Returns the stored boolean value
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetShouldDelayContentTouches_ValidElement_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ScrollView.ShouldDelayContentTouchesProperty).Returns(expectedValue);

            // Act
            bool result = ScrollView.GetShouldDelayContentTouches(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetShouldDelayContentTouches works with a concrete ScrollView instance.
        /// Input: ScrollView instance
        /// Expected: Returns the default value (true) when property not explicitly set
        /// </summary>
        [Fact]
        public void GetShouldDelayContentTouches_ScrollViewInstance_ReturnsDefaultValue()
        {
            // Arrange
            var scrollView = new global::Microsoft.Maui.Controls.ScrollView();

            // Act
            bool result = ScrollView.GetShouldDelayContentTouches(scrollView);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetShouldDelayContentTouches returns the correct value after property is explicitly set.
        /// Input: ScrollView with ShouldDelayContentTouches explicitly set to false
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void GetShouldDelayContentTouches_ScrollViewWithExplicitValue_ReturnsSetValue()
        {
            // Arrange
            var scrollView = new global::Microsoft.Maui.Controls.ScrollView();
            ScrollView.SetShouldDelayContentTouches(scrollView, false);

            // Act
            bool result = ScrollView.GetShouldDelayContentTouches(scrollView);

            // Assert
            Assert.False(result);
        }
    }
}