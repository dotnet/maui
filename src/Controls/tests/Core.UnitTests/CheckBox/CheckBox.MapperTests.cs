#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class CheckBoxTests
    {
        /// <summary>
        /// Tests that MapColor method does not throw when handler is null.
        /// Input: null handler, valid ICheckBox view.
        /// Expected: No exception thrown, UpdateValue not called.
        /// </summary>
        [Fact]
        public void MapColor_NullHandler_DoesNotThrow()
        {
            // Arrange
            ICheckBoxHandler handler = null;
            var view = Substitute.For<ICheckBox>();

            // Act & Assert
            var exception = Record.Exception(() => CheckBox.MapColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapColor method calls UpdateValue with correct property name when handler is not null.
        /// Input: valid handler, valid ICheckBox view.
        /// Expected: UpdateValue called with "Foreground" parameter.
        /// </summary>
        [Fact]
        public void MapColor_ValidHandler_CallsUpdateValueWithForegroundProperty()
        {
            // Arrange
            var handler = Substitute.For<ICheckBoxHandler>();
            var view = Substitute.For<ICheckBox>();

            // Act
            CheckBox.MapColor(handler, view);

            // Assert
            handler.Received(1).UpdateValue("Foreground");
        }

        /// <summary>
        /// Tests that MapColor method behavior with null view parameter.
        /// Input: valid handler, null ICheckBox view.
        /// Expected: UpdateValue still called with "Foreground" parameter.
        /// </summary>
        [Fact]
        public void MapColor_NullView_StillCallsUpdateValue()
        {
            // Arrange
            var handler = Substitute.For<ICheckBoxHandler>();
            ICheckBox view = null;

            // Act
            CheckBox.MapColor(handler, view);

            // Assert
            handler.Received(1).UpdateValue("Foreground");
        }

        /// <summary>
        /// Tests that MapColor method does not call UpdateValue when both parameters are null.
        /// Input: null handler, null ICheckBox view.
        /// Expected: No UpdateValue called, no exception thrown.
        /// </summary>
        [Fact]
        public void MapColor_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ICheckBoxHandler handler = null;
            ICheckBox view = null;

            // Act & Assert
            var exception = Record.Exception(() => CheckBox.MapColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapColor method calls UpdateValue exactly once with valid handler.
        /// Input: valid handler, valid ICheckBox view.
        /// Expected: UpdateValue called exactly once.
        /// </summary>
        [Fact]
        public void MapColor_ValidHandler_CallsUpdateValueExactlyOnce()
        {
            // Arrange
            var handler = Substitute.For<ICheckBoxHandler>();
            var view = Substitute.For<ICheckBox>();

            // Act
            CheckBox.MapColor(handler, view);

            // Assert
            handler.Received(1).UpdateValue(Arg.Any<string>());
            handler.DidNotReceive().UpdateValue(Arg.Is<string>(s => s != "Foreground"));
        }
    }
}
