using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ToolbarElementTests
    {
        /// <summary>
        /// Tests that SetValue returns early when the current toolbar equals the new value,
        /// ensuring no handler operations are performed for optimization.
        /// </summary>
        [Fact]
        public void SetValue_WhenToolbarEqualsValue_ReturnsEarly()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            var mockHandler = Substitute.For<IElementHandler>();
            var mockToolbarHandler = Substitute.For<IElementHandler>();
            toolbar.Handler = mockToolbarHandler;

            // Act
            ToolbarElement.SetValue(ref toolbar, toolbar, mockHandler);

            // Assert
            mockToolbarHandler.DidNotReceive().DisconnectHandler();
            mockHandler.DidNotReceive().UpdateValue(Arg.Any<string>());
        }

        /// <summary>
        /// Tests that SetValue returns early when both toolbar and value are null,
        /// ensuring no handler operations are performed.
        /// </summary>
        [Fact]
        public void SetValue_WhenBothToolbarAndValueAreNull_ReturnsEarly()
        {
            // Arrange
            Toolbar toolbar = null;
            Toolbar value = null;
            var mockHandler = Substitute.For<IElementHandler>();

            // Act
            ToolbarElement.SetValue(ref toolbar, value, mockHandler);

            // Assert
            Assert.Null(toolbar);
            mockHandler.DidNotReceive().UpdateValue(Arg.Any<string>());
        }

        /// <summary>
        /// Tests that SetValue properly sets a new toolbar when the current toolbar is null,
        /// and calls UpdateValue on the handler.
        /// </summary>
        [Fact]
        public void SetValue_WhenToolbarIsNullAndValueIsNotNull_SetsValueAndUpdatesHandler()
        {
            // Arrange
            Toolbar toolbar = null;
            var mockParent = Substitute.For<IElement>();
            var value = new Toolbar(mockParent);
            var mockHandler = Substitute.For<IElementHandler>();

            // Act
            ToolbarElement.SetValue(ref toolbar, value, mockHandler);

            // Assert
            Assert.Same(value, toolbar);
            mockHandler.Received(1).UpdateValue(nameof(IToolbarElement.Toolbar));
        }

        /// <summary>
        /// Tests that SetValue disconnects the existing toolbar's handler and sets the new value to null,
        /// then calls UpdateValue on the handler.
        /// </summary>
        [Fact]
        public void SetValue_WhenToolbarIsNotNullAndValueIsNull_DisconnectsAndSetsNull()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            var mockToolbarHandler = Substitute.For<IElementHandler>();
            toolbar.Handler = mockToolbarHandler;
            Toolbar value = null;
            var mockHandler = Substitute.For<IElementHandler>();

            // Act
            ToolbarElement.SetValue(ref toolbar, value, mockHandler);

            // Assert
            Assert.Null(toolbar);
            mockToolbarHandler.Received(1).DisconnectHandler();
            mockHandler.Received(1).UpdateValue(nameof(IToolbarElement.Toolbar));
        }

        /// <summary>
        /// Tests that SetValue disconnects the existing toolbar's handler and sets the new toolbar value,
        /// then calls UpdateValue on the handler.
        /// </summary>
        [Fact]
        public void SetValue_WhenReplacingToolbarWithDifferentToolbar_DisconnectsAndSetsNew()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            var mockToolbarHandler = Substitute.For<IElementHandler>();
            toolbar.Handler = mockToolbarHandler;

            var newToolbar = new Toolbar(mockParent);
            var mockHandler = Substitute.For<IElementHandler>();

            // Act
            ToolbarElement.SetValue(ref toolbar, newToolbar, mockHandler);

            // Assert
            Assert.Same(newToolbar, toolbar);
            mockToolbarHandler.Received(1).DisconnectHandler();
            mockHandler.Received(1).UpdateValue(nameof(IToolbarElement.Toolbar));
        }

        /// <summary>
        /// Tests that SetValue handles null handler parameter gracefully,
        /// ensuring no UpdateValue call is made when handler is null.
        /// </summary>
        [Fact]
        public void SetValue_WhenHandlerIsNull_DoesNotCallUpdateValue()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            var mockToolbarHandler = Substitute.For<IElementHandler>();
            toolbar.Handler = mockToolbarHandler;

            var newToolbar = new Toolbar(mockParent);
            IElementHandler handler = null;

            // Act
            ToolbarElement.SetValue(ref toolbar, newToolbar, handler);

            // Assert
            Assert.Same(newToolbar, toolbar);
            mockToolbarHandler.Received(1).DisconnectHandler();
        }

        /// <summary>
        /// Tests that SetValue handles null toolbar handler gracefully,
        /// ensuring no DisconnectHandler call is made when toolbar.Handler is null.
        /// </summary>
        [Fact]
        public void SetValue_WhenExistingToolbarHandlerIsNull_DoesNotCallDisconnectHandler()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            toolbar.Handler = null;

            var newToolbar = new Toolbar(mockParent);
            var mockHandler = Substitute.For<IElementHandler>();

            // Act
            ToolbarElement.SetValue(ref toolbar, newToolbar, mockHandler);

            // Assert
            Assert.Same(newToolbar, toolbar);
            mockHandler.Received(1).UpdateValue(nameof(IToolbarElement.Toolbar));
        }
    }
}
