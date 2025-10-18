#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ObservableListTests
    {
        /// <summary>
        /// Tests that InsertRange throws ArgumentOutOfRangeException when index is negative.
        /// Input: negative index value.
        /// Expected: ArgumentOutOfRangeException with correct parameter name.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void InsertRange_NegativeIndex_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var list = new ObservableList<string>();
            var range = new[] { "item1", "item2" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(index, range));
            Assert.Equal("index", exception.ParamName);
        }

        /// <summary>
        /// Tests that InsertRange throws ArgumentOutOfRangeException when index is greater than Count.
        /// Input: index value greater than collection count.
        /// Expected: ArgumentOutOfRangeException with correct parameter name.
        /// </summary>
        [Theory]
        [InlineData(1, 0)] // index=1, count=0
        [InlineData(3, 2)] // index=3, count=2
        [InlineData(int.MaxValue, 0)] // index=MaxValue, count=0
        public void InsertRange_IndexGreaterThanCount_ThrowsArgumentOutOfRangeException(int index, int initialCount)
        {
            // Arrange
            var list = new ObservableList<string>();
            for (int i = 0; i < initialCount; i++)
            {
                list.Add($"item{i}");
            }
            var range = new[] { "newitem1", "newitem2" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(index, range));
            Assert.Equal("index", exception.ParamName);
        }

        /// <summary>
        /// Tests that InsertRange throws ArgumentNullException when range parameter is null.
        /// Input: null range parameter.
        /// Expected: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void InsertRange_NullRange_ThrowsArgumentNullException()
        {
            // Arrange
            var list = new ObservableList<string>();
            IEnumerable<string> range = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, range));
            Assert.Equal("range", exception.ParamName);
        }

        /// <summary>
        /// Tests that InsertRange inserts items at the specified index in correct order.
        /// Input: valid index and multiple items.
        /// Expected: items inserted at correct positions maintaining order.
        /// </summary>
        [Theory]
        [InlineData(0)] // Insert at beginning
        [InlineData(1)] // Insert in middle
        [InlineData(2)] // Insert at end
        public void InsertRange_ValidIndexAndItems_InsertsItemsInCorrectOrder(int insertIndex)
        {
            // Arrange
            var list = new ObservableList<string> { "item0", "item1" };
            var itemsToInsert = new[] { "new1", "new2", "new3" };

            // Act
            list.InsertRange(insertIndex, itemsToInsert);

            // Assert
            Assert.Equal(5, list.Count);

            // Verify items are inserted in correct order at correct position
            for (int i = 0; i < itemsToInsert.Length; i++)
            {
                Assert.Equal(itemsToInsert[i], list[insertIndex + i]);
            }
        }

        /// <summary>
        /// Tests that InsertRange with empty range does not modify the collection.
        /// Input: valid index and empty enumerable.
        /// Expected: collection remains unchanged.
        /// </summary>
        [Fact]
        public void InsertRange_EmptyRange_DoesNotModifyCollection()
        {
            // Arrange
            var list = new ObservableList<string> { "item1", "item2" };
            var originalCount = list.Count;
            var originalItems = list.ToArray();
            var emptyRange = new string[0];

            // Act
            list.InsertRange(1, emptyRange);

            // Assert
            Assert.Equal(originalCount, list.Count);
            Assert.Equal(originalItems, list.ToArray());
        }

        /// <summary>
        /// Tests that InsertRange inserts single item correctly.
        /// Input: valid index and single item enumerable.
        /// Expected: single item inserted at correct position.
        /// </summary>
        [Fact]
        public void InsertRange_SingleItem_InsertsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "before", "after" };
            var singleItem = new[] { "middle" };

            // Act
            list.InsertRange(1, singleItem);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal("before", list[0]);
            Assert.Equal("middle", list[1]);
            Assert.Equal("after", list[2]);
        }

        /// <summary>
        /// Tests that InsertRange works correctly when inserting into empty collection.
        /// Input: empty collection with index 0 and multiple items.
        /// Expected: items added to empty collection in correct order.
        /// </summary>
        [Fact]
        public void InsertRange_EmptyCollection_InsertsItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string>();
            var itemsToInsert = new[] { "first", "second", "third" };

            // Act
            list.InsertRange(0, itemsToInsert);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(itemsToInsert, list.ToArray());
        }

        /// <summary>
        /// Tests that InsertRange raises CollectionChanged event with correct parameters.
        /// Input: valid index and items to insert.
        /// Expected: CollectionChanged event fired with Add action, correct items, and start index.
        /// </summary>
        [Fact]
        public void InsertRange_ValidInput_RaisesCollectionChangedEvent()
        {
            // Arrange
            var list = new ObservableList<string> { "existing" };
            var itemsToInsert = new[] { "new1", "new2" };
            NotifyCollectionChangedEventArgs eventArgs = null;

            list.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            list.InsertRange(1, itemsToInsert);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Equal(1, eventArgs.NewStartingIndex);
            Assert.Equal(2, eventArgs.NewItems.Count);
            Assert.Equal("new1", eventArgs.NewItems[0]);
            Assert.Equal("new2", eventArgs.NewItems[1]);
        }

        /// <summary>
        /// Tests that InsertRange preserves existing items when inserting at beginning.
        /// Input: items inserted at index 0 of non-empty collection.
        /// Expected: new items at beginning, existing items shifted to higher indices.
        /// </summary>
        [Fact]
        public void InsertRange_InsertAtBeginning_PreservesExistingItems()
        {
            // Arrange
            var list = new ObservableList<string> { "existing1", "existing2" };
            var itemsToInsert = new[] { "new1", "new2" };

            // Act
            list.InsertRange(0, itemsToInsert);

            // Assert
            Assert.Equal(4, list.Count);
            Assert.Equal("new1", list[0]);
            Assert.Equal("new2", list[1]);
            Assert.Equal("existing1", list[2]);
            Assert.Equal("existing2", list[3]);
        }

        /// <summary>
        /// Tests that InsertRange works with boundary index values.
        /// Input: valid boundary index values (0 and Count).
        /// Expected: items inserted correctly at boundaries.
        /// </summary>
        [Theory]
        [InlineData(0)] // Insert at start
        [InlineData(2)] // Insert at end (Count)
        public void InsertRange_BoundaryIndices_InsertsCorrectly(int index)
        {
            // Arrange
            var list = new ObservableList<string> { "item1", "item2" };
            var itemsToInsert = new[] { "boundary" };

            // Act
            list.InsertRange(index, itemsToInsert);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal("boundary", list[index]);
        }

        /// <summary>
        /// Tests that InsertRange handles duplicate items correctly.
        /// Input: enumerable containing duplicate values.
        /// Expected: all items including duplicates are inserted.
        /// </summary>
        [Fact]
        public void InsertRange_DuplicateItems_InsertsAllItems()
        {
            // Arrange
            var list = new ObservableList<string> { "original" };
            var itemsWithDuplicates = new[] { "dup", "unique", "dup" };

            // Act
            list.InsertRange(1, itemsWithDuplicates);

            // Assert
            Assert.Equal(4, list.Count);
            Assert.Equal("original", list[0]);
            Assert.Equal("dup", list[1]);
            Assert.Equal("unique", list[2]);
            Assert.Equal("dup", list[3]);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when oldIndex is negative.
        /// </summary>
        [Fact]
        public void Move_NegativeOldIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(-1, 1, 1));
            Assert.Equal("oldIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when oldIndex plus count exceeds collection count.
        /// </summary>
        [Fact]
        public void Move_OldIndexPlusCountExceedsCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(2, 0, 2));
            Assert.Equal("oldIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when oldIndex equals collection count.
        /// </summary>
        [Fact]
        public void Move_OldIndexEqualsCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(3, 0, 1));
            Assert.Equal("oldIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when newIndex is negative.
        /// </summary>
        [Fact]
        public void Move_NegativeNewIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(0, -1, 1));
            Assert.Equal("newIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when newIndex plus count exceeds collection count.
        /// </summary>
        [Fact]
        public void Move_NewIndexPlusCountExceedsCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(0, 2, 2));
            Assert.Equal("newIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when newIndex equals collection count.
        /// </summary>
        [Fact]
        public void Move_NewIndexEqualsCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(0, 3, 1));
            Assert.Equal("newIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move successfully moves a single item forward in the collection.
        /// </summary>
        [Fact]
        public void Move_SingleItemForward_MovesItemCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            list.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            list.Move(1, 3, 1);

            // Assert
            Assert.Equal(new[] { "A", "C", "D", "B" }, list.ToArray());
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Move, eventArgs.Action);
            Assert.Equal(3, eventArgs.NewStartingIndex);
            Assert.Equal(1, eventArgs.OldStartingIndex);
            Assert.Single(eventArgs.NewItems);
            Assert.Equal("B", eventArgs.NewItems[0]);
        }

        /// <summary>
        /// Tests that Move successfully moves a single item backward in the collection.
        /// </summary>
        [Fact]
        public void Move_SingleItemBackward_MovesItemCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            list.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            list.Move(3, 1, 1);

            // Assert
            Assert.Equal(new[] { "A", "D", "B", "C" }, list.ToArray());
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Move, eventArgs.Action);
            Assert.Equal(1, eventArgs.NewStartingIndex);
            Assert.Equal(3, eventArgs.OldStartingIndex);
            Assert.Single(eventArgs.NewItems);
            Assert.Equal("D", eventArgs.NewItems[0]);
        }

        /// <summary>
        /// Tests that Move successfully moves multiple items forward in the collection.
        /// </summary>
        [Fact]
        public void Move_MultipleItemsForward_MovesItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D", "E", "F" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            list.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            list.Move(1, 4, 2);

            // Assert
            Assert.Equal(new[] { "A", "D", "E", "B", "C", "F" }, list.ToArray());
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Move, eventArgs.Action);
            Assert.Equal(4, eventArgs.NewStartingIndex);
            Assert.Equal(1, eventArgs.OldStartingIndex);
            Assert.Equal(2, eventArgs.NewItems.Count);
            Assert.Equal("B", eventArgs.NewItems[0]);
            Assert.Equal("C", eventArgs.NewItems[1]);
        }

        /// <summary>
        /// Tests that Move successfully moves multiple items backward in the collection.
        /// </summary>
        [Fact]
        public void Move_MultipleItemsBackward_MovesItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D", "E", "F" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            list.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            list.Move(3, 1, 2);

            // Assert
            Assert.Equal(new[] { "A", "D", "E", "B", "C", "F" }, list.ToArray());
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Move, eventArgs.Action);
            Assert.Equal(1, eventArgs.NewStartingIndex);
            Assert.Equal(3, eventArgs.OldStartingIndex);
            Assert.Equal(2, eventArgs.NewItems.Count);
            Assert.Equal("D", eventArgs.NewItems[0]);
            Assert.Equal("E", eventArgs.NewItems[1]);
        }

        /// <summary>
        /// Tests that Move with same oldIndex and newIndex does not change the collection but still fires event.
        /// </summary>
        [Fact]
        public void Move_SameOldAndNewIndex_NoChangeButFiresEvent()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };
            var originalItems = list.ToArray();
            NotifyCollectionChangedEventArgs eventArgs = null;
            list.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            list.Move(1, 1, 2);

            // Assert
            Assert.Equal(originalItems, list.ToArray());
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Move, eventArgs.Action);
            Assert.Equal(1, eventArgs.NewStartingIndex);
            Assert.Equal(1, eventArgs.OldStartingIndex);
        }

        /// <summary>
        /// Tests that Move with zero count on empty collection throws ArgumentOutOfRangeException.
        /// </summary>
        [Fact]
        public void Move_ZeroCountOnEmptyCollection_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var list = new ObservableList<string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.Move(0, 0, 0));
            Assert.Equal("oldIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that Move moves items from beginning to end of collection.
        /// </summary>
        [Fact]
        public void Move_FromBeginningToEnd_MovesItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };

            // Act
            list.Move(0, 3, 1);

            // Assert
            Assert.Equal(new[] { "B", "C", "D", "A" }, list.ToArray());
        }

        /// <summary>
        /// Tests that Move moves items from end to beginning of collection.
        /// </summary>
        [Fact]
        public void Move_FromEndToBeginning_MovesItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };

            // Act
            list.Move(3, 0, 1);

            // Assert
            Assert.Equal(new[] { "D", "A", "B", "C" }, list.ToArray());
        }

        /// <summary>
        /// Tests boundary condition where oldIndex + count equals collection count.
        /// </summary>
        [Fact]
        public void Move_OldIndexPlusCountEqualsCount_MovesItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };

            // Act
            list.Move(2, 0, 2);

            // Assert
            Assert.Equal(new[] { "C", "D", "A", "B" }, list.ToArray());
        }

        /// <summary>
        /// Tests boundary condition where newIndex + count equals collection count.
        /// </summary>
        [Fact]
        public void Move_NewIndexPlusCountEqualsCount_MovesItemsCorrectly()
        {
            // Arrange
            var list = new ObservableList<string> { "A", "B", "C", "D" };

            // Act
            list.Move(0, 2, 2);

            // Assert
            Assert.Equal(new[] { "C", "D", "A", "B" }, list.ToArray());
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when index is negative.
        /// Input: negative index value.
        /// Expected: ArgumentOutOfRangeException with parameter name "index".
        /// </summary>
        [Theory]
        [InlineData(-1, 1)]
        [InlineData(-10, 5)]
        [InlineData(int.MinValue, 1)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int index, int count)
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => observableList.RemoveAt(index, count));
            Assert.Equal("index", exception.ParamName);
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when index plus count exceeds collection count.
        /// Input: index and count values where index + count > Count.
        /// Expected: ArgumentOutOfRangeException with parameter name "index".
        /// </summary>
        [Theory]
        [InlineData(3, 1)] // index = Count, count = 1
        [InlineData(2, 2)] // index + count = 4, Count = 3
        [InlineData(1, 5)] // index + count = 6, Count = 3
        [InlineData(0, 4)] // index + count = 4, Count = 3
        public void RemoveAt_IndexPlusCountExceedsCount_ThrowsArgumentOutOfRangeException(int index, int count)
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" }; // Count = 3

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => observableList.RemoveAt(index, count));
            Assert.Equal("index", exception.ParamName);
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when integer overflow occurs in index + count.
        /// Input: large values that cause integer overflow.
        /// Expected: ArgumentOutOfRangeException with parameter name "index".
        /// </summary>
        [Fact]
        public void RemoveAt_IntegerOverflowInIndexPlusCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => observableList.RemoveAt(int.MaxValue, int.MaxValue));
            Assert.Equal("index", exception.ParamName);
        }

        /// <summary>
        /// Tests that RemoveAt with count zero does not throw exception and does not modify collection.
        /// Input: valid index with count = 0.
        /// Expected: no exception thrown, collection unchanged.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        public void RemoveAt_CountZero_DoesNotThrowAndDoesNotModifyCollection(int index, int count)
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var originalCount = observableList.Count;
            var originalItems = observableList.ToArray();

            // Act
            observableList.RemoveAt(index, count);

            // Assert
            Assert.Equal(originalCount, observableList.Count);
            Assert.Equal(originalItems, observableList.ToArray());
        }

        /// <summary>
        /// Tests that RemoveAt with empty collection and valid parameters does not throw exception.
        /// Input: empty collection with index = 0, count = 0.
        /// Expected: no exception thrown.
        /// </summary>
        [Fact]
        public void RemoveAt_EmptyCollection_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var observableList = new ObservableList<string>();

            // Act & Assert - should not throw
            observableList.RemoveAt(0, 0);
        }

        /// <summary>
        /// Tests that RemoveAt with empty collection and invalid parameters throws exception.
        /// Input: empty collection with count > 0.
        /// Expected: ArgumentOutOfRangeException.
        /// </summary>
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void RemoveAt_EmptyCollection_InvalidParameters_ThrowsArgumentOutOfRangeException(int index, int count)
        {
            // Arrange
            var observableList = new ObservableList<string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => observableList.RemoveAt(index, count));
            Assert.Equal("index", exception.ParamName);
        }

        /// <summary>
        /// Tests that RemoveAt raises CollectionChanged event with correct parameters for valid removal.
        /// Input: valid index and count for removal.
        /// Expected: CollectionChanged event raised with Remove action, correct items, and correct index.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidParameters_RaisesCollectionChangedEvent()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3", "item4" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.RemoveAt(1, 2);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(1, eventArgs.OldStartingIndex);
            Assert.Equal(2, eventArgs.OldItems.Count);
            Assert.Contains("item2", eventArgs.OldItems.Cast<string>());
            Assert.Contains("item3", eventArgs.OldItems.Cast<string>());
        }

        /// <summary>
        /// Tests RemoveAt behavior with single item removal.
        /// Input: remove single item at various positions.
        /// Expected: correct item removed, collection count decreased by one.
        /// </summary>
        [Theory]
        [InlineData(0)] // Remove first item
        [InlineData(1)] // Remove middle item  
        [InlineData(2)] // Remove last item
        public void RemoveAt_SingleItem_RemovesCorrectItem(int index)
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var expectedRemainingItems = observableList.Where((item, i) => i != index).ToArray();

            // Act
            observableList.RemoveAt(index, 1);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Equal(expectedRemainingItems, observableList.ToArray());
        }

        /// <summary>
        /// Tests RemoveAt behavior when removing all items from collection.
        /// Input: index = 0, count = collection.Count.
        /// Expected: empty collection.
        /// </summary>
        [Fact]
        public void RemoveAt_RemoveAllItems_ResultsInEmptyCollection()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };

            // Act
            observableList.RemoveAt(0, 3);

            // Assert
            Assert.Empty(observableList);
        }

        /// <summary>
        /// Tests edge case where index equals Count and count is zero.
        /// Input: index = Count, count = 0.
        /// Expected: no exception thrown, collection unchanged.
        /// </summary>
        [Fact]
        public void RemoveAt_IndexEqualsCount_CountZero_DoesNotThrow()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var originalCount = observableList.Count;

            // Act
            observableList.RemoveAt(3, 0); // index = Count = 3, count = 0

            // Assert
            Assert.Equal(originalCount, observableList.Count);
        }

        /// <summary>
        /// Tests that RemoveRange throws ArgumentNullException when the range parameter is null.
        /// </summary>
        [Fact]
        public void RemoveRange_NullRange_ThrowsArgumentNullException()
        {
            // Arrange
            var observableList = new ObservableList<string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => observableList.RemoveRange(null));
            Assert.Equal("range", exception.ParamName);
        }

        /// <summary>
        /// Tests that RemoveRange handles an empty range without throwing exceptions.
        /// </summary>
        [Fact]
        public void RemoveRange_EmptyRange_DoesNotThrowAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2" };
            var emptyRange = new List<string>();
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(emptyRange);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Contains("item1", observableList);
            Assert.Contains("item2", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Empty(eventArgs.OldItems);
        }

        /// <summary>
        /// Tests that RemoveRange successfully removes a single item that exists in the collection.
        /// </summary>
        [Fact]
        public void RemoveRange_SingleExistingItem_RemovesItemAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var rangeToRemove = new List<string> { "item2" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Contains("item1", observableList);
            Assert.DoesNotContain("item2", observableList);
            Assert.Contains("item3", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Single(eventArgs.OldItems);
            Assert.Equal("item2", eventArgs.OldItems[0]);
        }

        /// <summary>
        /// Tests that RemoveRange successfully removes multiple items that exist in the collection.
        /// </summary>
        [Fact]
        public void RemoveRange_MultipleExistingItems_RemovesItemsAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3", "item4" };
            var rangeToRemove = new List<string> { "item2", "item4" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Contains("item1", observableList);
            Assert.DoesNotContain("item2", observableList);
            Assert.Contains("item3", observableList);
            Assert.DoesNotContain("item4", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(2, eventArgs.OldItems.Count);
            Assert.Contains("item2", eventArgs.OldItems);
            Assert.Contains("item4", eventArgs.OldItems);
        }

        /// <summary>
        /// Tests that RemoveRange handles items that don't exist in the collection gracefully.
        /// </summary>
        [Fact]
        public void RemoveRange_NonExistingItems_DoesNotAffectCollectionAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2" };
            var rangeToRemove = new List<string> { "item3", "item4" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Contains("item1", observableList);
            Assert.Contains("item2", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(2, eventArgs.OldItems.Count);
            Assert.Contains("item3", eventArgs.OldItems);
            Assert.Contains("item4", eventArgs.OldItems);
        }

        /// <summary>
        /// Tests that RemoveRange handles a mix of existing and non-existing items correctly.
        /// </summary>
        [Fact]
        public void RemoveRange_MixedExistingAndNonExistingItems_RemovesOnlyExistingItemsAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var rangeToRemove = new List<string> { "item2", "item4", "item1", "item5" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Single(observableList);
            Assert.Contains("item3", observableList);
            Assert.DoesNotContain("item1", observableList);
            Assert.DoesNotContain("item2", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(4, eventArgs.OldItems.Count);
        }

        /// <summary>
        /// Tests that RemoveRange handles duplicate items in the range correctly.
        /// </summary>
        [Fact]
        public void RemoveRange_DuplicateItemsInRange_ProcessesAllItemsAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var rangeToRemove = new List<string> { "item2", "item2", "item1" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Single(observableList);
            Assert.Contains("item3", observableList);
            Assert.DoesNotContain("item1", observableList);
            Assert.DoesNotContain("item2", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(3, eventArgs.OldItems.Count);
        }

        /// <summary>
        /// Tests that RemoveRange works with different IEnumerable implementations like arrays.
        /// </summary>
        [Fact]
        public void RemoveRange_ArrayAsRange_RemovesItemsAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3", "item4" };
            var rangeToRemove = new[] { "item1", "item3" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.DoesNotContain("item1", observableList);
            Assert.Contains("item2", observableList);
            Assert.DoesNotContain("item3", observableList);
            Assert.Contains("item4", observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(2, eventArgs.OldItems.Count);
        }

        /// <summary>
        /// Tests that RemoveRange works with LINQ query as the range parameter.
        /// </summary>
        [Fact]
        public void RemoveRange_LinqQueryAsRange_RemovesItemsAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<int> { 1, 2, 3, 4, 5, 6 };
            var rangeToRemove = new[] { 1, 2, 3, 4, 5, 6 }.Where(x => x % 2 == 0); // Even numbers
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Contains(1, observableList);
            Assert.DoesNotContain(2, observableList);
            Assert.Contains(3, observableList);
            Assert.DoesNotContain(4, observableList);
            Assert.Contains(5, observableList);
            Assert.DoesNotContain(6, observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(3, eventArgs.OldItems.Count);
        }

        /// <summary>
        /// Tests that RemoveRange removes all items when the range contains all collection items.
        /// </summary>
        [Fact]
        public void RemoveRange_AllItemsInCollection_ClearsCollectionAndFiresCollectionChanged()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var rangeToRemove = new List<string> { "item1", "item2", "item3" };
            bool collectionChangedFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                collectionChangedFired = true;
                eventArgs = e;
            };

            // Act
            observableList.RemoveRange(rangeToRemove);

            // Assert
            Assert.Empty(observableList);
            Assert.True(collectionChangedFired);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(3, eventArgs.OldItems.Count);
        }

        /// <summary>
        /// Tests that ReplaceRange throws ArgumentNullException when items parameter is null.
        /// </summary>
        [Fact]
        public void ReplaceRange_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => observableList.ReplaceRange(0, null));
            Assert.Equal("items", exception.ParamName);
        }

        /// <summary>
        /// Tests that ReplaceRange throws ArgumentOutOfRangeException when startIndex is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void ReplaceRange_NegativeStartIndex_ThrowsArgumentOutOfRangeException(int startIndex)
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var replacementItems = new[] { "new1" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => observableList.ReplaceRange(startIndex, replacementItems));
            Assert.Equal("startIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that ReplaceRange throws ArgumentOutOfRangeException when startIndex plus items length exceeds collection count.
        /// </summary>
        [Theory]
        [InlineData(3, 2)] // startIndex 3 + 2 items = 5 > count 3
        [InlineData(2, 3)] // startIndex 2 + 3 items = 5 > count 3
        [InlineData(1, 4)] // startIndex 1 + 4 items = 5 > count 3
        public void ReplaceRange_StartIndexPlusLengthExceedsCount_ThrowsArgumentOutOfRangeException(int startIndex, int itemCount)
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var replacementItems = new string[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                replacementItems[i] = $"new{i}";
            }

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => observableList.ReplaceRange(startIndex, replacementItems));
            Assert.Equal("startIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that ReplaceRange successfully replaces items and fires CollectionChanged event with empty replacement collection.
        /// </summary>
        [Fact]
        public void ReplaceRange_EmptyItems_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var replacementItems = new string[0];
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(0, replacementItems);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Equal("item1", observableList[0]);
            Assert.Equal("item2", observableList[1]);
            Assert.Equal("item3", observableList[2]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(0, eventArgs.StartingIndex);
            Assert.Empty(eventArgs.NewItems);
            Assert.Empty(eventArgs.OldItems);
        }

        /// <summary>
        /// Tests that ReplaceRange successfully replaces single item at the beginning of collection.
        /// </summary>
        [Fact]
        public void ReplaceRange_SingleItemAtBeginning_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var replacementItems = new[] { "new1" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(0, replacementItems);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Equal("new1", observableList[0]);
            Assert.Equal("item2", observableList[1]);
            Assert.Equal("item3", observableList[2]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(0, eventArgs.StartingIndex);
            Assert.Single(eventArgs.NewItems);
            Assert.Equal("new1", eventArgs.NewItems[0]);
            Assert.Single(eventArgs.OldItems);
            Assert.Equal("item1", eventArgs.OldItems[0]);
        }

        /// <summary>
        /// Tests that ReplaceRange successfully replaces single item at the end of collection.
        /// </summary>
        [Fact]
        public void ReplaceRange_SingleItemAtEnd_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var replacementItems = new[] { "new3" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(2, replacementItems);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Equal("item1", observableList[0]);
            Assert.Equal("item2", observableList[1]);
            Assert.Equal("new3", observableList[2]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(2, eventArgs.StartingIndex);
            Assert.Single(eventArgs.NewItems);
            Assert.Equal("new3", eventArgs.NewItems[0]);
            Assert.Single(eventArgs.OldItems);
            Assert.Equal("item3", eventArgs.OldItems[0]);
        }

        /// <summary>
        /// Tests that ReplaceRange successfully replaces multiple items in the middle of collection.
        /// </summary>
        [Fact]
        public void ReplaceRange_MultipleItemsInMiddle_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3", "item4", "item5" };
            var replacementItems = new[] { "new2", "new3" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(1, replacementItems);

            // Assert
            Assert.Equal(5, observableList.Count);
            Assert.Equal("item1", observableList[0]);
            Assert.Equal("new2", observableList[1]);
            Assert.Equal("new3", observableList[2]);
            Assert.Equal("item4", observableList[3]);
            Assert.Equal("item5", observableList[4]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(1, eventArgs.StartingIndex);
            Assert.Equal(2, eventArgs.NewItems.Count);
            Assert.Equal("new2", eventArgs.NewItems[0]);
            Assert.Equal("new3", eventArgs.NewItems[1]);
            Assert.Equal(2, eventArgs.OldItems.Count);
            Assert.Equal("item2", eventArgs.OldItems[0]);
            Assert.Equal("item3", eventArgs.OldItems[1]);
        }

        /// <summary>
        /// Tests that ReplaceRange successfully replaces entire collection.
        /// </summary>
        [Fact]
        public void ReplaceRange_EntireCollection_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2", "item3" };
            var replacementItems = new[] { "new1", "new2", "new3" };
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(0, replacementItems);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Equal("new1", observableList[0]);
            Assert.Equal("new2", observableList[1]);
            Assert.Equal("new3", observableList[2]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(0, eventArgs.StartingIndex);
            Assert.Equal(3, eventArgs.NewItems.Count);
            Assert.Equal("new1", eventArgs.NewItems[0]);
            Assert.Equal("new2", eventArgs.NewItems[1]);
            Assert.Equal("new3", eventArgs.NewItems[2]);
            Assert.Equal(3, eventArgs.OldItems.Count);
            Assert.Equal("item1", eventArgs.OldItems[0]);
            Assert.Equal("item2", eventArgs.OldItems[1]);
            Assert.Equal("item3", eventArgs.OldItems[2]);
        }

        /// <summary>
        /// Tests that ReplaceRange works correctly at boundary condition when startIndex equals zero and single item is replaced.
        /// </summary>
        [Fact]
        public void ReplaceRange_BoundaryAtIndexZero_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<int> { 1 };
            var replacementItems = new[] { 100 };
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(0, replacementItems);

            // Assert
            Assert.Single(observableList);
            Assert.Equal(100, observableList[0]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(0, eventArgs.StartingIndex);
        }

        /// <summary>
        /// Tests that ReplaceRange works correctly when replacing items with IEnumerable that is not an array.
        /// </summary>
        [Fact]
        public void ReplaceRange_WithIEnumerableNotArray_ReplacesSuccessfully()
        {
            // Arrange
            var observableList = new ObservableList<string> { "item1", "item2" };
            var replacementItems = new List<string> { "new1", "new2" }.Where(x => x.StartsWith("new"));
            NotifyCollectionChangedEventArgs eventArgs = null;
            observableList.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            observableList.ReplaceRange(0, replacementItems);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Equal("new1", observableList[0]);
            Assert.Equal("new2", observableList[1]);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, eventArgs.Action);
            Assert.Equal(0, eventArgs.StartingIndex);
            Assert.Equal(2, eventArgs.NewItems.Count);
            Assert.Equal(2, eventArgs.OldItems.Count);
        }

        /// <summary>
        /// Tests that AddRange throws ArgumentNullException when range parameter is null.
        /// This test covers the null validation path that is currently not covered by existing tests.
        /// </summary>
        [Fact]
        public void AddRange_NullRange_ThrowsArgumentNullException()
        {
            // Arrange
            var observableList = new ObservableList<string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => observableList.AddRange(null));
            Assert.Equal("range", exception.ParamName);
        }

        /// <summary>
        /// Tests that AddRange properly handles an empty enumerable by adding no items
        /// but still firing the CollectionChanged event with an empty items list.
        /// </summary>
        [Fact]
        public void AddRange_EmptyRange_AddsNoItemsButFiresEvent()
        {
            // Arrange
            var observableList = new ObservableList<string>();
            var emptyRange = new List<string>();
            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            observableList.AddRange(emptyRange);

            // Assert
            Assert.Equal(0, observableList.Count);
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Equal(0, eventArgs.NewItems.Count);
            Assert.Equal(0, eventArgs.NewStartingIndex);
        }

        /// <summary>
        /// Tests that AddRange correctly adds a single item and fires CollectionChanged event
        /// with proper parameters including the correct starting index.
        /// </summary>
        [Fact]
        public void AddRange_SingleItem_AddsItemAndFiresEvent()
        {
            // Arrange
            var observableList = new ObservableList<string>();
            var singleItem = new[] { "test" };
            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            observableList.AddRange(singleItem);

            // Assert
            Assert.Equal(1, observableList.Count);
            Assert.Equal("test", observableList[0]);
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Equal(1, eventArgs.NewItems.Count);
            Assert.Equal("test", eventArgs.NewItems[0]);
            Assert.Equal(0, eventArgs.NewStartingIndex);
        }

        /// <summary>
        /// Tests that AddRange correctly adds multiple items in order and fires CollectionChanged event
        /// with all items and the correct starting index.
        /// </summary>
        [Fact]
        public void AddRange_MultipleItems_AddsAllItemsInOrderAndFiresEvent()
        {
            // Arrange
            var observableList = new ObservableList<string>();
            var multipleItems = new[] { "first", "second", "third" };
            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            observableList.AddRange(multipleItems);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Equal("first", observableList[0]);
            Assert.Equal("second", observableList[1]);
            Assert.Equal("third", observableList[2]);
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Equal(3, eventArgs.NewItems.Count);
            Assert.Equal("first", eventArgs.NewItems[0]);
            Assert.Equal("second", eventArgs.NewItems[1]);
            Assert.Equal("third", eventArgs.NewItems[2]);
            Assert.Equal(0, eventArgs.NewStartingIndex);
        }

        /// <summary>
        /// Tests that AddRange correctly calculates the starting index when adding items
        /// to a non-empty collection.
        /// </summary>
        [Fact]
        public void AddRange_NonEmptyCollection_UsesCorrectStartingIndex()
        {
            // Arrange
            var observableList = new ObservableList<string> { "existing1", "existing2" };
            var newItems = new[] { "new1", "new2" };
            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            observableList.AddRange(newItems);

            // Assert
            Assert.Equal(4, observableList.Count);
            Assert.Equal("existing1", observableList[0]);
            Assert.Equal("existing2", observableList[1]);
            Assert.Equal("new1", observableList[2]);
            Assert.Equal("new2", observableList[3]);
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Equal(2, eventArgs.NewItems.Count);
            Assert.Equal(2, eventArgs.NewStartingIndex);
        }

        /// <summary>
        /// Tests that AddRange works with different IEnumerable implementations
        /// including HashSet, which may have different enumeration characteristics.
        /// </summary>
        [Fact]
        public void AddRange_DifferentEnumerableTypes_WorksCorrectly()
        {
            // Arrange
            var observableList = new ObservableList<int>();
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var eventFired = false;

            observableList.CollectionChanged += (sender, e) => eventFired = true;

            // Act
            observableList.AddRange(hashSet);

            // Assert
            Assert.Equal(3, observableList.Count);
            Assert.Contains(1, observableList);
            Assert.Contains(2, observableList);
            Assert.Contains(3, observableList);
            Assert.True(eventFired);
        }

        /// <summary>
        /// Tests that AddRange handles duplicate items correctly by adding all of them
        /// including the duplicates.
        /// </summary>
        [Fact]
        public void AddRange_DuplicateItems_AddsAllItems()
        {
            // Arrange
            var observableList = new ObservableList<string>();
            var itemsWithDuplicates = new[] { "item", "item", "different", "item" };
            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            observableList.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            observableList.AddRange(itemsWithDuplicates);

            // Assert
            Assert.Equal(4, observableList.Count);
            Assert.Equal("item", observableList[0]);
            Assert.Equal("item", observableList[1]);
            Assert.Equal("different", observableList[2]);
            Assert.Equal("item", observableList[3]);
            Assert.True(eventFired);
            Assert.Equal(4, eventArgs.NewItems.Count);
        }

        /// <summary>
        /// Tests AddRange with LINQ-generated enumerable to ensure it works
        /// with deferred execution scenarios.
        /// </summary>
        [Fact]
        public void AddRange_LinqEnumerable_WorksCorrectly()
        {
            // Arrange
            var observableList = new ObservableList<int>();
            var linqEnumerable = Enumerable.Range(1, 5).Where(x => x % 2 == 0);
            var eventFired = false;

            observableList.CollectionChanged += (sender, e) => eventFired = true;

            // Act
            observableList.AddRange(linqEnumerable);

            // Assert
            Assert.Equal(2, observableList.Count);
            Assert.Contains(2, observableList);
            Assert.Contains(4, observableList);
            Assert.True(eventFired);
        }
    }
}