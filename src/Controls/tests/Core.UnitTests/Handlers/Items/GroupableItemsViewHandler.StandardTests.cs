#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class GroupableItemsViewHandlerTests
    {
        /// <summary>
        /// Tests that MapIsGrouped method executes successfully with valid handler and itemsView parameters
        /// without throwing any exceptions, verifying the basic method contract.
        /// </summary>
        [Fact]
        public void MapIsGrouped_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<GroupableItemsViewHandler<GroupableItemsView>>();
            var itemsView = new GroupableItemsView();

            // Act & Assert
            var exception = Record.Exception(() =>
                GroupableItemsViewHandler<GroupableItemsView>.MapIsGrouped(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsGrouped method executes successfully when the GroupableItemsView
        /// has IsGrouped property set to true, ensuring the method handles grouped state correctly.
        /// </summary>
        [Fact]
        public void MapIsGrouped_ItemsViewWithIsGroupedTrue_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<GroupableItemsViewHandler<GroupableItemsView>>();
            var itemsView = new GroupableItemsView { IsGrouped = true };

            // Act & Assert
            var exception = Record.Exception(() =>
                GroupableItemsViewHandler<GroupableItemsView>.MapIsGrouped(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsGrouped method executes successfully when the GroupableItemsView
        /// has IsGrouped property set to false, ensuring the method handles non-grouped state correctly.
        /// </summary>
        [Fact]
        public void MapIsGrouped_ItemsViewWithIsGroupedFalse_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<GroupableItemsViewHandler<GroupableItemsView>>();
            var itemsView = new GroupableItemsView { IsGrouped = false };

            // Act & Assert
            var exception = Record.Exception(() =>
                GroupableItemsViewHandler<GroupableItemsView>.MapIsGrouped(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsGrouped method handles null handler parameter appropriately,
        /// expecting an ArgumentNullException to be thrown for invalid input.
        /// </summary>
        [Fact]
        public void MapIsGrouped_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            GroupableItemsViewHandler<GroupableItemsView> handler = null;
            var itemsView = new GroupableItemsView();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                GroupableItemsViewHandler<GroupableItemsView>.MapIsGrouped(handler, itemsView));
        }

        /// <summary>
        /// Tests that MapIsGrouped method handles null itemsView parameter appropriately,
        /// expecting an ArgumentNullException to be thrown for invalid input.
        /// </summary>
        [Fact]
        public void MapIsGrouped_NullItemsView_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = Substitute.For<GroupableItemsViewHandler<GroupableItemsView>>();
            GroupableItemsView itemsView = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                GroupableItemsViewHandler<GroupableItemsView>.MapIsGrouped(handler, itemsView));
        }

        /// <summary>
        /// Tests that MapIsGrouped method handles both null parameters appropriately,
        /// expecting an ArgumentNullException to be thrown for invalid input.
        /// </summary>
        [Fact]
        public void MapIsGrouped_BothParametersNull_ThrowsArgumentNullException()
        {
            // Arrange
            GroupableItemsViewHandler<GroupableItemsView> handler = null;
            GroupableItemsView itemsView = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                GroupableItemsViewHandler<GroupableItemsView>.MapIsGrouped(handler, itemsView));
        }

        /// <summary>
        /// Tests that CreatePlatformView throws NotImplementedException.
        /// This verifies the standard implementation properly indicates the method is not implemented on this platform.
        /// </summary>
        [Fact]
        public void CreatePlatformView_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableGroupableItemsViewHandler();

            // Act & Assert
            var exception = Assert.Throws<NotImplementedException>(() => handler.CreatePlatformView());
            Assert.IsType<NotImplementedException>(exception);
        }

        /// <summary>
        /// Helper class to expose the protected CreatePlatformView method for testing.
        /// </summary>
        private class TestableGroupableItemsViewHandler : GroupableItemsViewHandler<CollectionView>
        {
            public new object CreatePlatformView() => base.CreatePlatformView();
        }
    }
}