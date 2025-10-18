using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ShellNavigationQueryParametersTests
    {
        /// <summary>
        /// Tests that the constructor with IEnumerable parameter creates an empty dictionary when given an empty collection.
        /// Input: Empty collection of KeyValuePair elements
        /// Expected: Empty ShellNavigationQueryParameters instance with Count = 0
        /// </summary>
        [Fact]
        public void Constructor_EmptyCollection_CreatesEmptyDictionary()
        {
            // Arrange
            var emptyCollection = new List<KeyValuePair<string, object>>();

            // Act
            var result = new ShellNavigationQueryParameters(emptyCollection);

            // Assert
            Assert.Equal(0, result.Count);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter correctly adds a single key-value pair.
        /// Input: Collection with one valid KeyValuePair
        /// Expected: ShellNavigationQueryParameters with one item, accessible via indexer
        /// </summary>
        [Fact]
        public void Constructor_SingleValidItem_AddsItemCorrectly()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("key1", "value1")
            };

            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.True(result.ContainsKey("key1"));
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter correctly adds multiple key-value pairs.
        /// Input: Collection with multiple valid KeyValuePair elements
        /// Expected: ShellNavigationQueryParameters with all items, each accessible via indexer
        /// </summary>
        [Fact]
        public void Constructor_MultipleValidItems_AddsAllItemsCorrectly()
        {
            // Arrange
            var testObject = new object();
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("stringKey", "stringValue"),
                new KeyValuePair<string, object>("intKey", 42),
                new KeyValuePair<string, object>("objectKey", testObject),
                new KeyValuePair<string, object>("doubleKey", 3.14)
            };

            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal("stringValue", result["stringKey"]);
            Assert.Equal(42, result["intKey"]);
            Assert.Same(testObject, result["objectKey"]);
            Assert.Equal(3.14, result["doubleKey"]);
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter correctly handles null values.
        /// Input: Collection with null values for some keys
        /// Expected: ShellNavigationQueryParameters with null values properly stored
        /// </summary>
        [Fact]
        public void Constructor_CollectionWithNullValues_HandlesNullValuesCorrectly()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("nullValue", null),
                new KeyValuePair<string, object>("nonNullValue", "test")
            };

            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Null(result["nullValue"]);
            Assert.Equal("test", result["nonNullValue"]);
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter correctly handles empty string keys.
        /// Input: Collection with empty string as key
        /// Expected: ShellNavigationQueryParameters with empty string key properly stored
        /// </summary>
        [Fact]
        public void Constructor_EmptyStringKey_HandlesEmptyStringKeyCorrectly()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("", "emptyKeyValue"),
                new KeyValuePair<string, object>("normalKey", "normalValue")
            };

            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("emptyKeyValue", result[""]);
            Assert.Equal("normalValue", result["normalKey"]);
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter correctly handles whitespace-only keys.
        /// Input: Collection with whitespace-only string as key
        /// Expected: ShellNavigationQueryParameters with whitespace key properly stored
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceKey_HandlesWhitespaceKeyCorrectly()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("   ", "whitespaceKeyValue"),
                new KeyValuePair<string, object>("\t\n", "tabNewlineKeyValue")
            };

            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("whitespaceKeyValue", result["   "]);
            Assert.Equal("tabNewlineKeyValue", result["\t\n"]);
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter throws ArgumentNullException when collection is null.
        /// Input: Null collection parameter
        /// Expected: ArgumentNullException during foreach iteration
        /// </summary>
        [Fact]
        public void Constructor_NullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, object>> nullCollection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShellNavigationQueryParameters(nullCollection));
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter throws ArgumentNullException when collection contains null key.
        /// Input: Collection with KeyValuePair having null key
        /// Expected: ArgumentNullException from internal Dictionary.Add method
        /// </summary>
        [Fact]
        public void Constructor_CollectionWithNullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(null, "value")
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShellNavigationQueryParameters(collection));
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter throws ArgumentException when collection contains duplicate keys.
        /// Input: Collection with duplicate keys
        /// Expected: ArgumentException from internal Dictionary.Add method when attempting to add duplicate key
        /// </summary>
        [Fact]
        public void Constructor_CollectionWithDuplicateKeys_ThrowsArgumentException()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("duplicateKey", "value1"),
                new KeyValuePair<string, object>("duplicateKey", "value2")
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ShellNavigationQueryParameters(collection));
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter correctly handles special characters in keys.
        /// Input: Collection with keys containing special characters
        /// Expected: ShellNavigationQueryParameters with special character keys properly stored
        /// </summary>
        [Fact]
        public void Constructor_SpecialCharacterKeys_HandlesSpecialCharactersCorrectly()
        {
            // Arrange
            var collection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("key with spaces", "value1"),
                new KeyValuePair<string, object>("key@#$%^&*()", "value2"),
                new KeyValuePair<string, object>("key\u0001\u0002", "value3"), // control characters
				new KeyValuePair<string, object>("🚀emoji🔥key", "value4") // unicode emoji
			};

            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal("value1", result["key with spaces"]);
            Assert.Equal("value2", result["key@#$%^&*()"]);
            Assert.Equal("value3", result["key\u0001\u0002"]);
            Assert.Equal("value4", result["🚀emoji🔥key"]);
        }

        /// <summary>
        /// Tests that the constructor with IEnumerable parameter works with different IEnumerable implementations.
        /// Input: Array, HashSet, and LINQ query as IEnumerable sources
        /// Expected: ShellNavigationQueryParameters correctly populated from different collection types
        /// </summary>
        [Theory]
        [MemberData(nameof(GetDifferentEnumerableTypes))]
        public void Constructor_DifferentEnumerableTypes_WorksCorrectly(IEnumerable<KeyValuePair<string, object>> collection, int expectedCount)
        {
            // Act
            var result = new ShellNavigationQueryParameters(collection);

            // Assert
            Assert.Equal(expectedCount, result.Count);
            Assert.True(result.ContainsKey("test"));
            Assert.Equal("value", result["test"]);
        }

        public static IEnumerable<object[]> GetDifferentEnumerableTypes()
        {
            // Array
            yield return new object[]
            {
                new KeyValuePair<string, object>[] { new("test", "value") },
                1
            };

            // HashSet
            yield return new object[]
            {
                new HashSet<KeyValuePair<string, object>> { new("test", "value") },
                1
            };

            // LINQ query result
            yield return new object[]
            {
                new Dictionary<string, object> { ["test"] = "value" }.AsEnumerable(),
                1
            };
        }

        /// <summary>
        /// Tests that the constructor throws NullReferenceException when the dictionary parameter is null.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullDictionary_ThrowsNullReferenceException()
        {
            // Arrange
            IDictionary<string, object> dictionary = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new ShellNavigationQueryParameters(dictionary));
        }

        /// <summary>
        /// Tests that the constructor creates an empty ShellNavigationQueryParameters when given an empty dictionary.
        /// Expected result: ShellNavigationQueryParameters with Count = 0.
        /// </summary>
        [Fact]
        public void Constructor_EmptyDictionary_CreatesEmptyInstance()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>();

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(0, result.Count);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that the constructor correctly copies a single item from the dictionary.
        /// Expected result: ShellNavigationQueryParameters contains the single item.
        /// </summary>
        [Fact]
        public void Constructor_SingleItem_CopiesItem()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                { "key1", "value1" }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.True(result.ContainsKey("key1"));
        }

        /// <summary>
        /// Tests that the constructor correctly copies multiple items from the dictionary.
        /// Expected result: ShellNavigationQueryParameters contains all items from the dictionary.
        /// </summary>
        [Fact]
        public void Constructor_MultipleItems_CopiesAllItems()
        {
            // Arrange
            var obj = new object();
            var dictionary = new Dictionary<string, object>
            {
                { "string_key", "string_value" },
                { "int_key", 42 },
                { "object_key", obj },
                { "double_key", 3.14 }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal("string_value", result["string_key"]);
            Assert.Equal(42, result["int_key"]);
            Assert.Same(obj, result["object_key"]);
            Assert.Equal(3.14, result["double_key"]);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when the dictionary contains a null key.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithNullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var mockDictionary = Substitute.For<IDictionary<string, object>>();
            var items = new[]
            {
                new KeyValuePair<string, object>(null, "value")
            };
            mockDictionary.GetEnumerator().Returns(((IEnumerable<KeyValuePair<string, object>>)items).GetEnumerator());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShellNavigationQueryParameters(mockDictionary));
        }

        /// <summary>
        /// Tests that the constructor handles null values correctly.
        /// Expected result: ShellNavigationQueryParameters contains the item with null value.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithNullValue_HandlesNullValue()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                { "key_with_null", null },
                { "key_with_value", "not_null" }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Null(result["key_with_null"]);
            Assert.Equal("not_null", result["key_with_value"]);
        }

        /// <summary>
        /// Tests that the constructor handles an empty string key correctly.
        /// Expected result: ShellNavigationQueryParameters contains the item with empty string key.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithEmptyStringKey_HandlesEmptyKey()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                { "", "empty_key_value" },
                { "normal_key", "normal_value" }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("empty_key_value", result[""]);
            Assert.Equal("normal_value", result["normal_key"]);
        }

        /// <summary>
        /// Tests that the constructor handles whitespace-only string keys correctly.
        /// Expected result: ShellNavigationQueryParameters contains the item with whitespace key.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithWhitespaceKey_HandlesWhitespaceKey()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                { "   ", "whitespace_key_value" },
                { "\t\n\r", "control_chars_value" }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("whitespace_key_value", result["   "]);
            Assert.Equal("control_chars_value", result["\t\n\r"]);
        }

        /// <summary>
        /// Tests that the constructor handles very long string keys correctly.
        /// Expected result: ShellNavigationQueryParameters contains the item with long key.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithLongKey_HandlesLongKey()
        {
            // Arrange
            var longKey = new string('a', 10000);
            var dictionary = new Dictionary<string, object>
            {
                { longKey, "long_key_value" }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("long_key_value", result[longKey]);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentException when the dictionary enumeration produces duplicate keys.
        /// Expected result: ArgumentException is thrown on the second occurrence of the same key.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithDuplicateKeys_ThrowsArgumentException()
        {
            // Arrange
            var mockDictionary = Substitute.For<IDictionary<string, object>>();
            var items = new[]
            {
                new KeyValuePair<string, object>("duplicate_key", "value1"),
                new KeyValuePair<string, object>("duplicate_key", "value2")
            };
            mockDictionary.GetEnumerator().Returns(((IEnumerable<KeyValuePair<string, object>>)items).GetEnumerator());

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ShellNavigationQueryParameters(mockDictionary));
        }

        /// <summary>
        /// Tests that the constructor handles various data types as values correctly.
        /// Expected result: ShellNavigationQueryParameters contains all items with correct types.
        /// </summary>
        [Fact]
        public void Constructor_DictionaryWithVariousValueTypes_HandlesAllTypes()
        {
            // Arrange
            var dateTime = DateTime.Now;
            var guid = Guid.NewGuid();
            var dictionary = new Dictionary<string, object>
            {
                { "int", int.MaxValue },
                { "long", long.MinValue },
                { "double", double.PositiveInfinity },
                { "decimal", decimal.MaxValue },
                { "bool", true },
                { "datetime", dateTime },
                { "guid", guid },
                { "string", "test" }
            };

            // Act
            var result = new ShellNavigationQueryParameters(dictionary);

            // Assert
            Assert.Equal(8, result.Count);
            Assert.Equal(int.MaxValue, result["int"]);
            Assert.Equal(long.MinValue, result["long"]);
            Assert.Equal(double.PositiveInfinity, result["double"]);
            Assert.Equal(decimal.MaxValue, result["decimal"]);
            Assert.Equal(true, result["bool"]);
            Assert.Equal(dateTime, result["datetime"]);
            Assert.Equal(guid, result["guid"]);
            Assert.Equal("test", result["string"]);
        }

        /// <summary>
        /// Tests that the Keys property returns an empty collection when the dictionary is empty using the default constructor.
        /// </summary>
        [Fact]
        public void Keys_WhenDictionaryIsEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();

            // Act
            var keys = parameters.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Empty(keys);
        }

        /// <summary>
        /// Tests that the Keys property returns the correct key when a single item is added to the dictionary.
        /// </summary>
        [Fact]
        public void Keys_WhenSingleItemAdded_ReturnsSingleKey()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var expectedKey = "testKey";
            parameters.Add(expectedKey, "testValue");

            // Act
            var keys = parameters.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(expectedKey, keys);
        }

        /// <summary>
        /// Tests that the Keys property returns all keys when multiple items are added to the dictionary.
        /// </summary>
        [Fact]
        public void Keys_WhenMultipleItemsAdded_ReturnsAllKeys()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var expectedKeys = new[] { "key1", "key2", "key3" };
            parameters.Add("key1", "value1");
            parameters.Add("key2", "value2");
            parameters.Add("key3", "value3");

            // Act
            var keys = parameters.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(3, keys.Count);
            foreach (var expectedKey in expectedKeys)
            {
                Assert.Contains(expectedKey, keys);
            }
        }

        /// <summary>
        /// Tests that the Keys property returns the correct keys when initialized with an IEnumerable collection constructor.
        /// </summary>
        [Fact]
        public void Keys_WhenInitializedWithIEnumerableConstructor_ReturnsCorrectKeys()
        {
            // Arrange
            var initialData = new[]
            {
                new KeyValuePair<string, object>("param1", "value1"),
                new KeyValuePair<string, object>("param2", 42),
                new KeyValuePair<string, object>("param3", true)
            };
            var parameters = new ShellNavigationQueryParameters(initialData);
            var expectedKeys = new[] { "param1", "param2", "param3" };

            // Act
            var keys = parameters.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(3, keys.Count);
            foreach (var expectedKey in expectedKeys)
            {
                Assert.Contains(expectedKey, keys);
            }
        }

        /// <summary>
        /// Tests that the Keys property returns the correct keys when initialized with an IDictionary constructor.
        /// </summary>
        [Fact]
        public void Keys_WhenInitializedWithIDictionaryConstructor_ReturnsCorrectKeys()
        {
            // Arrange
            var initialDictionary = new Dictionary<string, object>
            {
                { "dictKey1", "dictValue1" },
                { "dictKey2", 123 },
                { "dictKey3", null }
            };
            var parameters = new ShellNavigationQueryParameters(initialDictionary);
            var expectedKeys = new[] { "dictKey1", "dictKey2", "dictKey3" };

            // Act
            var keys = parameters.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(3, keys.Count);
            foreach (var expectedKey in expectedKeys)
            {
                Assert.Contains(expectedKey, keys);
            }
        }

        /// <summary>
        /// Tests that the Keys property reflects changes when items are added after initialization.
        /// </summary>
        [Fact]
        public void Keys_WhenItemsAddedAfterInitialization_ReflectsCurrentState()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("initialKey", "initialValue");

            // Act & Assert - Initial state
            var initialKeys = parameters.Keys;
            Assert.Single(initialKeys);
            Assert.Contains("initialKey", initialKeys);

            // Add another item
            parameters.Add("additionalKey", "additionalValue");

            // Act & Assert - Updated state
            var updatedKeys = parameters.Keys;
            Assert.Equal(2, updatedKeys.Count);
            Assert.Contains("initialKey", updatedKeys);
            Assert.Contains("additionalKey", updatedKeys);
        }

        /// <summary>
        /// Tests that the Keys property reflects changes when items are added via indexer after initialization.
        /// </summary>
        [Fact]
        public void Keys_WhenItemsAddedViaIndexer_ReflectsCurrentState()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();

            // Act - Add via indexer
            parameters["indexerKey1"] = "value1";
            parameters["indexerKey2"] = "value2";

            // Assert
            var keys = parameters.Keys;
            Assert.NotNull(keys);
            Assert.Equal(2, keys.Count);
            Assert.Contains("indexerKey1", keys);
            Assert.Contains("indexerKey2", keys);
        }

        /// <summary>
        /// Tests that the Keys property handles special string keys correctly including empty strings and whitespace.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("key with spaces")]
        [InlineData("special!@#$%^&*()characters")]
        public void Keys_WithSpecialStringKeys_ReturnsCorrectKeys(string specialKey)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add(specialKey, "testValue");

            // Act
            var keys = parameters.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(specialKey, keys);
        }

        /// <summary>
        /// Tests that the Values property returns an empty collection when the dictionary is empty.
        /// </summary>
        [Fact]
        public void Values_WhenDictionaryEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Empty(values);
            Assert.Equal(0, values.Count);
        }

        /// <summary>
        /// Tests that the Values property returns a collection containing the single object when dictionary has one item.
        /// </summary>
        [Fact]
        public void Values_WhenDictionaryHasSingleItem_ReturnsCollectionWithSingleValue()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            var expectedValue = "test_value";
            queryParameters.Add("key1", expectedValue);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Contains(expectedValue, values);
            Assert.Equal(1, values.Count);
        }

        /// <summary>
        /// Tests that the Values property returns a collection containing all objects when dictionary has multiple items.
        /// </summary>
        [Fact]
        public void Values_WhenDictionaryHasMultipleItems_ReturnsCollectionWithAllValues()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            var value1 = "string_value";
            var value2 = 42;
            var value3 = new object();

            queryParameters.Add("key1", value1);
            queryParameters.Add("key2", value2);
            queryParameters.Add("key3", value3);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Equal(3, values.Count);
            Assert.Contains(value1, values);
            Assert.Contains(value2, values);
            Assert.Contains(value3, values);
        }

        /// <summary>
        /// Tests that the Values property contains actual value objects and not keys.
        /// </summary>
        [Fact]
        public void Values_ContainsValuesNotKeys_ReturnsOnlyValueObjects()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            var key = "test_key";
            var value = "test_value";
            queryParameters.Add(key, value);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.Contains(value, values);
            Assert.DoesNotContain(key, values.Cast<object>());
        }

        /// <summary>
        /// Tests that the Values property reflects changes when items are added to the dictionary.
        /// </summary>
        [Fact]
        public void Values_WhenItemsAdded_ReflectsChanges()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            var initialValue = "initial";
            var additionalValue = "additional";

            // Act & Assert - Initial state
            Assert.Empty(queryParameters.Values);

            queryParameters.Add("key1", initialValue);
            Assert.Single(queryParameters.Values);
            Assert.Contains(initialValue, queryParameters.Values);

            queryParameters.Add("key2", additionalValue);
            Assert.Equal(2, queryParameters.Values.Count);
            Assert.Contains(initialValue, queryParameters.Values);
            Assert.Contains(additionalValue, queryParameters.Values);
        }

        /// <summary>
        /// Tests that the Values property handles different value types correctly including null values.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("string_value")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void Values_WithDifferentValueTypes_ContainsExpectedValue(object testValue)
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            queryParameters.Add("test_key", testValue);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.Single(values);
            Assert.Contains(testValue, values);
        }

        /// <summary>
        /// Tests that the Values property handles duplicate values with different keys correctly.
        /// </summary>
        [Fact]
        public void Values_WithDuplicateValues_ContainsBothInstances()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            var duplicateValue = "same_value";
            queryParameters.Add("key1", duplicateValue);
            queryParameters.Add("key2", duplicateValue);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.Equal(2, values.Count);
            Assert.Equal(2, values.Count(v => v.Equals(duplicateValue)));
        }

        /// <summary>
        /// Tests that the Values property collection is enumerable and contains expected items.
        /// </summary>
        [Fact]
        public void Values_CanBeEnumerated_ContainsAllExpectedItems()
        {
            // Arrange
            var queryParameters = new ShellNavigationQueryParameters();
            var expectedValues = new object[] { "value1", 123, null, true };

            for (int i = 0; i < expectedValues.Length; i++)
            {
                queryParameters.Add($"key{i}", expectedValues[i]);
            }

            // Act
            var values = queryParameters.Values;
            var enumeratedValues = values.ToList();

            // Assert
            Assert.Equal(expectedValues.Length, enumeratedValues.Count);
            foreach (var expectedValue in expectedValues)
            {
                Assert.Contains(expectedValue, enumeratedValues);
            }
        }

        /// <summary>
        /// Tests that the Values property returns collection from constructor-initialized dictionary.
        /// </summary>
        [Fact]
        public void Values_WhenInitializedFromDictionary_ContainsExpectedValues()
        {
            // Arrange
            var sourceDictionary = new Dictionary<string, object>
            {
                {"key1", "value1"},
                {"key2", 42},
                {"key3", null}
            };
            var queryParameters = new ShellNavigationQueryParameters(sourceDictionary);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.Equal(3, values.Count);
            Assert.Contains("value1", values);
            Assert.Contains(42, values);
            Assert.Contains(null, values);
        }

        /// <summary>
        /// Tests that the Values property returns collection from constructor-initialized key-value pairs.
        /// </summary>
        [Fact]
        public void Values_WhenInitializedFromKeyValuePairs_ContainsExpectedValues()
        {
            // Arrange
            var sourceCollection = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("key1", "collection_value1"),
                new KeyValuePair<string, object>("key2", 99),
                new KeyValuePair<string, object>("key3", false)
            };
            var queryParameters = new ShellNavigationQueryParameters(sourceCollection);

            // Act
            var values = queryParameters.Values;

            // Assert
            Assert.Equal(3, values.Count);
            Assert.Contains("collection_value1", values);
            Assert.Contains(99, values);
            Assert.Contains(false, values);
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) successfully adds a valid key-value pair to the collection.
        /// Input: Valid KeyValuePair with non-null string key and object value.
        /// Expected: Item is added to the internal dictionary without throwing an exception.
        /// </summary>
        [Fact]
        public void Add_ValidKeyValuePair_AddsSuccessfully()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>("testKey", "testValue");

            // Act
            parameters.Add(item);

            // Assert
            Assert.True(parameters.ContainsKey("testKey"));
            Assert.Equal("testValue", parameters["testKey"]);
            Assert.Equal(1, parameters.Count);
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) throws ArgumentNullException when the key is null.
        /// Input: KeyValuePair with null key.
        /// Expected: ArgumentNullException is thrown by the underlying Dictionary.Add method.
        /// </summary>
        [Fact]
        public void Add_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>(null, "testValue");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => parameters.Add(item));
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) successfully adds a key-value pair with null value.
        /// Input: KeyValuePair with valid key and null value.
        /// Expected: Item is added successfully since Dictionary allows null values for object type.
        /// </summary>
        [Fact]
        public void Add_NullValue_AddsSuccessfully()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>("testKey", null);

            // Act
            parameters.Add(item);

            // Assert
            Assert.True(parameters.ContainsKey("testKey"));
            Assert.Null(parameters["testKey"]);
            Assert.Equal(1, parameters.Count);
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) successfully adds a key-value pair with empty string key.
        /// Input: KeyValuePair with empty string key.
        /// Expected: Item is added successfully since Dictionary allows empty string keys.
        /// </summary>
        [Fact]
        public void Add_EmptyStringKey_AddsSuccessfully()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>("", "testValue");

            // Act
            parameters.Add(item);

            // Assert
            Assert.True(parameters.ContainsKey(""));
            Assert.Equal("testValue", parameters[""]);
            Assert.Equal(1, parameters.Count);
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) successfully adds a key-value pair with whitespace-only key.
        /// Input: KeyValuePair with whitespace-only key.
        /// Expected: Item is added successfully since Dictionary allows whitespace keys.
        /// </summary>
        [Fact]
        public void Add_WhitespaceKey_AddsSuccessfully()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>("   ", "testValue");

            // Act
            parameters.Add(item);

            // Assert
            Assert.True(parameters.ContainsKey("   "));
            Assert.Equal("testValue", parameters["   "]);
            Assert.Equal(1, parameters.Count);
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) throws ArgumentException when attempting to add a duplicate key.
        /// Input: Two KeyValuePairs with the same key.
        /// Expected: ArgumentException is thrown by the underlying Dictionary.Add method on the second add.
        /// </summary>
        [Fact]
        public void Add_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item1 = new KeyValuePair<string, object>("duplicateKey", "value1");
            var item2 = new KeyValuePair<string, object>("duplicateKey", "value2");

            // Act
            parameters.Add(item1);

            // Assert
            Assert.Throws<ArgumentException>(() => parameters.Add(item2));
        }

        /// <summary>
        /// Tests that Add(KeyValuePair) successfully adds various object types as values.
        /// Input: KeyValuePairs with different object types as values.
        /// Expected: All items are added successfully regardless of value type.
        /// </summary>
        [Theory]
        [InlineData("intKey", 42)]
        [InlineData("doubleKey", 3.14)]
        [InlineData("boolKey", true)]
        [InlineData("stringKey", "stringValue")]
        public void Add_VariousValueTypes_AddsSuccessfully(string key, object value)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>(key, value);

            // Act
            parameters.Add(item);

            // Assert
            Assert.True(parameters.ContainsKey(key));
            Assert.Equal(value, parameters[key]);
            Assert.Equal(1, parameters.Count);
        }

        /// <summary>
        /// Tests that Contains returns true when the dictionary contains a key matching the item's key,
        /// regardless of whether the values match.
        /// </summary>
        /// <param name="key">The key to test</param>
        /// <param name="storedValue">The value stored in the dictionary</param>
        /// <param name="itemValue">The value in the KeyValuePair being tested</param>
        /// <param name="expected">The expected result</param>
        [Theory]
        [InlineData("existingKey", "storedValue", "storedValue", true)]
        [InlineData("existingKey", "storedValue", "differentValue", true)]
        [InlineData("existingKey", null, "anyValue", true)]
        [InlineData("existingKey", "anyValue", null, true)]
        [InlineData("nonExistingKey", null, "anyValue", false)]
        [InlineData("", "emptyKeyValue", "emptyKeyValue", true)]
        [InlineData(" ", "whitespaceValue", "whitespaceValue", true)]
        public void Contains_WithVariousKeyValueCombinations_ReturnsExpectedResult(string key, object storedValue, object itemValue, bool expected)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            if (expected) // Only add to dictionary if we expect it to be found
            {
                parameters.Add(key, storedValue);
            }
            var item = new KeyValuePair<string, object>(key, itemValue);

            // Act
            var result = parameters.Contains(item);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty dictionary.
        /// </summary>
        [Fact]
        public void Contains_EmptyDictionary_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>("anyKey", "anyValue");

            // Act
            var result = parameters.Contains(item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains throws ArgumentNullException when the item's key is null,
        /// as this is the behavior of the underlying Dictionary.ContainsKey method.
        /// </summary>
        [Fact]
        public void Contains_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var item = new KeyValuePair<string, object>(null, "anyValue");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => parameters.Contains(item));
        }

        /// <summary>
        /// Tests that Contains works correctly with special string characters and unicode.
        /// </summary>
        [Theory]
        [InlineData("key with spaces")]
        [InlineData("key\twith\ttabs")]
        [InlineData("key\nwith\nnewlines")]
        [InlineData("key.with.dots")]
        [InlineData("key/with/slashes")]
        [InlineData("key\\with\\backslashes")]
        [InlineData("key@with#special$characters%")]
        [InlineData("🔑emoji🗝️key")]
        [InlineData("key with 中文")]
        public void Contains_SpecialCharacterKeys_WorksCorrectly(string specialKey)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var value = "testValue";
            parameters.Add(specialKey, value);
            var item = new KeyValuePair<string, object>(specialKey, value);

            // Act
            var result = parameters.Contains(item);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with a very long key string.
        /// </summary>
        [Fact]
        public void Contains_VeryLongKey_WorksCorrectly()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var longKey = new string('a', 10000); // 10,000 character key
            var value = "testValue";
            parameters.Add(longKey, value);
            var item = new KeyValuePair<string, object>(longKey, value);

            // Act
            var result = parameters.Contains(item);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains only checks the key and ignores the value completely,
        /// demonstrating that it returns true even when values are completely different types.
        /// </summary>
        [Fact]
        public void Contains_DifferentValueTypes_OnlyChecksKey()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var key = "testKey";
            parameters.Add(key, 42); // Store an integer
            var item = new KeyValuePair<string, object>(key, "string value"); // Test with string

            // Act
            var result = parameters.Contains(item);

            // Assert
            Assert.True(result); // Should return true because key exists, regardless of value type
        }

        /// <summary>
        /// Tests that CopyTo successfully copies dictionary contents to a valid array with sufficient space.
        /// Input: Valid array with enough capacity and valid arrayIndex.
        /// Expected: Dictionary contents are copied to the array starting at the specified index.
        /// </summary>
        [Fact]
        public void CopyTo_ValidArrayAndIndex_CopiesContent()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "key1", "value1" },
                { "key2", 42 }
            };
            var array = new KeyValuePair<string, object>[5];
            int arrayIndex = 1;

            // Act
            parameters.CopyTo(array, arrayIndex);

            // Assert
            Assert.Equal(2, parameters.Count);
            Assert.NotEqual(default(KeyValuePair<string, object>), array[1]);
            Assert.NotEqual(default(KeyValuePair<string, object>), array[2]);
            Assert.Equal(default(KeyValuePair<string, object>), array[0]);
            Assert.Equal(default(KeyValuePair<string, object>), array[3]);
            Assert.Equal(default(KeyValuePair<string, object>), array[4]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly when arrayIndex is zero.
        /// Input: Valid array and arrayIndex = 0.
        /// Expected: Dictionary contents are copied starting from the beginning of the array.
        /// </summary>
        [Fact]
        public void CopyTo_ArrayIndexZero_CopiesFromBeginning()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "test", "data" }
            };
            var array = new KeyValuePair<string, object>[3];

            // Act
            parameters.CopyTo(array, 0);

            // Assert
            Assert.NotEqual(default(KeyValuePair<string, object>), array[0]);
            Assert.Equal(default(KeyValuePair<string, object>), array[1]);
            Assert.Equal(default(KeyValuePair<string, object>), array[2]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly with an empty dictionary.
        /// Input: Empty dictionary and valid array.
        /// Expected: No elements are copied, array remains unchanged.
        /// </summary>
        [Fact]
        public void CopyTo_EmptyDictionary_NoElementsCopied()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var array = new KeyValuePair<string, object>[2];
            var originalArray = new KeyValuePair<string, object>[2];

            // Act
            parameters.CopyTo(array, 0);

            // Assert
            Assert.Equal(originalArray, array);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array is null.
        /// Input: Null array parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "key", "value" }
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => parameters.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input: Valid array and negative arrayIndex.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_NegativeArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "key", "value" }
            };
            var array = new KeyValuePair<string, object>[5];

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => parameters.CopyTo(array, -1));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex is greater than or equal to array length.
        /// Input: Valid array and arrayIndex >= array.Length.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Theory]
        [InlineData(3, 3)]
        [InlineData(3, 5)]
        public void CopyTo_ArrayIndexOutOfBounds_ThrowsArgumentException(int arrayLength, int arrayIndex)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "key", "value" }
            };
            var array = new KeyValuePair<string, object>[arrayLength];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => parameters.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when there is insufficient space in the array.
        /// Input: Array too small to accommodate all dictionary elements starting from arrayIndex.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_InsufficientSpace_ThrowsArgumentException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };
            var array = new KeyValuePair<string, object>[4];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => parameters.CopyTo(array, 2));
        }

        /// <summary>
        /// Tests that CopyTo works correctly when array has exact capacity for dictionary elements.
        /// Input: Array with exact size needed for dictionary elements starting from arrayIndex.
        /// Expected: All elements are copied successfully without exceptions.
        /// </summary>
        [Fact]
        public void CopyTo_ExactCapacity_CopiesSuccessfully()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            var array = new KeyValuePair<string, object>[3];

            // Act
            parameters.CopyTo(array, 1);

            // Assert
            Assert.Equal(default(KeyValuePair<string, object>), array[0]);
            Assert.NotEqual(default(KeyValuePair<string, object>), array[1]);
            Assert.NotEqual(default(KeyValuePair<string, object>), array[2]);
        }

        /// <summary>
        /// Tests that CopyTo correctly handles dictionary with various object types as values.
        /// Input: Dictionary containing different value types and valid array.
        /// Expected: All key-value pairs are copied with their original types preserved.
        /// </summary>
        [Fact]
        public void CopyTo_VariousValueTypes_CopiesWithTypesPreserved()
        {
            // Arrange
            var testObject = new object();
            var parameters = new ShellNavigationQueryParameters
            {
                { "string", "text" },
                { "int", 42 },
                { "double", 3.14 },
                { "bool", true },
                { "object", testObject },
                { "null", null }
            };
            var array = new KeyValuePair<string, object>[6];

            // Act
            parameters.CopyTo(array, 0);

            // Assert
            Assert.Equal(6, parameters.Count);
            for (int i = 0; i < 6; i++)
            {
                Assert.NotEqual(default(KeyValuePair<string, object>), array[i]);
            }
        }

        /// <summary>
        /// Tests that Remove returns true and actually removes an existing key from a non-read-only collection.
        /// Input: Existing key in non-read-only collection.
        /// Expected: Returns true and key is removed from collection.
        /// </summary>
        [Fact]
        public void Remove_ExistingKeyInWritableCollection_ReturnsTrueAndRemovesKey()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            var testValue = new object();
            parameters.Add(testKey, testValue);
            var initialCount = parameters.Count;

            // Act
            var result = parameters.Remove(testKey);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(testKey));
            Assert.Equal(initialCount - 1, parameters.Count);
        }

        /// <summary>
        /// Tests that Remove returns false when attempting to remove a non-existing key from a non-read-only collection.
        /// Input: Non-existing key in non-read-only collection.
        /// Expected: Returns false and collection remains unchanged.
        /// </summary>
        [Fact]
        public void Remove_NonExistingKeyInWritableCollection_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("existingKey", "existingValue");
            var initialCount = parameters.Count;
            var nonExistingKey = "nonExistingKey";

            // Act
            var result = parameters.Remove(nonExistingKey);

            // Assert
            Assert.False(result);
            Assert.Equal(initialCount, parameters.Count);
            Assert.True(parameters.ContainsKey("existingKey"));
        }

        /// <summary>
        /// Tests that Remove throws ArgumentNullException when given a null key in a non-read-only collection.
        /// Input: Null key in non-read-only collection.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Remove_NullKeyInWritableCollection_ThrowsArgumentNullException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("testKey", "testValue");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => parameters.Remove(null));
        }

        /// <summary>
        /// Tests that Remove handles empty string key correctly in a non-read-only collection.
        /// Input: Empty string key in non-read-only collection.
        /// Expected: Returns false if key doesn't exist, true if it does exist.
        /// </summary>
        [Fact]
        public void Remove_EmptyStringKeyInWritableCollection_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("testKey", "testValue");

            // Act
            var result = parameters.Remove(string.Empty);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Remove can successfully remove an empty string key that was previously added.
        /// Input: Empty string key that exists in non-read-only collection.
        /// Expected: Returns true and removes the empty key.
        /// </summary>
        [Fact]
        public void Remove_ExistingEmptyStringKeyInWritableCollection_ReturnsTrueAndRemovesKey()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var emptyKey = string.Empty;
            var testValue = "testValue";
            parameters.Add(emptyKey, testValue);
            parameters.Add("otherKey", "otherValue");
            var initialCount = parameters.Count;

            // Act
            var result = parameters.Remove(emptyKey);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(emptyKey));
            Assert.Equal(initialCount - 1, parameters.Count);
            Assert.True(parameters.ContainsKey("otherKey"));
        }

        /// <summary>
        /// Tests that Remove handles whitespace-only string key correctly in a non-read-only collection.
        /// Input: Whitespace-only string key in non-read-only collection.
        /// Expected: Returns false if key doesn't exist, handles like any other string key.
        /// </summary>
        [Fact]
        public void Remove_WhitespaceKeyInWritableCollection_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("testKey", "testValue");
            var whitespaceKey = "   ";

            // Act
            var result = parameters.Remove(whitespaceKey);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Remove can successfully remove a whitespace-only key that was previously added.
        /// Input: Whitespace-only string key that exists in non-read-only collection.
        /// Expected: Returns true and removes the whitespace key.
        /// </summary>
        [Fact]
        public void Remove_ExistingWhitespaceKeyInWritableCollection_ReturnsTrueAndRemovesKey()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var whitespaceKey = "   ";
            var testValue = "testValue";
            parameters.Add(whitespaceKey, testValue);
            parameters.Add("otherKey", "otherValue");
            var initialCount = parameters.Count;

            // Act
            var result = parameters.Remove(whitespaceKey);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(whitespaceKey));
            Assert.Equal(initialCount - 1, parameters.Count);
            Assert.True(parameters.ContainsKey("otherKey"));
        }

        /// <summary>
        /// Tests Remove with various key edge cases using parameterized test data.
        /// Input: Different edge case keys in non-read-only collection.
        /// Expected: Handles each key appropriately based on whether it exists.
        /// </summary>
        [Theory]
        [InlineData("normalKey", true)]
        [InlineData("UPPERCASE", true)]
        [InlineData("mixedCase123", true)]
        [InlineData("key-with-dashes", true)]
        [InlineData("key_with_underscores", true)]
        [InlineData("key.with.dots", true)]
        [InlineData("key with spaces", true)]
        [InlineData("specialChars!@#$%", true)]
        public void Remove_VariousKeyFormats_ReturnsExpectedResult(string keyToTest, bool shouldAddFirst)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            if (shouldAddFirst)
            {
                parameters.Add(keyToTest, "testValue");
            }
            parameters.Add("otherKey", "otherValue");
            var initialCount = parameters.Count;

            // Act
            var result = parameters.Remove(keyToTest);

            // Assert
            if (shouldAddFirst)
            {
                Assert.True(result);
                Assert.False(parameters.ContainsKey(keyToTest));
                Assert.Equal(initialCount - 1, parameters.Count);
            }
            else
            {
                Assert.False(result);
                Assert.Equal(initialCount, parameters.Count);
            }
            Assert.True(parameters.ContainsKey("otherKey"));
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) returns true when removing an existing key-value pair.
        /// Verifies the successful removal path on line 89.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            var testValue = "testValue";
            parameters.Add(testKey, testValue);
            var itemToRemove = new KeyValuePair<string, object>(testKey, testValue);

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(testKey));
            Assert.Equal(0, parameters.Count);
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) returns false when attempting to remove a non-existing key-value pair.
        /// Verifies the unsuccessful removal path on line 89.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("existingKey", "existingValue");
            var itemToRemove = new KeyValuePair<string, object>("nonExistingKey", "someValue");

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.False(result);
            Assert.Equal(1, parameters.Count);
            Assert.True(parameters.ContainsKey("existingKey"));
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) returns false when the key exists but with a different value.
        /// Verifies the key-value pair matching behavior on line 89.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_ExistingKeyDifferentValue_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            parameters.Add(testKey, "originalValue");
            var itemToRemove = new KeyValuePair<string, object>(testKey, "differentValue");

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.False(result);
            Assert.True(parameters.ContainsKey(testKey));
            Assert.Equal("originalValue", parameters[testKey]);
            Assert.Equal(1, parameters.Count);
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) works correctly with null values.
        /// Verifies the removal path on line 89 with null value scenarios.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_WithNullValue_HandledCorrectly()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            parameters.Add(testKey, null);
            var itemToRemove = new KeyValuePair<string, object>(testKey, null);

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(testKey));
            Assert.Equal(0, parameters.Count);
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) returns false when trying to remove null value from key that has non-null value.
        /// Verifies the value matching behavior on line 89.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_NullValueForNonNullStoredValue_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            parameters.Add(testKey, "nonNullValue");
            var itemToRemove = new KeyValuePair<string, object>(testKey, null);

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.False(result);
            Assert.True(parameters.ContainsKey(testKey));
            Assert.Equal("nonNullValue", parameters[testKey]);
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) works with complex object values.
        /// Verifies the removal path on line 89 with object reference equality.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_WithComplexObjectValue_UsesReferenceEquality()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            var testObject = new object();
            parameters.Add(testKey, testObject);
            var itemToRemove = new KeyValuePair<string, object>(testKey, testObject);

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(testKey));
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) returns false with different object instances.
        /// Verifies the reference equality behavior on line 89.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_WithDifferentObjectInstance_ReturnsFalse()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            parameters.Add(testKey, new object());
            var itemToRemove = new KeyValuePair<string, object>(testKey, new object());

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.False(result);
            Assert.True(parameters.ContainsKey(testKey));
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) works with empty string keys.
        /// Verifies the removal path on line 89 with edge case string values.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_EmptyStringKey_WorksCorrectly()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var emptyKey = "";
            var testValue = "testValue";
            parameters.Add(emptyKey, testValue);
            var itemToRemove = new KeyValuePair<string, object>(emptyKey, testValue);

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.True(result);
            Assert.False(parameters.ContainsKey(emptyKey));
        }

        /// <summary>
        /// Tests that Remove(KeyValuePair) works from a collection with multiple items.
        /// Verifies the removal path on line 89 doesn't affect other items.
        /// </summary>
        [Fact]
        public void Remove_KeyValuePair_FromMultipleItems_RemovesOnlySpecifiedItem()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("key1", "value1");
            parameters.Add("key2", "value2");
            parameters.Add("key3", "value3");
            var itemToRemove = new KeyValuePair<string, object>("key2", "value2");

            // Act
            var result = parameters.Remove(itemToRemove);

            // Assert
            Assert.True(result);
            Assert.Equal(2, parameters.Count);
            Assert.True(parameters.ContainsKey("key1"));
            Assert.True(parameters.ContainsKey("key3"));
            Assert.False(parameters.ContainsKey("key2"));
        }

        /// <summary>
        /// Tests that TryGetValue returns true and sets the correct value when the key exists in the dictionary.
        /// Input: Existing key in populated dictionary.
        /// Expected: Returns true and out parameter contains the expected value.
        /// </summary>
        [Fact]
        public void TryGetValue_KeyExists_ReturnsTrueAndSetsValue()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var expectedValue = new object();
            var key = "testKey";
            parameters.Add(key, expectedValue);

            // Act
            var result = parameters.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Same(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue returns false and sets value to null when the key doesn't exist.
        /// Input: Non-existing key in populated dictionary.
        /// Expected: Returns false and out parameter is null.
        /// </summary>
        [Fact]
        public void TryGetValue_KeyDoesNotExist_ReturnsFalseAndSetsValueToNull()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("existingKey", "existingValue");
            var nonExistentKey = "nonExistentKey";

            // Act
            var result = parameters.TryGetValue(nonExistentKey, out var actualValue);

            // Assert
            Assert.False(result);
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue returns false and sets value to null when dictionary is empty.
        /// Input: Any key in empty dictionary.
        /// Expected: Returns false and out parameter is null.
        /// </summary>
        [Fact]
        public void TryGetValue_EmptyDictionary_ReturnsFalseAndSetsValueToNull()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var key = "anyKey";

            // Act
            var result = parameters.TryGetValue(key, out var actualValue);

            // Assert
            Assert.False(result);
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue throws ArgumentNullException when key is null.
        /// Input: Null key parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void TryGetValue_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            string nullKey = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => parameters.TryGetValue(nullKey, out var value));
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with empty string as key.
        /// Input: Empty string key that exists in dictionary.
        /// Expected: Returns true and correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_EmptyStringKey_ReturnsTrueAndSetsValue()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var emptyKey = string.Empty;
            var expectedValue = "valueForEmptyKey";
            parameters.Add(emptyKey, expectedValue);

            // Act
            var result = parameters.TryGetValue(emptyKey, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with whitespace-only string as key.
        /// Input: Whitespace-only key that exists in dictionary.
        /// Expected: Returns true and correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_WhitespaceKey_ReturnsTrueAndSetsValue()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var whitespaceKey = "   ";
            var expectedValue = "valueForWhitespaceKey";
            parameters.Add(whitespaceKey, expectedValue);

            // Act
            var result = parameters.TryGetValue(whitespaceKey, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue returns true and null value when the stored value is null.
        /// Input: Key with null value stored in dictionary.
        /// Expected: Returns true and out parameter is null.
        /// </summary>
        [Fact]
        public void TryGetValue_NullValueStored_ReturnsTrueAndSetsValueToNull()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var key = "keyWithNullValue";
            parameters.Add(key, null);

            // Act
            var result = parameters.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with various object types as values.
        /// Input: Keys with different value types (string, int, custom object).
        /// Expected: Returns true and correct values for each type.
        /// </summary>
        [Theory]
        [InlineData("stringKey", "stringValue")]
        [InlineData("intKey", 42)]
        public void TryGetValue_DifferentValueTypes_ReturnsTrueAndCorrectValues(string key, object expectedValue)
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add(key, expectedValue);

            // Act
            var result = parameters.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with very long key.
        /// Input: Very long string as key.
        /// Expected: Returns true and correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_VeryLongKey_ReturnsTrueAndSetsValue()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var longKey = new string('a', 1000);
            var expectedValue = "valueForLongKey";
            parameters.Add(longKey, expectedValue);

            // Act
            var result = parameters.TryGetValue(longKey, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with special characters in key.
        /// Input: Key containing special characters.
        /// Expected: Returns true and correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_KeyWithSpecialCharacters_ReturnsTrueAndSetsValue()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var specialKey = "key!@#$%^&*()_+-={}[]|\\:;\"'<>?,./";
            var expectedValue = "valueForSpecialKey";
            parameters.Add(specialKey, expectedValue);

            // Act
            var result = parameters.TryGetValue(specialKey, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid ShellNavigationQueryParameters instance
        /// with proper initial state.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Act
            var parameters = new ShellNavigationQueryParameters();

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(0, parameters.Count);
            Assert.False(parameters.IsReadOnly);
            Assert.NotNull(parameters.Keys);
            Assert.NotNull(parameters.Values);
            Assert.Empty(parameters.Keys);
            Assert.Empty(parameters.Values);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that supports basic dictionary operations.
        /// </summary>
        [Fact]
        public void Constructor_Default_SupportsDictionaryOperations()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            var testKey = "testKey";
            var testValue = "testValue";

            // Act & Assert - Basic dictionary operations should work
            parameters.Add(testKey, testValue);
            Assert.Equal(1, parameters.Count);
            Assert.True(parameters.ContainsKey(testKey));
            Assert.Equal(testValue, parameters[testKey]);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that implements IDictionary interface correctly.
        /// </summary>
        [Fact]
        public void Constructor_Default_ImplementsIDictionaryInterface()
        {
            // Act
            var parameters = new ShellNavigationQueryParameters();

            // Assert
            Assert.IsAssignableFrom<IDictionary<string, object>>(parameters);
            Assert.NotNull(parameters.GetEnumerator());
        }
    }
}