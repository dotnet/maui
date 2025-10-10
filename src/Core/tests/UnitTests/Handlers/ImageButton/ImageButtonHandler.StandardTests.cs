using System;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class ImageButtonHandlerTests
    {
        /// <summary>
        /// Tests that MapStrokeColor can be called with valid handler and buttonStroke parameters without throwing exceptions.
        /// This test validates the basic functionality with properly mocked interfaces.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeColor can be called with null handler parameter without throwing exceptions.
        /// This test validates the current behavior when handler is null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            IImageButtonHandler handler = null;
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeColor can be called with null buttonStroke parameter without throwing exceptions.
        /// This test validates the current behavior when buttonStroke is null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_WithNullButtonStroke_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            IButtonStroke buttonStroke = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeColor can be called with both parameters null without throwing exceptions.
        /// This test validates the current behavior when both parameters are null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            IImageButtonHandler handler = null;
            IButtonStroke buttonStroke = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeColor can be called multiple times with the same parameters without issues.
        /// This test validates that the method is idempotent and can be called repeatedly.
        /// Expected result: All calls execute without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception1 = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));
            var exception2 = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));
            var exception3 = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests that MapStrokeColor can be called with different mock configurations without issues.
        /// This test validates the method behavior with various mock setups.
        /// Expected result: Method executes without throwing any exceptions regardless of mock configuration.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(5.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_WithVariousStrokeThickness_DoesNotThrow(double strokeThickness)
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var buttonStroke = Substitute.For<IButtonStroke>();
            buttonStroke.StrokeThickness.Returns(strokeThickness);

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeColor can be called with various corner radius values without issues.
        /// This test validates the method behavior with different corner radius configurations.
        /// Expected result: Method executes without throwing any exceptions regardless of corner radius value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeColor_WithVariousCornerRadius_DoesNotThrow(int cornerRadius)
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var buttonStroke = Substitute.For<IButtonStroke>();
            buttonStroke.CornerRadius.Returns(cornerRadius);

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeColor(handler, buttonStroke));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeThickness method can be called with valid handler and buttonStroke parameters
        /// without throwing any exceptions. This validates the method accepts valid inputs correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeThickness_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeThickness(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeThickness method can be called with null handler parameter
        /// without throwing any exceptions. This validates the method handles null handler gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeThickness_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            IImageButtonHandler handler = null;
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeThickness(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeThickness method can be called with null buttonStroke parameter
        /// without throwing any exceptions. This validates the method handles null buttonStroke gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeThickness_WithNullButtonStroke_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            IButtonStroke buttonStroke = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeThickness(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapStrokeThickness method can be called with both parameters null
        /// without throwing any exceptions. This validates the method handles all null inputs gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapStrokeThickness_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            IImageButtonHandler handler = null;
            IButtonStroke buttonStroke = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapStrokeThickness(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCornerRadius method executes without throwing exceptions when provided with valid handler and buttonStroke parameters.
        /// This test ensures the method signature is correct and the method can be called successfully.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCornerRadius_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapCornerRadius(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCornerRadius method handles null handler parameter without throwing exceptions.
        /// Since the method has an empty implementation, it should not perform null validation.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCornerRadius_NullHandler_DoesNotThrow()
        {
            // Arrange
            IImageButtonHandler handler = null;
            var buttonStroke = Substitute.For<IButtonStroke>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapCornerRadius(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCornerRadius method handles null buttonStroke parameter without throwing exceptions.
        /// Since the method has an empty implementation, it should not perform null validation.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCornerRadius_NullButtonStroke_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            IButtonStroke buttonStroke = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapCornerRadius(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCornerRadius method handles both null parameters without throwing exceptions.
        /// Since the method has an empty implementation, it should not perform any parameter validation.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCornerRadius_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IImageButtonHandler handler = null;
            IButtonStroke buttonStroke = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapCornerRadius(handler, buttonStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapPadding completes successfully when both handler and imageButton parameters are valid non-null values.
        /// This verifies the method executes without throwing exceptions for normal input conditions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapPadding_WithValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            var imageButton = Substitute.For<IImageButton>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapPadding(handler, imageButton));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapPadding handles null handler parameter appropriately.
        /// Since the method body is empty, it should complete without throwing regardless of null handler.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapPadding_WithNullHandler_CompletesSuccessfully()
        {
            // Arrange
            IImageButtonHandler handler = null;
            var imageButton = Substitute.For<IImageButton>();

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapPadding(handler, imageButton));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapPadding handles null imageButton parameter appropriately.
        /// Since the method body is empty, it should complete without throwing regardless of null imageButton.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapPadding_WithNullImageButton_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IImageButtonHandler>();
            IImageButton imageButton = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapPadding(handler, imageButton));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapPadding handles both parameters being null appropriately.
        /// Since the method body is empty, it should complete without throwing even when both parameters are null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapPadding_WithBothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            IImageButtonHandler handler = null;
            IImageButton imageButton = null;

            // Act & Assert
            var exception = Record.Exception(() => ImageButtonHandler.MapPadding(handler, imageButton));
            Assert.Null(exception);
        }
    }
}

namespace Core.UnitTests.Handlers.ImageButton
{
    /// <summary>
    /// Unit tests for ImageButtonImageSourcePartSetter class.
    /// </summary>
    public partial class ImageButtonImageSourcePartSetterTests
    {
    }
}