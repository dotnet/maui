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
    public class ReadOnlyCastingListTests
    {
        /// <summary>
        /// Tests that the constructor accepts a valid IList and stores it correctly.
        /// Verifies the list is properly initialized by checking the Count property.
        /// </summary>
        [Fact]
        public void Constructor_WithValidList_StoresListCorrectly()
        {
            // Arrange
            var sourceList = new List<string> { "item1", "item2", "item3" };

            // Act
            var castingList = new ReadOnlyCastingList<object, string>(sourceList);

            // Assert
            Assert.Equal(3, castingList.Count);
            Assert.Equal("item1", castingList[0]);
            Assert.Equal("item2", castingList[1]);
            Assert.Equal("item3", castingList[2]);
        }

        /// <summary>
        /// Tests that the constructor accepts an empty list and handles it correctly.
        /// Verifies that an empty list results in a Count of zero.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyList_StoresListCorrectly()
        {
            // Arrange
            var sourceList = new List<string>();

            // Act
            var castingList = new ReadOnlyCastingList<object, string>(sourceList);

            // Assert
            Assert.Equal(0, castingList.Count);
        }

        /// <summary>
        /// Tests that the constructor accepts a null parameter without throwing an exception.
        /// Note: While accepted by constructor, null will cause NullReferenceException when accessing members.
        /// </summary>
        [Fact]
        public void Constructor_WithNull_AcceptsNullParameter()
        {
            // Arrange & Act
            var castingList = new ReadOnlyCastingList<object, string>(null);

            // Assert - Constructor should complete without exception
            Assert.NotNull(castingList);
            // Note: Accessing Count or indexer will throw NullReferenceException
        }

        /// <summary>
        /// Tests that the constructor works with array implementations of IList.
        /// Verifies that arrays are handled correctly as they implement IList interface.
        /// </summary>
        [Fact]
        public void Constructor_WithArray_StoresArrayCorrectly()
        {
            // Arrange
            string[] sourceArray = new string[] { "first", "second" };

            // Act
            var castingList = new ReadOnlyCastingList<object, string>(sourceArray);

            // Assert
            Assert.Equal(2, castingList.Count);
            Assert.Equal("first", castingList[0]);
            Assert.Equal("second", castingList[1]);
        }

        /// <summary>
        /// Tests that the constructor works with different generic type combinations.
        /// Verifies casting behavior from string to object works correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithStringToObjectCasting_WorksCorrectly()
        {
            // Arrange
            var sourceList = new List<string> { "test" };

            // Act
            var castingList = new ReadOnlyCastingList<object, string>(sourceList);

            // Assert
            Assert.Equal(1, castingList.Count);
            Assert.IsType<string>(castingList[0]);
            Assert.Equal("test", castingList[0]);
        }

        /// <summary>
        /// Tests constructor with a list implementation that has different characteristics.
        /// Verifies that various IList implementations work correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithCustomListImplementation_WorksCorrectly()
        {
            // Arrange
            var sourceList = new LinkedList<string>();
            sourceList.AddLast("node1");
            sourceList.AddLast("node2");
            var listWrapper = new List<string>(sourceList); // Convert to IList

            // Act
            var castingList = new ReadOnlyCastingList<object, string>(listWrapper);

            // Assert
            Assert.Equal(2, castingList.Count);
            Assert.Equal("node1", castingList[0]);
            Assert.Equal("node2", castingList[1]);
        }
    }

    public partial class ReadOnlyCastingReadOnlyListTests
    {
        /// <summary>
        /// Tests that GetEnumerator returns a non-null enumerator when called with a valid readonly list.
        /// Verifies the basic functionality of the GetEnumerator method.
        /// Expected result: A non-null IEnumerator<T> instance is returned.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithValidList_ReturnsNonNullEnumerator()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockReadOnlyList.GetEnumerator().Returns(mockEnumerator);

            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<object, object>(mockReadOnlyList);

            // Act
            var result = readOnlyCastingList.GetEnumerator();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GetEnumerator returns a CastingEnumerator instance when called.
        /// Verifies that the correct enumerator type is created and returned.
        /// Expected result: The returned enumerator is of type CastingEnumerator.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithValidList_ReturnsCastingEnumerator()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockReadOnlyList.GetEnumerator().Returns(mockEnumerator);

            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<object, object>(mockReadOnlyList);

            // Act
            var result = readOnlyCastingList.GetEnumerator();

            // Assert
            Assert.IsType<CastingEnumerator<object, object>>(result);
        }

        /// <summary>
        /// Tests that GetEnumerator calls the underlying readonly list's GetEnumerator method.
        /// Verifies that the method properly delegates to the wrapped readonly list.
        /// Expected result: GetEnumerator is called exactly once on the underlying list.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithValidList_CallsUnderlyingListGetEnumerator()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockReadOnlyList.GetEnumerator().Returns(mockEnumerator);

            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<object, object>(mockReadOnlyList);

            // Act
            var result = readOnlyCastingList.GetEnumerator();

            // Assert
            mockReadOnlyList.Received(1).GetEnumerator();
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with different generic type parameters.
        /// Verifies type casting functionality between TFrom and T generic parameters.
        /// Expected result: A CastingEnumerator with correct generic type parameters is returned.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithDifferentGenericTypes_ReturnsCastingEnumeratorWithCorrectTypes()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockReadOnlyList.GetEnumerator().Returns(mockEnumerator);

            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<string, object>(mockReadOnlyList);

            // Act
            var result = readOnlyCastingList.GetEnumerator();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CastingEnumerator<string, object>>(result);
        }

        /// <summary>
        /// Tests that GetEnumerator returns an enumerator that implements IEnumerator<T>.
        /// Verifies that the returned enumerator conforms to the expected interface.
        /// Expected result: The returned object is assignable to IEnumerator<T>.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithValidList_ReturnsIEnumeratorOfT()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockReadOnlyList.GetEnumerator().Returns(mockEnumerator);

            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<object, object>(mockReadOnlyList);

            // Act
            var result = readOnlyCastingList.GetEnumerator();

            // Assert
            Assert.IsAssignableFrom<IEnumerator<object>>(result);
        }

        /// <summary>
        /// Tests that Count property returns the correct count from the underlying readonly list when the list contains multiple elements.
        /// Input: Mock readonly list with specific count values (0, 1, 5, 100).
        /// Expected: Count property returns the same value as the underlying list's Count property.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void Count_WithVariousCountValues_ReturnsUnderlyingListCount(int expectedCount)
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            mockReadOnlyList.Count.Returns(expectedCount);
            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<string, object>(mockReadOnlyList);

            // Act
            var actualCount = readOnlyCastingList.Count;

            // Assert
            Assert.Equal(expectedCount, actualCount);
        }

        /// <summary>
        /// Tests that Count property throws NullReferenceException when the underlying readonly list is null.
        /// Input: Null readonly list passed to constructor.
        /// Expected: NullReferenceException when accessing Count property.
        /// </summary>
        [Fact]
        public void Count_WithNullUnderlyingList_ThrowsNullReferenceException()
        {
            // Arrange
            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<string, object>(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => readOnlyCastingList.Count);
        }

        /// <summary>
        /// Tests that Count property returns zero when the underlying readonly list is empty.
        /// Input: Mock readonly list with Count returning 0.
        /// Expected: Count property returns 0.
        /// </summary>
        [Fact]
        public void Count_WithEmptyUnderlyingList_ReturnsZero()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            mockReadOnlyList.Count.Returns(0);
            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<string, object>(mockReadOnlyList);

            // Act
            var actualCount = readOnlyCastingList.Count;

            // Assert
            Assert.Equal(0, actualCount);
        }

        /// <summary>
        /// Tests that Count property delegates to the underlying readonly list's Count property exactly once per access.
        /// Input: Mock readonly list with Count property.
        /// Expected: Underlying list's Count property is called exactly once when accessing the wrapper's Count.
        /// </summary>
        [Fact]
        public void Count_AccessedOnce_CallsUnderlyingListCountOnce()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            mockReadOnlyList.Count.Returns(42);
            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<string, object>(mockReadOnlyList);

            // Act
            var count = readOnlyCastingList.Count;

            // Assert
            Assert.Equal(42, count);
            var received = mockReadOnlyList.Received(1).Count;
        }

        /// <summary>
        /// Tests that multiple accesses to Count property call the underlying list's Count property each time.
        /// Input: Mock readonly list with Count property accessed multiple times.
        /// Expected: Underlying list's Count property is called for each access to the wrapper's Count.
        /// </summary>
        [Fact]
        public void Count_AccessedMultipleTimes_CallsUnderlyingListCountEachTime()
        {
            // Arrange
            var mockReadOnlyList = Substitute.For<IReadOnlyList<object>>();
            mockReadOnlyList.Count.Returns(10);
            var readOnlyCastingList = new ReadOnlyCastingReadOnlyList<string, object>(mockReadOnlyList);

            // Act
            var count1 = readOnlyCastingList.Count;
            var count2 = readOnlyCastingList.Count;
            var count3 = readOnlyCastingList.Count;

            // Assert
            Assert.Equal(10, count1);
            Assert.Equal(10, count2);
            Assert.Equal(10, count3);
            var received = mockReadOnlyList.Received(3).Count;
        }

        /// <summary>
        /// Tests that the constructor properly stores a valid IReadOnlyList reference by verifying 
        /// that the Count property delegates to the stored list.
        /// </summary>
        [Fact]
        public void Constructor_WithValidList_StoresListReference()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Count.Returns(5);

            // Act
            var castingList = new ReadOnlyCastingReadOnlyList<object, object>(mockList);

            // Assert
            Assert.Equal(5, castingList.Count);
        }

        /// <summary>
        /// Tests that the constructor accepts a null list parameter and stores the null reference,
        /// which will cause subsequent property access to throw NullReferenceException.
        /// </summary>
        [Fact]
        public void Constructor_WithNullList_StoresNullReference()
        {
            // Arrange
            IReadOnlyList<object> nullList = null;

            // Act
            var castingList = new ReadOnlyCastingReadOnlyList<object, object>(nullList);

            // Assert
            Assert.Throws<NullReferenceException>(() => castingList.Count);
        }

        /// <summary>
        /// Tests that the constructor properly stores an empty list reference by verifying
        /// that the Count property returns zero.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyList_StoresEmptyListReference()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Count.Returns(0);

            // Act
            var castingList = new ReadOnlyCastingReadOnlyList<object, object>(mockList);

            // Assert
            Assert.Equal(0, castingList.Count);
        }

        /// <summary>
        /// Tests that the constructor works with different generic type parameters where TFrom can be cast to T,
        /// using string as TFrom and object as T.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentGenericTypes_StoresListReference()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<string>>();
            mockList.Count.Returns(3);

            // Act
            var castingList = new ReadOnlyCastingReadOnlyList<object, string>(mockList);

            // Assert
            Assert.Equal(3, castingList.Count);
        }

        /// <summary>
        /// Tests that the constructor properly handles a list with maximum integer count value
        /// to verify it works with boundary values.
        /// </summary>
        [Fact]
        public void Constructor_WithMaxCountList_StoresListReference()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Count.Returns(int.MaxValue);

            // Act
            var castingList = new ReadOnlyCastingReadOnlyList<object, object>(mockList);

            // Assert
            Assert.Equal(int.MaxValue, castingList.Count);
        }
    }

    public class CastingListTests
    {
        /// <summary>
        /// Tests that the CastingList constructor successfully accepts a valid IList parameter
        /// and creates an instance without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_ValidList_CreatesInstance()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();

            // Act & Assert - Constructor should not throw
            var result = new CastingList<object, string>(mockList);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the CastingList constructor accepts a null list parameter
        /// without throwing exceptions, as there is no null validation in the constructor.
        /// </summary>
        [Fact]
        public void Constructor_NullList_CreatesInstance()
        {
            // Arrange
            IList<string> nullList = null;

            // Act & Assert - Constructor should not throw even with null
            var result = new CastingList<object, string>(nullList);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the CastingList constructor works with an empty list,
        /// verifying the Count property returns 0 as expected.
        /// </summary>
        [Fact]
        public void Constructor_EmptyList_CreatesInstanceWithZeroCount()
        {
            // Arrange
            var emptyList = Substitute.For<IList<string>>();
            emptyList.Count.Returns(0);

            // Act
            var result = new CastingList<object, string>(emptyList);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        /// <summary>
        /// Tests that the CastingList constructor works with a populated list,
        /// verifying the Count property reflects the underlying list count.
        /// </summary>
        [Fact]
        public void Constructor_PopulatedList_CreatesInstanceWithCorrectCount()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.Count.Returns(3);

            // Act
            var result = new CastingList<object, string>(mockList);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        /// <summary>
        /// Tests that the CastingList constructor works with different reference types
        /// for T and TFrom parameters, ensuring type constraints are satisfied.
        /// </summary>
        [Fact]
        public void Constructor_DifferentReferenceTypes_CreatesInstance()
        {
            // Arrange
            var mockList = Substitute.For<IList<object>>();

            // Act & Assert - Should work with different reference types
            var result = new CastingList<string, object>(mockList);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the CastingList constructor works with the same type
        /// for both T and TFrom parameters.
        /// </summary>
        [Fact]
        public void Constructor_SameTypeParameters_CreatesInstance()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();

            // Act & Assert - Should work with same types
            var result = new CastingList<string, string>(mockList);
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the CastingList constructor properly maintains the IsReadOnly property
        /// from the underlying list.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_ListWithReadOnlyProperty_MaintainsReadOnlyState(bool isReadOnly)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.IsReadOnly.Returns(isReadOnly);

            // Act
            var result = new CastingList<object, string>(mockList);

            // Assert
            Assert.Equal(isReadOnly, result.IsReadOnly);
        }

        /// <summary>
        /// Tests that the IsReadOnly property correctly returns the underlying list's IsReadOnly value.
        /// </summary>
        /// <param name="underlyingIsReadOnly">The IsReadOnly value of the underlying list</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsReadOnly_ReturnsUnderlyingListIsReadOnlyValue(bool underlyingIsReadOnly)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.IsReadOnly.Returns(underlyingIsReadOnly);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            var result = castingList.IsReadOnly;

            // Assert
            Assert.Equal(underlyingIsReadOnly, result);
        }

        /// <summary>
        /// Tests that IsReadOnly returns true when using a concrete read-only list implementation.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithReadOnlyArrayList_ReturnsTrue()
        {
            // Arrange
            var readOnlyList = Array.AsReadOnly(new[] { "test" });
            var castingList = new CastingList<object, string>(readOnlyList);

            // Act
            var result = castingList.IsReadOnly;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsReadOnly returns false when using a concrete mutable list implementation.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithMutableList_ReturnsFalse()
        {
            // Arrange
            var mutableList = new List<string> { "test" };
            var castingList = new CastingList<object, string>(mutableList);

            // Act
            var result = castingList.IsReadOnly;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Clear method delegates to the underlying list's Clear method and results in an empty list.
        /// Input: CastingList with multiple items.
        /// Expected: Underlying list's Clear method is called and Count becomes 0.
        /// </summary>
        [Fact]
        public void Clear_WithMultipleItems_ClearsUnderlyingListAndBecomesEmpty()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.Count.Returns(3, 0); // Initially 3 items, then 0 after clear
            var castingList = new CastingList<object, string>(mockList);

            // Act
            castingList.Clear();

            // Assert
            mockList.Received(1).Clear();
            Assert.Equal(0, castingList.Count);
        }

        /// <summary>
        /// Tests that Clear method works correctly when called on an already empty list.
        /// Input: CastingList with no items.
        /// Expected: Underlying list's Clear method is called.
        /// </summary>
        [Fact]
        public void Clear_WithEmptyList_DelegatesToUnderlyingClear()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.Count.Returns(0);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            castingList.Clear();

            // Assert
            mockList.Received(1).Clear();
            Assert.Equal(0, castingList.Count);
        }

        /// <summary>
        /// Tests that Clear method delegates to the underlying list's Clear method when list has single item.
        /// Input: CastingList with one item.
        /// Expected: Underlying list's Clear method is called and Count becomes 0.
        /// </summary>
        [Fact]
        public void Clear_WithSingleItem_ClearsUnderlyingListAndBecomesEmpty()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.Count.Returns(1, 0); // Initially 1 item, then 0 after clear
            var castingList = new CastingList<object, string>(mockList);

            // Act
            castingList.Clear();

            // Assert
            mockList.Received(1).Clear();
            Assert.Equal(0, castingList.Count);
        }

        private class TestBaseType
        {
            public string Name { get; set; }
        }

        private class TestDerivedType : TestBaseType
        {
            public int Value { get; set; }
        }

        private class TestUnrelatedType
        {
            public bool Flag { get; set; }
        }

        /// <summary>
        /// Tests Contains method when item is null.
        /// Should call underlying list's Contains method with null.
        /// Expected to return the result from the underlying list.
        /// </summary>
        [Fact]
        public void Contains_ItemIsNull_CallsUnderlyingListContainsWithNull()
        {
            // Arrange
            var mockList = Substitute.For<IList<TestBaseType>>();
            mockList.Contains(null).Returns(false);
            var castingList = new CastingList<TestBaseType, TestBaseType>(mockList);

            // Act
            var result = castingList.Contains(null);

            // Assert
            Assert.False(result);
            mockList.Received(1).Contains(null);
        }

        /// <summary>
        /// Tests Contains method when item exists in underlying list.
        /// Should cast item to TFrom type and call underlying list's Contains method.
        /// Expected to return true when underlying list contains the cast item.
        /// </summary>
        [Fact]
        public void Contains_ItemExistsInList_ReturnsTrue()
        {
            // Arrange
            var testItem = new TestBaseType { Name = "Test" };
            var mockList = Substitute.For<IList<TestBaseType>>();
            mockList.Contains(testItem).Returns(true);
            var castingList = new CastingList<TestBaseType, TestBaseType>(mockList);

            // Act
            var result = castingList.Contains(testItem);

            // Assert
            Assert.True(result);
            mockList.Received(1).Contains(testItem);
        }

        /// <summary>
        /// Tests Contains method when item does not exist in underlying list.
        /// Should cast item to TFrom type and call underlying list's Contains method.
        /// Expected to return false when underlying list does not contain the cast item.
        /// </summary>
        [Fact]
        public void Contains_ItemDoesNotExistInList_ReturnsFalse()
        {
            // Arrange
            var testItem = new TestBaseType { Name = "Test" };
            var mockList = Substitute.For<IList<TestBaseType>>();
            mockList.Contains(testItem).Returns(false);
            var castingList = new CastingList<TestBaseType, TestBaseType>(mockList);

            // Act
            var result = castingList.Contains(testItem);

            // Assert
            Assert.False(result);
            mockList.Received(1).Contains(testItem);
        }

        /// <summary>
        /// Tests Contains method when item can be cast from T to TFrom (inheritance).
        /// Should cast derived type to base type and call underlying list's Contains method.
        /// Expected to return the result from underlying list when cast succeeds.
        /// </summary>
        [Fact]
        public void Contains_ItemCanBeCastToTFrom_CallsUnderlyingListWithCastItem()
        {
            // Arrange
            var derivedItem = new TestDerivedType { Name = "Test", Value = 42 };
            var mockList = Substitute.For<IList<TestBaseType>>();
            mockList.Contains(derivedItem).Returns(true);
            var castingList = new CastingList<TestDerivedType, TestBaseType>(mockList);

            // Act
            var result = castingList.Contains(derivedItem);

            // Assert
            Assert.True(result);
            mockList.Received(1).Contains(derivedItem);
        }

        /// <summary>
        /// Tests Contains method when item cannot be cast from T to TFrom (unrelated types).
        /// Should attempt cast which results in null, then call underlying list's Contains with null.
        /// Expected to return the result from underlying list's Contains(null) call.
        /// </summary>
        [Fact]
        public void Contains_ItemCannotBeCastToTFrom_CallsUnderlyingListWithNull()
        {
            // Arrange
            var unrelatedItem = new TestUnrelatedType { Flag = true };
            var mockList = Substitute.For<IList<TestBaseType>>();
            mockList.Contains(null).Returns(false);
            var castingList = new CastingList<TestUnrelatedType, TestBaseType>(mockList);

            // Act
            var result = castingList.Contains(unrelatedItem);

            // Assert
            Assert.False(result);
            mockList.Received(1).Contains(null);
        }

        /// <summary>
        /// Tests Contains method when cast fails but underlying list contains null.
        /// Should attempt cast which results in null, then call underlying list's Contains with null.
        /// Expected to return true when underlying list contains null.
        /// </summary>
        [Fact]
        public void Contains_CastFailsButListContainsNull_ReturnsTrue()
        {
            // Arrange
            var unrelatedItem = new TestUnrelatedType { Flag = true };
            var mockList = Substitute.For<IList<TestBaseType>>();
            mockList.Contains(null).Returns(true);
            var castingList = new CastingList<TestUnrelatedType, TestBaseType>(mockList);

            // Act
            var result = castingList.Contains(unrelatedItem);

            // Assert
            Assert.True(result);
            mockList.Received(1).Contains(null);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// Input: null array, any arrayIndex.
        /// Expected: ArgumentNullException thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var mockList = Substitute.For<IList<object>>();
            var castingList = new CastingList<string, object>(mockList);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => castingList.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input: valid array, negative arrayIndex.
        /// Expected: ArgumentOutOfRangeException thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mockList = Substitute.For<IList<object>>();
            var castingList = new CastingList<string, object>(mockList);
            var array = new string[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => castingList.CopyTo(array, -1));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// Input: array with length 3, arrayIndex = 3.
        /// Expected: ArgumentException thrown.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexEqualToArrayLength_ThrowsArgumentException()
        {
            // Arrange
            var mockList = Substitute.For<IList<object>>();
            var castingList = new CastingList<string, object>(mockList);
            var array = new string[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => castingList.CopyTo(array, 3));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than array length.
        /// Input: array with length 3, arrayIndex = 5.
        /// Expected: ArgumentException thrown.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexGreaterThanArrayLength_ThrowsArgumentException()
        {
            // Arrange
            var mockList = Substitute.For<IList<object>>();
            var castingList = new CastingList<string, object>(mockList);
            var array = new string[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => castingList.CopyTo(array, 5));
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements from source list to destination array starting at arrayIndex 0.
        /// Input: source list with 3 elements, array with length 5, arrayIndex = 0.
        /// Expected: Elements copied with proper casting from TFrom to T.
        /// </summary>
        [Fact]
        public void CopyTo_ValidInputs_CopiesElementsStartingAtZero()
        {
            // Arrange
            var sourceItem1 = "item1";
            var sourceItem2 = "item2";
            var sourceItem3 = "item3";

            var mockList = Substitute.For<IList<object>>();
            mockList[0].Returns(sourceItem1);
            mockList[1].Returns(sourceItem2);
            mockList[2].Returns(sourceItem3);

            var castingList = new CastingList<string, object>(mockList);
            var array = new string[5];

            // Act
            castingList.CopyTo(array, 0);

            // Assert
            Assert.Equal(sourceItem1, array[0]);
            Assert.Equal(sourceItem2, array[1]);
            Assert.Equal(sourceItem3, array[2]);
            Assert.Null(array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies elements starting at a non-zero arrayIndex.
        /// Input: source list with 2 elements, array with length 5, arrayIndex = 2.
        /// Expected: Elements copied starting at index 2 in destination array.
        /// </summary>
        [Fact]
        public void CopyTo_ValidInputsWithOffset_CopiesElementsWithOffset()
        {
            // Arrange
            var sourceItem1 = "item1";
            var sourceItem2 = "item2";

            var mockList = Substitute.For<IList<object>>();
            mockList[2].Returns(sourceItem1);
            mockList[3].Returns(sourceItem2);

            var castingList = new CastingList<string, object>(mockList);
            var array = new string[5];

            // Act
            castingList.CopyTo(array, 2);

            // Assert
            Assert.Null(array[0]);
            Assert.Null(array[1]);
            Assert.Equal(sourceItem1, array[2]);
            Assert.Equal(sourceItem2, array[3]);
            Assert.Null(array[4]);
        }

        /// <summary>
        /// Tests that CopyTo handles casting when source elements cannot be cast to target type.
        /// Input: source list with non-string objects, attempting to cast to string.
        /// Expected: null values in destination array for failed casts.
        /// </summary>
        [Fact]
        public void CopyTo_CastingFailure_ResultsInNullValues()
        {
            // Arrange
            var sourceItem1 = new object();
            var sourceItem2 = 123;

            var mockList = Substitute.For<IList<object>>();
            mockList[0].Returns(sourceItem1);
            mockList[1].Returns(sourceItem2);

            var castingList = new CastingList<string, object>(mockList);
            var array = new string[3];

            // Act
            castingList.CopyTo(array, 0);

            // Assert
            Assert.Null(array[0]); // object cannot be cast to string
            Assert.Null(array[1]); // int cannot be cast to string
            Assert.Null(array[2]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly with empty destination array.
        /// Input: empty array, arrayIndex = 0.
        /// Expected: No exception thrown, no copying performed.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyArray_NoExceptionThrown()
        {
            // Arrange
            var mockList = Substitute.For<IList<object>>();
            var castingList = new CastingList<string, object>(mockList);
            var array = new string[0];

            // Act & Assert
            // Should not throw exception for empty array with arrayIndex 0
            castingList.CopyTo(array, 0);
        }

        /// <summary>
        /// Tests that CopyTo handles maximum boundary values correctly.
        /// Input: array with maximum practical size, arrayIndex at boundary.
        /// Expected: Proper handling of boundary conditions.
        /// </summary>
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        public void CopyTo_BoundaryValues_HandledCorrectly(int arrayIndex, int arrayLength)
        {
            // Arrange
            var sourceItem = "test";
            var mockList = Substitute.For<IList<object>>();
            mockList[arrayIndex].Returns(sourceItem);

            var castingList = new CastingList<string, object>(mockList);
            var array = new string[arrayLength];

            // Act
            castingList.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(sourceItem, array[arrayIndex]);
        }

        /// <summary>
        /// Tests IndexOf method when the item parameter is null.
        /// Should cast null to TFrom and call IndexOf on underlying list with null.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_CallsUnderlyingListIndexOfWithNull()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.IndexOf(null).Returns(2);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            int result = castingList.IndexOf(null);

            // Assert
            Assert.Equal(2, result);
            mockList.Received(1).IndexOf(null);
        }

        /// <summary>
        /// Tests IndexOf method when the item cannot be cast to TFrom type.
        /// Should cast to null using 'as' operator and call IndexOf on underlying list with null.
        /// </summary>
        [Fact]
        public void IndexOf_ItemCannotBeCastToTFrom_CallsUnderlyingListIndexOfWithNull()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.IndexOf(null).Returns(-1);
            var castingList = new CastingList<object, string>(mockList);
            var nonStringItem = new object(); // Cannot be cast to string

            // Act
            int result = castingList.IndexOf(nonStringItem);

            // Assert
            Assert.Equal(-1, result);
            mockList.Received(1).IndexOf(null);
        }

        /// <summary>
        /// Tests IndexOf method when the item can be cast to TFrom and exists in the list.
        /// Should return the correct index from the underlying list.
        /// </summary>
        [Fact]
        public void IndexOf_ItemExistsInList_ReturnsCorrectIndex()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var testString = "test item";
            mockList.IndexOf(testString).Returns(3);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            int result = castingList.IndexOf(testString);

            // Assert
            Assert.Equal(3, result);
            mockList.Received(1).IndexOf(testString);
        }

        /// <summary>
        /// Tests IndexOf method when the item can be cast to TFrom but does not exist in the list.
        /// Should return -1 from the underlying list.
        /// </summary>
        [Fact]
        public void IndexOf_ItemDoesNotExistInList_ReturnsMinusOne()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var testString = "non-existent item";
            mockList.IndexOf(testString).Returns(-1);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            int result = castingList.IndexOf(testString);

            // Assert
            Assert.Equal(-1, result);
            mockList.Received(1).IndexOf(testString);
        }

        /// <summary>
        /// Tests IndexOf method when the item exists multiple times in the list.
        /// Should return the index of the first occurrence from the underlying list.
        /// </summary>
        [Fact]
        public void IndexOf_ItemExistsMultipleTimes_ReturnsFirstOccurrenceIndex()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var testString = "duplicate item";
            mockList.IndexOf(testString).Returns(1); // First occurrence at index 1
            var castingList = new CastingList<object, string>(mockList);

            // Act
            int result = castingList.IndexOf(testString);

            // Assert
            Assert.Equal(1, result);
            mockList.Received(1).IndexOf(testString);
        }

        /// <summary>
        /// Tests IndexOf method with empty underlying list.
        /// Should return -1 as the item cannot be found in an empty list.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyList_ReturnsMinusOne()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var testString = "any item";
            mockList.IndexOf(testString).Returns(-1);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            int result = castingList.IndexOf(testString);

            // Assert
            Assert.Equal(-1, result);
            mockList.Received(1).IndexOf(testString);
        }

        /// <summary>
        /// Tests IndexOf method when item at index 0 exists.
        /// Should return 0 to test boundary condition.
        /// </summary>
        [Fact]
        public void IndexOf_ItemAtIndexZero_ReturnsZero()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var testString = "first item";
            mockList.IndexOf(testString).Returns(0);
            var castingList = new CastingList<object, string>(mockList);

            // Act
            int result = castingList.IndexOf(testString);

            // Assert
            Assert.Equal(0, result);
            mockList.Received(1).IndexOf(testString);
        }

        /// <summary>
        /// Tests that RemoveAt method calls the underlying list's RemoveAt with the correct index
        /// when provided with a valid index value.
        /// Expected result: The underlying list's RemoveAt method is called with the same index.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void RemoveAt_ValidIndex_CallsUnderlyingListRemoveAt(int index)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var castingList = new CastingList<object, string>(mockList);

            // Act
            castingList.RemoveAt(index);

            // Assert
            mockList.Received(1).RemoveAt(index);
        }

        /// <summary>
        /// Tests that RemoveAt method propagates ArgumentOutOfRangeException from the underlying list
        /// when provided with a negative index.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsArgumentOutOfRangeException(int negativeIndex)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.When(x => x.RemoveAt(negativeIndex)).Do(x => throw new ArgumentOutOfRangeException());
            var castingList = new CastingList<object, string>(mockList);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => castingList.RemoveAt(negativeIndex));
        }

        /// <summary>
        /// Tests that RemoveAt method propagates ArgumentOutOfRangeException from the underlying list
        /// when provided with an index greater than or equal to the list count.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(0)] // Empty list
        [InlineData(1)] // Single item list, index out of bounds
        [InlineData(100)] // Large index
        [InlineData(int.MaxValue)]
        public void RemoveAt_IndexOutOfBounds_ThrowsArgumentOutOfRangeException(int outOfBoundsIndex)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.When(x => x.RemoveAt(outOfBoundsIndex)).Do(x => throw new ArgumentOutOfRangeException());
            var castingList = new CastingList<object, string>(mockList);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => castingList.RemoveAt(outOfBoundsIndex));
        }

        /// <summary>
        /// Tests that RemoveAt method propagates any other exception from the underlying list
        /// when the underlying implementation throws an unexpected exception.
        /// Expected result: The original exception is propagated.
        /// </summary>
        [Fact]
        public void RemoveAt_UnderlyingListThrowsException_PropagatesException()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var expectedException = new InvalidOperationException("Test exception");
            mockList.When(x => x.RemoveAt(Arg.Any<int>())).Do(x => throw expectedException);
            var castingList = new CastingList<object, string>(mockList);

            // Act & Assert
            var thrownException = Assert.Throws<InvalidOperationException>(() => castingList.RemoveAt(0));
            Assert.Same(expectedException, thrownException);
        }

        /// <summary>
        /// Tests that RemoveAt method handles boundary index values correctly
        /// when working with a list that has multiple elements.
        /// Expected result: The underlying list's RemoveAt method is called with the correct boundary indices.
        /// </summary>
        [Theory]
        [InlineData(0)] // First element
        [InlineData(4)] // Last element (assuming 5 elements)
        public void RemoveAt_BoundaryIndices_CallsUnderlyingListRemoveAt(int boundaryIndex)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.Count.Returns(5); // Simulate a list with 5 elements
            var castingList = new CastingList<object, string>(mockList);

            // Act
            castingList.RemoveAt(boundaryIndex);

            // Assert
            mockList.Received(1).RemoveAt(boundaryIndex);
        }
    }
}