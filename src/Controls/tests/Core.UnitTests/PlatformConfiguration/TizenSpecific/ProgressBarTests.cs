#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the TizenSpecific ProgressBar extension methods.
    /// </summary>
    public partial class ProgressBarTests
    {
        /// <summary>
        /// Tests that GetPulsingStatus extension method throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void GetPulsingStatus_ConfigIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, ProgressBar> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TizenSpecific.ProgressBar.GetPulsingStatus(config));
        }

        /// <summary>
        /// Tests that GetPulsingStatus extension method returns the correct pulsing status when config and element are valid.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPulsingStatus_ValidConfigAndElement_ReturnsPulsingStatus(bool expectedPulsingStatus)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, ProgressBar>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(TizenSpecific.ProgressBar.ProgressBarPulsingStatusProperty)
                .Returns(expectedPulsingStatus);

            // Act
            bool result = TizenSpecific.ProgressBar.GetPulsingStatus(mockConfig);

            // Assert
            Assert.Equal(expectedPulsingStatus, result);
            mockConfig.Received(1).Element;
            mockElement.Received(1).GetValue(TizenSpecific.ProgressBar.ProgressBarPulsingStatusProperty);
        }

        /// <summary>
        /// Tests that GetPulsingStatus extension method throws NullReferenceException when config.Element is null.
        /// </summary>
        [Fact]
        public void GetPulsingStatus_ConfigElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, ProgressBar>>();
            mockConfig.Element.Returns((ProgressBar)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TizenSpecific.ProgressBar.GetPulsingStatus(mockConfig));
        }

        /// <summary>
        /// Tests that GetPulsingStatus throws NullReferenceException when element parameter is null.
        /// Verifies proper null parameter handling.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetPulsingStatus_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ProgressBar.GetPulsingStatus(element));
        }

        /// <summary>
        /// Tests that GetPulsingStatus returns the correct boolean value from the bindable property.
        /// Verifies that the method properly retrieves and casts the property value.
        /// Expected result: Should return the boolean value stored in the ProgressBarPulsingStatusProperty.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetPulsingStatus_ValidElement_ReturnsExpectedBooleanValue(bool expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ProgressBar.ProgressBarPulsingStatusProperty).Returns(expectedValue);

            // Act
            bool result = ProgressBar.GetPulsingStatus(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetPulsingStatus properly calls GetValue with the correct BindableProperty.
        /// Verifies that the method uses the correct property to retrieve the value.
        /// Expected result: GetValue should be called once with ProgressBarPulsingStatusProperty.
        /// </summary>
        [Fact]
        public void GetPulsingStatus_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ProgressBar.ProgressBarPulsingStatusProperty).Returns(true);

            // Act
            ProgressBar.GetPulsingStatus(element);

            // Assert
            element.Received(1).GetValue(ProgressBar.ProgressBarPulsingStatusProperty);
        }
    }
}