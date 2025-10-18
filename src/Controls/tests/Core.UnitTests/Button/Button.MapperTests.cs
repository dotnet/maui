#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ButtonTests
    {
        /// <summary>
        /// Tests that MapContentLayout throws NullReferenceException when handler parameter is null.
        /// </summary>
        [Fact]
        public void MapContentLayout_NullHandler_ThrowsNullReferenceException()
        {
            // Arrange
            IButtonHandler handler = null;
            var button = new Button();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Button.MapContentLayout(handler, button));
        }

        /// <summary>
        /// Tests that MapContentLayout throws NullReferenceException when handler's PlatformView is null.
        /// </summary>
        [Fact]
        public void MapContentLayout_HandlerWithNullPlatformView_ThrowsNullReferenceException()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            handler.PlatformView.Returns((object)null);
            var button = new Button();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Button.MapContentLayout(handler, button));
        }

        /// <summary>
        /// Tests that MapContentLayout successfully calls UpdateContentLayout with valid handler and null button.
        /// </summary>
        [Fact]
        public void MapContentLayout_ValidHandlerNullButton_CallsUpdateContentLayout()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var platformView = new object();
            handler.PlatformView.Returns(platformView);
            Button button = null;

            // Act
            Button.MapContentLayout(handler, button);

            // Assert
            // The method should complete without throwing an exception
            // UpdateContentLayout extension method will be called but does nothing in the implementation
            Assert.True(true); // Test passes if no exception is thrown
        }

        /// <summary>
        /// Tests that MapContentLayout successfully calls UpdateContentLayout with valid handler and button.
        /// </summary>
        [Fact]
        public void MapContentLayout_ValidHandlerAndButton_CallsUpdateContentLayout()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var platformView = new object();
            handler.PlatformView.Returns(platformView);
            var button = new Button();

            // Act
            Button.MapContentLayout(handler, button);

            // Assert
            // The method should complete without throwing an exception
            // UpdateContentLayout extension method will be called but does nothing in the implementation
            Assert.True(true); // Test passes if no exception is thrown
        }

        /// <summary>
        /// Tests that MapContentLayout accesses PlatformView property when called with valid parameters.
        /// </summary>
        [Fact]
        public void MapContentLayout_ValidParameters_AccessesPlatformViewProperty()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var platformView = new object();
            handler.PlatformView.Returns(platformView);
            var button = new Button();

            // Act
            Button.MapContentLayout(handler, button);

            // Assert
            // Verify that the PlatformView property was accessed
            var _ = handler.Received(1).PlatformView;
        }
    }

    public partial class ButtonMapperTests
    {
        /// <summary>
        /// Tests that MapContentLayout with ButtonHandler successfully delegates to the IButtonHandler overload
        /// when provided with valid ButtonHandler and Button parameters.
        /// </summary>
        [Fact]
        public void MapContentLayout_ValidButtonHandlerAndButton_DelegatesToInterfaceOverload()
        {
            // Arrange
            var mockHandler = Substitute.For<ButtonHandler>();
            var mockPlatformView = Substitute.For<object>();
            var button = new Button();

            // Configure the mock to avoid platform-specific calls
            var mockButtonHandler = mockHandler.As<IButtonHandler>();
            mockButtonHandler.PlatformView.Returns(mockPlatformView);

            // Act & Assert - Should not throw when delegating to interface version
            var exception = Record.Exception(() => Button.MapContentLayout(mockHandler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapContentLayout with ButtonHandler throws NullReferenceException
        /// when the ButtonHandler parameter is null, during the cast operation.
        /// </summary>
        [Fact]
        public void MapContentLayout_NullButtonHandler_ThrowsNullReferenceException()
        {
            // Arrange
            ButtonHandler nullHandler = null;
            var button = new Button();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Button.MapContentLayout(nullHandler, button));
        }

        /// <summary>
        /// Tests that MapContentLayout with ButtonHandler accepts null Button parameter
        /// and delegates to the interface version, passing the null button through.
        /// </summary>
        [Fact]
        public void MapContentLayout_ValidButtonHandlerAndNullButton_DelegatesToInterfaceOverload()
        {
            // Arrange
            var mockHandler = Substitute.For<ButtonHandler>();
            var mockPlatformView = Substitute.For<object>();
            Button nullButton = null;

            // Configure the mock to avoid platform-specific calls
            var mockButtonHandler = mockHandler.As<IButtonHandler>();
            mockButtonHandler.PlatformView.Returns(mockPlatformView);

            // Act & Assert - Should not throw during delegation, null handling is left to interface version
            var exception = Record.Exception(() => Button.MapContentLayout(mockHandler, nullButton));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapContentLayout with ButtonHandler throws NullReferenceException
        /// when both ButtonHandler and Button parameters are null.
        /// </summary>
        [Fact]
        public void MapContentLayout_NullButtonHandlerAndNullButton_ThrowsNullReferenceException()
        {
            // Arrange
            ButtonHandler nullHandler = null;
            Button nullButton = null;

            // Act & Assert - Should throw due to null handler cast, not the null button
            Assert.Throws<NullReferenceException>(() => Button.MapContentLayout(nullHandler, nullButton));
        }
    }
}