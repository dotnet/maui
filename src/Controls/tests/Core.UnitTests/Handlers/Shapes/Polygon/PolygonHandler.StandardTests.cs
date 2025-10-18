#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PolygonHandlerTests
    {
        /// <summary>
        /// Tests that MapFillRule executes successfully with valid handler and polygon parameters.
        /// Verifies the method can be called without throwing exceptions when both parameters are provided.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapFillRule_ValidParameters_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            var polygon = new Polygon();

            // Act & Assert
            var exception = Record.Exception(() => PolygonHandler.MapFillRule(handler, polygon));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFillRule executes successfully when handler parameter is null.
        /// Verifies the method handles null handler gracefully since it has an empty implementation.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapFillRule_NullHandler_ExecutesWithoutException()
        {
            // Arrange
            IShapeViewHandler handler = null;
            var polygon = new Polygon();

            // Act & Assert
            var exception = Record.Exception(() => PolygonHandler.MapFillRule(handler, polygon));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFillRule executes successfully when polygon parameter is null.
        /// Verifies the method handles null polygon gracefully since it has an empty implementation.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapFillRule_NullPolygon_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            Polygon polygon = null;

            // Act & Assert
            var exception = Record.Exception(() => PolygonHandler.MapFillRule(handler, polygon));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFillRule executes successfully when both parameters are null.
        /// Verifies the method handles all null parameters gracefully since it has an empty implementation.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapFillRule_BothParametersNull_ExecutesWithoutException()
        {
            // Arrange
            IShapeViewHandler handler = null;
            Polygon polygon = null;

            // Act & Assert
            var exception = Record.Exception(() => PolygonHandler.MapFillRule(handler, polygon));
            Assert.Null(exception);
        }
    }
}
