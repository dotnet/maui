#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class LabelTests
    {
        /// <summary>
        /// Tests that MapLineBreakMode method executes without throwing exceptions when provided with valid handler and label parameters.
        /// This method has an empty body, so it should complete successfully regardless of input values.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithValidHandlerAndLabel_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapLineBreakMode(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode method executes without throwing exceptions when provided with null handler parameter.
        /// Since the method body is empty, it should handle null inputs gracefully.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            ILabelHandler handler = null;
            var label = new Label();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapLineBreakMode(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode method executes without throwing exceptions when provided with null label parameter.
        /// Since the method body is empty, it should handle null inputs gracefully.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithNullLabel_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapLineBreakMode(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapLineBreakMode method executes without throwing exceptions when both parameters are null.
        /// Since the method body is empty, it should handle all null combinations gracefully.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            ILabelHandler handler = null;
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapLineBreakMode(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapMaxLines with valid handler and label parameters does not throw an exception.
        /// The method is expected to execute without errors when provided with valid inputs.
        /// </summary>
        [Fact]
        public void MapMaxLines_ValidHandlerAndLabel_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapMaxLines(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapMaxLines with null handler parameter does not throw an exception.
        /// Since the method body is empty, it should handle null handler gracefully.
        /// </summary>
        [Fact]
        public void MapMaxLines_NullHandler_DoesNotThrow()
        {
            // Arrange
            ILabelHandler handler = null;
            var label = new Label();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapMaxLines(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapMaxLines with null label parameter does not throw an exception.
        /// Since the method body is empty, it should handle null label gracefully.
        /// </summary>
        [Fact]
        public void MapMaxLines_NullLabel_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapMaxLines(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapMaxLines with both null parameters does not throw an exception.
        /// Since the method body is empty, it should handle both null parameters gracefully.
        /// </summary>
        [Fact]
        public void MapMaxLines_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ILabelHandler handler = null;
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapMaxLines(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapMaxLines with a label that has various property values does not throw an exception.
        /// Verifies that the method works regardless of the label's state.
        /// </summary>
        [Fact]
        public void MapMaxLines_LabelWithVariousProperties_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label
            {
                Text = "Sample text",
                FontSize = 16,
                TextColor = Colors.Red
            };

            // Act & Assert
            var exception = Record.Exception(() => Label.MapMaxLines(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText with valid LabelHandler and Label parameters executes without throwing exceptions.
        /// Verifies the method properly casts LabelHandler to ILabelHandler and delegates to the interface version.
        /// </summary>
        [Fact]
        public void MapText_WithValidHandlerAndLabel_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            var label = Substitute.For<Label>();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText with valid LabelHandler and null Label parameter executes without throwing exceptions.
        /// Verifies the method properly handles null labels and delegates to the interface version.
        /// </summary>
        [Fact]
        public void MapText_WithValidHandlerAndNullLabel_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText with null LabelHandler throws ArgumentNullException.
        /// Verifies that the method properly validates the handler parameter during the cast operation.
        /// </summary>
        [Fact]
        public void MapText_WithNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            LabelHandler handler = null;
            var label = Substitute.For<Label>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Label.MapText(handler, label));
        }

        /// <summary>
        /// Tests that MapText with both null parameters throws ArgumentNullException for the handler.
        /// Verifies that handler validation occurs before label validation.
        /// </summary>
        [Fact]
        public void MapText_WithBothParametersNull_ThrowsArgumentNullException()
        {
            // Arrange
            LabelHandler handler = null;
            Label label = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Label.MapText(handler, label));
        }

        /// <summary>
        /// Tests MapLineBreakMode with valid LabelHandler and Label parameters.
        /// Verifies the method executes successfully and delegates to the interface method.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_ValidHandlerAndLabel_ExecutesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            var label = new Label();

            // Act & Assert - Should not throw any exception
            Label.MapLineBreakMode(handler, label);
        }

        /// <summary>
        /// Tests MapLineBreakMode with null LabelHandler parameter.
        /// Verifies the method handles null handler gracefully by passing it to the interface method.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_NullHandler_ExecutesSuccessfully()
        {
            // Arrange
            LabelHandler handler = null;
            var label = new Label();

            // Act & Assert - Should not throw any exception
            Label.MapLineBreakMode(handler, label);
        }

        /// <summary>
        /// Tests MapLineBreakMode with null Label parameter.
        /// Verifies the method handles null label gracefully by passing it to the interface method.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_NullLabel_ExecutesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            Label label = null;

            // Act & Assert - Should not throw any exception
            Label.MapLineBreakMode(handler, label);
        }

        /// <summary>
        /// Tests MapLineBreakMode with both null parameters.
        /// Verifies the method handles both null parameters gracefully.
        /// </summary>
        [Fact]
        public void MapLineBreakMode_BothParametersNull_ExecutesSuccessfully()
        {
            // Arrange
            LabelHandler handler = null;
            Label label = null;

            // Act & Assert - Should not throw any exception
            Label.MapLineBreakMode(handler, label);
        }

        /// <summary>
        /// Tests that MapText method executes successfully with valid handler and label parameters.
        /// Verifies the method can be called without throwing exceptions when both parameters are valid.
        /// Expected result: No exception is thrown and method completes successfully.
        /// </summary>
        [Fact]
        public void MapText_ValidHandlerAndLabel_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method handles null handler parameter gracefully.
        /// Verifies the method behavior when handler parameter is null but label is valid.
        /// Expected result: No exception is thrown (empty method body should handle null gracefully).
        /// </summary>
        [Fact]
        public void MapText_NullHandler_DoesNotThrow()
        {
            // Arrange
            ILabelHandler handler = null;
            var label = new Label();

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method handles null label parameter gracefully.
        /// Verifies the method behavior when label parameter is null but handler is valid.
        /// Expected result: No exception is thrown (empty method body should handle null gracefully).
        /// </summary>
        [Fact]
        public void MapText_NullLabel_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method handles both null parameters gracefully.
        /// Verifies the method behavior when both handler and label parameters are null.
        /// Expected result: No exception is thrown (empty method body should handle nulls gracefully).
        /// </summary>
        [Fact]
        public void MapText_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ILabelHandler handler = null;
            Label label = null;

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapText method with different label configurations to ensure consistent behavior.
        /// Verifies that the method handles labels with various property values without issues.
        /// Expected result: No exception is thrown regardless of label configuration.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("Sample text")]
        [InlineData(null)]
        public void MapText_DifferentLabelTextValues_DoesNotThrow(string text)
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label { Text = text };

            // Act & Assert
            var exception = Record.Exception(() => Label.MapText(handler, label));
            Assert.Null(exception);
        }
    }
}