#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.WindowsSpecific
{
    public class FlyoutPageTests
    {
        /// <summary>
        /// Tests that SetCollapseStyle throws NullReferenceException when element parameter is null.
        /// Verifies null parameter handling for the element parameter.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void SetCollapseStyle_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var collapseStyle = CollapseStyle.Full;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlyoutPage.SetCollapseStyle(element, collapseStyle));
        }

        /// <summary>
        /// Tests that SetCollapseStyle calls SetValue with correct parameters for valid CollapseStyle values.
        /// Verifies the method correctly delegates to BindableObject.SetValue with proper arguments.
        /// Expected result: SetValue should be called once with CollapseStyleProperty and the specified collapseStyle.
        /// </summary>
        [Theory]
        [InlineData(CollapseStyle.Full)]
        [InlineData(CollapseStyle.Partial)]
        public void SetCollapseStyle_ValidCollapseStyle_CallsSetValueWithCorrectParameters(CollapseStyle collapseStyle)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            FlyoutPage.SetCollapseStyle(element, collapseStyle);

            // Assert
            element.Received(1).SetValue(FlyoutPage.CollapseStyleProperty, collapseStyle);
        }

        /// <summary>
        /// Tests that SetCollapseStyle handles invalid CollapseStyle enum values by passing them to SetValue.
        /// Verifies the method behavior with out-of-range enum values.
        /// Expected result: SetValue should be called with the invalid enum value without validation.
        /// </summary>
        [Theory]
        [InlineData((CollapseStyle)(-1))]
        [InlineData((CollapseStyle)999)]
        [InlineData((CollapseStyle)int.MaxValue)]
        [InlineData((CollapseStyle)int.MinValue)]
        public void SetCollapseStyle_InvalidCollapseStyleValue_CallsSetValueWithInvalidValue(CollapseStyle invalidCollapseStyle)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            FlyoutPage.SetCollapseStyle(element, invalidCollapseStyle);

            // Assert
            element.Received(1).SetValue(FlyoutPage.CollapseStyleProperty, invalidCollapseStyle);
        }

        /// <summary>
        /// Tests that GetCollapseStyle throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void GetCollapseStyle_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => FlyoutPage.GetCollapseStyle(config));
        }

        /// <summary>
        /// Tests that GetCollapseStyle returns the correct CollapseStyle value from the configuration.
        /// Tests both Full and Partial enum values to ensure proper casting and retrieval.
        /// </summary>
        /// <param name="expectedCollapseStyle">The CollapseStyle value that should be returned by GetValue.</param>
        [Theory]
        [InlineData(CollapseStyle.Full)]
        [InlineData(CollapseStyle.Partial)]
        public void GetCollapseStyle_ValidConfig_ReturnsCorrectCollapseStyle(CollapseStyle expectedCollapseStyle)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.FlyoutPage>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(FlyoutPage.CollapseStyleProperty).Returns(expectedCollapseStyle);

            // Act
            var result = FlyoutPage.GetCollapseStyle(mockConfig);

            // Assert
            Assert.Equal(expectedCollapseStyle, result);
            mockElement.Received(1).GetValue(FlyoutPage.CollapseStyleProperty);
        }

        /// <summary>
        /// Tests that SetCollapseStyle with CollapseStyle.Full calls SetValue on the element with the correct parameters and returns the same config.
        /// </summary>
        [Fact]
        public void SetCollapseStyle_WithCollapseStyleFull_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.FlyoutPage>();
            mockConfig.Element.Returns(mockElement);
            var collapseStyle = CollapseStyle.Full;

            // Act
            var result = WindowsSpecific.FlyoutPage.SetCollapseStyle(mockConfig, collapseStyle);

            // Assert
            mockElement.Received(1).SetValue(WindowsSpecific.FlyoutPage.CollapseStyleProperty, collapseStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetCollapseStyle with CollapseStyle.Partial calls SetValue on the element with the correct parameters and returns the same config.
        /// </summary>
        [Fact]
        public void SetCollapseStyle_WithCollapseStylePartial_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.FlyoutPage>();
            mockConfig.Element.Returns(mockElement);
            var collapseStyle = CollapseStyle.Partial;

            // Act
            var result = WindowsSpecific.FlyoutPage.SetCollapseStyle(mockConfig, collapseStyle);

            // Assert
            mockElement.Received(1).SetValue(WindowsSpecific.FlyoutPage.CollapseStyleProperty, collapseStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetCollapseStyle with an invalid enum value (outside defined range) still calls SetValue and returns the config.
        /// </summary>
        [Fact]
        public void SetCollapseStyle_WithInvalidEnumValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.FlyoutPage>();
            mockConfig.Element.Returns(mockElement);
            var invalidCollapseStyle = (CollapseStyle)999;

            // Act
            var result = WindowsSpecific.FlyoutPage.SetCollapseStyle(mockConfig, invalidCollapseStyle);

            // Assert
            mockElement.Received(1).SetValue(WindowsSpecific.FlyoutPage.CollapseStyleProperty, invalidCollapseStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetCollapseStyle throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetCollapseStyle_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.FlyoutPage> nullConfig = null;
            var collapseStyle = CollapseStyle.Full;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => WindowsSpecific.FlyoutPage.SetCollapseStyle(nullConfig, collapseStyle));
        }

        /// <summary>
        /// Tests that SetCollapseStyle can be used in a fluent interface chain by returning the same config object.
        /// </summary>
        [Fact]
        public void SetCollapseStyle_FluentInterface_ReturnsSameConfigObject()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.FlyoutPage>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result1 = WindowsSpecific.FlyoutPage.SetCollapseStyle(mockConfig, CollapseStyle.Full);
            var result2 = WindowsSpecific.FlyoutPage.SetCollapseStyle(result1, CollapseStyle.Partial);

            // Assert
            Assert.Same(mockConfig, result1);
            Assert.Same(mockConfig, result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that GetCollapsedPaneWidth throws NullReferenceException when element parameter is null.
        /// Input: null BindableObject
        /// Expected: NullReferenceException thrown
        /// </summary>
        [Fact]
        public void GetCollapsedPaneWidth_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlyoutPage.GetCollapsedPaneWidth(element));
        }

        /// <summary>
        /// Tests that GetCollapsedPaneWidth returns the default value when called on a valid element.
        /// Input: Valid BindableObject with default collapsed pane width value (48.0)
        /// Expected: Returns 48.0
        /// </summary>
        [Fact]
        public void GetCollapsedPaneWidth_ValidElementWithDefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.CollapsedPaneWidthProperty).Returns(48.0);

            // Act
            var result = FlyoutPage.GetCollapsedPaneWidth(element);

            // Assert
            Assert.Equal(48.0, result);
        }

        /// <summary>
        /// Tests that GetCollapsedPaneWidth returns a custom value when set on the element.
        /// Input: Valid BindableObject with custom collapsed pane width value (100.5)
        /// Expected: Returns 100.5
        /// </summary>
        [Fact]
        public void GetCollapsedPaneWidth_ValidElementWithCustomValue_ReturnsCustomValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var customValue = 100.5;
            element.GetValue(FlyoutPage.CollapsedPaneWidthProperty).Returns(customValue);

            // Act
            var result = FlyoutPage.GetCollapsedPaneWidth(element);

            // Assert
            Assert.Equal(customValue, result);
        }

        /// <summary>
        /// Tests that GetCollapsedPaneWidth handles boundary value zero correctly.
        /// Input: Valid BindableObject with collapsed pane width value of 0.0
        /// Expected: Returns 0.0
        /// </summary>
        [Fact]
        public void GetCollapsedPaneWidth_ValidElementWithZeroValue_ReturnsZero()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.CollapsedPaneWidthProperty).Returns(0.0);

            // Act
            var result = FlyoutPage.GetCollapsedPaneWidth(element);

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that GetCollapsedPaneWidth handles very large values correctly.
        /// Input: Valid BindableObject with collapsed pane width value of double.MaxValue
        /// Expected: Returns double.MaxValue
        /// </summary>
        [Fact]
        public void GetCollapsedPaneWidth_ValidElementWithMaxValue_ReturnsMaxValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.CollapsedPaneWidthProperty).Returns(double.MaxValue);

            // Act
            var result = FlyoutPage.GetCollapsedPaneWidth(element);

            // Assert
            Assert.Equal(double.MaxValue, result);
        }

        /// <summary>
        /// Tests that GetCollapsedPaneWidth handles very small positive values correctly.
        /// Input: Valid BindableObject with collapsed pane width value of double.Epsilon
        /// Expected: Returns double.Epsilon
        /// </summary>
        [Fact]
        public void GetCollapsedPaneWidth_ValidElementWithEpsilonValue_ReturnsEpsilon()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.CollapsedPaneWidthProperty).Returns(double.Epsilon);

            // Act
            var result = FlyoutPage.GetCollapsedPaneWidth(element);

            // Assert
            Assert.Equal(double.Epsilon, result);
        }

        /// <summary>
        /// Tests that SetCollapsedPaneWidth throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetCollapsedPaneWidth_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            double collapsedPaneWidth = 48.0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                FlyoutPage.SetCollapsedPaneWidth(element, collapsedPaneWidth));
        }

        /// <summary>
        /// Tests that SetCollapsedPaneWidth successfully sets a valid positive value.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(48.0)]
        [InlineData(100.5)]
        [InlineData(double.MaxValue)]
        public void SetCollapsedPaneWidth_ValidValue_SetsPropertyValue(double collapsedPaneWidth)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            FlyoutPage.SetCollapsedPaneWidth(element, collapsedPaneWidth);

            // Assert
            element.Received(1).SetValue(FlyoutPage.CollapsedPaneWidthProperty, collapsedPaneWidth);
        }

        /// <summary>
        /// Tests that SetCollapsedPaneWidth handles negative values according to the property validation.
        /// The validation should prevent negative values, but the method itself should still call SetValue.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-48.0)]
        [InlineData(double.MinValue)]
        public void SetCollapsedPaneWidth_NegativeValue_CallsSetValue(double collapsedPaneWidth)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            FlyoutPage.SetCollapsedPaneWidth(element, collapsedPaneWidth);

            // Assert
            element.Received(1).SetValue(FlyoutPage.CollapsedPaneWidthProperty, collapsedPaneWidth);
        }

        /// <summary>
        /// Tests that SetCollapsedPaneWidth handles special double values like NaN and infinity.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void SetCollapsedPaneWidth_SpecialDoubleValues_CallsSetValue(double collapsedPaneWidth)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            FlyoutPage.SetCollapsedPaneWidth(element, collapsedPaneWidth);

            // Assert
            element.Received(1).SetValue(FlyoutPage.CollapsedPaneWidthProperty, collapsedPaneWidth);
        }

        /// <summary>
        /// Tests that CollapsedPaneWidth sets the value on the element and returns the same config object.
        /// Validates the basic functionality with a normal positive value.
        /// </summary>
        [Fact]
        public void CollapsedPaneWidth_WithValidPositiveValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);
            double testValue = 100.5;

            // Act
            var result = mockConfig.CollapsedPaneWidth(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.FlyoutPage.CollapsedPaneWidthProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that CollapsedPaneWidth correctly handles zero value.
        /// Zero should be valid according to the property validation (>= 0).
        /// </summary>
        [Fact]
        public void CollapsedPaneWidth_WithZeroValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);
            double testValue = 0.0;

            // Act
            var result = mockConfig.CollapsedPaneWidth(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.FlyoutPage.CollapsedPaneWidthProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests CollapsedPaneWidth with various edge case double values.
        /// Validates that the method passes through all double values to SetValue without validation.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        [InlineData(-1.0)]
        [InlineData(48.0)]
        [InlineData(1.23456789)]
        public void CollapsedPaneWidth_WithEdgeCaseValues_SetsValueAndReturnsConfig(double testValue)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.CollapsedPaneWidth(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.FlyoutPage.CollapsedPaneWidthProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that CollapsedPaneWidth throws ArgumentNullException when config is null.
        /// Validates null reference handling for the extension method parameter.
        /// </summary>
        [Fact]
        public void CollapsedPaneWidth_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage> nullConfig = null;
            double testValue = 50.0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.CollapsedPaneWidth(testValue));
        }

        /// <summary>
        /// Tests that CollapsedPaneWidth propagates exceptions from SetValue.
        /// Validates that any exception thrown by the underlying SetValue call is not caught.
        /// </summary>
        [Fact]
        public void CollapsedPaneWidth_WhenSetValueThrowsException_PropagatesException()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.FlyoutPage>>();
            mockConfig.Element.Returns(mockElement);
            var expectedException = new InvalidOperationException("Test exception");
            mockElement.When(x => x.SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>()))
                      .Do(x => throw expectedException);
            double testValue = 100.0;

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => mockConfig.CollapsedPaneWidth(testValue));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that GetCollapseStyle throws NullReferenceException when element parameter is null.
        /// This verifies proper null parameter handling in the method.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetCollapseStyle_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => FlyoutPage.GetCollapseStyle(element));
        }

        /// <summary>
        /// Tests that GetCollapseStyle returns the correct CollapseStyle value when element contains valid enum values.
        /// This verifies the method correctly retrieves and casts values from the BindableObject.
        /// Expected result: Returns the specific CollapseStyle enum value that was set on the element.
        /// </summary>
        [Theory]
        [InlineData(CollapseStyle.Full)]
        [InlineData(CollapseStyle.Partial)]
        public void GetCollapseStyle_ValidElement_ReturnsCorrectCollapseStyle(CollapseStyle expectedStyle)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.CollapseStyleProperty).Returns(expectedStyle);

            // Act
            var result = FlyoutPage.GetCollapseStyle(element);

            // Assert
            Assert.Equal(expectedStyle, result);
        }

        /// <summary>
        /// Tests that GetCollapseStyle correctly handles enum values outside the defined range.
        /// This verifies the method can handle invalid enum values that might be stored.
        /// Expected result: Returns the cast enum value even if it's outside the defined range.
        /// </summary>
        [Fact]
        public void GetCollapseStyle_InvalidEnumValue_ReturnsCastValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var invalidEnumValue = (CollapseStyle)999;
            element.GetValue(FlyoutPage.CollapseStyleProperty).Returns(invalidEnumValue);

            // Act
            var result = FlyoutPage.GetCollapseStyle(element);

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that GetCollapseStyle returns the default CollapseStyle.Full when element returns the default value.
        /// This verifies the method works correctly with the property's default value.
        /// Expected result: Returns CollapseStyle.Full (the default value).
        /// </summary>
        [Fact]
        public void GetCollapseStyle_ElementReturnsDefaultValue_ReturnsCollapseStyleFull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(FlyoutPage.CollapseStyleProperty).Returns(CollapseStyle.Full);

            // Act
            var result = FlyoutPage.GetCollapseStyle(element);

            // Assert
            Assert.Equal(CollapseStyle.Full, result);
        }
    }
}