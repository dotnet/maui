#nullable disable

#nullable disable
using System;
using System.Collections;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ItemsViewHandlerTests
    {
        /// <summary>
        /// Tests MapItemsSource with valid handler and itemsView parameters.
        /// Verifies that the method completes successfully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsSource_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsSource(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapItemsSource with null handler parameter.
        /// Verifies that the method handles null handler gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsSource_NullHandler_CompletesSuccessfully()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsSource(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapItemsSource with null itemsView parameter.
        /// Verifies that the method handles null itemsView gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsSource_NullItemsView_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsSource(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapItemsSource with both handler and itemsView parameters as null.
        /// Verifies that the method handles null parameters gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsSource_BothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsSource(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapHorizontalScrollBarVisibility with valid handler and itemsView parameters.
        /// Verifies the method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapHorizontalScrollBarVisibility_ValidParameters_CompletesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapHorizontalScrollBarVisibility(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapHorizontalScrollBarVisibility with null handler parameter.
        /// Verifies the method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapHorizontalScrollBarVisibility_NullHandler_CompletesWithoutException()
        {
            // Arrange
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapHorizontalScrollBarVisibility(null, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapHorizontalScrollBarVisibility with null itemsView parameter.
        /// Verifies the method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapHorizontalScrollBarVisibility_NullItemsView_CompletesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapHorizontalScrollBarVisibility(handler, null));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapHorizontalScrollBarVisibility with both parameters null.
        /// Verifies the method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapHorizontalScrollBarVisibility_BothParametersNull_CompletesWithoutException()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapHorizontalScrollBarVisibility(null, null));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapVerticalScrollBarVisibility executes successfully with valid handler and itemsView parameters.
        /// This verifies the method can be called without throwing exceptions under normal conditions.
        /// </summary>
        [Fact]
        public void MapVerticalScrollBarVisibility_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapVerticalScrollBarVisibility(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapVerticalScrollBarVisibility executes successfully when handler parameter is null.
        /// This verifies the method handles null handler gracefully since the method body is empty.
        /// </summary>
        [Fact]
        public void MapVerticalScrollBarVisibility_NullHandler_DoesNotThrow()
        {
            // Arrange
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapVerticalScrollBarVisibility(null, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapVerticalScrollBarVisibility executes successfully when itemsView parameter is null.
        /// This verifies the method handles null itemsView gracefully since the method body is empty.
        /// </summary>
        [Fact]
        public void MapVerticalScrollBarVisibility_NullItemsView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();

            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapVerticalScrollBarVisibility(handler, null));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapVerticalScrollBarVisibility executes successfully when both parameters are null.
        /// This verifies the method handles all-null parameters gracefully since the method body is empty.
        /// </summary>
        [Fact]
        public void MapVerticalScrollBarVisibility_BothParametersNull_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ItemsViewHandler<ItemsView>.MapVerticalScrollBarVisibility(null, null));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate executes successfully with valid handler and itemsView parameters.
        /// This test verifies that the method does not throw any exceptions when called with valid inputs.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_ValidHandlerAndItemsView_DoesNotThrowException()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<TestItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<TestItemsView>.MapItemTemplate(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate executes successfully with null handler parameter.
        /// This test verifies that the method does not throw any exceptions when called with null handler.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_NullHandler_DoesNotThrowException()
        {
            // Arrange
            ItemsViewHandler<TestItemsView> handler = null;
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<TestItemsView>.MapItemTemplate(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate executes successfully with null itemsView parameter.
        /// This test verifies that the method does not throw any exceptions when called with null itemsView.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_NullItemsView_DoesNotThrowException()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<TestItemsView>>();
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<TestItemsView>.MapItemTemplate(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate executes successfully with both null parameters.
        /// This test verifies that the method does not throw any exceptions when called with both null inputs.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_BothParametersNull_DoesNotThrowException()
        {
            // Arrange
            ItemsViewHandler<TestItemsView> handler = null;
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<TestItemsView>.MapItemTemplate(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Test helper class that extends ItemsView to use as concrete type parameter for generic ItemsViewHandler.
        /// </summary>
        private class TestItemsView : ItemsView
        {
        }

        /// <summary>
        /// Tests that MapEmptyView method executes successfully with both valid handler and itemsView parameters.
        /// This test verifies the method completes without throwing exceptions when provided with non-null parameters.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public void MapEmptyView_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyView(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyView method executes successfully when handler parameter is null.
        /// This test verifies the method handles null handler gracefully without throwing exceptions.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public void MapEmptyView_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyView(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyView method executes successfully when itemsView parameter is null.
        /// This test verifies the method handles null itemsView gracefully without throwing exceptions.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public void MapEmptyView_WithNullItemsView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyView(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyView method executes successfully when both parameters are null.
        /// This test verifies the method handles all null parameters gracefully without throwing exceptions.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public void MapEmptyView_WithAllNullParameters_DoesNotThrow()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyView(handler, itemsView));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFlowDirection completes successfully with valid handler and itemsView parameters.
        /// This verifies the method executes without throwing exceptions for normal input conditions.
        /// </summary>
        [Fact]
        public void MapFlowDirection_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapFlowDirection(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFlowDirection completes successfully with null handler parameter.
        /// This verifies the method handles null handler gracefully since the method body is empty.
        /// </summary>
        [Fact]
        public void MapFlowDirection_NullHandler_DoesNotThrow()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapFlowDirection(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFlowDirection completes successfully with null itemsView parameter.
        /// This verifies the method handles null itemsView gracefully since the method body is empty.
        /// </summary>
        [Fact]
        public void MapFlowDirection_NullItemsView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapFlowDirection(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapFlowDirection completes successfully with both null parameters.
        /// This verifies the method handles all null parameters gracefully since the method body is empty.
        /// </summary>
        [Fact]
        public void MapFlowDirection_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapFlowDirection(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyViewTemplate can be called with valid handler and itemsView parameters without throwing exceptions.
        /// Input: Valid mocked handler and itemsView instances.
        /// Expected: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapEmptyViewTemplate_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyViewTemplate(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyViewTemplate can be called with null handler parameter without throwing exceptions.
        /// Input: Null handler and valid itemsView.
        /// Expected: Method executes successfully without throwing any exceptions since the method body is empty.
        /// </summary>
        [Fact]
        public void MapEmptyViewTemplate_NullHandler_DoesNotThrow()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyViewTemplate(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyViewTemplate can be called with null itemsView parameter without throwing exceptions.
        /// Input: Valid handler and null itemsView.
        /// Expected: Method executes successfully without throwing any exceptions since the method body is empty.
        /// </summary>
        [Fact]
        public void MapEmptyViewTemplate_NullItemsView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyViewTemplate(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapEmptyViewTemplate can be called with both null parameters without throwing exceptions.
        /// Input: Null handler and null itemsView.
        /// Expected: Method executes successfully without throwing any exceptions since the method body is empty.
        /// </summary>
        [Fact]
        public void MapEmptyViewTemplate_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapEmptyViewTemplate(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapIsVisible method executes without throwing exceptions for various parameter combinations.
        /// Tests null and valid parameter values to ensure the method handles all input scenarios gracefully.
        /// Expected result: Method completes execution without throwing any exceptions.
        /// </summary>
        /// <param name="handlerIsNull">Whether the handler parameter should be null</param>
        /// <param name="itemsViewIsNull">Whether the itemsView parameter should be null</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void MapIsVisible_VariousParameterCombinations_CompletesWithoutException(bool handlerIsNull, bool itemsViewIsNull)
        {
            // Arrange
            var handler = handlerIsNull ? null : Substitute.For<TestItemsViewHandler>();
            var itemsView = itemsViewIsNull ? null : Substitute.For<ItemsView>();

            // Act & Assert - Should not throw any exceptions
            var exception = Record.Exception(() => TestItemsViewHandler.MapIsVisible(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapIsVisible method with null handler parameter.
        /// Tests that the method can handle null handler gracefully.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapIsVisible_NullHandler_DoesNotThrow()
        {
            // Arrange
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => TestItemsViewHandler.MapIsVisible(null, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapIsVisible method with null itemsView parameter.
        /// Tests that the method can handle null itemsView gracefully.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapIsVisible_NullItemsView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<TestItemsViewHandler>();

            // Act & Assert
            var exception = Record.Exception(() => TestItemsViewHandler.MapIsVisible(handler, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapIsVisible method with both parameters as null.
        /// Tests that the method can handle all null parameters gracefully.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapIsVisible_BothParametersNull_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => TestItemsViewHandler.MapIsVisible(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapIsVisible method with valid parameters.
        /// Tests that the method executes successfully with valid handler and itemsView instances.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapIsVisible_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<TestItemsViewHandler>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => TestItemsViewHandler.MapIsVisible(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsUpdatingScrollMode completes successfully with valid handler and itemsView parameters.
        /// Verifies the method executes without throwing exceptions when provided with non-null valid parameters.
        /// Expected: Method completes execution without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsUpdatingScrollMode_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsUpdatingScrollMode(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsUpdatingScrollMode handles null handler parameter.
        /// Verifies the method behavior when the handler parameter is null.
        /// Expected: Method completes execution without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsUpdatingScrollMode_NullHandler_CompletesSuccessfully()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            var itemsView = Substitute.For<ItemsView>();

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsUpdatingScrollMode(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsUpdatingScrollMode handles null itemsView parameter.
        /// Verifies the method behavior when the itemsView parameter is null.
        /// Expected: Method completes execution without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsUpdatingScrollMode_NullItemsView_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ItemsViewHandler<ItemsView>>();
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsUpdatingScrollMode(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsUpdatingScrollMode handles both null parameters.
        /// Verifies the method behavior when both handler and itemsView parameters are null.
        /// Expected: Method completes execution without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemsUpdatingScrollMode_BothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            ItemsViewHandler<ItemsView> handler = null;
            ItemsView itemsView = null;

            // Act & Assert
            var exception = Record.Exception(() => ItemsViewHandler<ItemsView>.MapItemsUpdatingScrollMode(handler, itemsView));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CreatePlatformView throws NotImplementedException when called.
        /// This verifies the standard implementation behavior for the abstract handler.
        /// </summary>
        [Fact]
        public void CreatePlatformView_WhenCalled_ThrowsNotImplementedException()
        {
            // Arrange
            var mauiContext = Substitute.For<IMauiContext>();
            var itemsView = Substitute.For<ItemsView>();
            var handler = new TestItemsViewHandler(mauiContext);
            handler.SetVirtualView(itemsView);

            // Act & Assert
            var exception = Assert.Throws<NotImplementedException>(() => handler.CallCreatePlatformView());
        }

    }
}