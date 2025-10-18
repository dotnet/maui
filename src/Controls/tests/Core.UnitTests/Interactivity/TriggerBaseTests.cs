#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TriggerBaseSealedListTests
    {
        /// <summary>
        /// Tests that the SealedList constructor properly initializes the internal list with string type parameter.
        /// Verifies the list starts empty, is not read-only, and can accept new items.
        /// </summary>
        [Fact]
        public void Constructor_WithStringType_InitializesEmptyNonReadOnlyList()
        {
            // Arrange & Act
            var sealedList = new TriggerBase.SealedList<string>();

            // Assert
            Assert.Equal(0, sealedList.Count);
            Assert.False(sealedList.IsReadOnly);

            // Verify functional behavior - should be able to add items
            sealedList.Add("test");
            Assert.Equal(1, sealedList.Count);
        }

        /// <summary>
        /// Tests that the SealedList constructor properly initializes the internal list with integer type parameter.
        /// Verifies the list starts empty, is not read-only, and can accept new items.
        /// </summary>
        [Fact]
        public void Constructor_WithIntType_InitializesEmptyNonReadOnlyList()
        {
            // Arrange & Act
            var sealedList = new TriggerBase.SealedList<int>();

            // Assert
            Assert.Equal(0, sealedList.Count);
            Assert.False(sealedList.IsReadOnly);

            // Verify functional behavior - should be able to add items
            sealedList.Add(42);
            Assert.Equal(1, sealedList.Count);
        }

        /// <summary>
        /// Tests that the SealedList constructor properly initializes the internal list with object type parameter.
        /// Verifies the list starts empty, is not read-only, and can accept new items.
        /// </summary>
        [Fact]
        public void Constructor_WithObjectType_InitializesEmptyNonReadOnlyList()
        {
            // Arrange & Act
            var sealedList = new TriggerBase.SealedList<object>();

            // Assert
            Assert.Equal(0, sealedList.Count);
            Assert.False(sealedList.IsReadOnly);

            // Verify functional behavior - should be able to add items
            var testObject = new object();
            sealedList.Add(testObject);
            Assert.Equal(1, sealedList.Count);
        }

        /// <summary>
        /// Tests that multiple SealedList instances created through the constructor are independent.
        /// Verifies that each instance has its own internal list and state.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_AreIndependent()
        {
            // Arrange & Act
            var sealedList1 = new TriggerBase.SealedList<string>();
            var sealedList2 = new TriggerBase.SealedList<string>();

            // Assert
            Assert.Equal(0, sealedList1.Count);
            Assert.Equal(0, sealedList2.Count);

            // Modify one instance
            sealedList1.Add("test");

            // Verify independence
            Assert.Equal(1, sealedList1.Count);
            Assert.Equal(0, sealedList2.Count);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// Input: null array parameter with any arrayIndex
        /// Expected: ArgumentNullException with parameter name "array"
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("item1");

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => sealedList.CopyTo(null, 0));
            Assert.Equal("array", exception.ParamName);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input: valid array with negative arrayIndex
        /// Expected: ArgumentOutOfRangeException
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException(int arrayIndex)
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("item1");
            var array = new string[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => sealedList.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// Input: array with arrayIndex >= array.Length
        /// Expected: ArgumentException
        /// </summary>
        [Theory]
        [InlineData(5, 5)]  // arrayIndex == array.Length
        [InlineData(5, 6)]  // arrayIndex > array.Length
        [InlineData(5, int.MaxValue)]  // arrayIndex >> array.Length
        [InlineData(0, 1)]  // empty array, any positive index
        public void CopyTo_ArrayIndexOutOfBounds_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("item1");
            var array = new string[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => sealedList.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is not enough space in the destination array.
        /// Input: array with insufficient space for all items starting at arrayIndex
        /// Expected: ArgumentException
        /// </summary>
        [Theory]
        [InlineData(3, 2, 2)]  // 3 items, array length 2, start at index 2 -> not enough space
        [InlineData(2, 3, 2)]  // 2 items, array length 3, start at index 2 -> only 1 space available
        [InlineData(5, 10, 6)] // 5 items, array length 10, start at index 6 -> only 4 spaces available
        public void CopyTo_InsufficientSpace_ThrowsArgumentException(int itemCount, int arrayLength, int arrayIndex)
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            for (int i = 0; i < itemCount; i++)
            {
                sealedList.Add($"item{i}");
            }
            var array = new string[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => sealedList.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies items from an empty list to any valid array position.
        /// Input: empty list with valid array and arrayIndex
        /// Expected: array remains unchanged (no items copied)
        /// </summary>
        [Theory]
        [InlineData(5, 0)]
        [InlineData(5, 2)]
        [InlineData(5, 4)]
        [InlineData(1, 0)]
        public void CopyTo_EmptyList_DoesNotModifyArray(int arrayLength, int arrayIndex)
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            var array = new string[arrayLength];
            var originalArray = new string[arrayLength];
            Array.Copy(array, originalArray, arrayLength);

            // Act
            sealedList.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(originalArray, array);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies a single item to the correct position in the destination array.
        /// Input: list with one item, valid array and arrayIndex
        /// Expected: item copied to array[arrayIndex], other positions unchanged
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void CopyTo_SingleItem_CopiesCorrectly(int arrayIndex)
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("test-item");
            var array = new string[5];

            // Act
            sealedList.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal("test-item", array[arrayIndex]);
            // Verify other positions are still null
            for (int i = 0; i < array.Length; i++)
            {
                if (i != arrayIndex)
                {
                    Assert.Null(array[i]);
                }
            }
        }

        /// <summary>
        /// Tests that CopyTo successfully copies multiple items to the correct sequential positions in the destination array.
        /// Input: list with multiple items, valid array and arrayIndex
        /// Expected: all items copied sequentially starting at array[arrayIndex]
        /// </summary>
        [Fact]
        public void CopyTo_MultipleItems_CopiesInOrder()
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("first");
            sealedList.Add("second");
            sealedList.Add("third");
            var array = new string[6];

            // Act
            sealedList.CopyTo(array, 2);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Equal("first", array[2]);
            Assert.Equal("second", array[3]);
            Assert.Equal("third", array[4]);
            Assert.Null(array[5]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when copying to the beginning of the array.
        /// Input: list with items, arrayIndex = 0
        /// Expected: items copied starting from index 0
        /// </summary>
        [Fact]
        public void CopyTo_CopyToArrayStart_CopiesCorrectly()
        {
            // Arrange
            var sealedList = new TestableSealed<int>();
            sealedList.Add(10);
            sealedList.Add(20);
            var array = new int[3];

            // Act
            sealedList.CopyTo(array, 0);

            // Assert
            Assert.Equal(10, array[0]);
            Assert.Equal(20, array[1]);
            Assert.Equal(0, array[2]); // default int value
        }

        /// <summary>
        /// Tests that CopyTo works correctly when the destination array is exactly the right size.
        /// Input: list with items, array with exact size needed starting at arrayIndex
        /// Expected: all items fit perfectly with no extra space
        /// </summary>
        [Fact]
        public void CopyTo_ExactFit_CopiesCorrectly()
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("alpha");
            sealedList.Add("beta");
            sealedList.Add("gamma");
            var array = new string[5];

            // Act - copy 3 items starting at index 2 (positions 2,3,4)
            sealedList.CopyTo(array, 2);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Equal("alpha", array[2]);
            Assert.Equal("beta", array[3]);
            Assert.Equal("gamma", array[4]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when the list is marked as read-only.
        /// Input: read-only list with items
        /// Expected: CopyTo works normally (read operations should not be affected by read-only status)
        /// </summary>
        [Fact]
        public void CopyTo_ReadOnlyList_CopiesCorrectly()
        {
            // Arrange
            var sealedList = new TestableSealed<string>();
            sealedList.Add("item1");
            sealedList.Add("item2");
            sealedList.IsReadOnly = true; // Seal the list
            var array = new string[4];

            // Act
            sealedList.CopyTo(array, 1);

            // Assert
            Assert.Null(array[0]);
            Assert.Equal("item1", array[1]);
            Assert.Equal("item2", array[2]);
            Assert.Null(array[3]);
        }

        /// <summary>
        /// Helper class to expose SealedList<T> for testing since it's an internal nested class.
        /// </summary>
        private class TestableSealed<T> : IList<T>
        {
            private readonly TriggerBase.SealedList<T> _sealedList;

            public TestableSealed()
            {
                _sealedList = new TriggerBase.SealedList<T>();
            }

            public T this[int index]
            {
                get => _sealedList[index];
                set => _sealedList[index] = value;
            }

            public int Count => _sealedList.Count;

            public bool IsReadOnly
            {
                get => _sealedList.IsReadOnly;
                set => _sealedList.IsReadOnly = value;
            }

            public void Add(T item) => _sealedList.Add(item);
            public void Clear() => _sealedList.Clear();
            public bool Contains(T item) => _sealedList.Contains(item);
            public void CopyTo(T[] array, int arrayIndex) => _sealedList.CopyTo(array, arrayIndex);
            public IEnumerator<T> GetEnumerator() => _sealedList.GetEnumerator();
            public int IndexOf(T item) => _sealedList.IndexOf(item);
            public void Insert(int index, T item) => _sealedList.Insert(index, item);
            public bool Remove(T item) => _sealedList.Remove(item);
            public void RemoveAt(int index) => _sealedList.RemoveAt(index);
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_sealedList).GetEnumerator();
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when the item exists in the list.
        /// Input: List containing multiple items, searching for an existing item.
        /// Expected: Returns the zero-based index of the first occurrence of the item.
        /// </summary>
        [Theory]
        [InlineData("first", 0)]
        [InlineData("second", 1)]
        [InlineData("third", 2)]
        public void IndexOf_ItemExists_ReturnsCorrectIndex(string searchItem, int expectedIndex)
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("first");
            sealedList.Add("second");
            sealedList.Add("third");

            // Act
            int actualIndex = sealedList.IndexOf(searchItem);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when the item does not exist in the list.
        /// Input: List containing items, searching for a non-existent item.
        /// Expected: Returns -1 to indicate the item was not found.
        /// </summary>
        [Fact]
        public void IndexOf_ItemDoesNotExist_ReturnsMinusOne()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("existing");
            sealedList.Add("items");

            // Act
            int index = sealedList.IndexOf("nonexistent");

            // Assert
            Assert.Equal(-1, index);
        }

        /// <summary>
        /// Tests that IndexOf handles null items appropriately for reference types.
        /// Input: List with null and non-null items, searching for null.
        /// Expected: Returns the correct index of the null item or -1 if not found.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsCorrectIndex()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("item1");
            sealedList.Add(null);
            sealedList.Add("item3");

            // Act
            int index = sealedList.IndexOf(null);

            // Assert
            Assert.Equal(1, index);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when searching for null in a list that doesn't contain null.
        /// Input: List without null items, searching for null.
        /// Expected: Returns -1 to indicate null was not found.
        /// </summary>
        [Fact]
        public void IndexOf_NullItemNotInList_ReturnsMinusOne()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("item1");
            sealedList.Add("item2");

            // Act
            int index = sealedList.IndexOf(null);

            // Assert
            Assert.Equal(-1, index);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when called on an empty list.
        /// Input: Empty list, searching for any item.
        /// Expected: Returns -1 since no items exist in the list.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyList_ReturnsMinusOne()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();

            // Act
            int index = sealedList.IndexOf("anyitem");

            // Assert
            Assert.Equal(-1, index);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when multiple instances exist.
        /// Input: List with duplicate items, searching for the duplicated item.
        /// Expected: Returns the index of the first occurrence of the item.
        /// </summary>
        [Fact]
        public void IndexOf_MultipleOccurrences_ReturnsFirstIndex()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("unique");
            sealedList.Add("duplicate");
            sealedList.Add("another");
            sealedList.Add("duplicate");
            sealedList.Add("final");

            // Act
            int index = sealedList.IndexOf("duplicate");

            // Assert
            Assert.Equal(1, index);
        }

        /// <summary>
        /// Tests IndexOf with integer value types to ensure it works with different generic type parameters.
        /// Input: List of integers, searching for existing and non-existing values.
        /// Expected: Returns correct index for existing values and -1 for non-existing values.
        /// </summary>
        [Theory]
        [InlineData(10, 0)]
        [InlineData(20, 1)]
        [InlineData(30, 2)]
        [InlineData(99, -1)]
        public void IndexOf_IntegerValues_ReturnsCorrectResult(int searchValue, int expectedIndex)
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<int>();
            sealedList.Add(10);
            sealedList.Add(20);
            sealedList.Add(30);

            // Act
            int actualIndex = sealedList.IndexOf(searchValue);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }
    }

    public class SealedListTests
    {
        /// <summary>
        /// Tests that Add method successfully adds an item when the list is not read-only.
        /// Verifies that the item is added to the internal collection.
        /// </summary>
        [Fact]
        public void Add_WhenNotReadOnly_AddsItemSuccessfully()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            var item = "test item";

            // Act
            sealedList.Add(item);

            // Assert
            Assert.Equal(1, sealedList.Count);
            Assert.True(sealedList.Contains(item));
        }

        /// <summary>
        /// Tests that Add method throws InvalidOperationException when the list is read-only.
        /// This test covers the uncovered line 91: if (IsReadOnly).
        /// </summary>
        [Fact]
        public void Add_WhenReadOnly_ThrowsInvalidOperationException()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.IsReadOnly = true;
            var item = "test item";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Add(item));
            Assert.Equal("This list is ReadOnly", exception.Message);
        }

        /// <summary>
        /// Tests that Add method successfully adds a null item when the list is not read-only.
        /// Tests edge case with null values.
        /// </summary>
        [Fact]
        public void Add_WithNullItem_WhenNotReadOnly_AddsNullSuccessfully()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            string nullItem = null;

            // Act
            sealedList.Add(nullItem);

            // Assert
            Assert.Equal(1, sealedList.Count);
            Assert.True(sealedList.Contains(nullItem));
        }

        /// <summary>
        /// Tests that Add method throws InvalidOperationException when trying to add null to a read-only list.
        /// Verifies that read-only check takes precedence over null checking.
        /// </summary>
        [Fact]
        public void Add_WithNullItem_WhenReadOnly_ThrowsInvalidOperationException()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.IsReadOnly = true;
            string nullItem = null;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Add(nullItem));
            Assert.Equal("This list is ReadOnly", exception.Message);
        }

        /// <summary>
        /// Tests that Add method works correctly with value types.
        /// Verifies generic behavior with different type parameters.
        /// </summary>
        [Fact]
        public void Add_WithValueType_WhenNotReadOnly_AddsItemSuccessfully()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<int>();
            var item = 42;

            // Act
            sealedList.Add(item);

            // Assert
            Assert.Equal(1, sealedList.Count);
            Assert.True(sealedList.Contains(item));
        }

        /// <summary>
        /// Tests that Add method throws InvalidOperationException with value types when read-only.
        /// Ensures consistent behavior across different generic type parameters.
        /// </summary>
        [Fact]
        public void Add_WithValueType_WhenReadOnly_ThrowsInvalidOperationException()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<int>();
            sealedList.IsReadOnly = true;
            var item = 42;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Add(item));
            Assert.Equal("This list is ReadOnly", exception.Message);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in the list.
        /// Tests the scenario where a string item is present in the SealedList.
        /// Expected result: Contains should return true.
        /// </summary>
        [Fact]
        public void Contains_ItemExists_ReturnsTrue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            var testItem = "test item";
            sealedList.Add(testItem);

            // Act
            bool result = sealedList.Contains(testItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in the list.
        /// Tests the scenario where a string item is not present in the SealedList.
        /// Expected result: Contains should return false.
        /// </summary>
        [Fact]
        public void Contains_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("existing item");
            var nonExistentItem = "non-existent item";

            // Act
            bool result = sealedList.Contains(nonExistentItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty list.
        /// Tests the scenario where the SealedList is empty.
        /// Expected result: Contains should return false.
        /// </summary>
        [Fact]
        public void Contains_EmptyList_ReturnsFalse()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            var testItem = "test item";

            // Act
            bool result = sealedList.Contains(testItem);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for null in an empty list.
        /// Tests the scenario where null is searched in an empty SealedList.
        /// Expected result: Contains should return false.
        /// </summary>
        [Fact]
        public void Contains_NullItemInEmptyList_ReturnsFalse()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();

            // Act
            bool result = sealedList.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when searching for null in a list that contains null.
        /// Tests the scenario where null is present in the SealedList.
        /// Expected result: Contains should return true.
        /// </summary>
        [Fact]
        public void Contains_NullItemExists_ReturnsTrue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add(null);
            sealedList.Add("other item");

            // Act
            bool result = sealedList.Contains(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for null in a list that does not contain null.
        /// Tests the scenario where null is not present in the SealedList.
        /// Expected result: Contains should return false.
        /// </summary>
        [Fact]
        public void Contains_NullItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("item1");
            sealedList.Add("item2");

            // Act
            bool result = sealedList.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with value types.
        /// Tests the scenario where an integer item is present in the SealedList.
        /// Expected result: Contains should return true for existing integer and false for non-existing integer.
        /// </summary>
        [Theory]
        [InlineData(42, true)]
        [InlineData(99, false)]
        public void Contains_ValueType_ReturnsCorrectResult(int searchItem, bool expectedResult)
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<int>();
            sealedList.Add(42);
            sealedList.Add(100);

            // Act
            bool result = sealedList.Contains(searchItem);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Contains returns true when duplicate items exist in the list.
        /// Tests the scenario where the same item appears multiple times in the SealedList.
        /// Expected result: Contains should return true.
        /// </summary>
        [Fact]
        public void Contains_DuplicateItems_ReturnsTrue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            var duplicateItem = "duplicate";
            sealedList.Add(duplicateItem);
            sealedList.Add("other item");
            sealedList.Add(duplicateItem);

            // Act
            bool result = sealedList.Contains(duplicateItem);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with custom objects using reference equality.
        /// Tests the scenario where custom object instances are stored in the SealedList.
        /// Expected result: Contains should return true for the same reference and false for different references with same content.
        /// </summary>
        [Fact]
        public void Contains_CustomObjects_UsesReferenceEquality()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<TestObject>();
            var obj1 = new TestObject { Value = "test" };
            var obj2 = new TestObject { Value = "test" }; // Same content, different reference
            sealedList.Add(obj1);

            // Act
            bool resultSameReference = sealedList.Contains(obj1);
            bool resultDifferentReference = sealedList.Contains(obj2);

            // Assert
            Assert.True(resultSameReference);
            Assert.False(resultDifferentReference);
        }

        /// <summary>
        /// Helper class for testing custom object scenarios.
        /// </summary>
        private class TestObject
        {
            public string Value { get; set; }
        }
    }

    public partial class TriggerBaseTests
    {
        /// <summary>
        /// Tests that Clear() successfully clears the list when IsReadOnly is false.
        /// Verifies that the underlying _actual.Clear() method is called and the list becomes empty.
        /// </summary>
        [Fact]
        public void Clear_WhenNotReadOnly_ClearsListSuccessfully()
        {
            // Arrange
            var trigger = new TestTriggerBase(typeof(object));
            var enterActions = (TriggerBase.SealedList<TriggerAction>)trigger.EnterActions;
            var mockAction = Substitute.For<TriggerAction>();
            enterActions.Add(mockAction);

            // Verify list has content before clearing
            Assert.Single(enterActions);

            // Act
            enterActions.Clear();

            // Assert
            Assert.Empty(enterActions);
        }

        /// <summary>
        /// Tests that Clear() throws InvalidOperationException when IsReadOnly is true.
        /// Verifies the correct exception type and message are thrown.
        /// </summary>
        [Fact]
        public void Clear_WhenReadOnly_ThrowsInvalidOperationException()
        {
            // Arrange
            var trigger = new TestTriggerBase(typeof(object));
            var enterActions = (TriggerBase.SealedList<TriggerAction>)trigger.EnterActions;
            var mockAction = Substitute.For<TriggerAction>();
            enterActions.Add(mockAction);
            enterActions.IsReadOnly = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => enterActions.Clear());
            Assert.Equal("This list is ReadOnly", exception.Message);
        }

        private class TestTriggerBase : TriggerBase
        {
            public TestTriggerBase(Type targetType) : base(targetType)
            {
            }
        }

        /// <summary>
        /// Helper class to test TriggerBase functionality since it's abstract
        /// </summary>
        private class TestTrigger : TriggerBase
        {
            public TestTrigger() : base(typeof(BindableObject))
            {
            }
        }

        /// <summary>
        /// Tests that Count returns 0 when EnterActions list is empty
        /// </summary>
        [Fact]
        public void Count_WhenEnterActionsEmpty_ReturnsZero()
        {
            // Arrange
            var trigger = new TestTrigger();

            // Act
            var count = trigger.EnterActions.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns 0 when ExitActions list is empty
        /// </summary>
        [Fact]
        public void Count_WhenExitActionsEmpty_ReturnsZero()
        {
            // Arrange
            var trigger = new TestTrigger();

            // Act
            var count = trigger.ExitActions.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count increases when items are added to EnterActions
        /// </summary>
        [Fact]
        public void Count_AfterAddingToEnterActions_ReturnsCorrectCount()
        {
            // Arrange
            var trigger = new TestTrigger();
            var action1 = Substitute.For<TriggerAction>();
            var action2 = Substitute.For<TriggerAction>();

            // Act & Assert
            trigger.EnterActions.Add(action1);
            Assert.Equal(1, trigger.EnterActions.Count);

            trigger.EnterActions.Add(action2);
            Assert.Equal(2, trigger.EnterActions.Count);
        }

        /// <summary>
        /// Tests that Count increases when items are added to ExitActions
        /// </summary>
        [Fact]
        public void Count_AfterAddingToExitActions_ReturnsCorrectCount()
        {
            // Arrange
            var trigger = new TestTrigger();
            var action1 = Substitute.For<TriggerAction>();
            var action2 = Substitute.For<TriggerAction>();

            // Act & Assert
            trigger.ExitActions.Add(action1);
            Assert.Equal(1, trigger.ExitActions.Count);

            trigger.ExitActions.Add(action2);
            Assert.Equal(2, trigger.ExitActions.Count);
        }

        /// <summary>
        /// Tests that Count decreases when items are removed from EnterActions
        /// </summary>
        [Fact]
        public void Count_AfterRemovingFromEnterActions_ReturnsCorrectCount()
        {
            // Arrange
            var trigger = new TestTrigger();
            var action1 = Substitute.For<TriggerAction>();
            var action2 = Substitute.For<TriggerAction>();
            trigger.EnterActions.Add(action1);
            trigger.EnterActions.Add(action2);

            // Act & Assert
            trigger.EnterActions.Remove(action1);
            Assert.Equal(1, trigger.EnterActions.Count);

            trigger.EnterActions.Remove(action2);
            Assert.Equal(0, trigger.EnterActions.Count);
        }

        /// <summary>
        /// Tests that Count decreases when items are removed from ExitActions
        /// </summary>
        [Fact]
        public void Count_AfterRemovingFromExitActions_ReturnsCorrectCount()
        {
            // Arrange
            var trigger = new TestTrigger();
            var action1 = Substitute.For<TriggerAction>();
            var action2 = Substitute.For<TriggerAction>();
            trigger.ExitActions.Add(action1);
            trigger.ExitActions.Add(action2);

            // Act & Assert
            trigger.ExitActions.Remove(action1);
            Assert.Equal(1, trigger.ExitActions.Count);

            trigger.ExitActions.Remove(action2);
            Assert.Equal(0, trigger.ExitActions.Count);
        }

        /// <summary>
        /// Tests that Count returns 0 after clearing EnterActions
        /// </summary>
        [Fact]
        public void Count_AfterClearingEnterActions_ReturnsZero()
        {
            // Arrange
            var trigger = new TestTrigger();
            var action1 = Substitute.For<TriggerAction>();
            var action2 = Substitute.For<TriggerAction>();
            trigger.EnterActions.Add(action1);
            trigger.EnterActions.Add(action2);

            // Act
            trigger.EnterActions.Clear();

            // Assert
            Assert.Equal(0, trigger.EnterActions.Count);
        }

        /// <summary>
        /// Tests that Count returns 0 after clearing ExitActions
        /// </summary>
        [Fact]
        public void Count_AfterClearingExitActions_ReturnsZero()
        {
            // Arrange
            var trigger = new TestTrigger();
            var action1 = Substitute.For<TriggerAction>();
            var action2 = Substitute.For<TriggerAction>();
            trigger.ExitActions.Add(action1);
            trigger.ExitActions.Add(action2);

            // Act
            trigger.ExitActions.Clear();

            // Assert
            Assert.Equal(0, trigger.ExitActions.Count);
        }

        /// <summary>
        /// Tests that Count works correctly with multiple operations on EnterActions
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(10, 10)]
        public void Count_WithMultipleOperationsOnEnterActions_ReturnsCorrectCount(int itemsToAdd, int expectedCount)
        {
            // Arrange
            var trigger = new TestTrigger();

            // Act
            for (int i = 0; i < itemsToAdd; i++)
            {
                var action = Substitute.For<TriggerAction>();
                trigger.EnterActions.Add(action);
            }

            // Assert
            Assert.Equal(expectedCount, trigger.EnterActions.Count);
        }

        /// <summary>
        /// Tests that Count works correctly with multiple operations on ExitActions
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(10, 10)]
        public void Count_WithMultipleOperationsOnExitActions_ReturnsCorrectCount(int itemsToAdd, int expectedCount)
        {
            // Arrange
            var trigger = new TestTrigger();

            // Act
            for (int i = 0; i < itemsToAdd; i++)
            {
                var action = Substitute.For<TriggerAction>();
                trigger.ExitActions.Add(action);
            }

            // Assert
            Assert.Equal(expectedCount, trigger.ExitActions.Count);
        }

        /// <summary>
        /// Tests that Remove throws InvalidOperationException when the SealedList is read-only.
        /// Verifies the exception message matches the expected text.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WhenReadOnly_ThrowsInvalidOperationException()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("test item");
            sealedList.IsReadOnly = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Remove("test item"));
            Assert.Equal("This list is ReadOnly", exception.Message);
        }

        /// <summary>
        /// Tests that Remove returns true when successfully removing an existing item from a non-read-only SealedList.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WhenNotReadOnlyAndItemExists_ReturnsTrue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("existing item");

            // Act
            bool result = sealedList.Remove("existing item");

            // Assert
            Assert.True(result);
            Assert.DoesNotContain("existing item", sealedList);
        }

        /// <summary>
        /// Tests that Remove returns false when trying to remove a non-existing item from a non-read-only SealedList.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WhenNotReadOnlyAndItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("existing item");

            // Act
            bool result = sealedList.Remove("non-existing item");

            // Assert
            Assert.False(result);
            Assert.Contains("existing item", sealedList);
        }

        /// <summary>
        /// Tests that Remove handles null items appropriately when the list is not read-only.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WhenNotReadOnlyAndItemIsNull_ReturnsFalse()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add("existing item");

            // Act
            bool result = sealedList.Remove(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Remove can successfully remove null values that were previously added to the list.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WhenNotReadOnlyAndNullItemExists_ReturnsTrue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.Add(null);
            sealedList.Add("other item");

            // Act
            bool result = sealedList.Remove(null);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(null, sealedList);
            Assert.Contains("other item", sealedList);
        }

        /// <summary>
        /// Tests that Remove works correctly with value types like integers.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WithValueType_WhenNotReadOnlyAndItemExists_ReturnsTrue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<int>();
            sealedList.Add(42);
            sealedList.Add(100);

            // Act
            bool result = sealedList.Remove(42);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(42, sealedList);
            Assert.Contains(100, sealedList);
        }

        /// <summary>
        /// Tests that Remove throws InvalidOperationException when the SealedList is read-only, even with null items.
        /// </summary>
        [Fact]
        public void SealedList_Remove_WhenReadOnlyWithNullItem_ThrowsInvalidOperationException()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.IsReadOnly = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Remove(null));
            Assert.Equal("This list is ReadOnly", exception.Message);
        }

        /// <summary>
        /// Tests the IsReadOnly property getter returns the correct value.
        /// </summary>
        [Fact]
        public void IsReadOnly_Get_ReturnsCorrectValue()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();

            // Act
            bool result = sealedList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests setting IsReadOnly to true when currently false succeeds.
        /// </summary>
        [Fact]
        public void IsReadOnly_SetToTrueWhenFalse_Succeeds()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();

            // Act
            sealedList.IsReadOnly = true;

            // Assert
            Assert.True(sealedList.IsReadOnly);
        }

        /// <summary>
        /// Tests setting IsReadOnly to false when currently true throws InvalidOperationException.
        /// </summary>
        [Fact]
        public void IsReadOnly_SetToFalseWhenTrue_ThrowsInvalidOperationException()
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            sealedList.IsReadOnly = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sealedList.IsReadOnly = false);
            Assert.Equal("Can't change this back to non readonly", exception.Message);
        }

        /// <summary>
        /// Tests setting IsReadOnly to the same value does nothing and doesn't throw.
        /// Covers the condition where _isReadOnly == value.
        /// </summary>
        /// <param name="initialValue">The initial value to set</param>
        /// <param name="newValue">The new value to set (same as initial)</param>
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void IsReadOnly_SetToSameValue_DoesNothing(bool initialValue, bool newValue)
        {
            // Arrange
            var sealedList = new TriggerBase.SealedList<string>();
            if (initialValue)
            {
                sealedList.IsReadOnly = true;
            }

            // Act
            sealedList.IsReadOnly = newValue;

            // Assert
            Assert.Equal(initialValue, sealedList.IsReadOnly);
        }

        public partial class SealedListTests
        {
            /// <summary>
            /// Tests that Insert throws InvalidOperationException with correct message when the list is read-only.
            /// </summary>
            [Fact]
            public void Insert_WhenIsReadOnlyTrue_ThrowsInvalidOperationException()
            {
                // Arrange
                var sealedList = new TriggerBase.SealedList<string>();
                sealedList.IsReadOnly = true;
                var item = "test";
                var index = 0;

                // Act & Assert
                var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Insert(index, item));
                Assert.Equal("This list is ReadOnly", exception.Message);
            }

            /// <summary>
            /// Tests that Insert successfully adds item at specified index when the list is not read-only.
            /// </summary>
            [Fact]
            public void Insert_WhenIsReadOnlyFalse_InsertsItemAtSpecifiedIndex()
            {
                // Arrange
                var sealedList = new TriggerBase.SealedList<string>();
                sealedList.Add("existing");
                var item = "inserted";
                var index = 0;

                // Act
                sealedList.Insert(index, item);

                // Assert
                Assert.Equal(2, sealedList.Count);
                Assert.Equal("inserted", sealedList[0]);
                Assert.Equal("existing", sealedList[1]);
            }

            /// <summary>
            /// Tests that Insert works with null values when the list is not read-only.
            /// </summary>
            [Fact]
            public void Insert_WithNullItem_WhenIsReadOnlyFalse_InsertsNull()
            {
                // Arrange
                var sealedList = new TriggerBase.SealedList<string>();
                string item = null;
                var index = 0;

                // Act
                sealedList.Insert(index, item);

                // Assert
                Assert.Equal(1, sealedList.Count);
                Assert.Null(sealedList[0]);
            }

            /// <summary>
            /// Tests that Insert works at different valid indices when the list is not read-only.
            /// </summary>
            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(2)]
            public void Insert_WithValidIndex_WhenIsReadOnlyFalse_InsertsAtCorrectPosition(int index)
            {
                // Arrange
                var sealedList = new TriggerBase.SealedList<int>();
                sealedList.Add(10);
                sealedList.Add(30);
                var item = 20;

                // Act
                sealedList.Insert(index, item);

                // Assert
                Assert.Equal(3, sealedList.Count);
                Assert.Equal(20, sealedList[index]);
            }

            /// <summary>
            /// Tests that Insert throws InvalidOperationException even with valid parameters when read-only.
            /// </summary>
            [Theory]
            [InlineData(0, "first")]
            [InlineData(1, "second")]
            public void Insert_WithValidParameters_WhenIsReadOnlyTrue_ThrowsInvalidOperationException(int index, string item)
            {
                // Arrange
                var sealedList = new TriggerBase.SealedList<string>();
                sealedList.Add("existing");
                sealedList.IsReadOnly = true;

                // Act & Assert
                var exception = Assert.Throws<InvalidOperationException>(() => sealedList.Insert(index, item));
                Assert.Equal("This list is ReadOnly", exception.Message);
            }
        }

        /// <summary>
        /// Tests that IsSealed property returns false initially when a trigger is created.
        /// Verifies the initial state of a newly created trigger.
        /// Expected result: IsSealed should return false.
        /// </summary>
        [Fact]
        public void IsSealed_InitialValue_ReturnsFalse()
        {
            // Arrange
            var targetType = typeof(BindableObject);
            var trigger = new TestTriggerBase(targetType);

            // Act
            var result = trigger.IsSealed;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsSealed property returns true after AttachTo is called.
        /// Verifies that the trigger becomes sealed when attached to a bindable object.
        /// Expected result: IsSealed should return true after attachment.
        /// </summary>
        [Fact]
        public void IsSealed_AfterAttachTo_ReturnsTrue()
        {
            // Arrange
            var targetType = typeof(BindableObject);
            var trigger = new TestTriggerBase(targetType);
            var bindableObject = Substitute.For<BindableObject>();

            // Act
            ((IAttachedObject)trigger).AttachTo(bindableObject);

            // Assert
            Assert.True(trigger.IsSealed);
        }

        /// <summary>
        /// Tests that calling AttachTo multiple times on the same trigger maintains sealed state.
        /// Verifies that re-sealing an already sealed trigger doesn't cause issues.
        /// Expected result: IsSealed should remain true and no exception should be thrown.
        /// </summary>
        [Fact]
        public void IsSealed_AttachToCalledTwice_RemainsSealedWithoutException()
        {
            // Arrange
            var targetType = typeof(BindableObject);
            var trigger = new TestTriggerBase(targetType);
            var bindableObject = Substitute.For<BindableObject>();

            // Act
            ((IAttachedObject)trigger).AttachTo(bindableObject);
            ((IAttachedObject)trigger).AttachTo(bindableObject);

            // Assert
            Assert.True(trigger.IsSealed);
        }

        /// <summary>
        /// Tests the unsealing protection mechanism of the IsSealed property.
        /// This test cannot be automatically implemented because the IsSealed setter is private
        /// and reflection access to private members is prohibited by the test guidelines.
        /// 
        /// To test the uncovered line 42 (if (!value)) in the IsSealed setter, which throws
        /// InvalidOperationException when attempting to set IsSealed to false, the following
        /// approaches could be used:
        /// 
        /// 1. Add an internal method to TriggerBase that allows setting IsSealed to false for testing
        /// 2. Use reflection to access the private setter (not allowed per guidelines)
        /// 3. Modify the access level of the setter to internal for testing purposes
        /// 
        /// Expected behavior: Setting IsSealed to false should throw InvalidOperationException
        /// with message "What is sealed cannot be unsealed."
        /// </summary>
        [Fact(Skip = "Cannot test private setter without reflection - see comments for details")]
        public void IsSealed_SetToFalse_ThrowsInvalidOperationException()
        {
            // This test is skipped because the IsSealed setter is private and cannot be accessed
            // without reflection, which is prohibited by the testing guidelines.

            // Expected test implementation would be:
            // 1. Create trigger and seal it by calling AttachTo
            // 2. Attempt to set IsSealed = false using private setter access
            // 3. Verify InvalidOperationException is thrown with correct message

            Assert.True(true, "Test skipped due to private member access limitation");
        }

    }
}