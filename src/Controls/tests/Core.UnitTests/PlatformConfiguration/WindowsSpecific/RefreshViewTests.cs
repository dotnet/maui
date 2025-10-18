#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the WindowsSpecific RefreshView static class.
    /// </summary>
    public class RefreshViewTests
    {
        /// <summary>
        /// Tests that SetRefreshPullDirection throws a NullReferenceException when 
        /// a null BindableObject element is provided, as it attempts to call SetValue on null.
        /// </summary>
        [Fact]
        public void SetRefreshPullDirection_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var direction = RefreshView.RefreshPullDirection.TopToBottom;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => RefreshView.SetRefreshPullDirection(element, direction));
        }

        /// <summary>
        /// Tests that SetRefreshPullDirection handles invalid enum values gracefully 
        /// without throwing exceptions, demonstrating robustness with out-of-range enum values.
        /// </summary>
        /// <param name="invalidEnumValue">An invalid integer value cast to the enum type.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetRefreshPullDirection_InvalidEnumValue_CallsSetValueWithoutException(int invalidEnumValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var invalidDirection = (RefreshView.RefreshPullDirection)invalidEnumValue;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => RefreshView.SetRefreshPullDirection(element, invalidDirection));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that GetRefreshPullDirection throws NullReferenceException when the element parameter is null.
        /// </summary>
        [Fact]
        public void GetRefreshPullDirection_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => RefreshView.GetRefreshPullDirection(nullElement));
        }

        /// <summary>
        /// Tests that GetRefreshPullDirection properly casts and returns enum values when BindableObject.GetValue returns boxed enum values.
        /// </summary>
        [Fact]
        public void GetRefreshPullDirection_ElementWithBoxedEnumValue_ReturnsCorrectEnumValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            object boxedEnumValue = RefreshView.RefreshPullDirection.RightToLeft;
            mockElement.GetValue(RefreshView.RefreshPullDirectionProperty).Returns(boxedEnumValue);

            // Act
            var result = RefreshView.GetRefreshPullDirection(mockElement);

            // Assert
            Assert.Equal(RefreshView.RefreshPullDirection.RightToLeft, result);
        }

        /// <summary>
        /// Tests that SetRefreshPullDirection throws NullReferenceException when config parameter is null.
        /// This test verifies proper null handling behavior for the extension method.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void SetRefreshPullDirection_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, RefreshView> config = null;
            var value = RefreshView.RefreshPullDirection.TopToBottom;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.SetRefreshPullDirection(value));
        }

        /// <summary>
        /// Tests that SetRefreshPullDirection handles invalid enum values by casting them and setting the property.
        /// This test verifies behavior with enum values outside the defined range.
        /// Expected result: Invalid enum value should be set (no validation performed) and config should be returned.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetRefreshPullDirection_InvalidEnumValues_SetsPropertyAndReturnsConfig(int invalidValue)
        {
            // Arrange
            var refreshView = new RefreshView();
            var config = Substitute.For<IPlatformElementConfiguration<Windows, RefreshView>>();
            config.Element.Returns(refreshView);
            var value = (RefreshView.RefreshPullDirection)invalidValue;

            // Act
            var result = config.SetRefreshPullDirection(value);

            // Assert
            Assert.Same(config, result);
            Assert.Equal(value, RefreshView.GetRefreshPullDirection(refreshView));
        }

        /// <summary>
        /// Tests that GetRefreshPullDirection throws NullReferenceException when config parameter is null.
        /// Input: null config parameter.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetRefreshPullDirection_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, RefreshView> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => RefreshView.GetRefreshPullDirection(config));
        }

        /// <summary>
        /// Tests that GetRefreshPullDirection throws NullReferenceException when config.Element is null.
        /// Input: valid config with null Element property.
        /// Expected: NullReferenceException is thrown when accessing Element.GetValue().
        /// </summary>
        [Fact]
        public void GetRefreshPullDirection_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, RefreshView>>();
            config.Element.Returns((RefreshView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => RefreshView.GetRefreshPullDirection(config));
        }

        /// <summary>
        /// Tests that GetRefreshPullDirection handles invalid enum values by casting them directly.
        /// Input: valid config with Element returning an invalid enum value (cast from int).
        /// Expected: returns the cast value as RefreshPullDirection enum.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void GetRefreshPullDirection_ValidConfigWithInvalidEnumValue_ReturnsCastValue(int invalidEnumValue)
        {
            // Arrange
            var mockRefreshView = Substitute.For<RefreshView>();
            mockRefreshView.GetValue(RefreshView.RefreshPullDirectionProperty).Returns((RefreshView.RefreshPullDirection)invalidEnumValue);

            var config = Substitute.For<IPlatformElementConfiguration<Windows, RefreshView>>();
            config.Element.Returns(mockRefreshView);

            // Act
            var result = RefreshView.GetRefreshPullDirection(config);

            // Assert
            Assert.Equal((RefreshView.RefreshPullDirection)invalidEnumValue, result);
        }
    }
}