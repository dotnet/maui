#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.UnitTests
{
    /// <summary>
    /// Unit tests for the ItemsView static class in TizenSpecific platform configuration.
    /// </summary>
    public class ItemsViewTests
    {
        /// <summary>
        /// Tests that SetFocusedItemScrollPosition throws NullReferenceException when element parameter is null.
        /// Verifies that the method properly validates the element parameter and throws the expected exception.
        /// </summary>
        [Fact]
        public void SetFocusedItemScrollPosition_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var position = ScrollToPosition.MakeVisible;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ItemsView.SetFocusedItemScrollPosition(element, position));
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition successfully calls SetValue with the correct parameters for valid ScrollToPosition enum values.
        /// Verifies that the method properly forwards the position parameter to the BindableObject's SetValue method.
        /// </summary>
        /// <param name="position">The ScrollToPosition enum value to test.</param>
        [Theory]
        [InlineData(ScrollToPosition.MakeVisible)]
        [InlineData(ScrollToPosition.Start)]
        [InlineData(ScrollToPosition.Center)]
        [InlineData(ScrollToPosition.End)]
        public void SetFocusedItemScrollPosition_ValidPosition_CallsSetValueWithCorrectParameters(ScrollToPosition position)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ItemsView.SetFocusedItemScrollPosition(element, position);

            // Assert
            element.Received(1).SetValue(ItemsView.FocusedItemScrollPositionProperty, position);
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition handles invalid ScrollToPosition enum values by passing them through to SetValue.
        /// Verifies that the method does not perform enum validation and delegates behavior to the underlying SetValue method.
        /// </summary>
        /// <param name="invalidPosition">An invalid ScrollToPosition enum value created by casting an integer outside the defined range.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetFocusedItemScrollPosition_InvalidPosition_CallsSetValueWithInvalidEnum(int invalidPosition)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var position = (ScrollToPosition)invalidPosition;

            // Act
            ItemsView.SetFocusedItemScrollPosition(element, position);

            // Assert
            element.Received(1).SetValue(ItemsView.FocusedItemScrollPositionProperty, position);
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.ItemsView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ItemsView.GetFocusedItemScrollPosition(config));
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition returns MakeVisible when Element has MakeVisible scroll position.
        /// Input: Valid config with Element returning ScrollToPosition.MakeVisible.
        /// Expected: ScrollToPosition.MakeVisible is returned.
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_ValidConfigWithMakeVisible_ReturnsMakeVisible()
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ItemsView>();
            mockElement.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns(ScrollToPosition.MakeVisible);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.ItemsView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(mockConfig);

            // Assert
            Assert.Equal(ScrollToPosition.MakeVisible, result);
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition returns Start when Element has Start scroll position.
        /// Input: Valid config with Element returning ScrollToPosition.Start.
        /// Expected: ScrollToPosition.Start is returned.
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_ValidConfigWithStart_ReturnsStart()
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ItemsView>();
            mockElement.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns(ScrollToPosition.Start);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.ItemsView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(mockConfig);

            // Assert
            Assert.Equal(ScrollToPosition.Start, result);
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition returns Center when Element has Center scroll position.
        /// Input: Valid config with Element returning ScrollToPosition.Center.
        /// Expected: ScrollToPosition.Center is returned.
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_ValidConfigWithCenter_ReturnsCenter()
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ItemsView>();
            mockElement.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns(ScrollToPosition.Center);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.ItemsView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(mockConfig);

            // Assert
            Assert.Equal(ScrollToPosition.Center, result);
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition returns End when Element has End scroll position.
        /// Input: Valid config with Element returning ScrollToPosition.End.
        /// Expected: ScrollToPosition.End is returned.
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_ValidConfigWithEnd_ReturnsEnd()
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ItemsView>();
            mockElement.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns(ScrollToPosition.End);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.ItemsView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(mockConfig);

            // Assert
            Assert.Equal(ScrollToPosition.End, result);
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition handles boundary enum values correctly.
        /// Input: Valid config with Element returning cast boundary enum values.
        /// Expected: Boundary enum values are returned correctly.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(999)]
        public void GetFocusedItemScrollPosition_ValidConfigWithBoundaryEnumValues_ReturnsBoundaryValues(int enumValue)
        {
            // Arrange
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ItemsView>();
            mockElement.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns((ScrollToPosition)enumValue);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.ItemsView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(mockConfig);

            // Assert
            Assert.Equal((ScrollToPosition)enumValue, result);
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition extension method returns the same config object
        /// when called with valid parameters.
        /// </summary>
        [Fact]
        public void SetFocusedItemScrollPosition_ValidConfig_ReturnsSameConfigObject()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, ItemsView>>();
            var mockElement = Substitute.For<ItemsView>();
            mockConfig.Element.Returns(mockElement);
            var position = ScrollToPosition.MakeVisible;

            // Act
            var result = mockConfig.SetFocusedItemScrollPosition(position);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition extension method works correctly with all valid ScrollToPosition enum values.
        /// </summary>
        /// <param name="position">The ScrollToPosition enum value to test.</param>
        [Theory]
        [InlineData(ScrollToPosition.MakeVisible)]
        [InlineData(ScrollToPosition.Start)]
        [InlineData(ScrollToPosition.Center)]
        [InlineData(ScrollToPosition.End)]
        public void SetFocusedItemScrollPosition_AllValidEnumValues_CompletesSuccessfully(ScrollToPosition position)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, ItemsView>>();
            var mockElement = Substitute.For<ItemsView>();
            mockConfig.Element.Returns(mockElement);

            // Act & Assert
            var exception = Record.Exception(() => mockConfig.SetFocusedItemScrollPosition(position));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition extension method throws ArgumentNullException
        /// when called with null config parameter.
        /// </summary>
        [Fact]
        public void SetFocusedItemScrollPosition_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, ItemsView> nullConfig = null;
            var position = ScrollToPosition.MakeVisible;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.SetFocusedItemScrollPosition(position));
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition extension method handles invalid enum values
        /// without throwing exceptions, allowing the underlying method to handle validation.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        public void SetFocusedItemScrollPosition_InvalidEnumValue_CompletesSuccessfully(int invalidValue)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, ItemsView>>();
            var mockElement = Substitute.For<ItemsView>();
            mockConfig.Element.Returns(mockElement);
            var invalidPosition = (ScrollToPosition)invalidValue;

            // Act & Assert
            var exception = Record.Exception(() => mockConfig.SetFocusedItemScrollPosition(invalidPosition));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SetFocusedItemScrollPosition extension method throws NullReferenceException
        /// when config.Element returns null.
        /// </summary>
        [Fact]
        public void SetFocusedItemScrollPosition_ConfigElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, ItemsView>>();
            mockConfig.Element.Returns((ItemsView)null);
            var position = ScrollToPosition.MakeVisible;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.SetFocusedItemScrollPosition(position));
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition throws NullReferenceException when element parameter is null.
        /// Input: null element
        /// Expected: NullReferenceException
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ItemsView.GetFocusedItemScrollPosition(element));
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition returns the correct ScrollToPosition value for valid BindableObject elements.
        /// Input: Valid BindableObject with different ScrollToPosition values
        /// Expected: Corresponding ScrollToPosition enum value
        /// </summary>
        [Theory]
        [InlineData(ScrollToPosition.MakeVisible)]
        [InlineData(ScrollToPosition.Start)]
        [InlineData(ScrollToPosition.Center)]
        [InlineData(ScrollToPosition.End)]
        public void GetFocusedItemScrollPosition_ValidElement_ReturnsCorrectScrollToPosition(ScrollToPosition expectedPosition)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns(expectedPosition);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(element);

            // Assert
            Assert.Equal(expectedPosition, result);
        }

        /// <summary>
        /// Tests that GetFocusedItemScrollPosition returns the default value when element has default property value.
        /// Input: Valid BindableObject with default ScrollToPosition.MakeVisible value
        /// Expected: ScrollToPosition.MakeVisible
        /// </summary>
        [Fact]
        public void GetFocusedItemScrollPosition_ElementWithDefaultValue_ReturnsMakeVisible()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ItemsView.FocusedItemScrollPositionProperty).Returns(ScrollToPosition.MakeVisible);

            // Act
            var result = ItemsView.GetFocusedItemScrollPosition(element);

            // Assert
            Assert.Equal(ScrollToPosition.MakeVisible, result);
        }
    }
}