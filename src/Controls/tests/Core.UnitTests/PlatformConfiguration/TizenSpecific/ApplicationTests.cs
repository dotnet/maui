#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.TizenSpecific
{
    public class ApplicationTests
    {
        /// <summary>
        /// Tests that GetUseBezelInteraction throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetUseBezelInteraction_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetUseBezelInteraction(element));
        }

        /// <summary>
        /// Tests that GetUseBezelInteraction returns true when the underlying bindable property value is true.
        /// Verifies correct retrieval and casting of true boolean value from bindable property.
        /// Expected result: Returns true.
        /// </summary>
        [Fact]
        public void GetUseBezelInteraction_ElementReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty).Returns(true);

            // Act
            bool result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetUseBezelInteraction(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetUseBezelInteraction returns false when the underlying bindable property value is false.
        /// Verifies correct retrieval and casting of false boolean value from bindable property.
        /// Expected result: Returns false.
        /// </summary>
        [Fact]
        public void GetUseBezelInteraction_ElementReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty).Returns(false);

            // Act
            bool result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetUseBezelInteraction(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetUseBezelInteraction throws InvalidCastException when the bindable property value cannot be cast to bool.
        /// Verifies proper handling of invalid type casting scenarios.
        /// Expected result: InvalidCastException is thrown.
        /// </summary>
        [Fact]
        public void GetUseBezelInteraction_ElementReturnsNonBoolValue_ThrowsInvalidCastException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty).Returns("not a bool");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetUseBezelInteraction(element));
        }

        /// <summary>
        /// Tests that GetUseBezelInteraction throws NullReferenceException when the bindable property returns null.
        /// Verifies behavior when attempting to cast null to bool.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetUseBezelInteraction_ElementReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty).Returns(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetUseBezelInteraction(element));
        }

        /// <summary>
        /// Tests that SetUseBezelInteraction extension method calls the static method with correct parameters and returns the config object when value is true.
        /// Input: Valid config with mocked element, value = true.
        /// Expected: Static method called with element and true, same config returned.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_ValidConfigWithTrueValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns(mockElement);
            bool value = true;

            // Act
            var result = mockConfig.SetUseBezelInteraction(value);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.Application.UseBezelInteractionProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUseBezelInteraction extension method calls the static method with correct parameters and returns the config object when value is false.
        /// Input: Valid config with mocked element, value = false.
        /// Expected: Static method called with element and false, same config returned.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_ValidConfigWithFalseValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns(mockElement);
            bool value = false;

            // Act
            var result = mockConfig.SetUseBezelInteraction(value);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.Application.UseBezelInteractionProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUseBezelInteraction extension method throws NullReferenceException when config is null.
        /// Input: config = null, value = true.
        /// Expected: NullReferenceException thrown when accessing config.Element.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application> config = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.SetUseBezelInteraction(value));
        }

        /// <summary>
        /// Tests that SetUseBezelInteraction extension method throws NullReferenceException when config.Element is null.
        /// Input: Valid config with Element = null, value = true.
        /// Expected: NullReferenceException thrown when trying to call SetValue on null element.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_ConfigElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.Application)null);
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.SetUseBezelInteraction(value));
        }

        /// <summary>
        /// Tests that SetOverlayContent calls SetValue on the provided BindableObject with the correct property and value when both parameters are valid.
        /// Input: Valid BindableObject and valid View.
        /// Expected: SetValue is called with OverlayContentProperty and the provided view.
        /// </summary>
        [Fact]
        public void SetOverlayContent_ValidApplicationAndValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockApplication = Substitute.For<BindableObject>();
            var view = Substitute.For<View>();

            // Act
            Application.SetOverlayContent(mockApplication, view);

            // Assert
            mockApplication.Received(1).SetValue(Application.OverlayContentProperty, view);
        }

        /// <summary>
        /// Tests that SetOverlayContent calls SetValue with null value when the value parameter is null.
        /// Input: Valid BindableObject and null View.
        /// Expected: SetValue is called with OverlayContentProperty and null value.
        /// </summary>
        [Fact]
        public void SetOverlayContent_ValidApplicationAndNullValue_CallsSetValueWithNull()
        {
            // Arrange
            var mockApplication = Substitute.For<BindableObject>();

            // Act
            Application.SetOverlayContent(mockApplication, null);

            // Assert
            mockApplication.Received(1).SetValue(Application.OverlayContentProperty, null);
        }

        /// <summary>
        /// Tests that SetOverlayContent throws NullReferenceException when the application parameter is null.
        /// Input: Null BindableObject and valid View.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetOverlayContent_NullApplication_ThrowsNullReferenceException()
        {
            // Arrange
            var view = Substitute.For<View>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Application.SetOverlayContent(null, view));
        }

        /// <summary>
        /// Tests that SetOverlayContent throws NullReferenceException when both parameters are null.
        /// Input: Null BindableObject and null View.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetOverlayContent_BothParametersNull_ThrowsNullReferenceException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Application.SetOverlayContent(null, null));
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement throws NullReferenceException when application parameter is null.
        /// Input: null BindableObject
        /// Expected: NullReferenceException thrown
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_NullApplication_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullApplication = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(nullApplication));
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement returns null when no value is set on the application.
        /// Input: Valid BindableObject with no ActiveBezelInteractionElement value set
        /// Expected: Returns null (default value)
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_NoValueSet_ReturnsNull()
        {
            // Arrange
            var mockApplication = Substitute.For<BindableObject>();
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey.BindableProperty)
                .Returns(null);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(mockApplication);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement returns the correct Element when a value is set on the application.
        /// Input: Valid BindableObject with ActiveBezelInteractionElement value set
        /// Expected: Returns the set Element value
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_ValueSet_ReturnsElement()
        {
            // Arrange
            var mockApplication = Substitute.For<BindableObject>();
            var mockElement = Substitute.For<Element>();
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey.BindableProperty)
                .Returns(mockElement);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(mockApplication);

            // Assert
            Assert.Equal(mockElement, result);
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation for the extension method.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.Application> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(config));
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement returns the result from the underlying method when config is valid.
        /// Verifies that the extension method properly delegates to the base implementation using config.Element.
        /// Expected result: Returns the Element from the underlying GetActiveBezelInteractionElement method.
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_ValidConfig_ReturnsElementFromUnderlyingMethod()
        {
            // Arrange
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var expectedElement = Substitute.For<Element>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.Application>>();

            mockConfig.Element.Returns(mockApplication);

            // Set up the underlying method behavior by setting the property value directly
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey.BindableProperty)
                .Returns(expectedElement);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(mockConfig);

            // Assert
            Assert.Equal(expectedElement, result);
            mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement handles null Element property gracefully.
        /// Verifies behavior when config.Element is null, which should delegate to the underlying method.
        /// Expected result: ArgumentNullException is thrown by the underlying method.
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.Application>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.Application)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(mockConfig));
        }

        /// <summary>
        /// Tests that GetActiveBezelInteractionElement returns null when the underlying property value is null.
        /// Verifies that the method properly returns null when no active bezel interaction element is set.
        /// Expected result: Returns null.
        /// </summary>
        [Fact]
        public void GetActiveBezelInteractionElement_UnderlyingPropertyReturnsNull_ReturnsNull()
        {
            // Arrange
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.Application>>();

            mockConfig.Element.Returns(mockApplication);
            mockApplication.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey.BindableProperty)
                .Returns(null);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.GetActiveBezelInteractionElement(mockConfig);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement correctly calls SetValue on the provided BindableObject
        /// with the ActiveBezelInteractionElementPropertyKey and the provided Element value.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_ValidParameters_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockApplication = Substitute.For<BindableObject>();
            var mockElement = Substitute.For<Element>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(mockApplication, mockElement);

            // Assert
            mockApplication.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey,
                mockElement);
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement correctly handles null Element value
        /// by passing null to SetValue method.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_NullElementValue_CallsSetValueWithNull()
        {
            // Arrange
            var mockApplication = Substitute.For<BindableObject>();
            Element nullElement = null;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(mockApplication, nullElement);

            // Assert
            mockApplication.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey,
                nullElement);
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement throws ArgumentNullException
        /// when the application parameter is null.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_NullApplication_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullApplication = null;
            var mockElement = Substitute.For<Element>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(nullApplication, mockElement));
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement throws ArgumentNullException
        /// when both application and element parameters are null.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_BothParametersNull_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullApplication = null;
            Element nullElement = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(nullApplication, nullElement));
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement extension method throws ArgumentNullException when config parameter is null.
        /// This verifies null parameter validation and ensures proper exception handling.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application> config = null;
            var element = Substitute.For<Element>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(config, element));
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement extension method works correctly with valid config and null element value.
        /// This verifies that null element values are properly handled as they are valid inputs.
        /// Expected result: Method should execute successfully and return the original config.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_ValidConfigNullElement_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application>>();
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            config.Element.Returns(mockApplication);
            Element element = null;

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(config, element);

            // Assert
            Assert.Same(config, result);
            mockApplication.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey, element);
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement extension method works correctly with valid config and valid element.
        /// This verifies normal operation with valid parameters and proper method chaining.
        /// Expected result: Static method should be called with correct parameters and original config should be returned.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_ValidConfigValidElement_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application>>();
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var element = Substitute.For<Element>();
            config.Element.Returns(mockApplication);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(config, element);

            // Assert
            Assert.Same(config, result);
            mockApplication.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.ActiveBezelInteractionElementPropertyKey, element);
        }

        /// <summary>
        /// Tests that SetActiveBezelInteractionElement extension method properly accesses the Element property of the config.
        /// This verifies that the method correctly delegates to the static method using config.Element.
        /// Expected result: The Element property should be accessed exactly once.
        /// </summary>
        [Fact]
        public void SetActiveBezelInteractionElement_ValidConfig_AccessesElementProperty()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.Application>>();
            var mockApplication = Substitute.For<Microsoft.Maui.Controls.Application>();
            var element = Substitute.For<Element>();
            config.Element.Returns(mockApplication);

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetActiveBezelInteractionElement(config, element);

            // Assert
            var elementAccessed = config.Received(1).Element;
        }

        /// <summary>
        /// Tests SetUseBezelInteraction with a null element parameter.
        /// Verifies that the method throws a NullReferenceException when element is null.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetUseBezelInteraction(element, value));
        }

        /// <summary>
        /// Tests SetUseBezelInteraction with valid element and true value.
        /// Verifies that SetValue is called with UseBezelInteractionProperty and true value.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_ValidElementTrueValue_CallsSetValueWithTrueValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetUseBezelInteraction(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty,
                value);
        }

        /// <summary>
        /// Tests SetUseBezelInteraction with valid element and false value.
        /// Verifies that SetValue is called with UseBezelInteractionProperty and false value.
        /// </summary>
        [Fact]
        public void SetUseBezelInteraction_ValidElementFalseValue_CallsSetValueWithFalseValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetUseBezelInteraction(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty,
                value);
        }

        /// <summary>
        /// Tests SetUseBezelInteraction with both true and false values using parameterized test.
        /// Verifies that SetValue is called with correct parameters for both boolean values.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetUseBezelInteraction_ValidElement_CallsSetValueWithCorrectParameters(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.SetUseBezelInteraction(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application.UseBezelInteractionProperty,
                value);
        }
    }
}