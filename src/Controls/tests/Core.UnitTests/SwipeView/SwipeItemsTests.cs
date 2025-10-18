#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SwipeItems class IsReadOnly property.
    /// </summary>
    public class SwipeItemsTests
    {
        /// <summary>
        /// Tests that IsReadOnly property returns false when SwipeItems is initialized with default constructor.
        /// This verifies that the collection is not read-only for an empty collection.
        /// </summary>
        [Fact]
        public void IsReadOnly_DefaultConstructor_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();

            // Act
            var isReadOnly = swipeItems.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false when SwipeItems is initialized with empty collection.
        /// This ensures the property behaves consistently regardless of initialization method.
        /// </summary>
        [Fact]
        public void IsReadOnly_EmptyCollection_ReturnsFalse()
        {
            // Arrange
            var emptyCollection = Enumerable.Empty<ISwipeItem>();
            var swipeItems = new SwipeItems(emptyCollection);

            // Act
            var isReadOnly = swipeItems.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false when SwipeItems contains mocked items.
        /// This verifies that the collection remains writable even when it contains items.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithMockedItems_ReturnsFalse()
        {
            // Arrange
            var mockItem1 = Substitute.For<ISwipeItem>();
            var mockItem2 = Substitute.For<ISwipeItem>();
            var itemsCollection = new List<ISwipeItem> { mockItem1, mockItem2 };
            var swipeItems = new SwipeItems(itemsCollection);

            // Act
            var isReadOnly = swipeItems.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that IsReadOnly property consistently returns false across multiple instances.
        /// This ensures the property value is consistent and not dependent on instance state.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void IsReadOnly_MultipleInstances_AlwaysReturnsFalse(int itemCount)
        {
            // Arrange
            var items = new List<ISwipeItem>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(Substitute.For<ISwipeItem>());
            }
            var swipeItems = new SwipeItems(items);

            // Act
            var isReadOnly = swipeItems.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false after items are added to the collection.
        /// This verifies that the property remains false even after collection modifications.
        /// </summary>
        [Fact]
        public void IsReadOnly_AfterAddingItems_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();

            // Act
            swipeItems.Add(mockItem);
            var isReadOnly = swipeItems.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that Contains returns false when called with a null item parameter.
        /// </summary>
        [Fact]
        public void Contains_NullItem_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();

            // Act
            bool result = swipeItems.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the collection.
        /// </summary>
        [Fact]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem);

            // Act
            bool result = swipeItems.Contains(mockItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the collection.
        /// </summary>
        [Fact]
        public void Contains_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem1 = Substitute.For<ISwipeItem>();
            var mockItem2 = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem1);

            // Act
            bool result = swipeItems.Contains(mockItem2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection.
        /// </summary>
        [Fact]
        public void Contains_EmptyCollection_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();

            // Act
            bool result = swipeItems.Contains(mockItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with multiple items in the collection.
        /// </summary>
        [Fact]
        public void Contains_MultipleItems_ReturnsCorrectResult()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem1 = Substitute.For<ISwipeItem>();
            var mockItem2 = Substitute.For<ISwipeItem>();
            var mockItem3 = Substitute.For<ISwipeItem>();

            swipeItems.Add(mockItem1);
            swipeItems.Add(mockItem2);

            // Act & Assert
            Assert.True(swipeItems.Contains(mockItem1));
            Assert.True(swipeItems.Contains(mockItem2));
            Assert.False(swipeItems.Contains(mockItem3));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            ISwipeItem[] array = null;
            int arrayIndex = 0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => swipeItems.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException(int arrayIndex)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var array = new ISwipeItem[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => swipeItems.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// </summary>
        [Theory]
        [InlineData(5, 5)]
        [InlineData(5, 6)]
        [InlineData(5, int.MaxValue)]
        [InlineData(0, 1)]
        public void CopyTo_ArrayIndexOutOfBounds_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem);
            var array = new ISwipeItem[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => swipeItems.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there's insufficient space in destination array.
        /// </summary>
        [Theory]
        [InlineData(3, 2, 2)] // 3 items, array size 2, start at index 2 - not enough space
        [InlineData(2, 3, 2)] // 2 items, array size 3, start at index 2 - not enough space
        [InlineData(5, 5, 1)] // 5 items, array size 5, start at index 1 - not enough space
        public void CopyTo_InsufficientSpace_ThrowsArgumentException(int itemCount, int arraySize, int arrayIndex)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            for (int i = 0; i < itemCount; i++)
            {
                var mockItem = Substitute.For<ISwipeItem>();
                swipeItems.Add(mockItem);
            }
            var array = new ISwipeItem[arraySize];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => swipeItems.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies empty collection to empty array.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollectionToEmptyArray_Success()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var array = new ISwipeItem[0];
            int arrayIndex = 0;

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.Empty(array);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies empty collection to non-empty array.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollectionToNonEmptyArray_Success()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var array = new ISwipeItem[5];
            int arrayIndex = 2;

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.All(array, item => Assert.Null(item));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies single item to array at index zero.
        /// </summary>
        [Fact]
        public void CopyTo_SingleItemAtIndexZero_Success()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem);
            var array = new ISwipeItem[3];
            int arrayIndex = 0;

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(mockItem, array[0]);
            Assert.Null(array[1]);
            Assert.Null(array[2]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies single item to array at non-zero index.
        /// </summary>
        [Fact]
        public void CopyTo_SingleItemAtNonZeroIndex_Success()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem);
            var array = new ISwipeItem[5];
            int arrayIndex = 2;

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Equal(mockItem, array[2]);
            Assert.Null(array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies multiple items maintaining order.
        /// </summary>
        [Fact]
        public void CopyTo_MultipleItems_MaintainsOrder()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem1 = Substitute.For<ISwipeItem>();
            var mockItem2 = Substitute.For<ISwipeItem>();
            var mockItem3 = Substitute.For<ISwipeItem>();

            swipeItems.Add(mockItem1);
            swipeItems.Add(mockItem2);
            swipeItems.Add(mockItem3);

            var array = new ISwipeItem[5];
            int arrayIndex = 1;

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.Null(array[0]);
            Assert.Equal(mockItem1, array[1]);
            Assert.Equal(mockItem2, array[2]);
            Assert.Equal(mockItem3, array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies items to array with exact fit.
        /// </summary>
        [Fact]
        public void CopyTo_ExactFit_Success()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem1 = Substitute.For<ISwipeItem>();
            var mockItem2 = Substitute.For<ISwipeItem>();

            swipeItems.Add(mockItem1);
            swipeItems.Add(mockItem2);

            var array = new ISwipeItem[2];
            int arrayIndex = 0;

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(mockItem1, array[0]);
            Assert.Equal(mockItem2, array[1]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when copying to the last possible position in array.
        /// </summary>
        [Fact]
        public void CopyTo_LastPossiblePosition_Success()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem);

            var array = new ISwipeItem[3];
            int arrayIndex = 2; // Last valid position for single item

            // Act
            swipeItems.CopyTo(array, arrayIndex);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Equal(mockItem, array[2]);
        }

        /// <summary>
        /// Tests that GetEnumerator returns an empty enumerator when the SwipeItems collection is empty.
        /// Verifies that no items are yielded from an empty collection.
        /// </summary>
        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmptyEnumerator()
        {
            // Arrange
            var swipeItems = new SwipeItems();

            // Act
            var enumerator = swipeItems.GetEnumerator();
            var items = new List<ISwipeItem>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            // Assert
            Assert.Empty(items);
        }

        /// <summary>
        /// Tests that GetEnumerator returns an enumerator that yields a single item when the collection contains one item.
        /// Verifies that the yielded item matches the item that was added to the collection.
        /// </summary>
        [Fact]
        public void GetEnumerator_SingleItem_ReturnsSingleItem()
        {
            // Arrange
            var mockSwipeItem = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(mockSwipeItem);

            // Act
            var enumerator = swipeItems.GetEnumerator();
            var items = new List<ISwipeItem>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            // Assert
            Assert.Single(items);
            Assert.Equal(mockSwipeItem, items[0]);
        }

        /// <summary>
        /// Tests that GetEnumerator returns an enumerator that yields multiple items in the correct order.
        /// Verifies that all items are yielded and in the same order they were added to the collection.
        /// </summary>
        [Fact]
        public void GetEnumerator_MultipleItems_ReturnsAllItemsInOrder()
        {
            // Arrange
            var mockSwipeItem1 = Substitute.For<ISwipeItem>();
            var mockSwipeItem2 = Substitute.For<ISwipeItem>();
            var mockSwipeItem3 = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(mockSwipeItem1);
            swipeItems.Add(mockSwipeItem2);
            swipeItems.Add(mockSwipeItem3);

            // Act
            var enumerator = swipeItems.GetEnumerator();
            var items = new List<ISwipeItem>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            // Assert
            Assert.Equal(3, items.Count);
            Assert.Equal(mockSwipeItem1, items[0]);
            Assert.Equal(mockSwipeItem2, items[1]);
            Assert.Equal(mockSwipeItem3, items[2]);
        }

        /// <summary>
        /// Tests that GetEnumerator can be called multiple times and each enumerator works independently.
        /// Verifies that multiple enumerators can be obtained from the same SwipeItems instance.
        /// </summary>
        [Fact]
        public void GetEnumerator_MultipleEnumerations_WorksCorrectly()
        {
            // Arrange
            var mockSwipeItem1 = Substitute.For<ISwipeItem>();
            var mockSwipeItem2 = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(mockSwipeItem1);
            swipeItems.Add(mockSwipeItem2);

            // Act
            var firstEnumerator = swipeItems.GetEnumerator();
            var secondEnumerator = swipeItems.GetEnumerator();

            var firstItems = new List<ISwipeItem>();
            while (firstEnumerator.MoveNext())
            {
                firstItems.Add(firstEnumerator.Current);
            }

            var secondItems = new List<ISwipeItem>();
            while (secondEnumerator.MoveNext())
            {
                secondItems.Add(secondEnumerator.Current);
            }

            // Assert
            Assert.Equal(2, firstItems.Count);
            Assert.Equal(2, secondItems.Count);
            Assert.Equal(firstItems[0], secondItems[0]);
            Assert.Equal(firstItems[1], secondItems[1]);
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with foreach syntax.
        /// Verifies that the yield return implementation is compatible with foreach loops.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithForeachLoop_YieldsAllItems()
        {
            // Arrange
            var mockSwipeItem1 = Substitute.For<ISwipeItem>();
            var mockSwipeItem2 = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(mockSwipeItem1);
            swipeItems.Add(mockSwipeItem2);

            // Act
            var items = new List<ISwipeItem>();
            foreach (var item in swipeItems)
            {
                items.Add(item);
            }

            // Assert
            Assert.Equal(2, items.Count);
            Assert.Equal(mockSwipeItem1, items[0]);
            Assert.Equal(mockSwipeItem2, items[1]);
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly when initialized with items via constructor.
        /// Verifies that items passed to the constructor are properly enumerated.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithConstructorInitializedItems_ReturnsAllItems()
        {
            // Arrange
            var mockSwipeItem1 = Substitute.For<ISwipeItem>();
            var mockSwipeItem2 = Substitute.For<ISwipeItem>();
            var initialItems = new List<ISwipeItem> { mockSwipeItem1, mockSwipeItem2 };
            var swipeItems = new SwipeItems(initialItems);

            // Act
            var enumeratedItems = swipeItems.ToList();

            // Assert
            Assert.Equal(2, enumeratedItems.Count);
            Assert.Equal(mockSwipeItem1, enumeratedItems[0]);
            Assert.Equal(mockSwipeItem2, enumeratedItems[1]);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when called on an empty SwipeItems collection.
        /// Input conditions: Empty SwipeItems collection and any ISwipeItem.
        /// Expected result: Returns -1 indicating the item is not found.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsMinusOne()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var swipeItem = new SwipeItem();

            // Act
            var result = swipeItems.IndexOf(swipeItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the collection.
        /// Input conditions: SwipeItems collection with multiple items and searching for existing items.
        /// Expected result: Returns the correct zero-based index of each item.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void IndexOf_ItemExists_ReturnsCorrectIndex(int expectedIndex)
        {
            // Arrange
            var swipeItem1 = new SwipeItem { Text = "Item1" };
            var swipeItem2 = new SwipeItem { Text = "Item2" };
            var swipeItem3 = new SwipeItem { Text = "Item3" };
            var swipeItems = new SwipeItems { swipeItem1, swipeItem2, swipeItem3 };
            var itemsArray = new[] { swipeItem1, swipeItem2, swipeItem3 };

            // Act
            var result = swipeItems.IndexOf(itemsArray[expectedIndex]);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when searching for an item that doesn't exist in the collection.
        /// Input conditions: SwipeItems collection with items and searching for a different item.
        /// Expected result: Returns -1 indicating the item is not found.
        /// </summary>
        [Fact]
        public void IndexOf_ItemDoesNotExist_ReturnsMinusOne()
        {
            // Arrange
            var swipeItem1 = new SwipeItem { Text = "Item1" };
            var swipeItem2 = new SwipeItem { Text = "Item2" };
            var searchItem = new SwipeItem { Text = "SearchItem" };
            var swipeItems = new SwipeItems { swipeItem1, swipeItem2 };

            // Act
            var result = swipeItems.IndexOf(searchItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf handles null input parameter correctly.
        /// Input conditions: SwipeItems collection and null ISwipeItem parameter.
        /// Expected result: Returns -1 for null item (following ObservableCollection behavior).
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var swipeItem = new SwipeItem { Text = "Item1" };
            var swipeItems = new SwipeItems { swipeItem };

            // Act
            var result = swipeItems.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when duplicate items exist.
        /// Input conditions: SwipeItems collection with the same item added multiple times.
        /// Expected result: Returns the index of the first occurrence.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrence()
        {
            // Arrange
            var swipeItem = new SwipeItem { Text = "DuplicateItem" };
            var otherItem = new SwipeItem { Text = "OtherItem" };
            var swipeItems = new SwipeItems { swipeItem, otherItem, swipeItem };

            // Act
            var result = swipeItems.IndexOf(swipeItem);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf works correctly with a single item collection.
        /// Input conditions: SwipeItems collection with one item.
        /// Expected result: Returns 0 when searching for the existing item, -1 for non-existing item.
        /// </summary>
        [Fact]
        public void IndexOf_SingleItemCollection_ReturnsCorrectResult()
        {
            // Arrange
            var swipeItem = new SwipeItem { Text = "SingleItem" };
            var otherItem = new SwipeItem { Text = "OtherItem" };
            var swipeItems = new SwipeItems { swipeItem };

            // Act
            var existingItemResult = swipeItems.IndexOf(swipeItem);
            var nonExistingItemResult = swipeItems.IndexOf(otherItem);

            // Assert
            Assert.Equal(0, existingItemResult);
            Assert.Equal(-1, nonExistingItemResult);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the beginning of an empty collection.
        /// Verifies the item is inserted at index 0 and the collection count increases to 1.
        /// </summary>
        [Fact]
        public void Insert_AtIndexZeroInEmptyCollection_InsertsItemSuccessfully()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();

            // Act
            swipeItems.Insert(0, mockItem);

            // Assert
            Assert.Equal(1, swipeItems.Count);
            Assert.Equal(mockItem, swipeItems[0]);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the beginning of a non-empty collection.
        /// Verifies the item is inserted at index 0, existing items are shifted, and collection count increases.
        /// </summary>
        [Fact]
        public void Insert_AtIndexZeroInNonEmptyCollection_InsertsItemAtBeginning()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var existingItem = Substitute.For<ISwipeItem>();
            var newItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(existingItem);

            // Act
            swipeItems.Insert(0, newItem);

            // Assert
            Assert.Equal(2, swipeItems.Count);
            Assert.Equal(newItem, swipeItems[0]);
            Assert.Equal(existingItem, swipeItems[1]);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item in the middle of a collection.
        /// Verifies the item is inserted at the correct index and existing items are properly shifted.
        /// </summary>
        [Fact]
        public void Insert_AtMiddleIndex_InsertsItemAtCorrectPosition()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var item1 = Substitute.For<ISwipeItem>();
            var item2 = Substitute.For<ISwipeItem>();
            var newItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(item1);
            swipeItems.Add(item2);

            // Act
            swipeItems.Insert(1, newItem);

            // Assert
            Assert.Equal(3, swipeItems.Count);
            Assert.Equal(item1, swipeItems[0]);
            Assert.Equal(newItem, swipeItems[1]);
            Assert.Equal(item2, swipeItems[2]);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the end of a collection.
        /// Verifies the item is inserted at the last position when index equals Count.
        /// </summary>
        [Fact]
        public void Insert_AtEndIndex_InsertsItemAtEnd()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var existingItem = Substitute.For<ISwipeItem>();
            var newItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(existingItem);

            // Act
            swipeItems.Insert(1, newItem);

            // Assert
            Assert.Equal(2, swipeItems.Count);
            Assert.Equal(existingItem, swipeItems[0]);
            Assert.Equal(newItem, swipeItems[1]);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when called with a negative index.
        /// Verifies that invalid index values are properly rejected.
        /// </summary>
        [Fact]
        public void Insert_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => swipeItems.Insert(-1, mockItem));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when called with an index greater than Count.
        /// Verifies that out-of-bounds index values are properly rejected.
        /// </summary>
        [Fact]
        public void Insert_WithIndexGreaterThanCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var existingItem = Substitute.For<ISwipeItem>();
            var newItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(existingItem);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => swipeItems.Insert(2, newItem));
        }

        /// <summary>
        /// Tests that Insert method accepts null items without throwing an exception.
        /// Verifies the null item is inserted at the correct position and collection count increases.
        /// </summary>
        [Fact]
        public void Insert_WithNullItem_InsertsNullSuccessfully()
        {
            // Arrange
            var swipeItems = new SwipeItems();

            // Act
            swipeItems.Insert(0, null);

            // Assert
            Assert.Equal(1, swipeItems.Count);
            Assert.Null(swipeItems[0]);
        }

        /// <summary>
        /// Tests that Insert method raises CollectionChanged event with correct arguments.
        /// Verifies the event is raised with Add action, correct item, and correct index.
        /// </summary>
        [Fact]
        public void Insert_ValidItem_RaisesCollectionChangedEvent()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            NotifyCollectionChangedEventArgs eventArgs = null;
            swipeItems.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            swipeItems.Insert(0, mockItem);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Equal(mockItem, eventArgs.NewItems[0]);
            Assert.Equal(0, eventArgs.NewStartingIndex);
        }

        /// <summary>
        /// Tests Insert method with multiple edge case index values using parameterized test.
        /// Verifies that various invalid index values consistently throw ArgumentOutOfRangeException.
        /// </summary>
        [Theory]
        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void Insert_WithInvalidIndices_ThrowsArgumentOutOfRangeException(int invalidIndex)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => swipeItems.Insert(invalidIndex, mockItem));
        }

        /// <summary>
        /// Tests Insert method with various valid index positions using parameterized test.
        /// Verifies that items are inserted at correct positions in collections of different sizes.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(3, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        public void Insert_WithValidIndices_InsertsItemAtCorrectPosition(int collectionSize, int insertIndex)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            for (int i = 0; i < collectionSize; i++)
            {
                swipeItems.Add(Substitute.For<ISwipeItem>());
            }
            var newItem = Substitute.For<ISwipeItem>();

            // Act
            swipeItems.Insert(insertIndex, newItem);

            // Assert
            Assert.Equal(collectionSize + 1, swipeItems.Count);
            Assert.Equal(newItem, swipeItems[insertIndex]);
        }

        /// <summary>
        /// Tests that Insert method handles Element items by setting up parent-child relationships.
        /// Verifies that when an Element implementing ISwipeItem is inserted, the logical parent is set correctly.
        /// </summary>
        [Fact]
        public void Insert_WithElementImplementingISwipeItem_SetsUpParentChildRelationship()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var elementItem = new TestSwipeItemElement();

            // Act
            swipeItems.Insert(0, elementItem);

            // Assert
            Assert.Equal(1, swipeItems.Count);
            Assert.Equal(elementItem, swipeItems[0]);
            Assert.Equal(swipeItems, elementItem.Parent);
        }

        /// <summary>
        /// Tests that Remove method returns true when removing an existing item from the collection.
        /// </summary>
        [Fact]
        public void Remove_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var mockItem = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(mockItem);

            // Act
            var result = swipeItems.Remove(mockItem);

            // Assert
            Assert.True(result);
            Assert.Equal(0, swipeItems.Count);
            Assert.False(swipeItems.Contains(mockItem));
        }

        /// <summary>
        /// Tests that Remove method returns false when attempting to remove an item that doesn't exist in the collection.
        /// </summary>
        [Fact]
        public void Remove_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var existingItem = Substitute.For<ISwipeItem>();
            var nonExistingItem = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(existingItem);

            // Act
            var result = swipeItems.Remove(nonExistingItem);

            // Assert
            Assert.False(result);
            Assert.Equal(1, swipeItems.Count);
            Assert.True(swipeItems.Contains(existingItem));
        }

        /// <summary>
        /// Tests that Remove method returns false when attempting to remove a null item.
        /// </summary>
        [Fact]
        public void Remove_NullItem_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();
            swipeItems.Add(mockItem);

            // Act
            var result = swipeItems.Remove(null);

            // Assert
            Assert.False(result);
            Assert.Equal(1, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Remove method returns false when attempting to remove an item from an empty collection.
        /// </summary>
        [Fact]
        public void Remove_FromEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var mockItem = Substitute.For<ISwipeItem>();

            // Act
            var result = swipeItems.Remove(mockItem);

            // Assert
            Assert.False(result);
            Assert.Equal(0, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Remove method returns false when attempting to remove the same item twice.
        /// The first removal should succeed, the second should fail.
        /// </summary>
        [Fact]
        public void Remove_SameItemTwice_SecondReturnsFalse()
        {
            // Arrange
            var mockItem = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(mockItem);

            // Act
            var firstResult = swipeItems.Remove(mockItem);
            var secondResult = swipeItems.Remove(mockItem);

            // Assert
            Assert.True(firstResult);
            Assert.False(secondResult);
            Assert.Equal(0, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Remove method works correctly with multiple items in the collection.
        /// Only the specified item should be removed while others remain.
        /// </summary>
        [Fact]
        public void Remove_FromMultipleItems_RemovesOnlySpecifiedItem()
        {
            // Arrange
            var item1 = Substitute.For<ISwipeItem>();
            var item2 = Substitute.For<ISwipeItem>();
            var item3 = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(item1);
            swipeItems.Add(item2);
            swipeItems.Add(item3);

            // Act
            var result = swipeItems.Remove(item2);

            // Assert
            Assert.True(result);
            Assert.Equal(2, swipeItems.Count);
            Assert.True(swipeItems.Contains(item1));
            Assert.False(swipeItems.Contains(item2));
            Assert.True(swipeItems.Contains(item3));
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes an item at a valid index and updates the collection count.
        /// </summary>
        [Theory]
        [InlineData(0, 1)] // Remove first item from single-item collection
        [InlineData(0, 3)] // Remove first item from multi-item collection
        [InlineData(1, 3)] // Remove middle item from multi-item collection
        [InlineData(2, 3)] // Remove last item from multi-item collection
        public void RemoveAt_ValidIndex_RemovesItemAndUpdatesCount(int indexToRemove, int initialCount)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var itemsToAdd = new List<SwipeItem>();

            for (int i = 0; i < initialCount; i++)
            {
                var item = new SwipeItem { Text = $"Item {i}" };
                itemsToAdd.Add(item);
                swipeItems.Add(item);
            }

            var itemToRemove = itemsToAdd[indexToRemove];
            var expectedCount = initialCount - 1;

            // Act
            swipeItems.RemoveAt(indexToRemove);

            // Assert
            Assert.Equal(expectedCount, swipeItems.Count);
            Assert.False(swipeItems.Contains(itemToRemove));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called with a negative index.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            swipeItems.Add(new SwipeItem { Text = "Test Item" });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => swipeItems.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called with an index equal to or greater than Count.
        /// </summary>
        [Theory]
        [InlineData(0, 0)] // Index 0 in empty collection
        [InlineData(1, 0)] // Index 1 in empty collection  
        [InlineData(1, 1)] // Index 1 in single-item collection
        [InlineData(3, 3)] // Index 3 in 3-item collection
        [InlineData(10, 3)] // Index 10 in 3-item collection
        [InlineData(int.MaxValue, 1)] // Max value index
        public void RemoveAt_IndexOutOfRange_ThrowsArgumentOutOfRangeException(int index, int collectionSize)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            for (int i = 0; i < collectionSize; i++)
            {
                swipeItems.Add(new SwipeItem { Text = $"Item {i}" });
            }

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => swipeItems.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt triggers CollectionChanged event when an item is removed.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_TriggersCollectionChangedEvent()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var item1 = new SwipeItem { Text = "Item 1" };
            var item2 = new SwipeItem { Text = "Item 2" };
            swipeItems.Add(item1);
            swipeItems.Add(item2);

            NotifyCollectionChangedEventArgs eventArgs = null;
            swipeItems.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            swipeItems.RemoveAt(0);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(0, eventArgs.OldStartingIndex);
            Assert.Contains(item1, eventArgs.OldItems.Cast<ISwipeItem>());
        }

        /// <summary>
        /// Tests that RemoveAt maintains correct order of remaining items after removal.
        /// </summary>
        [Fact]
        public void RemoveAt_MiddleIndex_MaintainsCorrectOrder()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var item1 = new SwipeItem { Text = "Item 1" };
            var item2 = new SwipeItem { Text = "Item 2" };
            var item3 = new SwipeItem { Text = "Item 3" };
            swipeItems.Add(item1);
            swipeItems.Add(item2);
            swipeItems.Add(item3);

            // Act
            swipeItems.RemoveAt(1); // Remove middle item

            // Assert
            Assert.Equal(2, swipeItems.Count);
            Assert.Equal(item1, swipeItems[0]);
            Assert.Equal(item3, swipeItems[1]);
        }

        /// <summary>
        /// Tests that RemoveAt works correctly when removing the last remaining item from the collection.
        /// </summary>
        [Fact]
        public void RemoveAt_LastRemainingItem_EmptiesCollection()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var item = new SwipeItem { Text = "Only Item" };
            swipeItems.Add(item);

            // Act
            swipeItems.RemoveAt(0);

            // Assert
            Assert.Equal(0, swipeItems.Count);
            Assert.False(swipeItems.Contains(item));
        }

        /// <summary>
        /// Tests that Count returns 0 when the collection is created with the default constructor.
        /// </summary>
        [Fact]
        public void Count_WhenCreatedWithDefaultConstructor_ReturnsZero()
        {
            // Arrange & Act
            var swipeItems = new SwipeItems();

            // Assert
            Assert.Equal(0, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count returns 0 when the collection is created with an empty enumerable.
        /// </summary>
        [Fact]
        public void Count_WhenCreatedWithEmptyEnumerable_ReturnsZero()
        {
            // Arrange
            var emptyItems = Enumerable.Empty<ISwipeItem>();

            // Act
            var swipeItems = new SwipeItems(emptyItems);

            // Assert
            Assert.Equal(0, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count returns the correct number when the collection is created with initial items.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public void Count_WhenCreatedWithItems_ReturnsCorrectCount(int itemCount)
        {
            // Arrange
            var items = new List<ISwipeItem>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(Substitute.For<ISwipeItem>());
            }

            // Act
            var swipeItems = new SwipeItems(items);

            // Assert
            Assert.Equal(itemCount, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count increases correctly after adding items to the collection.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void Count_AfterAddingItems_ReturnsIncreasedCount(int itemsToAdd)
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var initialCount = swipeItems.Count;

            // Act
            for (int i = 0; i < itemsToAdd; i++)
            {
                swipeItems.Add(Substitute.For<ISwipeItem>());
            }

            // Assert
            Assert.Equal(initialCount + itemsToAdd, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count increases correctly after inserting items at specific positions.
        /// </summary>
        [Fact]
        public void Count_AfterInsertingItem_ReturnsIncreasedCount()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            swipeItems.Add(Substitute.For<ISwipeItem>());
            var initialCount = swipeItems.Count;

            // Act
            swipeItems.Insert(0, Substitute.For<ISwipeItem>());

            // Assert
            Assert.Equal(initialCount + 1, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count decreases correctly after removing items from the collection.
        /// </summary>
        [Fact]
        public void Count_AfterRemovingItem_ReturnsDecreasedCount()
        {
            // Arrange
            var item = Substitute.For<ISwipeItem>();
            var swipeItems = new SwipeItems();
            swipeItems.Add(item);
            swipeItems.Add(Substitute.For<ISwipeItem>());
            var initialCount = swipeItems.Count;

            // Act
            var removed = swipeItems.Remove(item);

            // Assert
            Assert.True(removed);
            Assert.Equal(initialCount - 1, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count decreases correctly after removing items by index.
        /// </summary>
        [Fact]
        public void Count_AfterRemovingItemAtIndex_ReturnsDecreasedCount()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            swipeItems.Add(Substitute.For<ISwipeItem>());
            swipeItems.Add(Substitute.For<ISwipeItem>());
            var initialCount = swipeItems.Count;

            // Act
            swipeItems.RemoveAt(0);

            // Assert
            Assert.Equal(initialCount - 1, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count returns 0 after clearing the collection.
        /// </summary>
        [Fact]
        public void Count_AfterClearing_ReturnsZero()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            swipeItems.Add(Substitute.For<ISwipeItem>());
            swipeItems.Add(Substitute.For<ISwipeItem>());
            swipeItems.Add(Substitute.For<ISwipeItem>());

            // Act
            swipeItems.Clear();

            // Assert
            Assert.Equal(0, swipeItems.Count);
        }

        /// <summary>
        /// Tests that Count remains consistent during multiple operations.
        /// </summary>
        [Fact]
        public void Count_DuringMultipleOperations_RemainsConsistent()
        {
            // Arrange
            var swipeItems = new SwipeItems();
            var item1 = Substitute.For<ISwipeItem>();
            var item2 = Substitute.For<ISwipeItem>();
            var item3 = Substitute.For<ISwipeItem>();

            // Act & Assert
            Assert.Equal(0, swipeItems.Count);

            swipeItems.Add(item1);
            Assert.Equal(1, swipeItems.Count);

            swipeItems.Add(item2);
            Assert.Equal(2, swipeItems.Count);

            swipeItems.Insert(1, item3);
            Assert.Equal(3, swipeItems.Count);

            swipeItems.Remove(item2);
            Assert.Equal(2, swipeItems.Count);

            swipeItems.RemoveAt(0);
            Assert.Equal(1, swipeItems.Count);

            swipeItems.Clear();
            Assert.Equal(0, swipeItems.Count);
        }
    }
}