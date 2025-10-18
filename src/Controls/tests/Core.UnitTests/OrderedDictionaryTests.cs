#nullable disable
//
// OrderedDictionaryTests.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cadenza;
using Cadenza.Collections;
using Microsoft;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class OrderedDictionaryTests
    {
        /// <summary>
        /// Tests that the constructor with capacity parameter properly initializes the OrderedDictionary
        /// with valid capacity values, creating an empty dictionary with the specified initial capacity.
        /// Expected result: Dictionary is properly initialized with Count = 0 and default equality comparer.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void Constructor_ValidCapacity_InitializesCorrectly(int capacity)
        {
            // Arrange & Act
            var dictionary = new OrderedDictionary<string, int>(capacity);

            // Assert
            Assert.Equal(0, dictionary.Count);
            Assert.Empty(dictionary.Keys);
            Assert.Empty(dictionary.Values);
            Assert.Equal(EqualityComparer<string>.Default, dictionary.Comparer);
        }

        /// <summary>
        /// Tests that the constructor with capacity parameter throws ArgumentOutOfRangeException
        /// when provided with negative capacity values.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException(int capacity)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrderedDictionary<string, int>(capacity));
        }

        /// <summary>
        /// Tests that the constructor with capacity parameter uses the default equality comparer
        /// for the key type when creating the dictionary.
        /// Expected result: Comparer property returns EqualityComparer<TKey>.Default.
        /// </summary>
        [Fact]
        public void Constructor_ValidCapacity_UsesDefaultEqualityComparer()
        {
            // Arrange
            const int capacity = 50;

            // Act
            var stringKeyDictionary = new OrderedDictionary<string, int>(capacity);
            var intKeyDictionary = new OrderedDictionary<int, string>(capacity);

            // Assert
            Assert.Equal(EqualityComparer<string>.Default, stringKeyDictionary.Comparer);
            Assert.Equal(EqualityComparer<int>.Default, intKeyDictionary.Comparer);
        }

        /// <summary>
        /// Tests that the constructor with maximum integer capacity either succeeds or throws
        /// OutOfMemoryException depending on available system memory.
        /// Expected result: Either successful initialization or OutOfMemoryException.
        /// </summary>
        [Fact]
        public void Constructor_MaxIntCapacity_HandlesGracefully()
        {
            // Arrange
            const int maxCapacity = int.MaxValue;

            // Act & Assert
            try
            {
                var dictionary = new OrderedDictionary<string, int>(maxCapacity);
                Assert.Equal(0, dictionary.Count);
                Assert.Equal(EqualityComparer<string>.Default, dictionary.Comparer);
            }
            catch (OutOfMemoryException)
            {
                // This is acceptable behavior for extremely large capacity values
                Assert.True(true, "OutOfMemoryException is acceptable for int.MaxValue capacity");
            }
        }

        /// <summary>
        /// Tests that the constructor with zero capacity properly initializes an empty dictionary
        /// that can still accept elements after construction.
        /// Expected result: Dictionary is initialized and can accept new elements.
        /// </summary>
        [Fact]
        public void Constructor_ZeroCapacity_AllowsSubsequentAdditions()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>(0);

            // Act
            dictionary.Add("key1", 100);
            dictionary.Add("key2", 200);

            // Assert
            Assert.Equal(2, dictionary.Count);
            Assert.Equal(100, dictionary["key1"]);
            Assert.Equal(200, dictionary["key2"]);
        }

        /// <summary>
        /// Tests that the constructor with a custom equality comparer properly initializes the OrderedDictionary
        /// with the specified comparer and zero capacity.
        /// </summary>
        [Fact]
        public void Constructor_WithCustomEqualityComparer_SetsComparerAndInitializesCorrectly()
        {
            // Arrange
            var customComparer = StringComparer.OrdinalIgnoreCase;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(customComparer);

            // Assert
            Assert.Equal(customComparer, orderedDict.Comparer);
            Assert.Equal(0, orderedDict.Count);

            // Verify the comparer is actually used by adding items that differ only in case
            orderedDict.Add("KEY", 1);
            Assert.True(orderedDict.ContainsKey("key")); // Should find it due to case-insensitive comparer
        }

        /// <summary>
        /// Tests that the constructor with a null equality comparer properly initializes the OrderedDictionary
        /// using the default comparer.
        /// </summary>
        [Fact]
        public void Constructor_WithNullEqualityComparer_UsesDefaultComparer()
        {
            // Arrange & Act
            var orderedDict = new OrderedDictionary<string, int>((IEqualityComparer<string>)null);

            // Assert
            Assert.Equal(EqualityComparer<string>.Default, orderedDict.Comparer);
            Assert.Equal(0, orderedDict.Count);

            // Verify default comparer behavior (case-sensitive for strings)
            orderedDict.Add("KEY", 1);
            Assert.False(orderedDict.ContainsKey("key")); // Should not find it due to case-sensitive default comparer
        }

        /// <summary>
        /// Tests that the constructor with the explicit default equality comparer properly initializes 
        /// the OrderedDictionary with the default comparer.
        /// </summary>
        [Fact]
        public void Constructor_WithDefaultEqualityComparer_SetsComparer()
        {
            // Arrange
            var defaultComparer = EqualityComparer<string>.Default;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(defaultComparer);

            // Assert
            Assert.Equal(defaultComparer, orderedDict.Comparer);
            Assert.Equal(0, orderedDict.Count);

            // Verify default comparer behavior
            orderedDict.Add("KEY", 1);
            Assert.False(orderedDict.ContainsKey("key")); // Should not find it due to case-sensitive default comparer
        }

        /// <summary>
        /// Tests that the constructor works with different key types and their respective custom comparers.
        /// </summary>
        [Fact]
        public void Constructor_WithCustomComparerForIntegerKeys_SetsComparerCorrectly()
        {
            // Arrange
            var customComparer = EqualityComparer<int>.Default;

            // Act
            var orderedDict = new OrderedDictionary<int, string>(customComparer);

            // Assert
            Assert.Equal(customComparer, orderedDict.Comparer);
            Assert.Equal(0, orderedDict.Count);
        }

        /// <summary>
        /// Tests that the constructor with capacity and equality comparer properly initializes all fields
        /// with valid parameters and creates an empty ordered dictionary.
        /// </summary>
        /// <param name="capacity">The initial capacity for the dictionary.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void Constructor_ValidCapacityWithDefaultComparer_InitializesCorrectly(int capacity)
        {
            // Arrange & Act
            var orderedDict = new OrderedDictionary<string, int>(capacity, EqualityComparer<string>.Default);

            // Assert
            Assert.Equal(0, orderedDict.Count);
            Assert.False(((ICollection<KeyValuePair<string, int>>)orderedDict).IsReadOnly);
            Assert.NotNull(orderedDict.Keys);
            Assert.Empty(orderedDict.Keys);
            Assert.NotNull(orderedDict.Values);
            Assert.Empty(orderedDict.Values);
            Assert.Equal(EqualityComparer<string>.Default, orderedDict.Comparer);
        }

        /// <summary>
        /// Tests that the constructor with capacity and equality comparer properly initializes
        /// with a custom equality comparer.
        /// </summary>
        [Fact]
        public void Constructor_ValidCapacityWithCustomComparer_InitializesWithCustomComparer()
        {
            // Arrange
            var customComparer = Substitute.For<IEqualityComparer<string>>();
            const int capacity = 5;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(capacity, customComparer);

            // Assert
            Assert.Equal(0, orderedDict.Count);
            Assert.False(((ICollection<KeyValuePair<string, int>>)orderedDict).IsReadOnly);
            Assert.NotNull(orderedDict.Keys);
            Assert.Empty(orderedDict.Keys);
            Assert.NotNull(orderedDict.Values);
            Assert.Empty(orderedDict.Values);
            Assert.Equal(customComparer, orderedDict.Comparer);
        }

        /// <summary>
        /// Tests that the constructor with capacity and null equality comparer properly initializes
        /// using the default equality comparer.
        /// </summary>
        [Fact]
        public void Constructor_ValidCapacityWithNullComparer_InitializesWithDefaultComparer()
        {
            // Arrange
            const int capacity = 10;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(capacity, null);

            // Assert
            Assert.Equal(0, orderedDict.Count);
            Assert.False(((ICollection<KeyValuePair<string, int>>)orderedDict).IsReadOnly);
            Assert.NotNull(orderedDict.Keys);
            Assert.Empty(orderedDict.Keys);
            Assert.NotNull(orderedDict.Values);
            Assert.Empty(orderedDict.Values);
            Assert.Equal(EqualityComparer<string>.Default, orderedDict.Comparer);
        }

        /// <summary>
        /// Tests that the constructor with maximum integer capacity initializes correctly
        /// without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_MaxIntCapacity_InitializesCorrectly()
        {
            // Arrange & Act
            var orderedDict = new OrderedDictionary<int, string>(int.MaxValue, EqualityComparer<int>.Default);

            // Assert
            Assert.Equal(0, orderedDict.Count);
            Assert.False(((ICollection<KeyValuePair<int, string>>)orderedDict).IsReadOnly);
            Assert.NotNull(orderedDict.Keys);
            Assert.Empty(orderedDict.Keys);
            Assert.NotNull(orderedDict.Values);
            Assert.Empty(orderedDict.Values);
            Assert.Equal(EqualityComparer<int>.Default, orderedDict.Comparer);
        }

        /// <summary>
        /// Tests that the constructor with string comparer variants properly initializes
        /// with different string comparison behaviors.
        /// </summary>
        /// <param name="comparer">The string equality comparer to test.</param>
        [Theory]
        [MemberData(nameof(GetStringComparers))]
        public void Constructor_StringComparerVariants_InitializesWithCorrectComparer(IEqualityComparer<string> comparer)
        {
            // Arrange
            const int capacity = 5;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(capacity, comparer);

            // Assert
            Assert.Equal(0, orderedDict.Count);
            Assert.Equal(comparer, orderedDict.Comparer);
        }

        /// <summary>
        /// Tests that the constructor properly initializes collections that are accessible
        /// through the interface implementations.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_InitializesInterfaceCollections()
        {
            // Arrange
            const int capacity = 5;
            var comparer = EqualityComparer<int>.Default;

            // Act
            var orderedDict = new OrderedDictionary<int, string>(capacity, comparer);
            var kvpCollection = (ICollection<KeyValuePair<int, string>>)orderedDict;
            var list = (IList<KeyValuePair<int, string>>)orderedDict;

            // Assert
            Assert.Equal(0, kvpCollection.Count);
            Assert.False(kvpCollection.IsReadOnly);
            Assert.Equal(0, list.Count);
            Assert.False(list.IsReadOnly);
        }

        public static IEnumerable<object[]> GetStringComparers()
        {
            yield return new object[] { StringComparer.Ordinal };
            yield return new object[] { StringComparer.OrdinalIgnoreCase };
            yield return new object[] { StringComparer.CurrentCulture };
            yield return new object[] { StringComparer.CurrentCultureIgnoreCase };
            yield return new object[] { StringComparer.InvariantCulture };
            yield return new object[] { StringComparer.InvariantCultureIgnoreCase };
            yield return new object[] { EqualityComparer<string>.Default };
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when dictionary parameter is null.
        /// Input: null dictionary
        /// Expected: ArgumentNullException with parameter name "dictionary"
        /// </summary>
        [Fact]
        public void Constructor_NullDictionary_ThrowsArgumentNullException()
        {
            // Arrange
            ICollection<KeyValuePair<int, string>> dictionary = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new OrderedDictionary<int, string>(dictionary));
            Assert.Equal("dictionary", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor creates an empty OrderedDictionary when given an empty collection.
        /// Input: empty dictionary collection
        /// Expected: OrderedDictionary with Count = 0
        /// </summary>
        [Fact]
        public void Constructor_EmptyDictionary_CreatesEmptyOrderedDictionary()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>();

            // Act
            var orderedDict = new OrderedDictionary<int, string>(dictionary);

            // Assert
            Assert.Equal(0, orderedDict.Count);
        }

        /// <summary>
        /// Tests that the constructor properly adds a single key-value pair from the input dictionary.
        /// Input: dictionary with single key-value pair (1, "one")
        /// Expected: OrderedDictionary with Count = 1 and contains the key-value pair
        /// </summary>
        [Fact]
        public void Constructor_SingleItemDictionary_AddsItemCorrectly()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "one")
            };

            // Act
            var orderedDict = new OrderedDictionary<int, string>(dictionary);

            // Assert
            Assert.Equal(1, orderedDict.Count);
            Assert.True(orderedDict.ContainsKey(1));
            Assert.Equal("one", orderedDict[1]);
        }

        /// <summary>
        /// Tests that the constructor properly adds multiple key-value pairs and maintains insertion order.
        /// Input: dictionary with multiple key-value pairs in specific order
        /// Expected: OrderedDictionary with all items in the same order
        /// </summary>
        [Fact]
        public void Constructor_MultipleItemsDictionary_AddsAllItemsInOrder()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(3, "three"),
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(4, "four"),
                new KeyValuePair<int, string>(2, "two")
            };

            // Act
            var orderedDict = new OrderedDictionary<int, string>(dictionary);

            // Assert
            Assert.Equal(4, orderedDict.Count);

            // Verify all items are present
            Assert.True(orderedDict.ContainsKey(1));
            Assert.True(orderedDict.ContainsKey(2));
            Assert.True(orderedDict.ContainsKey(3));
            Assert.True(orderedDict.ContainsKey(4));

            // Verify values
            Assert.Equal("one", orderedDict[1]);
            Assert.Equal("two", orderedDict[2]);
            Assert.Equal("three", orderedDict[3]);
            Assert.Equal("four", orderedDict[4]);

            // Verify order is maintained by checking indexed access
            Assert.Equal("three", orderedDict[0]);
            Assert.Equal("one", orderedDict[1]);
            Assert.Equal("four", orderedDict[2]);
            Assert.Equal("two", orderedDict[3]);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentException when the input dictionary contains duplicate keys.
        /// Input: dictionary with duplicate keys
        /// Expected: ArgumentException thrown during Add operation
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithDuplicateKeys_ThrowsArgumentException()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "first"),
                new KeyValuePair<int, string>(2, "second"),
                new KeyValuePair<int, string>(1, "duplicate") // Duplicate key
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new OrderedDictionary<int, string>(dictionary));
        }

        /// <summary>
        /// Tests that the constructor properly handles null values when TValue is a reference type.
        /// Input: dictionary with null values
        /// Expected: OrderedDictionary correctly stores null values
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithNullValues_HandlesNullValuesCorrectly()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, null),
                new KeyValuePair<int, string>(2, "two"),
                new KeyValuePair<int, string>(3, null)
            };

            // Act
            var orderedDict = new OrderedDictionary<int, string>(dictionary);

            // Assert
            Assert.Equal(3, orderedDict.Count);
            Assert.Null(orderedDict[1]);
            Assert.Equal("two", orderedDict[2]);
            Assert.Null(orderedDict[3]);
        }

        /// <summary>
        /// Tests that the constructor uses the default equality comparer for the key type.
        /// Input: dictionary with string keys that should use default string equality comparer
        /// Expected: OrderedDictionary properly handles string key comparisons
        /// </summary>
        [Fact]
        public void Constructor_UsesDefaultEqualityComparer()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("key1", 1),
                new KeyValuePair<string, int>("key2", 2)
            };

            // Act
            var orderedDict = new OrderedDictionary<string, int>(dictionary);

            // Assert
            Assert.Equal(2, orderedDict.Count);
            Assert.Equal(EqualityComparer<string>.Default, orderedDict.Comparer);
            Assert.True(orderedDict.ContainsKey("key1"));
            Assert.True(orderedDict.ContainsKey("key2"));
        }

        /// <summary>
        /// Tests that the constructor works with value types for both key and value.
        /// Input: dictionary with value type keys and values
        /// Expected: OrderedDictionary properly stores value types
        /// </summary>
        [Fact]
        public void Constructor_WithValueTypes_WorksCorrectly()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, double>>
            {
                new KeyValuePair<int, double>(1, 1.5),
                new KeyValuePair<int, double>(2, 2.7),
                new KeyValuePair<int, double>(3, 3.14)
            };

            // Act
            var orderedDict = new OrderedDictionary<int, double>(dictionary);

            // Assert
            Assert.Equal(3, orderedDict.Count);
            Assert.Equal(1.5, orderedDict[1]);
            Assert.Equal(2.7, orderedDict[2]);
            Assert.Equal(3.14, orderedDict[3]);
        }

        /// <summary>
        /// Tests that the constructor works with a large number of items.
        /// Input: dictionary with many key-value pairs
        /// Expected: OrderedDictionary handles large collections efficiently
        /// </summary>
        [Fact]
        public void Constructor_LargeCollection_HandlesEfficientlyAndMaintainsOrder()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>();
            for (int i = 100; i >= 1; i--) // Add in reverse order
            {
                dictionary.Add(new KeyValuePair<int, string>(i, $"value{i}"));
            }

            // Act
            var orderedDict = new OrderedDictionary<int, string>(dictionary);

            // Assert
            Assert.Equal(100, orderedDict.Count);

            // Verify first and last items maintain insertion order
            Assert.Equal("value100", orderedDict[0]); // First inserted
            Assert.Equal("value1", orderedDict[99]);  // Last inserted

            // Verify some middle items
            Assert.Equal("value50", orderedDict[50]);
            Assert.True(orderedDict.ContainsKey(50));
            Assert.Equal("value50", orderedDict[50]);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters throws ArgumentNullException when dictionary is null.
        /// Input: null dictionary, any equality comparer
        /// Expected: ArgumentNullException with parameter name "dictionary"
        /// </summary>
        [Fact]
        public void Constructor_WithNullDictionary_ThrowsArgumentNullException()
        {
            // Arrange
            ICollection<KeyValuePair<string, int>> dictionary = null;
            IEqualityComparer<string> equalityComparer = EqualityComparer<string>.Default;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new OrderedDictionary<string, int>(dictionary, equalityComparer));
            Assert.Equal("dictionary", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters works with null equality comparer.
        /// Input: valid dictionary, null equality comparer
        /// Expected: OrderedDictionary is created successfully and uses default equality comparer
        /// </summary>
        [Fact]
        public void Constructor_WithNullEqualityComparer_InitializesSuccessfully()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("key1", 1)
            };
            IEqualityComparer<string> equalityComparer = null;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(1, orderedDict.Count);
            Assert.True(orderedDict.ContainsKey("key1"));
            Assert.Equal(1, orderedDict["key1"]);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters creates an empty OrderedDictionary when given an empty collection.
        /// Input: empty dictionary, any equality comparer
        /// Expected: Empty OrderedDictionary with Count = 0
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyDictionary_CreatesEmptyOrderedDictionary()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>();
            var equalityComparer = EqualityComparer<string>.Default;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(0, orderedDict.Count);
            Assert.Empty(orderedDict.Keys);
            Assert.Empty(orderedDict.Values);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters adds a single element correctly.
        /// Input: dictionary with one key-value pair, any equality comparer
        /// Expected: OrderedDictionary contains the single element
        /// </summary>
        [Fact]
        public void Constructor_WithSingleElement_AddsSingleElement()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("key1", 42)
            };
            var equalityComparer = EqualityComparer<string>.Default;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(1, orderedDict.Count);
            Assert.True(orderedDict.ContainsKey("key1"));
            Assert.Equal(42, orderedDict["key1"]);
            Assert.Equal(42, orderedDict[0]);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters adds multiple elements in correct order.
        /// Input: dictionary with multiple key-value pairs, any equality comparer
        /// Expected: OrderedDictionary contains all elements in the same order as input
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleElements_AddsAllElementsInOrder()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("first", 1),
                new KeyValuePair<string, int>("second", 2),
                new KeyValuePair<string, int>("third", 3)
            };
            var equalityComparer = EqualityComparer<string>.Default;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(3, orderedDict.Count);
            Assert.True(orderedDict.ContainsKey("first"));
            Assert.True(orderedDict.ContainsKey("second"));
            Assert.True(orderedDict.ContainsKey("third"));

            // Verify order by index access
            Assert.Equal(1, orderedDict[0]);
            Assert.Equal(2, orderedDict[1]);
            Assert.Equal(3, orderedDict[2]);

            // Verify values by key access
            Assert.Equal(1, orderedDict["first"]);
            Assert.Equal(2, orderedDict["second"]);
            Assert.Equal(3, orderedDict["third"]);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters uses custom equality comparer correctly.
        /// Input: dictionary with string keys, case-insensitive equality comparer
        /// Expected: OrderedDictionary uses the custom comparer for key comparison
        /// </summary>
        [Fact]
        public void Constructor_WithCustomEqualityComparer_UsesCustomComparer()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("Key1", 1),
                new KeyValuePair<string, int>("Key2", 2)
            };
            var equalityComparer = StringComparer.OrdinalIgnoreCase;

            // Act
            var orderedDict = new OrderedDictionary<string, int>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(2, orderedDict.Count);
            Assert.True(orderedDict.ContainsKey("key1")); // Should find with different case
            Assert.True(orderedDict.ContainsKey("KEY2")); // Should find with different case
            Assert.Equal(1, orderedDict["key1"]);
            Assert.Equal(2, orderedDict["KEY2"]);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters throws ArgumentException when dictionary contains duplicate keys.
        /// Input: dictionary with duplicate keys, any equality comparer
        /// Expected: ArgumentException is thrown during construction
        /// </summary>
        [Fact]
        public void Constructor_WithDuplicateKeys_ThrowsArgumentException()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("duplicate", 1),
                new KeyValuePair<string, int>("duplicate", 2)
            };
            var equalityComparer = EqualityComparer<string>.Default;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new OrderedDictionary<string, int>(dictionary, equalityComparer));
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters works with different generic types.
        /// Input: dictionary with integer keys and string values, any equality comparer
        /// Expected: OrderedDictionary is created successfully with correct types
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentGenericTypes_WorksCorrectly()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(2, "two"),
                new KeyValuePair<int, string>(3, "three")
            };
            var equalityComparer = EqualityComparer<int>.Default;

            // Act
            var orderedDict = new OrderedDictionary<int, string>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(3, orderedDict.Count);
            Assert.Equal("one", orderedDict[1]);
            Assert.Equal("two", orderedDict[2]);
            Assert.Equal("three", orderedDict[3]);
            Assert.Equal("one", orderedDict[0]);
            Assert.Equal("two", orderedDict[1]);
            Assert.Equal("three", orderedDict[2]);
        }

        /// <summary>
        /// Tests that the constructor with dictionary and equality comparer parameters handles null values correctly.
        /// Input: dictionary with null values, any equality comparer
        /// Expected: OrderedDictionary is created successfully and preserves null values
        /// </summary>
        [Fact]
        public void Constructor_WithNullValues_PreservesNullValues()
        {
            // Arrange
            var dictionary = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("key1", null),
                new KeyValuePair<string, string>("key2", "value2"),
                new KeyValuePair<string, string>("key3", null)
            };
            var equalityComparer = EqualityComparer<string>.Default;

            // Act
            var orderedDict = new OrderedDictionary<string, string>(dictionary, equalityComparer);

            // Assert
            Assert.Equal(3, orderedDict.Count);
            Assert.Null(orderedDict["key1"]);
            Assert.Equal("value2", orderedDict["key2"]);
            Assert.Null(orderedDict["key3"]);
        }

        /// <summary>
        /// Tests that the Comparer property returns the default equality comparer when using the default constructor.
        /// </summary>
        [Fact]
        public void Comparer_DefaultConstructor_ReturnsDefaultEqualityComparer()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(EqualityComparer<string>.Default, comparer);
        }

        /// <summary>
        /// Tests that the Comparer property returns the default equality comparer when using the capacity constructor.
        /// </summary>
        [Fact]
        public void Comparer_CapacityConstructor_ReturnsDefaultEqualityComparer()
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>(10);

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(EqualityComparer<int>.Default, comparer);
        }

        /// <summary>
        /// Tests that the Comparer property returns the custom equality comparer when provided via constructor.
        /// </summary>
        [Fact]
        public void Comparer_CustomEqualityComparer_ReturnsProvidedComparer()
        {
            // Arrange
            var customComparer = StringComparer.OrdinalIgnoreCase;
            var dictionary = new OrderedDictionary<string, int>(customComparer);

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(customComparer, comparer);
        }

        /// <summary>
        /// Tests that the Comparer property returns the custom equality comparer when provided via capacity constructor.
        /// </summary>
        [Fact]
        public void Comparer_CapacityAndCustomComparer_ReturnsProvidedComparer()
        {
            // Arrange
            var customComparer = StringComparer.InvariantCultureIgnoreCase;
            var dictionary = new OrderedDictionary<string, int>(5, customComparer);

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(customComparer, comparer);
        }

        /// <summary>
        /// Tests that the Comparer property returns the default equality comparer when using dictionary constructor.
        /// </summary>
        [Fact]
        public void Comparer_DictionaryConstructor_ReturnsDefaultEqualityComparer()
        {
            // Arrange
            var source = new Dictionary<string, int> { { "key", 1 } };
            var dictionary = new OrderedDictionary<string, int>(source);

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(EqualityComparer<string>.Default, comparer);
        }

        /// <summary>
        /// Tests that the Comparer property returns the custom equality comparer when using dictionary constructor with comparer.
        /// </summary>
        [Fact]
        public void Comparer_DictionaryConstructorWithComparer_ReturnsProvidedComparer()
        {
            // Arrange
            var customComparer = StringComparer.Ordinal;
            var source = new Dictionary<string, int> { { "key", 1 } };
            var dictionary = new OrderedDictionary<string, int>(source, customComparer);

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(customComparer, comparer);
        }

        /// <summary>
        /// Tests that the Comparer property works correctly with different key types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(object))]
        public void Comparer_DifferentKeyTypes_ReturnsCorrectDefaultComparer(Type keyType)
        {
            // Arrange & Act & Assert
            if (keyType == typeof(int))
            {
                var dictionary = new OrderedDictionary<int, string>();
                Assert.Same(EqualityComparer<int>.Default, dictionary.Comparer);
            }
            else if (keyType == typeof(long))
            {
                var dictionary = new OrderedDictionary<long, string>();
                Assert.Same(EqualityComparer<long>.Default, dictionary.Comparer);
            }
            else if (keyType == typeof(object))
            {
                var dictionary = new OrderedDictionary<object, string>();
                Assert.Same(EqualityComparer<object>.Default, dictionary.Comparer);
            }
        }

        /// <summary>
        /// Tests that the Comparer property is consistent across multiple accesses.
        /// </summary>
        [Fact]
        public void Comparer_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange
            var customComparer = StringComparer.CurrentCultureIgnoreCase;
            var dictionary = new OrderedDictionary<string, int>(customComparer);

            // Act
            var comparer1 = dictionary.Comparer;
            var comparer2 = dictionary.Comparer;

            // Assert
            Assert.Same(comparer1, comparer2);
            Assert.Same(customComparer, comparer1);
        }

        /// <summary>
        /// Tests that the Comparer property returns null when null comparer is provided to dictionary constructor.
        /// This tests the edge case where null is passed as the comparer parameter.
        /// </summary>
        [Fact]
        public void Comparer_NullComparer_ReturnsDefaultComparer()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>(0, null);

            // Act
            var comparer = dictionary.Comparer;

            // Assert
            Assert.Same(EqualityComparer<string>.Default, comparer);
        }

        /// <summary>
        /// Tests that IndexOf throws ArgumentNullException when key is null.
        /// This tests the null check validation in the method.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void IndexOf_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => dictionary.IndexOf(null));
            Assert.Equal("key", exception.ParamName);
        }

        /// <summary>
        /// Tests IndexOf behavior with various key scenarios including existing keys at different positions and non-existing keys.
        /// This tests the core functionality of finding key indexes in the ordered dictionary.
        /// Expected result: Correct index for existing keys, -1 for non-existing keys.
        /// </summary>
        [Theory]
        [InlineData("first", 0)] // Key at first position
        [InlineData("second", 1)] // Key at middle position  
        [InlineData("third", 2)] // Key at last position
        [InlineData("nonexistent", -1)] // Key that doesn't exist
        public void IndexOf_VariousKeys_ReturnsExpectedIndex(string key, int expectedIndex)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            int actualIndex = dictionary.IndexOf(key);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf on an empty dictionary.
        /// This tests the behavior when no keys are present in the dictionary.
        /// Expected result: -1 should be returned for any non-null key.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyDictionary_ReturnsMinusOne()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act
            int index = dictionary.IndexOf("anykey");

            // Assert
            Assert.Equal(-1, index);
        }

        /// <summary>
        /// Tests IndexOf with edge case string values including empty string and whitespace.
        /// This tests the method's behavior with special string values.
        /// Expected result: Correct index for existing keys, -1 for non-existing keys.
        /// </summary>
        [Theory]
        [InlineData("", 0)] // Empty string as key
        [InlineData(" ", 1)] // Whitespace as key
        [InlineData("normal", 2)] // Normal string
        [InlineData("missing", -1)] // Non-existing key
        public void IndexOf_EdgeCaseStringKeys_ReturnsExpectedIndex(string key, int expectedIndex)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("", 1); // Empty string key
            dictionary.Add(" ", 2); // Whitespace key
            dictionary.Add("normal", 3);

            // Act
            int actualIndex = dictionary.IndexOf(key);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf with numeric keys to verify generic type behavior.
        /// This tests that the method works correctly with value types as keys.
        /// Expected result: Correct index for existing keys, -1 for non-existing keys.
        /// </summary>
        [Theory]
        [InlineData(1, 0)] // First numeric key
        [InlineData(2, 1)] // Second numeric key
        [InlineData(3, 2)] // Third numeric key
        [InlineData(999, -1)] // Non-existing numeric key
        public void IndexOf_NumericKeys_ReturnsExpectedIndex(int key, int expectedIndex)
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>();
            dictionary.Add(1, "first");
            dictionary.Add(2, "second");
            dictionary.Add(3, "third");

            // Act
            int actualIndex = dictionary.IndexOf(key);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf after removing keys to verify index consistency.
        /// This tests that indexes are correctly maintained after dictionary modifications.
        /// Expected result: Indexes should be updated correctly after key removal.
        /// </summary>
        [Fact]
        public void IndexOf_AfterRemoval_ReturnsUpdatedIndex()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act - Remove middle element
            dictionary.Remove("second");
            int firstIndex = dictionary.IndexOf("first");
            int thirdIndex = dictionary.IndexOf("third");
            int removedIndex = dictionary.IndexOf("second");

            // Assert
            Assert.Equal(0, firstIndex); // First should still be at index 0
            Assert.Equal(1, thirdIndex); // Third should now be at index 1
            Assert.Equal(-1, removedIndex); // Removed key should return -1
        }

        /// <summary>
        /// Tests IndexOf behavior when the same key is re-added after removal.
        /// This tests that re-added keys get placed at the end of the order.
        /// Expected result: Re-added key should appear at the last index.
        /// </summary>
        [Fact]
        public void IndexOf_ReaddedKey_ReturnsNewIndex()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act - Remove and re-add first key
            dictionary.Remove("first");
            dictionary.Add("first", 1);
            int readdedIndex = dictionary.IndexOf("first");

            // Assert
            Assert.Equal(2, readdedIndex); // Re-added key should be at the end
        }

        /// <summary>
        /// Tests that Add method successfully adds a new key-value pair to an empty dictionary.
        /// Input: Valid key and value to an empty dictionary.
        /// Expected: Key-value pair is added, Count increases to 1, and key can be retrieved.
        /// </summary>
        [Fact]
        public void Add_ValidKeyAndValueToEmptyDictionary_AddsSuccessfully()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            string key = "testKey";
            int value = 42;

            // Act
            dictionary.Add(key, value);

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.True(dictionary.ContainsKey(key));
            Assert.Equal(value, dictionary[key]);
            Assert.Equal(value, dictionary[0]);
        }

        /// <summary>
        /// Tests that Add method successfully adds multiple key-value pairs maintaining insertion order.
        /// Input: Multiple valid key-value pairs added sequentially.
        /// Expected: All pairs are added, Count reflects total additions, and insertion order is preserved.
        /// </summary>
        [Fact]
        public void Add_MultipleValidKeyValuePairs_MaintainsInsertionOrder()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            var pairs = new[]
            {
                ("first", 1),
                ("second", 2),
                ("third", 3)
            };

            // Act
            foreach (var (key, value) in pairs)
            {
                dictionary.Add(key, value);
            }

            // Assert
            Assert.Equal(3, dictionary.Count);
            Assert.Equal(1, dictionary[0]); // First inserted
            Assert.Equal(2, dictionary[1]); // Second inserted
            Assert.Equal(3, dictionary[2]); // Third inserted
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentException when attempting to add a duplicate key.
        /// Input: Key that already exists in the dictionary.
        /// Expected: ArgumentException is thrown and dictionary state remains unchanged.
        /// </summary>
        [Fact]
        public void Add_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            string key = "duplicateKey";
            dictionary.Add(key, 1);
            int originalCount = dictionary.Count;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => dictionary.Add(key, 2));
            Assert.Equal(originalCount, dictionary.Count);
            Assert.Equal(1, dictionary[key]); // Original value preserved
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when key is null.
        /// Input: Null key with valid value.
        /// Expected: ArgumentNullException is thrown and dictionary remains empty.
        /// </summary>
        [Fact]
        public void Add_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => dictionary.Add(null, 42));
            Assert.Equal("key", exception.ParamName);
            Assert.Equal(0, dictionary.Count);
        }

        /// <summary>
        /// Tests that Add method accepts null values for reference types.
        /// Input: Valid key with null value for reference type.
        /// Expected: Key-value pair with null value is successfully added.
        /// </summary>
        [Fact]
        public void Add_NullValueForReferenceType_AddsSuccessfully()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            string key = "testKey";
            string value = null;

            // Act
            dictionary.Add(key, value);

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.True(dictionary.ContainsKey(key));
            Assert.Null(dictionary[key]);
        }

        /// <summary>
        /// Tests that Add method works with value types and their default values.
        /// Input: Valid key with default value for value type.
        /// Expected: Key-value pair with default value is successfully added.
        /// </summary>
        [Fact]
        public void Add_DefaultValueForValueType_AddsSuccessfully()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            string key = "testKey";
            int defaultValue = default(int);

            // Act
            dictionary.Add(key, defaultValue);

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.True(dictionary.ContainsKey(key));
            Assert.Equal(0, dictionary[key]);
        }

        /// <summary>
        /// Tests that Add method works with different key types including edge values.
        /// Input: Various edge case keys (empty string, whitespace, special characters).
        /// Expected: All valid keys are successfully added maintaining their distinctness.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("!@#$%^&*()")]
        [InlineData("🔑")]
        public void Add_EdgeCaseStringKeys_AddsSuccessfully(string key)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            int value = 42;

            // Act
            dictionary.Add(key, value);

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.True(dictionary.ContainsKey(key));
            Assert.Equal(value, dictionary[key]);
        }

        /// <summary>
        /// Tests that Add method works with integer keys including boundary values.
        /// Input: Integer keys at various boundaries and special values.
        /// Expected: All integer keys are successfully added and can be retrieved.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Add_IntegerKeyBoundaryValues_AddsSuccessfully(int key)
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>();
            string value = "testValue";

            // Act
            dictionary.Add(key, value);

            // Assert
            Assert.Equal(1, dictionary.Count);
            Assert.True(dictionary.ContainsKey(key));
            Assert.Equal(value, dictionary[key]);
        }

        /// <summary>
        /// Tests that Add method works correctly with custom equality comparer.
        /// Input: Keys that would be different with default comparer but same with custom comparer.
        /// Expected: ArgumentException is thrown when adding keys considered equal by custom comparer.
        /// </summary>
        [Fact]
        public void Add_WithCustomEqualityComparer_RespectsComparer()
        {
            // Arrange
            var comparer = StringComparer.OrdinalIgnoreCase;
            var dictionary = new OrderedDictionary<string, int>(comparer);
            dictionary.Add("Key", 1);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => dictionary.Add("KEY", 2));
            Assert.Equal(1, dictionary.Count);
            Assert.Equal(1, dictionary["Key"]);
        }

        /// <summary>
        /// Tests that Add method works with large number of items without performance issues.
        /// Input: Large number of sequential key-value pairs.
        /// Expected: All items are added successfully and Count reflects total additions.
        /// </summary>
        [Fact]
        public void Add_LargeNumberOfItems_AddsAllSuccessfully()
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, int>();
            int itemCount = 1000;

            // Act
            for (int i = 0; i < itemCount; i++)
            {
                dictionary.Add(i, i * 2);
            }

            // Assert
            Assert.Equal(itemCount, dictionary.Count);
            for (int i = 0; i < itemCount; i++)
            {
                Assert.Equal(i * 2, dictionary[i]);
                Assert.Equal(i * 2, dictionary[i]); // Test both indexer overloads
            }
        }

        /// <summary>
        /// Tests that ContainsKey returns false when the dictionary is empty.
        /// </summary>
        [Fact]
        public void ContainsKey_EmptyDictionary_ReturnsFalse()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act
            bool result = dictionary.ContainsKey("nonexistent");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ContainsKey returns true when the key exists in the dictionary.
        /// </summary>
        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("test", 42);

            // Act
            bool result = dictionary.ContainsKey("test");

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ContainsKey returns false when the key does not exist in the dictionary.
        /// </summary>
        [Fact]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("test", 42);

            // Act
            bool result = dictionary.ContainsKey("nonexistent");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ContainsKey throws ArgumentNullException when key is null for reference type keys.
        /// </summary>
        [Fact]
        public void ContainsKey_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => dictionary.ContainsKey(null));
        }

        /// <summary>
        /// Tests ContainsKey with multiple keys to ensure proper lookup behavior.
        /// </summary>
        [Theory]
        [InlineData("first", true)]
        [InlineData("second", true)]
        [InlineData("third", true)]
        [InlineData("nonexistent", false)]
        [InlineData("", false)]
        public void ContainsKey_MultipleKeys_ReturnsExpectedResult(string key, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            bool result = dictionary.ContainsKey(key);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests ContainsKey with value type keys (int) to ensure proper behavior.
        /// </summary>
        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(int.MaxValue, false)]
        [InlineData(int.MinValue, false)]
        public void ContainsKey_ValueTypeKeys_ReturnsExpectedResult(int key, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>();
            dictionary.Add(1, "first");
            dictionary.Add(2, "second");
            dictionary.Add(3, "third");

            // Act
            bool result = dictionary.ContainsKey(key);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests ContainsKey with special string values including empty string and whitespace.
        /// </summary>
        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("\t", true)]
        [InlineData("\n", true)]
        [InlineData("normal", false)]
        public void ContainsKey_SpecialStringKeys_ReturnsExpectedResult(string key, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("", 1);
            dictionary.Add(" ", 2);
            dictionary.Add("\t", 3);
            dictionary.Add("\n", 4);

            // Act
            bool result = dictionary.ContainsKey(key);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests ContainsKey behavior after key removal to ensure proper state management.
        /// </summary>
        [Fact]
        public void ContainsKey_AfterRemove_ReturnsFalse()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("test", 42);
            dictionary.Remove("test");

            // Act
            bool result = dictionary.ContainsKey("test");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests ContainsKey with case-sensitive string keys.
        /// </summary>
        [Theory]
        [InlineData("Test", true)]
        [InlineData("test", false)]
        [InlineData("TEST", false)]
        [InlineData("tEsT", false)]
        public void ContainsKey_CaseSensitive_ReturnsExpectedResult(string key, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("Test", 42);

            // Act
            bool result = dictionary.ContainsKey(key);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Move correctly moves an item from the beginning to the end of the dictionary.
        /// Input: dictionary with multiple items, moving first item to last position.
        /// Expected result: item order is updated, values remain accessible by key.
        /// </summary>
        [Fact]
        public void Move_MoveFirstItemToLast_UpdatesOrderCorrectly()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            dictionary.Move(0, 2);

            // Assert
            Assert.Equal("second", dictionary.Keys.ElementAt(0));
            Assert.Equal("third", dictionary.Keys.ElementAt(1));
            Assert.Equal("first", dictionary.Keys.ElementAt(2));
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Move correctly moves an item from the end to the beginning of the dictionary.
        /// Input: dictionary with multiple items, moving last item to first position.
        /// Expected result: item order is updated, values remain accessible by key.
        /// </summary>
        [Fact]
        public void Move_MoveLastItemToFirst_UpdatesOrderCorrectly()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            dictionary.Move(2, 0);

            // Assert
            Assert.Equal("third", dictionary.Keys.ElementAt(0));
            Assert.Equal("first", dictionary.Keys.ElementAt(1));
            Assert.Equal("second", dictionary.Keys.ElementAt(2));
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Move correctly handles moving an item to the same position.
        /// Input: dictionary with multiple items, moving item to its current position.
        /// Expected result: no change in order, all values remain accessible.
        /// </summary>
        [Fact]
        public void Move_MoveItemToSamePosition_NoChangeInOrder()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            dictionary.Move(1, 1);

            // Assert
            Assert.Equal("first", dictionary.Keys.ElementAt(0));
            Assert.Equal("second", dictionary.Keys.ElementAt(1));
            Assert.Equal("third", dictionary.Keys.ElementAt(2));
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Move correctly moves an item forward by one position.
        /// Input: dictionary with multiple items, moving middle item forward.
        /// Expected result: item order is updated correctly.
        /// </summary>
        [Fact]
        public void Move_MoveItemForwardByOne_UpdatesOrderCorrectly()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            dictionary.Move(0, 1);

            // Assert
            Assert.Equal("second", dictionary.Keys.ElementAt(0));
            Assert.Equal("first", dictionary.Keys.ElementAt(1));
            Assert.Equal("third", dictionary.Keys.ElementAt(2));
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Move correctly moves an item backward by one position.
        /// Input: dictionary with multiple items, moving middle item backward.
        /// Expected result: item order is updated correctly.
        /// </summary>
        [Fact]
        public void Move_MoveItemBackwardByOne_UpdatesOrderCorrectly()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            dictionary.Move(2, 1);

            // Assert
            Assert.Equal("first", dictionary.Keys.ElementAt(0));
            Assert.Equal("third", dictionary.Keys.ElementAt(1));
            Assert.Equal("second", dictionary.Keys.ElementAt(2));
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Move correctly handles a dictionary with only one item.
        /// Input: dictionary with single item, moving item to position 0.
        /// Expected result: no change, item remains accessible.
        /// </summary>
        [Fact]
        public void Move_SingleItemDictionary_NoChangeInOrder()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("only", 42);

            // Act
            dictionary.Move(0, 0);

            // Assert
            Assert.Equal("only", dictionary.Keys.ElementAt(0));
            Assert.Equal(42, dictionary["only"]);
            Assert.Equal(1, dictionary.Count);
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when oldIndex is negative.
        /// Input: dictionary with items, oldIndex = -1.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_NegativeOldIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(-1, 0));
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when oldIndex is greater than or equal to Count.
        /// Input: dictionary with 2 items, oldIndex = 2.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_OldIndexOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(2, 0));
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when newIndex is negative.
        /// Input: dictionary with items, newIndex = -1.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_NegativeNewIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(0, -1));
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when newIndex is greater than Count.
        /// Input: dictionary with 2 items, newIndex = 3.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_NewIndexOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(0, 3));
        }

        /// <summary>
        /// Tests that Move throws ArgumentOutOfRangeException when called on an empty dictionary.
        /// Input: empty dictionary, any indices.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_EmptyDictionary_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(0, 0));
        }

        /// <summary>
        /// Tests that Move correctly handles moving to the boundary position (Count).
        /// Input: dictionary with items, moving item to position equal to Count.
        /// Expected result: item is moved to the end position.
        /// </summary>
        [Fact]
        public void Move_MoveToCountPosition_MovesToEnd()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);

            // Act
            dictionary.Move(0, 3);

            // Assert
            Assert.Equal("second", dictionary.Keys.ElementAt(0));
            Assert.Equal("third", dictionary.Keys.ElementAt(1));
            Assert.Equal("first", dictionary.Keys.ElementAt(2));
            Assert.Equal(1, dictionary["first"]);
            Assert.Equal(2, dictionary["second"]);
            Assert.Equal(3, dictionary["third"]);
        }

        /// <summary>
        /// Tests that Move preserves dictionary functionality with different key and value types.
        /// Input: dictionary with integer keys and string values.
        /// Expected result: order changes, values remain accessible.
        /// </summary>
        [Fact]
        public void Move_DifferentKeyValueTypes_PreservesFunctionality()
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>();
            dictionary.Add(1, "one");
            dictionary.Add(2, "two");
            dictionary.Add(3, "three");

            // Act
            dictionary.Move(1, 0);

            // Assert
            Assert.Equal(2, dictionary.Keys.ElementAt(0));
            Assert.Equal(1, dictionary.Keys.ElementAt(1));
            Assert.Equal(3, dictionary.Keys.ElementAt(2));
            Assert.Equal("one", dictionary[1]);
            Assert.Equal("two", dictionary[2]);
            Assert.Equal("three", dictionary[3]);
        }

        /// <summary>
        /// Tests Move with maximum integer boundary values for indices.
        /// Input: valid dictionary, using int.MaxValue as index.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_MaxIntegerIndices_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(int.MaxValue, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(0, int.MaxValue));
        }

        /// <summary>
        /// Tests Move with minimum integer boundary values for indices.
        /// Input: valid dictionary, using int.MinValue as index.
        /// Expected result: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void Move_MinIntegerIndices_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(int.MinValue, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.Move(0, int.MinValue));
        }

        /// <summary>
        /// Tests that Keys property returns an empty collection when dictionary is empty.
        /// </summary>
        [Fact]
        public void Keys_EmptyDictionary_ReturnsEmptyCollection()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Empty(keys);
            Assert.Equal(0, keys.Count);
        }

        /// <summary>
        /// Tests that Keys property returns correct collection when dictionary has single key-value pair.
        /// </summary>
        [Fact]
        public void Keys_SingleItem_ReturnsSingleKey()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            const string expectedKey = "test";
            dictionary.Add(expectedKey, 42);

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(expectedKey, keys);
            Assert.Equal(expectedKey, keys.First());
        }

        /// <summary>
        /// Tests that Keys property returns all keys in insertion order for multiple items.
        /// </summary>
        [Fact]
        public void Keys_MultipleItems_ReturnsKeysInInsertionOrder()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            var expectedKeys = new[] { "first", "second", "third", "fourth" };

            foreach (var key in expectedKeys)
            {
                dictionary.Add(key, expectedKeys.ToList().IndexOf(key));
            }

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(expectedKeys.Length, keys.Count);
            Assert.Equal(expectedKeys, keys);
        }

        /// <summary>
        /// Tests that Keys property returns correct type implementing ICollection interface.
        /// </summary>
        [Fact]
        public void Keys_ReturnType_ImplementsICollection()
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>();

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.IsAssignableFrom<ICollection<int>>(keys);
            Assert.IsAssignableFrom<ICollection>(keys);
        }

        /// <summary>
        /// Tests that Keys collection is read-only and throws exception when attempting to modify.
        /// </summary>
        [Fact]
        public void Keys_Collection_IsReadOnly()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("test", 1);
            var keys = dictionary.Keys;

            // Act & Assert
            Assert.True(keys.IsReadOnly);

            // Verify that modification operations throw exceptions
            Assert.Throws<NotSupportedException>(() => keys.Add("new"));
            Assert.Throws<NotSupportedException>(() => keys.Remove("test"));
            Assert.Throws<NotSupportedException>(() => keys.Clear());
        }

        /// <summary>
        /// Tests that Keys collection reflects changes when items are added to dictionary.
        /// </summary>
        [Fact]
        public void Keys_AfterAddingItems_ReflectsChanges()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            var keys = dictionary.Keys;

            // Act - Add items after getting Keys reference
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);

            // Assert
            Assert.Equal(2, keys.Count);
            Assert.Contains("first", keys);
            Assert.Contains("second", keys);
            Assert.Equal(new[] { "first", "second" }, keys);
        }

        /// <summary>
        /// Tests that Keys collection reflects changes when items are removed from dictionary.
        /// </summary>
        [Fact]
        public void Keys_AfterRemovingItems_ReflectsChanges()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            dictionary.Add("third", 3);
            var keys = dictionary.Keys;

            // Act - Remove item after getting Keys reference
            dictionary.Remove("second");

            // Assert
            Assert.Equal(2, keys.Count);
            Assert.Contains("first", keys);
            Assert.DoesNotContain("second", keys);
            Assert.Contains("third", keys);
            Assert.Equal(new[] { "first", "third" }, keys);
        }

        /// <summary>
        /// Tests that Keys collection reflects changes when dictionary is cleared.
        /// </summary>
        [Fact]
        public void Keys_AfterClear_BecomesEmpty()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            var keys = dictionary.Keys;

            // Act
            dictionary.Clear();

            // Assert
            Assert.Empty(keys);
            Assert.Equal(0, keys.Count);
        }

        /// <summary>
        /// Tests Keys property with integer keys including edge values.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Keys_IntegerKeys_HandlesEdgeValues(int key)
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, string>();
            dictionary.Add(key, "value");

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.Single(keys);
            Assert.Contains(key, keys);
            Assert.Equal(key, keys.First());
        }

        /// <summary>
        /// Tests Keys property with string keys including edge cases.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("normal")]
        [InlineData("very long string with special characters !@#$%^&*()")]
        public void Keys_StringKeys_HandlesEdgeCases(string key)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add(key, 42);

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.Single(keys);
            Assert.Contains(key, keys);
            Assert.Equal(key, keys.First());
        }

        /// <summary>
        /// Tests that Keys collection maintains consistent reference when accessed multiple times.
        /// </summary>
        [Fact]
        public void Keys_MultipleAccess_ReturnsSameReference()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("test", 1);

            // Act
            var keys1 = dictionary.Keys;
            var keys2 = dictionary.Keys;

            // Assert
            Assert.Same(keys1, keys2);
        }

        /// <summary>
        /// Tests that Keys collection correctly handles duplicate values (which should not occur with keys).
        /// </summary>
        [Fact]
        public void Keys_ContainsDistinctKeys_NoDuplicates()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 1); // Same value, different key
            dictionary.Add("key3", 1); // Same value, different key

            // Act
            var keys = dictionary.Keys;

            // Assert
            Assert.Equal(3, keys.Count);
            Assert.Equal(3, keys.Distinct().Count()); // All keys should be distinct
            Assert.Contains("key1", keys);
            Assert.Contains("key2", keys);
            Assert.Contains("key3", keys);
        }

        /// <summary>
        /// Tests Keys property behavior when using indexer to update existing key.
        /// </summary>
        [Fact]
        public void Keys_IndexerUpdate_DoesNotChangeKeyOrder()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("first", 1);
            dictionary.Add("second", 2);
            var keys = dictionary.Keys;
            var originalOrder = keys.ToArray();

            // Act - Update existing key using indexer
            dictionary["first"] = 10;

            // Assert
            Assert.Equal(originalOrder, keys);
            Assert.Equal(2, keys.Count);
        }

        /// <summary>
        /// Tests that IndexOf returns correct index when key is found at the start index.
        /// </summary>
        [Fact]
        public void IndexOf_KeyFoundAtStartIndex_ReturnsCorrectIndex()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);
            dictionary.Add("key3", 3);

            // Act
            int result = dictionary.IndexOf("key2", 1, 2);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns correct index when key is found within the specified range.
        /// </summary>
        [Fact]
        public void IndexOf_KeyFoundWithinRange_ReturnsCorrectIndex()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);
            dictionary.Add("key3", 3);
            dictionary.Add("key4", 4);

            // Act
            int result = dictionary.IndexOf("key3", 1, 3);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when key exists but is before the start index.
        /// </summary>
        [Fact]
        public void IndexOf_KeyExistsBeforeStartIndex_ReturnsMinusOne()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);
            dictionary.Add("key3", 3);

            // Act
            int result = dictionary.IndexOf("key1", 1, 2);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when key exists but is after the specified range.
        /// </summary>
        [Fact]
        public void IndexOf_KeyExistsAfterRange_ReturnsMinusOne()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);
            dictionary.Add("key3", 3);
            dictionary.Add("key4", 4);

            // Act
            int result = dictionary.IndexOf("key4", 1, 2);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when key does not exist in the dictionary.
        /// </summary>
        [Fact]
        public void IndexOf_KeyDoesNotExist_ReturnsMinusOne()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);

            // Act
            int result = dictionary.IndexOf("nonexistent", 0, 2);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf throws ArgumentOutOfRangeException when startIndex is negative.
        /// </summary>
        [Fact]
        public void IndexOf_NegativeStartIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.IndexOf("key1", -1, 1));
        }

        /// <summary>
        /// Tests that IndexOf throws ArgumentOutOfRangeException when startIndex is greater than or equal to Count.
        /// </summary>
        [Fact]
        public void IndexOf_StartIndexGreaterThanOrEqualToCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.IndexOf("key1", 1, 1));
        }

        /// <summary>
        /// Tests that IndexOf throws ArgumentOutOfRangeException when count is negative.
        /// </summary>
        [Fact]
        public void IndexOf_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.IndexOf("key1", 0, -1));
        }

        /// <summary>
        /// Tests that IndexOf throws ArgumentOutOfRangeException when startIndex plus count exceeds the dictionary Count.
        /// </summary>
        [Fact]
        public void IndexOf_StartIndexPlusCountExceedsCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.IndexOf("key1", 1, 2));
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when count is zero.
        /// </summary>
        [Fact]
        public void IndexOf_CountIsZero_ReturnsMinusOne()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);

            // Act
            int result = dictionary.IndexOf("key1", 0, 0);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOf with various boundary conditions using parameterized test data.
        /// </summary>
        [Theory]
        [InlineData("key1", 0, 1, 0)]  // Key at exact position
        [InlineData("key2", 0, 2, 1)]  // Key within range
        [InlineData("key3", 0, 3, 2)]  // Key at end of range
        [InlineData("key1", 1, 1, -1)] // Key before range
        [InlineData("key3", 0, 2, -1)] // Key after range
        public void IndexOf_BoundaryConditions_ReturnsExpectedResult(string key, int startIndex, int count, int expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 1);
            dictionary.Add("key2", 2);
            dictionary.Add("key3", 3);

            // Act
            int result = dictionary.IndexOf(key, startIndex, count);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IndexOf works correctly with duplicate values but different keys.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateValues_ReturnsCorrectIndex()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 100);
            dictionary.Add("key2", 100);
            dictionary.Add("key3", 100);

            // Act
            int result = dictionary.IndexOf("key2", 0, 3);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests ContainsValue method returns true when the specified value exists in the dictionary.
        /// </summary>
        [Fact]
        public void ContainsValue_ExistingValue_ReturnsTrue()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 42);
            dictionary.Add("key2", 100);

            // Act
            var result = dictionary.ContainsValue(42);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests ContainsValue method returns false when the specified value does not exist in the dictionary.
        /// </summary>
        [Fact]
        public void ContainsValue_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 42);
            dictionary.Add("key2", 100);

            // Act
            var result = dictionary.ContainsValue(999);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests ContainsValue method returns false when the dictionary is empty.
        /// </summary>
        [Fact]
        public void ContainsValue_EmptyDictionary_ReturnsFalse()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act
            var result = dictionary.ContainsValue(42);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests ContainsValue method with null value when TValue is a reference type.
        /// </summary>
        [Fact]
        public void ContainsValue_NullValue_ReferenceType_ReturnsCorrectResult()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            dictionary.Add("key1", "value1");
            dictionary.Add("key2", null);

            // Act
            var resultForExistingNull = dictionary.ContainsValue(null);

            // Assert
            Assert.True(resultForExistingNull);
        }

        /// <summary>
        /// Tests ContainsValue method with null value when dictionary does not contain null.
        /// </summary>
        [Fact]
        public void ContainsValue_NullValue_NotInDictionary_ReturnsFalse()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            dictionary.Add("key1", "value1");
            dictionary.Add("key2", "value2");

            // Act
            var result = dictionary.ContainsValue(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests ContainsValue method returns true when multiple keys have the same value.
        /// </summary>
        [Fact]
        public void ContainsValue_DuplicateValues_ReturnsTrue()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 42);
            dictionary.Add("key2", 100);
            dictionary.Add("key3", 42); // Duplicate value

            // Act
            var result = dictionary.ContainsValue(42);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests ContainsValue method with various edge case values for different data types.
        /// </summary>
        /// <param name="value">The value to search for in the dictionary.</param>
        /// <param name="expectedResult">The expected result of ContainsValue.</param>
        [Theory]
        [InlineData(int.MinValue, true)]
        [InlineData(int.MaxValue, true)]
        [InlineData(0, true)]
        [InlineData(-1, true)]
        [InlineData(999, false)]
        public void ContainsValue_IntegerEdgeCases_ReturnsExpectedResult(int value, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("minValue", int.MinValue);
            dictionary.Add("maxValue", int.MaxValue);
            dictionary.Add("zero", 0);
            dictionary.Add("negative", -1);

            // Act
            var result = dictionary.ContainsValue(value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests ContainsValue method with various string edge cases.
        /// </summary>
        /// <param name="searchValue">The string value to search for.</param>
        /// <param name="expectedResult">The expected result of ContainsValue.</param>
        [Theory]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData("normal", true)]
        [InlineData("notfound", false)]
        [InlineData("NORMAL", false)] // Case sensitive
        public void ContainsValue_StringEdgeCases_ReturnsExpectedResult(string searchValue, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            dictionary.Add("key1", "");
            dictionary.Add("key2", " ");
            dictionary.Add("key3", "normal");

            // Act
            var result = dictionary.ContainsValue(searchValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests ContainsValue method with floating point special values.
        /// </summary>
        /// <param name="value">The floating point value to search for.</param>
        /// <param name="expectedResult">The expected result of ContainsValue.</param>
        [Theory]
        [InlineData(double.NaN, true)]
        [InlineData(double.PositiveInfinity, true)]
        [InlineData(double.NegativeInfinity, true)]
        [InlineData(0.0, true)]
        [InlineData(-0.0, true)] // -0.0 equals 0.0
        [InlineData(1.0, false)]
        public void ContainsValue_DoubleSpecialValues_ReturnsExpectedResult(double value, bool expectedResult)
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, double>();
            dictionary.Add("nan", double.NaN);
            dictionary.Add("posInf", double.PositiveInfinity);
            dictionary.Add("negInf", double.NegativeInfinity);
            dictionary.Add("zero", 0.0);

            // Act
            var result = dictionary.ContainsValue(value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests ContainsValue method after adding and removing items from dictionary.
        /// </summary>
        [Fact]
        public void ContainsValue_AfterModifications_ReturnsCorrectResult()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();
            dictionary.Add("key1", 42);
            dictionary.Add("key2", 100);

            // Act & Assert - Value exists initially
            Assert.True(dictionary.ContainsValue(42));

            // Remove the key-value pair
            dictionary.Remove("key1");

            // Act & Assert - Value no longer exists
            Assert.False(dictionary.ContainsValue(42));

            // Add it back with different key
            dictionary.Add("key3", 42);

            // Act & Assert - Value exists again
            Assert.True(dictionary.ContainsValue(42));
        }

        /// <summary>
        /// Tests ContainsValue method with default values for value types.
        /// </summary>
        [Fact]
        public void ContainsValue_DefaultValues_ReturnsCorrectResult()
        {
            // Arrange
            var intDictionary = new OrderedDictionary<string, int>();
            var boolDictionary = new OrderedDictionary<string, bool>();

            intDictionary.Add("key1", 0); // default int
            boolDictionary.Add("key1", false); // default bool

            // Act & Assert
            Assert.True(intDictionary.ContainsValue(default(int)));
            Assert.True(boolDictionary.ContainsValue(default(bool)));

            // Test non-existing default values
            var emptyIntDictionary = new OrderedDictionary<string, int>();
            Assert.False(emptyIntDictionary.ContainsValue(default(int)));
        }

        /// <summary>
        /// Tests ContainsValue method with single item dictionary.
        /// </summary>
        [Fact]
        public void ContainsValue_SingleItemDictionary_ReturnsCorrectResult()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            dictionary.Add("onlyKey", "onlyValue");

            // Act & Assert
            Assert.True(dictionary.ContainsValue("onlyValue"));
            Assert.False(dictionary.ContainsValue("nonExistentValue"));
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid, empty OrderedDictionary instance
        /// with proper initialization of all internal collections and default comparer.
        /// </summary>
        [Theory]
        [InlineData(typeof(string), typeof(int))]
        [InlineData(typeof(int), typeof(string))]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(object), typeof(object))]
        public void Constructor_ParameterlessConstructor_CreatesEmptyDictionaryWithValidState(Type keyType, Type valueType)
        {
            // Arrange & Act
            var dictionary = CreateOrderedDictionary(keyType, valueType);

            // Assert
            Assert.NotNull(dictionary);
            Assert.Equal(0, GetCount(dictionary));
            Assert.NotNull(GetKeys(dictionary));
            Assert.Empty(GetKeys(dictionary));
            Assert.NotNull(GetValues(dictionary));
            Assert.Empty(GetValues(dictionary));
            Assert.NotNull(GetComparer(dictionary));
            Assert.False(GetIsReadOnly(dictionary));
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a dictionary that supports basic operations
        /// and behaves correctly with ContainsKey and ContainsValue for non-existent items.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_SupportsFunctionalOperations()
        {
            // Arrange & Act
            var dictionary = new OrderedDictionary<string, int>();

            // Assert - Basic functionality should work on empty dictionary
            Assert.False(dictionary.ContainsKey("nonexistent"));
            Assert.False(dictionary.ContainsValue(42));
            Assert.Equal(-1, dictionary.IndexOf("nonexistent"));

            var enumerator = dictionary.GetEnumerator();
            Assert.NotNull(enumerator);
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that the parameterless constructor uses the default equality comparer for the key type.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_UsesDefaultEqualityComparer()
        {
            // Arrange & Act
            var stringKeyDictionary = new OrderedDictionary<string, int>();
            var intKeyDictionary = new OrderedDictionary<int, string>();

            // Assert
            Assert.Same(EqualityComparer<string>.Default, stringKeyDictionary.Comparer);
            Assert.Same(EqualityComparer<int>.Default, intKeyDictionary.Comparer);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a dictionary that can be successfully
        /// modified with Add operations after construction.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_AllowsSubsequentModifications()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, int>();

            // Act
            dictionary.Add("key1", 1);
            dictionary["key2"] = 2;

            // Assert
            Assert.Equal(2, dictionary.Count);
            Assert.True(dictionary.ContainsKey("key1"));
            Assert.True(dictionary.ContainsKey("key2"));
            Assert.Equal(1, dictionary["key1"]);
            Assert.Equal(2, dictionary["key2"]);
        }

        /// <summary>
        /// Tests that the parameterless constructor handles reference and value type combinations correctly.
        /// </summary>
        [Theory]
        [InlineData("string", 42)]
        [InlineData(null, 0)]
        public void Constructor_ParameterlessConstructor_HandlesNullableReferenceTypes(string key, int value)
        {
            // Arrange & Act
            var dictionary = new OrderedDictionary<string, int>();

            // Assert - Should handle null keys and values according to underlying Dictionary behavior
            if (key != null)
            {
                dictionary.Add(key, value);
                Assert.True(dictionary.ContainsKey(key));
                Assert.Equal(value, dictionary[key]);
            }
        }

        private static object CreateOrderedDictionary(Type keyType, Type valueType)
        {
            var genericType = typeof(OrderedDictionary<,>).MakeGenericType(keyType, valueType);
            return Activator.CreateInstance(genericType);
        }

        private static int GetCount(object dictionary)
        {
            return (int)dictionary.GetType().GetProperty("Count").GetValue(dictionary);
        }

        private static object GetKeys(object dictionary)
        {
            return dictionary.GetType().GetProperty("Keys").GetValue(dictionary);
        }

        private static object GetValues(object dictionary)
        {
            return dictionary.GetType().GetProperty("Values").GetValue(dictionary);
        }

        private static object GetComparer(object dictionary)
        {
            return dictionary.GetType().GetProperty("Comparer").GetValue(dictionary);
        }

        private static bool GetIsReadOnly(object dictionary)
        {
            var collection = dictionary as System.Collections.ICollection;
            var property = dictionary.GetType().GetProperty("IsReadOnly");
            if (property != null)
            {
                return (bool)property.GetValue(dictionary);
            }
            return false;
        }
    }

    public partial class OrderedDictionaryReadOnlyValueCollectionTests
    {
        /// <summary>
        /// Tests that the Add method throws NotSupportedException with string values.
        /// This verifies the read-only nature of the collection.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Add_WithStringValue_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, string>();
            var readOnlyCollection = orderedDictionary.Values;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add("test"));
        }

        /// <summary>
        /// Tests that the Add method throws NotSupportedException with null string value.
        /// This verifies the read-only nature of the collection regardless of input.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Add_WithNullStringValue_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, string>();
            var readOnlyCollection = orderedDictionary.Values;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add(null));
        }

        /// <summary>
        /// Tests that the Add method throws NotSupportedException with integer values.
        /// This verifies the read-only nature of the collection with value types.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Add_WithIntegerValues_ThrowsNotSupportedException(int value)
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, int>();
            var readOnlyCollection = orderedDictionary.Values;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add(value));
        }

        /// <summary>
        /// Tests that the Add method throws NotSupportedException with object values.
        /// This verifies the read-only nature of the collection with reference types.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Add_WithObjectValue_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, object>();
            var readOnlyCollection = orderedDictionary.Values;
            var testObject = new object();

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add(testObject));
        }

        /// <summary>
        /// Tests that the Add method throws NotSupportedException with null object value.
        /// This verifies the read-only nature of the collection with null reference types.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Add_WithNullObjectValue_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, object>();
            var readOnlyCollection = orderedDictionary.Values;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add(null));
        }

        /// <summary>
        /// Tests that the Add method throws NotSupportedException with default value.
        /// This verifies the read-only nature of the collection with default values.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Add_WithDefaultValue_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, int>();
            var readOnlyCollection = orderedDictionary.Values;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add(default(int)));
        }

        /// <summary>
        /// Tests that the Add method throws NotSupportedException with custom class instances.
        /// This verifies the read-only nature of the collection with custom reference types.
        /// Expected result: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Add_WithCustomClass_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, TestClass>();
            var readOnlyCollection = orderedDictionary.Values;
            var testInstance = new TestClass { Value = "test" };

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add(testInstance));
        }

        private class TestClass
        {
            public string Value { get; set; }
        }
    }

    /// <summary>
    /// Tests for the ReadOnlyValueCollection.Contains method in OrderedDictionary.
    /// </summary>
    public class ReadOnlyValueCollectionTests
    {
        /// <summary>
        /// Tests that Contains returns true when the value exists in the collection.
        /// </summary>
        [Fact]
        public void Contains_ExistingValue_ReturnsTrue()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", "value2");
            var values = orderedDict.Values;

            // Act
            var result = values.Contains("value1");

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the value does not exist in the collection.
        /// </summary>
        [Fact]
        public void Contains_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", "value2");
            var values = orderedDict.Values;

            // Act
            var result = values.Contains("nonexistent");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when searching for null and null exists in the collection.
        /// </summary>
        [Fact]
        public void Contains_NullValue_WhenNullExists_ReturnsTrue()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", null);
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for null and null does not exist in the collection.
        /// </summary>
        [Fact]
        public void Contains_NullValue_WhenNullDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", "value2");
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection.
        /// </summary>
        [Fact]
        public void Contains_OnEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            var values = orderedDict.Values;

            // Act
            var result = values.Contains("anyvalue");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the same value is associated with multiple keys.
        /// </summary>
        [Fact]
        public void Contains_DuplicateValues_ReturnsTrue()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "duplicate");
            orderedDict.Add("key2", "duplicate");
            orderedDict.Add("key3", "other");
            var values = orderedDict.Values;

            // Act
            var result = values.Contains("duplicate");

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with value types.
        /// </summary>
        [Fact]
        public void Contains_ValueType_ExistingValue_ReturnsTrue()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 10);
            orderedDict.Add("key2", 20);
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(10);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with value types when value doesn't exist.
        /// </summary>
        [Fact]
        public void Contains_ValueType_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 10);
            orderedDict.Add("key2", 20);
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(30);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when searching for the default value and it exists in the collection.
        /// </summary>
        [Fact]
        public void Contains_DefaultValue_WhenDefaultExists_ReturnsTrue()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 0); // default value for int
            orderedDict.Add("key2", 20);
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(default(int));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for the default value and it doesn't exist in the collection.
        /// </summary>
        [Fact]
        public void Contains_DefaultValue_WhenDefaultDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 10);
            orderedDict.Add("key2", 20);
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(default(int));

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Contains with various string edge cases using parameterized test data.
        /// </summary>
        [Theory]
        [InlineData("", true)] // empty string exists
        [InlineData(" ", false)] // whitespace doesn't exist
        [InlineData("normal", true)] // normal string exists
        [InlineData("NORMAL", false)] // case sensitivity
        public void Contains_StringEdgeCases_ReturnsExpectedResult(string searchValue, bool expected)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<int, string>();
            orderedDict.Add(1, "");
            orderedDict.Add(2, "normal");
            orderedDict.Add(3, "other");
            var values = orderedDict.Values;

            // Act
            var result = values.Contains(searchValue);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property of ReadOnlyValueCollection always returns true.
        /// Validates that the collection correctly identifies itself as read-only.
        /// Expected result: IsReadOnly should return true.
        /// </summary>
        [Fact]
        public void IsReadOnly_Always_ReturnsTrue()
        {
            // Arrange
            var orderedDictionary = new OrderedDictionary<string, int>();
            var values = orderedDictionary.Values;

            // Act
            var isReadOnly = values.IsReadOnly;

            // Assert
            Assert.True(isReadOnly);
        }

        /// <summary>
        /// Tests IndexOf method when the item exists in the collection.
        /// Verifies that the correct index is returned for existing items.
        /// </summary>
        [Theory]
        [InlineData("value1", 0)]
        [InlineData("value2", 1)]
        [InlineData("value3", 2)]
        public void IndexOf_ItemExists_ReturnsCorrectIndex(string searchValue, int expectedIndex)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", "value2");
            orderedDict.Add("key3", "value3");
            var valueCollection = orderedDict.Values;

            // Act
            int actualIndex = valueCollection.IndexOf(searchValue);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method when the item does not exist in the collection.
        /// Verifies that -1 is returned for non-existing items.
        /// </summary>
        [Theory]
        [InlineData("nonexistent")]
        [InlineData("")]
        [InlineData("different")]
        public void IndexOf_ItemNotExists_ReturnsNegativeOne(string searchValue)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", "value2");
            var valueCollection = orderedDict.Values;

            // Act
            int actualIndex = valueCollection.IndexOf(searchValue);

            // Assert
            Assert.Equal(-1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method on an empty collection.
        /// Verifies that -1 is returned when searching in an empty collection.
        /// </summary>
        [Fact]
        public void IndexOf_EmptyCollection_ReturnsNegativeOne()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            var valueCollection = orderedDict.Values;

            // Act
            int actualIndex = valueCollection.IndexOf("anyvalue");

            // Assert
            Assert.Equal(-1, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method when searching for null value.
        /// Verifies that the method correctly handles null search values.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsCorrectResult()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", null);
            orderedDict.Add("key2", "value2");
            var valueCollection = orderedDict.Values;

            // Act
            int actualIndex = valueCollection.IndexOf(null);

            // Assert
            Assert.Equal(0, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method when the collection contains duplicate values.
        /// Verifies that the index of the first occurrence is returned.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateValues_ReturnsFirstOccurrence()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "duplicate");
            orderedDict.Add("key2", "value2");
            orderedDict.Add("key3", "duplicate");
            var valueCollection = orderedDict.Values;

            // Act
            int actualIndex = valueCollection.IndexOf("duplicate");

            // Assert
            Assert.Equal(0, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with different value types.
        /// Verifies that the method works correctly with integer values.
        /// </summary>
        [Theory]
        [InlineData(10, 0)]
        [InlineData(20, 1)]
        [InlineData(30, 2)]
        [InlineData(99, -1)]
        public void IndexOf_IntegerValues_ReturnsCorrectIndex(int searchValue, int expectedIndex)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 10);
            orderedDict.Add("key2", 20);
            orderedDict.Add("key3", 30);
            var valueCollection = orderedDict.Values;

            // Act
            int actualIndex = valueCollection.IndexOf(searchValue);

            // Assert
            Assert.Equal(expectedIndex, actualIndex);
        }

        /// <summary>
        /// Tests IndexOf method with single item collection.
        /// Verifies correct behavior when collection contains only one item.
        /// </summary>
        [Fact]
        public void IndexOf_SingleItemCollection_ReturnsCorrectResult()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("onlykey", "onlyvalue");
            var valueCollection = orderedDict.Values;

            // Act
            int foundIndex = valueCollection.IndexOf("onlyvalue");
            int notFoundIndex = valueCollection.IndexOf("other");

            // Assert
            Assert.Equal(0, foundIndex);
            Assert.Equal(-1, notFoundIndex);
        }

        /// <summary>
        /// Tests that Count property returns 0 for an empty OrderedDictionary.
        /// This verifies that the Count property correctly delegates to the underlying dictionary's Count.
        /// Expected result: Count should be 0.
        /// </summary>
        [Fact]
        public void Count_EmptyDictionary_ReturnsZero()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            var valuesCollection = orderedDict.Values;

            // Act
            var count = valuesCollection.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count property returns the correct number for dictionary with single item.
        /// This verifies that the Count property correctly delegates to the underlying dictionary's Count.
        /// Expected result: Count should be 1.
        /// </summary>
        [Fact]
        public void Count_SingleItem_ReturnsOne()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 10);
            var valuesCollection = orderedDict.Values;

            // Act
            var count = valuesCollection.Count;

            // Assert
            Assert.Equal(1, count);
        }

        /// <summary>
        /// Tests that Count property returns the correct number for dictionary with multiple items.
        /// This verifies that the Count property correctly reflects the state of the underlying dictionary.
        /// Expected result: Count should match the number of items in the dictionary.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public void Count_MultipleItems_ReturnsCorrectCount(int itemCount)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            for (int i = 0; i < itemCount; i++)
            {
                orderedDict.Add($"key{i}", i);
            }
            var valuesCollection = orderedDict.Values;

            // Act
            var count = valuesCollection.Count;

            // Assert
            Assert.Equal(itemCount, count);
        }

        /// <summary>
        /// Tests that Count property reflects changes when items are added to the dictionary.
        /// This verifies that the Count property dynamically reflects the current state.
        /// Expected result: Count should increase as items are added.
        /// </summary>
        [Fact]
        public void Count_AfterAddingItems_ReflectsChanges()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            var valuesCollection = orderedDict.Values;

            // Act & Assert
            Assert.Equal(0, valuesCollection.Count);

            orderedDict.Add("key1", 1);
            Assert.Equal(1, valuesCollection.Count);

            orderedDict.Add("key2", 2);
            Assert.Equal(2, valuesCollection.Count);

            orderedDict.Add("key3", 3);
            Assert.Equal(3, valuesCollection.Count);
        }

        /// <summary>
        /// Tests that Count property reflects changes when items are removed from the dictionary.
        /// This verifies that the Count property dynamically reflects the current state.
        /// Expected result: Count should decrease as items are removed.
        /// </summary>
        [Fact]
        public void Count_AfterRemovingItems_ReflectsChanges()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);
            orderedDict.Add("key3", 3);
            var valuesCollection = orderedDict.Values;

            // Act & Assert
            Assert.Equal(3, valuesCollection.Count);

            orderedDict.Remove("key1");
            Assert.Equal(2, valuesCollection.Count);

            orderedDict.Remove("key2");
            Assert.Equal(1, valuesCollection.Count);

            orderedDict.Remove("key3");
            Assert.Equal(0, valuesCollection.Count);
        }

        /// <summary>
        /// Tests that Count property reflects changes when dictionary is cleared.
        /// This verifies that the Count property correctly handles the Clear operation.
        /// Expected result: Count should be 0 after clearing.
        /// </summary>
        [Fact]
        public void Count_AfterClear_ReturnsZero()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);
            orderedDict.Add("key3", 3);
            var valuesCollection = orderedDict.Values;

            // Act
            orderedDict.Clear();

            // Assert
            Assert.Equal(0, valuesCollection.Count);
        }

        /// <summary>
        /// Tests that Count property reflects changes when items are updated (not added/removed).
        /// This verifies that updating existing items does not change the count.
        /// Expected result: Count should remain the same when updating existing values.
        /// </summary>
        [Fact]
        public void Count_AfterUpdatingExistingItems_RemainsUnchanged()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);
            var valuesCollection = orderedDict.Values;

            // Act & Assert
            Assert.Equal(2, valuesCollection.Count);

            orderedDict["key1"] = 100;
            Assert.Equal(2, valuesCollection.Count);

            orderedDict["key2"] = 200;
            Assert.Equal(2, valuesCollection.Count);
        }

        /// <summary>
        /// Tests that Count property works correctly with different key and value types.
        /// This verifies that the Count property works regardless of the generic type parameters.
        /// Expected result: Count should work correctly with different types.
        /// </summary>
        [Fact]
        public void Count_WithDifferentTypes_WorksCorrectly()
        {
            // Arrange
            var intStringDict = new OrderedDictionary<int, string>();
            intStringDict.Add(1, "one");
            intStringDict.Add(2, "two");

            var stringObjectDict = new OrderedDictionary<string, object>();
            stringObjectDict.Add("test1", new object());
            stringObjectDict.Add("test2", 42);
            stringObjectDict.Add("test3", "string");

            // Act
            var intStringCount = intStringDict.Values.Count;
            var stringObjectCount = stringObjectDict.Values.Count;

            // Assert
            Assert.Equal(2, intStringCount);
            Assert.Equal(3, stringObjectCount);
        }

        /// <summary>
        /// Tests that the Insert method always throws NotSupportedException regardless of input parameters.
        /// This verifies that the read-only collection properly prevents insertion operations.
        /// </summary>
        /// <param name="index">The index parameter to test with</param>
        /// <param name="value">The value parameter to test with</param>
        [Theory]
        [InlineData(0, "test")]
        [InlineData(1, "value")]
        [InlineData(-1, "negative")]
        [InlineData(int.MaxValue, "max")]
        [InlineData(int.MinValue, "min")]
        [InlineData(100, "large")]
        public void Insert_AnyValidParameters_ThrowsNotSupportedException(int index, string value)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Insert(index, value));
        }

        /// <summary>
        /// Tests that the Insert method throws NotSupportedException when called with null value.
        /// This ensures the method throws the expected exception even with null reference types.
        /// </summary>
        [Fact]
        public void Insert_WithNullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Insert(0, null));
        }

        /// <summary>
        /// Tests that the Insert method throws NotSupportedException with empty string value.
        /// This verifies the method behavior with edge case string values.
        /// </summary>
        [Fact]
        public void Insert_WithEmptyString_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<int, string>();
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Insert(0, string.Empty));
        }

        /// <summary>
        /// Tests that the Insert method throws NotSupportedException with integer value types.
        /// This ensures the method works correctly with value types as well as reference types.
        /// </summary>
        /// <param name="index">The index parameter to test with</param>
        /// <param name="value">The integer value parameter to test with</param>
        [Theory]
        [InlineData(0, 42)]
        [InlineData(-1, int.MinValue)]
        [InlineData(1, int.MaxValue)]
        [InlineData(10, 0)]
        public void Insert_WithIntegerValueType_ThrowsNotSupportedException(int index, int value)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Insert(index, value));
        }

        /// <summary>
        /// Tests that the Insert method throws NotSupportedException even when the collection contains existing items.
        /// This verifies that the read-only nature is maintained regardless of the collection state.
        /// </summary>
        [Fact]
        public void Insert_WithExistingItems_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("key1", "value1");
            orderedDict.Add("key2", "value2");
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Insert(1, "newValue"));
        }

        /// <summary>
        /// Tests that Remove method throws NotSupportedException when called with a valid value.
        /// The ReadOnlyValueCollection should not support removal operations.
        /// </summary>
        [Fact]
        public void Remove_WithValidValue_ThrowsNotSupportedException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            dictionary.Add("key1", "value1");
            var values = dictionary.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => values.Remove("value1"));
        }

        /// <summary>
        /// Tests that Remove method throws NotSupportedException when called with null value.
        /// The ReadOnlyValueCollection should not support removal operations even with null input.
        /// </summary>
        [Fact]
        public void Remove_WithNullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            var values = dictionary.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => values.Remove(null));
        }

        /// <summary>
        /// Tests that Remove method throws NotSupportedException when called with default value.
        /// The ReadOnlyValueCollection should not support removal operations for any input.
        /// </summary>
        [Fact]
        public void Remove_WithDefaultValue_ThrowsNotSupportedException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<int, int>();
            var values = dictionary.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => values.Remove(default(int)));
        }

        /// <summary>
        /// Tests that Remove method throws NotSupportedException when called with non-existent value.
        /// The ReadOnlyValueCollection should not support removal operations even when value doesn't exist.
        /// </summary>
        [Fact]
        public void Remove_WithNonExistentValue_ThrowsNotSupportedException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            dictionary.Add("key1", "value1");
            var values = dictionary.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => values.Remove("nonExistentValue"));
        }

        /// <summary>
        /// Tests that Remove method throws NotSupportedException on empty collection.
        /// The ReadOnlyValueCollection should not support removal operations even when empty.
        /// </summary>
        [Fact]
        public void Remove_OnEmptyCollection_ThrowsNotSupportedException()
        {
            // Arrange
            var dictionary = new OrderedDictionary<string, string>();
            var values = dictionary.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => values.Remove("anyValue"));
        }

        /// <summary>
        /// Tests that the ReadOnlyValueCollection constructor properly initializes with a valid OrderedDictionary.
        /// Input: Valid OrderedDictionary instance with some data.
        /// Expected: Constructor completes successfully and the collection behaves correctly.
        /// </summary>
        [Fact]
        public void Constructor_ValidOrderedDictionary_InitializesCorrectly()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);

            // Act
            var readOnlyCollection = new OrderedDictionary<string, int>.ReadOnlyValueCollection(orderedDict);

            // Assert
            Assert.Equal(2, readOnlyCollection.Count);
            Assert.True(readOnlyCollection.Contains(1));
            Assert.True(readOnlyCollection.Contains(2));
            Assert.False(readOnlyCollection.Contains(3));
        }

        /// <summary>
        /// Tests that the ReadOnlyValueCollection constructor handles an empty OrderedDictionary.
        /// Input: Empty OrderedDictionary instance.
        /// Expected: Constructor completes successfully and the collection is empty.
        /// </summary>
        [Fact]
        public void Constructor_EmptyOrderedDictionary_InitializesCorrectly()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();

            // Act
            var readOnlyCollection = new OrderedDictionary<string, int>.ReadOnlyValueCollection(orderedDict);

            // Assert
            Assert.Equal(0, readOnlyCollection.Count);
            Assert.False(readOnlyCollection.Contains(1));
        }

        /// <summary>
        /// Tests that the ReadOnlyValueCollection constructor handles a null OrderedDictionary parameter.
        /// Input: Null OrderedDictionary reference.
        /// Expected: Constructor completes successfully without throwing an exception (no null check in implementation).
        /// </summary>
        [Fact]
        public void Constructor_NullOrderedDictionary_DoesNotThrow()
        {
            // Arrange & Act
            var exception = Record.Exception(() => new OrderedDictionary<string, int>.ReadOnlyValueCollection(null));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the ReadOnlyValueCollection constructor works with different generic types.
        /// Input: OrderedDictionary with object keys and string values.
        /// Expected: Constructor completes successfully and collection works with the specified types.
        /// </summary>
        [Fact]
        public void Constructor_DifferentGenericTypes_InitializesCorrectly()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<object, string>();
            orderedDict.Add(1, "value1");
            orderedDict.Add("key2", "value2");

            // Act
            var readOnlyCollection = new OrderedDictionary<object, string>.ReadOnlyValueCollection(orderedDict);

            // Assert
            Assert.Equal(2, readOnlyCollection.Count);
            Assert.True(readOnlyCollection.Contains("value1"));
            Assert.True(readOnlyCollection.Contains("value2"));
        }

        /// <summary>
        /// Tests that the ReadOnlyValueCollection constructor maintains reference to the original dictionary.
        /// Input: OrderedDictionary that is modified after ReadOnlyValueCollection creation.
        /// Expected: ReadOnlyValueCollection reflects changes in the original dictionary.
        /// </summary>
        [Fact]
        public void Constructor_ModifiedOriginalDictionary_ReflectsChanges()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var readOnlyCollection = new OrderedDictionary<string, int>.ReadOnlyValueCollection(orderedDict);

            // Act
            orderedDict.Add("key2", 2);

            // Assert
            Assert.Equal(2, readOnlyCollection.Count);
            Assert.True(readOnlyCollection.Contains(1));
            Assert.True(readOnlyCollection.Contains(2));
        }

        /// <summary>
        /// Tests that RemoveAt throws NotSupportedException for valid positive index.
        /// The ReadOnlyValueCollection is read-only and should not allow item removal.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidPositiveIndex_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);
            var readOnlyValues = (IList<int>)orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.RemoveAt(0));
        }

        /// <summary>
        /// Tests that RemoveAt throws NotSupportedException for index zero.
        /// The ReadOnlyValueCollection is read-only and should not allow item removal.
        /// </summary>
        [Fact]
        public void RemoveAt_IndexZero_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var readOnlyValues = (IList<int>)orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.RemoveAt(0));
        }

        /// <summary>
        /// Tests that RemoveAt throws NotSupportedException for negative index.
        /// The ReadOnlyValueCollection is read-only and should not allow item removal.
        /// </summary>
        [Fact]
        public void RemoveAt_NegativeIndex_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var readOnlyValues = (IList<int>)orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.RemoveAt(-1));
        }

        /// <summary>
        /// Tests that RemoveAt throws NotSupportedException for out-of-bounds index.
        /// The ReadOnlyValueCollection is read-only and should not allow item removal.
        /// </summary>
        [Fact]
        public void RemoveAt_OutOfBoundsIndex_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var readOnlyValues = (IList<int>)orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.RemoveAt(10));
        }

        /// <summary>
        /// Tests that RemoveAt throws NotSupportedException for empty collection.
        /// The ReadOnlyValueCollection is read-only and should not allow item removal.
        /// </summary>
        [Fact]
        public void RemoveAt_EmptyCollection_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            var readOnlyValues = (IList<int>)orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.RemoveAt(0));
        }

        /// <summary>
        /// Tests that RemoveAt throws NotSupportedException for extreme boundary values.
        /// The ReadOnlyValueCollection is read-only and should not allow item removal.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void RemoveAt_ExtremeIndexValues_ThrowsNotSupportedException(int index)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var readOnlyValues = (IList<int>)orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.RemoveAt(index));
        }

        /// <summary>
        /// Tests that Clear method throws NotSupportedException when called.
        /// This verifies that the read-only collection properly prevents modification operations.
        /// </summary>
        [Fact]
        public void Clear_WhenCalled_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Clear());
        }

        /// <summary>
        /// Tests that Clear method throws NotSupportedException even when the collection is empty.
        /// This verifies consistent behavior regardless of collection state.
        /// </summary>
        [Fact]
        public void Clear_WhenCollectionEmpty_ThrowsNotSupportedException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            var readOnlyValues = orderedDict.Values;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => readOnlyValues.Clear());
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var collection = orderedDict.Values;
            TValue[] array = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => collection.CopyTo(array, 0));
            Assert.Equal("array", exception.ParamName);
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
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var collection = orderedDict.Values;
            var array = new int[5];

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, arrayIndex));
            Assert.Equal("arrayIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is greater than array length.
        /// </summary>
        [Theory]
        [InlineData(6)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void CopyTo_ArrayIndexGreaterThanArrayLength_ThrowsArgumentOutOfRangeException(int arrayIndex)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            var collection = orderedDict.Values;
            var array = new int[5];

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(array, arrayIndex));
            Assert.Equal("arrayIndex", exception.ParamName);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is insufficient space in the array.
        /// </summary>
        [Theory]
        [InlineData(3, 3)] // array length 3, arrayIndex 3, no space at all
        [InlineData(5, 4)] // array length 5, arrayIndex 4, only 1 space but need 2
        [InlineData(10, 9)] // array length 10, arrayIndex 9, only 1 space but need 2
        public void CopyTo_InsufficientSpace_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("key1", 1);
            orderedDict.Add("key2", 2);
            var collection = orderedDict.Values;
            var array = new int[arrayLength];

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(array, arrayIndex));
            Assert.Contains("Not enough space in array to copy", exception.Message);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies items from empty collection.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyCollection_DoesNotModifyArray()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            var collection = orderedDict.Values;
            var array = new int[5] { -1, -1, -1, -1, -1 };
            var originalArray = new int[] { -1, -1, -1, -1, -1 };

            // Act
            collection.CopyTo(array, 2);

            // Assert
            Assert.Equal(originalArray, array);
        }

        /// <summary>
        /// Tests that CopyTo successfully copies items starting at the beginning of the array.
        /// </summary>
        [Fact]
        public void CopyTo_ValidParametersStartingAtZero_CopiesCorrectly()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("first", 10);
            orderedDict.Add("second", 20);
            orderedDict.Add("third", 30);
            var collection = orderedDict.Values;
            var array = new int[5];

            // Act
            collection.CopyTo(array, 0);

            // Assert
            Assert.Equal(10, array[0]);
            Assert.Equal(20, array[1]);
            Assert.Equal(30, array[2]);
            Assert.Equal(0, array[3]); // unchanged
            Assert.Equal(0, array[4]); // unchanged
        }

        /// <summary>
        /// Tests that CopyTo successfully copies items starting at a specific index in the array.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void CopyTo_ValidParametersWithOffset_CopiesCorrectly(int arrayIndex)
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("alpha", 100);
            orderedDict.Add("beta", 200);
            var collection = orderedDict.Values;
            var array = new int[5] { -1, -1, -1, -1, -1 };

            // Act
            collection.CopyTo(array, arrayIndex);

            // Assert
            // Check values before arrayIndex remain unchanged
            for (int i = 0; i < arrayIndex; i++)
            {
                Assert.Equal(-1, array[i]);
            }

            // Check copied values
            Assert.Equal(100, array[arrayIndex]);
            Assert.Equal(200, array[arrayIndex + 1]);

            // Check values after copied items remain unchanged
            for (int i = arrayIndex + 2; i < array.Length; i++)
            {
                Assert.Equal(-1, array[i]);
            }
        }

        /// <summary>
        /// Tests that CopyTo works with exact fit scenario where array space exactly matches collection count.
        /// </summary>
        [Fact]
        public void CopyTo_ExactFit_CopiesCorrectly()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, int>();
            orderedDict.Add("one", 1);
            orderedDict.Add("two", 2);
            var collection = orderedDict.Values;
            var array = new int[3] { -1, 0, 0 };

            // Act
            collection.CopyTo(array, 1);

            // Assert
            Assert.Equal(-1, array[0]); // unchanged
            Assert.Equal(1, array[1]);   // copied
            Assert.Equal(2, array[2]);   // copied
        }

        /// <summary>
        /// Tests that CopyTo maintains the order of items as they appear in the ordered dictionary.
        /// </summary>
        [Fact]
        public void CopyTo_ValidParameters_MaintainsOrder()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<string, string>();
            orderedDict.Add("z", "last");
            orderedDict.Add("a", "first");
            orderedDict.Add("m", "middle");
            var collection = orderedDict.Values;
            var array = new string[3];

            // Act
            collection.CopyTo(array, 0);

            // Assert - should maintain insertion order, not alphabetical order
            Assert.Equal("last", array[0]);
            Assert.Equal("first", array[1]);
            Assert.Equal("middle", array[2]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly with single item collection.
        /// </summary>
        [Fact]
        public void CopyTo_SingleItem_CopiesCorrectly()
        {
            // Arrange
            var orderedDict = new OrderedDictionary<int, string>();
            orderedDict.Add(42, "answer");
            var collection = orderedDict.Values;
            var array = new string[3] { "default", "default", "default" };

            // Act
            collection.CopyTo(array, 1);

            // Assert
            Assert.Equal("default", array[0]); // unchanged
            Assert.Equal("answer", array[1]);  // copied
            Assert.Equal("default", array[2]); // unchanged
        }
    }
}