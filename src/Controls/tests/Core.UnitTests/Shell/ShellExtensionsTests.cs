#nullable disable

using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ShellExtensionsTests
    {
        /// <summary>
        /// Tests that SearchForPart returns null when GetItems() returns an empty collection.
        /// This test ensures coverage of the return null statement when no items exist to search.
        /// </summary>
        [Fact]
        public void SearchForPart_EmptyItemsCollection_ReturnsNull()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var emptyItems = new ReadOnlyCollection<ShellItem>(new ShellItem[0]);
            shellController.GetItems().Returns(emptyItems);

            Func<BaseShellItem, bool> searchBy = item => true;

            // Act
            var result = ShellExtensions.SearchForPart(shellController, searchBy);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SearchForPart returns null when items exist but none match the search criteria.
        /// This test ensures coverage of the return null statement when all recursive searches return null.
        /// </summary>
        [Fact]
        public void SearchForPart_ItemsExistButNoMatches_ReturnsNull()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem1 = Substitute.For<ShellItem>();
            var mockShellItem2 = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem1, mockShellItem2 });
            shellController.GetItems().Returns(items);

            // Use a predicate that never matches any item
            Func<BaseShellItem, bool> searchBy = item => false;

            // Act
            var result = ShellExtensions.SearchForPart(shellController, searchBy);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SearchForPart returns the first matching item when multiple items exist.
        /// This test validates the method returns immediately upon finding the first match.
        /// </summary>
        [Fact]
        public void SearchForPart_MultipleItemsFirstMatches_ReturnsFirstMatch()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem1 = Substitute.For<ShellItem>();
            var mockShellItem2 = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem1, mockShellItem2 });
            shellController.GetItems().Returns(items);

            // Predicate that matches the first item
            Func<BaseShellItem, bool> searchBy = item => ReferenceEquals(item, mockShellItem1);

            // Act
            var result = ShellExtensions.SearchForPart(shellController, searchBy);

            // Assert
            Assert.Equal(mockShellItem1, result);
        }

        /// <summary>
        /// Tests that SearchForPart returns a later matching item when earlier items don't match.
        /// This test validates the method continues searching through all items until a match is found.
        /// </summary>
        [Fact]
        public void SearchForPart_LaterItemMatches_ReturnsMatchingItem()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem1 = Substitute.For<ShellItem>();
            var mockShellItem2 = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem1, mockShellItem2 });
            shellController.GetItems().Returns(items);

            // Predicate that only matches the second item
            Func<BaseShellItem, bool> searchBy = item => ReferenceEquals(item, mockShellItem2);

            // Act
            var result = ShellExtensions.SearchForPart(shellController, searchBy);

            // Assert
            Assert.Equal(mockShellItem2, result);
        }

        /// <summary>
        /// Tests that SearchForPart throws ArgumentNullException when searchBy parameter is null.
        /// This test validates proper null parameter handling for the search predicate.
        /// </summary>
        [Fact]
        public void SearchForPart_NullSearchByPredicate_ThrowsArgumentNullException()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem });
            shellController.GetItems().Returns(items);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ShellExtensions.SearchForPart(shellController, null));
        }

        /// <summary>
        /// Tests that SearchForPart handles null return from GetItems() gracefully.
        /// This test validates resilience when the shell controller returns null instead of a collection.
        /// </summary>
        [Fact]
        public void SearchForPart_GetItemsReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            shellController.GetItems().Returns((ReadOnlyCollection<ShellItem>)null);

            Func<BaseShellItem, bool> searchBy = item => true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ShellExtensions.SearchForPart(shellController, searchBy));
        }

        /// <summary>
        /// Tests that SearchForPart handles exceptions thrown by the search predicate.
        /// This test validates error handling when the search predicate throws during execution.
        /// </summary>
        [Fact]
        public void SearchForPart_SearchByPredicateThrows_PropagatesException()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem });
            shellController.GetItems().Returns(items);

            var expectedException = new InvalidOperationException("Test exception");
            Func<BaseShellItem, bool> searchBy = item => throw expectedException;

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() =>
                ShellExtensions.SearchForPart(shellController, searchBy));
            Assert.Equal(expectedException.Message, actualException.Message);
        }

        /// <summary>
        /// Tests that SearchForPart works correctly with a single item that matches.
        /// This test validates the basic functionality with minimal input.
        /// </summary>
        [Fact]
        public void SearchForPart_SingleItemMatches_ReturnsItem()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem });
            shellController.GetItems().Returns(items);

            Func<BaseShellItem, bool> searchBy = item => true;

            // Act
            var result = ShellExtensions.SearchForPart(shellController, searchBy);

            // Assert
            Assert.Equal(mockShellItem, result);
        }

        /// <summary>
        /// Tests that SearchForPart returns null when single item doesn't match.
        /// This test validates the null return path with a single non-matching item.
        /// </summary>
        [Fact]
        public void SearchForPart_SingleItemDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var shellController = Substitute.For<IShellController>();
            var mockShellItem = Substitute.For<ShellItem>();
            var items = new ReadOnlyCollection<ShellItem>(new[] { mockShellItem });
            shellController.GetItems().Returns(items);

            Func<BaseShellItem, bool> searchBy = item => false;

            // Act
            var result = ShellExtensions.SearchForPart(shellController, searchBy);

            // Assert
            Assert.Null(result);
        }
    }
}
