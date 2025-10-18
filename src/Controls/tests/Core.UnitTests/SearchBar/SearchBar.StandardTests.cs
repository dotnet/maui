#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for SearchBar.MapText method
    /// </summary>
    public partial class SearchBarTests
    {
        /// <summary>
        /// Tests that MapText method completes successfully with various parameter combinations.
        /// Verifies the method handles null and valid parameters without throwing exceptions.
        /// </summary>
        /// <param name="handlerIsNull">Whether the handler parameter should be null</param>
        /// <param name="searchBarIsNull">Whether the searchBar parameter should be null</param>
        [Theory]
        [InlineData(true, true)]   // Both null
        [InlineData(true, false)]  // Handler null, SearchBar valid
        [InlineData(false, true)]  // Handler valid, SearchBar null
        [InlineData(false, false)] // Both valid
        public void MapText_VariousParameterCombinations_CompletesWithoutException(bool handlerIsNull, bool searchBarIsNull)
        {
            // Arrange
            ISearchBarHandler handler = handlerIsNull ? null : Substitute.For<ISearchBarHandler>();
            SearchBar searchBar = searchBarIsNull ? null : new SearchBar();

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => SearchBar.MapText(handler, searchBar));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with valid parameters completes successfully.
        /// Verifies the method executes normally with non-null parameters.
        /// </summary>
        [Fact]
        public void MapText_WithValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var mockHandler = Substitute.For<ISearchBarHandler>();
            var searchBar = new SearchBar { Text = "test search" };

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => SearchBar.MapText(mockHandler, searchBar));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with null handler parameter does not throw exception.
        /// Verifies the method handles null handler gracefully.
        /// </summary>
        [Fact]
        public void MapText_WithNullHandler_DoesNotThrowException()
        {
            // Arrange
            ISearchBarHandler handler = null;
            var searchBar = new SearchBar { Text = "test" };

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => SearchBar.MapText(handler, searchBar));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with null searchBar parameter does not throw exception.
        /// Verifies the method handles null searchBar gracefully.
        /// </summary>
        [Fact]
        public void MapText_WithNullSearchBar_DoesNotThrowException()
        {
            // Arrange
            var mockHandler = Substitute.For<ISearchBarHandler>();
            SearchBar searchBar = null;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => SearchBar.MapText(mockHandler, searchBar));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with both null parameters does not throw exception.
        /// Verifies the method handles both null parameters gracefully.
        /// </summary>
        [Fact]
        public void MapText_WithBothParametersNull_DoesNotThrowException()
        {
            // Arrange
            ISearchBarHandler handler = null;
            SearchBar searchBar = null;

            // Act & Assert - Should not throw any exception
            var exception = Record.Exception(() => SearchBar.MapText(handler, searchBar));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method with SearchBarHandler parameter successfully delegates to the ISearchBarHandler overload
        /// when provided with valid handler and searchBar instances.
        /// </summary>
        [Fact]
        public void MapText_ValidHandlerAndSearchBar_ExecutesWithoutException()
        {
            // Arrange
            var handler = new SearchBarHandler();
            var searchBar = new SearchBar();

            // Act & Assert
            var exception = Record.Exception(() => SearchBar.MapText(handler, searchBar));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method throws ArgumentNullException when handler parameter is null.
        /// </summary>
        [Fact]
        public void MapText_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            SearchBarHandler handler = null;
            var searchBar = new SearchBar();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SearchBar.MapText(handler, searchBar));
        }

        /// <summary>
        /// Tests that MapText method throws ArgumentNullException when searchBar parameter is null.
        /// </summary>
        [Fact]
        public void MapText_NullSearchBar_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = new SearchBarHandler();
            SearchBar searchBar = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SearchBar.MapText(handler, searchBar));
        }

        /// <summary>
        /// Tests that MapText method throws ArgumentNullException when both parameters are null.
        /// </summary>
        [Fact]
        public void MapText_BothParametersNull_ThrowsArgumentNullException()
        {
            // Arrange
            SearchBarHandler handler = null;
            SearchBar searchBar = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SearchBar.MapText(handler, searchBar));
        }
    }
}