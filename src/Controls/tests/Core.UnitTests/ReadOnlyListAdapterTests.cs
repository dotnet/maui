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
    public class ReadOnlyListAdapterTests
    {
        /// <summary>
        /// Tests that the Add method throws NotImplementedException when called with null value on adapter created with IReadOnlyList constructor.
        /// </summary>
        [Fact]
        public void Add_WithNullValueOnListConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Add(null));
        }

        /// <summary>
        /// Tests that the Add method throws NotImplementedException when called with null value on adapter created with IReadOnlyCollection constructor.
        /// </summary>
        [Fact]
        public void Add_WithNullValueOnCollectionConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Add(null));
        }

        /// <summary>
        /// Tests that the Add method throws NotImplementedException for various input values on adapter created with IReadOnlyList constructor.
        /// Input conditions tested: valid object, string, integer, boolean values.
        /// Expected result: NotImplementedException thrown in all cases.
        /// </summary>
        [Theory]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void Add_WithVariousValuesOnListConstructor_ThrowsNotImplementedException(object value)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Add(value));
        }

        /// <summary>
        /// Tests that the Add method throws NotImplementedException for various input values on adapter created with IReadOnlyCollection constructor.
        /// Input conditions tested: valid object, string, integer, boolean values.
        /// Expected result: NotImplementedException thrown in all cases.
        /// </summary>
        [Theory]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void Add_WithVariousValuesOnCollectionConstructor_ThrowsNotImplementedException(object value)
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Add(value));
        }

        /// <summary>
        /// Tests that the Add method throws NotImplementedException when called with a complex object on adapter created with IReadOnlyList constructor.
        /// Input conditions tested: new object instance.
        /// Expected result: NotImplementedException thrown.
        /// </summary>
        [Fact]
        public void Add_WithComplexObjectOnListConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);
            var complexObject = new { Name = "Test", Value = 123 };

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Add(complexObject));
        }

        /// <summary>
        /// Tests that the Add method throws NotImplementedException when called with a complex object on adapter created with IReadOnlyCollection constructor.
        /// Input conditions tested: new object instance.
        /// Expected result: NotImplementedException thrown.
        /// </summary>
        [Fact]
        public void Add_WithComplexObjectOnCollectionConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);
            var complexObject = new { Name = "Test", Value = 123 };

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Add(complexObject));
        }

        /// <summary>
        /// Tests that accessing the SyncRoot property throws NotImplementedException
        /// when the adapter is created with an IReadOnlyList.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void SyncRoot_WhenCreatedWithIReadOnlyList_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.SyncRoot);
        }

        /// <summary>
        /// Tests that accessing the SyncRoot property throws NotImplementedException
        /// when the adapter is created with an IReadOnlyCollection.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void SyncRoot_WhenCreatedWithIReadOnlyCollection_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.SyncRoot);
        }

        /// <summary>
        /// Tests that GetEnumerator returns the enumerator from the underlying collection 
        /// when adapter is created with IReadOnlyList constructor and collection is empty.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithListConstructorEmptyCollection_ReturnsCollectionEnumerator()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockList.GetEnumerator().Returns(mockEnumerator);
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var result = adapter.GetEnumerator();

            // Assert
            Assert.Same(mockEnumerator, result);
            mockList.Received(1).GetEnumerator();
        }

        /// <summary>
        /// Tests that GetEnumerator returns the enumerator from the underlying collection 
        /// when adapter is created with IReadOnlyCollection constructor.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithCollectionConstructor_ReturnsCollectionEnumerator()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockCollection.GetEnumerator().Returns(mockEnumerator);
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act
            var result = adapter.GetEnumerator();

            // Assert
            Assert.Same(mockEnumerator, result);
            mockCollection.Received(1).GetEnumerator();
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with a collection containing multiple elements
        /// and the returned enumerator can iterate through all elements.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithMultipleElements_AllowsIterationThroughAllElements()
        {
            // Arrange
            var elements = new List<object> { "first", 42, null, new object() };
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.GetEnumerator().Returns(elements.GetEnumerator());
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var enumerator = adapter.GetEnumerator();
            var iteratedElements = new List<object>();
            while (enumerator.MoveNext())
            {
                iteratedElements.Add(enumerator.Current);
            }

            // Assert
            Assert.Equal(elements, iteratedElements);
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with an empty collection
        /// and the returned enumerator reflects the empty state.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithEmptyCollection_ReturnsEmptyEnumerator()
        {
            // Arrange
            var emptyList = new List<object>();
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.GetEnumerator().Returns(emptyList.GetEnumerator());
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var enumerator = adapter.GetEnumerator();
            var hasElements = enumerator.MoveNext();

            // Assert
            Assert.False(hasElements);
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with a collection containing only null elements
        /// and properly handles null values during iteration.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithNullElements_HandlesNullElementsCorrectly()
        {
            // Arrange
            var elementsWithNulls = new List<object> { null, null, "not null", null };
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.GetEnumerator().Returns(elementsWithNulls.GetEnumerator());
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var enumerator = adapter.GetEnumerator();
            var iteratedElements = new List<object>();
            while (enumerator.MoveNext())
            {
                iteratedElements.Add(enumerator.Current);
            }

            // Assert
            Assert.Equal(elementsWithNulls, iteratedElements);
            Assert.Equal(4, iteratedElements.Count);
            Assert.Null(iteratedElements[0]);
            Assert.Null(iteratedElements[1]);
            Assert.Equal("not null", iteratedElements[2]);
            Assert.Null(iteratedElements[3]);
        }

        /// <summary>
        /// Tests that GetEnumerator works correctly with a single element collection
        /// and the returned enumerator iterates through the single element.
        /// </summary>
        [Fact]
        public void GetEnumerator_WithSingleElement_IteratesSingleElement()
        {
            // Arrange
            var singleElementList = new List<object> { "single element" };
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            mockCollection.GetEnumerator().Returns(singleElementList.GetEnumerator());
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act
            var enumerator = adapter.GetEnumerator();
            var hasFirst = enumerator.MoveNext();
            var firstElement = enumerator.Current;
            var hasSecond = enumerator.MoveNext();

            // Assert
            Assert.True(hasFirst);
            Assert.Equal("single element", firstElement);
            Assert.False(hasSecond);
        }

        /// <summary>
        /// Tests that Contains returns true when the value exists in the list and the adapter was created with IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with IReadOnlyList containing the search value.
        /// Expected: Returns true.
        /// </summary>
        [Fact]
        public void Contains_ValueExistsInList_ReturnsTrue()
        {
            // Arrange
            var testValue = new object();
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Contains(testValue).Returns(true);
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var result = adapter.Contains(testValue);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the value does not exist in the list and the adapter was created with IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with IReadOnlyList not containing the search value.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void Contains_ValueDoesNotExistInList_ReturnsFalse()
        {
            // Arrange
            var testValue = new object();
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Contains(testValue).Returns(false);
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var result = adapter.Contains(testValue);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains handles null values correctly when the adapter was created with IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with IReadOnlyList, searching for null value.
        /// Expected: Delegates to underlying list's Contains method and returns the result.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Contains_NullValue_ReturnsExpectedResult(bool listContainsNull)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Contains(null).Returns(listContainsNull);
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var result = adapter.Contains(null);

            // Assert
            Assert.Equal(listContainsNull, result);
        }

        /// <summary>
        /// Tests that Contains throws NullReferenceException when the adapter was created with IReadOnlyCollection instead of IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with IReadOnlyCollection (making _list field null), any search value.
        /// Expected: Throws NullReferenceException because _list is null.
        /// </summary>
        [Fact]
        public void Contains_AdapterCreatedWithCollection_ThrowsNullReferenceException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);
            var testValue = new object();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => adapter.Contains(testValue));
        }

        /// <summary>
        /// Tests that Contains works correctly with various object types when the adapter was created with IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with IReadOnlyList, searching for different types of objects.
        /// Expected: Delegates to underlying list's Contains method and returns the result.
        /// </summary>
        [Theory]
        [InlineData("test string", true)]
        [InlineData("test string", false)]
        [InlineData(42, true)]
        [InlineData(42, false)]
        public void Contains_VariousObjectTypes_ReturnsExpectedResult(object searchValue, bool expectedResult)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Contains(searchValue).Returns(expectedResult);
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            var result = adapter.Contains(searchValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Contains with null value throws NullReferenceException when adapter was created with IReadOnlyCollection.
        /// Input: ReadOnlyListAdapter created with IReadOnlyCollection, searching for null.
        /// Expected: Throws NullReferenceException because _list is null.
        /// </summary>
        [Fact]
        public void Contains_AdapterCreatedWithCollectionSearchingNull_ThrowsNullReferenceException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => adapter.Contains(null));
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns true when the adapter is constructed with an IReadOnlyList.
        /// This ensures the property correctly indicates the read-only nature of the adapter.
        /// Expected result: IsReadOnly should return true.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithReadOnlyListConstructor_ReturnsTrue()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act
            bool result = adapter.IsReadOnly;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property returns true when the adapter is constructed with an IReadOnlyCollection.
        /// This ensures the property correctly indicates the read-only nature of the adapter regardless of constructor used.
        /// Expected result: IsReadOnly should return true.
        /// </summary>
        [Fact]
        public void IsReadOnly_WithReadOnlyCollectionConstructor_ReturnsTrue()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act
            bool result = adapter.IsReadOnly;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the Clear method always throws NotImplementedException regardless of the adapter state.
        /// This verifies that the read-only adapter correctly prevents modification operations.
        /// </summary>
        [Fact]
        public void Clear_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Clear());
        }

        /// <summary>
        /// Tests that the Clear method throws NotImplementedException when constructed with IReadOnlyCollection.
        /// This ensures consistent behavior regardless of which constructor was used.
        /// </summary>
        [Fact]
        public void Clear_WithCollectionConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Clear());
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException when called with null value on adapter created with IReadOnlyList.
        /// Input: null value parameter.
        /// Expected: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void Remove_WithNullValueOnListAdapter_ThrowsNotImplementedException()
        {
            // Arrange
            var list = new List<object> { "item1", "item2" };
            var adapter = new ReadOnlyListAdapter(list);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(null));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException when called with null value on adapter created with IReadOnlyCollection.
        /// Input: null value parameter.
        /// Expected: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void Remove_WithNullValueOnCollectionAdapter_ThrowsNotImplementedException()
        {
            // Arrange
            var collection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(collection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(null));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException for various object types on list adapter.
        /// Input: different object types including strings, integers, and custom objects.
        /// Expected: NotImplementedException is thrown for all cases.
        /// </summary>
        [Theory]
        [InlineData("test")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Remove_WithVariousValuesOnListAdapter_ThrowsNotImplementedException(object value)
        {
            // Arrange
            var list = new List<object> { "item1", 42, "item2" };
            var adapter = new ReadOnlyListAdapter(list);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(value));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException for various object types on collection adapter.
        /// Input: different object types including strings, integers, and boundary values.
        /// Expected: NotImplementedException is thrown for all cases.
        /// </summary>
        [Theory]
        [InlineData("test")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Remove_WithVariousValuesOnCollectionAdapter_ThrowsNotImplementedException(object value)
        {
            // Arrange
            var collection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(collection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(value));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException with special string values on list adapter.
        /// Input: empty string, whitespace string, and very long string.
        /// Expected: NotImplementedException is thrown for all cases.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("very long string that exceeds normal length boundaries to test edge cases")]
        public void Remove_WithSpecialStringValuesOnListAdapter_ThrowsNotImplementedException(string value)
        {
            // Arrange
            var list = new List<object> { "item1", "item2" };
            var adapter = new ReadOnlyListAdapter(list);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(value));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException with floating-point edge values on list adapter.
        /// Input: NaN, positive infinity, negative infinity, and other floating-point boundary values.
        /// Expected: NotImplementedException is thrown for all cases.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        public void Remove_WithFloatingPointValuesOnListAdapter_ThrowsNotImplementedException(double value)
        {
            // Arrange
            var list = new List<object> { 1.0, 2.0, 3.0 };
            var adapter = new ReadOnlyListAdapter(list);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(value));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException when called with existing items from the collection.
        /// Input: objects that actually exist in the underlying collection.
        /// Expected: NotImplementedException is thrown regardless of item existence.
        /// </summary>
        [Fact]
        public void Remove_WithExistingItemOnListAdapter_ThrowsNotImplementedException()
        {
            // Arrange
            var existingItem = "existing item";
            var list = new List<object> { existingItem, "other item" };
            var adapter = new ReadOnlyListAdapter(list);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(existingItem));
        }

        /// <summary>
        /// Tests that Remove method throws NotImplementedException when called with non-existing items.
        /// Input: objects that do not exist in the underlying collection.
        /// Expected: NotImplementedException is thrown regardless of item existence.
        /// </summary>
        [Fact]
        public void Remove_WithNonExistingItemOnListAdapter_ThrowsNotImplementedException()
        {
            // Arrange
            var nonExistingItem = "non existing item";
            var list = new List<object> { "item1", "item2" };
            var adapter = new ReadOnlyListAdapter(list);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.Remove(nonExistingItem));
        }

        /// <summary>
        /// Tests that the IsFixedSize property throws NotImplementedException when accessed
        /// on an adapter created with IReadOnlyList.
        /// Input conditions: ReadOnlyListAdapter created with a valid IReadOnlyList.
        /// Expected result: NotImplementedException is thrown when accessing IsFixedSize.
        /// </summary>
        [Fact]
        public void IsFixedSize_WhenAccessedWithListAdapter_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsFixedSize);
        }

        /// <summary>
        /// Tests that the IsFixedSize property throws NotImplementedException when accessed
        /// on an adapter created with IReadOnlyCollection.
        /// Input conditions: ReadOnlyListAdapter created with a valid IReadOnlyCollection.
        /// Expected result: NotImplementedException is thrown when accessing IsFixedSize.
        /// </summary>
        [Fact]
        public void IsFixedSize_WhenAccessedWithCollectionAdapter_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsFixedSize);
        }

        /// <summary>
        /// Tests that IsSynchronized property throws NotImplementedException when adapter is created with IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with a mocked IReadOnlyList.
        /// Expected: NotImplementedException is thrown when accessing IsSynchronized.
        /// </summary>
        [Fact]
        public void IsSynchronized_WithReadOnlyListConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsSynchronized);
        }

        /// <summary>
        /// Tests that IsSynchronized property throws NotImplementedException when adapter is created with IReadOnlyCollection.
        /// Input: ReadOnlyListAdapter created with a mocked IReadOnlyCollection.
        /// Expected: NotImplementedException is thrown when accessing IsSynchronized.
        /// </summary>
        [Fact]
        public void IsSynchronized_WithReadOnlyCollectionConstructor_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsSynchronized);
        }

        /// <summary>
        /// Tests that IsSynchronized property throws NotImplementedException when adapter is created with null IReadOnlyList.
        /// Input: ReadOnlyListAdapter created with null IReadOnlyList.
        /// Expected: NotImplementedException is thrown when accessing IsSynchronized.
        /// </summary>
        [Fact]
        public void IsSynchronized_WithNullReadOnlyList_ThrowsNotImplementedException()
        {
            // Arrange
            var adapter = new ReadOnlyListAdapter((IReadOnlyList<object>)null);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsSynchronized);
        }

        /// <summary>
        /// Tests that IsSynchronized property throws NotImplementedException when adapter is created with null IReadOnlyCollection.
        /// Input: ReadOnlyListAdapter created with null IReadOnlyCollection.
        /// Expected: NotImplementedException is thrown when accessing IsSynchronized.
        /// </summary>
        [Fact]
        public void IsSynchronized_WithNullReadOnlyCollection_ThrowsNotImplementedException()
        {
            // Arrange
            var adapter = new ReadOnlyListAdapter((IReadOnlyCollection<object>)null);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsSynchronized);
        }

        /// <summary>
        /// Tests that IsSynchronized property throws NotImplementedException when adapter is created with empty list.
        /// Input: ReadOnlyListAdapter created with an empty IReadOnlyList.
        /// Expected: NotImplementedException is thrown when accessing IsSynchronized.
        /// </summary>
        [Fact]
        public void IsSynchronized_WithEmptyReadOnlyList_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Count.Returns(0);
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsSynchronized);
        }

        /// <summary>
        /// Tests that IsSynchronized property throws NotImplementedException when adapter is created with populated list.
        /// Input: ReadOnlyListAdapter created with an IReadOnlyList containing elements.
        /// Expected: NotImplementedException is thrown when accessing IsSynchronized.
        /// </summary>
        [Fact]
        public void IsSynchronized_WithPopulatedReadOnlyList_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            mockList.Count.Returns(3);
            mockList[0].Returns("item1");
            mockList[1].Returns("item2");
            mockList[2].Returns("item3");
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.IsSynchronized);
        }

        /// <summary>
        /// Tests that RemoveAt method throws NotImplementedException for valid positive indices.
        /// Verifies that the method correctly indicates read-only behavior.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void RemoveAt_ValidPositiveIndex_ThrowsNotImplementedException(int index)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt method throws NotImplementedException for negative indices.
        /// Verifies that the method throws the expected exception regardless of invalid input.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ThrowsNotImplementedException(int index)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.RemoveAt(index));
        }

        /// <summary>
        /// Tests that RemoveAt method throws NotImplementedException when adapter is created with IReadOnlyCollection constructor.
        /// Verifies that the exception behavior is consistent across different constructor variants.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(10)]
        public void RemoveAt_WithCollectionConstructor_ThrowsNotImplementedException(int index)
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.RemoveAt(index));
        }

        /// <summary>
        /// Tests that CopyTo throws NotImplementedException when called with valid parameters on an adapter created with IReadOnlyList.
        /// </summary>
        [Fact]
        public void CopyTo_ValidParametersWithReadOnlyList_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);
            var array = new object[5];
            var index = 0;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.CopyTo(array, index));
        }

        /// <summary>
        /// Tests that CopyTo throws NotImplementedException when called with valid parameters on an adapter created with IReadOnlyCollection.
        /// </summary>
        [Fact]
        public void CopyTo_ValidParametersWithReadOnlyCollection_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);
            var array = new object[5];
            var index = 2;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.CopyTo(array, index));
        }

        /// <summary>
        /// Tests that CopyTo throws NotImplementedException when called with null array parameter.
        /// Input: null array, valid index.
        /// Expected: NotImplementedException is thrown regardless of parameter validity.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsNotImplementedException()
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);
            Array array = null;
            var index = 0;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.CopyTo(array, index));
        }

        /// <summary>
        /// Tests that CopyTo throws NotImplementedException when called with boundary index values.
        /// Input: valid array, boundary index values (negative, zero, maximum).
        /// Expected: NotImplementedException is thrown for all boundary conditions.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(100)]
        public void CopyTo_BoundaryIndexValues_ThrowsNotImplementedException(int index)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);
            var array = new object[10];

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.CopyTo(array, index));
        }

        /// <summary>
        /// Tests that CopyTo throws NotImplementedException when called with empty array.
        /// Input: empty array, valid index.
        /// Expected: NotImplementedException is thrown regardless of array size.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyArray_ThrowsNotImplementedException()
        {
            // Arrange
            var mockCollection = Substitute.For<IReadOnlyCollection<object>>();
            var adapter = new ReadOnlyListAdapter(mockCollection);
            var array = new object[0];
            var index = 0;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.CopyTo(array, index));
        }

        /// <summary>
        /// Tests that CopyTo throws NotImplementedException when called with different array types.
        /// Input: various array types (string[], int[], etc.), valid index.
        /// Expected: NotImplementedException is thrown regardless of array element type.
        /// </summary>
        [Theory]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(double[]))]
        public void CopyTo_DifferentArrayTypes_ThrowsNotImplementedException(Type arrayType)
        {
            // Arrange
            var mockList = Substitute.For<IReadOnlyList<object>>();
            var adapter = new ReadOnlyListAdapter(mockList);
            var array = Array.CreateInstance(arrayType.GetElementType(), 5);
            var index = 1;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => adapter.CopyTo(array, index));
        }
    }
}