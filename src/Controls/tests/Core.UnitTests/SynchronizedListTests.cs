#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class SynchronizedListTests
    {
        /// <summary>
        /// Tests that RemoveAt successfully removes an item from the middle of a populated list and updates the count correctly.
        /// </summary>
        [Fact]
        public void RemoveAt_RemoveFromMiddle_RemovesItemAndDecreasesCount()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");
            var initialCount = list.Count;

            // Act
            list.RemoveAt(1);

            // Assert
            Assert.Equal(initialCount - 1, list.Count);
            Assert.Equal("first", list[0]);
            Assert.Equal("third", list[1]);
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the first item (index 0) from a populated list.
        /// </summary>
        [Fact]
        public void RemoveAt_RemoveFirstItem_RemovesItemAndDecreasesCount()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");
            var initialCount = list.Count;

            // Act
            list.RemoveAt(0);

            // Assert
            Assert.Equal(initialCount - 1, list.Count);
            Assert.Equal("second", list[0]);
            Assert.Equal("third", list[1]);
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the last item from a populated list.
        /// </summary>
        [Fact]
        public void RemoveAt_RemoveLastItem_RemovesItemAndDecreasesCount()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");
            var initialCount = list.Count;
            var lastIndex = initialCount - 1;

            // Act
            list.RemoveAt(lastIndex);

            // Assert
            Assert.Equal(initialCount - 1, list.Count);
            Assert.Equal("first", list[0]);
            Assert.Equal("second", list[1]);
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the only item from a single-item list, resulting in an empty list.
        /// </summary>
        [Fact]
        public void RemoveAt_RemoveFromSingleItemList_ResultsInEmptyList()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("only");

            // Act
            list.RemoveAt(0);

            // Assert
            Assert.Equal(0, list.Count);
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called with a negative index.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called with an index equal to or greater than the count.
        /// </summary>
        [Fact]
        public void RemoveAt_IndexEqualToCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            var count = list.Count;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(count));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called with an index greater than the count.
        /// </summary>
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void RemoveAt_IndexGreaterThanCount_ThrowsArgumentOutOfRangeException(int largeIndex)
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(largeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called on an empty list.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void RemoveAt_EmptyList_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var list = new SynchronizedList<string>();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt properly handles sequential removal operations and maintains correct item positioning.
        /// </summary>
        [Fact]
        public void RemoveAt_SequentialRemovals_MaintainsCorrectOrder()
        {
            // Arrange
            var list = new SynchronizedList<int>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(i);
            }

            // Act
            list.RemoveAt(2); // Remove item 2, list becomes [0, 1, 3, 4]
            list.RemoveAt(1); // Remove item 1, list becomes [0, 3, 4]

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(0, list[0]);
            Assert.Equal(3, list[1]);
            Assert.Equal(4, list[2]);
        }

        /// <summary>
        /// Tests that RemoveAt invalidates any cached snapshots by verifying subsequent read operations return correct values.
        /// </summary>
        [Fact]
        public void RemoveAt_InvalidatesSnapshot_SubsequentReadsReturnCorrectValues()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");

            // Force snapshot creation by accessing via indexer
            var beforeRemoval = list[1];

            // Act
            list.RemoveAt(1);

            // Assert
            Assert.Equal("second", beforeRemoval);
            Assert.Equal("third", list[1]); // Should read the new value after removal
            Assert.Equal(2, list.Count);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the beginning of an empty list.
        /// Input: empty list, index 0, valid item.
        /// Expected: item is inserted, Count increases to 1.
        /// </summary>
        [Fact]
        public void Insert_EmptyListAtIndexZero_InsertsItemSuccessfully()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var item = "test";

            // Act
            synchronizedList.Insert(0, item);

            // Assert
            Assert.Equal(1, synchronizedList.Count);
            Assert.Equal(item, synchronizedList[0]);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the beginning of a non-empty list.
        /// Input: list with existing items, index 0, valid item.
        /// Expected: item is inserted at beginning, existing items shift right, Count increases.
        /// </summary>
        [Fact]
        public void Insert_AtBeginningOfNonEmptyList_InsertsAndShiftsItems()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("existing1");
            synchronizedList.Add("existing2");
            var newItem = "new";

            // Act
            synchronizedList.Insert(0, newItem);

            // Assert
            Assert.Equal(3, synchronizedList.Count);
            Assert.Equal(newItem, synchronizedList[0]);
            Assert.Equal("existing1", synchronizedList[1]);
            Assert.Equal("existing2", synchronizedList[2]);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the middle of a list.
        /// Input: list with multiple items, middle index, valid item.
        /// Expected: item is inserted at correct position, other items shift appropriately.
        /// </summary>
        [Fact]
        public void Insert_AtMiddleIndex_InsertsAtCorrectPosition()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");
            synchronizedList.Add("item2");
            synchronizedList.Add("item3");
            var newItem = "middle";

            // Act
            synchronizedList.Insert(1, newItem);

            // Assert
            Assert.Equal(4, synchronizedList.Count);
            Assert.Equal("item1", synchronizedList[0]);
            Assert.Equal(newItem, synchronizedList[1]);
            Assert.Equal("item2", synchronizedList[2]);
            Assert.Equal("item3", synchronizedList[3]);
        }

        /// <summary>
        /// Tests that Insert method successfully inserts an item at the end of a list.
        /// Input: list with items, index equal to Count, valid item.
        /// Expected: item is appended to end, Count increases.
        /// </summary>
        [Fact]
        public void Insert_AtEndIndex_AppendsItem()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");
            synchronizedList.Add("item2");
            var newItem = "last";

            // Act
            synchronizedList.Insert(2, newItem);

            // Assert
            Assert.Equal(3, synchronizedList.Count);
            Assert.Equal("item1", synchronizedList[0]);
            Assert.Equal("item2", synchronizedList[1]);
            Assert.Equal(newItem, synchronizedList[2]);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException for negative index.
        /// Input: valid list, negative index, valid item.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => synchronizedList.Insert(-1, "test"));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException for index greater than Count.
        /// Input: list with items, index beyond bounds, valid item.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Insert_IndexBeyondCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");
            synchronizedList.Add("item2");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => synchronizedList.Insert(3, "test"));
        }

        /// <summary>
        /// Tests that Insert method accepts null values for reference types.
        /// Input: list, valid index, null item.
        /// Expected: null is inserted successfully.
        /// </summary>
        [Fact]
        public void Insert_NullItem_InsertsNull()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");

            // Act
            synchronizedList.Insert(0, null);

            // Assert
            Assert.Equal(2, synchronizedList.Count);
            Assert.Null(synchronizedList[0]);
            Assert.Equal("item1", synchronizedList[1]);
        }

        /// <summary>
        /// Tests that Insert method works correctly with value types.
        /// Input: list of integers, valid index, integer item.
        /// Expected: integer is inserted successfully at correct position.
        /// </summary>
        [Fact]
        public void Insert_ValueType_InsertsSuccessfully()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(1);
            synchronizedList.Add(3);

            // Act
            synchronizedList.Insert(1, 2);

            // Assert
            Assert.Equal(3, synchronizedList.Count);
            Assert.Equal(1, synchronizedList[0]);
            Assert.Equal(2, synchronizedList[1]);
            Assert.Equal(3, synchronizedList[2]);
        }

        /// <summary>
        /// Tests that Insert method invalidates cached snapshot by verifying enumerator behavior.
        /// Input: list with cached snapshot, insert operation, enumerate.
        /// Expected: enumeration reflects the inserted item.
        /// </summary>
        [Fact]
        public void Insert_WithCachedSnapshot_InvalidatesSnapshot()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");
            synchronizedList.Add("item2");

            // Force snapshot creation by enumerating
            var initialCount = synchronizedList.ToList().Count;

            // Act
            synchronizedList.Insert(1, "inserted");

            // Assert - enumeration should reflect the new item
            var finalList = synchronizedList.ToList();
            Assert.Equal(3, finalList.Count);
            Assert.Equal("item1", finalList[0]);
            Assert.Equal("inserted", finalList[1]);
            Assert.Equal("item2", finalList[2]);
        }

        /// <summary>
        /// Tests Insert method with extreme index values at boundaries.
        /// Input: various boundary index values.
        /// Expected: valid boundaries work, invalid boundaries throw exceptions.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void Insert_ExtremeIndices_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item");

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => synchronizedList.Insert(index, "test"));
        }

        /// <summary>
        /// Tests IndexOf method when the item exists in the list.
        /// Should return the zero-based index of the first occurrence of the item.
        /// </summary>
        [Theory]
        [InlineData("test", 0)]
        [InlineData("first", 0)]
        [InlineData("second", 1)]
        [InlineData("third", 2)]
        public void IndexOf_ItemExists_ReturnsCorrectIndex(string item, int expectedIndex)
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");

            // Act
            int actualIndex = list.IndexOf(item);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method when the item does not exist in the list.
        /// Should return -1 when the item is not found.
        /// </summary>
        [Theory]
        [InlineData("nonexistent")]
        [InlineData("missing")]
        [InlineData("")]
        public void IndexOf_ItemDoesNotExist_ReturnsMinusOne(string item)
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");

            // Act
            int actualIndex = list.IndexOf(item);

            // Assert
            Assert.Equal(-1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method on an empty list.
        /// Should return -1 when searching in an empty list.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyList_ReturnsMinusOne()
        {
            // Arrange
            var list = new SynchronizedList<string>();

            // Act
            int actualIndex = list.IndexOf("any item");

            // Assert
            Assert.Equal(-1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with null item when T allows null values.
        /// Should return correct index if null exists, or -1 if it doesn't.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsCorrectResult()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add(null);
            list.Add("third");

            // Act
            int actualIndex = list.IndexOf(null);

            // Assert
            Assert.Equal(1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with null item when null doesn't exist in the list.
        /// Should return -1 when null is not found in the list.
        /// </summary>
        [Fact]
        public void IndexOf_NullItemNotInList_ReturnsMinusOne()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");

            // Act
            int actualIndex = list.IndexOf(null);

            // Assert
            Assert.Equal(-1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with duplicate items in the list.
        /// Should return the index of the first occurrence when duplicates exist.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("duplicate");
            list.Add("third");
            list.Add("duplicate");
            list.Add("fifth");

            // Act
            int actualIndex = list.IndexOf("duplicate");

            // Assert
            Assert.Equal(1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with integer types to verify generic type behavior.
        /// Should work correctly with non-string types.
        /// </summary>
        [Theory]
        [InlineData(10, 0)]
        [InlineData(20, 1)]
        [InlineData(30, 2)]
        [InlineData(99, -1)]
        public void IndexOf_IntegerType_ReturnsCorrectIndex(int item, int expectedIndex)
        {
            // Arrange
            var list = new SynchronizedList<int>();
            list.Add(10);
            list.Add(20);
            list.Add(30);

            // Act
            int actualIndex = list.IndexOf(item);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with edge case values for integer types.
        /// Should handle boundary values correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        public void IndexOf_IntegerEdgeCases_HandlesCorrectly(int item)
        {
            // Arrange
            var list = new SynchronizedList<int>();
            list.Add(int.MinValue);
            list.Add(int.MaxValue);
            list.Add(0);
            list.Add(-1);

            // Act
            int actualIndex = list.IndexOf(item);

            // Assert
            Assert.True(actualIndex >= 0 && actualIndex < 4);
        }

        /// <summary>
        /// Tests that GetEnumerator creates a new snapshot when _snapshot is null and the list is empty.
        /// Should return an enumerator that iterates over no items.
        /// </summary>
        [Fact]
        public void GetEnumerator_WhenSnapshotIsNullAndListIsEmpty_CreatesEmptySnapshot()
        {
            // Arrange
            var list = new SynchronizedList<string>();

            // Act
            var enumerator = list.GetEnumerator();

            // Assert
            Assert.NotNull(enumerator);
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that GetEnumerator creates a new snapshot when _snapshot is null and the list contains items.
        /// Should return an enumerator that iterates over all items in the correct order.
        /// </summary>
        [Fact]
        public void GetEnumerator_WhenSnapshotIsNullAndListHasItems_CreatesSnapshotWithItems()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("first");
            list.Add("second");
            list.Add("third");

            // Act
            var enumerator = list.GetEnumerator();

            // Assert
            var items = new List<string>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            Assert.Equal(3, items.Count);
            Assert.Equal("first", items[0]);
            Assert.Equal("second", items[1]);
            Assert.Equal("third", items[2]);
        }

        /// <summary>
        /// Tests that GetEnumerator reuses existing snapshot when _snapshot is not null.
        /// Multiple calls should return enumerators from the same snapshot.
        /// </summary>
        [Fact]
        public void GetEnumerator_WhenSnapshotExists_ReusesExistingSnapshot()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item");

            // Act - First call creates snapshot
            var enumerator1 = list.GetEnumerator();

            // Act - Second call should reuse snapshot
            var enumerator2 = list.GetEnumerator();

            // Assert - Both enumerators should work and have same content
            Assert.True(enumerator1.MoveNext());
            Assert.Equal("item", enumerator1.Current);
            Assert.False(enumerator1.MoveNext());

            Assert.True(enumerator2.MoveNext());
            Assert.Equal("item", enumerator2.Current);
            Assert.False(enumerator2.MoveNext());
        }

        /// <summary>
        /// Tests that GetEnumerator creates a new snapshot after the list has been modified.
        /// Modifying the list should invalidate the snapshot, requiring a new one on next enumeration.
        /// </summary>
        [Fact]
        public void GetEnumerator_AfterListModification_CreatesNewSnapshot()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("original");

            // Act - First enumeration creates initial snapshot
            var enumerator1 = list.GetEnumerator();
            var items1 = new List<string>();
            while (enumerator1.MoveNext())
            {
                items1.Add(enumerator1.Current);
            }

            // Modify list (this should invalidate snapshot)
            list.Add("added");

            // Act - Second enumeration should create new snapshot
            var enumerator2 = list.GetEnumerator();
            var items2 = new List<string>();
            while (enumerator2.MoveNext())
            {
                items2.Add(enumerator2.Current);
            }

            // Assert
            Assert.Single(items1);
            Assert.Equal("original", items1[0]);

            Assert.Equal(2, items2.Count);
            Assert.Equal("original", items2[0]);
            Assert.Equal("added", items2[1]);
        }

        /// <summary>
        /// Tests that GetEnumerator with null items works correctly.
        /// Should handle null items in the collection properly.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithNullItems_HandlesNullCorrectly()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add(null);
            list.Add("not null");
            list.Add(null);

            // Act
            var enumerator = list.GetEnumerator();

            // Assert
            var items = new List<string>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            Assert.Equal(3, items.Count);
            Assert.Null(items[0]);
            Assert.Equal("not null", items[1]);
            Assert.Null(items[2]);
        }

        /// <summary>
        /// Tests that enumerator reflects snapshot state at time of GetEnumerator call, not current list state.
        /// Modifications made after getting enumerator should not affect the enumeration.
        /// </summary>
        [Fact]
        public void GetEnumerator_EnumeratorReflectsSnapshotNotCurrentState_IsolatesFromSubsequentModifications()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item1");
            list.Add("item2");

            // Act - Get enumerator (creates snapshot)
            var enumerator = list.GetEnumerator();

            // Modify list after getting enumerator
            list.Add("item3");
            list.Remove("item1");

            // Assert - Enumerator should still see original snapshot
            var items = new List<string>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            Assert.Equal(2, items.Count);
            Assert.Equal("item1", items[0]);
            Assert.Equal("item2", items[1]);
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with value types.
        /// Should properly handle value type collections and enumeration.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithValueTypes_WorksCorrectly()
        {
            // Arrange
            var list = new SynchronizedList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            // Act
            var enumerator = list.GetEnumerator();

            // Assert
            var items = new List<int>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            Assert.Equal(3, items.Count);
            Assert.Equal(1, items[0]);
            Assert.Equal(2, items[1]);
            Assert.Equal(3, items[2]);
        }

        /// <summary>
        /// Tests that GetEnumerator handles single item collections correctly.
        /// Should create snapshot and enumerate single item properly.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithSingleItem_WorksCorrectly()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("single");

            // Act
            var enumerator = list.GetEnumerator();

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal("single", enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that removing an existing item returns true and actually removes the item from the list.
        /// Input: A list with multiple items, removing an existing item.
        /// Expected result: Method returns true and the item is no longer in the list.
        /// </summary>
        [Fact]
        public void Remove_ExistingItem_ReturnsTrueAndRemovesItem()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item1");
            list.Add("item2");
            list.Add("item3");
            var initialCount = list.Count;

            // Act
            var result = list.Remove("item2");

            // Assert
            Assert.True(result);
            Assert.Equal(initialCount - 1, list.Count);
            Assert.False(list.Contains("item2"));
            Assert.True(list.Contains("item1"));
            Assert.True(list.Contains("item3"));
        }

        /// <summary>
        /// Tests that removing a non-existing item returns false and leaves the list unchanged.
        /// Input: A list with items, attempting to remove an item that doesn't exist.
        /// Expected result: Method returns false and the list remains unchanged.
        /// </summary>
        [Fact]
        public void Remove_NonExistingItem_ReturnsFalseAndListUnchanged()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item1");
            list.Add("item2");
            var initialCount = list.Count;

            // Act
            var result = list.Remove("nonexistent");

            // Assert
            Assert.False(result);
            Assert.Equal(initialCount, list.Count);
            Assert.True(list.Contains("item1"));
            Assert.True(list.Contains("item2"));
        }

        /// <summary>
        /// Tests that attempting to remove an item from an empty list returns false.
        /// Input: An empty list and any item to remove.
        /// Expected result: Method returns false.
        /// </summary>
        [Fact]
        public void Remove_FromEmptyList_ReturnsFalse()
        {
            // Arrange
            var list = new SynchronizedList<string>();

            // Act
            var result = list.Remove("anyitem");

            // Assert
            Assert.False(result);
            Assert.Equal(0, list.Count);
        }

        /// <summary>
        /// Tests that removing null from a list works correctly when null is present.
        /// Input: A list containing null and other items, removing null.
        /// Expected result: Method returns true and null is removed from the list.
        /// </summary>
        [Fact]
        public void Remove_NullItemWhenPresent_ReturnsTrueAndRemovesNull()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item1");
            list.Add(null);
            list.Add("item2");
            var initialCount = list.Count;

            // Act
            var result = list.Remove(null);

            // Assert
            Assert.True(result);
            Assert.Equal(initialCount - 1, list.Count);
            Assert.False(list.Contains(null));
            Assert.True(list.Contains("item1"));
            Assert.True(list.Contains("item2"));
        }

        /// <summary>
        /// Tests that removing null from a list returns false when null is not present.
        /// Input: A list without null, attempting to remove null.
        /// Expected result: Method returns false and the list remains unchanged.
        /// </summary>
        [Fact]
        public void Remove_NullItemWhenNotPresent_ReturnsFalse()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item1");
            list.Add("item2");
            var initialCount = list.Count;

            // Act
            var result = list.Remove(null);

            // Assert
            Assert.False(result);
            Assert.Equal(initialCount, list.Count);
            Assert.True(list.Contains("item1"));
            Assert.True(list.Contains("item2"));
        }

        /// <summary>
        /// Tests that removing an item when duplicates exist only removes the first occurrence.
        /// Input: A list with duplicate items, removing one of the duplicates.
        /// Expected result: Method returns true and only one occurrence is removed.
        /// </summary>
        [Fact]
        public void Remove_DuplicateItems_RemovesFirstOccurrence()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("item1");
            list.Add("duplicate");
            list.Add("item2");
            list.Add("duplicate");
            list.Add("item3");
            var initialCount = list.Count;

            // Act
            var result = list.Remove("duplicate");

            // Assert
            Assert.True(result);
            Assert.Equal(initialCount - 1, list.Count);
            Assert.True(list.Contains("duplicate")); // One occurrence should still remain
            Assert.True(list.Contains("item1"));
            Assert.True(list.Contains("item2"));
            Assert.True(list.Contains("item3"));
        }

        /// <summary>
        /// Tests that removing the last remaining item from a list returns true and empties the list.
        /// Input: A list with a single item, removing that item.
        /// Expected result: Method returns true and the list becomes empty.
        /// </summary>
        [Fact]
        public void Remove_LastRemainingItem_ReturnsTrueAndEmptiesList()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("onlyitem");

            // Act
            var result = list.Remove("onlyitem");

            // Assert
            Assert.True(result);
            Assert.Equal(0, list.Count);
            Assert.False(list.Contains("onlyitem"));
        }

        /// <summary>
        /// Tests removing items with different data types to ensure generic behavior works correctly.
        /// Input: A list of integers with various values including boundary values.
        /// Expected result: Method correctly removes existing integers and returns appropriate boolean values.
        /// </summary>
        [Theory]
        [InlineData(0, true)]
        [InlineData(42, true)]
        [InlineData(-1, true)]
        [InlineData(int.MaxValue, true)]
        [InlineData(int.MinValue, true)]
        [InlineData(999, false)] // Non-existing item
        public void Remove_IntegerItems_HandlesVariousValues(int itemToRemove, bool expectedResult)
        {
            // Arrange
            var list = new SynchronizedList<int>();
            list.Add(0);
            list.Add(42);
            list.Add(-1);
            list.Add(int.MaxValue);
            list.Add(int.MinValue);
            var initialCount = list.Count;

            // Act
            var result = list.Remove(itemToRemove);

            // Assert
            Assert.Equal(expectedResult, result);
            if (expectedResult)
            {
                Assert.Equal(initialCount - 1, list.Count);
                Assert.False(list.Contains(itemToRemove));
            }
            else
            {
                Assert.Equal(initialCount, list.Count);
            }
        }

        /// <summary>
        /// Tests that removing items works correctly when the list contains boundary string values.
        /// Input: A list with empty string, whitespace, and normal strings.
        /// Expected result: Method correctly handles removal of various string edge cases.
        /// </summary>
        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("normal", true)]
        [InlineData("nonexistent", false)]
        public void Remove_StringEdgeCases_HandlesVariousStringValues(string itemToRemove, bool expectedResult)
        {
            // Arrange
            var list = new SynchronizedList<string>();
            list.Add("");
            list.Add(" ");
            list.Add("normal");
            var initialCount = list.Count;

            // Act
            var result = list.Remove(itemToRemove);

            // Assert
            Assert.Equal(expectedResult, result);
            if (expectedResult)
            {
                Assert.Equal(initialCount - 1, list.Count);
                Assert.False(list.Contains(itemToRemove));
            }
            else
            {
                Assert.Equal(initialCount, list.Count);
            }
        }

        /// <summary>
        /// Tests that Count returns 0 when the SynchronizedList is empty (initial state).
        /// Input: Empty SynchronizedList of strings.
        /// Expected: Count property returns 0.
        /// </summary>
        [Fact]
        public void Count_EmptyList_ReturnsZero()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();

            // Act
            int count = synchronizedList.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns the correct value after adding a single item.
        /// Input: SynchronizedList with one item added.
        /// Expected: Count property returns 1.
        /// </summary>
        [Fact]
        public void Count_SingleItem_ReturnsOne()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("test");

            // Act
            int count = synchronizedList.Count;

            // Assert
            Assert.Equal(1, count);
        }

        /// <summary>
        /// Tests that Count returns the correct value after adding multiple items.
        /// Input: Various numbers of items added to SynchronizedList.
        /// Expected: Count property returns the exact number of items added.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public void Count_MultipleItems_ReturnsCorrectCount(int numberOfItems)
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            for (int i = 0; i < numberOfItems; i++)
            {
                synchronizedList.Add(i);
            }

            // Act
            int count = synchronizedList.Count;

            // Assert
            Assert.Equal(numberOfItems, count);
        }

        /// <summary>
        /// Tests that Count returns 0 after clearing a list that previously contained items.
        /// Input: SynchronizedList with items that is then cleared.
        /// Expected: Count property returns 0 after clearing.
        /// </summary>
        [Fact]
        public void Count_AfterClear_ReturnsZero()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");
            synchronizedList.Add("item2");
            synchronizedList.Add("item3");

            // Act
            synchronizedList.Clear();
            int count = synchronizedList.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count works correctly with different generic types.
        /// Input: SynchronizedList of various types (int, string, object).
        /// Expected: Count property returns correct values regardless of generic type.
        /// </summary>
        [Fact]
        public void Count_DifferentGenericTypes_ReturnsCorrectCount()
        {
            // Arrange
            var intList = new SynchronizedList<int>();
            var stringList = new SynchronizedList<string>();
            var objectList = new SynchronizedList<object>();

            // Act & Assert - Empty lists
            Assert.Equal(0, intList.Count);
            Assert.Equal(0, stringList.Count);
            Assert.Equal(0, objectList.Count);

            // Act & Assert - After adding items
            intList.Add(42);
            stringList.Add("test");
            objectList.Add(new object());

            Assert.Equal(1, intList.Count);
            Assert.Equal(1, stringList.Count);
            Assert.Equal(1, objectList.Count);
        }

        /// <summary>
        /// Tests that Count handles null items correctly when the generic type allows nulls.
        /// Input: SynchronizedList with null items added.
        /// Expected: Count property includes null items in the count.
        /// </summary>
        [Fact]
        public void Count_WithNullItems_IncludesNullsInCount()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add(null);
            synchronizedList.Add("valid");
            synchronizedList.Add(null);

            // Act
            int count = synchronizedList.Count;

            // Assert
            Assert.Equal(3, count);
        }

        /// <summary>
        /// Tests that Count reflects changes accurately across multiple operations.
        /// Input: SynchronizedList with sequence of add and clear operations.
        /// Expected: Count property accurately reflects the current state after each operation.
        /// </summary>
        [Fact]
        public void Count_MultipleOperations_ReflectsCurrentState()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();

            // Act & Assert - Initial state
            Assert.Equal(0, synchronizedList.Count);

            // Act & Assert - After first add
            synchronizedList.Add(1);
            Assert.Equal(1, synchronizedList.Count);

            // Act & Assert - After more adds
            synchronizedList.Add(2);
            synchronizedList.Add(3);
            Assert.Equal(3, synchronizedList.Count);

            // Act & Assert - After clear
            synchronizedList.Clear();
            Assert.Equal(0, synchronizedList.Count);

            // Act & Assert - After adding again
            synchronizedList.Add(10);
            synchronizedList.Add(20);
            Assert.Equal(2, synchronizedList.Count);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies all elements to a valid destination array with sufficient space.
        /// Input conditions: Non-empty list, valid array with sufficient space, valid arrayIndex.
        /// Expected result: All elements are copied to the correct positions in the destination array.
        /// </summary>
        [Fact]
        public void CopyTo_ValidArrayAndIndex_CopiesElementsSuccessfully()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("first");
            synchronizedList.Add("second");
            synchronizedList.Add("third");

            var destinationArray = new string[5];
            int arrayIndex = 1;

            // Act
            synchronizedList.CopyTo(destinationArray, arrayIndex);

            // Assert
            Assert.Null(destinationArray[0]);
            Assert.Equal("first", destinationArray[1]);
            Assert.Equal("second", destinationArray[2]);
            Assert.Equal("third", destinationArray[3]);
            Assert.Null(destinationArray[4]);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when the destination array is null.
        /// Input conditions: Non-empty list, null array parameter.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(1);
            synchronizedList.Add(2);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => synchronizedList.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input conditions: Non-empty list, valid array, negative arrayIndex.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(1);
            synchronizedList.Add(2);
            var destinationArray = new int[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => synchronizedList.CopyTo(destinationArray, -1));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is insufficient space in the destination array.
        /// Input conditions: Non-empty list, array too small to accommodate all elements from the specified index.
        /// Expected result: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_InsufficientSpaceInArray_ThrowsArgumentException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(1);
            synchronizedList.Add(2);
            synchronizedList.Add(3);
            var destinationArray = new int[4]; // Not enough space for 3 elements starting at index 2

            // Act & Assert
            Assert.Throws<ArgumentException>(() => synchronizedList.CopyTo(destinationArray, 2));
        }

        /// <summary>
        /// Tests that CopyTo works correctly when arrayIndex equals the array length and the source list is empty.
        /// Input conditions: Empty list, valid array, arrayIndex equal to array length.
        /// Expected result: No exception is thrown and array remains unchanged.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyListWithArrayIndexEqualToArrayLength_DoesNotThrow()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var destinationArray = new string[3];

            // Act
            synchronizedList.CopyTo(destinationArray, 3);

            // Assert
            Assert.All(destinationArray, item => Assert.Null(item));
        }

        /// <summary>
        /// Tests that CopyTo works correctly when copying to the beginning of the array.
        /// Input conditions: Non-empty list, valid array, arrayIndex of 0.
        /// Expected result: All elements are copied starting from index 0.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexZero_CopiesFromBeginning()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(10);
            synchronizedList.Add(20);
            var destinationArray = new int[3];

            // Act
            synchronizedList.CopyTo(destinationArray, 0);

            // Assert
            Assert.Equal(10, destinationArray[0]);
            Assert.Equal(20, destinationArray[1]);
            Assert.Equal(0, destinationArray[2]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when the destination array has exactly the required space.
        /// Input conditions: Non-empty list, array with exact size needed from the specified index.
        /// Expected result: All elements are copied successfully with no remaining space.
        /// </summary>
        [Fact]
        public void CopyTo_ExactSpaceRequired_CopiesSuccessfully()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<char>();
            synchronizedList.Add('A');
            synchronizedList.Add('B');
            var destinationArray = new char[2]; // Exactly 2 elements needed

            // Act
            synchronizedList.CopyTo(destinationArray, 0);

            // Assert
            Assert.Equal('A', destinationArray[0]);
            Assert.Equal('B', destinationArray[1]);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is greater than array length.
        /// Input conditions: Non-empty list, valid array, arrayIndex greater than array length.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexGreaterThanArrayLength_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<double>();
            synchronizedList.Add(1.5);
            var destinationArray = new double[3];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => synchronizedList.CopyTo(destinationArray, 4));
        }

        /// <summary>
        /// Tests that CopyTo works correctly with an empty list.
        /// Input conditions: Empty list, valid array, valid arrayIndex.
        /// Expected result: No elements are copied and array remains unchanged.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyList_DoesNotModifyArray()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var destinationArray = new string[] { "existing1", "existing2", "existing3" };
            var originalArray = new string[] { "existing1", "existing2", "existing3" };

            // Act
            synchronizedList.CopyTo(destinationArray, 1);

            // Assert
            Assert.Equal(originalArray, destinationArray);
        }

        /// <summary>
        /// Tests that CopyTo handles maximum integer values for arrayIndex appropriately.
        /// Input conditions: Empty list, valid array, int.MaxValue as arrayIndex.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_MaxValueArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            var destinationArray = new int[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => synchronizedList.CopyTo(destinationArray, int.MaxValue));
        }

        /// <summary>
        /// Tests that Contains returns false when the list is empty.
        /// Input conditions: Empty SynchronizedList and any item to search for.
        /// Expected result: Contains returns false.
        /// </summary>
        [Fact]
        public void Contains_EmptyList_ReturnsFalse()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var itemToFind = "test";

            // Act
            var result = synchronizedList.Contains(itemToFind);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the list.
        /// Input conditions: SynchronizedList with added items and searching for an existing item.
        /// Expected result: Contains returns true.
        /// </summary>
        [Fact]
        public void Contains_ItemExists_ReturnsTrue()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var existingItem = "existing";
            synchronizedList.Add(existingItem);

            // Act
            var result = synchronizedList.Contains(existingItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the list.
        /// Input conditions: SynchronizedList with some items and searching for a non-existing item.
        /// Expected result: Contains returns false.
        /// </summary>
        [Fact]
        public void Contains_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("item1");
            synchronizedList.Add("item2");
            var nonExistingItem = "nonexisting";

            // Act
            var result = synchronizedList.Contains(nonExistingItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains handles null values correctly.
        /// Input conditions: SynchronizedList with null item added and searching for null.
        /// Expected result: Contains returns true when null exists, false when null doesn't exist.
        /// </summary>
        [Theory]
        [InlineData(true)]  // null item added to list
        [InlineData(false)] // null item not added to list
        public void Contains_NullValue_ReturnsExpectedResult(bool addNullToList)
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            if (addNullToList)
            {
                synchronizedList.Add(null);
            }
            synchronizedList.Add("other item");

            // Act
            var result = synchronizedList.Contains(null);

            // Assert
            Assert.Equal(addNullToList, result);
        }

        /// <summary>
        /// Tests that Contains works correctly with value types including default values.
        /// Input conditions: SynchronizedList of integers with default and non-default values.
        /// Expected result: Contains returns correct boolean based on item presence.
        /// </summary>
        [Theory]
        [InlineData(0, true)]    // default value exists
        [InlineData(5, true)]    // added value exists  
        [InlineData(10, false)]  // value doesn't exist
        [InlineData(-1, false)]  // negative value doesn't exist
        public void Contains_ValueTypes_ReturnsExpectedResult(int searchValue, bool expectedResult)
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(0);  // default value
            synchronizedList.Add(5);

            // Act
            var result = synchronizedList.Contains(searchValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Contains returns true when duplicate items exist in the list.
        /// Input conditions: SynchronizedList with duplicate items.
        /// Expected result: Contains returns true for the duplicated item.
        /// </summary>
        [Fact]
        public void Contains_DuplicateItems_ReturnsTrue()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var duplicateItem = "duplicate";
            synchronizedList.Add(duplicateItem);
            synchronizedList.Add("other");
            synchronizedList.Add(duplicateItem); // add duplicate

            // Act
            var result = synchronizedList.Contains(duplicateItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains works correctly after clearing the list.
        /// Input conditions: SynchronizedList with items that gets cleared, then searching for previously existing item.
        /// Expected result: Contains returns false after clearing.
        /// </summary>
        [Fact]
        public void Contains_AfterClear_ReturnsFalse()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var item = "test item";
            synchronizedList.Add(item);
            synchronizedList.Clear();

            // Act
            var result = synchronizedList.Contains(item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains handles edge case string values correctly.
        /// Input conditions: SynchronizedList with various edge case string values.
        /// Expected result: Contains returns correct boolean based on exact string matching.
        /// </summary>
        [Theory]
        [InlineData("", true)]           // empty string exists
        [InlineData("   ", true)]        // whitespace string exists
        [InlineData("normal", true)]     // normal string exists
        [InlineData("missing", false)]   // string doesn't exist
        [InlineData("NORMAL", false)]    // case-sensitive, doesn't exist
        public void Contains_EdgeCaseStrings_ReturnsExpectedResult(string searchValue, bool expectedResult)
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add("");           // empty string
            synchronizedList.Add("   ");        // whitespace string  
            synchronizedList.Add("normal");     // normal string

            // Act
            var result = synchronizedList.Contains(searchValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Clear method removes all items from an empty list without throwing exceptions.
        /// Input: Empty SynchronizedList
        /// Expected result: Count remains 0, no exceptions thrown
        /// </summary>
        [Fact]
        public void Clear_EmptyList_CountRemainsZero()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();

            // Act
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
        }

        /// <summary>
        /// Tests that Clear method removes all items from a list with a single item.
        /// Input: SynchronizedList with one string item
        /// Expected result: Count becomes 0, item no longer contained in list
        /// </summary>
        [Fact]
        public void Clear_SingleItem_CountBecomesZeroAndItemRemoved()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var testItem = "test";
            synchronizedList.Add(testItem);

            // Act
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
            Assert.False(synchronizedList.Contains(testItem));
        }

        /// <summary>
        /// Tests that Clear method removes all items from a list with multiple items.
        /// Input: SynchronizedList with multiple string items
        /// Expected result: Count becomes 0, all items no longer contained in list
        /// </summary>
        [Fact]
        public void Clear_MultipleItems_CountBecomesZeroAndAllItemsRemoved()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var items = new[] { "item1", "item2", "item3", "item4", "item5" };
            foreach (var item in items)
            {
                synchronizedList.Add(item);
            }

            // Act
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
            foreach (var item in items)
            {
                Assert.False(synchronizedList.Contains(item));
            }
        }

        /// <summary>
        /// Tests that Clear method can be called multiple times consecutively without issues.
        /// Input: SynchronizedList with items, cleared multiple times
        /// Expected result: Count remains 0 after each clear operation
        /// </summary>
        [Fact]
        public void Clear_CalledMultipleTimes_CountRemainsZero()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            synchronizedList.Add(1);
            synchronizedList.Add(2);

            // Act
            synchronizedList.Clear();
            synchronizedList.Clear();
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
        }

        /// <summary>
        /// Tests that Clear method works correctly with null items in the list.
        /// Input: SynchronizedList containing null items
        /// Expected result: Count becomes 0, null items are removed
        /// </summary>
        [Fact]
        public void Clear_WithNullItems_CountBecomesZeroAndNullItemsRemoved()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            synchronizedList.Add(null);
            synchronizedList.Add("test");
            synchronizedList.Add(null);

            // Act
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
            Assert.False(synchronizedList.Contains(null));
            Assert.False(synchronizedList.Contains("test"));
        }

        /// <summary>
        /// Tests that Clear method is thread-safe when called concurrently from multiple threads.
        /// Input: SynchronizedList with items, Clear called from multiple threads
        /// Expected result: Count becomes 0, no exceptions thrown due to concurrent access
        /// </summary>
        [Fact]
        public void Clear_ConcurrentAccess_ThreadSafeOperation()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            for (int i = 0; i < 100; i++)
            {
                synchronizedList.Add(i);
            }

            // Act
            var tasks = new Task[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => synchronizedList.Clear());
            }
            Task.WaitAll(tasks);

            // Assert
            Assert.Equal(0, synchronizedList.Count);
        }

        /// <summary>
        /// Tests that Clear method works with different generic types.
        /// Input: SynchronizedList with integer items
        /// Expected result: Count becomes 0, all integer items removed
        /// </summary>
        [Fact]
        public void Clear_IntegerGenericType_CountBecomesZeroAndItemsRemoved()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<int>();
            var items = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            foreach (var item in items)
            {
                synchronizedList.Add(item);
            }

            // Act
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
            foreach (var item in items)
            {
                Assert.False(synchronizedList.Contains(item));
            }
        }

        /// <summary>
        /// Tests that Clear method works when list has duplicate items.
        /// Input: SynchronizedList with duplicate string items
        /// Expected result: Count becomes 0, all duplicate items removed
        /// </summary>
        [Fact]
        public void Clear_DuplicateItems_CountBecomesZeroAndAllDuplicatesRemoved()
        {
            // Arrange
            var synchronizedList = new SynchronizedList<string>();
            var duplicateItem = "duplicate";
            synchronizedList.Add(duplicateItem);
            synchronizedList.Add("other");
            synchronizedList.Add(duplicateItem);
            synchronizedList.Add(duplicateItem);

            // Act
            synchronizedList.Clear();

            // Assert
            Assert.Equal(0, synchronizedList.Count);
            Assert.False(synchronizedList.Contains(duplicateItem));
            Assert.False(synchronizedList.Contains("other"));
        }

        /// <summary>
        /// Tests that adding a single item to an empty SynchronizedList increases the count and makes the item accessible.
        /// Input: Single integer item.
        /// Expected: Count increases to 1, item is contained in list, and can be retrieved by index.
        /// </summary>
        [Fact]
        public void Add_SingleItem_CountIncreasesAndItemIsAccessible()
        {
            // Arrange
            var list = new SynchronizedList<int>();
            int item = 42;

            // Act
            list.Add(item);

            // Assert
            Assert.Equal(1, list.Count);
            Assert.True(list.Contains(item));
            Assert.Equal(0, list.IndexOf(item));
            Assert.Equal(item, list[0]);
        }

        /// <summary>
        /// Tests adding multiple items to a SynchronizedList to verify cumulative behavior.
        /// Input: Multiple integer items added sequentially.
        /// Expected: Count reflects total items, all items are contained and accessible in correct order.
        /// </summary>
        [Fact]
        public void Add_MultipleItems_AllItemsAccessibleInCorrectOrder()
        {
            // Arrange
            var list = new SynchronizedList<int>();
            int[] items = { 1, 2, 3, 4, 5 };

            // Act
            foreach (int item in items)
            {
                list.Add(item);
            }

            // Assert
            Assert.Equal(5, list.Count);
            for (int i = 0; i < items.Length; i++)
            {
                Assert.True(list.Contains(items[i]));
                Assert.Equal(i, list.IndexOf(items[i]));
                Assert.Equal(items[i], list[i]);
            }
        }

        /// <summary>
        /// Tests adding null reference to a SynchronizedList with reference type.
        /// Input: Null string value.
        /// Expected: Null is added successfully and can be retrieved.
        /// </summary>
        [Fact]
        public void Add_NullReferenceType_NullAddedSuccessfully()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            string nullItem = null;

            // Act
            list.Add(nullItem);

            // Assert
            Assert.Equal(1, list.Count);
            Assert.True(list.Contains(null));
            Assert.Equal(0, list.IndexOf(null));
            Assert.Null(list[0]);
        }

        /// <summary>
        /// Tests adding duplicate items to verify SynchronizedList allows duplicates like List&lt;T&gt;.
        /// Input: Same string value added multiple times.
        /// Expected: All duplicate items are stored and accessible at different indices.
        /// </summary>
        [Fact]
        public void Add_DuplicateItems_AllDuplicatesStored()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            string duplicateItem = "duplicate";

            // Act
            list.Add(duplicateItem);
            list.Add(duplicateItem);
            list.Add(duplicateItem);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.True(list.Contains(duplicateItem));
            Assert.Equal(0, list.IndexOf(duplicateItem)); // IndexOf returns first occurrence
            Assert.Equal(duplicateItem, list[0]);
            Assert.Equal(duplicateItem, list[1]);
            Assert.Equal(duplicateItem, list[2]);
        }

        /// <summary>
        /// Tests that adding items to SynchronizedList allows proper enumeration.
        /// Input: Multiple string items.
        /// Expected: Enumeration returns all added items in correct order.
        /// </summary>
        [Fact]
        public void Add_MultipleItems_EnumerationWorksCorrectly()
        {
            // Arrange
            var list = new SynchronizedList<string>();
            string[] items = { "first", "second", "third" };

            // Act
            foreach (string item in items)
            {
                list.Add(item);
            }

            // Assert
            var enumeratedItems = list.ToList();
            Assert.Equal(items.Length, enumeratedItems.Count);
            for (int i = 0; i < items.Length; i++)
            {
                Assert.Equal(items[i], enumeratedItems[i]);
            }
        }

        /// <summary>
        /// Tests adding items with different data types to verify generic behavior.
        /// Input: Custom object instances.
        /// Expected: Objects are stored correctly and retrievable.
        /// </summary>
        [Fact]
        public void Add_CustomObjectType_ObjectsStoredCorrectly()
        {
            // Arrange
            var list = new SynchronizedList<TestObject>();
            var obj1 = new TestObject { Value = 1 };
            var obj2 = new TestObject { Value = 2 };

            // Act
            list.Add(obj1);
            list.Add(obj2);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.True(list.Contains(obj1));
            Assert.True(list.Contains(obj2));
            Assert.Equal(0, list.IndexOf(obj1));
            Assert.Equal(1, list.IndexOf(obj2));
            Assert.Same(obj1, list[0]);
            Assert.Same(obj2, list[1]);
        }

        /// <summary>
        /// Tests that enumeration creates a snapshot that remains consistent even after subsequent additions.
        /// Input: Items added before and after getting enumerator.
        /// Expected: Enumerator reflects snapshot state at time of creation.
        /// </summary>
        [Fact]
        public void Add_AfterGettingEnumerator_SnapshotBehaviorValidated()
        {
            // Arrange
            var list = new SynchronizedList<int>();
            list.Add(1);
            list.Add(2);

            // Act - Get enumerator (creates snapshot)
            var enumerator1 = list.GetEnumerator();
            var initialSnapshot = new List<int>();
            while (enumerator1.MoveNext())
            {
                initialSnapshot.Add(enumerator1.Current);
            }

            // Add more items after getting enumerator
            list.Add(3);
            list.Add(4);

            // Get new enumerator (should reflect new additions)
            var enumerator2 = list.GetEnumerator();
            var newSnapshot = new List<int>();
            while (enumerator2.MoveNext())
            {
                newSnapshot.Add(enumerator2.Current);
            }

            // Assert
            Assert.Equal(2, initialSnapshot.Count);
            Assert.Equal(4, newSnapshot.Count);
            Assert.Equal(4, list.Count);
        }

        private class TestObject
        {
            public int Value { get; set; }
        }
    }
}