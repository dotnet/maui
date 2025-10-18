#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class LineHandlerTests
    {
        /// <summary>
        /// Tests that MapY1 executes successfully with valid handler and line parameters.
        /// Verifies the method can be called without throwing exceptions when provided with valid inputs.
        /// </summary>
        [Fact]
        public void MapY1_ValidParameters_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            var line = new Line();

            // Act & Assert
            var exception = Record.Exception(() => LineHandler.MapY1(handler, line));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapY1 handles null handler parameter.
        /// Verifies the method behavior when the handler parameter is null.
        /// </summary>
        [Fact]
        public void MapY1_NullHandler_ExecutesWithoutException()
        {
            // Arrange
            IShapeViewHandler handler = null;
            var line = new Line();

            // Act & Assert
            var exception = Record.Exception(() => LineHandler.MapY1(handler, line));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapY1 handles null line parameter.
        /// Verifies the method behavior when the line parameter is null.
        /// </summary>
        [Fact]
        public void MapY1_NullLine_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            Line line = null;

            // Act & Assert
            var exception = Record.Exception(() => LineHandler.MapY1(handler, line));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapY1 handles both parameters being null.
        /// Verifies the method behavior when both handler and line parameters are null.
        /// </summary>
        [Fact]
        public void MapY1_BothParametersNull_ExecutesWithoutException()
        {
            // Arrange
            IShapeViewHandler handler = null;
            Line line = null;

            // Act & Assert
            var exception = Record.Exception(() => LineHandler.MapY1(handler, line));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapY1 with line having various Y1 coordinate values.
        /// Verifies the method handles different Y1 coordinate scenarios including boundary values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(-1000.5)]
        [InlineData(1000.5)]
        public void MapY1_VariousY1Values_ExecutesWithoutException(double y1Value)
        {
            // Arrange
            var handler = Substitute.For<IShapeViewHandler>();
            var line = new Line { Y1 = y1Value };

            // Act & Assert
            var exception = Record.Exception(() => LineHandler.MapY1(handler, line));
            Assert.Null(exception);
        }
    }
}
