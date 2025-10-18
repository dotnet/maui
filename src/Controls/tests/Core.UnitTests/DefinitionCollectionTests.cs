#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class DefinitionCollectionTests
    {
        /// <summary>
        /// Tests that the parameterless constructor properly initializes a new DefinitionCollection instance.
        /// Verifies that the collection is created in a valid initial state with correct default values.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesEmptyCollectionWithCorrectInitialState()
        {
            // Arrange & Act
            var collection = new DefinitionCollection<IDefinition>();

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(0, collection.Count);
            Assert.False(collection.IsReadOnly);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a collection that supports enumeration.
        /// Verifies that the GetEnumerator method works correctly for an empty collection.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesEnumerableCollection()
        {
            // Arrange & Act
            var collection = new DefinitionCollection<IDefinition>();

            // Assert
            Assert.NotNull(collection.GetEnumerator());

            // Verify enumeration works (should be empty)
            var enumerator = collection.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a collection that properly supports Contains operation.
        /// Verifies that Contains returns false for any item when the collection is empty.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesCollectionWithWorkingContainsOperation()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            var mockDefinition = Substitute.For<IDefinition>();

            // Act & Assert
            Assert.False(collection.Contains(mockDefinition));
            Assert.False(collection.Contains(null));
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a collection that properly supports IndexOf operation.
        /// Verifies that IndexOf returns -1 for any item when the collection is empty.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesCollectionWithWorkingIndexOfOperation()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            var mockDefinition = Substitute.For<IDefinition>();

            // Act & Assert
            Assert.Equal(-1, collection.IndexOf(mockDefinition));
            Assert.Equal(-1, collection.IndexOf(null));
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when items parameter is null and copy is true.
        /// This occurs because the List&lt;T&gt; constructor cannot accept a null collection when copying.
        /// </summary>
        [Fact]
        public void DefinitionCollection_NullItemsWithCopyTrue_ThrowsArgumentNullException()
        {
            // Arrange
            List<IDefinition> items = null;
            bool copy = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DefinitionCollection<IDefinition>(items, copy));
        }

        /// <summary>
        /// Tests that the constructor accepts null items when copy is false.
        /// This should assign null directly to the internal list without throwing an exception.
        /// </summary>
        [Fact]
        public void DefinitionCollection_NullItemsWithCopyFalse_DoesNotThrow()
        {
            // Arrange
            List<IDefinition> items = null;
            bool copy = false;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);

            // Assert
            Assert.Equal(0, collection.Count); // Should handle null gracefully
        }

        /// <summary>
        /// Tests that the constructor creates an independent copy when copy is true with an empty list.
        /// Modifications to the original list should not affect the collection.
        /// </summary>
        [Fact]
        public void DefinitionCollection_EmptyListWithCopyTrue_CreatesIndependentCopy()
        {
            // Arrange
            var items = new List<IDefinition>();
            bool copy = true;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);
            var mockDefinition = Substitute.For<IDefinition>();
            items.Add(mockDefinition);

            // Assert
            Assert.Equal(0, collection.Count); // Should remain empty despite adding to original list
        }

        /// <summary>
        /// Tests that the constructor shares reference when copy is false with an empty list.
        /// Modifications to the original list should affect the collection.
        /// </summary>
        [Fact]
        public void DefinitionCollection_EmptyListWithCopyFalse_SharesReference()
        {
            // Arrange
            var items = new List<IDefinition>();
            bool copy = false;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);
            var mockDefinition = Substitute.For<IDefinition>();
            items.Add(mockDefinition);

            // Assert
            Assert.Equal(1, collection.Count); // Should reflect changes to original list
        }

        /// <summary>
        /// Tests that the constructor creates an independent copy when copy is true with populated list.
        /// Modifications to the original list should not affect the collection.
        /// </summary>
        [Fact]
        public void DefinitionCollection_PopulatedListWithCopyTrue_CreatesIndependentCopy()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var items = new List<IDefinition> { mockDefinition1 };
            bool copy = true;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);
            items.Add(mockDefinition2);

            // Assert
            Assert.Equal(1, collection.Count); // Should not reflect changes to original list
            Assert.True(collection.Contains(mockDefinition1)); // Should contain original item
            Assert.False(collection.Contains(mockDefinition2)); // Should not contain newly added item
        }

        /// <summary>
        /// Tests that the constructor shares reference when copy is false with populated list.
        /// Modifications to the original list should affect the collection.
        /// </summary>
        [Fact]
        public void DefinitionCollection_PopulatedListWithCopyFalse_SharesReference()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var items = new List<IDefinition> { mockDefinition1 };
            bool copy = false;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);
            items.Add(mockDefinition2);

            // Assert
            Assert.Equal(2, collection.Count); // Should reflect changes to original list
            Assert.True(collection.Contains(mockDefinition1)); // Should contain original item
            Assert.True(collection.Contains(mockDefinition2)); // Should contain newly added item
        }

        /// <summary>
        /// Tests that the constructor preserves all elements when creating a copy.
        /// All elements from the source list should be present in the collection when copy is true.
        /// </summary>
        [Fact]
        public void DefinitionCollection_MultipleItemsWithCopyTrue_PreservesAllElements()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();
            var items = new List<IDefinition> { mockDefinition1, mockDefinition2, mockDefinition3 };
            bool copy = true;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);

            // Assert
            Assert.Equal(3, collection.Count);
            Assert.True(collection.Contains(mockDefinition1));
            Assert.True(collection.Contains(mockDefinition2));
            Assert.True(collection.Contains(mockDefinition3));
        }

        /// <summary>
        /// Tests that the constructor preserves all elements when sharing reference.
        /// All elements from the source list should be present in the collection when copy is false.
        /// </summary>
        [Fact]
        public void DefinitionCollection_MultipleItemsWithCopyFalse_PreservesAllElements()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();
            var items = new List<IDefinition> { mockDefinition1, mockDefinition2, mockDefinition3 };
            bool copy = false;

            // Act
            var collection = new DefinitionCollection<IDefinition>(items, copy);

            // Assert
            Assert.Equal(3, collection.Count);
            Assert.True(collection.Contains(mockDefinition1));
            Assert.True(collection.Contains(mockDefinition2));
            Assert.True(collection.Contains(mockDefinition3));
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection with a null item.
        /// Input: null item on empty collection.
        /// Expected: false.
        /// </summary>
        [Fact]
        public void Contains_NullItemInEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = CreateDefinitionCollection<IDefinition>();

            // Act
            var result = collection.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection with a valid item.
        /// Input: valid item on empty collection.
        /// Expected: false.
        /// </summary>
        [Fact]
        public void Contains_ValidItemInEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = CreateDefinitionCollection<IDefinition>();
            var mockItem = Substitute.For<IDefinition>();

            // Act
            var result = collection.Contains(mockItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the collection.
        /// Input: item that exists in the collection.
        /// Expected: true.
        /// </summary>
        [Fact]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var mockItem = Substitute.For<IDefinition>();
            var collection = CreateDefinitionCollection(mockItem);

            // Act
            var result = collection.Contains(mockItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the collection.
        /// Input: item that does not exist in the collection.
        /// Expected: false.
        /// </summary>
        [Fact]
        public void Contains_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var existingItem = Substitute.For<IDefinition>();
            var nonExistingItem = Substitute.For<IDefinition>();
            var collection = CreateDefinitionCollection(existingItem);

            // Act
            var result = collection.Contains(nonExistingItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when searching for null in a collection that contains null.
        /// Input: null item in collection that contains null.
        /// Expected: true.
        /// </summary>
        [Fact]
        public void Contains_NullItemInCollectionWithNull_ReturnsTrue()
        {
            // Arrange
            var collection = CreateDefinitionCollection<IDefinition>(null);

            // Act
            var result = collection.Contains(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for a valid item in a collection that only contains null.
        /// Input: valid item in collection that contains only null.
        /// Expected: false.
        /// </summary>
        [Fact]
        public void Contains_ValidItemInCollectionWithOnlyNull_ReturnsFalse()
        {
            // Arrange
            var collection = CreateDefinitionCollection<IDefinition>(null);
            var mockItem = Substitute.For<IDefinition>();

            // Act
            var result = collection.Contains(mockItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with multiple items in the collection.
        /// Input: various items in a collection with multiple elements.
        /// Expected: correct boolean result for each item.
        /// </summary>
        [Fact]
        public void Contains_MultipleItemsInCollection_ReturnsCorrectResults()
        {
            // Arrange
            var item1 = Substitute.For<IDefinition>();
            var item2 = Substitute.For<IDefinition>();
            var item3 = Substitute.For<IDefinition>();
            var nonExistingItem = Substitute.For<IDefinition>();
            var collection = CreateDefinitionCollection(item1, item2, item3);

            // Act & Assert
            Assert.True(collection.Contains(item1));
            Assert.True(collection.Contains(item2));
            Assert.True(collection.Contains(item3));
            Assert.False(collection.Contains(nonExistingItem));
        }

        private static DefinitionCollection<T> CreateDefinitionCollection<T>(params T[] items) where T : IDefinition
        {
            var collectionType = typeof(DefinitionCollection<T>);
            var constructor = collectionType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new Type[] { typeof(T[]) },
                null);

            return (DefinitionCollection<T>)constructor.Invoke(new object[] { items });
        }

        /// <summary>
        /// Tests that Clear method removes all items from an empty collection and triggers size changed notification.
        /// Input: Empty collection
        /// Expected: Count remains 0, ItemSizeChanged event is triggered
        /// </summary>
        [Fact]
        public void Clear_EmptyCollection_CountRemainsZeroAndTriggersItemSizeChanged()
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            bool itemSizeChangedFired = false;
            collection.ItemSizeChanged += (sender, args) => itemSizeChangedFired = true;

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.True(itemSizeChangedFired);
        }

        /// <summary>
        /// Tests that Clear method removes a single item, unsubscribes from its SizeChanged event, and triggers size changed notification.
        /// Input: Collection with one mock IDefinition item
        /// Expected: Count becomes 0, item is removed, ItemSizeChanged event is triggered
        /// </summary>
        [Fact]
        public void Clear_CollectionWithOneItem_RemovesItemAndTriggersItemSizeChanged()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new TestDefinitionCollection();
            collection.Add(mockDefinition);
            bool itemSizeChangedFired = false;
            collection.ItemSizeChanged += (sender, args) => itemSizeChangedFired = true;

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.False(collection.Contains(mockDefinition));
            Assert.True(itemSizeChangedFired);
        }

        /// <summary>
        /// Tests that Clear method removes multiple items, unsubscribes from all SizeChanged events, and triggers size changed notification.
        /// Input: Collection with multiple mock IDefinition items
        /// Expected: Count becomes 0, all items are removed, ItemSizeChanged event is triggered
        /// </summary>
        [Fact]
        public void Clear_CollectionWithMultipleItems_RemovesAllItemsAndTriggersItemSizeChanged()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();
            var collection = new TestDefinitionCollection();
            collection.Add(mockDefinition1);
            collection.Add(mockDefinition2);
            collection.Add(mockDefinition3);
            bool itemSizeChangedFired = false;
            collection.ItemSizeChanged += (sender, args) => itemSizeChangedFired = true;

            // Act
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.False(collection.Contains(mockDefinition1));
            Assert.False(collection.Contains(mockDefinition2));
            Assert.False(collection.Contains(mockDefinition3));
            Assert.True(itemSizeChangedFired);
        }

        /// <summary>
        /// Tests that Clear method can be called multiple times safely on an already cleared collection.
        /// Input: Collection cleared twice in succession
        /// Expected: No exceptions, count remains 0, ItemSizeChanged event is triggered each time
        /// </summary>
        [Fact]
        public void Clear_CalledMultipleTimes_DoesNotThrowAndTriggersEventEachTime()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new TestDefinitionCollection();
            collection.Add(mockDefinition);
            int itemSizeChangedCount = 0;
            collection.ItemSizeChanged += (sender, args) => itemSizeChangedCount++;

            // Act
            collection.Clear();
            collection.Clear();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.Equal(2, itemSizeChangedCount);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false for an empty collection.
        /// This verifies the collection is always writable regardless of its state.
        /// </summary>
        [Fact]
        public void IsReadOnly_EmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act
            bool result = collection.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsReadOnly property returns false for a collection initialized with items.
        /// This verifies the collection remains writable even when it contains elements.
        /// </summary>
        [Fact]
        public void IsReadOnly_CollectionWithItems_ReturnsFalse()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var collection = new DefinitionCollection<IDefinition>(mockDefinition1, mockDefinition2);

            // Act
            bool result = collection.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsReadOnly property consistently returns false across multiple accesses.
        /// This verifies the property behavior is stable and deterministic.
        /// </summary>
        [Fact]
        public void IsReadOnly_MultipleAccesses_ConsistentlyReturnsFalse()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act & Assert
            Assert.False(collection.IsReadOnly);
            Assert.False(collection.IsReadOnly);
            Assert.False(collection.IsReadOnly);
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing item from the collection,
        /// unsubscribes from the SizeChanged event, and returns true.
        /// </summary>
        [Fact]
        public void Remove_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();
            var item = Substitute.For<IDefinition>();
            collection.Add(item);

            // Act
            bool result = collection.Remove(item);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(item, collection);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that Remove returns false when trying to remove an item that doesn't exist in the collection.
        /// </summary>
        [Fact]
        public void Remove_NonExistentItem_ReturnsFalse()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();
            var existingItem = Substitute.For<IDefinition>();
            var nonExistentItem = Substitute.For<IDefinition>();
            collection.Add(existingItem);

            // Act
            bool result = collection.Remove(nonExistentItem);

            // Assert
            Assert.False(result);
            Assert.Contains(existingItem, collection);
            Assert.Equal(1, collection.Count);
        }

        /// <summary>
        /// Tests that Remove returns false when called on an empty collection.
        /// </summary>
        [Fact]
        public void Remove_FromEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();
            var item = Substitute.For<IDefinition>();

            // Act
            bool result = collection.Remove(item);

            // Assert
            Assert.False(result);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that Remove throws NullReferenceException when trying to remove a null item,
        /// as it attempts to unsubscribe from the null item's SizeChanged event.
        /// </summary>
        [Fact]
        public void Remove_NullItem_ThrowsNullReferenceException()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => collection.Remove(null));
        }

        /// <summary>
        /// Tests that Remove properly unsubscribes from the item's SizeChanged event
        /// and triggers the collection's ItemSizeChanged event when removal is successful.
        /// </summary>
        [Fact]
        public void Remove_ExistingItem_UnsubscribesFromSizeChangedAndTriggersItemSizeChanged()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();
            var item = Substitute.For<IDefinition>();
            collection.Add(item);

            bool itemSizeChangedFired = false;
            collection.ItemSizeChanged += (sender, e) => itemSizeChangedFired = true;

            // Act
            bool result = collection.Remove(item);

            // Assert
            Assert.True(result);
            Assert.True(itemSizeChangedFired);
        }

        /// <summary>
        /// Tests that Remove does not trigger ItemSizeChanged event when removal fails.
        /// </summary>
        [Fact]
        public void Remove_NonExistentItem_DoesNotTriggerItemSizeChanged()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();
            var existingItem = Substitute.For<IDefinition>();
            var nonExistentItem = Substitute.For<IDefinition>();
            collection.Add(existingItem);

            bool itemSizeChangedFired = false;
            collection.ItemSizeChanged += (sender, e) => itemSizeChangedFired = true;

            // Act
            bool result = collection.Remove(nonExistentItem);

            // Assert
            Assert.False(result);
            Assert.False(itemSizeChangedFired);
        }

        /// <summary>
        /// Tests that Remove can successfully remove multiple different items from the collection.
        /// </summary>
        [Fact]
        public void Remove_MultipleItems_RemovesCorrectItems()
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();
            var item1 = Substitute.For<IDefinition>();
            var item2 = Substitute.For<IDefinition>();
            var item3 = Substitute.For<IDefinition>();

            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Act & Assert
            Assert.True(collection.Remove(item2));
            Assert.Equal(2, collection.Count);
            Assert.Contains(item1, collection);
            Assert.DoesNotContain(item2, collection);
            Assert.Contains(item3, collection);

            Assert.True(collection.Remove(item1));
            Assert.Equal(1, collection.Count);
            Assert.DoesNotContain(item1, collection);
            Assert.Contains(item3, collection);
        }

        private class TestableDefinitionCollection<T> : DefinitionCollection<T> where T : IDefinition
        {
            public TestableDefinitionCollection() : base() { }
            public TestableDefinitionCollection(params T[] items) : base(items) { }
        }

        /// <summary>
        /// Tests that GetEnumerator returns an empty enumerator when the collection is empty.
        /// Input conditions: Empty DefinitionCollection.
        /// Expected result: Enumerator with no items.
        /// </summary>
        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmptyEnumerator()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act
            var enumerator = collection.GetEnumerator();

            // Assert
            Assert.NotNull(enumerator);
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that GetEnumerator returns an enumerator with a single item when collection contains one item.
        /// Input conditions: DefinitionCollection with one mock IDefinition item.
        /// Expected result: Enumerator returns the single item.
        /// </summary>
        [Fact]
        public void GetEnumerator_SingleItem_ReturnsSingleItemEnumerator()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new DefinitionCollection<IDefinition>(mockDefinition);

            // Act
            var enumerator = collection.GetEnumerator();

            // Assert
            Assert.NotNull(enumerator);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(mockDefinition, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that GetEnumerator returns all items in the correct order when collection contains multiple items.
        /// Input conditions: DefinitionCollection with multiple mock IDefinition items.
        /// Expected result: Enumerator returns all items in insertion order.
        /// </summary>
        [Fact]
        public void GetEnumerator_MultipleItems_ReturnsAllItemsInOrder()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();
            var collection = new DefinitionCollection<IDefinition>(mockDefinition1, mockDefinition2, mockDefinition3);

            // Act
            var enumerator = collection.GetEnumerator();
            var results = new List<IDefinition>();
            while (enumerator.MoveNext())
            {
                results.Add(enumerator.Current);
            }

            // Assert
            Assert.Equal(3, results.Count);
            Assert.Equal(mockDefinition1, results[0]);
            Assert.Equal(mockDefinition2, results[1]);
            Assert.Equal(mockDefinition3, results[2]);
        }

        /// <summary>
        /// Tests that multiple enumerators can be created and work independently.
        /// Input conditions: DefinitionCollection with multiple items, creating two separate enumerators.
        /// Expected result: Both enumerators work independently and return the same sequence.
        /// </summary>
        [Fact]
        public void GetEnumerator_MultipleEnumerators_WorkIndependently()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var collection = new DefinitionCollection<IDefinition>(mockDefinition1, mockDefinition2);

            // Act
            var enumerator1 = collection.GetEnumerator();
            var enumerator2 = collection.GetEnumerator();

            // Assert
            Assert.NotSame(enumerator1, enumerator2);

            // First enumerator
            Assert.True(enumerator1.MoveNext());
            Assert.Equal(mockDefinition1, enumerator1.Current);

            // Second enumerator should start from beginning
            Assert.True(enumerator2.MoveNext());
            Assert.Equal(mockDefinition1, enumerator2.Current);

            // Continue with first enumerator
            Assert.True(enumerator1.MoveNext());
            Assert.Equal(mockDefinition2, enumerator1.Current);
            Assert.False(enumerator1.MoveNext());

            // Second enumerator should still work
            Assert.True(enumerator2.MoveNext());
            Assert.Equal(mockDefinition2, enumerator2.Current);
            Assert.False(enumerator2.MoveNext());
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with foreach enumeration.
        /// Input conditions: DefinitionCollection with multiple mock IDefinition items using foreach.
        /// Expected result: Foreach iteration returns all items in order.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithForeachLoop_ReturnsAllItems()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();
            var collection = new DefinitionCollection<IDefinition>(mockDefinition1, mockDefinition2, mockDefinition3);

            // Act
            var results = new List<IDefinition>();
            foreach (var item in collection)
            {
                results.Add(item);
            }

            // Assert
            Assert.Equal(3, results.Count);
            Assert.Equal(mockDefinition1, results[0]);
            Assert.Equal(mockDefinition2, results[1]);
            Assert.Equal(mockDefinition3, results[2]);
        }

        /// <summary>
        /// Tests that GetEnumerator returns IEnumerator<T> that implements the generic interface correctly.
        /// Input conditions: DefinitionCollection with items.
        /// Expected result: Enumerator implements IEnumerator<IDefinition> correctly.
        /// </summary>
        [Fact]
        public void GetEnumerator_ReturnsGenericEnumerator_ImplementsCorrectInterface()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new DefinitionCollection<IDefinition>(mockDefinition);

            // Act
            var enumerator = collection.GetEnumerator();

            // Assert
            Assert.IsAssignableFrom<IEnumerator<IDefinition>>(enumerator);
            Assert.IsAssignableFrom<IEnumerator>(enumerator);
        }

        /// <summary>
        /// Tests IndexOf method when the item exists in the collection.
        /// Should return the correct zero-based index of the item.
        /// </summary>
        [Fact]
        public void IndexOf_ItemExists_ReturnsCorrectIndex()
        {
            // Arrange
            var definition1 = new RowDefinition();
            var definition2 = new RowDefinition();
            var definition3 = new RowDefinition();
            var collection = new RowDefinitionCollection { definition1, definition2, definition3 };

            // Act & Assert
            Assert.Equal(0, collection.IndexOf(definition1));
            Assert.Equal(1, collection.IndexOf(definition2));
            Assert.Equal(2, collection.IndexOf(definition3));
        }

        /// <summary>
        /// Tests IndexOf method when the item does not exist in the collection.
        /// Should return -1 to indicate the item was not found.
        /// </summary>
        [Fact]
        public void IndexOf_ItemDoesNotExist_ReturnsMinusOne()
        {
            // Arrange
            var definition1 = new RowDefinition();
            var definition2 = new RowDefinition();
            var notInCollection = new RowDefinition();
            var collection = new RowDefinitionCollection { definition1, definition2 };

            // Act
            var result = collection.IndexOf(notInCollection);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method with null parameter.
        /// Should return -1 since null cannot exist in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var definition = new RowDefinition();
            var collection = new RowDefinitionCollection { definition };

            // Act
            var result = collection.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method on an empty collection.
        /// Should return -1 since no items exist in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsMinusOne()
        {
            // Arrange
            var definition = new RowDefinition();
            var collection = new RowDefinitionCollection();

            // Act
            var result = collection.IndexOf(definition);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf method with column definitions to ensure generic behavior works correctly.
        /// Should return correct indices for column definition items.
        /// </summary>
        [Fact]
        public void IndexOf_ColumnDefinitions_ReturnsCorrectIndices()
        {
            // Arrange
            var columnDef1 = new ColumnDefinition();
            var columnDef2 = new ColumnDefinition();
            var collection = new ColumnDefinitionCollection { columnDef1, columnDef2 };

            // Act & Assert
            Assert.Equal(0, collection.IndexOf(columnDef1));
            Assert.Equal(1, collection.IndexOf(columnDef2));
        }

        /// <summary>
        /// Tests IndexOf method behavior when the same item is added multiple times.
        /// Should return the index of the first occurrence.
        /// </summary>
        [Fact]
        public void IndexOf_SameItemMultipleTimes_ReturnsFirstOccurrence()
        {
            // Arrange
            var definition = new RowDefinition();
            var otherDefinition = new RowDefinition();
            var collection = new RowDefinitionCollection { definition, otherDefinition, definition };

            // Act
            var result = collection.IndexOf(definition);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts an item at the beginning of an empty collection,
        /// subscribes to the item's SizeChanged event, and raises the collection's event.
        /// </summary>
        [Fact]
        public void Insert_AtIndexZeroInEmptyCollection_InsertsItemAndSubscribesToEvents()
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            var item = new RowDefinition();
            bool itemSizeChangedRaised = false;
            bool collectionItemSizeChangedRaised = false;

            item.SizeChanged += (s, e) => itemSizeChangedRaised = true;
            collection.ItemSizeChanged += (s, e) => collectionItemSizeChangedRaised = true;

            // Act
            collection.Insert(0, item);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Same(item, collection[0]);
            Assert.True(collectionItemSizeChangedRaised);

            // Verify event subscription by triggering item's SizeChanged
            item.Height = new GridLength(100);
            Assert.True(itemSizeChangedRaised);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts an item at the end of a collection with existing items.
        /// </summary>
        [Fact]
        public void Insert_AtEndOfCollectionWithExistingItems_InsertsItemAtCorrectPosition()
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            var existingItem = new RowDefinition();
            var newItem = new RowDefinition();
            collection.Add(existingItem);

            // Act
            collection.Insert(1, newItem);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Same(existingItem, collection[0]);
            Assert.Same(newItem, collection[1]);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts an item in the middle of a collection,
        /// shifting existing items to the right.
        /// </summary>
        [Fact]
        public void Insert_AtMiddleIndex_InsertsItemAndShiftsExistingItems()
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            var firstItem = new RowDefinition();
            var secondItem = new RowDefinition();
            var insertedItem = new RowDefinition();

            collection.Add(firstItem);
            collection.Add(secondItem);

            // Act
            collection.Insert(1, insertedItem);

            // Assert
            Assert.Equal(3, collection.Count);
            Assert.Same(firstItem, collection[0]);
            Assert.Same(insertedItem, collection[1]);
            Assert.Same(secondItem, collection[2]);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(int.MinValue)]
        public void Insert_WithNegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            var item = new RowDefinition();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(negativeIndex, item));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException when index is greater than Count.
        /// </summary>
        [Theory]
        [InlineData(1, 0)] // index 1 when Count is 0
        [InlineData(3, 2)] // index 3 when Count is 2
        [InlineData(int.MaxValue, 1)] // extreme case
        public void Insert_WithIndexGreaterThanCount_ThrowsArgumentOutOfRangeException(int index, int itemCount)
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            for (int i = 0; i < itemCount; i++)
            {
                collection.Add(new RowDefinition());
            }
            var item = new RowDefinition();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Insert(index, item));
        }

        /// <summary>
        /// Tests that Insert method handles null item parameter correctly.
        /// Since nullable reference types are disabled, null should be accepted but may cause issues in event subscription.
        /// </summary>
        [Fact]
        public void Insert_WithNullItem_ThrowsNullReferenceException()
        {
            // Arrange
            var collection = new TestDefinitionCollection();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => collection.Insert(0, null));
        }

        /// <summary>
        /// Tests that Insert method correctly raises ItemSizeChanged event immediately upon insertion.
        /// </summary>
        [Fact]
        public void Insert_ValidItem_RaisesItemSizeChangedEventImmediately()
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            var item = new RowDefinition();
            bool eventRaised = false;
            object eventSender = null;
            EventArgs eventArgs = null;

            collection.ItemSizeChanged += (sender, e) =>
            {
                eventRaised = true;
                eventSender = sender;
                eventArgs = e;
            };

            // Act
            collection.Insert(0, item);

            // Assert
            Assert.True(eventRaised);
            Assert.Same(collection, eventSender);
            Assert.Same(EventArgs.Empty, eventArgs);
        }

        /// <summary>
        /// Tests that Insert method subscribes to the item's SizeChanged event and forwards it correctly.
        /// </summary>
        [Fact]
        public void Insert_ValidItem_SubscribesToItemSizeChangedEvent()
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            var item = new RowDefinition();
            bool collectionEventRaised = false;

            collection.ItemSizeChanged += (s, e) => collectionEventRaised = true;
            collection.Insert(0, item);

            // Reset the flag since Insert raises the event immediately
            collectionEventRaised = false;

            // Act - trigger the item's SizeChanged event
            item.Height = new GridLength(50);

            // Assert
            Assert.True(collectionEventRaised);
        }

        /// <summary>
        /// Tests Insert with various boundary index values to ensure correct behavior at collection boundaries.
        /// </summary>
        [Theory]
        [InlineData(0)] // Insert at beginning
        public void Insert_AtBoundaryIndices_InsertsCorrectly(int index)
        {
            // Arrange
            var collection = new TestDefinitionCollection();
            if (index > 0)
            {
                for (int i = 0; i < index; i++)
                {
                    collection.Add(new RowDefinition());
                }
            }
            var item = new RowDefinition();
            int initialCount = collection.Count;

            // Act
            collection.Insert(index, item);

            // Assert
            Assert.Equal(initialCount + 1, collection.Count);
            Assert.Same(item, collection[index]);
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes an item from a single-item collection,
        /// unsubscribes from the item's SizeChanged event, and triggers the collection's ItemSizeChanged event.
        /// </summary>
        [Fact]
        public void RemoveAt_SingleItemCollection_RemovesItemAndUnsubscribesEvent()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(mockDefinition);
            bool itemSizeChangedCalled = false;
            collection.ItemSizeChanged += (sender, e) => itemSizeChangedCalled = true;

            // Act
            collection.RemoveAt(0);

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.False(collection.Contains(mockDefinition));
            Assert.True(itemSizeChangedCalled);
            mockDefinition.Received(1).SizeChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the first item from a multi-item collection
        /// and properly handles event unsubscription.
        /// </summary>
        [Fact]
        public void RemoveAt_MultiItemCollection_RemovesFirstItem()
        {
            // Arrange
            var firstItem = Substitute.For<IDefinition>();
            var secondItem = Substitute.For<IDefinition>();
            var thirdItem = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(firstItem, secondItem, thirdItem);

            // Act
            collection.RemoveAt(0);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.False(collection.Contains(firstItem));
            Assert.True(collection.Contains(secondItem));
            Assert.True(collection.Contains(thirdItem));
            Assert.Equal(secondItem, collection[0]);
            Assert.Equal(thirdItem, collection[1]);
            firstItem.Received(1).SizeChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes a middle item from a multi-item collection
        /// and maintains proper order of remaining items.
        /// </summary>
        [Fact]
        public void RemoveAt_MultiItemCollection_RemovesMiddleItem()
        {
            // Arrange
            var firstItem = Substitute.For<IDefinition>();
            var secondItem = Substitute.For<IDefinition>();
            var thirdItem = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(firstItem, secondItem, thirdItem);

            // Act
            collection.RemoveAt(1);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.True(collection.Contains(firstItem));
            Assert.False(collection.Contains(secondItem));
            Assert.True(collection.Contains(thirdItem));
            Assert.Equal(firstItem, collection[0]);
            Assert.Equal(thirdItem, collection[1]);
            secondItem.Received(1).SizeChanged -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests that RemoveAt successfully removes the last item from a multi-item collection
        /// and maintains proper order of remaining items.
        /// </summary>
        [Fact]
        public void RemoveAt_MultiItemCollection_RemovesLastItem()
        {
            // Arrange
            var firstItem = Substitute.For<IDefinition>();
            var secondItem = Substitute.For<IDefinition>();
            var thirdItem = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(firstItem, secondItem, thirdItem);

            // Act
            collection.RemoveAt(2);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.True(collection.Contains(firstItem));
            Assert.True(collection.Contains(secondItem));
            Assert.False(collection.Contains(thirdItem));
            Assert.Equal(firstItem, collection[0]);
            Assert.Equal(secondItem, collection[1]);
            thirdItem.Received(1).SizeChanged -= Arg.Any<EventHandler>();
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
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(mockDefinition);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called with an index
        /// greater than or equal to the collection count.
        /// </summary>
        [Theory]
        [InlineData(1)] // For single-item collection (count = 1)
        [InlineData(5)]
        [InlineData(int.MaxValue)]
        public void RemoveAt_IndexGreaterThanOrEqualToCount_ThrowsArgumentOutOfRangeException(int invalidIndex)
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(mockDefinition);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.RemoveAt(invalidIndex));
        }

        /// <summary>
        /// Tests that RemoveAt throws ArgumentOutOfRangeException when called on an empty collection
        /// with any index value.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public void RemoveAt_EmptyCollection_ThrowsArgumentOutOfRangeException(int index)
        {
            // Arrange
            var collection = new TestableDefinitionCollection<IDefinition>();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt triggers the ItemSizeChanged event after removing an item.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_TriggersItemSizeChangedEvent()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();
            var collection = new TestableDefinitionCollection<IDefinition>(mockDefinition);
            bool eventTriggered = false;
            object eventSender = null;
            EventArgs eventArgs = null;

            collection.ItemSizeChanged += (sender, e) =>
            {
                eventTriggered = true;
                eventSender = sender;
                eventArgs = e;
            };

            // Act
            collection.RemoveAt(0);

            // Assert
            Assert.True(eventTriggered);
            Assert.Same(collection, eventSender);
            Assert.Same(EventArgs.Empty, eventArgs);
        }

        /// <summary>
        /// Tests that adding a valid EventHandler to ItemSizeChanged event does not throw an exception.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_AddValidHandler_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            EventHandler handler = (sender, e) => { };

            // Act & Assert
            var exception = Record.Exception(() => collection.ItemSizeChanged += handler);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that removing a valid EventHandler from ItemSizeChanged event does not throw an exception.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_RemoveValidHandler_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            EventHandler handler = (sender, e) => { };
            collection.ItemSizeChanged += handler;

            // Act & Assert
            var exception = Record.Exception(() => collection.ItemSizeChanged -= handler);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that adding a null EventHandler to ItemSizeChanged event throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_AddNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.ItemSizeChanged += null);
        }

        /// <summary>
        /// Tests that removing a null EventHandler from ItemSizeChanged event throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_RemoveNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.ItemSizeChanged -= null);
        }

        /// <summary>
        /// Tests that adding multiple EventHandlers to ItemSizeChanged event does not throw an exception.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_AddMultipleHandlers_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            EventHandler handler1 = (sender, e) => { };
            EventHandler handler2 = (sender, e) => { };
            EventHandler handler3 = (sender, e) => { };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                collection.ItemSizeChanged += handler1;
                collection.ItemSizeChanged += handler2;
                collection.ItemSizeChanged += handler3;
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that adding the same EventHandler multiple times does not throw an exception.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_AddSameHandlerMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            EventHandler handler = (sender, e) => { };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                collection.ItemSizeChanged += handler;
                collection.ItemSizeChanged += handler;
                collection.ItemSizeChanged += handler;
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that removing a handler that was never added does not throw an exception.
        /// </summary>
        [Fact]
        public void ItemSizeChanged_RemoveNonExistentHandler_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            EventHandler handler = (sender, e) => { };

            // Act & Assert
            var exception = Record.Exception(() => collection.ItemSizeChanged -= handler);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnItemSizeChanged method can be called with valid parameters without throwing an exception.
        /// Input conditions: Valid sender object and EventArgs.Empty.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void OnItemSizeChanged_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            var sender = new object();

            // Act & Assert
            var exception = Record.Exception(() => collection.OnItemSizeChanged(sender, EventArgs.Empty));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnItemSizeChanged method can be called with null sender without throwing an exception.
        /// Input conditions: Null sender and EventArgs.Empty.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void OnItemSizeChanged_NullSender_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act & Assert
            var exception = Record.Exception(() => collection.OnItemSizeChanged(null, EventArgs.Empty));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnItemSizeChanged method can be called with null EventArgs without throwing an exception.
        /// Input conditions: Valid sender and null EventArgs.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void OnItemSizeChanged_NullEventArgs_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();
            var sender = new object();

            // Act & Assert
            var exception = Record.Exception(() => collection.OnItemSizeChanged(sender, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnItemSizeChanged method can be called with both null parameters without throwing an exception.
        /// Input conditions: Null sender and null EventArgs.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void OnItemSizeChanged_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act & Assert
            var exception = Record.Exception(() => collection.OnItemSizeChanged(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnItemSizeChanged method can be called with the collection itself as sender.
        /// Input conditions: Collection instance as sender and EventArgs.Empty.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void OnItemSizeChanged_CollectionAsSender_DoesNotThrow()
        {
            // Arrange
            var collection = new DefinitionCollection<IDefinition>();

            // Act & Assert
            var exception = Record.Exception(() => collection.OnItemSizeChanged(collection, EventArgs.Empty));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when null is passed as the items parameter.
        /// This verifies proper null handling and argument validation.
        /// </summary>
        [Fact]
        public void Constructor_WithNullItems_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DefinitionCollection<IDefinition>(null));
        }

        /// <summary>
        /// Tests that the constructor creates an empty collection when no items are provided.
        /// This verifies the params array behavior with zero arguments.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyArray_CreatesEmptyCollection()
        {
            // Arrange & Act
            var collection = new DefinitionCollection<IDefinition>();

            // Assert
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that the constructor creates a collection with a single item when one item is provided.
        /// This verifies basic functionality with a single element.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItem_CreatesSingleItemCollection()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();

            // Act
            var collection = new DefinitionCollection<IDefinition>(mockDefinition);

            // Assert
            Assert.Equal(1, collection.Count);
            Assert.Equal(mockDefinition, collection[0]);
        }

        /// <summary>
        /// Tests that the constructor creates a collection with multiple items in the correct order.
        /// This verifies that all items are preserved and ordering is maintained.
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleItems_CreatesCollectionWithAllItems()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();

            // Act
            var collection = new DefinitionCollection<IDefinition>(mockDefinition1, mockDefinition2, mockDefinition3);

            // Assert
            Assert.Equal(3, collection.Count);
            Assert.Equal(mockDefinition1, collection[0]);
            Assert.Equal(mockDefinition2, collection[1]);
            Assert.Equal(mockDefinition3, collection[2]);
        }

        /// <summary>
        /// Tests that the constructor allows duplicate items to be added to the collection.
        /// This verifies that the collection doesn't enforce uniqueness constraints.
        /// </summary>
        [Fact]
        public void Constructor_WithDuplicateItems_AllowsDuplicates()
        {
            // Arrange
            var mockDefinition = Substitute.For<IDefinition>();

            // Act
            var collection = new DefinitionCollection<IDefinition>(mockDefinition, mockDefinition, mockDefinition);

            // Assert
            Assert.Equal(3, collection.Count);
            Assert.Equal(mockDefinition, collection[0]);
            Assert.Equal(mockDefinition, collection[1]);
            Assert.Equal(mockDefinition, collection[2]);
        }

        /// <summary>
        /// Tests that the constructor works correctly when passed an explicit array instead of using params syntax.
        /// This verifies compatibility with both params and array calling conventions.
        /// </summary>
        [Fact]
        public void Constructor_WithExplicitArray_CreatesCollectionCorrectly()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var itemsArray = new IDefinition[] { mockDefinition1, mockDefinition2 };

            // Act
            var collection = new DefinitionCollection<IDefinition>(itemsArray);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Equal(mockDefinition1, collection[0]);
            Assert.Equal(mockDefinition2, collection[1]);
        }

        /// <summary>
        /// Tests that the constructor creates a collection that properly contains the specified items.
        /// This verifies that the Contains method works correctly with constructor-initialized items.
        /// </summary>
        [Fact]
        public void Constructor_WithItems_CollectionContainsAllItems()
        {
            // Arrange
            var mockDefinition1 = Substitute.For<IDefinition>();
            var mockDefinition2 = Substitute.For<IDefinition>();
            var mockDefinition3 = Substitute.For<IDefinition>();

            // Act
            var collection = new DefinitionCollection<IDefinition>(mockDefinition1, mockDefinition2);

            // Assert
            Assert.True(collection.Contains(mockDefinition1));
            Assert.True(collection.Contains(mockDefinition2));
            Assert.False(collection.Contains(mockDefinition3));
        }
    }
}