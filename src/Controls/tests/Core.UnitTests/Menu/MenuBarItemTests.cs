#nullable disable

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
    [Category("MenuBarItem")]
    public class MenuBarItemTests :
        MenuTestBase<MenuBarItem, IMenuElement, MenuFlyoutItem, MenuBarItemHandlerUpdate>
    {
        [Fact]
        public void StartsEnabled()
        {
            MenuBarItem menuBarItem = new MenuBarItem();
            Assert.True(menuBarItem.IsEnabled);
        }

        [Fact]
        public void DisableWorks()
        {
            MenuBarItem menuBarItem = new MenuBarItem();
            menuBarItem.IsEnabled = false;
            Assert.False(menuBarItem.IsEnabled);
        }

        protected override int GetIndex(MenuBarItemHandlerUpdate handlerUpdate) =>
            handlerUpdate.Index;

        protected override IMenuElement GetItem(MenuBarItemHandlerUpdate handlerUpdate) =>
            handlerUpdate.MenuElement;

        protected override void SetHandler(Maui.IElement element, List<(string Name, MenuBarItemHandlerUpdate? Args)> events)
        {
            element.Handler = CreateMenuBarItemHandler((n, h, l, a) => events.Add((n, a)));
        }

        MenuBarItemHandler CreateMenuBarItemHandler(Action<string, IMenuBarItemHandler, IMenuBarItem, MenuBarItemHandlerUpdate?>? action)
        {
            var handler = new NonThrowingMenuBarItemHandler(
                MenuBarItemHandler.Mapper,
                new CommandMapper<IMenuBarItem, IMenuBarItemHandler>(MenuBarItemHandler.CommandMapper)
                {
                    [nameof(IMenuBarItemHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Add), h, l, (MenuBarItemHandlerUpdate?)a),
                    [nameof(IMenuBarItemHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Remove), h, l, (MenuBarItemHandlerUpdate?)a),
                    [nameof(IMenuBarItemHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Clear), h, l, (MenuBarItemHandlerUpdate?)a),
                    [nameof(IMenuBarItemHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Insert), h, l, (MenuBarItemHandlerUpdate?)a),
                });

            return handler;
        }

        class NonThrowingMenuBarItemHandler : MenuBarItemHandler
        {
            public NonThrowingMenuBarItemHandler(IPropertyMapper mapper, CommandMapper commandMapper)
                : base(mapper, commandMapper)
            {
            }

            protected override object CreatePlatformElement() => new object();
        }

        /// <summary>
        /// Tests that the Count property returns 0 for a newly created MenuBarItem.
        /// Verifies the initial state of an empty menu collection.
        /// </summary>
        [Fact]
        public void Count_InitialState_ReturnsZero()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act
            var count = menuBarItem.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that the Count property returns the correct number after adding multiple items.
        /// Verifies that Count accurately reflects the number of items in the collection.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public void Count_AfterAddingItems_ReturnsCorrectNumber(int numberOfItems)
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act
            for (int i = 0; i < numberOfItems; i++)
            {
                menuBarItem.Add(new MenuFlyoutItem { Text = $"Item {i}" });
            }

            // Assert
            Assert.Equal(numberOfItems, menuBarItem.Count);
        }

        /// <summary>
        /// Tests that the Count property returns 0 after clearing all items.
        /// Verifies that Count accurately reflects an empty collection after Clear operation.
        /// </summary>
        [Fact]
        public void Count_AfterClear_ReturnsZero()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            menuBarItem.Add(new MenuFlyoutItem { Text = "Item 1" });
            menuBarItem.Add(new MenuFlyoutItem { Text = "Item 2" });
            menuBarItem.Add(new MenuFlyoutItem { Text = "Item 3" });

            // Act
            menuBarItem.Clear();

            // Assert
            Assert.Equal(0, menuBarItem.Count);
        }

        /// <summary>
        /// Tests that the Count property decreases correctly after removing items.
        /// Verifies that Count accurately reflects the collection size after removal operations.
        /// </summary>
        [Fact]
        public void Count_AfterRemovingItems_ReturnsCorrectNumber()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item1 = new MenuFlyoutItem { Text = "Item 1" };
            var item2 = new MenuFlyoutItem { Text = "Item 2" };
            var item3 = new MenuFlyoutItem { Text = "Item 3" };

            menuBarItem.Add(item1);
            menuBarItem.Add(item2);
            menuBarItem.Add(item3);

            // Act & Assert
            Assert.Equal(3, menuBarItem.Count);

            menuBarItem.Remove(item2);
            Assert.Equal(2, menuBarItem.Count);

            menuBarItem.RemoveAt(0);
            Assert.Equal(1, menuBarItem.Count);

            menuBarItem.Remove(item3);
            Assert.Equal(0, menuBarItem.Count);
        }

        /// <summary>
        /// Tests that the Count property updates correctly after inserting items at specific positions.
        /// Verifies that Count accurately reflects the collection size after insertion operations.
        /// </summary>
        [Fact]
        public void Count_AfterInsertingItems_ReturnsCorrectNumber()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item1 = new MenuFlyoutItem { Text = "Item 1" };
            var item2 = new MenuFlyoutItem { Text = "Item 2" };

            // Act & Assert
            menuBarItem.Insert(0, item1);
            Assert.Equal(1, menuBarItem.Count);

            menuBarItem.Insert(0, item2);
            Assert.Equal(2, menuBarItem.Count);
        }

        /// <summary>
        /// Tests that the Count property updates correctly when replacing items using the indexer.
        /// Verifies that Count remains the same when replacing existing items.
        /// </summary>
        [Fact]
        public void Count_AfterReplacingItemsWithIndexer_RemainsConstant()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var originalItem = new MenuFlyoutItem { Text = "Original Item" };
            var replacementItem = new MenuFlyoutItem { Text = "Replacement Item" };

            menuBarItem.Add(originalItem);

            // Act
            menuBarItem[0] = replacementItem;

            // Assert
            Assert.Equal(1, menuBarItem.Count);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => menuBarItem.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// </summary>
        [Fact]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var array = new IMenuElement[10];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => menuBarItem.CopyTo(array, -1));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than array length.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexGreaterThanArrayLength_ThrowsArgumentException()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var array = new IMenuElement[5];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuBarItem.CopyTo(array, 6));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is insufficient space in target array.
        /// </summary>
        [Fact]
        public void CopyTo_InsufficientSpaceInArray_ThrowsArgumentException()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var menuElement1 = Substitute.For<IMenuElement>();
            var menuElement2 = Substitute.For<IMenuElement>();
            menuBarItem.Add(menuElement1);
            menuBarItem.Add(menuElement2);
            var array = new IMenuElement[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuBarItem.CopyTo(array, 2));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements from empty MenuBarItem to array.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyMenuBarItem_CopiesNothing()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var array = new IMenuElement[5];

            // Act
            menuBarItem.CopyTo(array, 0);

            // Assert
            Assert.All(array, item => Assert.Null(item));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies single element to array at index 0.
        /// </summary>
        [Fact]
        public void CopyTo_SingleElement_CopiesElementToArrayAtIndexZero()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var menuElement = Substitute.For<IMenuElement>();
            menuBarItem.Add(menuElement);
            var array = new IMenuElement[5];

            // Act
            menuBarItem.CopyTo(array, 0);

            // Assert
            Assert.Same(menuElement, array[0]);
            Assert.Null(array[1]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies single element to array at specified index.
        /// </summary>
        [Fact]
        public void CopyTo_SingleElement_CopiesElementToArrayAtSpecifiedIndex()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var menuElement = Substitute.For<IMenuElement>();
            menuBarItem.Add(menuElement);
            var array = new IMenuElement[5];

            // Act
            menuBarItem.CopyTo(array, 2);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Same(menuElement, array[2]);
            Assert.Null(array[3]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies multiple elements to array preserving order.
        /// </summary>
        [Fact]
        public void CopyTo_MultipleElements_CopiesElementsInOrder()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var menuElement1 = Substitute.For<IMenuElement>();
            var menuElement2 = Substitute.For<IMenuElement>();
            var menuElement3 = Substitute.For<IMenuElement>();
            menuBarItem.Add(menuElement1);
            menuBarItem.Add(menuElement2);
            menuBarItem.Add(menuElement3);
            var array = new IMenuElement[6];

            // Act
            menuBarItem.CopyTo(array, 1);

            // Assert
            Assert.Null(array[0]);
            Assert.Same(menuElement1, array[1]);
            Assert.Same(menuElement2, array[2]);
            Assert.Same(menuElement3, array[3]);
            Assert.Null(array[4]);
            Assert.Null(array[5]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when array has exact size needed.
        /// </summary>
        [Fact]
        public void CopyTo_ExactArraySize_CopiesAllElements()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var menuElement1 = Substitute.For<IMenuElement>();
            var menuElement2 = Substitute.For<IMenuElement>();
            menuBarItem.Add(menuElement1);
            menuBarItem.Add(menuElement2);
            var array = new IMenuElement[2];

            // Act
            menuBarItem.CopyTo(array, 0);

            // Assert
            Assert.Same(menuElement1, array[0]);
            Assert.Same(menuElement2, array[1]);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the collection.
        /// Input: MenuBarItem with existing IMenuElement items.
        /// Expected: Returns the zero-based index of the found item.
        /// </summary>
        [Fact]
        public void IndexOf_ExistingItem_ReturnsCorrectIndex()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item1 = Substitute.For<IMenuElement>();
            var item2 = Substitute.For<IMenuElement>();
            var item3 = Substitute.For<IMenuElement>();

            menuBarItem.Add(item1);
            menuBarItem.Add(item2);
            menuBarItem.Add(item3);

            // Act
            var result = menuBarItem.IndexOf(item2);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when the item does not exist in the collection.
        /// Input: MenuBarItem with items and a non-existing IMenuElement.
        /// Expected: Returns -1.
        /// </summary>
        [Fact]
        public void IndexOf_NonExistingItem_ReturnsMinusOne()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var existingItem = Substitute.For<IMenuElement>();
            var nonExistingItem = Substitute.For<IMenuElement>();

            menuBarItem.Add(existingItem);

            // Act
            var result = menuBarItem.IndexOf(nonExistingItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when called on an empty collection.
        /// Input: Empty MenuBarItem and any IMenuElement.
        /// Expected: Returns -1.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsMinusOne()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item = Substitute.For<IMenuElement>();

            // Act
            var result = menuBarItem.IndexOf(item);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf handles null input appropriately.
        /// Input: MenuBarItem with items and null parameter.
        /// Expected: Returns -1.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item = Substitute.For<IMenuElement>();
            menuBarItem.Add(item);

            // Act
            var result = menuBarItem.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when duplicate items exist.
        /// Input: MenuBarItem with duplicate IMenuElement items.
        /// Expected: Returns the index of the first occurrence.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item1 = Substitute.For<IMenuElement>();
            var item2 = Substitute.For<IMenuElement>();

            menuBarItem.Add(item1);
            menuBarItem.Add(item2);
            menuBarItem.Add(item1); // Add duplicate

            // Act
            var result = menuBarItem.IndexOf(item1);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns 0 when searching for the first item in a single-item collection.
        /// Input: MenuBarItem with one IMenuElement.
        /// Expected: Returns 0.
        /// </summary>
        [Fact]
        public void IndexOf_SingleItemCollection_ReturnsZero()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item = Substitute.For<IMenuElement>();
            menuBarItem.Add(item);

            // Act
            var result = menuBarItem.IndexOf(item);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for the last item in a multi-item collection.
        /// Input: MenuBarItem with multiple IMenuElement items.
        /// Expected: Returns the index of the last item.
        /// </summary>
        [Fact]
        public void IndexOf_LastItemInCollection_ReturnsLastIndex()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item1 = Substitute.For<IMenuElement>();
            var item2 = Substitute.For<IMenuElement>();
            var item3 = Substitute.For<IMenuElement>();

            menuBarItem.Add(item1);
            menuBarItem.Add(item2);
            menuBarItem.Add(item3);

            // Act
            var result = menuBarItem.IndexOf(item3);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests that Contains returns false when called with a null item.
        /// Validates the behavior when checking for null items in the menu collection.
        /// Expected result: Should return false.
        /// </summary>
        [Fact]
        public void Contains_WithNullItem_ReturnsFalse()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act
            var result = menuBarItem.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection.
        /// Validates the behavior when checking for items in an empty menu collection.
        /// Expected result: Should return false.
        /// </summary>
        [Fact]
        public void Contains_WithEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var mockItem = Substitute.For<IMenuElement, Element>();

            // Act
            var result = menuBarItem.Contains(mockItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the collection.
        /// Validates the behavior when checking for an existing item in the menu collection.
        /// Expected result: Should return true.
        /// </summary>
        [Fact]
        public void Contains_WithExistingItem_ReturnsTrue()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var mockItem = Substitute.For<IMenuElement, Element>();
            menuBarItem.Add(mockItem);

            // Act
            var result = menuBarItem.Contains(mockItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the collection.
        /// Validates the behavior when checking for a non-existing item in a populated menu collection.
        /// Expected result: Should return false.
        /// </summary>
        [Fact]
        public void Contains_WithNonExistingItem_ReturnsFalse()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var existingItem = Substitute.For<IMenuElement, Element>();
            var nonExistingItem = Substitute.For<IMenuElement, Element>();
            menuBarItem.Add(existingItem);

            // Act
            var result = menuBarItem.Contains(nonExistingItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns correct results when multiple items exist in the collection.
        /// Validates the behavior when checking for items in a collection with multiple elements.
        /// Expected result: Should return true for existing items and false for non-existing items.
        /// </summary>
        [Fact]
        public void Contains_WithMultipleItems_ReturnsCorrectResults()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();
            var item1 = Substitute.For<IMenuElement, Element>();
            var item2 = Substitute.For<IMenuElement, Element>();
            var item3 = Substitute.For<IMenuElement, Element>();
            var nonExistingItem = Substitute.For<IMenuElement, Element>();

            menuBarItem.Add(item1);
            menuBarItem.Add(item2);
            menuBarItem.Add(item3);

            // Act & Assert
            Assert.True(menuBarItem.Contains(item1));
            Assert.True(menuBarItem.Contains(item2));
            Assert.True(menuBarItem.Contains(item3));
            Assert.False(menuBarItem.Contains(nonExistingItem));
        }

        /// <summary>
        /// Tests that the IsReadOnly property always returns false, indicating that the MenuBarItem collection is always modifiable.
        /// This test verifies that the collection interface implementation correctly signals that items can be added and removed.
        /// </summary>
        [Fact]
        public void IsReadOnly_Always_ReturnsFalse()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act
            bool isReadOnly = menuBarItem.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that the Priority property getter returns the correct default value.
        /// Verifies that a newly created MenuBarItem has Priority set to the default value of 0.
        /// </summary>
        [Fact]
        public void Priority_DefaultValue_ReturnsZero()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act
            int priority = menuBarItem.Priority;

            // Assert
            Assert.Equal(0, priority);
        }

        /// <summary>
        /// Tests that the Priority property getter and setter work correctly with various integer values.
        /// Verifies that setting Priority to different values and then retrieving them returns the expected values.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Priority_SetAndGet_ReturnsExpectedValue(int expectedPriority)
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act
            menuBarItem.Priority = expectedPriority;
            int actualPriority = menuBarItem.Priority;

            // Assert
            Assert.Equal(expectedPriority, actualPriority);
        }

        /// <summary>
        /// Tests that the Priority property getter returns the correct value when set multiple times.
        /// Verifies that the Priority property correctly updates when set to different values sequentially.
        /// </summary>
        [Fact]
        public void Priority_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var menuBarItem = new MenuBarItem();

            // Act & Assert
            menuBarItem.Priority = 10;
            Assert.Equal(10, menuBarItem.Priority);

            menuBarItem.Priority = -5;
            Assert.Equal(-5, menuBarItem.Priority);

            menuBarItem.Priority = int.MaxValue;
            Assert.Equal(int.MaxValue, menuBarItem.Priority);
        }
    }
}