using System;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class PickerHandlerTests
    {
        /// <summary>
        /// Tests that MapReload method executes successfully with valid handler and picker parameters.
        /// Verifies the obsolete method can be called without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapReload_WithValidParameters_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var picker = Substitute.For<IPicker>();
            object args = new object();

            // Act & Assert
#pragma warning disable CS0618 // Type or member is obsolete
            var exception = Record.Exception(() => PickerHandler.MapReload(handler, picker, args));
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapReload method executes successfully with null args parameter.
        /// Verifies the method handles nullable args parameter correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapReload_WithNullArgs_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var picker = Substitute.For<IPicker>();
            object args = null;

            // Act & Assert
#pragma warning disable CS0618 // Type or member is obsolete
            var exception = Record.Exception(() => PickerHandler.MapReload(handler, picker, args));
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapReload method with null handler parameter.
        /// Verifies the behavior when handler parameter is null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapReload_WithNullHandler_ExecutesWithoutException()
        {
            // Arrange
            IPickerHandler handler = null;
            var picker = Substitute.For<IPicker>();
            object args = new object();

            // Act & Assert
#pragma warning disable CS0618 // Type or member is obsolete
            var exception = Record.Exception(() => PickerHandler.MapReload(handler, picker, args));
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapReload method with null picker parameter.
        /// Verifies the behavior when picker parameter is null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapReload_WithNullPicker_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            IPicker picker = null;
            object args = new object();

            // Act & Assert
#pragma warning disable CS0618 // Type or member is obsolete
            var exception = Record.Exception(() => PickerHandler.MapReload(handler, picker, args));
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapReload method with all parameters null.
        /// Verifies the behavior when all parameters are null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapReload_WithAllParametersNull_ExecutesWithoutException()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker picker = null;
            object args = null;

            // Act & Assert
#pragma warning disable CS0618 // Type or member is obsolete
            var exception = Record.Exception(() => PickerHandler.MapReload(handler, picker, args));
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapReload method with various args parameter types.
        /// Verifies the method handles different object types for args parameter.
        /// </summary>
        [Theory]
        [InlineData("string argument")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapReload_WithVariousArgsTypes_ExecutesWithoutException(object args)
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var picker = Substitute.For<IPicker>();

            // Act & Assert
#pragma warning disable CS0618 // Type or member is obsolete
            var exception = Record.Exception(() => PickerHandler.MapReload(handler, picker, args));
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitleColor method can be called with valid handler and view parameters without throwing exceptions.
        /// The method has an empty implementation, so it should complete successfully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitleColor_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTitleColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitleColor method can be called with null handler parameter.
        /// Since the method implementation is empty, it should not throw an exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitleColor_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTitleColor(null, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitleColor method can be called with null view parameter.
        /// Since the method implementation is empty, it should not throw an exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitleColor_WithNullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTitleColor(handler, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTitleColor method can be called with both null parameters.
        /// Since the method implementation is empty, it should not throw an exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTitleColor_WithBothParametersNull_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTitleColor(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedIndex does not throw when both handler and view parameters are null.
        /// This verifies the method handles null inputs gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapSelectedIndex_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker view = null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapSelectedIndex(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedIndex does not throw when handler parameter is null but view is provided.
        /// This verifies the method handles partial null inputs gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapSelectedIndex_HandlerNullViewProvided_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapSelectedIndex(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedIndex does not throw when view parameter is null but handler is provided.
        /// This verifies the method handles partial null inputs gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapSelectedIndex_ViewNullHandlerProvided_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = Substitute.For<IPickerHandler>();
            IPicker view = null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapSelectedIndex(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedIndex does not throw when both valid handler and view parameters are provided.
        /// This verifies the method executes successfully with valid inputs.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapSelectedIndex_ValidParameters_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = Substitute.For<IPickerHandler>();
            IPicker view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapSelectedIndex(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedIndex completes execution successfully with various parameter combinations.
        /// This parameterized test verifies the method behavior across multiple input scenarios.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // Both parameters provided
        [InlineData(true, false)]  // Handler provided, view null
        [InlineData(false, true)]  // Handler null, view provided
        [InlineData(false, false)] // Both parameters null
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapSelectedIndex_ParameterCombinations_CompletesSuccessfully(bool provideHandler, bool provideView)
        {
            // Arrange
            IPickerHandler handler = provideHandler ? Substitute.For<IPickerHandler>() : null;
            IPicker view = provideView ? Substitute.For<IPicker>() : null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapSelectedIndex(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCharacterSpacing completes successfully with valid handler and view parameters.
        /// Verifies the method executes without throwing exceptions for normal input conditions.
        /// Expected result: Method completes without exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCharacterSpacing_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapCharacterSpacing(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCharacterSpacing handles null handler parameter gracefully.
        /// Verifies the method behavior when handler parameter is null.
        /// Expected result: Method completes without throwing exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCharacterSpacing_NullHandler_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = null;
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapCharacterSpacing(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCharacterSpacing handles null view parameter gracefully.
        /// Verifies the method behavior when view parameter is null.
        /// Expected result: Method completes without throwing exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCharacterSpacing_NullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            IPicker view = null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapCharacterSpacing(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCharacterSpacing handles both null parameters gracefully.
        /// Verifies the method behavior when both handler and view parameters are null.
        /// Expected result: Method completes without throwing exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCharacterSpacing_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker view = null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapCharacterSpacing(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapCharacterSpacing with various parameter combinations to ensure consistent behavior.
        /// Verifies the method handles different combinations of null and valid parameters.
        /// Expected result: All combinations complete without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // Both valid
        [InlineData(true, false)]  // Handler valid, view null
        [InlineData(false, true)]  // Handler null, view valid
        [InlineData(false, false)] // Both null
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapCharacterSpacing_VariousParameterCombinations_DoesNotThrow(bool handlerValid, bool viewValid)
        {
            // Arrange
            var handler = handlerValid ? Substitute.For<IPickerHandler>() : null;
            var view = viewValid ? Substitute.For<IPicker>() : null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapCharacterSpacing(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFont completes successfully when called with valid handler and view parameters.
        /// This test ensures the method executes without throwing exceptions.
        /// Expected result: Method completes without exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapFont_WithValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act & Assert (should not throw)
            PickerHandler.MapFont(handler, view);
        }

        /// <summary>
        /// Tests that MapFont handles null handler parameter.
        /// This test verifies the behavior when a null handler is passed to the method.
        /// Expected result: Method completes without exception (no-op implementation).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapFont_WithNullHandler_CompletesSuccessfully()
        {
            // Arrange
            IPickerHandler handler = null;
            var view = Substitute.For<IPicker>();

            // Act & Assert (should not throw)
            PickerHandler.MapFont(handler, view);
        }

        /// <summary>
        /// Tests that MapFont handles null view parameter.
        /// This test verifies the behavior when a null view is passed to the method.
        /// Expected result: Method completes without exception (no-op implementation).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapFont_WithNullView_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            IPicker view = null;

            // Act & Assert (should not throw)
            PickerHandler.MapFont(handler, view);
        }

        /// <summary>
        /// Tests that MapFont handles both null parameters.
        /// This test verifies the behavior when both handler and view parameters are null.
        /// Expected result: Method completes without exception (no-op implementation).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapFont_WithBothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker view = null;

            // Act & Assert (should not throw)
            PickerHandler.MapFont(handler, view);
        }

        /// <summary>
        /// Tests that MapFont can be called multiple times without side effects.
        /// This test ensures the method is idempotent and can be safely called repeatedly.
        /// Expected result: Multiple calls complete without exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapFont_CalledMultipleTimes_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act & Assert (should not throw on multiple calls)
            PickerHandler.MapFont(handler, view);
            PickerHandler.MapFont(handler, view);
            PickerHandler.MapFont(handler, view);
        }

        /// <summary>
        /// Tests that MapTextColor executes successfully with valid handler and view parameters.
        /// Verifies the method completes without throwing exceptions for normal usage scenarios.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTextColor_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTextColor handles null handler parameter gracefully.
        /// Verifies the method does not perform null validation and completes execution.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTextColor_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = null;
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTextColor handles null view parameter gracefully.
        /// Verifies the method does not perform null validation and completes execution.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTextColor_WithNullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            IPicker view = null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapTextColor handles both parameters being null gracefully.
        /// Verifies the method does not perform any null validation and completes execution.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapTextColor_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker view = null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapHorizontalTextAlignment executes successfully with valid handler and view parameters.
        /// Verifies the method completes without throwing exceptions when provided with mocked dependencies.
        /// Expected result: Method returns without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapHorizontalTextAlignment_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapHorizontalTextAlignment(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapHorizontalTextAlignment handles null handler parameter gracefully.
        /// Verifies the method behavior when the handler parameter is null.
        /// Expected result: Method returns without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapHorizontalTextAlignment_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            var view = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapHorizontalTextAlignment(null, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapHorizontalTextAlignment handles null view parameter gracefully.
        /// Verifies the method behavior when the view parameter is null.
        /// Expected result: Method returns without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapHorizontalTextAlignment_WithNullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapHorizontalTextAlignment(handler, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapHorizontalTextAlignment handles both null parameters gracefully.
        /// Verifies the method behavior when both handler and view parameters are null.
        /// Expected result: Method returns without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapHorizontalTextAlignment_WithBothParametersNull_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapHorizontalTextAlignment(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapVerticalTextAlignment does not throw when called with valid mocked parameters.
        /// This test verifies the method signature is correct and the method can be invoked successfully.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapVerticalTextAlignment_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var mockHandler = Substitute.For<IPickerHandler>();
            var mockView = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapVerticalTextAlignment(mockHandler, mockView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapVerticalTextAlignment handles null parameters correctly.
        /// Since the method body is empty, null parameters should not cause issues.
        /// Expected result: No exception is thrown for any combination of null parameters.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // Both non-null
        [InlineData(true, false)]  // Handler non-null, view null
        [InlineData(false, true)]  // Handler null, view non-null
        [InlineData(false, false)] // Both null
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapVerticalTextAlignment_WithVariousParameterStates_DoesNotThrow(bool useNonNullHandler, bool useNonNullView)
        {
            // Arrange
            var handler = useNonNullHandler ? Substitute.For<IPickerHandler>() : null;
            var view = useNonNullView ? Substitute.For<IPicker>() : null;

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapVerticalTextAlignment(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsOpen method executes successfully with valid handler and picker parameters.
        /// Verifies the method completes without throwing exceptions when called with properly mocked dependencies.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapIsOpen_WithValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var picker = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapIsOpen(handler, picker));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsOpen method handles null handler parameter without throwing exceptions.
        /// Verifies the method's behavior when the handler parameter is null.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapIsOpen_WithNullHandler_CompletesSuccessfully()
        {
            // Arrange
            var picker = Substitute.For<IPicker>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapIsOpen(null, picker));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsOpen method handles null picker parameter without throwing exceptions.
        /// Verifies the method's behavior when the picker parameter is null.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapIsOpen_WithNullPicker_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();

            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapIsOpen(handler, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsOpen method handles both null handler and picker parameters without throwing exceptions.
        /// Verifies the method's behavior when both parameters are null.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapIsOpen_WithBothParametersNull_CompletesSuccessfully()
        {
            // Act & Assert
            var exception = Record.Exception(() => PickerHandler.MapIsOpen(null, null));
            Assert.Null(exception);
        }
    }
}