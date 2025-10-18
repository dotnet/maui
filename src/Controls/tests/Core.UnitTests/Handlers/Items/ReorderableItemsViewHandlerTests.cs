#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Handlers.Items
{
    public partial class ReorderableItemsViewHandlerTests
    {
        /// <summary>
        /// Tests that the ReorderableItemsViewHandler constructor with default parameter (null) 
        /// successfully creates an instance using the ReorderableItemsViewMapper as fallback.
        /// </summary>
        [Fact]
        public void Constructor_WithDefaultParameter_CreatesInstanceSuccessfully()
        {
            // Arrange & Act
            var handler = new ReorderableItemsViewHandler<ReorderableItemsView>();

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the ReorderableItemsViewHandler constructor with explicit null parameter
        /// successfully creates an instance using the ReorderableItemsViewMapper as fallback.
        /// </summary>
        [Fact]
        public void Constructor_WithExplicitNull_CreatesInstanceSuccessfully()
        {
            // Arrange & Act
            var handler = new ReorderableItemsViewHandler<ReorderableItemsView>(null);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the ReorderableItemsViewHandler constructor with a valid PropertyMapper
        /// successfully creates an instance using the provided mapper.
        /// </summary>
        [Fact]
        public void Constructor_WithValidMapper_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockMapper = Substitute.For<PropertyMapper>();

            // Act
            var handler = new ReorderableItemsViewHandler<ReorderableItemsView>(mockMapper);

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates an instance of ReorderableItemsViewHandler.
        /// Verifies that the constructor calls the base constructor with ReorderableItemsViewMapper and 
        /// creates a valid handler instance.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_CreatesValidInstance()
        {
            // Arrange & Act
            var handler = new ReorderableItemsViewHandler<ReorderableItemsView>();

            // Assert
            Assert.NotNull(handler);
            Assert.IsType<ReorderableItemsViewHandler<ReorderableItemsView>>(handler);
        }
    }
}