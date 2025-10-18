#nullable disable

using System;
using Xunit;
using Microsoft.Maui.Controls.Handlers.Items;


namespace Microsoft.Maui.Controls.Core.UnitTests.Handlers.Items
{
    public partial class CollectionViewHandlerTests
    {
        /// <summary>
        /// Tests that CreatePlatformView throws NotImplementedException when called.
        /// This verifies the method correctly indicates that platform-specific implementation is not available.
        /// </summary>
        [Fact]
        public void CreatePlatformView_WhenCalled_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableCollectionViewHandler();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => handler.TestCreatePlatformView());
        }

        /// <summary>
        /// Helper class to expose the protected CreatePlatformView method for testing.
        /// </summary>
        private class TestableCollectionViewHandler : CollectionViewHandler
        {
            public object TestCreatePlatformView()
            {
                return CreatePlatformView();
            }
        }
    }
}
