#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for DataPackagePropertySet class.
    /// </summary>
    public partial class DataPackagePropertySetTests
    {
        /// <summary>
        /// Tests that Keys property returns an empty collection when the DataPackagePropertySet is newly constructed.
        /// </summary>
        [Fact]
        public void Keys_WhenEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();

            // Act
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Empty(keys);
        }

        /// <summary>
        /// Tests that Keys property returns a collection with one key when a single item is added.
        /// </summary>
        [Fact]
        public void Keys_WithSingleItem_ReturnsSingleKey()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testKey = "testKey";
            var testValue = "testValue";

            // Act
            propertySet.Add(testKey, testValue);
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(testKey, keys);
        }

        /// <summary>
        /// Tests that Keys property returns all keys when multiple items are added.
        /// </summary>
        [Fact]
        public void Keys_WithMultipleItems_ReturnsAllKeys()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testKeys = new[] { "key1", "key2", "key3" };
            var testValue = "value";

            foreach (var key in testKeys)
            {
                propertySet.Add(key, testValue);
            }

            // Act
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(testKeys.Length, keys.Count());
            foreach (var testKey in testKeys)
            {
                Assert.Contains(testKey, keys);
            }
        }

        /// <summary>
        /// Tests that Keys property reflects changes when items are added using the indexer.
        /// </summary>
        [Fact]
        public void Keys_WhenItemAddedViaIndexer_ReflectsChange()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testKey = "indexerKey";
            var testValue = "indexerValue";

            // Act
            propertySet[testKey] = testValue;
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(testKey, keys);
        }

        /// <summary>
        /// Tests that Keys property uses ordinal string comparison (case-sensitive).
        /// </summary>
        [Theory]
        [InlineData("Key", "key")]
        [InlineData("TEST", "test")]
        [InlineData("CamelCase", "camelcase")]
        public void Keys_WithCaseDifferences_TreatsKeysAsDifferent(string key1, string key2)
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testValue = "value";

            // Act
            propertySet.Add(key1, testValue);
            propertySet.Add(key2, testValue);
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(2, keys.Count());
            Assert.Contains(key1, keys);
            Assert.Contains(key2, keys);
        }

        /// <summary>
        /// Tests that Keys property handles special string values correctly.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("special!@#$%^&*()")]
        [InlineData("unicode中文")]
        public void Keys_WithSpecialStrings_HandlesCorrectly(string specialKey)
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testValue = "value";

            // Act
            propertySet.Add(specialKey, testValue);
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(specialKey, keys);
        }

        /// <summary>
        /// Tests that Keys property returns a live collection that reflects updates to the underlying dictionary.
        /// </summary>
        [Fact]
        public void Keys_ReflectsLiveChanges_WhenItemsAreModified()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var initialKey = "initialKey";
            var newKey = "newKey";
            var testValue = "value";

            propertySet.Add(initialKey, testValue);
            var keys = propertySet.Keys;

            // Act - Add another item after getting the Keys reference
            propertySet.Add(newKey, testValue);

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(2, keys.Count());
            Assert.Contains(initialKey, keys);
            Assert.Contains(newKey, keys);
        }

        /// <summary>
        /// Tests that Keys property handles null values in the dictionary correctly.
        /// </summary>
        [Fact]
        public void Keys_WithNullValues_ReturnsKeysCorrectly()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testKey = "keyWithNullValue";

            // Act
            propertySet.Add(testKey, null);
            var keys = propertySet.Keys;

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Contains(testKey, keys);
        }

        /// <summary>
        /// Tests that Keys property returns enumerable that can be iterated multiple times.
        /// </summary>
        [Fact]
        public void Keys_CanBeEnumeratedMultipleTimes_WithoutIssues()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var testKeys = new[] { "key1", "key2", "key3" };
            var testValue = "value";

            foreach (var key in testKeys)
            {
                propertySet.Add(key, testValue);
            }

            var keys = propertySet.Keys;

            // Act & Assert - Enumerate multiple times
            var firstEnumeration = keys.ToList();
            var secondEnumeration = keys.ToList();

            Assert.Equal(testKeys.Length, firstEnumeration.Count);
            Assert.Equal(testKeys.Length, secondEnumeration.Count);
            Assert.Equal(firstEnumeration, secondEnumeration);
        }

        /// <summary>
        /// Tests that the Values property returns an empty collection when no items have been added to the property set.
        /// </summary>
        [Fact]
        public void Values_WhenEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();

            // Act
            var values = propertySet.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Empty(values);
        }

        /// <summary>
        /// Tests that the Values property returns a single value when only one item has been added to the property set.
        /// </summary>
        [Fact]
        public void Values_WithSingleItem_ReturnsSingleValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var expectedValue = "test value";
            propertySet.Add("key1", expectedValue);

            // Act
            var values = propertySet.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Equal(expectedValue, values.First());
        }

        /// <summary>
        /// Tests that the Values property returns all values when multiple items have been added to the property set.
        /// </summary>
        [Fact]
        public void Values_WithMultipleItems_ReturnsAllValues()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var value1 = "first value";
            var value2 = 42;
            var value3 = DateTime.Now;
            propertySet.Add("key1", value1);
            propertySet.Add("key2", value2);
            propertySet.Add("key3", value3);

            // Act
            var values = propertySet.Values.ToList();

            // Assert
            Assert.NotNull(values);
            Assert.Equal(3, values.Count);
            Assert.Contains(value1, values);
            Assert.Contains(value2, values);
            Assert.Contains(value3, values);
        }

        /// <summary>
        /// Tests that the Values property correctly includes null values when they are stored in the property set.
        /// </summary>
        [Fact]
        public void Values_WithNullValues_IncludesNullValues()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            propertySet.Add("key1", null);
            propertySet.Add("key2", "not null");

            // Act
            var values = propertySet.Values.ToList();

            // Assert
            Assert.NotNull(values);
            Assert.Equal(2, values.Count);
            Assert.Contains(null, values);
            Assert.Contains("not null", values);
        }

        /// <summary>
        /// Tests that the Values property returns values of different types when various object types are stored.
        /// </summary>
        [Fact]
        public void Values_WithDifferentValueTypes_ReturnsAllTypes()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var stringValue = "text";
            var intValue = 123;
            var boolValue = true;
            var dateValue = new DateTime(2023, 1, 1);
            var listValue = new List<int> { 1, 2, 3 };

            propertySet.Add("string", stringValue);
            propertySet.Add("int", intValue);
            propertySet.Add("bool", boolValue);
            propertySet.Add("date", dateValue);
            propertySet.Add("list", listValue);

            // Act
            var values = propertySet.Values.ToList();

            // Assert
            Assert.NotNull(values);
            Assert.Equal(5, values.Count);
            Assert.Contains(stringValue, values);
            Assert.Contains(intValue, values);
            Assert.Contains(boolValue, values);
            Assert.Contains(dateValue, values);
            Assert.Contains(listValue, values);
        }

        /// <summary>
        /// Tests that the Values property returns all duplicate values when the same value is stored with different keys.
        /// </summary>
        [Fact]
        public void Values_WithDuplicateValues_ReturnsAllDuplicates()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var duplicateValue = "duplicate";
            propertySet.Add("key1", duplicateValue);
            propertySet.Add("key2", duplicateValue);
            propertySet.Add("key3", "unique");

            // Act
            var values = propertySet.Values.ToList();

            // Assert
            Assert.NotNull(values);
            Assert.Equal(3, values.Count);
            Assert.Equal(2, values.Count(v => v.Equals(duplicateValue)));
            Assert.Single(values, v => v.Equals("unique"));
        }

        /// <summary>
        /// Tests that the Values property reflects changes to the underlying property set after items are added.
        /// </summary>
        [Fact]
        public void Values_AfterAddingItems_ReflectsChanges()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var initialValues = propertySet.Values;

            // Act & Assert - Initial state
            Assert.Empty(initialValues);

            // Act - Add item
            propertySet.Add("key1", "value1");
            var valuesAfterAdd = propertySet.Values;

            // Assert - After adding
            Assert.Single(valuesAfterAdd);
            Assert.Contains("value1", valuesAfterAdd);
        }

        /// <summary>
        /// Tests that the Values property returns an enumerable that can be iterated multiple times.
        /// </summary>
        [Fact]
        public void Values_IsEnumerable_CanIterateMultipleTimes()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            propertySet.Add("key1", "value1");
            propertySet.Add("key2", "value2");

            // Act
            var values = propertySet.Values;
            var firstIteration = values.ToList();
            var secondIteration = values.ToList();

            // Assert
            Assert.NotNull(firstIteration);
            Assert.NotNull(secondIteration);
            Assert.Equal(2, firstIteration.Count);
            Assert.Equal(2, secondIteration.Count);
            Assert.Equal(firstIteration, secondIteration);
        }

        /// <summary>
        /// Tests that the Values property correctly handles indexer assignments by reflecting the updated values.
        /// </summary>
        [Fact]
        public void Values_AfterIndexerAssignment_ReflectsUpdatedValues()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            propertySet["key1"] = "initial value";

            // Act
            propertySet["key1"] = "updated value";
            var values = propertySet.Values.ToList();

            // Assert
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Equal("updated value", values.First());
            Assert.DoesNotContain("initial value", values);
        }

        /// <summary>
        /// Tests that TryGetValue returns true and sets the correct value when the key exists in the property set.
        /// Input: Valid key that exists in the property set with a string value.
        /// Expected: Returns true and outputs the correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_ExistingKey_ReturnsTrueAndCorrectValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var key = "testKey";
            var expectedValue = "testValue";
            propertySet.Add(key, expectedValue);

            // Act
            var result = propertySet.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue returns false and sets value to null when the key does not exist.
        /// Input: Key that has never been added to the property set.
        /// Expected: Returns false and outputs null.
        /// </summary>
        [Fact]
        public void TryGetValue_NonExistentKey_ReturnsFalseAndNullValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var key = "nonExistentKey";

            // Act
            var result = propertySet.TryGetValue(key, out var actualValue);

            // Assert
            Assert.False(result);
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue throws ArgumentNullException when key is null.
        /// Input: Null key parameter.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void TryGetValue_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => propertySet.TryGetValue(null, out var value));
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with empty string as key.
        /// Input: Empty string key that exists in the property set.
        /// Expected: Returns true and outputs the correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_EmptyStringKey_ReturnsTrueAndCorrectValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var key = "";
            var expectedValue = "valueForEmptyKey";
            propertySet.Add(key, expectedValue);

            // Act
            var result = propertySet.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with whitespace-only string as key.
        /// Input: Whitespace-only string key that exists in the property set.
        /// Expected: Returns true and outputs the correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_WhitespaceKey_ReturnsTrueAndCorrectValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var key = "   ";
            var expectedValue = "valueForWhitespaceKey";
            propertySet.Add(key, expectedValue);

            // Act
            var result = propertySet.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue returns true and null value when key exists but value is null.
        /// Input: Valid key with null value stored in the property set.
        /// Expected: Returns true and outputs null.
        /// </summary>
        [Fact]
        public void TryGetValue_ExistingKeyWithNullValue_ReturnsTrueAndNullValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var key = "keyWithNullValue";
            propertySet.Add(key, null);

            // Act
            var result = propertySet.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue is case-sensitive due to StringComparer.Ordinal usage.
        /// Input: Key with different casing than the stored key.
        /// Expected: Returns false as keys should be case-sensitive.
        /// </summary>
        [Fact]
        public void TryGetValue_CaseSensitiveKey_ReturnsFalse()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var originalKey = "TestKey";
            var differentCaseKey = "testkey";
            propertySet.Add(originalKey, "value");

            // Act
            var result = propertySet.TryGetValue(differentCaseKey, out var actualValue);

            // Assert
            Assert.False(result);
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works with various object types as values.
        /// Input: Keys with different value types (int, bool, complex object).
        /// Expected: Returns true and outputs the correct typed values.
        /// </summary>
        [Theory]
        [InlineData("intKey", 42)]
        [InlineData("boolKey", true)]
        [InlineData("stringKey", "test")]
        public void TryGetValue_VariousValueTypes_ReturnsTrueAndCorrectValues(string key, object expectedValue)
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            propertySet.Add(key, expectedValue);

            // Act
            var result = propertySet.TryGetValue(key, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with very long string keys.
        /// Input: Very long string key that exists in the property set.
        /// Expected: Returns true and outputs the correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_VeryLongKey_ReturnsTrueAndCorrectValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var longKey = new string('a', 10000);
            var expectedValue = "valueForLongKey";
            propertySet.Add(longKey, expectedValue);

            // Act
            var result = propertySet.TryGetValue(longKey, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that TryGetValue works correctly with keys containing special characters.
        /// Input: Key containing special characters that exists in the property set.
        /// Expected: Returns true and outputs the correct value.
        /// </summary>
        [Fact]
        public void TryGetValue_SpecialCharactersKey_ReturnsTrueAndCorrectValue()
        {
            // Arrange
            var propertySet = new DataPackagePropertySet();
            var specialKey = "key@#$%^&*(){}[]|\\:;\"'<>?,./";
            var expectedValue = "valueForSpecialKey";
            propertySet.Add(specialKey, expectedValue);

            // Act
            var result = propertySet.TryGetValue(specialKey, out var actualValue);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that Count returns 0 when the DataPackagePropertySet is newly created.
        /// Verifies the initial state of an empty property set.
        /// Expected result: Count should be 0.
        /// </summary>
        [Fact]
        public void Count_WhenNewlyCreated_ReturnsZero()
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();

            // Act
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns the correct number after adding items using the Add method.
        /// Verifies that Count accurately reflects the number of items added to the property set.
        /// Expected result: Count should equal the number of items added.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public void Count_AfterAddingItemsWithAdd_ReturnsCorrectCount(int numberOfItems)
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();

            // Act
            for (int i = 0; i < numberOfItems; i++)
            {
                dataPackagePropertySet.Add($"key{i}", $"value{i}");
            }
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(numberOfItems, count);
        }

        /// <summary>
        /// Tests that Count returns the correct number after adding items using the indexer.
        /// Verifies that Count accurately reflects the number of items added via indexer assignment.
        /// Expected result: Count should equal the number of unique keys set via indexer.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        public void Count_AfterAddingItemsWithIndexer_ReturnsCorrectCount(int numberOfItems)
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();

            // Act
            for (int i = 0; i < numberOfItems; i++)
            {
                dataPackagePropertySet[$"key{i}"] = $"value{i}";
            }
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(numberOfItems, count);
        }

        /// <summary>
        /// Tests that Count returns the correct number when items are added using mixed methods.
        /// Verifies that Count accurately reflects items added via both Add method and indexer.
        /// Expected result: Count should equal the total number of unique keys.
        /// </summary>
        [Fact]
        public void Count_AfterAddingItemsWithMixedMethods_ReturnsCorrectCount()
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();

            // Act
            dataPackagePropertySet.Add("key1", "value1");
            dataPackagePropertySet.Add("key2", "value2");
            dataPackagePropertySet["key3"] = "value3";
            dataPackagePropertySet["key4"] = "value4";
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(4, count);
        }

        /// <summary>
        /// Tests that Count returns the correct number when the same key is overwritten using indexer.
        /// Verifies that Count doesn't increase when overwriting existing keys via indexer.
        /// Expected result: Count should remain 1 when the same key is overwritten.
        /// </summary>
        [Fact]
        public void Count_WhenOverwritingKeyWithIndexer_RemainsUnchanged()
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();
            dataPackagePropertySet["key1"] = "originalValue";

            // Act
            dataPackagePropertySet["key1"] = "newValue";
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(1, count);
        }

        /// <summary>
        /// Tests that Count handles special string keys correctly.
        /// Verifies that Count works with edge case string keys like empty string, whitespace, and special characters.
        /// Expected result: Count should accurately reflect all unique keys including special string cases.
        /// </summary>
        [Fact]
        public void Count_WithSpecialStringKeys_ReturnsCorrectCount()
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();

            // Act
            dataPackagePropertySet[""] = "empty key";
            dataPackagePropertySet[" "] = "space key";
            dataPackagePropertySet["\t"] = "tab key";
            dataPackagePropertySet["special!@#$%^&*()"] = "special chars";
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(4, count);
        }

        /// <summary>
        /// Tests that Count handles null and various object types as values correctly.
        /// Verifies that Count reflects items regardless of the value type stored.
        /// Expected result: Count should equal the number of keys, regardless of value types.
        /// </summary>
        [Fact]
        public void Count_WithVariousValueTypes_ReturnsCorrectCount()
        {
            // Arrange
            var dataPackagePropertySet = new DataPackagePropertySet();

            // Act
            dataPackagePropertySet["nullValue"] = null;
            dataPackagePropertySet["stringValue"] = "test";
            dataPackagePropertySet["intValue"] = 42;
            dataPackagePropertySet["objectValue"] = new object();
            dataPackagePropertySet["arrayValue"] = new[] { 1, 2, 3 };
            var count = dataPackagePropertySet.Count;

            // Assert
            Assert.Equal(5, count);
        }
    }
}