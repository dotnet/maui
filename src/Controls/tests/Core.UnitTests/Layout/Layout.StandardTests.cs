#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class LayoutTests
    {
        /// <summary>
        /// Tests that MapInputTransparent executes without exception when called with valid parameters.
        /// This test ensures the obsolete method can be called with valid ILayoutHandler and Layout instances.
        /// Should complete successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapInputTransparent_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILayoutHandler>();
            var layout = Substitute.For<Layout>();

            // Act & Assert
            var exception = Record.Exception(() => Layout.MapInputTransparent(handler, layout));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapInputTransparent handles null handler parameter gracefully.
        /// This test verifies the obsolete method doesn't throw when the handler parameter is null.
        /// Should complete successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapInputTransparent_NullHandler_DoesNotThrow()
        {
            // Arrange
            ILayoutHandler handler = null;
            var layout = Substitute.For<Layout>();

            // Act & Assert
            var exception = Record.Exception(() => Layout.MapInputTransparent(handler, layout));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapInputTransparent handles null layout parameter gracefully.
        /// This test verifies the obsolete method doesn't throw when the layout parameter is null.
        /// Should complete successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapInputTransparent_NullLayout_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILayoutHandler>();
            Layout layout = null;

            // Act & Assert
            var exception = Record.Exception(() => Layout.MapInputTransparent(handler, layout));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapInputTransparent handles both null parameters gracefully.
        /// This test verifies the obsolete method doesn't throw when both parameters are null.
        /// Should complete successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapInputTransparent_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ILayoutHandler handler = null;
            Layout layout = null;

            // Act & Assert
            var exception = Record.Exception(() => Layout.MapInputTransparent(handler, layout));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapInputTransparent with valid LayoutHandler and Layout instances completes without throwing exceptions.
        /// The method is obsolete and has an empty body, so it should not perform any operations.
        /// </summary>
        [Fact]
        public void MapInputTransparent_ValidHandlerAndLayout_DoesNotThrow()
        {
            // Arrange
            var handler = new LayoutHandler();
            var layout = Substitute.For<Layout>();

            // Act & Assert
            var exception = Record.Exception(() => Layout.MapInputTransparent(handler, layout));
            Assert.Null(exception);
        }

    }
}