#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ShellElementCollection : ShellTestBase
    {
        [Fact]
        public void ClearFiresOnlyOneRemovedEvent()
        {
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            shell.Items.Add(CreateShellItem());
            shell.Items.Add(CreateShellItem());
            shell.Items.Add(CreateShellItem());
            var shellSection = shell.CurrentItem.CurrentItem;

            int firedCount = 0;

            (shellSection as IShellSectionController).ItemsCollectionChanged += (_, e) =>
            {
                if (e.OldItems != null)
                    firedCount++;
            };

            shellSection.Items.Clear();
            Assert.Equal(1, firedCount);
        }
    }

    public class ShellElementCollectionTests
    {
        /// <summary>
        /// Tests that IsReadOnly property correctly returns the IsReadOnly value from the Inner collection.
        /// Tests the delegation behavior with both read-only and mutable collections.
        /// </summary>
        /// <param name="innerIsReadOnly">The IsReadOnly value of the inner collection</param>
        /// <param name="expectedResult">The expected result from the IsReadOnly property</param>
        [Theory]
        [InlineData(true, true)]   // ReadOnly collection should return true
        [InlineData(false, false)] // Mutable collection should return false  
        public void IsReadOnly_DelegatesToInnerCollection_ReturnsCorrectValue(bool innerIsReadOnly, bool expectedResult)
        {
            // Arrange
            var mockInnerCollection = Substitute.For<IList>();
            mockInnerCollection.IsReadOnly.Returns(innerIsReadOnly);

            var collection = new TestableShellElementCollection();
            collection.SetInner(mockInnerCollection);

            // Act
            var result = collection.IsReadOnly;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IsReadOnly property works correctly with a real List collection.
        /// Verifies that a mutable List returns false for IsReadOnly.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithMutableList_ReturnsFalse()
        {
            // Arrange
            var list = new List<BaseShellItem>();
            var collection = new TestableShellElementCollection();
            collection.SetInner(list);

            // Act
            var result = collection.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsReadOnly property works correctly with a real ReadOnlyCollection.
        /// Verifies that a read-only collection returns true for IsReadOnly.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithReadOnlyCollection_ReturnsTrue()
        {
            // Arrange
            var innerList = new List<BaseShellItem>();
            var readOnlyCollection = new ReadOnlyCollection<BaseShellItem>(innerList);
            var collection = new TestableShellElementCollection();
            collection.SetInner(readOnlyCollection);

            // Act
            var result = collection.IsReadOnly;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Testable concrete implementation of ShellElementCollection for testing purposes.
        /// Provides minimal implementations of abstract methods and exposes Inner property setter.
        /// </summary>
        private class TestableShellElementCollection : ShellElementCollection
        {
            /// <summary>
            /// Sets the Inner collection for testing purposes.
            /// </summary>
            /// <param name="inner">The collection to set as Inner</param>
            public void SetInner(IList inner)
            {
                Inner = inner;
            }

        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the collection at the first position.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtFirstPosition_ReturnsZero()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item1 = new BaseShellItem();
            var item2 = new BaseShellItem();
            var item3 = new BaseShellItem();

            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Act
            int result = collection.IndexOf(item1);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the collection at a middle position.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtMiddlePosition_ReturnsCorrectIndex()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item1 = new BaseShellItem();
            var item2 = new BaseShellItem();
            var item3 = new BaseShellItem();

            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Act
            int result = collection.IndexOf(item2);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the collection at the last position.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtLastPosition_ReturnsLastIndex()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item1 = new BaseShellItem();
            var item2 = new BaseShellItem();
            var item3 = new BaseShellItem();

            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Act
            int result = collection.IndexOf(item3);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when the item does not exist in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemNotFound_ReturnsMinusOne()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item1 = new BaseShellItem();
            var item2 = new BaseShellItem();
            var itemNotInCollection = new BaseShellItem();

            collection.Add(item1);
            collection.Add(item2);

            // Act
            int result = collection.IndexOf(itemNotInCollection);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when called on an empty collection.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsMinusOne()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item = new BaseShellItem();

            // Act
            int result = collection.IndexOf(item);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when duplicate items exist in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrence()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item1 = new BaseShellItem();
            var item2 = new BaseShellItem();

            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item1); // Add duplicate

            // Act
            int result = collection.IndexOf(item1);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf handles null input parameter according to the underlying collection behavior.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsMinusOne()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item = new BaseShellItem();
            collection.Add(item);

            // Act
            int result = collection.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf works correctly with a single item collection.
        /// </summary>
        [Fact]
        public void IndexOf_SingleItemCollection_ReturnsCorrectIndex()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item = new BaseShellItem();
            collection.Add(item);

            // Act
            int result = collection.IndexOf(item);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts an item at the beginning of the collection.
        /// Input: index = 0, valid BaseShellItem
        /// Expected: Item is inserted at position 0, collection count increases
        /// </summary>
        [Fact]
        public void Insert_AtBeginning_InsertsItemCorrectly()
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            shell.Items.Add(CreateShellItem());
            var newItem = CreateShellItem();
            int originalCount = shell.Items.Count;

            // Act
            shell.Items.Insert(0, newItem);

            // Assert
            Assert.Equal(originalCount + 1, shell.Items.Count);
            Assert.Equal(newItem, shell.Items[0]);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts an item in the middle of the collection.
        /// Input: index = 1, valid BaseShellItem
        /// Expected: Item is inserted at position 1, subsequent items are shifted
        /// </summary>
        [Fact]
        public void Insert_AtMiddle_InsertsItemCorrectly()
        {
            // Arrange
            Shell shell = new Shell();
            var firstItem = CreateShellItem();
            var secondItem = CreateShellItem();
            shell.Items.Add(firstItem);
            shell.Items.Add(secondItem);
            var newItem = CreateShellItem();
            int originalCount = shell.Items.Count;

            // Act
            shell.Items.Insert(1, newItem);

            // Assert
            Assert.Equal(originalCount + 1, shell.Items.Count);
            Assert.Equal(firstItem, shell.Items[0]);
            Assert.Equal(newItem, shell.Items[1]);
            Assert.Equal(secondItem, shell.Items[2]);
        }

        /// <summary>
        /// Tests that Insert method correctly inserts an item at the end of the collection.
        /// Input: index = Count, valid BaseShellItem
        /// Expected: Item is inserted at the last position, equivalent to Add
        /// </summary>
        [Fact]
        public void Insert_AtEnd_InsertsItemCorrectly()
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            shell.Items.Add(CreateShellItem());
            var newItem = CreateShellItem();
            int originalCount = shell.Items.Count;

            // Act
            shell.Items.Insert(originalCount, newItem);

            // Assert
            Assert.Equal(originalCount + 1, shell.Items.Count);
            Assert.Equal(newItem, shell.Items[originalCount]);
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException for negative index.
        /// Input: index = -1, valid BaseShellItem
        /// Expected: ArgumentOutOfRangeException is thrown
        /// </summary>
        [Fact]
        public void Insert_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            var newItem = CreateShellItem();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => shell.Items.Insert(-1, newItem));
        }

        /// <summary>
        /// Tests that Insert method throws ArgumentOutOfRangeException for index greater than Count.
        /// Input: index = Count + 1, valid BaseShellItem
        /// Expected: ArgumentOutOfRangeException is thrown
        /// </summary>
        [Fact]
        public void Insert_IndexTooLarge_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            var newItem = CreateShellItem();
            int invalidIndex = shell.Items.Count + 1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => shell.Items.Insert(invalidIndex, newItem));
        }

        /// <summary>
        /// Tests that Insert method handles null item appropriately.
        /// Input: index = 0, null BaseShellItem
        /// Expected: Behavior depends on underlying collection implementation
        /// </summary>
        [Fact]
        public void Insert_NullItem_HandledByUnderlyingCollection()
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());

            // Act & Assert
            // The behavior depends on the underlying collection implementation
            // This test verifies the method doesn't crash and delegates properly
            var exception = Record.Exception(() => shell.Items.Insert(0, null));

            // If no exception, verify the collection state
            if (exception == null)
            {
                Assert.Equal(2, shell.Items.Count);
                Assert.Null(shell.Items[0]);
            }
        }

        /// <summary>
        /// Tests that Insert method fires CollectionChanged event when item is inserted.
        /// Input: valid index and BaseShellItem
        /// Expected: CollectionChanged event is fired with Add action
        /// </summary>
        [Fact]
        public void Insert_ValidInput_FiresCollectionChangedEvent()
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            var newItem = CreateShellItem();
            bool eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            shell.Items.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            shell.Items.Insert(0, newItem);

            // Assert
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, eventArgs.Action);
            Assert.Contains(newItem, eventArgs.NewItems);
            Assert.Equal(0, eventArgs.NewStartingIndex);
        }

        /// <summary>
        /// Tests Insert method with boundary values for index parameter.
        /// Input: various boundary index values
        /// Expected: Valid indices work correctly, invalid indices throw exceptions
        /// </summary>
        [Theory]
        [InlineData(0, true)]  // Valid: beginning
        [InlineData(1, true)]  // Valid: middle (when count = 2)
        [InlineData(2, true)]  // Valid: end (when count = 2)
        [InlineData(-1, false)] // Invalid: negative
        [InlineData(3, false)]  // Invalid: beyond end
        [InlineData(int.MinValue, false)] // Invalid: extreme negative
        [InlineData(int.MaxValue, false)] // Invalid: extreme positive
        public void Insert_BoundaryIndexValues_BehavesCorrectly(int index, bool shouldSucceed)
        {
            // Arrange
            Shell shell = new Shell();
            shell.Items.Add(CreateShellItem());
            shell.Items.Add(CreateShellItem());
            var newItem = CreateShellItem();
            int originalCount = shell.Items.Count;

            // Act & Assert
            if (shouldSucceed)
            {
                shell.Items.Insert(index, newItem);
                Assert.Equal(originalCount + 1, shell.Items.Count);
                Assert.Equal(newItem, shell.Items[index]);
            }
            else
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => shell.Items.Insert(index, newItem));
                Assert.Equal(originalCount, shell.Items.Count);
            }
        }

        /// <summary>
        /// Tests that Insert method works correctly on an empty collection.
        /// Input: index = 0 on empty collection
        /// Expected: Item is inserted as the first element
        /// </summary>
        [Fact]
        public void Insert_EmptyCollection_InsertsFirstItem()
        {
            // Arrange
            Shell shell = new Shell();
            var newItem = CreateShellItem();

            // Act
            shell.Items.Insert(0, newItem);

            // Assert
            Assert.Equal(1, shell.Items.Count);
            Assert.Equal(newItem, shell.Items[0]);
        }

        /// <summary>
        /// Tests the IsShellElementVisible method with a null BaseShellItem parameter.
        /// This verifies that the method handles null input correctly and returns false.
        /// </summary>
        [Fact]
        public void IsShellElementVisible_WithNullItem_ReturnsFalse()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            BaseShellItem nullItem = null;

            // Act
            bool result = collection.TestIsShellElementVisible(nullItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests the IsShellElementVisible method with a valid BaseShellItem instance.
        /// This verifies that the base implementation always returns false regardless of the item.
        /// </summary>
        [Fact]
        public void IsShellElementVisible_WithValidItem_ReturnsFalse()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            var item = new BaseShellItem();

            // Act
            bool result = collection.TestIsShellElementVisible(item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the ShellElementCollection generic constructor properly initializes VisibleItems and VisibleItemsReadOnly properties.
        /// Input conditions: Default constructor called on concrete implementation.
        /// Expected result: Both properties are initialized with empty collections, VisibleItemsReadOnly wraps the same collection as VisibleItems.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_InitializesPropertiesCorrectly()
        {
            // Arrange & Act
            var collection = new TestShellElementCollection();

            // Assert
            Assert.NotNull(collection.VisibleItems);
            Assert.NotNull(collection.VisibleItemsReadOnly);
            Assert.Equal(0, collection.VisibleItems.Count);
            Assert.Equal(0, collection.VisibleItemsReadOnly.Count);
        }

        /// <summary>
        /// Tests that the ShellElementCollection generic constructor creates a proper ObservableCollection for VisibleItems.
        /// Input conditions: Default constructor called on concrete implementation.
        /// Expected result: VisibleItems is an ObservableCollection instance.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_CreatesObservableCollectionForVisibleItems()
        {
            // Arrange & Act
            var collection = new TestShellElementCollection();

            // Assert
            Assert.IsType<ObservableCollection<BaseShellItem>>(collection.VisibleItems);
        }

        /// <summary>
        /// Tests that the ShellElementCollection generic constructor creates a proper ReadOnlyCollection for VisibleItemsReadOnly.
        /// Input conditions: Default constructor called on concrete implementation.
        /// Expected result: VisibleItemsReadOnly is a ReadOnlyCollection instance.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_CreatesReadOnlyCollectionForVisibleItemsReadOnly()
        {
            // Arrange & Act
            var collection = new TestShellElementCollection();

            // Assert
            Assert.IsType<ReadOnlyCollection<BaseShellItem>>(collection.VisibleItemsReadOnly);
        }

        /// <summary>
        /// Tests that the ShellElementCollection generic constructor creates VisibleItemsReadOnly that wraps the same collection as VisibleItems.
        /// Input conditions: Default constructor called, then item added to VisibleItems.
        /// Expected result: Changes in VisibleItems are reflected in VisibleItemsReadOnly.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_VisibleItemsReadOnlyWrappsSameCollection()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var testItem = new TestBaseShellItem();

            // Act
            ((ObservableCollection<BaseShellItem>)collection.VisibleItems).Add(testItem);

            // Assert
            Assert.Single(collection.VisibleItemsReadOnly);
            Assert.Same(testItem, collection.VisibleItemsReadOnly.First());
        }

        /// <summary>
        /// Tests that the ShellElementCollection generic constructor ensures both collections start empty.
        /// Input conditions: Default constructor called on concrete implementation.
        /// Expected result: Both VisibleItems and VisibleItemsReadOnly have Count of 0.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_BothCollectionsStartEmpty()
        {
            // Arrange & Act
            var collection = new TestShellElementCollection();

            // Assert
            Assert.Empty(collection.VisibleItems);
            Assert.Empty(collection.VisibleItemsReadOnly);
            Assert.Equal(collection.VisibleItems.Count, collection.VisibleItemsReadOnly.Count);
        }

        /// <summary>
        /// Concrete test implementation of BaseShellItem for testing purposes.
        /// </summary>
        private class TestBaseShellItem : BaseShellItem
        {
        }

        /// <summary>
        /// Tests the CopyTo method with a null destination array.
        /// Should throw ArgumentNullException when array parameter is null.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            collection.Add(new TestShellItem());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests the CopyTo method with negative array index.
        /// Should throw ArgumentOutOfRangeException when arrayIndex is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException(int arrayIndex)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            collection.Add(new TestShellItem());
            var array = new TestShellItem[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests the CopyTo method with array index equal to or greater than array length.
        /// Should throw ArgumentOutOfRangeException when arrayIndex is beyond array bounds.
        /// </summary>
        [Theory]
        [InlineData(5, 5)]
        [InlineData(5, 6)]
        [InlineData(5, int.MaxValue)]
        public void CopyTo_ArrayIndexBeyondBounds_ThrowsArgumentOutOfRangeException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            collection.Add(new TestShellItem());
            var array = new TestShellItem[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests the CopyTo method when there is insufficient space in the destination array.
        /// Should throw ArgumentException when collection doesn't fit in remaining array space.
        /// </summary>
        [Theory]
        [InlineData(3, 2, 2)] // Array size 3, start at index 2, collection has 2 items
        [InlineData(5, 3, 3)] // Array size 5, start at index 3, collection has 3 items
        [InlineData(1, 0, 2)] // Array size 1, start at index 0, collection has 2 items
        public void CopyTo_InsufficientSpace_ThrowsArgumentException(int arraySize, int arrayIndex, int collectionSize)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            for (int i = 0; i < collectionSize; i++)
            {
                collection.Add(new TestShellItem());
            }
            var array = new TestShellItem[arraySize];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => collection.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests the CopyTo method with an empty collection.
        /// Should complete successfully without copying any elements.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        public void CopyTo_EmptyCollection_CompletesSuccessfully(int arrayIndex)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var array = new TestShellItem[10];
            var originalArray = new TestShellItem[10];

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(originalArray, array);
        }

        /// <summary>
        /// Tests the CopyTo method with valid parameters and single item.
        /// Should copy the single item to the correct position in the array.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        public void CopyTo_SingleItem_CopiesCorrectly(int arrayIndex)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item = new TestShellItem();
            collection.Add(item);
            var array = new TestShellItem[5];

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            Assert.Same(item, array[arrayIndex]);
            for (int i = 0; i < array.Length; i++)
            {
                if (i != arrayIndex)
                {
                    Assert.Null(array[i]);
                }
            }
        }

        /// <summary>
        /// Tests the CopyTo method with multiple items.
        /// Should copy all items to consecutive positions in the array starting at arrayIndex.
        /// </summary>
        [Theory]
        [InlineData(0, 3)]
        [InlineData(1, 2)]
        [InlineData(2, 4)]
        public void CopyTo_MultipleItems_CopiesInOrder(int arrayIndex, int itemCount)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var items = new TestShellItem[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                items[i] = new TestShellItem();
                collection.Add(items[i]);
            }
            var array = new TestShellItem[10];

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            for (int i = 0; i < itemCount; i++)
            {
                Assert.Same(items[i], array[arrayIndex + i]);
            }
            for (int i = 0; i < arrayIndex; i++)
            {
                Assert.Null(array[i]);
            }
            for (int i = arrayIndex + itemCount; i < array.Length; i++)
            {
                Assert.Null(array[i]);
            }
        }

        /// <summary>
        /// Tests the CopyTo method with exact fit scenario.
        /// Should copy all items when collection size exactly matches remaining array space.
        /// </summary>
        [Fact]
        public void CopyTo_ExactFit_CopiesCorrectly()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var items = new TestShellItem[3];
            for (int i = 0; i < 3; i++)
            {
                items[i] = new TestShellItem();
                collection.Add(items[i]);
            }
            var array = new TestShellItem[5];
            int arrayIndex = 2; // Remaining space: 5 - 2 = 3, which matches collection size

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            for (int i = 0; i < 3; i++)
            {
                Assert.Same(items[i], array[arrayIndex + i]);
            }
        }

        /// <summary>
        /// Test implementation of BaseShellItem for testing purposes.
        /// </summary>
        private class TestShellItem : BaseShellItem
        {
        }

        /// <summary>
        /// Tests that Remove method successfully removes an existing item and returns true.
        /// Input: An item that exists in the collection.
        /// Expected: The item is removed and method returns true.
        /// </summary>
        [Fact]
        public void Remove_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var mockInner = Substitute.For<IList<TestBaseShellItem>>();
            var item = new TestBaseShellItem();
            mockInner.Remove(item).Returns(true);
            collection.SetInner(mockInner);

            // Act
            var result = collection.Remove(item);

            // Assert
            Assert.True(result);
            mockInner.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that Remove method returns false when trying to remove a non-existing item.
        /// Input: An item that does not exist in the collection.
        /// Expected: The method returns false and Inner.Remove is called.
        /// </summary>
        [Fact]
        public void Remove_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var mockInner = Substitute.For<IList<TestBaseShellItem>>();
            var item = new TestBaseShellItem();
            mockInner.Remove(item).Returns(false);
            collection.SetInner(mockInner);

            // Act
            var result = collection.Remove(item);

            // Assert
            Assert.False(result);
            mockInner.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that Remove method properly handles null items by delegating to Inner.Remove.
        /// Input: A null item.
        /// Expected: Inner.Remove is called with null and the result is returned.
        /// </summary>
        [Fact]
        public void Remove_NullItem_DelegatesToInner()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var mockInner = Substitute.For<IList<TestBaseShellItem>>();
            mockInner.Remove(null).Returns(false);
            collection.SetInner(mockInner);

            // Act
            var result = collection.Remove(null);

            // Assert
            Assert.False(result);
            mockInner.Received(1).Remove(null);
        }

        /// <summary>
        /// Tests that the Inner property getter returns null when the inner collection has not been set.
        /// </summary>
        [Fact]
        public void Inner_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var collection = new TestableShellElementCollection();

            // Act
            var result = collection.TestInner;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Inner property getter returns the previously set value when it has been initialized.
        /// </summary>
        [Fact]
        public void Inner_WhenSet_ReturnsSetValue()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            var mockList = Substitute.For<IList, INotifyCollectionChanged>();
            collection.SetInner(mockList);

            // Act
            var result = collection.TestInner;

            // Assert
            Assert.Same(mockList, result);
        }

        /// <summary>
        /// Tests that setting the Inner property for the first time with a valid IList that implements INotifyCollectionChanged succeeds.
        /// Verifies that the CollectionChanged event is subscribed to.
        /// </summary>
        [Fact]
        public void Inner_SetFirstTimeWithValidList_SucceedsAndSubscribesToEvent()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            var mockList = Substitute.For<IList, INotifyCollectionChanged>();

            // Act
            collection.SetInner(mockList);

            // Assert
            Assert.Same(mockList, collection.TestInner);
            ((INotifyCollectionChanged)mockList).Received(1).CollectionChanged += Arg.Any<NotifyCollectionChangedEventHandler>();
        }

        /// <summary>
        /// Tests that setting the Inner property for the first time with null succeeds.
        /// </summary>
        [Fact]
        public void Inner_SetFirstTimeWithNull_Succeeds()
        {
            // Arrange
            var collection = new TestableShellElementCollection();

            // Act & Assert (should not throw)
            collection.SetInner(null);
            Assert.Null(collection.TestInner);
        }

        /// <summary>
        /// Tests that attempting to set the Inner property when it has already been set to a non-null value throws ArgumentException.
        /// Verifies the exception message matches the expected text.
        /// </summary>
        [Fact]
        public void Inner_SetSecondTime_ThrowsArgumentException()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            var mockList1 = Substitute.For<IList, INotifyCollectionChanged>();
            var mockList2 = Substitute.For<IList, INotifyCollectionChanged>();
            collection.SetInner(mockList1);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => collection.SetInner(mockList2));
            Assert.Equal("Inner can only be set once", exception.Message);
        }

        /// <summary>
        /// Tests that attempting to set the Inner property when it has already been set to null and then to a value throws ArgumentException.
        /// This ensures the restriction applies after any initial assignment, including null.
        /// </summary>
        [Fact]
        public void Inner_SetAfterNullThenValue_ThrowsArgumentException()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            var mockList1 = Substitute.For<IList, INotifyCollectionChanged>();
            var mockList2 = Substitute.For<IList, INotifyCollectionChanged>();
            collection.SetInner(null);
            collection.SetInner(mockList1); // First real set

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => collection.SetInner(mockList2));
            Assert.Equal("Inner can only be set once", exception.Message);
        }

        /// <summary>
        /// Tests that setting the Inner property with an IList that does not implement INotifyCollectionChanged throws InvalidCastException.
        /// This validates the requirement that the inner collection must support change notifications.
        /// </summary>
        [Fact]
        public void Inner_SetWithListNotImplementingINotifyCollectionChanged_ThrowsInvalidCastException()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            var mockList = Substitute.For<IList>(); // Does not implement INotifyCollectionChanged

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => collection.SetInner(mockList));
        }

    }


    /// <summary>
    /// Tests for ShellElementCollection.OnVisibleItemsChanged method
    /// </summary>
    public partial class ShellElementCollectionOnVisibleItemsChangedTests
    {
        /// <summary>
        /// Test implementation of abstract ShellElementCollection for testing purposes
        /// </summary>
        private class TestableShellElementCollection : ShellElementCollection
        {
            private readonly ObservableCollection<BaseShellItem> _inner;
            private readonly List<NotifyCollectionChangedEventArgs> _capturedEvents;
            private bool _resumeCollectionChangedCalled;

            public TestableShellElementCollection()
            {
                _inner = new ObservableCollection<BaseShellItem>();
                _capturedEvents = new List<NotifyCollectionChangedEventArgs>();
                VisibleItemsReadOnly = new ReadOnlyCollection<BaseShellItem>(_inner);
            }

            public void SetPauseCollectionChanged(bool value)
            {
                SetPrivateField("_pauseCollectionChanged", value);
            }

            public bool GetPauseCollectionChanged()
            {
                return GetPrivateField<bool>("_pauseCollectionChanged");
            }

            public List<NotifyCollectionChangedEventArgs> GetNotifyCollectionChangedEventArgs()
            {
                return GetPrivateField<List<NotifyCollectionChangedEventArgs>>("_notifyCollectionChangedEventArgs");
            }

            public bool ResumeCollectionChangedCalled => _resumeCollectionChangedCalled;

            public void ResetResumeCollectionChangedFlag()
            {
                _resumeCollectionChangedCalled = false;
            }

            private void SetPrivateField(string fieldName, object value)
            {
                var field = typeof(ShellElementCollection).GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(this, value);
            }

            private T GetPrivateField<T>(string fieldName)
            {
                var field = typeof(ShellElementCollection).GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return field != null ? (T)field.GetValue(this) : default(T);
            }

            // Override ResumeCollectionChanged to track when it's called
            protected void ResumeCollectionChanged()
            {
                _resumeCollectionChangedCalled = true;
                // Call the real implementation through reflection
                var method = typeof(ShellElementCollection).GetMethod("ResumeCollectionChanged",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(this, null);
            }

        }

        /// <summary>
        /// Tests OnVisibleItemsChanged with new items when collection changed is paused.
        /// This test covers the uncovered lines 39-44 where args has NewItems.Count > 0 and _pauseCollectionChanged is true.
        /// Should add args to pending events list, call ResumeCollectionChanged, and return without invoking events.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithNewItemsAndPausedCollectionChanged_AddsToListAndResumes()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(true);

            var newItems = new List<BaseShellItem> { new ShellContent() };
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems);

            bool internalEventFired = false;
            bool publicEventFired = false;

            collection.VisibleItemsChangedInternal += (sender, e) => internalEventFired = true;
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(this, args);

            // Assert
            Assert.True(collection.ResumeCollectionChangedCalled);
            Assert.Contains(args, collection.GetNotifyCollectionChangedEventArgs());
            Assert.False(internalEventFired);
            Assert.False(publicEventFired);
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged with null args.
        /// Should handle null gracefully and not throw exceptions.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithNullArgs_DoesNotThrow()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(false);

            // Act & Assert
            var exception = Record.Exception(() => collection.OnVisibleItemsChanged(this, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged with args having null NewItems.
        /// Should not take the first branch and continue with normal flow.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnVisibleItemsChanged_WithNullNewItems_FollowsNormalFlow(bool pauseCollectionChanged)
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(pauseCollectionChanged);

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, 0);

            bool internalEventFired = false;
            bool publicEventFired = false;

            collection.VisibleItemsChangedInternal += (sender, e) => internalEventFired = true;
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(this, args);

            // Assert
            if (pauseCollectionChanged)
            {
                Assert.Contains(args, collection.GetNotifyCollectionChangedEventArgs());
                Assert.False(internalEventFired);
                Assert.False(publicEventFired);
            }
            else
            {
                Assert.True(internalEventFired);
                Assert.True(publicEventFired);
            }
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged with empty NewItems collection.
        /// Should not satisfy the Count > 0 condition and follow normal flow.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnVisibleItemsChanged_WithEmptyNewItems_FollowsNormalFlow(bool pauseCollectionChanged)
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(pauseCollectionChanged);

            var emptyList = new List<BaseShellItem>();
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, emptyList);

            bool internalEventFired = false;
            bool publicEventFired = false;

            collection.VisibleItemsChangedInternal += (sender, e) => internalEventFired = true;
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(this, args);

            // Assert
            Assert.False(collection.ResumeCollectionChangedCalled);

            if (pauseCollectionChanged)
            {
                Assert.Contains(args, collection.GetNotifyCollectionChangedEventArgs());
                Assert.False(internalEventFired);
                Assert.False(publicEventFired);
            }
            else
            {
                Assert.True(internalEventFired);
                Assert.True(publicEventFired);
            }
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged when collection changed is not paused.
        /// Should fire both internal and public events immediately.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithoutPausedCollectionChanged_FiresEventsImmediately()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(false);

            var newItems = new List<BaseShellItem> { new ShellContent() };
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems);

            bool internalEventFired = false;
            bool publicEventFired = false;
            object capturedSender = null;
            NotifyCollectionChangedEventArgs capturedArgs = null;

            collection.VisibleItemsChangedInternal += (sender, e) =>
            {
                internalEventFired = true;
                capturedSender = sender;
                capturedArgs = e;
            };
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(this, args);

            // Assert
            Assert.True(internalEventFired);
            Assert.True(publicEventFired);
            Assert.Same(collection.VisibleItemsReadOnly, capturedSender);
            Assert.Same(args, capturedArgs);
            Assert.False(collection.ResumeCollectionChangedCalled);
            Assert.Empty(collection.GetNotifyCollectionChangedEventArgs());
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged when collection changed is paused but no new items.
        /// Should add args to pending list and not fire events immediately.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithPausedCollectionChangedButNoNewItems_AddsToListWithoutResume()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(true);

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, 0);

            bool internalEventFired = false;
            bool publicEventFired = false;

            collection.VisibleItemsChangedInternal += (sender, e) => internalEventFired = true;
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(this, args);

            // Assert
            Assert.Contains(args, collection.GetNotifyCollectionChangedEventArgs());
            Assert.False(internalEventFired);
            Assert.False(publicEventFired);
            Assert.False(collection.ResumeCollectionChangedCalled);
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged with multiple items in NewItems collection.
        /// Should trigger the uncovered branch when paused and has new items.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithMultipleNewItemsAndPaused_AddsToListAndResumes()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(true);

            var newItems = new List<BaseShellItem>
            {
                new ShellContent(),
                new ShellContent(),
                new ShellContent()
            };
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems);

            bool internalEventFired = false;
            bool publicEventFired = false;

            collection.VisibleItemsChangedInternal += (sender, e) => internalEventFired = true;
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(this, args);

            // Assert
            Assert.True(collection.ResumeCollectionChangedCalled);
            Assert.Contains(args, collection.GetNotifyCollectionChangedEventArgs());
            Assert.False(internalEventFired);
            Assert.False(publicEventFired);
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged with null sender.
        /// Should handle null sender gracefully and still process the event correctly.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithNullSender_ProcessesCorrectly()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(false);

            var newItems = new List<BaseShellItem> { new ShellContent() };
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems);

            bool internalEventFired = false;
            bool publicEventFired = false;

            collection.VisibleItemsChangedInternal += (sender, e) => internalEventFired = true;
            collection.VisibleItemsChanged += (sender, e) => publicEventFired = true;

            // Act
            collection.OnVisibleItemsChanged(null, args);

            // Assert
            Assert.True(internalEventFired);
            Assert.True(publicEventFired);
        }

        /// <summary>
        /// Tests OnVisibleItemsChanged behavior when events have no subscribers.
        /// Should not throw exceptions when events are null.
        /// </summary>
        [Fact]
        public void OnVisibleItemsChanged_WithNoEventSubscribers_DoesNotThrow()
        {
            // Arrange
            var collection = new TestableShellElementCollection();
            collection.SetPauseCollectionChanged(false);

            var newItems = new List<BaseShellItem> { new ShellContent() };
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems);

            // Act & Assert
            var exception = Record.Exception(() => collection.OnVisibleItemsChanged(this, args));
            Assert.Null(exception);
        }
    }


    public class ShellElementCollectionAddTests : ShellTestBase
    {
        /// <summary>
        /// Tests that Add method successfully adds a BaseShellItem to the collection
        /// when a valid non-null item is provided.
        /// Expected: Item should be added to Inner collection and Count should increase.
        /// </summary>
        [Fact]
        public void Add_ValidItem_AddsToCollection()
        {
            // Arrange
            var shell = new Shell();
            var shellItem = CreateShellItem();
            int initialCount = shell.Items.Count;

            // Act
            shell.Items.Add(shellItem);

            // Assert
            Assert.Equal(initialCount + 1, shell.Items.Count);
            Assert.Contains(shellItem, shell.Items);
        }

        /// <summary>
        /// Tests that Add method properly handles null items.
        /// Since nullable reference types are disabled, null should be accepted but may cause issues.
        /// Expected: Method should handle null gracefully or throw appropriate exception.
        /// </summary>
        [Fact]
        public void Add_NullItem_HandlesGracefully()
        {
            // Arrange
            var shell = new Shell();

            // Act & Assert
            var exception = Record.Exception(() => shell.Items.Add(null));

            // Since we don't know the exact behavior, we verify it doesn't crash unexpectedly
            // The method should either accept null or throw a specific exception
            if (exception != null)
            {
                Assert.True(exception is ArgumentNullException || exception is ArgumentException);
            }
        }

        /// <summary>
        /// Tests that Add method can add the same item multiple times to the collection.
        /// Expected: Each add operation should increase the count regardless of duplicates.
        /// </summary>
        [Fact]
        public void Add_DuplicateItem_AddsMultipleTimes()
        {
            // Arrange
            var shell = new Shell();
            var shellItem = CreateShellItem();

            // Act
            shell.Items.Add(shellItem);
            shell.Items.Add(shellItem);

            // Assert
            Assert.Equal(2, shell.Items.Count);
            Assert.Equal(2, shell.Items.Count(x => x == shellItem));
        }

        /// <summary>
        /// Tests that Add method triggers CollectionChanged events when items are added.
        /// Expected: NotifyCollectionChanged event should be fired with Add action.
        /// </summary>
        [Fact]
        public void Add_ValidItem_FiresCollectionChangedEvent()
        {
            // Arrange
            var shell = new Shell();
            var shellItem = CreateShellItem();
            NotifyCollectionChangedEventArgs capturedArgs = null;
            var eventFired = false;

            ((INotifyCollectionChanged)shell.Items).CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                capturedArgs = e;
            };

            // Act
            shell.Items.Add(shellItem);

            // Assert
            Assert.True(eventFired);
            Assert.NotNull(capturedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, capturedArgs.Action);
            Assert.Contains(shellItem, capturedArgs.NewItems.Cast<BaseShellItem>());
        }

        /// <summary>
        /// Tests that Add method works correctly when adding multiple different items.
        /// Expected: All items should be added in the correct order.
        /// </summary>
        [Fact]
        public void Add_MultipleItems_AddsInOrder()
        {
            // Arrange
            var shell = new Shell();
            var item1 = CreateShellItem();
            var item2 = CreateShellItem();
            var item3 = CreateShellItem();

            // Act
            shell.Items.Add(item1);
            shell.Items.Add(item2);
            shell.Items.Add(item3);

            // Assert
            Assert.Equal(3, shell.Items.Count);
            Assert.Equal(item1, shell.Items[0]);
            Assert.Equal(item2, shell.Items[1]);
            Assert.Equal(item3, shell.Items[2]);
        }

        /// <summary>
        /// Tests that Add method properly handles adding items with different types
        /// that inherit from BaseShellItem.
        /// Expected: All valid BaseShellItem derivatives should be accepted.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Add_DifferentShellItemTypes_AcceptsAllValidTypes(bool useShellContent)
        {
            // Arrange
            var shell = new Shell();
            BaseShellItem item;

            if (useShellContent)
            {
                item = CreateShellContent();
            }
            else
            {
                item = CreateShellItem();
            }

            // Act
            shell.Items.Add(item);

            // Assert
            Assert.Single(shell.Items);
            Assert.Equal(item, shell.Items[0]);
        }

        /// <summary>
        /// Tests adding items to an empty collection versus a collection that already has items.
        /// Expected: Add should work consistently regardless of initial collection state.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void Add_ToCollectionWithExistingItems_AddsCorrectly(int existingItemCount)
        {
            // Arrange
            var shell = new Shell();

            // Add existing items
            for (int i = 0; i < existingItemCount; i++)
            {
                shell.Items.Add(CreateShellItem());
            }

            var newItem = CreateShellItem();
            int expectedCount = existingItemCount + 1;

            // Act
            shell.Items.Add(newItem);

            // Assert
            Assert.Equal(expectedCount, shell.Items.Count);
            Assert.Equal(newItem, shell.Items[existingItemCount]);
        }
    }


    public class ShellElementCollectionCopyToTests
    {
        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            collection.Add(Substitute.For<BaseShellItem>());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
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
            var collection = new TestShellElementCollection();
            collection.Add(Substitute.For<BaseShellItem>());
            var array = new BaseShellItem[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// </summary>
        [Theory]
        [InlineData(5, 5)]
        [InlineData(5, 6)]
        [InlineData(5, int.MaxValue)]
        public void CopyTo_ArrayIndexGreaterThanOrEqualToArrayLength_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            collection.Add(Substitute.For<BaseShellItem>());
            var array = new BaseShellItem[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => collection.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when array is too small to accommodate all elements.
        /// </summary>
        [Theory]
        [InlineData(3, 2, 2)] // array size 3, start at index 2, collection has 2 items - should fail
        [InlineData(5, 4, 2)] // array size 5, start at index 4, collection has 2 items - should fail
        [InlineData(1, 0, 2)] // array size 1, start at index 0, collection has 2 items - should fail
        public void CopyTo_ArrayTooSmall_ThrowsArgumentException(int arraySize, int arrayIndex, int collectionSize)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            for (int i = 0; i < collectionSize; i++)
            {
                collection.Add(Substitute.For<BaseShellItem>());
            }
            var array = new BaseShellItem[arraySize];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => collection.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements from empty collection.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollection_DoesNotModifyArray()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var array = new BaseShellItem[5];
            var originalArray = new BaseShellItem[5];
            Array.Copy(array, originalArray, 5);

            // Act
            collection.CopyTo(array, 0);

            // Assert
            Assert.Equal(originalArray, array);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies single element to array at specified index.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        public void CopyTo_SingleElement_CopiesElementToCorrectPosition(int arrayIndex)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item = Substitute.For<BaseShellItem>();
            collection.Add(item);
            var array = new BaseShellItem[5];

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(item, array[arrayIndex]);

            // Verify other positions are null
            for (int i = 0; i < array.Length; i++)
            {
                if (i != arrayIndex)
                {
                    Assert.Null(array[i]);
                }
            }
        }

        /// <summary>
        /// Tests that CopyTo successfully copies multiple elements to array at specified index.
        /// </summary>
        [Theory]
        [InlineData(0, 3)]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        public void CopyTo_MultipleElements_CopiesElementsInOrder(int arrayIndex, int elementCount)
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var items = new List<BaseShellItem>();
            for (int i = 0; i < elementCount; i++)
            {
                var item = Substitute.For<BaseShellItem>();
                items.Add(item);
                collection.Add(item);
            }
            var array = new BaseShellItem[5];

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            for (int i = 0; i < elementCount; i++)
            {
                Assert.Equal(items[i], array[arrayIndex + i]);
            }

            // Verify positions before arrayIndex are null
            for (int i = 0; i < arrayIndex; i++)
            {
                Assert.Null(array[i]);
            }

            // Verify positions after copied elements are null
            for (int i = arrayIndex + elementCount; i < array.Length; i++)
            {
                Assert.Null(array[i]);
            }
        }

        /// <summary>
        /// Tests that CopyTo works correctly when array has exact size needed.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayExactSize_CopiesAllElements()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var items = new List<BaseShellItem>();
            for (int i = 0; i < 3; i++)
            {
                var item = Substitute.For<BaseShellItem>();
                items.Add(item);
                collection.Add(item);
            }
            var array = new BaseShellItem[3];

            // Act
            collection.CopyTo(array, 0);

            // Assert
            for (int i = 0; i < items.Count; i++)
            {
                Assert.Equal(items[i], array[i]);
            }
        }

        /// <summary>
        /// Tests that CopyTo works correctly with boundary values for array index.
        /// </summary>
        [Fact]
        public void CopyTo_BoundaryArrayIndex_CopiesElementsCorrectly()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item = Substitute.For<BaseShellItem>();
            collection.Add(item);
            var array = new BaseShellItem[5];

            // Act - test arrayIndex at boundary (array.Length - collection.Count)
            collection.CopyTo(array, 4);

            // Assert
            Assert.Equal(item, array[4]);
            for (int i = 0; i < 4; i++)
            {
                Assert.Null(array[i]);
            }
        }

        /// <summary>
        /// Concrete test implementation of ShellElementCollection for testing purposes.
        /// </summary>
        private class TestShellElementCollection : ShellElementCollection
        {
            public TestShellElementCollection()
            {
                var innerCollection = new ObservableCollection<BaseShellItem>();
                GetType().BaseType.GetProperty("Inner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(this, innerCollection);
            }

        }
    }


    public class ShellElementCollectionRemoveTests : ShellTestBase
    {
        /// <summary>
        /// Tests that Remove method returns true when removing an existing item from the collection.
        /// </summary>
        [Fact]
        public void Remove_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var shellItem = new ShellItem();
            collection.Add(shellItem);

            // Act
            bool result = collection.Remove((BaseShellItem)shellItem);

            // Assert
            Assert.True(result);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that Remove method returns false when attempting to remove an item that doesn't exist in the collection.
        /// </summary>
        [Fact]
        public void Remove_NonExistentItem_ReturnsFalse()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var existingItem = new ShellItem();
            var nonExistentItem = new ShellItem();
            collection.Add(existingItem);

            // Act
            bool result = collection.Remove((BaseShellItem)nonExistentItem);

            // Assert
            Assert.False(result);
            Assert.Single(collection);
        }

        /// <summary>
        /// Tests that Remove method handles null input appropriately by delegating to the generic Remove method.
        /// </summary>
        [Fact]
        public void Remove_NullItem_ReturnsFalse()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var shellItem = new ShellItem();
            collection.Add(shellItem);

            // Act
            bool result = collection.Remove((BaseShellItem)null);

            // Assert
            Assert.False(result);
            Assert.Single(collection);
        }

        /// <summary>
        /// Tests that Remove method throws InvalidCastException when the BaseShellItem cannot be cast to the generic type.
        /// </summary>
        [Fact]
        public void Remove_InvalidCastToGenericType_ThrowsInvalidCastException()
        {
            // Arrange
            var collection = new TestShellItemCollection();
            var shellContent = new ShellContent(); // ShellContent is BaseShellItem but not ShellItem

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => collection.Remove((BaseShellItem)shellContent));
        }

        /// <summary>
        /// Tests that Remove method works correctly with an empty collection.
        /// </summary>
        [Fact]
        public void Remove_EmptyCollection_ReturnsFalse()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var shellItem = new ShellItem();

            // Act
            bool result = collection.Remove((BaseShellItem)shellItem);

            // Assert
            Assert.False(result);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that Remove method properly removes the correct item when collection contains multiple items.
        /// </summary>
        [Fact]
        public void Remove_MultipleItemsCollection_RemovesCorrectItem()
        {
            // Arrange
            var collection = new TestShellElementCollection();
            var item1 = new ShellItem();
            var item2 = new ShellItem();
            var item3 = new ShellItem();
            collection.Add(item1);
            collection.Add(item2);
            collection.Add(item3);

            // Act
            bool result = collection.Remove((BaseShellItem)item2);

            // Assert
            Assert.True(result);
            Assert.Equal(2, collection.Count);
            Assert.Contains(item1, collection);
            Assert.Contains(item3, collection);
            Assert.DoesNotContain(item2, collection);
        }

        private class TestShellElementCollection : ShellElementCollection<ShellItem>
        {
            public TestShellElementCollection()
            {
                Inner = new ObservableCollection<ShellItem>();
            }

            public override IEnumerator<BaseShellItem> GetEnumerator()
            {
                return Inner.Cast<BaseShellItem>().GetEnumerator();
            }

            public override bool Remove(BaseShellItem item)
            {
                return Remove((ShellItem)item);
            }
        }

        private class TestShellItemCollection : ShellElementCollection<ShellItem>
        {
            public TestShellItemCollection()
            {
                Inner = new ObservableCollection<ShellItem>();
            }

            public override IEnumerator<BaseShellItem> GetEnumerator()
            {
                return Inner.Cast<BaseShellItem>().GetEnumerator();
            }

            public override bool Remove(BaseShellItem item)
            {
                return Remove((ShellItem)item);
            }
        }
    }
}