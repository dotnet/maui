#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.StyleSheets;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the MenuShellItem class focusing on the Text property.
    /// </summary>
    public partial class MenuShellItemTests
    {
        /// <summary>
        /// Tests that the Text property returns the same value as the Title property for various string values.
        /// </summary>
        /// <param name="titleValue">The title value to test</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Test Title")]
        [InlineData("Title with special characters: !@#$%^&*()_+{}|:<>?[]\\;'\",./ ")]
        [InlineData("Very long title that exceeds normal length expectations and contains multiple words to test boundary conditions")]
        [InlineData("Title\nwith\nnewlines")]
        [InlineData("Title\twith\ttabs")]
        [InlineData("Title with unicode characters: 你好世界 🌍 ñáéíóú")]
        public void Text_WithVariousTitleValues_ReturnsSameValueAsTitle(string titleValue)
        {
            // Arrange
            var mockMenuItem = Substitute.For<MenuItem>();
            var menuShellItem = new MenuShellItem(mockMenuItem);
            menuShellItem.Title = titleValue;

            // Act
            var result = menuShellItem.Text;

            // Assert
            Assert.Equal(menuShellItem.Title, result);
        }

        /// <summary>
        /// Tests that the Text property returns the Title property value when Title is changed after construction.
        /// </summary>
        [Fact]
        public void Text_WhenTitleChangesAfterConstruction_ReturnsUpdatedTitleValue()
        {
            // Arrange
            var mockMenuItem = Substitute.For<MenuItem>();
            var menuShellItem = new MenuShellItem(mockMenuItem);
            menuShellItem.Title = "Initial Title";

            // Act & Assert - Initial value
            Assert.Equal("Initial Title", menuShellItem.Text);

            // Act - Change title
            menuShellItem.Title = "Updated Title";

            // Assert - Updated value
            Assert.Equal("Updated Title", menuShellItem.Text);
        }

        /// <summary>
        /// Tests that the Text property correctly reflects Title property changes from null to non-null values.
        /// </summary>
        [Fact]
        public void Text_WhenTitleChangesFromNullToValue_ReturnsNewValue()
        {
            // Arrange
            var mockMenuItem = Substitute.For<MenuItem>();
            var menuShellItem = new MenuShellItem(mockMenuItem);
            menuShellItem.Title = null;

            // Act & Assert - Initial null value
            Assert.Null(menuShellItem.Text);

            // Act - Change to non-null value
            menuShellItem.Title = "New Title";

            // Assert - Non-null value
            Assert.Equal("New Title", menuShellItem.Text);
        }

        /// <summary>
        /// Tests that the Text property correctly reflects Title property changes from non-null to null values.
        /// </summary>
        [Fact]
        public void Text_WhenTitleChangesFromValueToNull_ReturnsNull()
        {
            // Arrange
            var mockMenuItem = Substitute.For<MenuItem>();
            var menuShellItem = new MenuShellItem(mockMenuItem);
            menuShellItem.Title = "Initial Title";

            // Act & Assert - Initial non-null value
            Assert.Equal("Initial Title", menuShellItem.Text);

            // Act - Change to null
            menuShellItem.Title = null;

            // Assert - Null value
            Assert.Null(menuShellItem.Text);
        }
    }
}
