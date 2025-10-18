#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class VisualElementLoadedTests : BaseTestFixture
    {
    }

    public class ShadowEffectTests
    {
        /// <summary>
        /// Tests that the ShadowEffect constructor creates a valid instance and properly initializes the base RoutingEffect.
        /// Input conditions: Default parameterless constructor call.
        /// Expected result: Valid ShadowEffect instance that inherits from RoutingEffect.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Arrange & Act
            var shadowEffect = new ShadowEffect();

            // Assert
            Assert.NotNull(shadowEffect);
            Assert.IsType<ShadowEffect>(shadowEffect);
            Assert.IsAssignableFrom<RoutingEffect>(shadowEffect);
        }

        /// <summary>
        /// Tests that the ShadowEffect constructor can be called multiple times to create distinct instances.
        /// Input conditions: Multiple constructor calls.
        /// Expected result: Each call creates a new, distinct instance.
        /// </summary>
        [Fact]
        public void Constructor_MultipleCalls_CreatesDistinctInstances()
        {
            // Arrange & Act
            var shadowEffect1 = new ShadowEffect();
            var shadowEffect2 = new ShadowEffect();

            // Assert
            Assert.NotNull(shadowEffect1);
            Assert.NotNull(shadowEffect2);
            Assert.NotSame(shadowEffect1, shadowEffect2);
        }
    }


    /// <summary>
    /// Tests for the VisualElement.GetShadowRadius method in the iOS platform configuration.
    /// </summary>
    public class VisualElementGetShadowRadiusTests
    {
        /// <summary>
        /// Tests that GetShadowRadius throws ArgumentNullException when element parameter is null.
        /// Input: null element
        /// Expected: ArgumentNullException should be thrown
        /// </summary>
        [Fact]
        public void GetShadowRadius_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.GetShadowRadius(element));
        }

        /// <summary>
        /// Tests that GetShadowRadius returns the correct shadow radius value from the bindable object.
        /// Input: Various shadow radius values including edge cases
        /// Expected: Returns the exact shadow radius value that was set on the element
        /// </summary>
        [Theory]
        [InlineData(10.0)] // Default value
        [InlineData(0.0)] // Zero
        [InlineData(-5.0)] // Negative value
        [InlineData(100.5)] // Positive decimal
        [InlineData(double.MaxValue)] // Maximum double value
        [InlineData(double.MinValue)] // Minimum double value
        [InlineData(double.PositiveInfinity)] // Positive infinity
        [InlineData(double.NegativeInfinity)] // Negative infinity
        [InlineData(double.NaN)] // Not a number
        public void GetShadowRadius_ValidElement_ReturnsExpectedValue(double expectedRadius)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(VisualElement.ShadowRadiusProperty).Returns(expectedRadius);

            // Act
            var result = VisualElement.GetShadowRadius(element);

            // Assert
            if (double.IsNaN(expectedRadius))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(expectedRadius, result);
            }
        }

        /// <summary>
        /// Tests that GetShadowRadius correctly calls GetValue with the ShadowRadiusProperty.
        /// Input: Valid bindable object
        /// Expected: GetValue method is called exactly once with the correct property
        /// </summary>
        [Fact]
        public void GetShadowRadius_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedValue = 15.0;
            element.GetValue(VisualElement.ShadowRadiusProperty).Returns(expectedValue);

            // Act
            var result = VisualElement.GetShadowRadius(element);

            // Assert
            element.Received(1).GetValue(VisualElement.ShadowRadiusProperty);
            Assert.Equal(expectedValue, result);
        }
    }


    /// <summary>
    /// Unit tests for the SetCanBecomeFirstResponder method in iOS-specific VisualElement configuration.
    /// </summary>
    public class VisualElementSetCanBecomeFirstResponderTests
    {
        /// <summary>
        /// Tests that SetCanBecomeFirstResponder throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetCanBecomeFirstResponder_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                VisualElement.SetCanBecomeFirstResponder(nullElement, value));
        }

        /// <summary>
        /// Tests that SetCanBecomeFirstResponder successfully sets the property to true when given a valid element.
        /// </summary>
        [Fact]
        public void SetCanBecomeFirstResponder_ValidElementWithTrue_SetsPropertyToTrue()
        {
            // Arrange
            var element = new Label();
            bool expectedValue = true;

            // Act
            VisualElement.SetCanBecomeFirstResponder(element, expectedValue);

            // Assert
            bool actualValue = VisualElement.GetCanBecomeFirstResponder(element);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that SetCanBecomeFirstResponder successfully sets the property to false when given a valid element.
        /// </summary>
        [Fact]
        public void SetCanBecomeFirstResponder_ValidElementWithFalse_SetsPropertyToFalse()
        {
            // Arrange
            var element = new Label();
            bool expectedValue = false;

            // Act
            VisualElement.SetCanBecomeFirstResponder(element, expectedValue);

            // Assert
            bool actualValue = VisualElement.GetCanBecomeFirstResponder(element);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that SetCanBecomeFirstResponder can toggle the property value correctly on the same element.
        /// </summary>
        [Fact]
        public void SetCanBecomeFirstResponder_ToggleValue_UpdatesPropertyCorrectly()
        {
            // Arrange
            var element = new Label();

            // Act & Assert - Set to true
            VisualElement.SetCanBecomeFirstResponder(element, true);
            Assert.True(VisualElement.GetCanBecomeFirstResponder(element));

            // Act & Assert - Set to false
            VisualElement.SetCanBecomeFirstResponder(element, false);
            Assert.False(VisualElement.GetCanBecomeFirstResponder(element));
        }

        /// <summary>
        /// Tests that SetCanBecomeFirstResponder works with different types of BindableObject elements.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetCanBecomeFirstResponder_DifferentBindableObjectTypes_SetsPropertyCorrectly(bool value)
        {
            // Arrange
            var button = new Button();
            var label = new Label();
            var entry = new Entry();

            // Act
            VisualElement.SetCanBecomeFirstResponder(button, value);
            VisualElement.SetCanBecomeFirstResponder(label, value);
            VisualElement.SetCanBecomeFirstResponder(entry, value);

            // Assert
            Assert.Equal(value, VisualElement.GetCanBecomeFirstResponder(button));
            Assert.Equal(value, VisualElement.GetCanBecomeFirstResponder(label));
            Assert.Equal(value, VisualElement.GetCanBecomeFirstResponder(entry));
        }

        /// <summary>
        /// Tests that SetCanBecomeFirstResponder sets the property value and returns the config object for fluent chaining.
        /// Tests both true and false values to ensure the method works correctly with different boolean inputs.
        /// </summary>
        /// <param name="value">The boolean value to set for CanBecomeFirstResponder property.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetCanBecomeFirstResponder_WithBooleanValue_SetsPropertyAndReturnsConfig(bool value)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.SetCanBecomeFirstResponder(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement.CanBecomeFirstResponderProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetCanBecomeFirstResponder throws NullReferenceException when the Element property returns null.
        /// This verifies proper error handling for invalid configuration objects.
        /// </summary>
        [Fact]
        public void SetCanBecomeFirstResponder_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.SetCanBecomeFirstResponder(true));
        }
    }


    /// <summary>
    /// Unit tests for the GetShadowOpacity method in VisualElement iOS platform configuration.
    /// </summary>
    public partial class VisualElementiOSGetShadowOpacityTests
    {
        /// <summary>
        /// Tests that GetShadowOpacity returns the default shadow opacity value when no value has been explicitly set.
        /// Input: A BindableObject with default shadow opacity.
        /// Expected: Returns the default shadow opacity value of 0.5.
        /// </summary>
        [Fact]
        public void GetShadowOpacity_DefaultValue_ReturnsDefaultOpacity()
        {
            // Arrange
            var element = new Label();

            // Act
            var result = VisualElement.GetShadowOpacity(element);

            // Assert
            Assert.Equal(0.5, result);
        }

        /// <summary>
        /// Tests that GetShadowOpacity returns the correct value when a custom shadow opacity has been set.
        /// Input: A BindableObject with a custom shadow opacity value.
        /// Expected: Returns the custom shadow opacity value.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(0.25)]
        [InlineData(0.75)]
        [InlineData(1.0)]
        [InlineData(2.5)]
        public void GetShadowOpacity_CustomValue_ReturnsCustomOpacity(double customOpacity)
        {
            // Arrange
            var element = new Label();
            VisualElement.SetShadowOpacity(element, customOpacity);

            // Act
            var result = VisualElement.GetShadowOpacity(element);

            // Assert
            Assert.Equal(customOpacity, result);
        }

        /// <summary>
        /// Tests that GetShadowOpacity handles extreme double values correctly.
        /// Input: A BindableObject with extreme double values for shadow opacity.
        /// Expected: Returns the extreme double values without error.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void GetShadowOpacity_ExtremeValues_ReturnsExtremeValues(double extremeValue)
        {
            // Arrange
            var element = new Label();
            VisualElement.SetShadowOpacity(element, extremeValue);

            // Act
            var result = VisualElement.GetShadowOpacity(element);

            // Assert
            if (double.IsNaN(extremeValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(extremeValue, result);
            }
        }

        /// <summary>
        /// Tests that GetShadowOpacity throws ArgumentNullException when passed a null element.
        /// Input: null BindableObject parameter.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetShadowOpacity_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.GetShadowOpacity(element));
        }

    }


    /// <summary>
    /// Unit tests for the SetShadowColor method in the iOS-specific VisualElement platform configuration.
    /// </summary>
    public partial class VisualElementSetShadowColorTests
    {
        /// <summary>
        /// Tests that SetShadowColor throws ArgumentNullException when element parameter is null.
        /// Validates that the method properly validates the required element parameter.
        /// </summary>
        [Fact]
        public void SetShadowColor_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullElement = null;
            var color = Colors.Red;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(nullElement, color));
        }

        /// <summary>
        /// Tests SetShadowColor with boundary and edge case color values.
        /// Validates that the method handles extreme color component values correctly.
        /// </summary>
        /// <param name="red">Red component value</param>
        /// <param name="green">Green component value</param>
        /// <param name="blue">Blue component value</param>
        /// <param name="alpha">Alpha component value</param>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Completely transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Opaque white
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Opaque black
        [InlineData(1.0f, 0.0f, 0.0f, 0.5f)] // Semi-transparent red
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(float.MinValue, float.MinValue, float.MinValue, float.MinValue)] // Extreme minimum values
        [InlineData(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue)] // Extreme maximum values
        public void SetShadowColor_BoundaryColorValues_SetsPropertySuccessfully(float red, float green, float blue, float alpha)
        {
            // Arrange
            var element = new Controls.VisualElement();
            var color = new Color(red, green, blue, alpha);

            // Act
            PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(element, color);

            // Assert
            var actualColor = PlatformConfiguration.iOSSpecific.VisualElement.GetShadowColor(element);
            Assert.Equal(color, actualColor);
        }

        /// <summary>
        /// Tests SetShadowColor with default Color value.
        /// Validates that the method handles the default/empty Color struct correctly.
        /// </summary>
        [Fact]
        public void SetShadowColor_DefaultColor_SetsPropertySuccessfully()
        {
            // Arrange
            var element = new Controls.VisualElement();
            var defaultColor = default(Color);

            // Act
            PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(element, defaultColor);

            // Assert
            var actualColor = PlatformConfiguration.iOSSpecific.VisualElement.GetShadowColor(element);
            Assert.Equal(defaultColor, actualColor);
        }

        /// <summary>
        /// Tests that SetShadowColor can overwrite previously set color values.
        /// Validates that multiple calls to SetShadowColor update the property correctly.
        /// </summary>
        [Fact]
        public void SetShadowColor_MultipleSetOperations_OverwritesPreviousValue()
        {
            // Arrange
            var element = new Controls.VisualElement();
            var initialColor = Colors.Red;
            var newColor = Colors.Green;

            // Act
            PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(element, initialColor);
            var intermediateColor = PlatformConfiguration.iOSSpecific.VisualElement.GetShadowColor(element);

            PlatformConfiguration.iOSSpecific.VisualElement.SetShadowColor(element, newColor);
            var finalColor = PlatformConfiguration.iOSSpecific.VisualElement.GetShadowColor(element);

            // Assert
            Assert.Equal(initialColor, intermediateColor);
            Assert.Equal(newColor, finalColor);
            Assert.NotEqual(initialColor, finalColor);
        }

    }
}