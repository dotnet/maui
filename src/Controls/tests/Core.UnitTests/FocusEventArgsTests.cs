#nullable disable

using System;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class FocusEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when visualElement parameter is null.
        /// This test verifies the null validation logic and ensures proper exception handling.
        /// </summary>
        [Fact]
        public void Constructor_NullVisualElement_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new FocusEventArgs(null, true));
            Assert.Equal("visualElement", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor properly initializes properties when provided with valid parameters.
        /// Verifies that both VisualElement and IsFocused properties are correctly assigned.
        /// </summary>
        /// <param name="isFocused">The focus state to test (true or false).</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_ValidParameters_InitializesPropertiesCorrectly(bool isFocused)
        {
            // Arrange
            var visualElement = Substitute.For<VisualElement>();

            // Act
            var focusEventArgs = new FocusEventArgs(visualElement, isFocused);

            // Assert
            Assert.Same(visualElement, focusEventArgs.VisualElement);
            Assert.Equal(isFocused, focusEventArgs.IsFocused);
        }
    }
}
