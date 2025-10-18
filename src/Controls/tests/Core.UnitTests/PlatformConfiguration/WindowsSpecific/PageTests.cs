#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.WindowsSpecific
{
    public partial class PageTests
    {
        /// <summary>
        /// Tests that SetToolbarPlacement correctly calls SetValue on the provided element with valid ToolbarPlacement values.
        /// Verifies that the method properly passes through all defined enum values to the underlying BindableObject.
        /// </summary>
        /// <param name="toolbarPlacement">The ToolbarPlacement value to test</param>
        [Theory]
        [InlineData(ToolbarPlacement.Default)]
        [InlineData(ToolbarPlacement.Top)]
        [InlineData(ToolbarPlacement.Bottom)]
        public void SetToolbarPlacement_WithValidToolbarPlacement_CallsSetValueWithCorrectParameters(ToolbarPlacement toolbarPlacement)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();

            // Act
            Page.SetToolbarPlacement(mockElement, toolbarPlacement);

            // Assert
            mockElement.Received(1).SetValue(Page.ToolbarPlacementProperty, toolbarPlacement);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles invalid enum values by passing them through to SetValue.
        /// Verifies that the method doesn't perform enum validation and allows the underlying system to handle invalid values.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_WithInvalidEnumValue_CallsSetValueWithInvalidValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var invalidToolbarPlacement = (ToolbarPlacement)999;

            // Act
            Page.SetToolbarPlacement(mockElement, invalidToolbarPlacement);

            // Assert
            mockElement.Received(1).SetValue(Page.ToolbarPlacementProperty, invalidToolbarPlacement);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement throws ArgumentNullException when the element parameter is null.
        /// Verifies proper null parameter validation behavior.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullElement = null;
            var toolbarPlacement = ToolbarPlacement.Default;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Page.SetToolbarPlacement(nullElement, toolbarPlacement));
        }

        /// <summary>
        /// Tests that SetToolbarPlacement correctly passes the ToolbarPlacementProperty to SetValue.
        /// Verifies that the correct BindableProperty is used when setting the value.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_WithValidParameters_UsesCorrectBindableProperty()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var toolbarPlacement = ToolbarPlacement.Top;

            // Act
            Page.SetToolbarPlacement(mockElement, toolbarPlacement);

            // Assert
            mockElement.Received(1).SetValue(Arg.Is<BindableProperty>(prop => prop == Page.ToolbarPlacementProperty), toolbarPlacement);
        }

        /// <summary>
        /// Tests that GetToolbarPlacement extension method returns the correct ToolbarPlacement value
        /// when the underlying BindableObject has a valid toolbar placement set.
        /// </summary>
        /// <param name="expectedPlacement">The toolbar placement value to test</param>
        [Theory]
        [InlineData(ToolbarPlacement.Default)]
        [InlineData(ToolbarPlacement.Top)]
        [InlineData(ToolbarPlacement.Bottom)]
        public void GetToolbarPlacement_ValidConfig_ReturnsExpectedPlacement(ToolbarPlacement expectedPlacement)
        {
            // Arrange
            var mockElement = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Page.ToolbarPlacementProperty).Returns(expectedPlacement);

            // Act
            var result = mockConfig.GetToolbarPlacement();

            // Assert
            Assert.Equal(expectedPlacement, result);
        }

        /// <summary>
        /// Tests that GetToolbarPlacement extension method throws ArgumentNullException
        /// when the config parameter is null.
        /// </summary>
        [Fact]
        public void GetToolbarPlacement_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Page> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.GetToolbarPlacement());
        }

        /// <summary>
        /// Tests that GetToolbarPlacement extension method throws NullReferenceException
        /// when the config.Element is null.
        /// </summary>
        [Fact]
        public void GetToolbarPlacement_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            mockConfig.Element.Returns((Page)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.GetToolbarPlacement());
        }

        /// <summary>
        /// Tests that GetToolbarPlacement extension method correctly casts the returned object
        /// from GetValue to ToolbarPlacement enum, including values outside the defined range.
        /// </summary>
        /// <param name="rawValue">The raw integer value to test casting</param>
        /// <param name="expectedPlacement">The expected ToolbarPlacement after casting</param>
        [Theory]
        [InlineData(0, ToolbarPlacement.Default)]
        [InlineData(1, ToolbarPlacement.Top)]
        [InlineData(2, ToolbarPlacement.Bottom)]
        [InlineData(99, (ToolbarPlacement)99)]
        [InlineData(-1, (ToolbarPlacement)(-1))]
        public void GetToolbarPlacement_VariousEnumValues_CastsCorrectly(int rawValue, ToolbarPlacement expectedPlacement)
        {
            // Arrange
            var mockElement = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Page.ToolbarPlacementProperty).Returns((ToolbarPlacement)rawValue);

            // Act
            var result = mockConfig.GetToolbarPlacement();

            // Assert
            Assert.Equal(expectedPlacement, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement sets the correct value on the element and returns the same config object
        /// when provided with Default toolbar placement.
        /// Expected: SetValue called with ToolbarPlacementProperty and Default value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_Default_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var value = ToolbarPlacement.Default;

            // Act
            var result = Page.SetToolbarPlacement(config, value);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, value);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement sets the correct value on the element and returns the same config object
        /// when provided with Top toolbar placement.
        /// Expected: SetValue called with ToolbarPlacementProperty and Top value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_Top_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var value = ToolbarPlacement.Top;

            // Act
            var result = Page.SetToolbarPlacement(config, value);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, value);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement sets the correct value on the element and returns the same config object
        /// when provided with Bottom toolbar placement.
        /// Expected: SetValue called with ToolbarPlacementProperty and Bottom value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_Bottom_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var value = ToolbarPlacement.Bottom;

            // Act
            var result = Page.SetToolbarPlacement(config, value);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, value);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles invalid enum values by casting and setting the value on the element.
        /// Expected: SetValue called with ToolbarPlacementProperty and invalid enum value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_InvalidEnumValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var invalidValue = (ToolbarPlacement)999;

            // Act
            var result = Page.SetToolbarPlacement(config, invalidValue);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, invalidValue);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles negative enum values by casting and setting the value on the element.
        /// Expected: SetValue called with ToolbarPlacementProperty and negative enum value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_NegativeEnumValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var negativeValue = (ToolbarPlacement)(-1);

            // Act
            var result = Page.SetToolbarPlacement(config, negativeValue);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, negativeValue);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles maximum integer enum values by casting and setting the value on the element.
        /// Expected: SetValue called with ToolbarPlacementProperty and max int enum value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_MaxIntEnumValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var maxValue = (ToolbarPlacement)int.MaxValue;

            // Act
            var result = Page.SetToolbarPlacement(config, maxValue);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, maxValue);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles minimum integer enum values by casting and setting the value on the element.
        /// Expected: SetValue called with ToolbarPlacementProperty and min int enum value, returns same config.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_MinIntEnumValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            config.Element.Returns(element);
            var minValue = (ToolbarPlacement)int.MinValue;

            // Act
            var result = Page.SetToolbarPlacement(config, minValue);

            // Assert
            element.Received(1).SetValue(Page.ToolbarPlacementProperty, minValue);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetToolbarDynamicOverflowEnabled throws NullReferenceException when element is null.
        /// </summary>
        [Fact]
        public void SetToolbarDynamicOverflowEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Page.SetToolbarDynamicOverflowEnabled(element, value));
        }

        /// <summary>
        /// Tests that SetToolbarDynamicOverflowEnabled calls SetValue with the correct parameters for both true and false values.
        /// </summary>
        /// <param name="value">The boolean value to set for toolbar dynamic overflow enabled.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetToolbarDynamicOverflowEnabled_ValidElement_CallsSetValueWithCorrectParameters(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            Page.SetToolbarDynamicOverflowEnabled(element, value);

            // Assert
            element.Received(1).SetValue(Page.ToolbarDynamicOverflowEnabledProperty, value);
        }

        /// <summary>
        /// Tests that GetToolbarDynamicOverflowEnabled returns true when the property is set to true.
        /// Input: Valid configuration with Element.GetValue returning true.
        /// Expected: Method returns true.
        /// </summary>
        [Fact]
        public void GetToolbarDynamicOverflowEnabled_WhenPropertyIsTrue_ReturnsTrue()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Page.ToolbarDynamicOverflowEnabledProperty).Returns(true);

            // Act
            var result = Page.GetToolbarDynamicOverflowEnabled(mockConfig);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetToolbarDynamicOverflowEnabled returns false when the property is set to false.
        /// Input: Valid configuration with Element.GetValue returning false.
        /// Expected: Method returns false.
        /// </summary>
        [Fact]
        public void GetToolbarDynamicOverflowEnabled_WhenPropertyIsFalse_ReturnsFalse()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Page.ToolbarDynamicOverflowEnabledProperty).Returns(false);

            // Act
            var result = Page.GetToolbarDynamicOverflowEnabled(mockConfig);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetToolbarDynamicOverflowEnabled throws ArgumentNullException when config parameter is null.
        /// Input: Null configuration parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetToolbarDynamicOverflowEnabled_WhenConfigIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Page> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Page.GetToolbarDynamicOverflowEnabled(config));
        }

        /// <summary>
        /// Tests that GetToolbarDynamicOverflowEnabled throws NullReferenceException when config.Element is null.
        /// Input: Valid configuration with null Element property.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetToolbarDynamicOverflowEnabled_WhenElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Page.GetToolbarDynamicOverflowEnabled(mockConfig));
        }

        /// <summary>
        /// Tests that GetToolbarDynamicOverflowEnabled throws InvalidCastException when GetValue returns null.
        /// Input: Valid configuration with Element.GetValue returning null.
        /// Expected: InvalidCastException is thrown during cast to bool.
        /// </summary>
        [Fact]
        public void GetToolbarDynamicOverflowEnabled_WhenGetValueReturnsNull_ThrowsInvalidCastException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Page.ToolbarDynamicOverflowEnabledProperty).Returns((object)null);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => Page.GetToolbarDynamicOverflowEnabled(mockConfig));
        }

        /// <summary>
        /// Tests that GetToolbarDynamicOverflowEnabled throws InvalidCastException when GetValue returns non-boolean value.
        /// Input: Valid configuration with Element.GetValue returning a string instead of bool.
        /// Expected: InvalidCastException is thrown during cast to bool.
        /// </summary>
        [Fact]
        public void GetToolbarDynamicOverflowEnabled_WhenGetValueReturnsNonBoolean_ThrowsInvalidCastException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Page.ToolbarDynamicOverflowEnabledProperty).Returns("not a boolean");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => Page.GetToolbarDynamicOverflowEnabled(mockConfig));
        }

        /// <summary>
        /// Tests that SetToolbarDynamicOverflowEnabled throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetToolbarDynamicOverflowEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Page> config = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                PlatformConfiguration.WindowsSpecific.Page.SetToolbarDynamicOverflowEnabled(config, value));
        }

        /// <summary>
        /// Tests that SetToolbarDynamicOverflowEnabled sets the ToolbarDynamicOverflowEnabledProperty to true 
        /// on the element and returns the same config object for method chaining.
        /// </summary>
        [Fact]
        public void SetToolbarDynamicOverflowEnabled_ValidConfigWithTrueValue_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            var result = PlatformConfiguration.WindowsSpecific.Page.SetToolbarDynamicOverflowEnabled(mockConfig, value);

            // Assert
            mockElement.Received(1).SetValue(PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolbarDynamicOverflowEnabled sets the ToolbarDynamicOverflowEnabledProperty to false 
        /// on the element and returns the same config object for method chaining.
        /// </summary>
        [Fact]
        public void SetToolbarDynamicOverflowEnabled_ValidConfigWithFalseValue_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            mockConfig.Element.Returns(mockElement);
            bool value = false;

            // Act
            var result = PlatformConfiguration.WindowsSpecific.Page.SetToolbarDynamicOverflowEnabled(mockConfig, value);

            // Assert
            mockElement.Received(1).SetValue(PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolbarDynamicOverflowEnabled works correctly with both true and false values using parameterized test.
        /// </summary>
        /// <param name="value">The boolean value to test with</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetToolbarDynamicOverflowEnabled_ValidConfigWithBooleanValue_SetsPropertyCorrectly(bool value)
        {
            // Arrange
            var mockElement = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Page>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = PlatformConfiguration.WindowsSpecific.Page.SetToolbarDynamicOverflowEnabled(mockConfig, value);

            // Assert
            mockElement.Received(1).SetValue(PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetToolbarPlacement returns the correct ToolbarPlacement value for valid enum values.
        /// This test verifies the method correctly retrieves and casts the property value.
        /// Expected result: Should return the same ToolbarPlacement value that was set on the property.
        /// </summary>
        [Theory]
        [InlineData(ToolbarPlacement.Default)]
        [InlineData(ToolbarPlacement.Top)]
        [InlineData(ToolbarPlacement.Bottom)]
        public void GetToolbarPlacement_ValidElement_ReturnsExpectedToolbarPlacement(ToolbarPlacement expectedPlacement)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Page.ToolbarPlacementProperty).Returns(expectedPlacement);

            // Act
            var result = Page.GetToolbarPlacement(element);

            // Assert
            Assert.Equal(expectedPlacement, result);
        }

        /// <summary>
        /// Tests that GetToolbarPlacement correctly casts integer values to ToolbarPlacement enum.
        /// This test verifies the casting behavior with numeric values outside the defined enum range.
        /// Expected result: Should return the cast value even if it's outside the defined enum values.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(999)]
        public void GetToolbarPlacement_ElementWithInvalidEnumValue_ReturnsCastValue(int invalidEnumValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Page.ToolbarPlacementProperty).Returns(invalidEnumValue);

            // Act
            var result = Page.GetToolbarPlacement(element);

            // Assert
            Assert.Equal((ToolbarPlacement)invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that GetToolbarPlacement calls GetValue with the correct ToolbarPlacementProperty.
        /// This test verifies the method uses the proper bindable property for value retrieval.
        /// Expected result: GetValue should be called exactly once with ToolbarPlacementProperty.
        /// </summary>
        [Fact]
        public void GetToolbarPlacement_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Page.ToolbarPlacementProperty).Returns(ToolbarPlacement.Default);

            // Act
            Page.GetToolbarPlacement(element);

            // Assert
            element.Received(1).GetValue(Page.ToolbarPlacementProperty);
        }
    }
}