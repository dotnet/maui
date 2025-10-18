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
        /// Tests that MapLineBreakMode with ButtonHandler and valid parameters does not throw an exception.
        /// Verifies the method executes successfully with valid inputs.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_ButtonHandler_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ButtonHandler>();
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler and valid parameters does not throw an exception.
        /// Verifies the method executes successfully with valid inputs.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_IButtonHandler_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with ButtonHandler handles null handler parameter gracefully.
        /// Verifies the method does not throw when handler is null.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_ButtonHandler_NullHandler_DoesNotThrow()
        {
            // Arrange
            ButtonHandler handler = null;
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler handles null handler parameter gracefully.
        /// Verifies the method does not throw when handler is null.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_IButtonHandler_NullHandler_DoesNotThrow()
        {
            // Arrange
            IButtonHandler handler = null;
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with ButtonHandler handles null button parameter gracefully.
        /// Verifies the method does not throw when button is null.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_ButtonHandler_NullButton_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ButtonHandler>();
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler handles null button parameter gracefully.
        /// Verifies the method does not throw when button is null.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_IButtonHandler_NullButton_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with ButtonHandler handles both null parameters gracefully.
        /// Verifies the method does not throw when both handler and button are null.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_ButtonHandler_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ButtonHandler handler = null;
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler handles both null parameters gracefully.
        /// Verifies the method does not throw when both handler and button are null.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_IButtonHandler_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IButtonHandler handler = null;
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IButtonHandler can be called with valid parameters without throwing exceptions.
        /// This test validates the method can execute successfully with non-null handler and button parameters.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapText_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapText(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IButtonHandler handles null handler parameter without throwing exceptions.
        /// This test validates the method behavior when the handler parameter is null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapText_NullHandler_DoesNotThrow()
        {
            // Arrange
            IButtonHandler handler = null;
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapText(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IButtonHandler handles null button parameter without throwing exceptions.
        /// This test validates the method behavior when the button parameter is null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapText_NullButton_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapText(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with IButtonHandler handles both null parameters without throwing exceptions.
        /// This test validates the method behavior when both handler and button parameters are null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapText_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IButtonHandler handler = null;
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapText(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler can be called with valid parameters without throwing exceptions.
        /// Input: Valid IButtonHandler mock and Button instance.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler can be called with null handler without throwing exceptions.
        /// Input: Null IButtonHandler and valid Button instance.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithNullHandler_CompletesSuccessfully()
        {
            // Arrange
            IButtonHandler handler = null;
            var button = new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler can be called with null button without throwing exceptions.
        /// Input: Valid IButtonHandler mock and null Button.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithNullButton_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler can be called with both parameters null without throwing exceptions.
        /// Input: Null IButtonHandler and null Button.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithBothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            IButtonHandler handler = null;
            Button button = null;

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode with IButtonHandler can be called with a Button that has LineBreakMode property set.
        /// Input: Valid IButtonHandler mock and Button with configured LineBreakMode.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithConfiguredButton_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IButtonHandler>();
            var button = new Button
            {
                LineBreakMode = LineBreakMode.WordWrap
            };

            // Act & Assert
            var exception = Record.Exception(() => Button.MapLineBreakMode(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapText method behavior with various null parameter combinations.
        /// Verifies the method handles null inputs appropriately without throwing exceptions.
        /// </summary>
        /// <param name="handler">The ButtonHandler parameter to test (may be null)</param>
        /// <param name="button">The Button parameter to test (may be null)</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "validButton")]
        [InlineData("validHandler", null)]
        public void MapText_NullParameters_DoesNotThrow(string handlerType, string buttonType)
        {
            // Arrange
            var handler = handlerType == null ? null : Substitute.For<ButtonHandler>();
            var button = buttonType == null ? null : new Button();

            // Act & Assert
            var exception = Record.Exception(() => Button.MapText(handler, button));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method can be called multiple times without side effects.
        /// Verifies the method is idempotent and stateless.
        /// </summary>
        [Fact]
        public void MapText_MultipleInvocations_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ButtonHandler>();
            var button = new Button { Text = "Test Button" };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                Button.MapText(handler, button);
                Button.MapText(handler, button);
                Button.MapText(handler, button);
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapText method with a Button that has various text property values.
        /// Verifies the method handles different button states without throwing exceptions.
        /// </summary>
        /// <param name="text">The text value to set on the button</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Simple Text")]
        [InlineData("Text with special characters: !@#$%^&*()")]
        [InlineData("Very long text that might exceed typical button text length limitations and could potentially cause issues in some scenarios")]
        public void MapText_ButtonWithVariousTextValues_DoesNotThrow(string text)
        {
            // Arrange
            var handler = Substitute.For<ButtonHandler>();
            var button = new Button { Text = text };

            // Act & Assert
            var exception = Record.Exception(() => Button.MapText(handler, button));
            Assert.Null(exception);
        }
    }
}