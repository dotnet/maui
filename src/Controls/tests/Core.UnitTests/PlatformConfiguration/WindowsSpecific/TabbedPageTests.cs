#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.WindowsSpecific
{
    public class TabbedPageTests
    {
        /// <summary>
        /// Tests that GetHeaderIconsEnabled throws NullReferenceException when element parameter is null.
        /// This verifies the method's behavior with invalid input conditions.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetHeaderIconsEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.GetHeaderIconsEnabled(element));
        }

        /// <summary>
        /// Tests that GetHeaderIconsEnabled returns true when the bindable object has HeaderIconsEnabled set to true.
        /// This verifies the method correctly retrieves and casts the property value.
        /// Expected result: Method should return true.
        /// </summary>
        [Fact]
        public void GetHeaderIconsEnabled_ElementWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.HeaderIconsEnabledProperty).Returns(true);

            // Act
            var result = TabbedPage.GetHeaderIconsEnabled(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetHeaderIconsEnabled returns false when the bindable object has HeaderIconsEnabled set to false.
        /// This verifies the method correctly retrieves and casts the property value.
        /// Expected result: Method should return false.
        /// </summary>
        [Fact]
        public void GetHeaderIconsEnabled_ElementWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.HeaderIconsEnabledProperty).Returns(false);

            // Act
            var result = TabbedPage.GetHeaderIconsEnabled(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetHeaderIconsEnabled properly calls GetValue with the correct HeaderIconsEnabledProperty.
        /// This verifies the method uses the correct bindable property for value retrieval.
        /// Expected result: GetValue should be called exactly once with HeaderIconsEnabledProperty.
        /// </summary>
        [Fact]
        public void GetHeaderIconsEnabled_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.HeaderIconsEnabledProperty).Returns(true);

            // Act
            TabbedPage.GetHeaderIconsEnabled(element);

            // Assert
            element.Received(1).GetValue(TabbedPage.HeaderIconsEnabledProperty);
        }

        /// <summary>
        /// Tests that GetHeaderIconsEnabled extension method throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void GetHeaderIconsEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TabbedPage.GetHeaderIconsEnabled(config));
        }

        /// <summary>
        /// Tests that GetHeaderIconsEnabled extension method returns the correct value when called with a valid config.
        /// Verifies that it properly delegates to the underlying GetHeaderIconsEnabled method.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetHeaderIconsEnabled_ValidConfig_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var mockTabbedPage = Substitute.For<Microsoft.Maui.Controls.TabbedPage>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();

            mockConfig.Element.Returns(mockTabbedPage);
            mockTabbedPage.GetValue(TabbedPage.HeaderIconsEnabledProperty).Returns(expectedValue);

            // Act
            var result = TabbedPage.GetHeaderIconsEnabled(mockConfig);

            // Assert
            Assert.Equal(expectedValue, result);
            mockTabbedPage.Received(1).GetValue(TabbedPage.HeaderIconsEnabledProperty);
        }

        /// <summary>
        /// Tests that GetHeaderIconsEnabled extension method throws NullReferenceException when config.Element is null.
        /// </summary>
        [Fact]
        public void GetHeaderIconsEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.TabbedPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.GetHeaderIconsEnabled(mockConfig));
        }

        /// <summary>
        /// Tests that SetHeaderIconsEnabled calls the underlying method with correct parameters and returns the config object when value is true.
        /// Input: Valid config with Element and value = true
        /// Expected: Underlying SetHeaderIconsEnabled is called with correct parameters and same config is returned
        /// </summary>
        [Fact]
        public void SetHeaderIconsEnabled_ValidConfigWithTrueValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var value = true;

            // Act
            var result = TabbedPage.SetHeaderIconsEnabled(mockConfig, value);

            // Assert
            mockElement.Received(1).SetValue(TabbedPage.HeaderIconsEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsEnabled calls the underlying method with correct parameters and returns the config object when value is false.
        /// Input: Valid config with Element and value = false
        /// Expected: Underlying SetHeaderIconsEnabled is called with correct parameters and same config is returned
        /// </summary>
        [Fact]
        public void SetHeaderIconsEnabled_ValidConfigWithFalseValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var value = false;

            // Act
            var result = TabbedPage.SetHeaderIconsEnabled(mockConfig, value);

            // Assert
            mockElement.Received(1).SetValue(TabbedPage.HeaderIconsEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsEnabled throws NullReferenceException when config parameter is null.
        /// Input: null config parameter
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void SetHeaderIconsEnabled_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, TabbedPage> config = null;
            var value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.SetHeaderIconsEnabled(config, value));
        }

        /// <summary>
        /// Tests that SetHeaderIconsEnabled throws NullReferenceException when config.Element is null.
        /// Input: Valid config with null Element
        /// Expected: NullReferenceException is thrown when underlying method tries to access null element
        /// </summary>
        [Fact]
        public void SetHeaderIconsEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, TabbedPage>>();
            mockConfig.Element.Returns((BindableObject)null);
            var value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.SetHeaderIconsEnabled(mockConfig, value));
        }

        /// <summary>
        /// Tests that IsHeaderIconsEnabled throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void IsHeaderIconsEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Controls.TabbedPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PlatformConfiguration.WindowsSpecific.TabbedPage.IsHeaderIconsEnabled(config));
        }

        /// <summary>
        /// Tests that IsHeaderIconsEnabled throws NullReferenceException when config.Element is null.
        /// Input: config with null Element property.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void IsHeaderIconsEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Controls.TabbedPage>>();
            config.Element.Returns((Controls.TabbedPage)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => PlatformConfiguration.WindowsSpecific.TabbedPage.IsHeaderIconsEnabled(config));
        }

        /// <summary>
        /// Tests that IsHeaderIconsEnabled returns true when the underlying Element has HeaderIconsEnabled set to true.
        /// Input: valid config with Element that has HeaderIconsEnabled property set to true.
        /// Expected: Returns true.
        /// </summary>
        [Fact]
        public void IsHeaderIconsEnabled_ElementWithHeaderIconsEnabledTrue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsEnabledProperty).Returns(true);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Controls.TabbedPage>>();
            config.Element.Returns((Controls.TabbedPage)element);

            // Act
            bool result = PlatformConfiguration.WindowsSpecific.TabbedPage.IsHeaderIconsEnabled(config);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsHeaderIconsEnabled returns false when the underlying Element has HeaderIconsEnabled set to false.
        /// Input: valid config with Element that has HeaderIconsEnabled property set to false.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void IsHeaderIconsEnabled_ElementWithHeaderIconsEnabledFalse_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsEnabledProperty).Returns(false);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Controls.TabbedPage>>();
            config.Element.Returns((Controls.TabbedPage)element);

            // Act
            bool result = PlatformConfiguration.WindowsSpecific.TabbedPage.IsHeaderIconsEnabled(config);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsHeaderIconsEnabled calls GetValue with the correct HeaderIconsEnabledProperty on the Element.
        /// Input: valid config with mocked Element.
        /// Expected: GetValue is called with HeaderIconsEnabledProperty and the returned value is passed through.
        /// </summary>
        [Fact]
        public void IsHeaderIconsEnabled_ValidConfig_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsEnabledProperty).Returns(true);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Controls.TabbedPage>>();
            config.Element.Returns((Controls.TabbedPage)element);

            // Act
            PlatformConfiguration.WindowsSpecific.TabbedPage.IsHeaderIconsEnabled(config);

            // Assert
            element.Received(1).GetValue(PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsEnabledProperty);
        }

        /// <summary>
        /// Tests that EnableHeaderIcons calls SetHeaderIconsEnabled with the correct parameters when provided with a valid configuration.
        /// Input: Valid IPlatformElementConfiguration with mocked Element.
        /// Expected: SetHeaderIconsEnabled should be called on the element with value true.
        /// </summary>
        [Fact]
        public void EnableHeaderIcons__ValidConfig_CallsSetHeaderIconsEnabledWithTrue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            TabbedPage.EnableHeaderIcons(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(TabbedPage.HeaderIconsEnabledProperty, true);
        }

        /// <summary>
        /// Tests that EnableHeaderIcons throws ArgumentNullException when provided with null configuration.
        /// Input: null config parameter.
        /// Expected: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void EnableHeaderIcons_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TabbedPage.EnableHeaderIcons(config));
        }

        /// <summary>
        /// Tests that EnableHeaderIcons throws NullReferenceException when configuration Element property is null.
        /// Input: Valid configuration with null Element property.
        /// Expected: NullReferenceException should be thrown when accessing Element.SetValue.
        /// </summary>
        [Fact]
        public void EnableHeaderIcons_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.EnableHeaderIcons(mockConfig));
        }

        /// <summary>
        /// Tests that DisableHeaderIcons throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void DisableHeaderIcons_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, TabbedPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TabbedPage.DisableHeaderIcons(config));
        }

        /// <summary>
        /// Tests that DisableHeaderIcons calls SetValue on the element with HeaderIconsEnabledProperty and false value.
        /// </summary>
        [Fact]
        public void DisableHeaderIcons_ValidConfig_CallsSetValueWithFalse()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, TabbedPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            TabbedPage.DisableHeaderIcons(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(TabbedPage.HeaderIconsEnabledProperty, false);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var size = new Size(16, 16);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                PlatformConfiguration.WindowsSpecific.TabbedPage.SetHeaderIconsSize(element, size));
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize successfully sets the HeaderIconsSize property with various valid Size values.
        /// </summary>
        /// <param name="width">The width component of the size to test.</param>
        /// <param name="height">The height component of the size to test.</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(16.0, 16.0)]
        [InlineData(32.0, 32.0)]
        [InlineData(1.0, 2.0)]
        [InlineData(100.5, 200.75)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(-10.0, -20.0)]
        public void SetHeaderIconsSize_ValidElement_SetsPropertyValue(double width, double height)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var size = new Size(width, height);

            // Act
            PlatformConfiguration.WindowsSpecific.TabbedPage.SetHeaderIconsSize(element, size);

            // Assert
            element.Received(1).SetValue(
                PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsSizeProperty,
                size);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize works correctly with Size.Zero.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ZeroSize_SetsPropertyValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var size = Size.Zero;

            // Act
            PlatformConfiguration.WindowsSpecific.TabbedPage.SetHeaderIconsSize(element, size);

            // Assert
            element.Received(1).SetValue(
                PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsSizeProperty,
                size);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize handles special double values correctly.
        /// </summary>
        /// <param name="width">The width component with special double values.</param>
        /// <param name="height">The height component with special double values.</param>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 16.0)]
        [InlineData(16.0, double.NegativeInfinity)]
        [InlineData(double.NaN, 32.0)]
        public void SetHeaderIconsSize_SpecialDoubleValues_SetsPropertyValue(double width, double height)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var size = new Size(width, height);

            // Act
            PlatformConfiguration.WindowsSpecific.TabbedPage.SetHeaderIconsSize(element, size);

            // Assert
            element.Received(1).SetValue(
                PlatformConfiguration.WindowsSpecific.TabbedPage.HeaderIconsSizeProperty,
                size);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize throws NullReferenceException when element parameter is null.
        /// Verifies proper null parameter handling for the bindable object parameter.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.GetHeaderIconsSize(element));
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize returns the default Size value when no custom value has been set.
        /// Verifies that the method correctly retrieves the default value from the HeaderIconsSizeProperty.
        /// Expected result: Returns Size(16, 16) which is the default value defined in HeaderIconsSizeProperty.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_DefaultValue_ReturnsDefaultSize()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var defaultSize = new Size(16, 16);
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(defaultSize);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            Assert.Equal(defaultSize, result);
            Assert.Equal(16, result.Width);
            Assert.Equal(16, result.Height);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize returns a custom Size value when a specific value has been set.
        /// Verifies that the method correctly retrieves custom values from the HeaderIconsSizeProperty.
        /// Expected result: Returns the custom Size value that was previously set.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_CustomValue_ReturnsCustomSize()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var customSize = new Size(32, 24);
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(customSize);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            Assert.Equal(customSize, result);
            Assert.Equal(32, result.Width);
            Assert.Equal(24, result.Height);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize handles zero-sized values correctly.
        /// Verifies that the method properly handles boundary case of Size.Zero.
        /// Expected result: Returns Size.Zero (0, 0).
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_ZeroSize_ReturnsZeroSize()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(Size.Zero);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            Assert.Equal(Size.Zero, result);
            Assert.Equal(0, result.Width);
            Assert.Equal(0, result.Height);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize handles extreme size values correctly.
        /// Verifies that the method properly handles boundary cases with large dimensions.
        /// Expected result: Returns the large Size value correctly.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_LargeSize_ReturnsLargeSize()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var largeSize = new Size(double.MaxValue, double.MaxValue);
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(largeSize);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            Assert.Equal(largeSize, result);
            Assert.Equal(double.MaxValue, result.Width);
            Assert.Equal(double.MaxValue, result.Height);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize handles negative size values correctly.
        /// Verifies that the method properly handles boundary cases with negative dimensions.
        /// Expected result: Returns the negative Size value correctly.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_NegativeSize_ReturnsNegativeSize()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var negativeSize = new Size(-10, -20);
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(negativeSize);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            Assert.Equal(negativeSize, result);
            Assert.Equal(-10, result.Width);
            Assert.Equal(-20, result.Height);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize handles special double values correctly.
        /// Verifies that the method properly handles special floating-point values like infinity and NaN.
        /// Expected result: Returns the Size with special values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void GetHeaderIconsSize_SpecialDoubleValues_ReturnsSpecialValues(double width, double height)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var specialSize = new Size(width, height);
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(specialSize);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            Assert.Equal(specialSize, result);
            Assert.Equal(width, result.Width);
            Assert.Equal(height, result.Height);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize correctly calls GetValue with the HeaderIconsSizeProperty.
        /// Verifies that the method uses the correct bindable property when retrieving the value.
        /// Expected result: GetValue is called once with the HeaderIconsSizeProperty parameter.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedSize = new Size(20, 30);
            element.GetValue(TabbedPage.HeaderIconsSizeProperty).Returns(expectedSize);

            // Act
            var result = TabbedPage.GetHeaderIconsSize(element);

            // Assert
            element.Received(1).GetValue(TabbedPage.HeaderIconsSizeProperty);
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize throws NullReferenceException when config parameter is null.
        /// This test verifies the method's behavior with null input and ensures proper exception handling.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.TabbedPage> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                WindowsSpecific.TabbedPage.GetHeaderIconsSize(config));
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize returns the correct Size value when provided with a valid config.
        /// This test verifies the method delegates correctly to the underlying GetHeaderIconsSize method
        /// and returns the expected Size from the element's HeaderIconsSizeProperty.
        /// Expected result: The Size value returned should match the mocked element's property value.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_ValidConfig_ReturnsExpectedSize()
        {
            // Arrange
            var expectedSize = new Size(24, 24);
            var mockElement = Substitute.For<Microsoft.Maui.Controls.TabbedPage>();
            mockElement.GetValue(WindowsSpecific.TabbedPage.HeaderIconsSizeProperty).Returns(expectedSize);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.TabbedPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = WindowsSpecific.TabbedPage.GetHeaderIconsSize(mockConfig);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize returns the default Size value when element has default property value.
        /// This test verifies the method works correctly with the default HeaderIconsSizeProperty value.
        /// Expected result: The default Size value (16, 16) should be returned.
        /// </summary>
        [Fact]
        public void GetHeaderIconsSize_ValidConfigWithDefaultSize_ReturnsDefaultSize()
        {
            // Arrange
            var defaultSize = new Size(16, 16);
            var mockElement = Substitute.For<Microsoft.Maui.Controls.TabbedPage>();
            mockElement.GetValue(WindowsSpecific.TabbedPage.HeaderIconsSizeProperty).Returns(defaultSize);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.TabbedPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = WindowsSpecific.TabbedPage.GetHeaderIconsSize(mockConfig);

            // Assert
            Assert.Equal(defaultSize, result);
        }

        /// <summary>
        /// Tests that GetHeaderIconsSize handles edge case Size values correctly.
        /// This test verifies the method works with boundary Size values like Zero and very large sizes.
        /// Expected result: The method should return the exact Size value from the element property.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(100, 100)]
        [InlineData(double.MaxValue, double.MaxValue)]
        public void GetHeaderIconsSize_ValidConfigWithVariousSizes_ReturnsCorrectSize(double width, double height)
        {
            // Arrange
            var expectedSize = new Size(width, height);
            var mockElement = Substitute.For<Microsoft.Maui.Controls.TabbedPage>();
            mockElement.GetValue(WindowsSpecific.TabbedPage.HeaderIconsSizeProperty).Returns(expectedSize);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.TabbedPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = WindowsSpecific.TabbedPage.GetHeaderIconsSize(mockConfig);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method calls the static SetHeaderIconsSize method with correct parameters and returns the config object.
        /// Input: Valid config and Size(16, 16).
        /// Expected: Static method called with correct parameters and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndPositiveSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(16, 16);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles zero size values correctly.
        /// Input: Valid config and Size(0, 0).
        /// Expected: Static method called with zero size and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndZeroSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(0, 0);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles negative size values correctly.
        /// Input: Valid config and Size(-10, -20).
        /// Expected: Static method called with negative size and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndNegativeSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(-10, -20);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles extreme large size values correctly.
        /// Input: Valid config and Size(double.MaxValue, double.MaxValue).
        /// Expected: Static method called with extreme values and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndMaxSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(double.MaxValue, double.MaxValue);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles extreme small size values correctly.
        /// Input: Valid config and Size(double.MinValue, double.MinValue).
        /// Expected: Static method called with extreme values and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndMinSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(double.MinValue, double.MinValue);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles NaN size values correctly.
        /// Input: Valid config and Size(double.NaN, double.NaN).
        /// Expected: Static method called with NaN values and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndNaNSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(double.NaN, double.NaN);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles positive infinity size values correctly.
        /// Input: Valid config and Size(double.PositiveInfinity, double.PositiveInfinity).
        /// Expected: Static method called with infinity values and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndPositiveInfinitySize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles negative infinity size values correctly.
        /// Input: Valid config and Size(double.NegativeInfinity, double.NegativeInfinity).
        /// Expected: Static method called with negative infinity values and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndNegativeInfinitySize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(double.NegativeInfinity, double.NegativeInfinity);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method handles mixed size values correctly.
        /// Input: Valid config and Size(100, -50).
        /// Expected: Static method called with mixed values and config returned.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_ValidConfigAndMixedSize_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);
            var size = new Size(100, -50);

            // Act
            var result = config.SetHeaderIconsSize(size);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsSizeProperty, size);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetHeaderIconsSize extension method throws when config is null.
        /// Input: Null config and valid size.
        /// Expected: ArgumentNullException thrown.
        /// </summary>
        [Fact]
        public void SetHeaderIconsSize_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.TabbedPage> config = null;
            var size = new Size(16, 16);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetHeaderIconsSize(size));
        }

        /// <summary>
        /// Tests that SetHeaderIconsEnabled calls SetValue on the element with the correct property and value
        /// when provided with a valid BindableObject element and boolean value.
        /// </summary>
        /// <param name="value">The boolean value to test (true or false)</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetHeaderIconsEnabled_ValidElement_CallsSetValueWithCorrectValue(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            TabbedPage.SetHeaderIconsEnabled(element, value);

            // Assert
            element.Received(1).SetValue(TabbedPage.HeaderIconsEnabledProperty, value);
        }

        /// <summary>
        /// Tests that SetHeaderIconsEnabled throws NullReferenceException when the element parameter is null,
        /// since the method attempts to call SetValue on the null element.
        /// </summary>
        [Fact]
        public void SetHeaderIconsEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.SetHeaderIconsEnabled(element, value));
        }
    }
}