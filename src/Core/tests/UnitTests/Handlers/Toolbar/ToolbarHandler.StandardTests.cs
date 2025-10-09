using System;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class ToolbarHandlerTests
    {
        /// <summary>
        /// Tests that MapTitle method can be called with both parameters as null without throwing exceptions.
        /// This verifies the method handles null inputs gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_BothParametersNull_DoesNotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => ToolbarHandler.MapTitle(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method can be called with null handler and valid toolbar without throwing exceptions.
        /// This verifies the method handles null handler parameter gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_NullHandler_ValidToolbar_DoesNotThrow()
        {
            // Arrange
            var toolbar = Substitute.For<IToolbar>();

            // Act & Assert
            var exception = Record.Exception(() => ToolbarHandler.MapTitle(null, toolbar));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method can be called with valid handler and null toolbar without throwing exceptions.
        /// This verifies the method handles null toolbar parameter gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_ValidHandler_NullToolbar_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IToolbarHandler>();

            // Act & Assert
            var exception = Record.Exception(() => ToolbarHandler.MapTitle(handler, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method can be called with valid handler and toolbar without throwing exceptions.
        /// This verifies the method executes successfully with valid inputs.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitle_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IToolbarHandler>();
            var toolbar = Substitute.For<IToolbar>();

            // Act & Assert
            var exception = Record.Exception(() => ToolbarHandler.MapTitle(handler, toolbar));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitle method can be called with valid parameters including toolbar with title property set.
        /// This verifies the method handles toolbars with actual title values without issues.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Test Title")]
        [InlineData("Very Long Title That Contains Multiple Words And Special Characters !@#$%")]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void MapTitle_ValidParametersWithVariousTitles_DoesNotThrow(string title)
        {
            // Arrange
            var handler = Substitute.For<IToolbarHandler>();
            var toolbar = Substitute.For<IToolbar>();
            toolbar.Title.Returns(title ?? string.Empty);

            // Act & Assert
            var exception = Record.Exception(() => ToolbarHandler.MapTitle(handler, toolbar));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CreatePlatformElement throws NotImplementedException.
        /// This verifies the expected behavior for the standard platform implementation.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void CreatePlatformElement_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableToolbarHandler();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => handler.CreatePlatformElement());
        }

        /// <summary>
        /// Helper class to expose the protected CreatePlatformElement method for testing.
        /// </summary>
        private class TestableToolbarHandler : ToolbarHandler
        {
            public new object CreatePlatformElement()
            {
                return base.CreatePlatformElement();
            }
        }
    }
}