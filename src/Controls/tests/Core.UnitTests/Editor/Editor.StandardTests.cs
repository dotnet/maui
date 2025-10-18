#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class EditorTests
    {
        /// <summary>
        /// Tests that MapText method with IEditorHandler does not throw when called with valid handler and valid editor.
        /// Input conditions: Non-null IEditorHandler mock and non-null Editor instance.
        /// Expected result: Method executes without throwing any exception.
        /// </summary>
        [Fact]
        public void MapText_ValidHandlerAndValidEditor_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IEditorHandler>();
            var editor = new Editor();

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IEditorHandler does not throw when called with null handler and valid editor.
        /// Input conditions: Null IEditorHandler and non-null Editor instance.
        /// Expected result: Method executes without throwing any exception.
        /// </summary>
        [Fact]
        public void MapText_NullHandlerAndValidEditor_DoesNotThrow()
        {
            // Arrange
            IEditorHandler handler = null;
            var editor = new Editor();

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IEditorHandler does not throw when called with valid handler and null editor.
        /// Input conditions: Non-null IEditorHandler mock and null Editor instance.
        /// Expected result: Method executes without throwing any exception.
        /// </summary>
        [Fact]
        public void MapText_ValidHandlerAndNullEditor_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IEditorHandler>();
            Editor editor = null;

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IEditorHandler does not throw when called with both null parameters.
        /// Input conditions: Null IEditorHandler and null Editor instance.
        /// Expected result: Method executes without throwing any exception.
        /// </summary>
        [Fact]
        public void MapText_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IEditorHandler handler = null;
            Editor editor = null;

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method can be called with valid EditorHandler and Editor parameters without throwing exceptions.
        /// This verifies the basic functionality of the static MapText method.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapText_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<EditorHandler>();
            var editor = Substitute.For<Editor>();

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapText method behavior when EditorHandler parameter is null.
        /// This verifies how the method handles null handler input.
        /// Expected result: Method completes without throwing any exceptions since the implementation is empty.
        /// </summary>
        [Fact]
        public void MapText_NullHandler_DoesNotThrow()
        {
            // Arrange
            EditorHandler handler = null;
            var editor = Substitute.For<Editor>();

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapText method behavior when Editor parameter is null.
        /// This verifies how the method handles null editor input.
        /// Expected result: Method completes without throwing any exceptions since the implementation is empty.
        /// </summary>
        [Fact]
        public void MapText_NullEditor_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<EditorHandler>();
            Editor editor = null;

            // Act & Assert
            var exception = Record.Exception(() => Editor.MapText(handler, editor));
            Assert.Null(exception);
        }

    }
}