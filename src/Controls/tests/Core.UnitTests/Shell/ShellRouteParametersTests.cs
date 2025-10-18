#nullable disable

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ShellRouteParametersTests
    {
        /// <summary>
        /// Tests that the copy constructor throws ArgumentNullException when shellRouteParams is null.
        /// </summary>
        [Fact]
        public void Constructor_NullShellRouteParams_ThrowsArgumentNullException()
        {
            // Arrange
            ShellRouteParameters nullParams = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShellRouteParameters(nullParams));
        }

        /// <summary>
        /// Tests that the copy constructor creates an empty instance when source has no items in dictionary or query parameters.
        /// </summary>
        [Fact]
        public void Constructor_EmptySource_CreatesEmptyInstance()
        {
            // Arrange
            var source = new ShellRouteParameters();

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Empty(result);
            Assert.NotSame(source, result);
        }

        /// <summary>
        /// Tests that the copy constructor copies all dictionary items when source contains only dictionary items.
        /// </summary>
        [Fact]
        public void Constructor_SourceWithDictionaryItemsOnly_CopiesDictionaryItems()
        {
            // Arrange
            var source = new ShellRouteParameters();
            source["key1"] = "value1";
            source["key2"] = 42;
            source["key3"] = null;

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal(42, result["key2"]);
            Assert.Null(result["key3"]);
            Assert.NotSame(source, result);
        }

        /// <summary>
        /// Tests that the copy constructor copies query parameters when source contains query parameters.
        /// </summary>
        [Fact]
        public void Constructor_SourceWithQueryParameters_CopiesQueryParameters()
        {
            // Arrange
            var queryParams = new ShellNavigationQueryParameters();
            queryParams["query1"] = "queryValue1";
            queryParams["query2"] = 123;
            var source = new ShellRouteParameters(queryParams);

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("queryValue1", result["query1"]);
            Assert.Equal(123, result["query2"]);
        }

        /// <summary>
        /// Tests that the copy constructor copies both dictionary and query parameter items when both are present.
        /// </summary>
        [Fact]
        public void Constructor_SourceWithBothDictionaryAndQueryItems_CopiesBothCollections()
        {
            // Arrange
            var queryParams = new ShellNavigationQueryParameters();
            queryParams["queryKey"] = "queryValue";
            var source = new ShellRouteParameters(queryParams);
            source["dictKey"] = "dictValue";

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("dictValue", result["dictKey"]);
            Assert.Equal("queryValue", result["queryKey"]);
        }

        /// <summary>
        /// Tests that the copy constructor creates an independent copy that can be modified without affecting the source.
        /// </summary>
        [Fact]
        public void Constructor_ModificationAfterCopy_DoesNotAffectSource()
        {
            // Arrange
            var source = new ShellRouteParameters();
            source["originalKey"] = "originalValue";

            // Act
            var result = new ShellRouteParameters(source);
            result["newKey"] = "newValue";
            result["originalKey"] = "modifiedValue";

            // Assert
            Assert.Equal("originalValue", source["originalKey"]);
            Assert.False(source.ContainsKey("newKey"));
            Assert.Equal("modifiedValue", result["originalKey"]);
            Assert.Equal("newValue", result["newKey"]);
        }

        /// <summary>
        /// Tests that the copy constructor handles various value types correctly including null values.
        /// </summary>
        [Theory]
        [InlineData("stringValue")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        [InlineData(null)]
        public void Constructor_VariousValueTypes_CopiesCorrectly(object value)
        {
            // Arrange
            var source = new ShellRouteParameters();
            source["testKey"] = value;

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Equal(value, result["testKey"]);
        }

        /// <summary>
        /// Tests that the copy constructor handles duplicate keys between dictionary and query parameters correctly.
        /// Query parameters should overwrite dictionary values during copy.
        /// </summary>
        [Fact]
        public void Constructor_DuplicateKeysBetweenDictionaryAndQuery_QueryParametersOverwrite()
        {
            // Arrange
            var queryParams = new ShellNavigationQueryParameters();
            queryParams["duplicateKey"] = "queryValue";
            var source = new ShellRouteParameters(queryParams);
            source["duplicateKey"] = "dictValue"; // This should be overwritten

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Equal("dictValue", result["duplicateKey"]); // Dictionary value takes precedence due to base constructor
        }

        /// <summary>
        /// Tests that the copy constructor handles large collections efficiently.
        /// </summary>
        [Fact]
        public void Constructor_LargeCollections_CopiesAllItems()
        {
            // Arrange
            var source = new ShellRouteParameters();
            for (int i = 0; i < 1000; i++)
            {
                source[$"key{i}"] = $"value{i}";
            }

            var queryParams = new ShellNavigationQueryParameters();
            for (int i = 1000; i < 2000; i++)
            {
                queryParams[$"queryKey{i}"] = $"queryValue{i}";
            }
            var sourceWithQuery = new ShellRouteParameters(queryParams);
            foreach (var item in source)
            {
                sourceWithQuery[item.Key] = item.Value;
            }

            // Act
            var result = new ShellRouteParameters(sourceWithQuery);

            // Assert
            Assert.Equal(2000, result.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal($"value{i}", result[$"key{i}"]);
            }
            for (int i = 1000; i < 2000; i++)
            {
                Assert.Equal($"queryValue{i}", result[$"queryKey{i}"]);
            }
        }

        /// <summary>
        /// Tests that the copy constructor handles special string keys correctly including empty and whitespace strings.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("key with spaces")]
        [InlineData("key.with.dots")]
        [InlineData("key/with/slashes")]
        public void Constructor_SpecialStringKeys_CopiesCorrectly(string key)
        {
            // Arrange
            var source = new ShellRouteParameters();
            source[key] = "testValue";

            // Act
            var result = new ShellRouteParameters(source);

            // Assert
            Assert.Equal("testValue", result[key]);
        }

        /// <summary>
        /// Tests the constructor that filters parameters based on prefix and excludes keys containing dots.
        /// Should create a new instance with filtered parameters and copy all shell navigation query parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithValidQueryAndPrefix_FiltersParametersCorrectly()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("app.page1", "value1");
            sourceParams.Add("app.page2", "value2");
            sourceParams.Add("other.item", "value3");
            sourceParams.Add("apptest", "value4");
            var prefix = "app.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("page1"));
            Assert.True(result.ContainsKey("page2"));
            Assert.Equal("value1", result["page1"]);
            Assert.Equal("value2", result["page2"]);
            Assert.False(result.ContainsKey("app.page1"));
            Assert.False(result.ContainsKey("other.item"));
            Assert.False(result.ContainsKey("apptest"));
        }

        /// <summary>
        /// Tests the constructor with keys that contain dots after prefix removal.
        /// Should exclude keys that contain dots after removing the prefix.
        /// </summary>
        [Fact]
        public void Constructor_WithKeysContainingDotsAfterPrefixRemoval_ExcludesDottedKeys()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("prefix.simple", "value1");
            sourceParams.Add("prefix.with.dots", "value2");
            sourceParams.Add("prefix.another.dot", "value3");
            var prefix = "prefix.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey("simple"));
            Assert.Equal("value1", result["simple"]);
            Assert.False(result.ContainsKey("with.dots"));
            Assert.False(result.ContainsKey("another.dot"));
        }

        /// <summary>
        /// Tests the constructor with an empty prefix.
        /// Should include keys that don't contain dots and exclude keys with dots.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyPrefix_IncludesKeysWithoutDots()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("simple", "value1");
            sourceParams.Add("with.dot", "value2");
            sourceParams.Add("another", "value3");
            var prefix = "";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("simple"));
            Assert.True(result.ContainsKey("another"));
            Assert.Equal("value1", result["simple"]);
            Assert.Equal("value3", result["another"]);
            Assert.False(result.ContainsKey("with.dot"));
        }

        /// <summary>
        /// Tests the constructor with keys that don't match the prefix.
        /// Should create an empty dictionary when no keys match the prefix.
        /// </summary>
        [Fact]
        public void Constructor_WithNoMatchingPrefix_CreatesEmptyDictionary()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("other1", "value1");
            sourceParams.Add("other2", "value2");
            var prefix = "app.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the constructor with an empty source parameters dictionary.
        /// Should create an empty dictionary.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptySourceParameters_CreatesEmptyDictionary()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            var prefix = "app.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the constructor with exact prefix matches.
        /// Should include keys where the key exactly equals the prefix (resulting in empty string after removal).
        /// </summary>
        [Fact]
        public void Constructor_WithExactPrefixMatch_IncludesEmptyKeyAfterRemoval()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("app", "value1");
            sourceParams.Add("appmore", "value2");
            var prefix = "app";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(""));
            Assert.True(result.ContainsKey("more"));
            Assert.Equal("value1", result[""]);
            Assert.Equal("value2", result["more"]);
        }

        /// <summary>
        /// Tests the constructor with case-sensitive prefix matching.
        /// Should only match keys with exact case due to StringComparison.Ordinal.
        /// </summary>
        [Fact]
        public void Constructor_WithCaseSensitivePrefix_MatchesExactCaseOnly()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("App.test", "value1");
            sourceParams.Add("app.test", "value2");
            sourceParams.Add("APP.test", "value3");
            var prefix = "app.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey("test"));
            Assert.Equal("value2", result["test"]);
        }

        /// <summary>
        /// Tests the constructor with various value types.
        /// Should preserve different value types in the filtered parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithVariousValueTypes_PreservesValueTypes()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("app.string", "stringValue");
            sourceParams.Add("app.int", 42);
            sourceParams.Add("app.null", null);
            sourceParams.Add("app.bool", true);
            var prefix = "app.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal("stringValue", result["string"]);
            Assert.Equal(42, result["int"]);
            Assert.Null(result["null"]);
            Assert.Equal(true, result["bool"]);
        }

        /// <summary>
        /// Tests the constructor with null query parameter.
        /// Should throw NullReferenceException when query is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullQuery_ThrowsNullReferenceException()
        {
            // Arrange
            ShellRouteParameters nullQuery = null;
            var prefix = "app.";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new ShellRouteParameters(nullQuery, prefix));
        }

        /// <summary>
        /// Tests the constructor with null prefix parameter.
        /// Should throw NullReferenceException when prefix is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullPrefix_ThrowsNullReferenceException()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("test", "value");
            string nullPrefix = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new ShellRouteParameters(sourceParams, nullPrefix));
        }

        /// <summary>
        /// Tests the constructor with prefix longer than key.
        /// Should throw ArgumentOutOfRangeException when Substring is called with invalid range.
        /// </summary>
        [Fact]
        public void Constructor_WithPrefixLongerThanKey_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("ab", "value");
            var prefix = "abc";

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShellRouteParameters(sourceParams, prefix));
        }

        /// <summary>
        /// Tests the constructor with special characters in keys and prefix.
        /// Should handle special characters correctly in prefix matching and key transformation.
        /// </summary>
        [Fact]
        public void Constructor_WithSpecialCharacters_HandlesSpecialCharactersCorrectly()
        {
            // Arrange
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add("special#.test", "value1");
            sourceParams.Add("special#.with.dot", "value2");
            sourceParams.Add("other", "value3");
            var prefix = "special#.";

            // Act
            var result = new ShellRouteParameters(sourceParams, prefix);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey("test"));
            Assert.Equal("value1", result["test"]);
            Assert.False(result.ContainsKey("with.dot"));
        }

        /// <summary>
        /// Tests the constructor with very long strings.
        /// Should handle very long prefix and key strings without issues.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongStrings_HandlesLongStringsCorrectly()
        {
            // Arrange
            var longPrefix = new string('a', 1000) + ".";
            var longKey = longPrefix + "test";
            var sourceParams = new ShellRouteParameters();
            sourceParams.Add(longKey, "value");

            // Act
            var result = new ShellRouteParameters(sourceParams, longPrefix);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey("test"));
            Assert.Equal("value", result["test"]);
        }

        /// <summary>
        /// Tests that the IDictionary constructor throws ArgumentNullException when provided with a null dictionary.
        /// Input: null dictionary
        /// Expected: ArgumentNullException is thrown
        /// </summary>
        [Fact]
        public void Constructor_NullDictionary_ThrowsArgumentNullException()
        {
            // Arrange
            IDictionary<string, object> nullDictionary = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ShellRouteParameters(nullDictionary));
        }

        /// <summary>
        /// Tests that the IDictionary constructor creates an empty ShellRouteParameters when provided with an empty dictionary.
        /// Input: empty dictionary
        /// Expected: ShellRouteParameters with zero items
        /// </summary>
        [Fact]
        public void Constructor_EmptyDictionary_CreatesEmptyShellRouteParameters()
        {
            // Arrange
            var emptyDictionary = new Dictionary<string, object>();

            // Act
            var result = new ShellRouteParameters(emptyDictionary);

            // Assert
            Assert.Empty(result);
            Assert.Equal(0, result.Count);
        }

        /// <summary>
        /// Tests that the IDictionary constructor properly copies a single item from the input dictionary.
        /// Input: dictionary with one key-value pair
        /// Expected: ShellRouteParameters contains the same key-value pair
        /// </summary>
        [Fact]
        public void Constructor_SingleItemDictionary_CopiesItem()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                ["key1"] = "value1"
            };

            // Act
            var result = new ShellRouteParameters(dictionary);

            // Assert
            Assert.Single(result);
            Assert.Equal("value1", result["key1"]);
            Assert.True(result.ContainsKey("key1"));
        }

        /// <summary>
        /// Tests that the IDictionary constructor properly copies multiple items from the input dictionary.
        /// Input: dictionary with multiple key-value pairs
        /// Expected: ShellRouteParameters contains all the same key-value pairs
        /// </summary>
        [Fact]
        public void Constructor_MultipleItemsDictionary_CopiesAllItems()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 42,
                ["key3"] = new object()
            };

            // Act
            var result = new ShellRouteParameters(dictionary);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal(42, result["key2"]);
            Assert.Equal(dictionary["key3"], result["key3"]);
        }

        /// <summary>
        /// Tests that the IDictionary constructor handles various value types including null values.
        /// Input: dictionary with string, int, object, and null values
        /// Expected: ShellRouteParameters contains all values with correct types
        /// </summary>
        [Theory]
        [MemberData(nameof(GetVariousValueTypes))]
        public void Constructor_VariousValueTypes_HandlesAllTypes(string key, object value, string description)
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                [key] = value
            };

            // Act
            var result = new ShellRouteParameters(dictionary);

            // Assert
            Assert.Single(result);
            Assert.Equal(value, result[key]);
            Assert.True(result.ContainsKey(key));
        }

        /// <summary>
        /// Tests that the IDictionary constructor handles special key formats including empty strings and whitespace.
        /// Input: dictionary with various special key formats
        /// Expected: ShellRouteParameters preserves all key formats correctly
        /// </summary>
        [Theory]
        [MemberData(nameof(GetSpecialKeys))]
        public void Constructor_SpecialKeys_PreservesKeyFormats(string key, string description)
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                [key] = "testValue"
            };

            // Act
            var result = new ShellRouteParameters(dictionary);

            // Assert
            Assert.Single(result);
            Assert.Equal("testValue", result[key]);
            Assert.True(result.ContainsKey(key));
        }

        /// <summary>
        /// Tests that the constructed ShellRouteParameters is independent of the original dictionary.
        /// Input: dictionary that is modified after construction
        /// Expected: ShellRouteParameters remains unchanged when original dictionary is modified
        /// </summary>
        [Fact]
        public void Constructor_IndependentCopy_ModifyingOriginalDoesNotAffectResult()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
            {
                ["key1"] = "value1"
            };

            // Act
            var result = new ShellRouteParameters(dictionary);
            dictionary["key1"] = "modifiedValue";
            dictionary["key2"] = "newValue";

            // Assert
            Assert.Single(result);
            Assert.Equal("value1", result["key1"]);
            Assert.False(result.ContainsKey("key2"));
        }

        public static IEnumerable<object[]> GetVariousValueTypes()
        {
            yield return new object[] { "stringKey", "stringValue", "string value" };
            yield return new object[] { "intKey", 42, "integer value" };
            yield return new object[] { "doubleKey", 3.14, "double value" };
            yield return new object[] { "boolKey", true, "boolean value" };
            yield return new object[] { "nullKey", null, "null value" };
            yield return new object[] { "objectKey", new object(), "object value" };
            yield return new object[] { "arrayKey", new[] { 1, 2, 3 }, "array value" };
            yield return new object[] { "dictionaryKey", new Dictionary<string, string> { ["nested"] = "value" }, "nested dictionary value" };
        }

        public static IEnumerable<object[]> GetSpecialKeys()
        {
            yield return new object[] { "", "empty string key" };
            yield return new object[] { " ", "single space key" };
            yield return new object[] { "\t", "tab character key" };
            yield return new object[] { "\n", "newline character key" };
            yield return new object[] { "  ", "multiple spaces key" };
            yield return new object[] { "key with spaces", "key with internal spaces" };
            yield return new object[] { "key.with.dots", "key with dots" };
            yield return new object[] { "key/with/slashes", "key with slashes" };
            yield return new object[] { "key-with-dashes", "key with dashes" };
            yield return new object[] { "key_with_underscores", "key with underscores" };
            yield return new object[] { "KeyWithMixedCase", "key with mixed case" };
            yield return new object[] { "123numerickey", "key starting with numbers" };
            yield return new object[] { "key@with!special#chars$", "key with special characters" };
        }

        /// <summary>
        /// Tests that the constructor throws NullReferenceException when passed a null ShellNavigationQueryParameters.
        /// Verifies proper null parameter validation.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_WithNullShellNavigationQueryParameters_ThrowsNullReferenceException()
        {
            // Arrange
            ShellNavigationQueryParameters nullParameters = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new ShellRouteParameters(nullParameters));
        }

        /// <summary>
        /// Tests that the constructor creates an empty ShellRouteParameters when passed an empty ShellNavigationQueryParameters.
        /// Verifies that both the dictionary and internal field remain empty.
        /// Expected result: Empty ShellRouteParameters with Count of 0.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyShellNavigationQueryParameters_CreatesEmptyDictionary()
        {
            // Arrange
            var emptyParameters = new ShellNavigationQueryParameters();

            // Act
            var result = new ShellRouteParameters(emptyParameters);

            // Assert
            Assert.Empty(result);
            Assert.Equal(0, result.Count);

            // Verify internal field is also empty by checking ToReadOnlyIfUsingShellNavigationQueryParameters behavior
            var readOnlyResult = result.ToReadOnlyIfUsingShellNavigationQueryParameters();
            Assert.Same(result, readOnlyResult); // Should return the same instance when internal field is empty
        }

        /// <summary>
        /// Tests that the constructor correctly adds a single key-value pair to both the dictionary and internal field.
        /// Verifies that the item is accessible through dictionary operations and internal field operations.
        /// Expected result: ShellRouteParameters contains the single item in both locations.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItem_AddsBothToDictionaryAndInternalField()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("key1", "value1");

            // Act
            var result = new ShellRouteParameters(parameters);

            // Assert
            Assert.Single(result);
            Assert.Equal("value1", result["key1"]);
            Assert.True(result.ContainsKey("key1"));

            // Verify internal field was populated by checking ToReadOnlyIfUsingShellNavigationQueryParameters behavior
            var readOnlyResult = result.ToReadOnlyIfUsingShellNavigationQueryParameters();
            Assert.NotSame(result, readOnlyResult); // Should return different instance when internal field has items
            Assert.Equal("value1", readOnlyResult["key1"]);
        }

        /// <summary>
        /// Tests that the constructor correctly adds multiple key-value pairs to both the dictionary and internal field.
        /// Verifies that all items are accessible and the count is correct.
        /// Expected result: ShellRouteParameters contains all items in both locations.
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleItems_AddsAllItems()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("key1", "value1");
            parameters.Add("key2", 42);
            parameters.Add("key3", true);

            // Act
            var result = new ShellRouteParameters(parameters);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal(42, result["key2"]);
            Assert.Equal(true, result["key3"]);

            // Verify all keys are present
            Assert.True(result.ContainsKey("key1"));
            Assert.True(result.ContainsKey("key2"));
            Assert.True(result.ContainsKey("key3"));

            // Verify internal field was populated
            var readOnlyResult = result.ToReadOnlyIfUsingShellNavigationQueryParameters();
            Assert.NotSame(result, readOnlyResult);
            Assert.Equal(3, readOnlyResult.Count);
            Assert.Equal("value1", readOnlyResult["key1"]);
            Assert.Equal(42, readOnlyResult["key2"]);
            Assert.Equal(true, readOnlyResult["key3"]);
        }

        /// <summary>
        /// Tests that the constructor handles various value types including null values.
        /// Verifies that different object types are stored and retrieved correctly.
        /// Expected result: All value types are handled correctly including null.
        /// </summary>
        [Fact]
        public void Constructor_WithVariousValueTypes_HandlesAllTypes()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("stringValue", "test");
            parameters.Add("intValue", 123);
            parameters.Add("doubleValue", 45.67);
            parameters.Add("boolValue", false);
            parameters.Add("nullValue", null);
            parameters.Add("dateValue", new DateTime(2024, 1, 1));

            // Act
            var result = new ShellRouteParameters(parameters);

            // Assert
            Assert.Equal(6, result.Count);
            Assert.Equal("test", result["stringValue"]);
            Assert.Equal(123, result["intValue"]);
            Assert.Equal(45.67, result["doubleValue"]);
            Assert.Equal(false, result["boolValue"]);
            Assert.Null(result["nullValue"]);
            Assert.Equal(new DateTime(2024, 1, 1), result["dateValue"]);

            // Verify internal field preserves all types
            var readOnlyResult = result.ToReadOnlyIfUsingShellNavigationQueryParameters();
            Assert.Equal(6, readOnlyResult.Count);
            Assert.Equal("test", readOnlyResult["stringValue"]);
            Assert.Equal(123, readOnlyResult["intValue"]);
            Assert.Equal(45.67, readOnlyResult["doubleValue"]);
            Assert.Equal(false, readOnlyResult["boolValue"]);
            Assert.Null(readOnlyResult["nullValue"]);
            Assert.Equal(new DateTime(2024, 1, 1), readOnlyResult["dateValue"]);
        }

        /// <summary>
        /// Tests that the constructor handles keys with special characters correctly.
        /// Verifies that unusual but valid string keys are processed without issues.
        /// Expected result: All special character keys are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithSpecialCharactersInKeys_HandlesCorrectly()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("key with spaces", "value1");
            parameters.Add("key-with-dashes", "value2");
            parameters.Add("key_with_underscores", "value3");
            parameters.Add("key.with.dots", "value4");
            parameters.Add("key/with/slashes", "value5");
            parameters.Add("", "emptyKey"); // Empty string key
            parameters.Add("キー", "unicodeValue"); // Unicode key

            // Act
            var result = new ShellRouteParameters(parameters);

            // Assert
            Assert.Equal(7, result.Count);
            Assert.Equal("value1", result["key with spaces"]);
            Assert.Equal("value2", result["key-with-dashes"]);
            Assert.Equal("value3", result["key_with_underscores"]);
            Assert.Equal("value4", result["key.with.dots"]);
            Assert.Equal("value5", result["key/with/slashes"]);
            Assert.Equal("emptyKey", result[""]);
            Assert.Equal("unicodeValue", result["キー"]);

            // Verify internal field preserves special characters
            var readOnlyResult = result.ToReadOnlyIfUsingShellNavigationQueryParameters();
            Assert.Equal(7, readOnlyResult.Count);
            Assert.Equal("value1", readOnlyResult["key with spaces"]);
            Assert.Equal("value2", readOnlyResult["key-with-dashes"]);
            Assert.Equal("value3", readOnlyResult["key_with_underscores"]);
            Assert.Equal("value4", readOnlyResult["key.with.dots"]);
            Assert.Equal("value5", readOnlyResult["key/with/slashes"]);
            Assert.Equal("emptyKey", readOnlyResult[""]);
            Assert.Equal("unicodeValue", readOnlyResult["キー"]);
        }

        /// <summary>
        /// Tests that the constructor maintains the exact same key-value pairs in both the dictionary and internal field.
        /// Verifies that the double iteration in the constructor produces identical results.
        /// Expected result: Dictionary contents and internal field contents are identical.
        /// </summary>
        [Fact]
        public void Constructor_WithShellNavigationQueryParameters_MaintainsIdenticalContent()
        {
            // Arrange
            var parameters = new ShellNavigationQueryParameters();
            parameters.Add("alpha", 1);
            parameters.Add("beta", "two");
            parameters.Add("gamma", 3.0);

            // Act
            var result = new ShellRouteParameters(parameters);

            // Assert
            // Verify dictionary contents
            Assert.Equal(3, result.Count);
            var dictionaryKeys = new List<string>(result.Keys);
            var dictionaryValues = new List<object>(result.Values);

            // Verify internal field contents through ToReadOnlyIfUsingShellNavigationQueryParameters
            var readOnlyResult = result.ToReadOnlyIfUsingShellNavigationQueryParameters();
            Assert.Equal(3, readOnlyResult.Count);

            // Verify all keys and values match
            foreach (var kvp in result)
            {
                Assert.True(readOnlyResult.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, readOnlyResult[kvp.Key]);
            }
        }

        /// <summary>
        /// Tests that the default parameterless constructor creates a valid ShellRouteParameters instance
        /// that behaves as an empty dictionary.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidEmptyInstance()
        {
            // Arrange & Act
            var shellRouteParameters = new ShellRouteParameters();

            // Assert
            Assert.NotNull(shellRouteParameters);
            Assert.Empty(shellRouteParameters);
            Assert.Equal(0, shellRouteParameters.Count);
            Assert.IsAssignableFrom<Dictionary<string, object>>(shellRouteParameters);
        }

        /// <summary>
        /// Tests that the default constructor creates an instance that supports basic dictionary operations
        /// like adding and retrieving key-value pairs.
        /// </summary>
        [Fact]
        public void Constructor_Default_SupportsDictionaryOperations()
        {
            // Arrange
            var shellRouteParameters = new ShellRouteParameters();

            // Act
            shellRouteParameters.Add("key1", "value1");
            shellRouteParameters["key2"] = 42;

            // Assert
            Assert.Equal(2, shellRouteParameters.Count);
            Assert.Equal("value1", shellRouteParameters["key1"]);
            Assert.Equal(42, shellRouteParameters["key2"]);
            Assert.True(shellRouteParameters.ContainsKey("key1"));
            Assert.True(shellRouteParameters.ContainsKey("key2"));
        }

        /// <summary>
        /// Tests that the default constructor creates an instance that supports enumeration
        /// and behaves as expected for an empty collection.
        /// </summary>
        [Fact]
        public void Constructor_Default_SupportsEnumeration()
        {
            // Arrange
            var shellRouteParameters = new ShellRouteParameters();

            // Act & Assert
            Assert.Empty(shellRouteParameters);

            var count = 0;
            foreach (var item in shellRouteParameters)
            {
                count++;
            }
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that the default constructor does not throw any exceptions during instance creation.
        /// </summary>
        [Fact]
        public void Constructor_Default_DoesNotThrowException()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => new ShellRouteParameters());
            Assert.Null(exception);
        }
    }

    public class ShellParameterExtensionsTests
    {
        /// <summary>
        /// Tests the Deconstruct method with a valid KeyValuePair containing a non-empty key and non-null value.
        /// Verifies that the key and value are correctly extracted into the out parameters.
        /// </summary>
        [Fact]
        public void Deconstruct_ValidKeyValuePair_ExtractsKeyAndValue()
        {
            // Arrange
            var kvp = new KeyValuePair<string, object>("testKey", "testValue");

            // Act
            var (key, value) = kvp;

            // Assert
            Assert.Equal("testKey", key);
            Assert.Equal("testValue", value);
        }

        /// <summary>
        /// Tests the Deconstruct method with various key and value combinations using parameterized test data.
        /// Verifies that different types of keys and values are correctly extracted.
        /// </summary>
        /// <param name="inputKey">The key to test</param>
        /// <param name="inputValue">The value to test</param>
        /// <param name="expectedKey">The expected extracted key</param>
        /// <param name="expectedValue">The expected extracted value</param>
        [Theory]
        [InlineData("", null, "", null)]
        [InlineData("key1", 42, "key1", 42)]
        [InlineData("  ", "value", "  ", "value")]
        [InlineData("specialKey", true, "specialKey", true)]
        [InlineData("numericKey", 3.14, "numericKey", 3.14)]
        public void Deconstruct_VariousKeyValuePairs_ExtractsCorrectly(string inputKey, object inputValue, string expectedKey, object expectedValue)
        {
            // Arrange
            var kvp = new KeyValuePair<string, object>(inputKey, inputValue);

            // Act
            var (key, value) = kvp;

            // Assert
            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Tests the Deconstruct method with a KeyValuePair containing a null value.
        /// Verifies that null values are correctly handled and extracted.
        /// </summary>
        [Fact]
        public void Deconstruct_NullValue_ExtractsNullCorrectly()
        {
            // Arrange
            var kvp = new KeyValuePair<string, object>("nullValueKey", null);

            // Act
            var (key, value) = kvp;

            // Assert
            Assert.Equal("nullValueKey", key);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests the Deconstruct method with a KeyValuePair containing an empty string key.
        /// Verifies that empty string keys are correctly handled and extracted.
        /// </summary>
        [Fact]
        public void Deconstruct_EmptyStringKey_ExtractsEmptyStringCorrectly()
        {
            // Arrange
            var kvp = new KeyValuePair<string, object>("", "someValue");

            // Act
            var (key, value) = kvp;

            // Assert
            Assert.Equal("", key);
            Assert.Equal("someValue", value);
        }

        /// <summary>
        /// Tests the Deconstruct method with a KeyValuePair containing complex object types as values.
        /// Verifies that complex objects are correctly extracted without modification.
        /// </summary>
        [Fact]
        public void Deconstruct_ComplexObjectValue_ExtractsObjectCorrectly()
        {
            // Arrange
            var complexObject = new List<string> { "item1", "item2" };
            var kvp = new KeyValuePair<string, object>("listKey", complexObject);

            // Act
            var (key, value) = kvp;

            // Assert
            Assert.Equal("listKey", key);
            Assert.Same(complexObject, value);
        }

        /// <summary>
        /// Tests the Deconstruct method with a KeyValuePair containing whitespace-only key.
        /// Verifies that whitespace keys are preserved and extracted correctly.
        /// </summary>
        [Fact]
        public void Deconstruct_WhitespaceKey_ExtractsWhitespaceCorrectly()
        {
            // Arrange
            var kvp = new KeyValuePair<string, object>("   \t\n   ", "whitespaceKeyValue");

            // Act
            var (key, value) = kvp;

            // Assert
            Assert.Equal("   \t\n   ", key);
            Assert.Equal("whitespaceKeyValue", value);
        }
    }
}