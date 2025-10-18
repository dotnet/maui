#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class MenuItemCollectionTests
    {
        /// <summary>
        /// Tests that calling Clear on an empty collection maintains Count as zero.
        /// </summary>
        [Fact]
        public void Clear_EmptyCollection_CountRemainsZero()
        {
            // Arrange
            var collection = new MenuItemCollection();

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that calling Clear on a collection with a single item removes the item.
        /// </summary>
        [Fact]
        public void Clear_CollectionWithSingleItem_RemovesItem()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            collection.Add(menuItem);

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that calling Clear on a collection with multiple items removes all items.
        /// </summary>
        [Fact]
        public void Clear_CollectionWithMultipleItems_RemovesAllItems()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());
            collection.Add(new MenuItem());
            collection.Add(new MenuItem());

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that calling Clear on a collection with items fires the CollectionChanged event with Reset action.
        /// </summary>
        [Fact]
        public void Clear_CollectionWithItems_FiresCollectionChangedEvent()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());
            collection.Add(new MenuItem());

            bool eventFired = false;
            NotifyCollectionChangedAction? eventAction = null;
            ((INotifyCollectionChanged)collection).CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventAction = e.Action;
            };

            // Act
            collection.Clear();

            // Assert
            Assert.True(eventFired);
            Assert.Equal(NotifyCollectionChangedAction.Reset, eventAction);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that calling Clear on an empty collection does not fire the CollectionChanged event.
        /// </summary>
        [Fact]
        public void Clear_EmptyCollection_DoesNotFireCollectionChangedEvent()
        {
            // Arrange
            var collection = new MenuItemCollection();
            bool eventFired = false;
            ((INotifyCollectionChanged)collection).CollectionChanged += (sender, e) =>
            {
                eventFired = true;
            };

            // Act
            collection.Clear();

            // Assert
            Assert.False(eventFired);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that Contains returns false when called with a null MenuItem.
        /// This verifies the method handles null input gracefully.
        /// </summary>
        [Fact]
        public void Contains_WithNullItem_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();
            MenuItem nullItem = null;

            // Act
            var result = collection.Contains(nullItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when called with a MenuItem that exists in the collection.
        /// This verifies the method correctly identifies existing items using reference equality.
        /// </summary>
        [Fact]
        public void Contains_WithExistingItem_ReturnsTrue()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            collection.Add(menuItem);

            // Act
            var result = collection.Contains(menuItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called with a MenuItem that does not exist in the collection.
        /// This verifies the method correctly identifies non-existing items.
        /// </summary>
        [Fact]
        public void Contains_WithNonExistingItem_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var existingItem = new MenuItem();
            var nonExistingItem = new MenuItem();
            collection.Add(existingItem);

            // Act
            var result = collection.Contains(nonExistingItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection.
        /// This verifies the method handles empty collection state correctly.
        /// </summary>
        [Fact]
        public void Contains_OnEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();

            // Act
            var result = collection.Contains(menuItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with multiple items in the collection.
        /// This verifies the method can find items in collections with multiple elements.
        /// </summary>
        [Fact]
        public void Contains_WithMultipleItems_ReturnsCorrectResult()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var firstItem = new MenuItem();
            var secondItem = new MenuItem();
            var thirdItem = new MenuItem();
            var notInCollectionItem = new MenuItem();

            collection.Add(firstItem);
            collection.Add(secondItem);
            collection.Add(thirdItem);

            // Act & Assert
            Assert.True(collection.Contains(firstItem));
            Assert.True(collection.Contains(secondItem));
            Assert.True(collection.Contains(thirdItem));
            Assert.False(collection.Contains(notInCollectionItem));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// </summary>
        [Fact]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());
            var array = new MenuItem[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, -1));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length and collection is not empty.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexEqualToArrayLength_ThrowsArgumentException()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());
            var array = new MenuItem[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => collection.CopyTo(array, 3));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is not enough space in the array.
        /// </summary>
        [Fact]
        public void CopyTo_InsufficientArraySpace_ThrowsArgumentException()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());
            collection.Add(new MenuItem());
            collection.Add(new MenuItem());
            var array = new MenuItem[4];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => collection.CopyTo(array, 2));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements from an empty collection.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollection_SucceedsWithoutCopying()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var array = new MenuItem[3];

            // Act
            collection.CopyTo(array, 1);

            // Assert
            Assert.All(array, item => Assert.Null(item));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies a single element to the beginning of an array.
        /// </summary>
        [Fact]
        public void CopyTo_SingleElement_CopiesToArrayStart()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem { Text = "Test Item" };
            collection.Add(menuItem);
            var array = new MenuItem[3];

            // Act
            collection.CopyTo(array, 0);

            // Assert
            Assert.Same(menuItem, array[0]);
            Assert.Null(array[1]);
            Assert.Null(array[2]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies a single element to a specific position in an array.
        /// </summary>
        [Fact]
        public void CopyTo_SingleElement_CopiesToArrayOffset()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem { Text = "Test Item" };
            collection.Add(menuItem);
            var array = new MenuItem[4];

            // Act
            collection.CopyTo(array, 2);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Same(menuItem, array[2]);
            Assert.Null(array[3]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies multiple elements preserving their order.
        /// </summary>
        [Fact]
        public void CopyTo_MultipleElements_CopiesToArrayInOrder()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem1 = new MenuItem { Text = "Item 1" };
            var menuItem2 = new MenuItem { Text = "Item 2" };
            var menuItem3 = new MenuItem { Text = "Item 3" };
            collection.Add(menuItem1);
            collection.Add(menuItem2);
            collection.Add(menuItem3);
            var array = new MenuItem[5];

            // Act
            collection.CopyTo(array, 1);

            // Assert
            Assert.Null(array[0]);
            Assert.Same(menuItem1, array[1]);
            Assert.Same(menuItem2, array[2]);
            Assert.Same(menuItem3, array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements to an array with exact fit.
        /// </summary>
        [Fact]
        public void CopyTo_ExactArrayFit_CopiesToArray()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem1 = new MenuItem { Text = "Item 1" };
            var menuItem2 = new MenuItem { Text = "Item 2" };
            collection.Add(menuItem1);
            collection.Add(menuItem2);
            var array = new MenuItem[2];

            // Act
            collection.CopyTo(array, 0);

            // Assert
            Assert.Same(menuItem1, array[0]);
            Assert.Same(menuItem2, array[1]);
        }

        /// <summary>
        /// Tests that CopyTo allows copying to array index equal to array length when collection is empty.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollectionArrayIndexEqualsLength_Succeeds()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var array = new MenuItem[2];

            // Act
            collection.CopyTo(array, 2);

            // Assert
            Assert.All(array, item => Assert.Null(item));
        }

        /// <summary>
        /// Tests IndexOf method when searching for a null item in a collection that doesn't contain null.
        /// Should return -1 indicating the item is not found.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            collection.Add(menuItem);

            // Act
            int result = collection.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method when searching for an item that exists at the first position (index 0).
        /// Should return 0.
        /// </summary>
        [Fact]
        public void IndexOf_ExistingItemAtFirstPosition_ReturnsZero()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var firstMenuItem = new MenuItem();
            var secondMenuItem = new MenuItem();
            collection.Add(firstMenuItem);
            collection.Add(secondMenuItem);

            // Act
            int result = collection.IndexOf(firstMenuItem);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests IndexOf method when searching for an item that exists at a middle position.
        /// Should return the correct index.
        /// </summary>
        [Fact]
        public void IndexOf_ExistingItemAtMiddlePosition_ReturnsCorrectIndex()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var firstMenuItem = new MenuItem();
            var middleMenuItem = new MenuItem();
            var lastMenuItem = new MenuItem();
            collection.Add(firstMenuItem);
            collection.Add(middleMenuItem);
            collection.Add(lastMenuItem);

            // Act
            int result = collection.IndexOf(middleMenuItem);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests IndexOf method when searching for an item that exists at the last position.
        /// Should return the correct index.
        /// </summary>
        [Fact]
        public void IndexOf_ExistingItemAtLastPosition_ReturnsCorrectIndex()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var firstMenuItem = new MenuItem();
            var secondMenuItem = new MenuItem();
            var lastMenuItem = new MenuItem();
            collection.Add(firstMenuItem);
            collection.Add(secondMenuItem);
            collection.Add(lastMenuItem);

            // Act
            int result = collection.IndexOf(lastMenuItem);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests IndexOf method when searching for an item that doesn't exist in the collection.
        /// Should return -1 indicating the item is not found.
        /// </summary>
        [Fact]
        public void IndexOf_NonExistingItem_ReturnsMinusOne()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var existingMenuItem = new MenuItem();
            var nonExistingMenuItem = new MenuItem();
            collection.Add(existingMenuItem);

            // Act
            int result = collection.IndexOf(nonExistingMenuItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method on an empty collection.
        /// Should return -1 indicating the item is not found.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsMinusOne()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();

            // Act
            int result = collection.IndexOf(menuItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method when the same item is added multiple times to the collection.
        /// Should return the index of the first occurrence.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            var otherMenuItem = new MenuItem();
            collection.Add(otherMenuItem);
            collection.Add(menuItem);
            collection.Add(menuItem); // Add the same item again

            // Act
            int result = collection.IndexOf(menuItem);

            // Assert
            Assert.Equal(1, result); // Should return index of first occurrence
        }

        /// <summary>
        /// Tests that Insert method correctly inserts a MenuItem at the beginning of an empty collection.
        /// Verifies the item is placed at index 0 and Count increases to 1.
        /// </summary>
        [Fact]
        public void Insert_ItemAtIndexZeroInEmptyCollection_InsertsCorrectly()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();

            // Act
            collection.Insert(0, menuItem);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(menuItem, collection[0]);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts a MenuItem at the beginning of a non-empty collection.
        /// Verifies existing items shift to the right and the new item is at index 0.
        /// </summary>
        [Fact]
        public void Insert_ItemAtIndexZeroInNonEmptyCollection_InsertsAtBeginningAndShiftsExistingItems()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var existingItem = new MenuItem();
            var newItem = new MenuItem();
            collection.Add(existingItem);

            // Act
            collection.Insert(0, newItem);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Equal(newItem, collection[0]);
            Assert.Equal(existingItem, collection[1]);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts a MenuItem in the middle of a collection.
        /// Verifies items are shifted correctly and the new item is at the specified index.
        /// </summary>
        [Fact]
        public void Insert_ItemInMiddleOfCollection_InsertsAtCorrectPosition()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var item1 = new MenuItem();
            var item2 = new MenuItem();
            var insertItem = new MenuItem();
            collection.Add(item1);
            collection.Add(item2);

            // Act
            collection.Insert(1, insertItem);

            // Assert
            Assert.Equal(3, collection.Count);
            Assert.Equal(item1, collection[0]);
            Assert.Equal(insertItem, collection[1]);
            Assert.Equal(item2, collection[2]);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts a MenuItem at the end of a collection.
        /// Verifies the item is appended and Count increases accordingly.
        /// </summary>
        [Fact]
        public void Insert_ItemAtEndOfCollection_AppendsCorrectly()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var existingItem = new MenuItem();
            var newItem = new MenuItem();
            collection.Add(existingItem);

            // Act
            collection.Insert(collection.Count, newItem);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Equal(existingItem, collection[0]);
            Assert.Equal(newItem, collection[1]);
        }

        /// <summary>
        /// Tests that Insert method allows null MenuItem insertion since nullable reference types are disabled.
        /// Verifies null items can be inserted without throwing exceptions.
        /// </summary>
        [Fact]
        public void Insert_NullItem_InsertsSuccessfully()
        {
            // Arrange
            var collection = new MenuItemCollection();

            // Act
            collection.Insert(0, null);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Null(collection[0]);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is negative.
        /// Verifies proper validation of index parameter lower bound.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void Insert_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(negativeIndex, menuItem));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index exceeds collection count.
        /// Verifies proper validation of index parameter upper bound.
        /// </summary>
        [Fact]
        public void Insert_IndexGreaterThanCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem());
            var menuItem = new MenuItem();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(2, menuItem));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is significantly larger than count.
        /// Verifies validation works for extreme values beyond the collection bounds.
        /// </summary>
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void Insert_IndexMuchGreaterThanCount_ThrowsArgumentOutOfRangeException(int largeIndex)
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(largeIndex, menuItem));
        }

        /// <summary>
        /// Tests that Remove returns true and removes the item when the item exists in the collection.
        /// </summary>
        [Fact]
        public void Remove_ItemExists_ReturnsTrueAndRemovesItem()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            collection.Add(menuItem);

            // Act
            bool result = collection.Remove(menuItem);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(menuItem, collection);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that Remove returns false when the item does not exist in the collection.
        /// </summary>
        [Fact]
        public void Remove_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var existingItem = new MenuItem();
            var nonExistentItem = new MenuItem();
            collection.Add(existingItem);

            // Act
            bool result = collection.Remove(nonExistentItem);

            // Assert
            Assert.False(result);
            Assert.Contains(existingItem, collection);
            Assert.Equal(1, collection.Count);
        }

        /// <summary>
        /// Tests that Remove returns false when attempting to remove from an empty collection.
        /// </summary>
        [Fact]
        public void Remove_EmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();

            // Act
            bool result = collection.Remove(menuItem);

            // Assert
            Assert.False(result);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that Remove handles null parameter according to ObservableCollection behavior.
        /// </summary>
        [Fact]
        public void Remove_NullItem_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            collection.Add(menuItem);

            // Act
            bool result = collection.Remove(null);

            // Assert
            Assert.False(result);
            Assert.Contains(menuItem, collection);
            Assert.Equal(1, collection.Count);
        }

        /// <summary>
        /// Tests that Remove only removes the first occurrence when there are duplicate items.
        /// </summary>
        [Fact]
        public void Remove_DuplicateItems_RemovesFirstOccurrenceOnly()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem1 = new MenuItem();
            var menuItem2 = new MenuItem();
            var duplicateItem = new MenuItem();

            collection.Add(menuItem1);
            collection.Add(duplicateItem);
            collection.Add(menuItem2);
            collection.Add(duplicateItem); // Add duplicate

            // Act
            bool result = collection.Remove(duplicateItem);

            // Assert
            Assert.True(result);
            Assert.Equal(3, collection.Count);
            Assert.Contains(duplicateItem, collection); // Second occurrence should still be there
        }

        /// <summary>
        /// Tests that CollectionChanged event is fired when an item is successfully removed.
        /// </summary>
        [Fact]
        public void Remove_ItemExists_FiresCollectionChangedEvent()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem = new MenuItem();
            collection.Add(menuItem);

            NotifyCollectionChangedEventArgs eventArgs = null;
            var notifyCollection = (INotifyCollectionChanged)collection;
            notifyCollection.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            bool result = collection.Remove(menuItem);

            // Assert
            Assert.True(result);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Contains(menuItem, eventArgs.OldItems);
        }

        /// <summary>
        /// Tests that no CollectionChanged event is fired when attempting to remove a non-existent item.
        /// </summary>
        [Fact]
        public void Remove_ItemDoesNotExist_DoesNotFireCollectionChangedEvent()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var existingItem = new MenuItem();
            var nonExistentItem = new MenuItem();
            collection.Add(existingItem);

            bool eventFired = false;
            var notifyCollection = (INotifyCollectionChanged)collection;
            notifyCollection.CollectionChanged += (sender, e) => eventFired = true;

            // Act
            bool result = collection.Remove(nonExistentItem);

            // Assert
            Assert.False(result);
            Assert.False(eventFired);
        }

        /// <summary>
        /// Tests that Remove works correctly with multiple different items in the collection.
        /// </summary>
        [Fact]
        public void Remove_MultipleItems_RemovesCorrectItem()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var menuItem1 = new MenuItem { Text = "Item1" };
            var menuItem2 = new MenuItem { Text = "Item2" };
            var menuItem3 = new MenuItem { Text = "Item3" };

            collection.Add(menuItem1);
            collection.Add(menuItem2);
            collection.Add(menuItem3);

            // Act
            bool result = collection.Remove(menuItem2);

            // Assert
            Assert.True(result);
            Assert.Equal(2, collection.Count);
            Assert.Contains(menuItem1, collection);
            Assert.DoesNotContain(menuItem2, collection);
            Assert.Contains(menuItem3, collection);
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes an item at a valid index from a collection with multiple items.
        /// Input: Collection with 3 items, removing at index 1.
        /// Expected: Item at index 1 is removed, Count decreases by 1, remaining items shift positions.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndexInMultipleItemCollection_RemovesItemAndShiftsRemaining()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var item1 = new MenuItem { Text = "Item1" };
            var item2 = new MenuItem { Text = "Item2" };
            var item3 = new MenuItem { Text = "Item3" };
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Act
            collection.RemoveAt(1);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Equal(item1, collection[0]);
            Assert.Equal(item3, collection[1]);
            Assert.False(collection.Contains(item2));
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the first item when index is 0.
        /// Input: Collection with multiple items, removing at index 0.
        /// Expected: First item is removed, remaining items shift down.
        /// </summary>
        [Fact]
        public void RemoveAt_IndexZero_RemovesFirstItem()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var item1 = new MenuItem { Text = "First" };
            var item2 = new MenuItem { Text = "Second" };
            collection.Add(item1);
            collection.Add(item2);

            // Act
            collection.RemoveAt(0);

            // Assert
            Assert.Single(collection);
            Assert.Equal(item2, collection[0]);
            Assert.False(collection.Contains(item1));
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the last item when index equals Count-1.
        /// Input: Collection with multiple items, removing at last index.
        /// Expected: Last item is removed, other items remain unchanged.
        /// </summary>
        [Fact]
        public void RemoveAt_LastIndex_RemovesLastItem()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var item1 = new MenuItem { Text = "First" };
            var item2 = new MenuItem { Text = "Last" };
            collection.Add(item1);
            collection.Add(item2);

            // Act
            collection.RemoveAt(1);

            // Assert
            Assert.Single(collection);
            Assert.Equal(item1, collection[0]);
            Assert.False(collection.Contains(item2));
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the only item from a single-item collection.
        /// Input: Collection with one item, removing at index 0.
        /// Expected: Collection becomes empty.
        /// </summary>
        [Fact]
        public void RemoveAt_SingleItemCollection_RemovesItemAndBecomesEmpty()
        {
            // Arrange
            var collection = new MenuItemCollection();
            var item = new MenuItem { Text = "OnlyItem" };
            collection.Add(item);

            // Act
            collection.RemoveAt(0);

            // Assert
            Assert.Empty(collection);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called on an empty collection.
        /// Input: Empty collection, any index value.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void RemoveAt_EmptyCollection_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var collection = new MenuItemCollection();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException for negative index values.
        /// Input: Collection with items, negative index values.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem { Text = "Item" });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when index is greater than or equal to Count.
        /// Input: Collection with items, index values >= Count.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(1)] // Collection has 1 item, so index 1 is invalid
        [InlineData(2)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void RemoveAt_IndexGreaterThanOrEqualToCount_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var collection = new MenuItemCollection();
            collection.Add(new MenuItem { Text = "Item" });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt works correctly at boundary values for a larger collection.
        /// Input: Collection with 5 items, removing at various boundary indices.
        /// Expected: Correct items are removed and collection state is maintained.
        /// </summary>
        [Theory]
        [InlineData(0)] // First item
        [InlineData(4)] // Last item (Count-1)
        [InlineData(2)] // Middle item
        public void RemoveAt_BoundaryIndicesInLargerCollection_RemovesCorrectItem(int index)
        {
            // Arrange
            var collection = new MenuItemCollection();
            var items = new MenuItem[5];
            for (int i = 0; i < 5; i++)
            {
                items[i] = new MenuItem { Text = $"Item{i}" };
                collection.Add(items[i]);
            }
            int originalCount = collection.Count;
            var itemToRemove = collection[index];

            // Act
            collection.RemoveAt(index);

            // Assert
            Assert.Equal(originalCount - 1, collection.Count);
            Assert.False(collection.Contains(itemToRemove));

            // Verify remaining items are in correct positions
            for (int i = 0; i < collection.Count; i++)
            {
                int originalIndex = i < index ? i : i + 1;
                Assert.Equal(items[originalIndex], collection[i]);
            }
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns false, indicating the collection can be modified.
        /// This verifies that the MenuItemCollection wrapper correctly exposes the underlying 
        /// ObservableCollection's read-only status through the IList interface.
        /// </summary>
        [Fact]
        public void IsReadOnly_Always_ReturnsFalse()
        {
            // Arrange
            var collection = new MenuItemCollection();

            // Act
            bool isReadOnly = collection.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }
    }
}