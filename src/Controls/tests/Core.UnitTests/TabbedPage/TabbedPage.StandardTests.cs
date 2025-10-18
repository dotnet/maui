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
    public partial class TabbedPageTests
    {
        /// <summary>
        /// Tests that MapBarBackgroundColor does not throw when called with valid handler and view parameters.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapBarBackgroundColor_ValidHandlerAndView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackgroundColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackgroundColor does not throw when called with null handler parameter.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapBarBackgroundColor_NullHandler_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackgroundColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackgroundColor does not throw when called with null view parameter.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapBarBackgroundColor_NullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackgroundColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackgroundColor does not throw when called with both null parameters.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapBarBackgroundColor_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackgroundColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarTextColor completes successfully with valid handler and view parameters.
        /// Input: Valid ITabbedViewHandler and TabbedPage instances.
        /// Expected: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapBarTextColor_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarTextColor handles null handler parameter without throwing exceptions.
        /// Input: Null handler, valid TabbedPage instance.
        /// Expected: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapBarTextColor_NullHandler_CompletesSuccessfully()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarTextColor handles null view parameter without throwing exceptions.
        /// Input: Valid ITabbedViewHandler, null TabbedPage.
        /// Expected: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapBarTextColor_NullView_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarTextColor handles both null parameters without throwing exceptions.
        /// Input: Null handler and null TabbedPage.
        /// Expected: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapBarTextColor_BothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarTextColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapUnselectedTabColor method can be called with valid parameters without throwing exceptions.
        /// Verifies the method handles valid ITabbedViewHandler and TabbedPage instances correctly.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapUnselectedTabColor_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapUnselectedTabColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapUnselectedTabColor method handles null handler parameter.
        /// Verifies the method behavior when ITabbedViewHandler parameter is null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapUnselectedTabColor_NullHandler_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapUnselectedTabColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapUnselectedTabColor method handles null view parameter.
        /// Verifies the method behavior when TabbedPage parameter is null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapUnselectedTabColor_NullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapUnselectedTabColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapUnselectedTabColor method handles both parameters being null.
        /// Verifies the method behavior when both ITabbedViewHandler and TabbedPage parameters are null.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapUnselectedTabColor_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapUnselectedTabColor(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsSource does not throw an exception when called with valid parameters.
        /// Input conditions: Valid ITabbedViewHandler mock and TabbedPage instance.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void MapItemsSource_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemsSource(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsSource does not throw an exception when called with null handler.
        /// Input conditions: Null handler parameter and valid TabbedPage instance.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void MapItemsSource_NullHandler_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemsSource(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsSource does not throw an exception when called with null view.
        /// Input conditions: Valid ITabbedViewHandler mock and null view parameter.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void MapItemsSource_NullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemsSource(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemsSource does not throw an exception when called with both null parameters.
        /// Input conditions: Null handler and null view parameters.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void MapItemsSource_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemsSource(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate does not throw when called with valid handler and view parameters.
        /// Input conditions: Valid ITabbedViewHandler mock and valid TabbedPage instance.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemTemplate(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate does not throw when called with null handler parameter.
        /// Input conditions: Null handler and valid TabbedPage instance.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemTemplate(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate does not throw when called with null view parameter.
        /// Input conditions: Valid ITabbedViewHandler mock and null view.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_WithNullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemTemplate(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapItemTemplate does not throw when called with both null parameters.
        /// Input conditions: Null handler and null view.
        /// Expected result: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapItemTemplate_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapItemTemplate(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedItem executes successfully with valid handler and view parameters.
        /// Verifies that the method can be called without throwing exceptions when provided with valid inputs.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapSelectedItem_ValidHandlerAndView_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapSelectedItem(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedItem handles null handler parameter.
        /// Verifies the method behavior when the handler parameter is null while view is valid.
        /// Expected result: Method executes without throwing exceptions (empty method body).
        /// </summary>
        [Fact]
        public void MapSelectedItem_NullHandler_ExecutesWithoutException()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapSelectedItem(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedItem handles null view parameter.
        /// Verifies the method behavior when the view parameter is null while handler is valid.
        /// Expected result: Method executes without throwing exceptions (empty method body).
        /// </summary>
        [Fact]
        public void MapSelectedItem_NullView_ExecutesWithoutException()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapSelectedItem(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapSelectedItem handles both null parameters.
        /// Verifies the method behavior when both handler and view parameters are null.
        /// Expected result: Method executes without throwing exceptions (empty method body).
        /// </summary>
        [Fact]
        public void MapSelectedItem_BothParametersNull_ExecutesWithoutException()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapSelectedItem(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCurrentPage completes successfully with valid handler and view parameters.
        /// This test verifies the method can be called without throwing exceptions.
        /// Expected result: Method executes without throwing any exceptions.
        /// </summary>
        [Fact]
        public void MapCurrentPage_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapCurrentPage(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCurrentPage handles null handler parameter.
        /// This test verifies the method behavior when handler parameter is null.
        /// Expected result: Method should handle null handler appropriately.
        /// </summary>
        [Fact]
        public void MapCurrentPage_NullHandler_HandlesGracefully()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapCurrentPage(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCurrentPage handles null view parameter.
        /// This test verifies the method behavior when view parameter is null.
        /// Expected result: Method should handle null view appropriately.
        /// </summary>
        [Fact]
        public void MapCurrentPage_NullView_HandlesGracefully()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapCurrentPage(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapCurrentPage handles both parameters being null.
        /// This test verifies the method behavior when both handler and view parameters are null.
        /// Expected result: Method should handle both null parameters appropriately.
        /// </summary>
        [Fact]
        public void MapCurrentPage_BothParametersNull_HandlesGracefully()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapCurrentPage(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackground method executes successfully with valid handler and view parameters.
        /// </summary>
        [Fact]
        public void MapBarBackground_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackground(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackground method handles null handler parameter gracefully.
        /// </summary>
        [Fact]
        public void MapBarBackground_NullHandler_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            var view = new TabbedPage();

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackground(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackground method handles null view parameter gracefully.
        /// </summary>
        [Fact]
        public void MapBarBackground_NullView_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<ITabbedViewHandler>();
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackground(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapBarBackground method handles both null parameters gracefully.
        /// </summary>
        [Fact]
        public void MapBarBackground_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            ITabbedViewHandler handler = null;
            TabbedPage view = null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackground(handler, view));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapBarBackground method with various parameter combinations to ensure consistent behavior.
        /// Input conditions: Different combinations of null and valid parameters.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // Valid handler, valid view
        [InlineData(true, false)]  // Valid handler, null view
        [InlineData(false, true)]  // Null handler, valid view
        [InlineData(false, false)] // Null handler, null view
        public void MapBarBackground_ParameterCombinations_DoesNotThrow(bool hasHandler, bool hasView)
        {
            // Arrange
            var handler = hasHandler ? Substitute.For<ITabbedViewHandler>() : null;
            var view = hasView ? new TabbedPage() : null;

            // Act & Assert
            var exception = Record.Exception(() => TabbedPage.MapBarBackground(handler, view));
            Assert.Null(exception);
        }
    }
}