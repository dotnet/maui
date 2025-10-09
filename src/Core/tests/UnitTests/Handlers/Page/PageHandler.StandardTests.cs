using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class PageHandlerTests
    {
        /// <summary>
        /// Tests that MapTitle method executes successfully with valid handler and page parameters.
        /// Verifies that the method completes without throwing any exceptions when provided with mock implementations.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPageHandler>();
            var page = Substitute.For<IContentView>();

            // Act & Assert
            var exception = Record.Exception(() => PageHandler.MapTitle(handler, page));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method handles null handler parameter gracefully.
        /// Verifies that the method does not throw when the handler parameter is null.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            IPageHandler handler = null;
            var page = Substitute.For<IContentView>();

            // Act & Assert
            var exception = Record.Exception(() => PageHandler.MapTitle(handler, page));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method handles null page parameter gracefully.
        /// Verifies that the method does not throw when the page parameter is null.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_WithNullPage_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPageHandler>();
            IContentView page = null;

            // Act & Assert
            var exception = Record.Exception(() => PageHandler.MapTitle(handler, page));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method handles both null parameters gracefully.
        /// Verifies that the method does not throw when both handler and page parameters are null.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            IPageHandler handler = null;
            IContentView page = null;

            // Act & Assert
            var exception = Record.Exception(() => PageHandler.MapTitle(handler, page));

            Assert.Null(exception);
        }
    }
}
