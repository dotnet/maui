using System;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class MenuBarItemHandlerTests
    {
        /// <summary>
        /// Tests that the Add method executes without throwing an exception when provided with a valid IMenuElement instance.
        /// This verifies the standard implementation handles valid menu elements gracefully.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Add_WithValidMenuElement_DoesNotThrowException()
        {
            // Arrange
            var handler = new MenuBarItemHandler();
            var menuElement = Substitute.For<IMenuElement>();

            // Act & Assert
            var exception = Record.Exception(() => handler.Add(menuElement));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the Add method handles null input gracefully, even though the parameter should be non-nullable.
        /// This tests the robustness of the standard implementation against invalid input.
        /// Expected result: Method should handle null input without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Add_WithNullMenuElement_DoesNotThrowException()
        {
            // Arrange
            var handler = new MenuBarItemHandler();

            // Act & Assert
            var exception = Record.Exception(() => handler.Add(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the Remove method executes without throwing exceptions when provided with a valid IMenuElement.
        /// This validates the basic functionality of the empty implementation.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Remove_WithValidMenuElement_DoesNotThrow()
        {
            // Arrange
            var handler = new MenuBarItemHandler();
            var menuElement = Substitute.For<IMenuElement>();

            // Act & Assert
            var exception = Record.Exception(() => handler.Remove(menuElement));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the Remove method executes without throwing exceptions when provided with a null IMenuElement.
        /// This validates that the empty implementation handles null input gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Remove_WithNullMenuElement_DoesNotThrow()
        {
            // Arrange
            var handler = new MenuBarItemHandler();

            // Act & Assert
            var exception = Record.Exception(() => handler.Remove(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple calls to Remove method execute without throwing exceptions.
        /// This validates that the empty implementation maintains consistent behavior across multiple invocations.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Remove_MultipleCalls_DoesNotThrow()
        {
            // Arrange
            var handler = new MenuBarItemHandler();
            var menuElement1 = Substitute.For<IMenuElement>();
            var menuElement2 = Substitute.For<IMenuElement>();

            // Act & Assert
            var exception1 = Record.Exception(() => handler.Remove(menuElement1));
            var exception2 = Record.Exception(() => handler.Remove(menuElement2));
            var exception3 = Record.Exception(() => handler.Remove(null));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests that CreatePlatformElement throws NotImplementedException.
        /// This verifies the standard platform implementation correctly indicates the method is not implemented.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void CreatePlatformElement_WhenCalled_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableMenuBarItemHandler();

            // Act & Assert
            var exception = Assert.Throws<NotImplementedException>(() => handler.CreatePlatformElementPublic());
            Assert.NotNull(exception);
        }

        /// <summary>
        /// Helper class to expose the protected CreatePlatformElement method for testing.
        /// </summary>
        private class TestableMenuBarItemHandler : MenuBarItemHandler
        {
            public object CreatePlatformElementPublic()
            {
                return CreatePlatformElement();
            }
        }
    }
}