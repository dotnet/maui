#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.AndroidSpecific
{
    public partial class SwipeViewTests
    {
        /// <summary>
        /// Tests that SetSwipeTransitionMode correctly calls SetValue on the BindableObject with the specified SwipeTransitionMode value.
        /// Verifies the method passes the SwipeTransitionModeProperty and the provided value to the element's SetValue method.
        /// </summary>
        /// <param name="mode">The SwipeTransitionMode value to test</param>
        [Theory]
        [InlineData(SwipeTransitionMode.Reveal)]
        [InlineData(SwipeTransitionMode.Drag)]
        public void SetSwipeTransitionMode_ValidElement_CallsSetValueWithCorrectParameters(SwipeTransitionMode mode)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            SwipeView.SetSwipeTransitionMode(element, mode);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, mode);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode throws NullReferenceException when called with null BindableObject element.
        /// Verifies that the method does not handle null element gracefully and throws the expected exception.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var mode = SwipeTransitionMode.Reveal;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => SwipeView.SetSwipeTransitionMode(element, mode));
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode correctly handles invalid SwipeTransitionMode values that are outside the defined enum range.
        /// Verifies the method still calls SetValue even with invalid enum values, as enum validation is not enforced by the method.
        /// </summary>
        [Theory]
        [InlineData((SwipeTransitionMode)(-1))]
        [InlineData((SwipeTransitionMode)999)]
        [InlineData((SwipeTransitionMode)int.MaxValue)]
        [InlineData((SwipeTransitionMode)int.MinValue)]
        public void SetSwipeTransitionMode_InvalidEnumValue_CallsSetValueWithInvalidValue(SwipeTransitionMode invalidMode)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            SwipeView.SetSwipeTransitionMode(element, invalidMode);

            // Assert
            element.Received(1).SetValue(SwipeView.SwipeTransitionModeProperty, invalidMode);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode uses the correct SwipeTransitionModeProperty when calling SetValue.
        /// Verifies that the static property reference is passed correctly to the BindableObject's SetValue method.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_ValidElement_UsesCorrectBindableProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var mode = SwipeTransitionMode.Drag;

            // Act
            SwipeView.SetSwipeTransitionMode(element, mode);

            // Assert
            element.Received(1).SetValue(Arg.Is<BindableProperty>(p => p == SwipeView.SwipeTransitionModeProperty), mode);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter
        /// Expected: ArgumentNullException should be thrown
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ConfigIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.SwipeView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.GetSwipeTransitionMode());
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method throws ArgumentNullException when config.Element is null.
        /// Input: valid config with null Element property
        /// Expected: ArgumentNullException should be thrown from the underlying GetSwipeTransitionMode method
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ConfigElementIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.SwipeView>>();
            config.Element.Returns((Controls.SwipeView)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.GetSwipeTransitionMode());
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method returns correct SwipeTransitionMode value when valid config is provided.
        /// Input: valid config with mocked Element that returns specific SwipeTransitionMode values
        /// Expected: Returns the correct SwipeTransitionMode value from the underlying method
        /// </summary>
        [Theory]
        [InlineData(SwipeTransitionMode.Reveal)]
        [InlineData(SwipeTransitionMode.Drag)]
        public void GetSwipeTransitionMode_ValidConfig_ReturnsCorrectValue(SwipeTransitionMode expectedMode)
        {
            // Arrange
            var element = Substitute.For<Controls.SwipeView>();
            element.GetValue(AndroidSpecific.SwipeView.SwipeTransitionModeProperty).Returns(expectedMode);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.SwipeView>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetSwipeTransitionMode();

            // Assert
            Assert.Equal(expectedMode, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode extension method properly delegates to the static overload.
        /// Input: valid config with mocked Element
        /// Expected: The Element.GetValue method should be called with the correct BindableProperty
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_ValidConfig_CallsElementGetValue()
        {
            // Arrange
            var element = Substitute.For<Controls.SwipeView>();
            element.GetValue(AndroidSpecific.SwipeView.SwipeTransitionModeProperty).Returns(SwipeTransitionMode.Reveal);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.SwipeView>>();
            config.Element.Returns(element);

            // Act
            config.GetSwipeTransitionMode();

            // Assert
            element.Received(1).GetValue(AndroidSpecific.SwipeView.SwipeTransitionModeProperty);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode with valid config and Reveal mode returns the same config object
        /// and calls the underlying static method without throwing exceptions.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_ValidConfigWithRevealMode_ReturnsSameConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.SwipeView>>();
            var mockSwipeView = Substitute.For<Microsoft.Maui.Controls.SwipeView>();
            mockConfig.Element.Returns(mockSwipeView);
            var transitionMode = SwipeTransitionMode.Reveal;

            // Act
            var result = mockConfig.SetSwipeTransitionMode(transitionMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode with valid config and Drag mode returns the same config object
        /// and calls the underlying static method without throwing exceptions.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_ValidConfigWithDragMode_ReturnsSameConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.SwipeView>>();
            var mockSwipeView = Substitute.For<Microsoft.Maui.Controls.SwipeView>();
            mockConfig.Element.Returns(mockSwipeView);
            var transitionMode = SwipeTransitionMode.Drag;

            // Act
            var result = mockConfig.SetSwipeTransitionMode(transitionMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode with null config parameter throws NullReferenceException
        /// when attempting to access the Element property.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.SwipeView> nullConfig = null;
            var transitionMode = SwipeTransitionMode.Reveal;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.SetSwipeTransitionMode(transitionMode));
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode with an invalid enum value (cast from int) returns the same config object.
        /// The underlying method should handle any validation of the enum value.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_InvalidEnumValue_ReturnsSameConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.SwipeView>>();
            var mockSwipeView = Substitute.For<Microsoft.Maui.Controls.SwipeView>();
            mockConfig.Element.Returns(mockSwipeView);
            var invalidTransitionMode = (SwipeTransitionMode)999;

            // Act
            var result = mockConfig.SetSwipeTransitionMode(invalidTransitionMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode with config having null Element property throws NullReferenceException
        /// when the underlying static method attempts to use the null Element.
        /// </summary>
        [Fact]
        public void SetSwipeTransitionMode_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.SwipeView>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.SwipeView)null);
            var transitionMode = SwipeTransitionMode.Reveal;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.SetSwipeTransitionMode(transitionMode));
        }

        /// <summary>
        /// Tests that SetSwipeTransitionMode with boundary enum values (minimum and maximum valid values)
        /// returns the same config object without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(SwipeTransitionMode.Reveal)]
        [InlineData(SwipeTransitionMode.Drag)]
        [InlineData((SwipeTransitionMode)0)]
        [InlineData((SwipeTransitionMode)1)]
        public void SetSwipeTransitionMode_BoundaryEnumValues_ReturnsSameConfig(SwipeTransitionMode transitionMode)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Microsoft.Maui.Controls.SwipeView>>();
            var mockSwipeView = Substitute.For<Microsoft.Maui.Controls.SwipeView>();
            mockConfig.Element.Returns(mockSwipeView);

            // Act
            var result = mockConfig.SetSwipeTransitionMode(transitionMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode returns the correct SwipeTransitionMode value
        /// when called with a valid BindableObject that has the property set to different enum values.
        /// </summary>
        /// <param name="expectedMode">The SwipeTransitionMode value to test.</param>
        [Theory]
        [InlineData(SwipeTransitionMode.Reveal)]
        [InlineData(SwipeTransitionMode.Drag)]
        public void GetSwipeTransitionMode_WithValidElement_ReturnsCorrectValue(SwipeTransitionMode expectedMode)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(SwipeView.SwipeTransitionModeProperty).Returns(expectedMode);

            // Act
            var result = SwipeView.GetSwipeTransitionMode(mockElement);

            // Assert
            Assert.Equal(expectedMode, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode throws an ArgumentNullException
        /// when called with a null BindableObject parameter.
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SwipeView.GetSwipeTransitionMode(element));
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode returns the default value (Reveal)
        /// when called with a BindableObject that returns the default property value.
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_WithDefaultValue_ReturnsReveal()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(SwipeView.SwipeTransitionModeProperty).Returns(SwipeTransitionMode.Reveal);

            // Act
            var result = SwipeView.GetSwipeTransitionMode(mockElement);

            // Assert
            Assert.Equal(SwipeTransitionMode.Reveal, result);
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode throws an InvalidCastException
        /// when the GetValue method returns a value that cannot be cast to SwipeTransitionMode.
        /// </summary>
        [Fact]
        public void GetSwipeTransitionMode_WithInvalidCastValue_ThrowsInvalidCastException()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(SwipeView.SwipeTransitionModeProperty).Returns("InvalidValue");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => SwipeView.GetSwipeTransitionMode(mockElement));
        }

        /// <summary>
        /// Tests that GetSwipeTransitionMode handles enum values cast from integers,
        /// including values outside the defined enum range.
        /// </summary>
        /// <param name="intValue">The integer value to cast to SwipeTransitionMode.</param>
        /// <param name="expectedMode">The expected SwipeTransitionMode result.</param>
        [Theory]
        [InlineData(0, SwipeTransitionMode.Reveal)]
        [InlineData(1, SwipeTransitionMode.Drag)]
        [InlineData(99, (SwipeTransitionMode)99)] // Invalid enum value
        public void GetSwipeTransitionMode_WithIntegerValues_ReturnsCorrectEnum(int intValue, SwipeTransitionMode expectedMode)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(SwipeView.SwipeTransitionModeProperty).Returns(intValue);

            // Act
            var result = SwipeView.GetSwipeTransitionMode(mockElement);

            // Assert
            Assert.Equal(expectedMode, result);
        }
    }
}