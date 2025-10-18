#nullable disable

using System;
using System.Collections;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ShapeTests
    {
        /// <summary>
        /// Tests that MapStrokeDashArray method executes successfully with valid handler and shapeView parameters
        /// without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapStrokeDashArray_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            var shapeView = Substitute.For<IShapeView>();

            // Act & Assert
            var exception = Record.Exception(() => Shapes.Shape.MapStrokeDashArray(handler, shapeView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeDashArray method handles null handler parameter
        /// without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapStrokeDashArray_NullHandler_DoesNotThrow()
        {
            // Arrange
            IShapeViewHandler handler = null;
            var shapeView = Substitute.For<IShapeView>();

            // Act & Assert
            var exception = Record.Exception(() => Shapes.Shape.MapStrokeDashArray(handler, shapeView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeDashArray method handles null shapeView parameter
        /// without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapStrokeDashArray_NullShapeView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            IShapeView shapeView = null;

            // Act & Assert
            var exception = Record.Exception(() => Shapes.Shape.MapStrokeDashArray(handler, shapeView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeDashArray method handles both null parameters
        /// without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapStrokeDashArray_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IShapeViewHandler handler = null;
            IShapeView shapeView = null;

            // Act & Assert
            var exception = Record.Exception(() => Shapes.Shape.MapStrokeDashArray(handler, shapeView));
            Assert.Null(exception);
        }
    }
}
