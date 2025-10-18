#nullable disable

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
    [Category("MenuFlyoutSubItem")]
    public class MenuFlyoutSubItemTests :
        MenuTestBase<MenuFlyoutSubItem, IMenuElement, MenuFlyoutItem, MenuFlyoutSubItemHandlerUpdate>
    {
        [Fact]
        public void CommandPropertyChangesEnabled()
        {
            MenuFlyoutItem menuBarItem = new MenuFlyoutItem();

            bool canExecute = true;
            var command = new Command((p) => { }, (p) => p != null && (bool)p);
            menuBarItem.CommandParameter = true;
            menuBarItem.Command = command;

            Assert.True(menuBarItem.IsEnabled);
            menuBarItem.CommandParameter = false;
            Assert.False(menuBarItem.IsEnabled);
            menuBarItem.CommandParameter = true;
            Assert.True(menuBarItem.IsEnabled);
        }

        protected override int GetIndex(MenuFlyoutSubItemHandlerUpdate handlerUpdate) =>
            handlerUpdate.Index;

        protected override IMenuElement GetItem(MenuFlyoutSubItemHandlerUpdate handlerUpdate) =>
            handlerUpdate.MenuElement;

        protected override void SetHandler(Maui.IElement element, List<(string Name, MenuFlyoutSubItemHandlerUpdate? Args)> events)
        {
            element.Handler = CreateMenuFlyoutSubItemHandler((n, h, l, a) => events.Add((n, a)));
        }

        MenuFlyoutSubItemHandler CreateMenuFlyoutSubItemHandler(Action<string, IMenuFlyoutSubItemHandler, IMenuFlyoutSubItem, MenuFlyoutSubItemHandlerUpdate?>? action)
        {
            var handler = new NonThrowingMenuFlyoutSubItemHandler(
                MenuFlyoutSubItemHandler.Mapper,
                new CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler>(MenuFlyoutSubItemHandler.CommandMapper)
                {
                    [nameof(IMenuFlyoutSubItemHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Add), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
                    [nameof(IMenuFlyoutSubItemHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Remove), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
                    [nameof(IMenuFlyoutSubItemHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Clear), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
                    [nameof(IMenuFlyoutSubItemHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Insert), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
                });

            return handler;
        }

        class NonThrowingMenuFlyoutSubItemHandler : MenuFlyoutSubItemHandler
        {
            public NonThrowingMenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper commandMapper)
                : base(mapper, commandMapper)
            {
            }

            protected override object CreatePlatformElement() => new object();
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false, indicating that the MenuFlyoutSubItem collection is modifiable.
        /// This test ensures the collection properly implements the ICollection interface contract for read-only behavior.
        /// </summary>
        [Fact]
        public void IsReadOnly_WhenAccessed_ReturnsFalse()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();

            // Act
            bool isReadOnly = menuFlyoutSubItem.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that Contains returns false when passed a null item.
        /// This verifies the method handles null input gracefully.
        /// </summary>
        [Fact]
        public void Contains_WithNullItem_ReturnsFalse()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();

            // Act
            var result = menuFlyoutSubItem.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the collection is empty.
        /// This verifies behavior when no items have been added to the menu.
        /// </summary>
        [Fact]
        public void Contains_WithEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockItem = Substitute.For<IMenuElement>();

            // Act
            var result = menuFlyoutSubItem.Contains(mockItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the collection.
        /// This verifies the method correctly identifies items that have been added.
        /// </summary>
        [Fact]
        public void Contains_WithExistingItem_ReturnsTrue()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var menuItem = new MenuFlyoutItem();
            menuFlyoutSubItem.Add(menuItem);

            // Act
            var result = menuFlyoutSubItem.Contains(menuItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the collection.
        /// This verifies the method correctly identifies items that are not present.
        /// </summary>
        [Fact]
        public void Contains_WithNonExistingItem_ReturnsFalse()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var existingItem = new MenuFlyoutItem();
            var nonExistingItem = new MenuFlyoutItem();
            menuFlyoutSubItem.Add(existingItem);

            // Act
            var result = menuFlyoutSubItem.Contains(nonExistingItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Contains with multiple items in the collection.
        /// This verifies the method works correctly with collections containing multiple elements.
        /// </summary>
        [Theory]
        [InlineData(0, true)]  // First item exists
        [InlineData(1, true)]  // Second item exists
        [InlineData(2, true)]  // Third item exists
        [InlineData(3, false)] // Fourth item doesn't exist
        public void Contains_WithMultipleItems_ReturnsCorrectResult(int itemIndex, bool expectedResult)
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var items = new[]
            {
                new MenuFlyoutItem(),
                new MenuFlyoutItem(),
                new MenuFlyoutItem(),
                new MenuFlyoutItem() // This one won't be added
			};

            // Add first three items
            for (int i = 0; i < 3; i++)
            {
                menuFlyoutSubItem.Add(items[i]);
            }

            // Act
            var result = menuFlyoutSubItem.Contains(items[itemIndex]);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Contains works correctly with different types of IMenuElement implementations.
        /// This verifies the method handles various menu element types properly.
        /// </summary>
        [Fact]
        public void Contains_WithDifferentMenuElementTypes_ReturnsCorrectResult()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var menuItem = new MenuFlyoutItem();
            var nestedSubItem = new MenuFlyoutSubItem();
            var nonExistingItem = new MenuFlyoutItem();

            menuFlyoutSubItem.Add(menuItem);
            menuFlyoutSubItem.Add(nestedSubItem);

            // Act & Assert
            Assert.True(menuFlyoutSubItem.Contains(menuItem));
            Assert.True(menuFlyoutSubItem.Contains(nestedSubItem));
            Assert.False(menuFlyoutSubItem.Contains(nonExistingItem));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            IMenuElement[] array = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => menuFlyoutSubItem.CopyTo(array, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Verifies proper boundary validation for negative indices.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException(int arrayIndex)
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var array = new IMenuElement[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => menuFlyoutSubItem.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// Verifies proper boundary validation for out-of-bounds indices.
        /// Expected result: ArgumentException is thrown.
        /// </summary>
        [Theory]
        [InlineData(5, 5)]
        [InlineData(5, 6)]
        [InlineData(5, int.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        public void CopyTo_ArrayIndexOutOfBounds_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var array = new IMenuElement[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuFlyoutSubItem.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when array is too small to hold all elements.
        /// Verifies proper validation when insufficient space is available in destination array.
        /// Expected result: ArgumentException is thrown.
        /// </summary>
        [Theory]
        [InlineData(3, 2, 2)] // 3 items, array size 2, index 2 - not enough space
        [InlineData(2, 3, 2)] // 2 items, array size 3, index 2 - only 1 space available
        [InlineData(5, 7, 3)] // 5 items, array size 7, index 3 - only 4 spaces available
        public void CopyTo_InsufficientArraySpace_ThrowsArgumentException(int itemCount, int arraySize, int arrayIndex)
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();

            // Add items to the menu
            for (int i = 0; i < itemCount; i++)
            {
                var mockItem = Substitute.For<IMenuElement>();
                menuFlyoutSubItem.Add(mockItem);
            }

            var array = new IMenuElement[arraySize];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuFlyoutSubItem.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies empty menu to array.
        /// Verifies behavior when source collection is empty.
        /// Expected result: No items are copied, no exception is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyMenu_SuccessfullyCopies()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var array = new IMenuElement[5];
            var originalArray = new IMenuElement[5];

            // Act
            menuFlyoutSubItem.CopyTo(array, 2);

            // Assert
            Assert.Equal(originalArray, array); // Array should remain unchanged
        }

        /// <summary>
        /// Tests that CopyTo successfully copies single item to array at specified index.
        /// Verifies basic functionality with single element.
        /// Expected result: Item is copied to correct position in array.
        /// </summary>
        [Fact]
        public void CopyTo_SingleItem_CopiesCorrectly()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockItem = Substitute.For<IMenuElement>();
            menuFlyoutSubItem.Add(mockItem);

            var array = new IMenuElement[5];
            int arrayIndex = 2;

            // Act
            menuFlyoutSubItem.CopyTo(array, arrayIndex);

            // Assert
            Assert.Same(mockItem, array[arrayIndex]);
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Null(array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies multiple items to array in correct order.
        /// Verifies that elements are copied sequentially starting from specified index.
        /// Expected result: All items are copied in order starting from arrayIndex.
        /// </summary>
        [Fact]
        public void CopyTo_MultipleItems_CopiesInCorrectOrder()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockItem1 = Substitute.For<IMenuElement>();
            var mockItem2 = Substitute.For<IMenuElement>();
            var mockItem3 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(mockItem1);
            menuFlyoutSubItem.Add(mockItem2);
            menuFlyoutSubItem.Add(mockItem3);

            var array = new IMenuElement[6];
            int arrayIndex = 1;

            // Act
            menuFlyoutSubItem.CopyTo(array, arrayIndex);

            // Assert
            Assert.Null(array[0]);
            Assert.Same(mockItem1, array[1]);
            Assert.Same(mockItem2, array[2]);
            Assert.Same(mockItem3, array[3]);
            Assert.Null(array[4]);
            Assert.Null(array[5]);
        }

        /// <summary>
        /// Tests that CopyTo copies to exact-size array starting at index 0.
        /// Verifies functionality when array size exactly matches menu item count.
        /// Expected result: All items are copied, filling the entire array.
        /// </summary>
        [Fact]
        public void CopyTo_ExactSizeArray_CopiesSuccessfully()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockItem1 = Substitute.For<IMenuElement>();
            var mockItem2 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(mockItem1);
            menuFlyoutSubItem.Add(mockItem2);

            var array = new IMenuElement[2];

            // Act
            menuFlyoutSubItem.CopyTo(array, 0);

            // Assert
            Assert.Same(mockItem1, array[0]);
            Assert.Same(mockItem2, array[1]);
        }

        /// <summary>
        /// Tests that CopyTo operation does not modify the original menu collection.
        /// Verifies that the source collection remains unchanged after copying.
        /// Expected result: Original menu retains all items and properties.
        /// </summary>
        [Fact]
        public void CopyTo_DoesNotModifyOriginalMenu()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockItem1 = Substitute.For<IMenuElement>();
            var mockItem2 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(mockItem1);
            menuFlyoutSubItem.Add(mockItem2);

            var originalCount = menuFlyoutSubItem.Count;
            var array = new IMenuElement[5];

            // Act
            menuFlyoutSubItem.CopyTo(array, 1);

            // Assert
            Assert.Equal(originalCount, menuFlyoutSubItem.Count);
            Assert.Same(mockItem1, menuFlyoutSubItem[0]);
            Assert.Same(mockItem2, menuFlyoutSubItem[1]);
        }

        /// <summary>
        /// Tests CopyTo with various valid boundary conditions.
        /// Verifies behavior at edge cases for valid parameters.
        /// Expected result: Operations complete successfully without exceptions.
        /// </summary>
        [Theory]
        [InlineData(0, 1, 0)] // Empty menu, single element array, index 0
        [InlineData(1, 1, 0)] // Single item, single element array, index 0
        [InlineData(3, 10, 7)] // Multiple items, large array, high index
        [InlineData(2, 2, 0)] // Exact fit scenario
        public void CopyTo_BoundaryConditions_SucceedsWithoutException(int itemCount, int arraySize, int arrayIndex)
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();

            for (int i = 0; i < itemCount; i++)
            {
                var mockItem = Substitute.For<IMenuElement>();
                menuFlyoutSubItem.Add(mockItem);
            }

            var array = new IMenuElement[arraySize];

            // Act & Assert
            var exception = Record.Exception(() => menuFlyoutSubItem.CopyTo(array, arrayIndex));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemExists_ReturnsCorrectIndex()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var item1 = Substitute.For<IMenuElement>();
            var item2 = Substitute.For<IMenuElement>();
            var item3 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(item1);
            menuFlyoutSubItem.Add(item2);
            menuFlyoutSubItem.Add(item3);

            // Act
            int result = menuFlyoutSubItem.IndexOf(item2);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when the item does not exist in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemNotInCollection_ReturnsMinusOne()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var existingItem = Substitute.For<IMenuElement>();
            var nonExistingItem = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(existingItem);

            // Act
            int result = menuFlyoutSubItem.IndexOf(nonExistingItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when null is passed as parameter.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var item = Substitute.For<IMenuElement>();
            menuFlyoutSubItem.Add(item);

            // Act
            int result = menuFlyoutSubItem.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when searching in an empty collection.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsMinusOne()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var item = Substitute.For<IMenuElement>();

            // Act
            int result = menuFlyoutSubItem.IndexOf(item);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when the same item appears multiple times.
        /// </summary>
        [Fact]
        public void IndexOf_MultipleOccurrences_ReturnsIndexOfFirst()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var item1 = Substitute.For<IMenuElement>();
            var duplicateItem = Substitute.For<IMenuElement>();
            var item3 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(item1);
            menuFlyoutSubItem.Add(duplicateItem);
            menuFlyoutSubItem.Add(item3);
            menuFlyoutSubItem.Add(duplicateItem); // Add same item again

            // Act
            int result = menuFlyoutSubItem.IndexOf(duplicateItem);

            // Assert
            Assert.Equal(1, result); // Should return first occurrence at index 1, not 3
        }

        /// <summary>
        /// Tests that IndexOf returns correct index at the beginning of the collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtBeginning_ReturnsZero()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var firstItem = Substitute.For<IMenuElement>();
            var secondItem = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(firstItem);
            menuFlyoutSubItem.Add(secondItem);

            // Act
            int result = menuFlyoutSubItem.IndexOf(firstItem);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns correct index at the end of the collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtEnd_ReturnsLastIndex()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var item1 = Substitute.For<IMenuElement>();
            var item2 = Substitute.For<IMenuElement>();
            var lastItem = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(item1);
            menuFlyoutSubItem.Add(item2);
            menuFlyoutSubItem.Add(lastItem);

            // Act
            int result = menuFlyoutSubItem.IndexOf(lastItem);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests that Count property returns 0 for a newly created MenuFlyoutSubItem instance.
        /// Input conditions: New instance with no items added.
        /// Expected result: Count should return 0.
        /// </summary>
        [Fact]
        public void Count_NewInstance_ReturnsZero()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();

            // Act
            var count = menuFlyoutSubItem.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count property returns correct number after adding items.
        /// Input conditions: Adding 1, 2, and 3 items respectively.
        /// Expected result: Count should return the number of items added.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Count_AfterAddingItems_ReturnsCorrectCount(int numberOfItems)
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();

            for (int i = 0; i < numberOfItems; i++)
            {
                var mockMenuItem = Substitute.For<IMenuElement>();
                menuFlyoutSubItem.Add(mockMenuItem);
            }

            // Act
            var count = menuFlyoutSubItem.Count;

            // Assert
            Assert.Equal(numberOfItems, count);
        }

        /// <summary>
        /// Tests that Count property returns correct number after inserting items at specific positions.
        /// Input conditions: Inserting multiple items at index 0.
        /// Expected result: Count should return the total number of items inserted.
        /// </summary>
        [Fact]
        public void Count_AfterInsertingItems_ReturnsCorrectCount()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockMenuItem1 = Substitute.For<IMenuElement>();
            var mockMenuItem2 = Substitute.For<IMenuElement>();

            // Act
            menuFlyoutSubItem.Insert(0, mockMenuItem1);
            menuFlyoutSubItem.Insert(0, mockMenuItem2);

            // Assert
            Assert.Equal(2, menuFlyoutSubItem.Count);
        }

        /// <summary>
        /// Tests that Count property returns correct number after removing items.
        /// Input conditions: Adding 3 items then removing 1 item.
        /// Expected result: Count should return 2.
        /// </summary>
        [Fact]
        public void Count_AfterRemovingItem_ReturnsCorrectCount()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockMenuItem1 = Substitute.For<IMenuElement>();
            var mockMenuItem2 = Substitute.For<IMenuElement>();
            var mockMenuItem3 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(mockMenuItem1);
            menuFlyoutSubItem.Add(mockMenuItem2);
            menuFlyoutSubItem.Add(mockMenuItem3);

            // Act
            menuFlyoutSubItem.Remove(mockMenuItem2);

            // Assert
            Assert.Equal(2, menuFlyoutSubItem.Count);
        }

        /// <summary>
        /// Tests that Count property returns correct number after removing items by index.
        /// Input conditions: Adding 3 items then removing item at index 1.
        /// Expected result: Count should return 2.
        /// </summary>
        [Fact]
        public void Count_AfterRemovingAtIndex_ReturnsCorrectCount()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockMenuItem1 = Substitute.For<IMenuElement>();
            var mockMenuItem2 = Substitute.For<IMenuElement>();
            var mockMenuItem3 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(mockMenuItem1);
            menuFlyoutSubItem.Add(mockMenuItem2);
            menuFlyoutSubItem.Add(mockMenuItem3);

            // Act
            menuFlyoutSubItem.RemoveAt(1);

            // Assert
            Assert.Equal(2, menuFlyoutSubItem.Count);
        }

        /// <summary>
        /// Tests that Count property returns 0 after clearing all items.
        /// Input conditions: Adding multiple items then clearing the collection.
        /// Expected result: Count should return 0.
        /// </summary>
        [Fact]
        public void Count_AfterClear_ReturnsZero()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockMenuItem1 = Substitute.For<IMenuElement>();
            var mockMenuItem2 = Substitute.For<IMenuElement>();

            menuFlyoutSubItem.Add(mockMenuItem1);
            menuFlyoutSubItem.Add(mockMenuItem2);

            // Act
            menuFlyoutSubItem.Clear();

            // Assert
            Assert.Equal(0, menuFlyoutSubItem.Count);
        }

        /// <summary>
        /// Tests that Count property reflects cumulative changes after multiple operations.
        /// Input conditions: Mix of Add, Remove, Insert, and RemoveAt operations.
        /// Expected result: Count should correctly reflect the final number of items.
        /// </summary>
        [Fact]
        public void Count_AfterMixedOperations_ReturnsCorrectCount()
        {
            // Arrange
            var menuFlyoutSubItem = new MenuFlyoutSubItem();
            var mockMenuItem1 = Substitute.For<IMenuElement>();
            var mockMenuItem2 = Substitute.For<IMenuElement>();
            var mockMenuItem3 = Substitute.For<IMenuElement>();
            var mockMenuItem4 = Substitute.For<IMenuElement>();

            // Act & Assert - tracking count through operations
            menuFlyoutSubItem.Add(mockMenuItem1);
            Assert.Equal(1, menuFlyoutSubItem.Count);

            menuFlyoutSubItem.Add(mockMenuItem2);
            Assert.Equal(2, menuFlyoutSubItem.Count);

            menuFlyoutSubItem.Insert(0, mockMenuItem3);
            Assert.Equal(3, menuFlyoutSubItem.Count);

            menuFlyoutSubItem.Remove(mockMenuItem1);
            Assert.Equal(2, menuFlyoutSubItem.Count);

            menuFlyoutSubItem.Add(mockMenuItem4);
            Assert.Equal(3, menuFlyoutSubItem.Count);

            menuFlyoutSubItem.RemoveAt(0);
            Assert.Equal(2, menuFlyoutSubItem.Count);
        }
    }
}