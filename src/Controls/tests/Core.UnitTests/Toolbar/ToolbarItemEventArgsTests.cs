using Microsoft.Maui.Controls;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ToolbarItemEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly assigns a valid ToolbarItem to the ToolbarItem property.
        /// Input: Valid ToolbarItem instance.
        /// Expected: The ToolbarItem property returns the same instance that was passed to the constructor.
        /// </summary>
        [Fact]
        public void Constructor_ValidToolbarItem_AssignsToolbarItemProperty()
        {
            // Arrange
            var toolbarItem = new ToolbarItem();

            // Act
            var eventArgs = new ToolbarItemEventArgs(toolbarItem);

            // Assert
            Assert.Same(toolbarItem, eventArgs.ToolbarItem);
        }

        /// <summary>
        /// Tests that the constructor accepts a null ToolbarItem and assigns it to the ToolbarItem property.
        /// Input: null ToolbarItem.
        /// Expected: The ToolbarItem property returns null without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_NullToolbarItem_AssignsNullToToolbarItemProperty()
        {
            // Arrange
            ToolbarItem nullToolbarItem = null;

            // Act
            var eventArgs = new ToolbarItemEventArgs(nullToolbarItem);

            // Assert
            Assert.Null(eventArgs.ToolbarItem);
        }

        /// <summary>
        /// Tests that the constructor creates an instance that inherits from EventArgs.
        /// Input: Valid ToolbarItem instance.
        /// Expected: The created instance is of type EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_ValidToolbarItem_CreatesEventArgsInstance()
        {
            // Arrange
            var toolbarItem = new ToolbarItem();

            // Act
            var eventArgs = new ToolbarItemEventArgs(toolbarItem);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}
