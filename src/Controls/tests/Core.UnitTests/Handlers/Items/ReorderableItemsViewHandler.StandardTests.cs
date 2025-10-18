#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ReorderableItemsViewHandlerTests
    {
        /// <summary>
        /// Tests that MapCanReorderItems method can be called with null handler parameter without throwing an exception.
        /// This verifies the method handles null handler gracefully in the Standard implementation.
        /// </summary>
        [Fact]
        public void MapCanReorderItems_NullHandler_DoesNotThrow()
        {
            // Arrange
            ReorderableItemsViewHandler<ReorderableItemsView> handler = null;
            var itemsView = Substitute.For<ReorderableItemsView>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ReorderableItemsViewHandler<ReorderableItemsView>.MapCanReorderItems(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCanReorderItems method can be called with null itemsView parameter without throwing an exception.
        /// This verifies the method handles null itemsView gracefully in the Standard implementation.
        /// </summary>
        [Fact]
        public void MapCanReorderItems_NullItemsView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ReorderableItemsViewHandler<ReorderableItemsView>>();
            ReorderableItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() =>
                ReorderableItemsViewHandler<ReorderableItemsView>.MapCanReorderItems(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCanReorderItems method can be called with both null parameters without throwing an exception.
        /// This verifies the method handles all null inputs gracefully in the Standard implementation.
        /// </summary>
        [Fact]
        public void MapCanReorderItems_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ReorderableItemsViewHandler<ReorderableItemsView> handler = null;
            ReorderableItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() =>
                ReorderableItemsViewHandler<ReorderableItemsView>.MapCanReorderItems(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCanReorderItems method can be called with valid parameters without throwing an exception.
        /// This verifies the method executes successfully with proper inputs in the Standard implementation.
        /// </summary>
        [Fact]
        public void MapCanReorderItems_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ReorderableItemsViewHandler<ReorderableItemsView>>();
            var itemsView = Substitute.For<ReorderableItemsView>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ReorderableItemsViewHandler<ReorderableItemsView>.MapCanReorderItems(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CreatePlatformView method always throws NotImplementedException.
        /// This verifies the standard platform implementation correctly indicates the method is not implemented.
        /// </summary>
        [Fact]
        public void CreatePlatformView_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableReorderableItemsViewHandler();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => handler.CallCreatePlatformView());
        }

        /// <summary>
        /// Helper class to expose the protected CreatePlatformView method for testing.
        /// </summary>
        private class TestableReorderableItemsViewHandler : ReorderableItemsViewHandler<ReorderableItemsView>
        {
            /// <summary>
            /// Exposes the protected CreatePlatformView method for testing purposes.
            /// </summary>
            /// <returns>The result of calling CreatePlatformView.</returns>
            public object CallCreatePlatformView()
            {
                return CreatePlatformView();
            }
        }
    }
}