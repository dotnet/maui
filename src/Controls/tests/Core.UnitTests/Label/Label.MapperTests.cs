#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the Label.MapTextType method
    /// </summary>
    public partial class LabelTests
    {
        /// <summary>
        /// Tests that MapTextType throws ArgumentNullException when handler is null.
        /// Verifies that null handler parameter is properly validated.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void MapTextType_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var label = new Label();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Label.MapTextType(null, label));
        }

        /// <summary>
        /// Tests that MapTextType throws ArgumentNullException when label is null.
        /// Verifies that null label parameter is properly validated.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void MapTextType_NullLabel_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Label.MapTextType(handler, null));
        }

        /// <summary>
        /// Tests that MapTextType throws ArgumentNullException when both parameters are null.
        /// Verifies that null parameter validation works with multiple null parameters.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void MapTextType_BothParametersNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Label.MapTextType(null, null));
        }

        /// <summary>
        /// Tests that MapTextType calls UpdateValue with "Text" when label has no formatted text spans.
        /// Verifies the delegation to MapTextOrFormattedText works correctly for plain text.
        /// Expected result: UpdateValue should be called with "Text" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithNoFormattedTextSpans_CallsUpdateValueWithText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label { Text = "Plain text" };

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("Text");
        }

        /// <summary>
        /// Tests that MapTextType calls UpdateValue with "FormattedText" when label has formatted text spans.
        /// Verifies the delegation to MapTextOrFormattedText works correctly for formatted text.
        /// Expected result: UpdateValue should be called with "FormattedText" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithFormattedTextSpans_CallsUpdateValueWithFormattedText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Formatted text" });
            label.FormattedText = formattedString;

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("FormattedText");
        }

        /// <summary>
        /// Tests that MapTextType works correctly with empty label (no text or formatted text).
        /// Verifies the behavior when label is in default state.
        /// Expected result: UpdateValue should be called with "Text" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_EmptyLabel_CallsUpdateValueWithText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("Text");
        }

        /// <summary>
        /// Tests that MapTextType works correctly with label having empty text.
        /// Verifies the behavior when label text is empty string.
        /// Expected result: UpdateValue should be called with "Text" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithEmptyText_CallsUpdateValueWithText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label { Text = string.Empty };

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("Text");
        }

        /// <summary>
        /// Tests that MapTextType works correctly with label having whitespace-only text.
        /// Verifies the behavior when label text contains only whitespace characters.
        /// Expected result: UpdateValue should be called with "Text" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithWhitespaceText_CallsUpdateValueWithText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label { Text = "   " };

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("Text");
        }

        /// <summary>
        /// Tests that MapTextType works correctly with empty FormattedString.
        /// Verifies the behavior when FormattedText is set but has no spans.
        /// Expected result: UpdateValue should be called with "Text" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithEmptyFormattedString_CallsUpdateValueWithText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();
            label.FormattedText = new FormattedString();

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("Text");
        }

        /// <summary>
        /// Tests that MapTextType works correctly with FormattedString having empty span.
        /// Verifies the behavior when FormattedText has spans but they are empty.
        /// Expected result: UpdateValue should be called with "FormattedText" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithEmptySpan_CallsUpdateValueWithFormattedText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = string.Empty });
            label.FormattedText = formattedString;

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("FormattedText");
        }

        /// <summary>
        /// Tests that MapTextType works correctly with multiple spans in FormattedString.
        /// Verifies the behavior when FormattedText has multiple spans.
        /// Expected result: UpdateValue should be called with "FormattedText" parameter.
        /// </summary>
        [Fact]
        public void MapTextType_LabelWithMultipleSpans_CallsUpdateValueWithFormattedText()
        {
            // Arrange
            var handler = Substitute.For<ILabelHandler>();
            var label = new Label();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "First span" });
            formattedString.Spans.Add(new Span { Text = "Second span" });
            label.FormattedText = formattedString;

            // Act
            Label.MapTextType(handler, label);

            // Assert
            handler.Received(1).UpdateValue("FormattedText");
        }

        /// <summary>
        /// Tests that MapTextType returns early when label is connecting handler and makes no UpdateValue calls.
        /// </summary>
        [Fact]
        public void MapTextType_LabelIsConnectingHandler_ReturnsEarlyWithoutUpdateValue()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            var mockHandler = handler.As<ILabelHandler>();
            var label = Substitute.For<Label>();

            label.IsConnectingHandler().Returns(true);
            label.HasFormattedTextSpans.Returns(false);

            // Act
            Label.MapTextType(handler, label);

            // Assert
            mockHandler.DidNotReceive().UpdateValue(Arg.Any<string>());
        }

        /// <summary>
        /// Tests that MapTextType calls UpdateValue with "FormattedText" when label has formatted text spans.
        /// </summary>
        [Fact]
        public void MapTextType_HasFormattedTextSpans_CallsUpdateValueWithFormattedText()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            var mockHandler = handler.As<ILabelHandler>();
            var label = Substitute.For<Label>();

            label.IsConnectingHandler().Returns(false);
            label.HasFormattedTextSpans.Returns(true);

            // Act
            Label.MapTextType(handler, label);

            // Assert
            mockHandler.Received(1).UpdateValue("FormattedText");
        }

        /// <summary>
        /// Tests that MapTextType calls UpdateValue with "Text" when label does not have formatted text spans.
        /// </summary>
        [Fact]
        public void MapTextType_NoFormattedTextSpans_CallsUpdateValueWithText()
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            var mockHandler = handler.As<ILabelHandler>();
            var label = Substitute.For<Label>();

            label.IsConnectingHandler().Returns(false);
            label.HasFormattedTextSpans.Returns(false);

            // Act
            Label.MapTextType(handler, label);

            // Assert
            mockHandler.Received(1).UpdateValue("Text");
        }

        /// <summary>
        /// Tests that MapTextType with valid parameters does not throw any exceptions.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // IsConnectingHandler=true, HasFormattedTextSpans=true
        [InlineData(true, false)]  // IsConnectingHandler=true, HasFormattedTextSpans=false
        [InlineData(false, true)]  // IsConnectingHandler=false, HasFormattedTextSpans=true
        [InlineData(false, false)] // IsConnectingHandler=false, HasFormattedTextSpans=false
        public void MapTextType_ValidParameters_DoesNotThrowException(bool isConnectingHandler, bool hasFormattedTextSpans)
        {
            // Arrange
            var handler = Substitute.For<LabelHandler>();
            var label = Substitute.For<Label>();

            label.IsConnectingHandler().Returns(isConnectingHandler);
            label.HasFormattedTextSpans.Returns(hasFormattedTextSpans);

            // Act & Assert
            var exception = Record.Exception(() => Label.MapTextType(handler, label));
            Assert.Null(exception);
        }
    }
}