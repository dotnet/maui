#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.TizenSpecific
{
    public partial class ScrollViewTests
    {
        /// <summary>
        /// Tests that SetHorizontalScrollStep calls SetValue on the element with the correct property and scrollStep value.
        /// </summary>
        /// <param name="scrollStep">The horizontal scroll step value to set.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetHorizontalScrollStep_WithValidElement_CallsSetValueWithCorrectParameters(int scrollStep)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ScrollView.SetHorizontalScrollStep(element, scrollStep);

            // Assert
            element.Received(1).SetValue(ScrollView.HorizontalScrollStepProperty, scrollStep);
        }

        /// <summary>
        /// Tests that SetHorizontalScrollStep throws ArgumentNullException when element is null.
        /// </summary>
        [Fact]
        public void SetHorizontalScrollStep_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            int scrollStep = 10;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ScrollView.SetHorizontalScrollStep(element, scrollStep));
        }

        /// <summary>
        /// Tests that GetVerticalScrollStep returns the correct value when called with a valid BindableObject element.
        /// Input: Valid BindableObject with VerticalScrollStepProperty set to a specific value.
        /// Expected: Returns the integer value stored in the VerticalScrollStepProperty.
        /// </summary>
        [Theory]
        [InlineData(-1)] // Default value
        [InlineData(0)]  // Boundary value
        [InlineData(1)]  // Positive value
        [InlineData(100)] // Larger positive value
        [InlineData(int.MaxValue)] // Maximum value
        public void GetVerticalScrollStep_ValidElement_ReturnsExpectedValue(int expectedValue)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(TizenSpecific.ScrollView.VerticalScrollStepProperty).Returns(expectedValue);

            // Act
            int result = TizenSpecific.ScrollView.GetVerticalScrollStep(mockElement);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetVerticalScrollStep throws NullReferenceException when called with a null element.
        /// Input: null BindableObject element.
        /// Expected: Throws NullReferenceException when attempting to call GetValue on null element.
        /// </summary>
        [Fact]
        public void GetVerticalScrollStep_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TizenSpecific.ScrollView.GetVerticalScrollStep(nullElement));
        }
    }
}