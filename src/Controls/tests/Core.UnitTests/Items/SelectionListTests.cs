#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for SelectionList class
    /// </summary>
    public class SelectionListTests
    {
        /// <summary>
        /// Tests that Count returns 0 when SelectionList is created with no initial items
        /// </summary>
        [Fact]
        public void Count_WithNoInitialItems_ReturnsZero()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(selectableItemsView);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns 0 when SelectionList is created with explicit null items parameter
        /// </summary>
        [Fact]
        public void Count_WithExplicitNullItems_ReturnsZero()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(selectableItemsView, null);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns 0 when SelectionList is created with empty list
        /// </summary>
        [Fact]
        public void Count_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var emptyList = new List<object>();
            var selectionList = new SelectionList(selectableItemsView, emptyList);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns correct value when SelectionList is created with single item
        /// </summary>
        [Fact]
        public void Count_WithSingleItem_ReturnsOne()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var singleItemList = new List<object> { "item1" };
            var selectionList = new SelectionList(selectableItemsView, singleItemList);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(1, count);
        }

        /// <summary>
        /// Tests that Count returns correct value for various numbers of items using parameterized test
        /// </summary>
        /// <param name="itemCount">Number of items to test with</param>
        /// <param name="expectedCount">Expected count value</param>
        [Theory]
        [InlineData(2, 2)]
        [InlineData(5, 5)]
        [InlineData(10, 10)]
        [InlineData(100, 100)]
        public void Count_WithMultipleItems_ReturnsCorrectCount(int itemCount, int expectedCount)
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add($"item{i}");
            }
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that Count includes null items in the count
        /// </summary>
        [Fact]
        public void Count_WithNullItems_IncludesNullInCount()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var itemsWithNulls = new List<object> { "item1", null, "item2", null, "item3" };
            var selectionList = new SelectionList(selectableItemsView, itemsWithNulls);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(5, count);
        }

        /// <summary>
        /// Tests that Count works correctly with list containing only null items
        /// </summary>
        [Fact]
        public void Count_WithOnlyNullItems_ReturnsCorrectCount()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var nullOnlyList = new List<object> { null, null, null };
            var selectionList = new SelectionList(selectableItemsView, nullOnlyList);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(3, count);
        }

        /// <summary>
        /// Tests that Count works with different object types in the list
        /// </summary>
        [Fact]
        public void Count_WithMixedObjectTypes_ReturnsCorrectCount()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var mixedItems = new List<object> { "string", 42, new DateTime(2023, 1, 1), new object() };
            var selectionList = new SelectionList(selectableItemsView, mixedItems);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(4, count);
        }

        /// <summary>
        /// Tests that Count works with ObservableCollection as initial items
        /// </summary>
        [Fact]
        public void Count_WithObservableCollection_ReturnsCorrectCount()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var observableCollection = new System.Collections.ObjectModel.ObservableCollection<object> { "item1", "item2", "item3" };
            var selectionList = new SelectionList(selectableItemsView, observableCollection);

            // Act
            var count = selectionList.Count;

            // Assert
            Assert.Equal(3, count);
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false, indicating the SelectionList is modifiable.
        /// This validates that the collection allows modification operations.
        /// </summary>
        [Fact]
        public void IsReadOnly_Always_ReturnsFalse()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act
            var isReadOnly = selectionList.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that Clear method properly clears an empty selection list.
        /// Verifies that SelectedItemsPropertyChanged is called with shadow and empty list parameters.
        /// </summary>
        [Fact]
        public void Clear_EmptyList_ClearsAndNotifiesChange()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act
            selectionList.Clear();

            // Assert
            Assert.Equal(0, selectionList.Count);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Clear method properly clears a selection list containing items.
        /// Verifies that internal list is cleared, shadow is cleared, and SelectedItemsPropertyChanged is called.
        /// </summary>
        [Fact]
        public void Clear_ListWithItems_ClearsAllItemsAndNotifiesChange()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2", "item3" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Verify initial state
            Assert.Equal(3, selectionList.Count);

            // Act
            selectionList.Clear();

            // Assert
            Assert.Equal(0, selectionList.Count);
            Assert.Equal(0, initialItems.Count); // Internal list should be cleared
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Clear method calls SelectedItemsPropertyChanged with correct parameters.
        /// Verifies that shadow (old selection) and empty list (new selection) are passed correctly.
        /// </summary>
        [Fact]
        public void Clear_WithItems_CallsSelectedItemsPropertyChangedWithCorrectParameters()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            selectionList.Clear();

            // Assert
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(shadow => shadow.Count == 0), // Shadow should be cleared
                Arg.Is<IList<object>>(empty => empty.Count == 0)    // Empty list should be passed
            );
        }

        /// <summary>
        /// Tests that Clear method can be called multiple times without throwing exceptions.
        /// Verifies that calling Clear on an already empty list behaves correctly.
        /// </summary>
        [Fact]
        public void Clear_CalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act & Assert - should not throw
            selectionList.Clear();
            selectionList.Clear();
            selectionList.Clear();

            Assert.Equal(0, selectionList.Count);
            mockSelectableItemsView.Received(3).SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Clear method works correctly with various object types in the list.
        /// Verifies that different object types can be cleared without issues.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void Clear_WithDifferentObjectTypes_ClearsSuccessfully(object item)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { item };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            Assert.Equal(1, selectionList.Count);

            // Act
            selectionList.Clear();

            // Assert
            Assert.Equal(0, selectionList.Count);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Clear method works correctly when initialized with null items parameter.
        /// Verifies that the internal list is properly initialized and can be cleared.
        /// </summary>
        [Fact]
        public void Clear_InitializedWithNullItems_ClearsSuccessfully()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView, null);

            // Act & Assert - should not throw
            selectionList.Clear();

            Assert.Equal(0, selectionList.Count);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the selection list.
        /// Verifies the basic functionality of the Contains method with a string item.
        /// </summary>
        [Fact]
        public void Contains_ItemExists_ReturnsTrue()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var testItem = "test item";
            var initialItems = new List<object> { testItem, "other item" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.Contains(testItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the selection list.
        /// Verifies the method correctly identifies when an item is not present.
        /// </summary>
        [Fact]
        public void Contains_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);
            var nonExistentItem = "non-existent item";

            // Act
            var result = selectionList.Contains(nonExistentItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when searching for null and null exists in the list.
        /// Verifies proper null handling when null is present in the selection list.
        /// </summary>
        [Fact]
        public void Contains_NullItemWhenNullExists_ReturnsTrue()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", null, "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.Contains(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for null and null does not exist in the list.
        /// Verifies proper null handling when null is not present in the selection list.
        /// </summary>
        [Fact]
        public void Contains_NullItemWhenNullDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching in an empty selection list.
        /// Verifies the method works correctly with an empty list.
        /// </summary>
        [Fact]
        public void Contains_EmptyList_ReturnsFalse()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act
            var result = selectionList.Contains("any item");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with different object types and uses proper equality comparison.
        /// Verifies the method handles various data types including strings, integers, and custom objects.
        /// </summary>
        [Theory]
        [InlineData("string item", "string item", true)]
        [InlineData("string item", "different string", false)]
        [InlineData(42, 42, true)]
        [InlineData(42, 43, false)]
        [InlineData(0, 0, true)]
        [InlineData(-1, -1, true)]
        public void Contains_WithDifferentObjectTypes_ReturnsCorrectResult(object itemToAdd, object itemToSearch, bool expected)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { itemToAdd };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.Contains(itemToSearch);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that Contains works correctly when items are added after construction.
        /// Verifies the method reflects changes made to the list after it was created.
        /// </summary>
        [Fact]
        public void Contains_ItemAddedAfterConstruction_ReturnsTrue()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var testItem = "added item";

            // Act
            selectionList.Add(testItem);
            var result = selectionList.Contains(testItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests Contains behavior with boundary values for numeric types.
        /// Verifies the method correctly handles extreme numeric values.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Contains_BoundaryValues_ReturnsCorrectResult(object value)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { value };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.Contains(value);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests CopyTo method throws ArgumentNullException when array parameter is null.
        /// Input: null array
        /// Expected: ArgumentNullException
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => selectionList.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests CopyTo method throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input: valid array, negative arrayIndex
        /// Expected: ArgumentOutOfRangeException
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException(int arrayIndex)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var array = new object[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests CopyTo method throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// Input: valid array, arrayIndex >= array.Length
        /// Expected: ArgumentException
        /// </summary>
        [Theory]
        [InlineData(5, 5)]
        [InlineData(5, 6)]
        [InlineData(5, 10)]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        public void CopyTo_ArrayIndexOutOfBounds_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            selectionList.Add("item1");
            var array = new object[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => selectionList.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests CopyTo method throws ArgumentException when array has insufficient space.
        /// Input: array with insufficient space from arrayIndex
        /// Expected: ArgumentException
        /// </summary>
        [Fact]
        public void CopyTo_InsufficientSpace_ThrowsArgumentException()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            selectionList.Add("item1");
            selectionList.Add("item2");
            selectionList.Add("item3");
            var array = new object[4]; // Need 3 items starting at index 2, but only 2 spaces available

            // Act & Assert
            Assert.Throws<ArgumentException>(() => selectionList.CopyTo(array, 2));
        }

        /// <summary>
        /// Tests CopyTo method successfully copies empty collection to array.
        /// Input: empty SelectionList, valid array and arrayIndex
        /// Expected: no exception, array remains unchanged at specified positions
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollection_DoesNotModifyArray()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var array = new object[] { "existing1", "existing2", "existing3" };
            var originalArray = new object[] { "existing1", "existing2", "existing3" };

            // Act
            selectionList.CopyTo(array, 1);

            // Assert
            Assert.Equal(originalArray, array);
        }

        /// <summary>
        /// Tests CopyTo method successfully copies single item to array.
        /// Input: SelectionList with one item, valid array and arrayIndex
        /// Expected: item copied to correct position in array
        /// </summary>
        [Fact]
        public void CopyTo_SingleItem_CopiesCorrectly()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var testItem = "test_item";
            selectionList.Add(testItem);
            var array = new object[3];

            // Act
            selectionList.CopyTo(array, 1);

            // Assert
            Assert.Null(array[0]);
            Assert.Equal(testItem, array[1]);
            Assert.Null(array[2]);
        }

        /// <summary>
        /// Tests CopyTo method successfully copies multiple items to array at start index.
        /// Input: SelectionList with multiple items, array starting at index 0
        /// Expected: all items copied to correct positions in array
        /// </summary>
        [Fact]
        public void CopyTo_MultipleItemsAtStart_CopiesCorrectly()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var item1 = "item1";
            var item2 = "item2";
            var item3 = "item3";
            selectionList.Add(item1);
            selectionList.Add(item2);
            selectionList.Add(item3);
            var array = new object[5];

            // Act
            selectionList.CopyTo(array, 0);

            // Assert
            Assert.Equal(item1, array[0]);
            Assert.Equal(item2, array[1]);
            Assert.Equal(item3, array[2]);
            Assert.Null(array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests CopyTo method successfully copies multiple items to array at middle index.
        /// Input: SelectionList with multiple items, array starting at middle index
        /// Expected: all items copied to correct positions in array
        /// </summary>
        [Fact]
        public void CopyTo_MultipleItemsAtMiddleIndex_CopiesCorrectly()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var item1 = "item1";
            var item2 = "item2";
            selectionList.Add(item1);
            selectionList.Add(item2);
            var array = new object[5];
            array[0] = "existing";
            array[1] = "existing";

            // Act
            selectionList.CopyTo(array, 2);

            // Assert
            Assert.Equal("existing", array[0]);
            Assert.Equal("existing", array[1]);
            Assert.Equal(item1, array[2]);
            Assert.Equal(item2, array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests CopyTo method with exact array size matches collection size.
        /// Input: SelectionList and array of same size, arrayIndex 0
        /// Expected: all items copied correctly, array fully filled
        /// </summary>
        [Fact]
        public void CopyTo_ExactArraySize_CopiesCorrectly()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var item1 = "item1";
            var item2 = "item2";
            selectionList.Add(item1);
            selectionList.Add(item2);
            var array = new object[2];

            // Act
            selectionList.CopyTo(array, 0);

            // Assert
            Assert.Equal(item1, array[0]);
            Assert.Equal(item2, array[1]);
        }

        /// <summary>
        /// Tests CopyTo method with null and various object types in collection.
        /// Input: SelectionList with null values and different object types
        /// Expected: all items including null values copied correctly
        /// </summary>
        [Fact]
        public void CopyTo_WithNullAndVariousTypes_CopiesCorrectly()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var stringItem = "string";
            var intItem = 42;
            object nullItem = null;
            selectionList.Add(stringItem);
            selectionList.Add(intItem);
            selectionList.Add(nullItem);
            var array = new object[4];

            // Act
            selectionList.CopyTo(array, 1);

            // Assert
            Assert.Null(array[0]);
            Assert.Equal(stringItem, array[1]);
            Assert.Equal(intItem, array[2]);
            Assert.Null(array[3]);
        }

        /// <summary>
        /// Tests CopyTo method with maximum integer arrayIndex within bounds.
        /// Input: large but valid arrayIndex
        /// Expected: items copied correctly to high indices
        /// </summary>
        [Fact]
        public void CopyTo_LargeValidArrayIndex_CopiesCorrectly()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var item = "test_item";
            selectionList.Add(item);
            var array = new object[1000];
            var arrayIndex = 999;

            // Act
            selectionList.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(item, array[arrayIndex]);
        }

        /// <summary>
        /// Tests IndexOf method with null item input.
        /// Verifies that the method correctly handles null values and returns the appropriate index.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsCorrectIndex()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", null, "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(null);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests IndexOf method with null item when null is not in the list.
        /// Verifies that the method returns -1 when null is not found.
        /// </summary>
        [Fact]
        public void IndexOf_NullItemNotInList_ReturnsNegativeOne()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method with existing items at various positions.
        /// Verifies that the method returns the correct index for items that exist in the list.
        /// </summary>
        [Theory]
        [InlineData("first", 0)]
        [InlineData("second", 1)]
        [InlineData("third", 2)]
        public void IndexOf_ExistingItem_ReturnsCorrectIndex(object item, int expectedIndex)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "first", "second", "third" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(item);

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOf method with non-existing item.
        /// Verifies that the method returns -1 when the item is not found in the list.
        /// </summary>
        [Fact]
        public void IndexOf_NonExistingItem_ReturnsNegativeOne()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2", "item3" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf("nonexistent");

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method with empty list.
        /// Verifies that the method returns -1 when searching in an empty list.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyList_ReturnsNegativeOne()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act
            var result = selectionList.IndexOf("anyitem");

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method with different object types.
        /// Verifies that the method works correctly with various object types including value types and reference types.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void IndexOf_DifferentObjectTypes_ReturnsCorrectIndex(object item)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "string", 42, true, 3.14, 'c' };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(item);

            // Assert
            Assert.True(result >= 0);
            Assert.Equal(item, initialItems[result]);
        }

        /// <summary>
        /// Tests IndexOf method with duplicate items.
        /// Verifies that the method returns the index of the first occurrence when duplicates exist.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var duplicateItem = "duplicate";
            var initialItems = new List<object> { "first", duplicateItem, "middle", duplicateItem, "last" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(duplicateItem);

            // Assert
            Assert.Equal(1, result); // Should return first occurrence
        }

        /// <summary>
        /// Tests IndexOf method with reference equality vs value equality.
        /// Verifies that the method uses reference equality for reference types.
        /// </summary>
        [Fact]
        public void IndexOf_ReferenceEquality_ReturnsCorrectIndex()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var specificObject = new object();
            var anotherObject = new object();
            var initialItems = new List<object> { "item1", specificObject, "item2", anotherObject };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(specificObject);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests IndexOf method with string objects having same content but different references.
        /// Verifies that the method handles string equality correctly.
        /// </summary>
        [Fact]
        public void IndexOf_StringEquality_ReturnsCorrectIndex()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var originalString = "test";
            var sameContentString = new string("test".ToCharArray()); // Different reference, same content
            var initialItems = new List<object> { "other", originalString, "another" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);

            // Act
            var result = selectionList.IndexOf(sameContentString);

            // Assert
            Assert.Equal(1, result); // Should find due to string value equality
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the beginning of an empty list.
        /// Verifies that the item is added to both internal and shadow collections and that
        /// SelectedItemsPropertyChanged is called with correct parameters.
        /// </summary>
        [Fact]
        public void Insert_AtBeginningOfEmptyList_InsertsItemAndNotifiesChange()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var testItem = new object();

            // Act
            selectionList.Insert(0, testItem);

            // Assert
            Assert.Equal(1, selectionList.Count);
            Assert.Equal(testItem, selectionList[0]);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(list => list.Count == 0),
                Arg.Is<IList<object>>(list => list.Count == 1 && list[0] == testItem));
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the beginning of a populated list.
        /// Verifies that existing items are shifted and SelectedItemsPropertyChanged is called.
        /// </summary>
        [Fact]
        public void Insert_AtBeginningOfPopulatedList_InsertsItemAndShiftsExistingItems()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);
            var newItem = "newItem";

            // Act
            selectionList.Insert(0, newItem);

            // Assert
            Assert.Equal(3, selectionList.Count);
            Assert.Equal(newItem, selectionList[0]);
            Assert.Equal("item1", selectionList[1]);
            Assert.Equal("item2", selectionList[2]);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(list => list.Count == 2),
                Arg.Is<IList<object>>(list => list.Count == 3 && list[0] == newItem));
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the end of a populated list.
        /// Verifies that the item is appended and SelectedItemsPropertyChanged is called.
        /// </summary>
        [Fact]
        public void Insert_AtEndOfList_InsertsItemAtEnd()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);
            var newItem = "newItem";

            // Act
            selectionList.Insert(2, newItem);

            // Assert
            Assert.Equal(3, selectionList.Count);
            Assert.Equal("item1", selectionList[0]);
            Assert.Equal("item2", selectionList[1]);
            Assert.Equal(newItem, selectionList[2]);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(list => list.Count == 2),
                Arg.Is<IList<object>>(list => list.Count == 3 && list[2] == newItem));
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item in the middle of a populated list.
        /// Verifies that items are properly shifted and SelectedItemsPropertyChanged is called.
        /// </summary>
        [Fact]
        public void Insert_InMiddleOfList_InsertsItemAndShiftsItems()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2", "item3" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);
            var newItem = "newItem";

            // Act
            selectionList.Insert(1, newItem);

            // Assert
            Assert.Equal(4, selectionList.Count);
            Assert.Equal("item1", selectionList[0]);
            Assert.Equal(newItem, selectionList[1]);
            Assert.Equal("item2", selectionList[2]);
            Assert.Equal("item3", selectionList[3]);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(list => list.Count == 3),
                Arg.Is<IList<object>>(list => list.Count == 4 && list[1] == newItem));
        }

        /// <summary>
        /// Tests that Insert method successfully inserts a null item.
        /// Verifies that null values are accepted and handled properly.
        /// </summary>
        [Fact]
        public void Insert_WithNullItem_InsertsNullItem()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act
            selectionList.Insert(0, null);

            // Assert
            Assert.Equal(1, selectionList.Count);
            Assert.Null(selectionList[0]);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(list => list.Count == 0),
                Arg.Is<IList<object>>(list => list.Count == 1 && list[0] == null));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is negative.
        /// Verifies proper boundary validation for the index parameter.
        /// </summary>
        [Fact]
        public void Insert_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var testItem = new object();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.Insert(-1, testItem));
            mockSelectableItemsView.DidNotReceive().SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index exceeds list count.
        /// Verifies proper boundary validation for the index parameter.
        /// </summary>
        [Fact]
        public void Insert_WithIndexBeyondCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var initialItems = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(mockSelectableItemsView, initialItems);
            var testItem = new object();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.Insert(3, testItem));
            mockSelectableItemsView.DidNotReceive().SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Insert method with various boundary index values.
        /// Verifies correct behavior at edge cases like int.MinValue and int.MaxValue.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-100)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void Insert_WithInvalidIndexValues_ThrowsArgumentOutOfRangeException(int invalidIndex)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);
            var testItem = new object();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.Insert(invalidIndex, testItem));
            mockSelectableItemsView.DidNotReceive().SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
        }

        /// <summary>
        /// Tests that Insert method properly handles different object types as items.
        /// Verifies that various object types can be inserted without issues.
        /// </summary>
        [Theory]
        [InlineData("string item")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void Insert_WithDifferentObjectTypes_InsertsSuccessfully(object item)
        {
            // Arrange
            var mockSelectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(mockSelectableItemsView);

            // Act
            selectionList.Insert(0, item);

            // Assert
            Assert.Equal(1, selectionList.Count);
            Assert.Equal(item, selectionList[0]);
            mockSelectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(list => list.Count == 0),
                Arg.Is<IList<object>>(list => list.Count == 1 && Equals(list[0], item)));
        }

        /// <summary>
        /// Tests that RemoveAt removes the item at the specified valid index and updates both internal and shadow collections.
        /// </summary>
        /// <param name="initialItems">The initial items in the list</param>
        /// <param name="indexToRemove">The index to remove</param>
        /// <param name="expectedCount">The expected count after removal</param>
        [Theory]
        [InlineData(new object[] { "item1" }, 0, 0)]
        [InlineData(new object[] { "item1", "item2" }, 0, 1)]
        [InlineData(new object[] { "item1", "item2" }, 1, 1)]
        [InlineData(new object[] { "item1", "item2", "item3" }, 1, 2)]
        public void RemoveAt_ValidIndex_RemovesItemAndUpdatesCount(object[] initialItems, int indexToRemove, int expectedCount)
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object>(initialItems);
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act
            selectionList.RemoveAt(indexToRemove);

            // Assert
            Assert.Equal(expectedCount, selectionList.Count);
        }

        /// <summary>
        /// Tests that RemoveAt calls SelectedItemsPropertyChanged with the correct old and new selection states.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_CallsSelectedItemsPropertyChanged()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object> { "item1", "item2", "item3" };
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act
            selectionList.RemoveAt(1);

            // Assert
            selectableItemsView.Received(1).SelectedItemsPropertyChanged(
                Arg.Is<IList<object>>(shadow => shadow.Count == 3 && shadow[1].Equals("item2")),
                Arg.Is<IList<object>>(current => current.Count == 2 && !current.Contains("item2"))
            );
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when index is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object> { "item1", "item2" };
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when index equals or exceeds the count.
        /// </summary>
        [Theory]
        [InlineData(2, 2)] // index equals count
        [InlineData(3, 2)] // index greater than count
        [InlineData(10, 2)] // index much greater than count
        [InlineData(int.MaxValue, 2)] // maximum index value
        public void RemoveAt_IndexOutOfRange_ThrowsArgumentOutOfRangeException(int index, int listCount)
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object>();
            for (int i = 0; i < listCount; i++)
            {
                items.Add($"item{i}");
            }
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called on an empty list.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void RemoveAt_EmptyList_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var selectionList = new SelectionList(selectableItemsView);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => selectionList.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt removes the correct item from the collection.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_RemovesCorrectItem()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object> { "first", "second", "third" };
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act
            selectionList.RemoveAt(1); // Remove "second"

            // Assert
            Assert.Equal(2, selectionList.Count);
            Assert.Equal("first", selectionList[0]);
            Assert.Equal("third", selectionList[1]);
            Assert.False(selectionList.Contains("second"));
        }

        /// <summary>
        /// Tests that RemoveAt works correctly when removing the first item.
        /// </summary>
        [Fact]
        public void RemoveAt_FirstIndex_RemovesFirstItem()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object> { "first", "second", "third" };
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act
            selectionList.RemoveAt(0);

            // Assert
            Assert.Equal(2, selectionList.Count);
            Assert.Equal("second", selectionList[0]);
            Assert.Equal("third", selectionList[1]);
        }

        /// <summary>
        /// Tests that RemoveAt works correctly when removing the last item.
        /// </summary>
        [Fact]
        public void RemoveAt_LastIndex_RemovesLastItem()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var items = new List<object> { "first", "second", "third" };
            var selectionList = new SelectionList(selectableItemsView, items);

            // Act
            selectionList.RemoveAt(2);

            // Assert
            Assert.Equal(2, selectionList.Count);
            Assert.Equal("first", selectionList[0]);
            Assert.Equal("second", selectionList[1]);
        }

        /// <summary>
        /// Tests that Remove returns true and calls notification methods when item is successfully removed from internal collection
        /// </summary>
        [Fact]
        public void Remove_ItemExistsInCollection_ReturnsTrueAndCallsNotifications()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var internalList = Substitute.For<IList<object>>();
            var shadowList = Substitute.For<IList<object>>();
            var testItem = new object();

            internalList.Remove(testItem).Returns(true);

            var selectionList = new SelectionList(selectableItemsView, internalList);
            // Set the shadow field using reflection since it's private
            var shadowField = typeof(SelectionList).GetField("_shadow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shadowField.SetValue(selectionList, shadowList);

            // Act
            bool result = selectionList.Remove(testItem);

            // Assert
            Assert.True(result);
            internalList.Received(1).Remove(testItem);
            selectableItemsView.Received(1).SelectedItemsPropertyChanged(shadowList, internalList);
            shadowList.Received(1).Remove(testItem);
        }

        /// <summary>
        /// Tests that Remove returns false and does not call notification methods when item does not exist in internal collection
        /// </summary>
        [Fact]
        public void Remove_ItemDoesNotExistInCollection_ReturnsFalseAndDoesNotCallNotifications()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var internalList = Substitute.For<IList<object>>();
            var shadowList = Substitute.For<IList<object>>();
            var testItem = new object();

            internalList.Remove(testItem).Returns(false);

            var selectionList = new SelectionList(selectableItemsView, internalList);
            // Set the shadow field using reflection since it's private
            var shadowField = typeof(SelectionList).GetField("_shadow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shadowField.SetValue(selectionList, shadowList);

            // Act
            bool result = selectionList.Remove(testItem);

            // Assert
            Assert.False(result);
            internalList.Received(1).Remove(testItem);
            selectableItemsView.DidNotReceive().SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
            shadowList.DidNotReceive().Remove(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that Remove handles null items correctly when item exists in collection
        /// </summary>
        [Fact]
        public void Remove_NullItemExistsInCollection_ReturnsTrueAndCallsNotifications()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var internalList = Substitute.For<IList<object>>();
            var shadowList = Substitute.For<IList<object>>();

            internalList.Remove(null).Returns(true);

            var selectionList = new SelectionList(selectableItemsView, internalList);
            // Set the shadow field using reflection since it's private
            var shadowField = typeof(SelectionList).GetField("_shadow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shadowField.SetValue(selectionList, shadowList);

            // Act
            bool result = selectionList.Remove(null);

            // Assert
            Assert.True(result);
            internalList.Received(1).Remove(null);
            selectableItemsView.Received(1).SelectedItemsPropertyChanged(shadowList, internalList);
            shadowList.Received(1).Remove(null);
        }

        /// <summary>
        /// Tests that Remove handles null items correctly when item does not exist in collection
        /// </summary>
        [Fact]
        public void Remove_NullItemDoesNotExistInCollection_ReturnsFalseAndDoesNotCallNotifications()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var internalList = Substitute.For<IList<object>>();
            var shadowList = Substitute.For<IList<object>>();

            internalList.Remove(null).Returns(false);

            var selectionList = new SelectionList(selectableItemsView, internalList);
            // Set the shadow field using reflection since it's private
            var shadowField = typeof(SelectionList).GetField("_shadow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shadowField.SetValue(selectionList, shadowList);

            // Act
            bool result = selectionList.Remove(null);

            // Assert
            Assert.False(result);
            internalList.Received(1).Remove(null);
            selectableItemsView.DidNotReceive().SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
            shadowList.DidNotReceive().Remove(Arg.Any<object>());
        }

        /// <summary>
        /// Tests that Remove properly manages _externalChange flag during execution
        /// </summary>
        [Fact]
        public void Remove_ExternalChangeFlagManagement_SetsAndResetsFlagCorrectly()
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var internalList = Substitute.For<IList<object>>();
            var testItem = new object();
            bool externalChangeValueDuringRemove = false;

            internalList.When(x => x.Remove(testItem)).Do(x =>
            {
                // Capture the _externalChange field value during the Remove call
                var externalChangeField = typeof(SelectionList).GetField("_externalChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                externalChangeValueDuringRemove = (bool)externalChangeField.GetValue(x.Target);
            });
            internalList.Remove(testItem).Returns(false);

            var selectionList = new SelectionList(selectableItemsView, internalList);

            // Act
            selectionList.Remove(testItem);

            // Assert - Verify that _externalChange was set to true during the operation
            Assert.True(externalChangeValueDuringRemove);

            // Verify that _externalChange is reset to false after the operation
            var externalChangeFieldAfter = typeof(SelectionList).GetField("_externalChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bool externalChangeValueAfter = (bool)externalChangeFieldAfter.GetValue(selectionList);
            Assert.False(externalChangeValueAfter);
        }

        /// <summary>
        /// Tests that Remove works with different object types including strings and integers
        /// </summary>
        [Theory]
        [InlineData("test string", true)]
        [InlineData("test string", false)]
        [InlineData(42, true)]
        [InlineData(42, false)]
        public void Remove_DifferentObjectTypes_HandlesCorrectly(object item, bool shouldExist)
        {
            // Arrange
            var selectableItemsView = Substitute.For<SelectableItemsView>();
            var internalList = Substitute.For<IList<object>>();
            var shadowList = Substitute.For<IList<object>>();

            internalList.Remove(item).Returns(shouldExist);

            var selectionList = new SelectionList(selectableItemsView, internalList);
            // Set the shadow field using reflection since it's private
            var shadowField = typeof(SelectionList).GetField("_shadow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            shadowField.SetValue(selectionList, shadowList);

            // Act
            bool result = selectionList.Remove(item);

            // Assert
            Assert.Equal(shouldExist, result);
            internalList.Received(1).Remove(item);

            if (shouldExist)
            {
                selectableItemsView.Received(1).SelectedItemsPropertyChanged(shadowList, internalList);
                shadowList.Received(1).Remove(item);
            }
            else
            {
                selectableItemsView.DidNotReceive().SelectedItemsPropertyChanged(Arg.Any<IList<object>>(), Arg.Any<IList<object>>());
                shadowList.DidNotReceive().Remove(Arg.Any<object>());
            }
        }

        /// <summary>
        /// Tests that the SelectionList constructor throws ArgumentNullException when selectableItemsView parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullSelectableItemsView_ThrowsArgumentNullException()
        {
            // Arrange
            SelectableItemsView selectableItemsView = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SelectionList(selectableItemsView));
            Assert.Equal("selectableItemsView", exception.ParamName);
        }

        /// <summary>
        /// Tests that the SelectionList constructor initializes correctly with valid selectableItemsView and null items parameter.
        /// Should create an empty internal list.
        /// </summary>
        [Fact]
        public void Constructor_ValidSelectableItemsViewNullItems_InitializesWithEmptyList()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            var selectionList = new SelectionList(selectableItemsView, null);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(0, selectionList.Count);
        }

        /// <summary>
        /// Tests that the SelectionList constructor initializes correctly with valid selectableItemsView and empty items list.
        /// Should use the provided empty list.
        /// </summary>
        [Fact]
        public void Constructor_ValidSelectableItemsViewEmptyItems_InitializesWithProvidedList()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var items = new List<object>();

            // Act
            var selectionList = new SelectionList(selectableItemsView, items);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(0, selectionList.Count);
        }

        /// <summary>
        /// Tests that the SelectionList constructor initializes correctly with valid selectableItemsView and non-empty items list.
        /// Should use the provided list with its items.
        /// </summary>
        [Fact]
        public void Constructor_ValidSelectableItemsViewNonEmptyItems_InitializesWithProvidedItems()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var items = new List<object> { "item1", "item2", 42 };

            // Act
            var selectionList = new SelectionList(selectableItemsView, items);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(3, selectionList.Count);
            Assert.Equal("item1", selectionList[0]);
            Assert.Equal("item2", selectionList[1]);
            Assert.Equal(42, selectionList[2]);
        }

        /// <summary>
        /// Tests that the SelectionList constructor does not subscribe to CollectionChanged event when items list 
        /// does not implement INotifyCollectionChanged.
        /// </summary>
        [Fact]
        public void Constructor_ItemsNotImplementingINotifyCollectionChanged_DoesNotSubscribeToEvent()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var items = new List<object> { "item1" };

            // Act
            var selectionList = new SelectionList(selectableItemsView, items);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(1, selectionList.Count);
            // No exception should be thrown and list should work normally
            Assert.Equal("item1", selectionList[0]);
        }

        /// <summary>
        /// Tests that the SelectionList constructor subscribes to CollectionChanged event when items list 
        /// implements INotifyCollectionChanged.
        /// </summary>
        [Fact]
        public void Constructor_ItemsImplementingINotifyCollectionChanged_SubscribesToCollectionChangedEvent()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var mockItems = Substitute.For<IList<object>, INotifyCollectionChanged>();
            mockItems.Count.Returns(2);
            mockItems[0].Returns("item1");
            mockItems[1].Returns("item2");

            // Act
            var selectionList = new SelectionList(selectableItemsView, mockItems);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(2, selectionList.Count);

            // Verify that the CollectionChanged event was subscribed to
            var notifyCollectionChanged = (INotifyCollectionChanged)mockItems;
            notifyCollectionChanged.Received(1).CollectionChanged += Arg.Any<NotifyCollectionChangedEventHandler>();
        }

        /// <summary>
        /// Tests that the SelectionList constructor works correctly with boundary case of maximum integer values in items.
        /// </summary>
        [Fact]
        public void Constructor_ItemsWithIntegerBoundaryValues_InitializesCorrectly()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var items = new List<object> { int.MinValue, int.MaxValue, 0, -1, 1 };

            // Act
            var selectionList = new SelectionList(selectableItemsView, items);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(5, selectionList.Count);
            Assert.Equal(int.MinValue, selectionList[0]);
            Assert.Equal(int.MaxValue, selectionList[1]);
            Assert.Equal(0, selectionList[2]);
            Assert.Equal(-1, selectionList[3]);
            Assert.Equal(1, selectionList[4]);
        }

        /// <summary>
        /// Tests that the SelectionList constructor works correctly with null objects in the items list.
        /// </summary>
        [Fact]
        public void Constructor_ItemsContainingNullObjects_InitializesCorrectly()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var items = new List<object> { "item1", null, "item3", null };

            // Act
            var selectionList = new SelectionList(selectableItemsView, items);

            // Assert
            Assert.NotNull(selectionList);
            Assert.Equal(4, selectionList.Count);
            Assert.Equal("item1", selectionList[0]);
            Assert.Null(selectionList[1]);
            Assert.Equal("item3", selectionList[2]);
            Assert.Null(selectionList[3]);
        }
    }
}