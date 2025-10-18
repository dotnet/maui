#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the TabbedPage.SetToolbarPlacement extension method.
    /// </summary>
    public class TabbedPageSetToolbarPlacementTests
    {
        /// <summary>
        /// Tests that SetToolbarPlacement throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, TabbedPage> config = null;
            var toolbarPlacement = ToolbarPlacement.Top;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                TabbedPage.SetToolbarPlacement(config, toolbarPlacement));
        }

        /// <summary>
        /// Tests that SetToolbarPlacement propagates InvalidOperationException when trying to change already set toolbar placement.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_ChangingAlreadySetValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.IsSet(TabbedPage.ToolbarPlacementProperty).Returns(true);
            mockElement.GetValue(TabbedPage.ToolbarPlacementProperty).Returns(ToolbarPlacement.Top);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                TabbedPage.SetToolbarPlacement(mockConfig, ToolbarPlacement.Bottom));

            Assert.Equal("Changing the tabs placement after it's been set is not supported.", exception.Message);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles invalid enum values by casting and still calls the overload method.
        /// </summary>
        /// <param name="invalidEnumValue">Invalid enum value to test.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void SetToolbarPlacement_InvalidEnumValues_CallsOverloadAndReturnsConfig(int invalidEnumValue)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.IsSet(TabbedPage.ToolbarPlacementProperty).Returns(false);
            var invalidToolbarPlacement = (ToolbarPlacement)invalidEnumValue;

            // Act
            var result = TabbedPage.SetToolbarPlacement(mockConfig, invalidToolbarPlacement);

            // Assert
            mockElement.Received(1).SetValue(TabbedPage.ToolbarPlacementProperty, invalidToolbarPlacement);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement works correctly when setting the same value that's already set.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_SameValueAlreadySet_CallsOverloadAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, TabbedPage>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            mockElement.IsSet(TabbedPage.ToolbarPlacementProperty).Returns(true);
            mockElement.GetValue(TabbedPage.ToolbarPlacementProperty).Returns(ToolbarPlacement.Bottom);

            // Act
            var result = TabbedPage.SetToolbarPlacement(mockConfig, ToolbarPlacement.Bottom);

            // Assert
            mockElement.Received(1).SetValue(TabbedPage.ToolbarPlacementProperty, ToolbarPlacement.Bottom);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolbarPlacement handles null Element property by propagating the resulting exception.
        /// </summary>
        [Fact]
        public void SetToolbarPlacement_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, TabbedPage>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                TabbedPage.SetToolbarPlacement(mockConfig, ToolbarPlacement.Top));
        }
    }
}