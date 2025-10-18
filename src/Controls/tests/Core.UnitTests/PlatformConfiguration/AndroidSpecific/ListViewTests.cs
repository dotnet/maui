#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ListView platform-specific configuration methods.
    /// </summary>
    public partial class ListViewTests
    {
        /// <summary>
        /// Tests that SetIsFastScrollEnabled throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.SetIsFastScrollEnabled(element, value));
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled calls SetValue with the correct parameters when value is true.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_ValidElementAndTrueValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.SetIsFastScrollEnabled(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabledProperty,
                value);
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled calls SetValue with the correct parameters when value is false.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_ValidElementAndFalseValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.SetIsFastScrollEnabled(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabledProperty,
                value);
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled works correctly with different boolean values using parameterized test.
        /// </summary>
        /// <param name="value">The boolean value to test with.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetIsFastScrollEnabled_ValidElement_CallsSetValueWithProvidedBooleanValue(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.SetIsFastScrollEnabled(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabledProperty,
                value);
        }

        /// <summary>
        /// Tests that IsFastScrollEnabled returns true when the underlying element has fast scroll enabled.
        /// Verifies the extension method correctly delegates to GetIsFastScrollEnabled and returns the expected true value.
        /// </summary>
        [Fact]
        public void IsFastScrollEnabled_ValidConfigWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ListView>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabledProperty).Returns(true);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.ListView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            bool result = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabled(mockConfig);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsFastScrollEnabled returns false when the underlying element has fast scroll disabled.
        /// Verifies the extension method correctly delegates to GetIsFastScrollEnabled and returns the expected false value.
        /// </summary>
        [Fact]
        public void IsFastScrollEnabled_ValidConfigWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ListView>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabledProperty).Returns(false);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.ListView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            bool result = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabled(mockConfig);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsFastScrollEnabled throws NullReferenceException when config parameter is null.
        /// Verifies proper null handling when attempting to access the Element property of a null config.
        /// </summary>
        [Fact]
        public void IsFastScrollEnabled_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.ListView> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabled(nullConfig));
        }

        /// <summary>
        /// Tests that IsFastScrollEnabled throws NullReferenceException when config.Element is null.
        /// Verifies proper null handling when the Element property returns null and GetIsFastScrollEnabled is called with null.
        /// </summary>
        [Fact]
        public void IsFastScrollEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.ListView>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.ListView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabled(mockConfig));
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled calls the static method with correct parameters when value is true
        /// and returns the config for method chaining.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_WithTrueValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Controls.ListView>>();
            var mockElement = Substitute.For<Controls.ListView>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            var result = mockConfig.SetIsFastScrollEnabled(value);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ListView.IsFastScrollEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled calls the static method with correct parameters when value is false
        /// and returns the config for method chaining.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_WithFalseValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Controls.ListView>>();
            var mockElement = Substitute.For<Controls.ListView>();
            mockConfig.Element.Returns(mockElement);
            bool value = false;

            // Act
            var result = mockConfig.SetIsFastScrollEnabled(value);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ListView.IsFastScrollEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Controls.ListView> config = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetIsFastScrollEnabled(value));
        }

        /// <summary>
        /// Tests that SetIsFastScrollEnabled correctly handles method chaining by verifying 
        /// the returned config can be used for subsequent operations.
        /// </summary>
        [Fact]
        public void SetIsFastScrollEnabled_MethodChaining_ReturnsOriginalConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Controls.ListView>>();
            var mockElement = Substitute.For<Controls.ListView>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result1 = mockConfig.SetIsFastScrollEnabled(true);
            var result2 = result1.SetIsFastScrollEnabled(false);

            // Assert
            Assert.Same(mockConfig, result1);
            Assert.Same(mockConfig, result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that GetIsFastScrollEnabled throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetIsFastScrollEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetIsFastScrollEnabled(element));
        }

        /// <summary>
        /// Tests that GetIsFastScrollEnabled returns the correct boolean value from the bindable property.
        /// </summary>
        /// <param name="propertyValue">The boolean value to be returned by the mocked GetValue method.</param>
        /// <param name="expected">The expected return value from GetIsFastScrollEnabled.</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void GetIsFastScrollEnabled_ValidElement_ReturnsPropertyValue(bool propertyValue, bool expected)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(ListView.IsFastScrollEnabledProperty).Returns(propertyValue);

            // Act
            var result = ListView.GetIsFastScrollEnabled(mockElement);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that GetIsFastScrollEnabled correctly calls GetValue with the IsFastScrollEnabledProperty.
        /// </summary>
        [Fact]
        public void GetIsFastScrollEnabled_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(ListView.IsFastScrollEnabledProperty).Returns(false);

            // Act
            ListView.GetIsFastScrollEnabled(mockElement);

            // Assert
            mockElement.Received(1).GetValue(ListView.IsFastScrollEnabledProperty);
        }
    }
}