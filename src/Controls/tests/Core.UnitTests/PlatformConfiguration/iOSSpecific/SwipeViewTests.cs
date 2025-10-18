#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UnitTests
{
    public class SwipeViewTests
    {
        /// <summary>
        /// Tests that SetSwipeTransitionMode throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var value = SwipeTransitionMode.Reveal;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SwipeView.SetSwipeTransitionMode(element, value));
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode calls SetValue with correct parameters for Reveal mode.
        /// Verifies that the method properly delegates to BindableObject.SetValue with the correct property and value.
        /// Expected result: SetValue is called once with SwipeTransitionModeProperty and SwipeTransitionMode.Reveal.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_ValidRevealMode_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = SwipeTransitionMode.Reveal;

            // Act
            SwipeView.SetSwipeTransitionMode(element, value);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode calls SetValue with correct parameters for Drag mode.
        /// Verifies that the method properly delegates to BindableObject.SetValue with the correct property and value.
        /// Expected result: SetValue is called once with SwipeTransitionModeProperty and SwipeTransitionMode.Drag.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_ValidDragMode_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = SwipeTransitionMode.Drag;

            // Act
            SwipeView.SetSwipeTransitionMode(element, value);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode handles invalid enum values by casting and passing them to SetValue.
        /// Verifies that the method doesn't perform enum validation and delegates responsibility to SetValue.
        /// Expected result: SetValue is called once with SwipeTransitionModeProperty and the invalid enum value.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_InvalidEnumValue_CallsSetValueWithValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var invalidValue = (SwipeTransitionMode)999;

            // Act
            SwipeView.SetSwipeTransitionMode(element, invalidValue);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, invalidValue);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode works with minimum enum value.
        /// Verifies edge case handling for the minimum defined enum value.
        /// Expected result: SetValue is called once with SwipeTransitionModeProperty and SwipeTransitionMode.Reveal (value 0).
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_MinimumEnumValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = (SwipeTransitionMode)0; // Reveal

            // Act
            SwipeView.SetSwipeTransitionMode(element, value);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode works with maximum enum value.
        /// Verifies edge case handling for the maximum defined enum value.
        /// Expected result: SetValue is called once with SwipeTransitionModeProperty and SwipeTransitionMode.Drag (value 1).
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_MaximumEnumValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = (SwipeTransitionMode)1; // Drag

            // Act
            SwipeView.SetSwipeTransitionMode(element, value);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.GetSwipeTransitionMode());
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method returns correct transition mode when config has valid element.
        /// Input: valid config with element that has SwipeTransitionMode.Reveal
        /// Expected: Returns SwipeTransitionMode.Reveal
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ValidConfigWithRevealMode_ReturnsReveal()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            var mockElement = Substitute.For<Controls.SwipeView>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.SwipeView.SwipeTransitionModeProperty).Returns(SwipeTransitionMode.Reveal);

            // Act
            var result = mockConfig.GetSwipeTransitionMode();

            // Assert
            Assert.Equal(SwipeTransitionMode.Reveal, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method returns correct transition mode when config has valid element with Drag mode.
        /// Input: valid config with element that has SwipeTransitionMode.Drag
        /// Expected: Returns SwipeTransitionMode.Drag
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ValidConfigWithDragMode_ReturnsDrag()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            var mockElement = Substitute.For<Controls.SwipeView>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.SwipeView.SwipeTransitionModeProperty).Returns(SwipeTransitionMode.Drag);

            // Act
            var result = mockConfig.GetSwipeTransitionMode();

            // Assert
            Assert.Equal(SwipeTransitionMode.Drag, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method throws ArgumentNullException when config.Element is null.
        /// Input: valid config but Element property returns null
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            mockConfig.Element.Returns((Controls.SwipeView)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.GetSwipeTransitionMode());
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode extension method correctly delegates to the static method
        /// with the Reveal enum value and returns the same config instance for method chaining.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_WithRevealValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            var mockElement = Substitute.For<Controls.SwipeView>();
            mockConfig.Element.Returns(mockElement);
            var value = SwipeTransitionMode.Reveal;

            // Act
            var result = iOSSpecific.SwipeView.SetSwipeTransitionMode(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            var _ = mockConfig.Received(1).Element;
            mockElement.Received(1).SetValue(iOSSpecific.SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode extension method correctly delegates to the static method
        /// with the Drag enum value and returns the same config instance for method chaining.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_WithDragValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            var mockElement = Substitute.For<Controls.SwipeView>();
            mockConfig.Element.Returns(mockElement);
            var value = SwipeTransitionMode.Drag;

            // Act
            var result = iOSSpecific.SwipeView.SetSwipeTransitionMode(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            var _ = mockConfig.Received(1).Element;
            mockElement.Received(1).SetValue(iOSSpecific.SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode extension method handles invalid enum values
        /// by casting and passing them through to the static method.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_WithInvalidEnumValue_PassesValueThroughToStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            var mockElement = Substitute.For<Controls.SwipeView>();
            mockConfig.Element.Returns(mockElement);
            var invalidValue = (SwipeTransitionMode)999;

            // Act
            var result = iOSSpecific.SwipeView.SetSwipeTransitionMode(mockConfig, invalidValue);

            // Assert
            Assert.Same(mockConfig, result);
            var _ = mockConfig.Received(1).Element;
            mockElement.Received(1).SetValue(iOSSpecific.SwipeView.SwipeTransitionModeProperty, invalidValue);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode extension method throws ArgumentNullException
        /// when called with a null config parameter.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView> nullConfig = null;
            var value = SwipeTransitionMode.Reveal;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                iOSSpecific.SwipeView.SetSwipeTransitionMode(nullConfig, value));
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode extension method correctly accesses the Element property
        /// and delegates the call when Element is a BindableObject.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_WithBindableObjectElement_AccessesElementAndCallsSetValue()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, Controls.SwipeView>>();
            var mockBindableObject = Substitute.For<BindableObject>();
            mockConfig.Element.Returns((Controls.SwipeView)mockBindableObject);
            var value = SwipeTransitionMode.Drag;

            // Act
            var result = iOSSpecific.SwipeView.SetSwipeTransitionMode(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            var _ = mockConfig.Received(1).Element;
            mockBindableObject.Received(1).SetValue(iOSSpecific.SwipeView.SwipeTransitionModeProperty, value);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode throws ArgumentNullException when element parameter is null.
        /// Input: null BindableObject
        /// Expected: ArgumentNullException or NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_NullElement_ThrowsException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => SwipeView.GetSwipeTransitionMode(element));
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode returns the default SwipeTransitionMode.Reveal value.
        /// Input: BindableObject with default SwipeTransitionMode value
        /// Expected: SwipeTransitionMode.Reveal is returned
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ElementWithDefaultValue_ReturnsReveal()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(SwipeView.SwipeTransitionModeProperty).Returns(SwipeTransitionMode.Reveal);

            // Act
            var result = SwipeView.GetSwipeTransitionMode(element);

            // Assert
            Assert.Equal(SwipeTransitionMode.Reveal, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode returns SwipeTransitionMode.Drag when that value is set.
        /// Input: BindableObject with SwipeTransitionMode.Drag value
        /// Expected: SwipeTransitionMode.Drag is returned
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ElementWithDragValue_ReturnsDrag()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(SwipeView.SwipeTransitionModeProperty).Returns(SwipeTransitionMode.Drag);

            // Act
            var result = SwipeView.GetSwipeTransitionMode(element);

            // Assert
            Assert.Equal(SwipeTransitionMode.Drag, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode properly handles all valid SwipeTransitionMode enum values.
        /// Input: BindableObject configured with different SwipeTransitionMode values
        /// Expected: Corresponding SwipeTransitionMode value is returned for each case
        /// </summary>
        [Theory]
        [InlineData(SwipeTransitionMode.Reveal)]
        [InlineData(SwipeTransitionMode.Drag)]
        public void GetSwipeTransitionMode_ValidEnumValues_ReturnsCorrectValue(SwipeTransitionMode expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(SwipeView.SwipeTransitionModeProperty).Returns(expectedValue);

            // Act
            var result = SwipeView.GetSwipeTransitionMode(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}