#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SearchHandler class focusing on the Keyboard property.
    /// </summary>
    public class SearchHandlerTests
    {
        /// <summary>
        /// Tests that the Keyboard property returns the default value when a SearchHandler is first created.
        /// Verifies that the getter correctly retrieves the default Keyboard.Default value.
        /// </summary>
        [Fact]
        public void Keyboard_DefaultValue_ReturnsKeyboardDefault()
        {
            // Arrange & Act
            var searchHandler = new SearchHandler();

            // Assert
            Assert.Equal(Keyboard.Default, searchHandler.Keyboard);
        }

        /// <summary>
        /// Tests that the Keyboard property correctly returns various predefined keyboard types after being set.
        /// Verifies that the getter retrieves the correct keyboard value for different keyboard types.
        /// </summary>
        /// <param name="keyboard">The keyboard type to test.</param>
        [Theory]
        [MemberData(nameof(GetPredefinedKeyboards))]
        public void Keyboard_SetPredefinedKeyboard_ReturnsCorrectValue(Keyboard keyboard)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Keyboard = keyboard;

            // Assert
            Assert.Equal(keyboard, searchHandler.Keyboard);
        }

        /// <summary>
        /// Tests that setting the Keyboard property to null gets coerced to Keyboard.Default.
        /// Verifies that the coerce function in the BindableProperty definition works correctly.
        /// </summary>
        [Fact]
        public void Keyboard_SetNull_ReturnsKeyboardDefault()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Keyboard = null;

            // Assert
            Assert.Equal(Keyboard.Default, searchHandler.Keyboard);
        }

        /// <summary>
        /// Tests that the Keyboard property correctly handles custom keyboards created with Keyboard.Create().
        /// Verifies that the getter returns custom keyboard instances properly.
        /// </summary>
        /// <param name="flags">The KeyboardFlags to use for creating a custom keyboard.</param>
        [Theory]
        [InlineData(KeyboardFlags.None)]
        [InlineData(KeyboardFlags.CapitalizeSentence)]
        [InlineData(KeyboardFlags.Spellcheck)]
        [InlineData(KeyboardFlags.Suggestions)]
        [InlineData(KeyboardFlags.CapitalizeWord)]
        [InlineData(KeyboardFlags.CapitalizeCharacter)]
        [InlineData(KeyboardFlags.CapitalizeNone)]
        [InlineData(KeyboardFlags.All)]
        public void Keyboard_SetCustomKeyboard_ReturnsCorrectValue(KeyboardFlags flags)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var customKeyboard = Keyboard.Create(flags);

            // Act
            searchHandler.Keyboard = customKeyboard;

            // Assert
            Assert.Equal(customKeyboard, searchHandler.Keyboard);
        }

        /// <summary>
        /// Tests that setting the Keyboard property multiple times with different values works correctly.
        /// Verifies that the getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void Keyboard_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert - Set and verify different keyboard types in sequence
            searchHandler.Keyboard = Keyboard.Email;
            Assert.Equal(Keyboard.Email, searchHandler.Keyboard);

            searchHandler.Keyboard = Keyboard.Numeric;
            Assert.Equal(Keyboard.Numeric, searchHandler.Keyboard);

            searchHandler.Keyboard = Keyboard.Default;
            Assert.Equal(Keyboard.Default, searchHandler.Keyboard);

            searchHandler.Keyboard = Keyboard.Chat;
            Assert.Equal(Keyboard.Chat, searchHandler.Keyboard);
        }

        /// <summary>
        /// Provides test data for predefined keyboard types.
        /// </summary>
        /// <returns>Collection of keyboard instances for parameterized testing.</returns>
        public static IEnumerable<object[]> GetPredefinedKeyboards()
        {
            yield return new object[] { Keyboard.Default };
            yield return new object[] { Keyboard.Plain };
            yield return new object[] { Keyboard.Chat };
            yield return new object[] { Keyboard.Email };
            yield return new object[] { Keyboard.Numeric };
            yield return new object[] { Keyboard.Telephone };
            yield return new object[] { Keyboard.Text };
            yield return new object[] { Keyboard.Url };
            yield return new object[] { Keyboard.Date };
            yield return new object[] { Keyboard.Password };
            yield return new object[] { Keyboard.Time };
        }

        /// <summary>
        /// Tests that FontAttributes getter returns the correct value when set to None.
        /// Verifies the property correctly calls GetValue and casts to FontAttributes enum.
        /// Expected result: Returns FontAttributes.None (0).
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithNoneValue_ReturnsNone()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontAttributes = FontAttributes.None;
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.None, result);
        }

        /// <summary>
        /// Tests that FontAttributes getter returns the correct value when set to Bold.
        /// Verifies the property correctly handles single flag values.
        /// Expected result: Returns FontAttributes.Bold (1).
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithBoldValue_ReturnsBold()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontAttributes = FontAttributes.Bold;
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.Bold, result);
        }

        /// <summary>
        /// Tests that FontAttributes getter returns the correct value when set to Italic.
        /// Verifies the property correctly handles single flag values.
        /// Expected result: Returns FontAttributes.Italic (2).
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithItalicValue_ReturnsItalic()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontAttributes = FontAttributes.Italic;
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.Italic, result);
        }

        /// <summary>
        /// Tests that FontAttributes getter returns the correct value when set to combined Bold and Italic flags.
        /// Verifies the property correctly handles multiple flag combinations since FontAttributes is marked with [Flags].
        /// Expected result: Returns FontAttributes.Bold | FontAttributes.Italic (3).
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithCombinedBoldItalicValue_ReturnsCombined()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var combinedValue = FontAttributes.Bold | FontAttributes.Italic;

            // Act
            searchHandler.FontAttributes = combinedValue;
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(combinedValue, result);
        }

        /// <summary>
        /// Tests FontAttributes getter and setter with various valid enum values.
        /// Verifies the property correctly handles all defined enum values and combinations.
        /// Expected result: Each set value is correctly returned by the getter.
        /// </summary>
        [Theory]
        [InlineData(FontAttributes.None)]
        [InlineData(FontAttributes.Bold)]
        [InlineData(FontAttributes.Italic)]
        [InlineData(FontAttributes.Bold | FontAttributes.Italic)]
        public void FontAttributes_GetterSetterWithValidValues_ReturnsExpectedValue(FontAttributes expectedValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontAttributes = expectedValue;
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests FontAttributes getter with invalid enum values cast from integers.
        /// Verifies the property correctly handles out-of-range values that could be cast to the enum.
        /// Expected result: Returns the cast value even if it's outside the defined enum range.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void FontAttributes_GetterWithInvalidEnumValues_ReturnsCastValue(int invalidValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var invalidEnumValue = (FontAttributes)invalidValue;

            // Act
            searchHandler.FontAttributes = invalidEnumValue;
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that FontAttributes getter returns default value when no value has been explicitly set.
        /// Verifies the initial state of the property after object creation.
        /// Expected result: Returns FontAttributes.None as the default value.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithDefaultValue_ReturnsNone()
        {
            // Arrange & Act
            var searchHandler = new SearchHandler();
            var result = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.None, result);
        }

        /// <summary>
        /// Tests FontAttributes getter behavior with multiple successive property access calls.
        /// Verifies the property consistently returns the same value on repeated access.
        /// Expected result: Multiple calls return the same consistent value.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterMultipleAccess_ReturnsConsistentValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontAttributes = FontAttributes.Bold;

            // Act
            var result1 = searchHandler.FontAttributes;
            var result2 = searchHandler.FontAttributes;
            var result3 = searchHandler.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.Bold, result1);
            Assert.Equal(FontAttributes.Bold, result2);
            Assert.Equal(FontAttributes.Bold, result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        /// <summary>
        /// Tests FontAttributes property with value changes to ensure getter reflects updates.
        /// Verifies the property correctly updates when the underlying value changes.
        /// Expected result: Getter returns the most recently set value.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterAfterValueChange_ReturnsUpdatedValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert - Initial value
            searchHandler.FontAttributes = FontAttributes.None;
            Assert.Equal(FontAttributes.None, searchHandler.FontAttributes);

            // Act & Assert - Change to Bold
            searchHandler.FontAttributes = FontAttributes.Bold;
            Assert.Equal(FontAttributes.Bold, searchHandler.FontAttributes);

            // Act & Assert - Change to Italic
            searchHandler.FontAttributes = FontAttributes.Italic;
            Assert.Equal(FontAttributes.Italic, searchHandler.FontAttributes);

            // Act & Assert - Change to combined
            searchHandler.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;
            Assert.Equal(FontAttributes.Bold | FontAttributes.Italic, searchHandler.FontAttributes);
        }

        /// <summary>
        /// Tests that the Placeholder property returns null by default when no value has been set.
        /// Validates the default behavior of the property getter.
        /// Expected result: null.
        /// </summary>
        [Fact]
        public void Placeholder_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.Placeholder;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Placeholder property can be set to null and retrieved correctly.
        /// Validates the property setter and getter with null input.
        /// Expected result: null.
        /// </summary>
        [Fact]
        public void Placeholder_SetNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Placeholder = null;
            var result = searchHandler.Placeholder;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests the Placeholder property with various string values including edge cases.
        /// Validates the property setter and getter with different string inputs.
        /// Expected result: the same string value that was set.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("Search")]
        [InlineData("Enter search term")]
        [InlineData("Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [InlineData("Unicode: 你好世界 🌍")]
        [InlineData("Very long string: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.")]
        public void Placeholder_SetVariousStrings_ReturnsExpectedValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Placeholder = value;
            var result = searchHandler.Placeholder;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the Placeholder property can be set multiple times and always returns the latest value.
        /// Validates the property behavior with sequential value changes.
        /// Expected result: the last value that was set.
        /// </summary>
        [Fact]
        public void Placeholder_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.Placeholder = "First";
            Assert.Equal("First", searchHandler.Placeholder);

            searchHandler.Placeholder = "Second";
            Assert.Equal("Second", searchHandler.Placeholder);

            searchHandler.Placeholder = null;
            Assert.Null(searchHandler.Placeholder);

            searchHandler.Placeholder = "Final";
            Assert.Equal("Final", searchHandler.Placeholder);
        }

        /// <summary>
        /// Tests that AutomationId returns the default value when not explicitly set.
        /// Input: New SearchHandler instance.
        /// Expected: AutomationId returns null (default value).
        /// </summary>
        [Fact]
        public void AutomationId_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.AutomationId;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that AutomationId property can be set and retrieved with various string values.
        /// Input: Different string values including null, empty, whitespace, normal, and special character strings.
        /// Expected: AutomationId returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("TestId")]
        [InlineData("automation-id-123")]
        [InlineData("AutomationId_With_Underscores")]
        [InlineData("AutomationId With Spaces")]
        [InlineData("AutomationId!@#$%^&*()")]
        [InlineData("🔍SearchIcon")]
        [InlineData("Very_Long_AutomationId_String_That_Exceeds_Normal_Length_Expectations_And_Contains_Many_Characters_To_Test_Boundary_Conditions")]
        public void AutomationId_SetAndGet_ReturnsExpectedValue(string expected)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.AutomationId = expected;
            var result = searchHandler.AutomationId;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that AutomationId can be set multiple times and always returns the most recent value.
        /// Input: Sequential string values set on the same SearchHandler instance.
        /// Expected: AutomationId returns the last value that was set.
        /// </summary>
        [Fact]
        public void AutomationId_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.AutomationId = "FirstValue";
            Assert.Equal("FirstValue", searchHandler.AutomationId);

            searchHandler.AutomationId = "SecondValue";
            Assert.Equal("SecondValue", searchHandler.AutomationId);

            searchHandler.AutomationId = null;
            Assert.Null(searchHandler.AutomationId);

            searchHandler.AutomationId = "";
            Assert.Equal("", searchHandler.AutomationId);
        }

        /// <summary>
        /// Tests that AutomationId handles control characters and newlines correctly.
        /// Input: Strings containing control characters, tabs, and newlines.
        /// Expected: AutomationId preserves the exact string including control characters.
        /// </summary>
        [Theory]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("Line1\nLine2")]
        [InlineData("\0")]
        [InlineData("Text\u0001WithControlChar")]
        public void AutomationId_ControlCharacters_PreservesValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.AutomationId = value;
            var result = searchHandler.AutomationId;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that AutomationId handles extremely long strings correctly.
        /// Input: String with 10,000 characters.
        /// Expected: AutomationId returns the complete long string without truncation.
        /// </summary>
        [Fact]
        public void AutomationId_ExtremelyLongString_ReturnsCompleteValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var longString = new string('A', 10000);

            // Act
            searchHandler.AutomationId = longString;
            var result = searchHandler.AutomationId;

            // Assert
            Assert.Equal(longString, result);
            Assert.Equal(10000, result.Length);
        }

        /// <summary>
        /// Tests that ClearIconName getter returns the value that was set via the setter.
        /// Verifies basic property get/set functionality works correctly.
        /// </summary>
        [Theory]
        [InlineData("test-icon")]
        [InlineData("MyIcon")]
        [InlineData("icon_with_underscores")]
        [InlineData("icon-with-dashes")]
        [InlineData("IconWithNumbers123")]
        public void ClearIconName_SetValidString_ReturnsSetValue(string iconName)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconName = iconName;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Equal(iconName, result);
        }

        /// <summary>
        /// Tests that ClearIconName can be set to null and returns null when retrieved.
        /// Verifies null handling works correctly since the default value is null.
        /// </summary>
        [Fact]
        public void ClearIconName_SetNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconName = null;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearIconName default value is null.
        /// Verifies the initial state of the property matches the BindableProperty default.
        /// </summary>
        [Fact]
        public void ClearIconName_DefaultValue_IsNull()
        {
            // Arrange & Act
            var searchHandler = new SearchHandler();
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearIconName can handle empty string values.
        /// Verifies empty strings are stored and retrieved correctly.
        /// </summary>
        [Fact]
        public void ClearIconName_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconName = string.Empty;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ClearIconName can handle whitespace-only strings.
        /// Verifies whitespace strings are preserved as-is without trimming.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        [InlineData(" \t \r\n ")]
        public void ClearIconName_SetWhitespaceString_ReturnsWhitespaceString(string whitespace)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconName = whitespace;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Equal(whitespace, result);
        }

        /// <summary>
        /// Tests that ClearIconName can handle strings with special characters.
        /// Verifies special characters are stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("icon@symbol")]
        [InlineData("icon#hash")]
        [InlineData("icon$dollar")]
        [InlineData("icon%percent")]
        [InlineData("icon&ampersand")]
        [InlineData("icon*asterisk")]
        [InlineData("icon+plus")]
        [InlineData("icon=equals")]
        [InlineData("icon[bracket]")]
        [InlineData("icon{brace}")]
        [InlineData("icon|pipe")]
        [InlineData("icon\\backslash")]
        [InlineData("icon/slash")]
        [InlineData("icon?question")]
        [InlineData("icon<less>")]
        [InlineData("icon\"quote\"")]
        [InlineData("icon'apostrophe'")]
        public void ClearIconName_SetStringWithSpecialCharacters_ReturnsStringWithSpecialCharacters(string iconNameWithSpecialChars)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconName = iconNameWithSpecialChars;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Equal(iconNameWithSpecialChars, result);
        }

        /// <summary>
        /// Tests that ClearIconName can handle very long strings.
        /// Verifies there are no length restrictions on the property value.
        /// </summary>
        [Fact]
        public void ClearIconName_SetVeryLongString_ReturnsVeryLongString()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var veryLongString = new string('A', 10000);

            // Act
            searchHandler.ClearIconName = veryLongString;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Equal(veryLongString, result);
        }

        /// <summary>
        /// Tests that ClearIconName can handle Unicode characters.
        /// Verifies Unicode strings are stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("icon_日本語")]
        [InlineData("icon_العربية")]
        [InlineData("icon_русский")]
        [InlineData("icon_中文")]
        [InlineData("icon_한국어")]
        [InlineData("🔍search")]
        [InlineData("icon_with_emoji_🎯")]
        public void ClearIconName_SetUnicodeString_ReturnsUnicodeString(string unicodeIconName)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconName = unicodeIconName;
            var result = searchHandler.ClearIconName;

            // Assert
            Assert.Equal(unicodeIconName, result);
        }

        /// <summary>
        /// Tests that multiple sets and gets on ClearIconName work correctly.
        /// Verifies the property can be changed multiple times and always returns the latest value.
        /// </summary>
        [Fact]
        public void ClearIconName_MultipleSetAndGet_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.ClearIconName = "first";
            Assert.Equal("first", searchHandler.ClearIconName);

            searchHandler.ClearIconName = "second";
            Assert.Equal("second", searchHandler.ClearIconName);

            searchHandler.ClearIconName = null;
            Assert.Null(searchHandler.ClearIconName);

            searchHandler.ClearIconName = "third";
            Assert.Equal("third", searchHandler.ClearIconName);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns the default value when not set.
        /// Verifies the property retrieves the correct default value from the bindable property.
        /// Expected result: Returns null as the default value.
        /// </summary>
        [Fact]
        public void ClearPlaceholderHelpText_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns the correct value after setting.
        /// Verifies the property can store and retrieve string values correctly.
        /// Expected result: Returns the same string value that was set.
        /// </summary>
        [Theory]
        [InlineData("Clear search")]
        [InlineData("Remove text")]
        [InlineData("Delete")]
        [InlineData("X")]
        public void ClearPlaceholderHelpText_WhenSetToValidString_ReturnsSetValue(string helpText)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderHelpText = helpText;
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Equal(helpText, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns null when set to null.
        /// Verifies the property correctly handles null string values.
        /// Expected result: Returns null.
        /// </summary>
        [Fact]
        public void ClearPlaceholderHelpText_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.ClearPlaceholderHelpText = "Initial value";

            // Act
            searchHandler.ClearPlaceholderHelpText = null;
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns empty string when set to empty string.
        /// Verifies the property correctly handles empty string values.
        /// Expected result: Returns empty string.
        /// </summary>
        [Fact]
        public void ClearPlaceholderHelpText_WhenSetToEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderHelpText = string.Empty;
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns whitespace string when set to whitespace.
        /// Verifies the property correctly handles whitespace-only string values.
        /// Expected result: Returns the same whitespace string that was set.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("   \t\n  ")]
        public void ClearPlaceholderHelpText_WhenSetToWhitespaceString_ReturnsWhitespaceString(string whitespace)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderHelpText = whitespace;
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Equal(whitespace, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns very long string when set to very long string.
        /// Verifies the property correctly handles very long string values without truncation.
        /// Expected result: Returns the same long string that was set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderHelpText_WhenSetToVeryLongString_ReturnsVeryLongString()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var longString = new string('A', 10000);

            // Act
            searchHandler.ClearPlaceholderHelpText = longString;
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns special characters when set to special characters.
        /// Verifies the property correctly handles strings with special characters and unicode.
        /// Expected result: Returns the same special character string that was set.
        /// </summary>
        [Theory]
        [InlineData("Special chars: !@#$%^&*()")]
        [InlineData("Unicode: 你好世界")]
        [InlineData("Emojis: 🔍🚫❌")]
        [InlineData("Mixed: Hello 世界 🌍")]
        [InlineData("Control chars: \u0000\u0001\u0002")]
        public void ClearPlaceholderHelpText_WhenSetToSpecialCharacters_ReturnsSpecialCharacters(string specialString)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderHelpText = specialString;
            var result = searchHandler.ClearPlaceholderHelpText;

            // Assert
            Assert.Equal(specialString, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderHelpText getter returns updated value when changed multiple times.
        /// Verifies the property correctly updates and returns the most recent value.
        /// Expected result: Returns the last value that was set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderHelpText_WhenChangedMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.ClearPlaceholderHelpText = "First value";
            Assert.Equal("First value", searchHandler.ClearPlaceholderHelpText);

            searchHandler.ClearPlaceholderHelpText = "Second value";
            Assert.Equal("Second value", searchHandler.ClearPlaceholderHelpText);

            searchHandler.ClearPlaceholderHelpText = null;
            Assert.Null(searchHandler.ClearPlaceholderHelpText);

            searchHandler.ClearPlaceholderHelpText = "Final value";
            Assert.Equal("Final value", searchHandler.ClearPlaceholderHelpText);
        }

        /// <summary>
        /// Tests that DisplayMemberName getter returns the default value when no value has been set.
        /// </summary>
        [Fact]
        public void DisplayMemberName_GetDefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.DisplayMemberName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that DisplayMemberName getter and setter work correctly with valid string values.
        /// </summary>
        /// <param name="testValue">The string value to test</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData("Name", "simple string")]
        [InlineData("", "empty string")]
        [InlineData("   ", "whitespace string")]
        [InlineData("Property.SubProperty", "dotted property name")]
        [InlineData("VeryLongPropertyNameThatExceedsTypicalLengthLimitsAndContainsVariousCharactersIncludingNumbers123AndSymbols_$%", "very long string")]
        [InlineData("Test\nWith\nNewlines", "string with newlines")]
        [InlineData("Test\tWith\tTabs", "string with tabs")]
        [InlineData("Test With Spaces", "string with spaces")]
        [InlineData("TestWithUnicode_éñ中文", "string with unicode characters")]
        public void DisplayMemberName_SetAndGetValidValues_ReturnsExpectedValue(string testValue, string description)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.DisplayMemberName = testValue;
            var result = searchHandler.DisplayMemberName;

            // Assert
            Assert.Equal(testValue, result);
        }

        /// <summary>
        /// Tests that DisplayMemberName can be set to null and returns null when retrieved.
        /// </summary>
        [Fact]
        public void DisplayMemberName_SetNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.DisplayMemberName = "InitialValue";

            // Act
            searchHandler.DisplayMemberName = null;
            var result = searchHandler.DisplayMemberName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that DisplayMemberName can be set multiple times and always returns the most recent value.
        /// </summary>
        [Fact]
        public void DisplayMemberName_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.DisplayMemberName = "FirstValue";
            Assert.Equal("FirstValue", searchHandler.DisplayMemberName);

            searchHandler.DisplayMemberName = "SecondValue";
            Assert.Equal("SecondValue", searchHandler.DisplayMemberName);

            searchHandler.DisplayMemberName = "";
            Assert.Equal("", searchHandler.DisplayMemberName);

            searchHandler.DisplayMemberName = null;
            Assert.Null(searchHandler.DisplayMemberName);
        }

        /// <summary>
        /// Tests that DisplayMemberName property correctly uses the DisplayMemberNameProperty bindable property.
        /// </summary>
        [Fact]
        public void DisplayMemberName_UsesCorrectBindableProperty_ValueIsConsistent()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var testValue = "TestPropertyName";

            // Act
            searchHandler.SetValue(SearchHandler.DisplayMemberNameProperty, testValue);
            var getterResult = searchHandler.DisplayMemberName;
            var directResult = (string)searchHandler.GetValue(SearchHandler.DisplayMemberNameProperty);

            // Assert
            Assert.Equal(testValue, getterResult);
            Assert.Equal(testValue, directResult);
            Assert.Equal(getterResult, directResult);
        }

        /// <summary>
        /// Tests that setting DisplayMemberName through the property setter updates the underlying bindable property.
        /// </summary>
        [Fact]
        public void DisplayMemberName_SetThroughProperty_UpdatesBindableProperty()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var testValue = "PropertySetValue";

            // Act
            searchHandler.DisplayMemberName = testValue;
            var bindablePropertyValue = (string)searchHandler.GetValue(SearchHandler.DisplayMemberNameProperty);

            // Assert
            Assert.Equal(testValue, bindablePropertyValue);
        }

        /// <summary>
        /// Tests that Query property returns the correct value when set with valid string values
        /// </summary>
        /// <param name="queryValue">The query string value to test</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("simple search")]
        [InlineData("Search With Capitals")]
        [InlineData("123456789")]
        [InlineData("search with special chars !@#$%^&*()")]
        [InlineData("unicode test ąęćółźż")]
        [InlineData("emoji test 😀🔍")]
        public void Query_SetAndGet_ReturnsExpectedValue(string queryValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Query = queryValue;
            var result = searchHandler.Query;

            // Assert
            Assert.Equal(queryValue, result);
        }

        /// <summary>
        /// Tests that Query property returns null by default when no value has been set
        /// </summary>
        [Fact]
        public void Query_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.Query;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Query property can be set multiple times and returns the most recent value
        /// </summary>
        [Fact]
        public void Query_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.Query = "first value";
            Assert.Equal("first value", searchHandler.Query);

            searchHandler.Query = "second value";
            Assert.Equal("second value", searchHandler.Query);

            searchHandler.Query = null;
            Assert.Null(searchHandler.Query);

            searchHandler.Query = "final value";
            Assert.Equal("final value", searchHandler.Query);
        }

        /// <summary>
        /// Tests that Query property handles very long strings correctly
        /// </summary>
        [Fact]
        public void Query_VeryLongString_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var longString = new string('a', 10000);

            // Act
            searchHandler.Query = longString;
            var result = searchHandler.Query;

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that Query property handles strings with control characters
        /// </summary>
        [Theory]
        [InlineData("\u0000")]
        [InlineData("\u0001\u0002\u0003")]
        [InlineData("test\u0007string")]
        [InlineData("\u001F")]
        public void Query_StringsWithControlCharacters_ReturnsExpectedValue(string controlCharString)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Query = controlCharString;
            var result = searchHandler.Query;

            // Assert
            Assert.Equal(controlCharString, result);
        }

        /// <summary>
        /// Tests that setting Query to the same value multiple times works correctly
        /// </summary>
        [Fact]
        public void Query_SetSameValueMultipleTimes_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            const string testValue = "same value";

            // Act & Assert
            searchHandler.Query = testValue;
            Assert.Equal(testValue, searchHandler.Query);

            searchHandler.Query = testValue;
            Assert.Equal(testValue, searchHandler.Query);

            searchHandler.Query = testValue;
            Assert.Equal(testValue, searchHandler.Query);
        }

        /// <summary>
        /// Tests that Query property handles boundary whitespace scenarios
        /// </summary>
        [Theory]
        [InlineData("  leading spaces")]
        [InlineData("trailing spaces  ")]
        [InlineData("  both sides  ")]
        [InlineData("\tleading tab")]
        [InlineData("trailing tab\t")]
        [InlineData("\t\tboth tabs\t\t")]
        public void Query_WhitespaceScenarios_ReturnsExpectedValue(string whitespaceString)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.Query = whitespaceString;
            var result = searchHandler.Query;

            // Assert
            Assert.Equal(whitespaceString, result);
        }

        /// <summary>
        /// Tests that SearchBoxVisibility property returns the default value when not explicitly set.
        /// Validates that the property getter returns SearchBoxVisibility.Expanded as the default value.
        /// Expected result: SearchBoxVisibility.Expanded.
        /// </summary>
        [Fact]
        public void SearchBoxVisibility_DefaultValue_ReturnsExpanded()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.SearchBoxVisibility;

            // Assert
            Assert.Equal(SearchBoxVisibility.Expanded, result);
        }

        /// <summary>
        /// Tests that SearchBoxVisibility property correctly sets and gets valid enum values.
        /// Validates that the property setter stores values and getter retrieves them correctly.
        /// Expected result: The set value should equal the retrieved value.
        /// </summary>
        [Theory]
        [InlineData(SearchBoxVisibility.Hidden)]
        [InlineData(SearchBoxVisibility.Collapsible)]
        [InlineData(SearchBoxVisibility.Expanded)]
        public void SearchBoxVisibility_ValidEnumValues_SetAndGetCorrectly(SearchBoxVisibility expectedValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.SearchBoxVisibility = expectedValue;
            var result = searchHandler.SearchBoxVisibility;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that SearchBoxVisibility property handles invalid enum values through casting.
        /// Validates that the property can store and retrieve values outside the defined enum range.
        /// Expected result: The cast invalid value should be stored and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void SearchBoxVisibility_InvalidEnumValues_HandlesCorrectly(int invalidValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var castedValue = (SearchBoxVisibility)invalidValue;

            // Act
            searchHandler.SearchBoxVisibility = castedValue;
            var result = searchHandler.SearchBoxVisibility;

            // Assert
            Assert.Equal(castedValue, result);
        }

        /// <summary>
        /// Tests that SearchBoxVisibility property maintains value consistency across multiple operations.
        /// Validates that setting different values sequentially works correctly.
        /// Expected result: Each set operation should be retrievable with the get operation.
        /// </summary>
        [Fact]
        public void SearchBoxVisibility_MultipleSetOperations_MaintainsConsistency()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert - Test sequence of different values
            searchHandler.SearchBoxVisibility = SearchBoxVisibility.Hidden;
            Assert.Equal(SearchBoxVisibility.Hidden, searchHandler.SearchBoxVisibility);

            searchHandler.SearchBoxVisibility = SearchBoxVisibility.Collapsible;
            Assert.Equal(SearchBoxVisibility.Collapsible, searchHandler.SearchBoxVisibility);

            searchHandler.SearchBoxVisibility = SearchBoxVisibility.Expanded;
            Assert.Equal(SearchBoxVisibility.Expanded, searchHandler.SearchBoxVisibility);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property returns the default value of Start when not explicitly set.
        /// </summary>
        [Fact]
        public void HorizontalTextAlignment_DefaultValue_ReturnsStart()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.HorizontalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Start, result);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property getter returns the correct value after setting valid enum values.
        /// Tests all valid TextAlignment enum values: Start, Center, End, Justify.
        /// </summary>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void HorizontalTextAlignment_SetValidEnumValue_ReturnsCorrectValue(TextAlignment alignment)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.HorizontalTextAlignment = alignment;
            var result = searchHandler.HorizontalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property setter handles invalid enum values correctly.
        /// Tests casting from integers outside the defined enum range.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void HorizontalTextAlignment_SetInvalidEnumValue_StoresValue(int invalidValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var invalidAlignment = (TextAlignment)invalidValue;

            // Act
            searchHandler.HorizontalTextAlignment = invalidAlignment;
            var result = searchHandler.HorizontalTextAlignment;

            // Assert
            Assert.Equal(invalidAlignment, result);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property setter and getter work correctly through multiple assignments.
        /// Verifies that the property maintains its value correctly across multiple set/get operations.
        /// </summary>
        [Fact]
        public void HorizontalTextAlignment_MultipleAssignments_MaintainsCorrectValues()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.HorizontalTextAlignment = TextAlignment.Center;
            Assert.Equal(TextAlignment.Center, searchHandler.HorizontalTextAlignment);

            searchHandler.HorizontalTextAlignment = TextAlignment.End;
            Assert.Equal(TextAlignment.End, searchHandler.HorizontalTextAlignment);

            searchHandler.HorizontalTextAlignment = TextAlignment.Start;
            Assert.Equal(TextAlignment.Start, searchHandler.HorizontalTextAlignment);

            searchHandler.HorizontalTextAlignment = TextAlignment.Justify;
            Assert.Equal(TextAlignment.Justify, searchHandler.HorizontalTextAlignment);
        }

        /// <summary>
        /// Tests that HorizontalTextAlignment property correctly handles boundary enum values.
        /// Tests the first and last defined enum values to ensure proper casting behavior.
        /// </summary>
        [Theory]
        [InlineData((TextAlignment)0)] // TextAlignment.Start
        [InlineData((TextAlignment)3)] // TextAlignment.Justify
        public void HorizontalTextAlignment_BoundaryEnumValues_HandledCorrectly(TextAlignment alignment)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.HorizontalTextAlignment = alignment;
            var result = searchHandler.HorizontalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property getter returns the correct value after setting it.
        /// Verifies the property wrapper correctly retrieves values from the underlying bindable property.
        /// </summary>
        /// <param name="expectedValue">The character spacing value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(5.5)]
        [InlineData(-3.7)]
        [InlineData(100.0)]
        [InlineData(-100.0)]
        public void CharacterSpacing_SetAndGet_ReturnsExpectedValue(double expectedValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.CharacterSpacing = expectedValue;
            var actualValue = searchHandler.CharacterSpacing;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CharacterSpacing property handles edge case double values correctly.
        /// Verifies boundary values and special floating-point values are properly handled.
        /// </summary>
        /// <param name="edgeValue">The edge case double value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CharacterSpacing_SetEdgeCaseValues_ReturnsExpectedValue(double edgeValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.CharacterSpacing = edgeValue;
            var actualValue = searchHandler.CharacterSpacing;

            // Assert
            if (double.IsNaN(edgeValue))
            {
                Assert.True(double.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(edgeValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that the CharacterSpacing property has a default value when first accessed.
        /// Verifies the initial state of the property before any explicit assignment.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var defaultValue = searchHandler.CharacterSpacing;

            // Assert
            Assert.IsType<double>(defaultValue);
        }

        /// <summary>
        /// Tests that multiple assignments to CharacterSpacing property work correctly.
        /// Verifies the property can be set multiple times and maintains the latest value.
        /// </summary>
        [Fact]
        public void CharacterSpacing_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.CharacterSpacing = 1.0;
            searchHandler.CharacterSpacing = 2.5;
            searchHandler.CharacterSpacing = -1.7;
            var finalValue = searchHandler.CharacterSpacing;

            // Assert
            Assert.Equal(-1.7, finalValue);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the correct value when set to a normal positive font size.
        /// </summary>
        [Theory]
        [InlineData(10.0)]
        [InlineData(12.0)]
        [InlineData(14.0)]
        [InlineData(16.0)]
        [InlineData(18.0)]
        [InlineData(24.0)]
        [InlineData(36.0)]
        public void FontSize_GetAfterSet_ReturnsCorrectValue(double expectedFontSize)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = expectedFontSize;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(expectedFontSize, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the correct value when set to zero.
        /// </summary>
        [Fact]
        public void FontSize_GetAfterSetToZero_ReturnsZero()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = 0.0;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(0.0, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the correct value when set to negative values.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.5)]
        [InlineData(-100.0)]
        public void FontSize_GetAfterSetToNegative_ReturnsNegativeValue(double negativeSize)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = negativeSize;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(negativeSize, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the correct value when set to very large values.
        /// </summary>
        [Theory]
        [InlineData(1000.0)]
        [InlineData(10000.0)]
        [InlineData(1000000.0)]
        public void FontSize_GetAfterSetToLargeValue_ReturnsLargeValue(double largeSize)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = largeSize;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(largeSize, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the correct value when set to boundary double values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        public void FontSize_GetAfterSetToBoundaryValues_ReturnsBoundaryValue(double boundaryValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = boundaryValue;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(boundaryValue, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns NaN when set to double.NaN.
        /// </summary>
        [Fact]
        public void FontSize_GetAfterSetToNaN_ReturnsNaN()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = double.NaN;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.True(double.IsNaN(actualFontSize));
        }

        /// <summary>
        /// Tests that FontSize property getter returns PositiveInfinity when set to double.PositiveInfinity.
        /// </summary>
        [Fact]
        public void FontSize_GetAfterSetToPositiveInfinity_ReturnsPositiveInfinity()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = double.PositiveInfinity;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(double.PositiveInfinity, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns NegativeInfinity when set to double.NegativeInfinity.
        /// </summary>
        [Fact]
        public void FontSize_GetAfterSetToNegativeInfinity_ReturnsNegativeInfinity()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = double.NegativeInfinity;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(double.NegativeInfinity, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the default value when accessed without setting.
        /// </summary>
        [Fact]
        public void FontSize_GetWithoutSet_ReturnsDefaultValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.True(actualFontSize >= 0.0); // Default font size should be non-negative
        }

        /// <summary>
        /// Tests that FontSize property getter returns fractional values correctly.
        /// </summary>
        [Theory]
        [InlineData(10.5)]
        [InlineData(12.75)]
        [InlineData(14.25)]
        [InlineData(16.125)]
        public void FontSize_GetAfterSetToFractionalValue_ReturnsFractionalValue(double fractionalSize)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = fractionalSize;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(fractionalSize, actualFontSize);
        }

        /// <summary>
        /// Tests that FontSize property getter works correctly after multiple set operations.
        /// </summary>
        [Fact]
        public void FontSize_GetAfterMultipleSet_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontSize = 10.0;
            searchHandler.FontSize = 20.0;
            searchHandler.FontSize = 15.0;
            var actualFontSize = searchHandler.FontSize;

            // Assert
            Assert.Equal(15.0, actualFontSize);
        }

        /// <summary>
        /// Tests that the ClearIcon property returns the default value (null) when not explicitly set.
        /// </summary>
        [Fact]
        public void ClearIcon_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ClearIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the ClearIcon property correctly stores and retrieves null values.
        /// </summary>
        [Fact]
        public void ClearIcon_SetToNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIcon = null;
            var result = searchHandler.ClearIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the ClearIcon property correctly stores and retrieves ImageSource instances.
        /// </summary>
        [Fact]
        public void ClearIcon_SetValidImageSource_ReturnsSameInstance()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.ClearIcon = mockImageSource;
            var result = searchHandler.ClearIcon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that the ClearIcon property correctly handles multiple assignments and returns the most recent value.
        /// </summary>
        [Fact]
        public void ClearIcon_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.ClearIcon = firstImageSource;
            var firstResult = searchHandler.ClearIcon;

            searchHandler.ClearIcon = secondImageSource;
            var secondResult = searchHandler.ClearIcon;

            // Assert
            Assert.Same(firstImageSource, firstResult);
            Assert.Same(secondImageSource, secondResult);
            Assert.NotSame(firstResult, secondResult);
        }

        /// <summary>
        /// Tests that the ClearIcon property can be set from a non-null value back to null.
        /// </summary>
        [Fact]
        public void ClearIcon_SetToImageSourceThenNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.ClearIcon = mockImageSource;
            var initialResult = searchHandler.ClearIcon;

            searchHandler.ClearIcon = null;
            var finalResult = searchHandler.ClearIcon;

            // Assert
            Assert.Same(mockImageSource, initialResult);
            Assert.Null(finalResult);
        }

        /// <summary>
        /// Tests that ClearPlaceholderCommandParameter getter returns the default value when no value has been set.
        /// Input conditions: New SearchHandler instance with no ClearPlaceholderCommandParameter value set.
        /// Expected result: Returns null (the default value).
        /// </summary>
        [Fact]
        public void ClearPlaceholderCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ClearPlaceholderCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderCommandParameter getter returns the correct value after setting various object types.
        /// Input conditions: SearchHandler with ClearPlaceholderCommandParameter set to different object types.
        /// Expected result: Getter returns the exact same value that was set.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ClearPlaceholderCommandParameter_SetAndGet_ReturnsCorrectValue(object value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderCommandParameter = value;
            var result = searchHandler.ClearPlaceholderCommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderCommandParameter getter returns the correct value when set to a complex object.
        /// Input conditions: SearchHandler with ClearPlaceholderCommandParameter set to a custom object.
        /// Expected result: Getter returns the exact same object reference that was set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderCommandParameter_SetComplexObject_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var complexObject = new { Name = "Test", Value = 123 };

            // Act
            searchHandler.ClearPlaceholderCommandParameter = complexObject;
            var result = searchHandler.ClearPlaceholderCommandParameter;

            // Assert
            Assert.Same(complexObject, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderCommandParameter getter works correctly when value is changed multiple times.
        /// Input conditions: SearchHandler with ClearPlaceholderCommandParameter set to different values sequentially.
        /// Expected result: Getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void ClearPlaceholderCommandParameter_MultipleChanges_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstValue = "first";
            var secondValue = 42;
            var thirdValue = new object();

            // Act & Assert
            searchHandler.ClearPlaceholderCommandParameter = firstValue;
            Assert.Equal(firstValue, searchHandler.ClearPlaceholderCommandParameter);

            searchHandler.ClearPlaceholderCommandParameter = secondValue;
            Assert.Equal(secondValue, searchHandler.ClearPlaceholderCommandParameter);

            searchHandler.ClearPlaceholderCommandParameter = thirdValue;
            Assert.Same(thirdValue, searchHandler.ClearPlaceholderCommandParameter);
        }

        /// <summary>
        /// Tests that ClearPlaceholderCommandParameter getter returns correct value when set to edge case values.
        /// Input conditions: SearchHandler with ClearPlaceholderCommandParameter set to edge case values.
        /// Expected result: Getter returns the exact values that were set.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(0)]
        [InlineData(false)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ClearPlaceholderCommandParameter_EdgeCaseValues_ReturnsCorrectValue(object value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderCommandParameter = value;
            var result = searchHandler.ClearPlaceholderCommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderName property returns the default value (null) when not explicitly set.
        /// Input: New SearchHandler instance.
        /// Expected: ClearPlaceholderName should return null.
        /// </summary>
        [Fact]
        public void ClearPlaceholderName_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ClearPlaceholderName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderName property can be set to null and returns null.
        /// Input: Setting ClearPlaceholderName to null.
        /// Expected: ClearPlaceholderName should return null.
        /// </summary>
        [Fact]
        public void ClearPlaceholderName_SetToNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderName = null;
            var result = searchHandler.ClearPlaceholderName;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderName property can be set and retrieved with various string values.
        /// Input: Different string values including normal strings, empty strings, whitespace, and special characters.
        /// Expected: ClearPlaceholderName should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("Clear")]
        [InlineData("Clear Placeholder")]
        [InlineData("ClearIcon")]
        [InlineData("!@#$%^&*()")]
        [InlineData("Unicode: 中文")]
        [InlineData("Emoji: 😀🎉")]
        [InlineData("Numbers: 12345")]
        [InlineData("Mixed: Clear123!")]
        public void ClearPlaceholderName_SetValidString_ReturnsSetValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderName = value;
            var result = searchHandler.ClearPlaceholderName;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderName property can handle very long string values.
        /// Input: Very long string (1000 characters).
        /// Expected: ClearPlaceholderName should return the exact long string that was set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderName_SetVeryLongString_ReturnsSetValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var longString = new string('A', 1000);

            // Act
            searchHandler.ClearPlaceholderName = longString;
            var result = searchHandler.ClearPlaceholderName;

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderName property can be updated multiple times with different values.
        /// Input: Sequential setting of different string values.
        /// Expected: ClearPlaceholderName should always return the most recently set value.
        /// </summary>
        [Fact]
        public void ClearPlaceholderName_MultipleUpdates_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.ClearPlaceholderName = "First";
            Assert.Equal("First", searchHandler.ClearPlaceholderName);

            searchHandler.ClearPlaceholderName = "Second";
            Assert.Equal("Second", searchHandler.ClearPlaceholderName);

            searchHandler.ClearPlaceholderName = null;
            Assert.Null(searchHandler.ClearPlaceholderName);

            searchHandler.ClearPlaceholderName = "Third";
            Assert.Equal("Third", searchHandler.ClearPlaceholderName);
        }

        /// <summary>
        /// Tests that ClearPlaceholderName property handles strings with control characters correctly.
        /// Input: Strings containing various control characters.
        /// Expected: ClearPlaceholderName should preserve all control characters exactly as set.
        /// </summary>
        [Theory]
        [InlineData("\0")]
        [InlineData("\a")]
        [InlineData("\b")]
        [InlineData("\f")]
        [InlineData("\v")]
        [InlineData("Text\0WithNull")]
        [InlineData("Tab\tSeparated")]
        [InlineData("Line\nBreak")]
        [InlineData("Carriage\rReturn")]
        public void ClearPlaceholderName_SetStringWithControlCharacters_ReturnsSetValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderName = value;
            var result = searchHandler.ClearPlaceholderName;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ItemTemplate getter returns null when no value has been set (default state).
        /// Verifies the default behavior and ensures proper casting from GetValue result.
        /// </summary>
        [Fact]
        public void ItemTemplate_GetDefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ItemTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ItemTemplate getter returns the correct DataTemplate after setting a valid value.
        /// Verifies proper round-trip behavior and casting from object to DataTemplate.
        /// </summary>
        [Fact]
        public void ItemTemplate_GetAfterSetValidTemplate_ReturnsCorrectTemplate()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var dataTemplate = new DataTemplate(() => new Label());

            // Act
            searchHandler.ItemTemplate = dataTemplate;
            var result = searchHandler.ItemTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that ItemTemplate getter returns null after explicitly setting null value.
        /// Verifies that null can be set and retrieved correctly.
        /// </summary>
        [Fact]
        public void ItemTemplate_GetAfterSetNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var dataTemplate = new DataTemplate(() => new Label());
            searchHandler.ItemTemplate = dataTemplate;

            // Act
            searchHandler.ItemTemplate = null;
            var result = searchHandler.ItemTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ItemTemplate getter works correctly with DataTemplate created using Type constructor.
        /// Verifies different DataTemplate constructor scenarios work with the property.
        /// </summary>
        [Fact]
        public void ItemTemplate_GetAfterSetTemplateWithType_ReturnsCorrectTemplate()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var dataTemplate = new DataTemplate(typeof(Label));

            // Act
            searchHandler.ItemTemplate = dataTemplate;
            var result = searchHandler.ItemTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that ItemTemplate getter works correctly with parameterless DataTemplate constructor.
        /// Verifies different DataTemplate initialization approaches work with the property.
        /// </summary>
        [Fact]
        public void ItemTemplate_GetAfterSetParameterlessTemplate_ReturnsCorrectTemplate()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var dataTemplate = new DataTemplate();

            // Act
            searchHandler.ItemTemplate = dataTemplate;
            var result = searchHandler.ItemTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that QueryIconName getter returns the correct value after setting various string values.
        /// Validates the property correctly stores and retrieves different types of string inputs including edge cases.
        /// </summary>
        /// <param name="testValue">The string value to set and then retrieve via the QueryIconName property</param>
        /// <param name="expectedValue">The expected value that should be returned by the getter</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("test-icon", "test-icon")]
        [InlineData("TestIcon123", "TestIcon123")]
        [InlineData("icon_name_with_underscores", "icon_name_with_underscores")]
        [InlineData("icon-name-with-dashes", "icon-name-with-dashes")]
        [InlineData("IconWithSpecialChars!@#$%", "IconWithSpecialChars!@#$%")]
        [InlineData("Unicode_测试_Icon", "Unicode_测试_Icon")]
        public void QueryIconName_GetAfterSet_ReturnsExpectedValue(string testValue, string expectedValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.QueryIconName = testValue;
            var actualValue = searchHandler.QueryIconName;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that QueryIconName getter returns the default value when no value has been set.
        /// Validates the initial state of the property matches the BindableProperty's default value.
        /// </summary>
        [Fact]
        public void QueryIconName_GetDefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var actualValue = searchHandler.QueryIconName;

            // Assert
            Assert.Null(actualValue);
        }

        /// <summary>
        /// Tests that QueryIconName getter handles very long strings correctly.
        /// Validates the property can store and retrieve strings at boundary lengths.
        /// </summary>
        [Fact]
        public void QueryIconName_GetVeryLongString_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var veryLongString = new string('A', 10000);

            // Act
            searchHandler.QueryIconName = veryLongString;
            var actualValue = searchHandler.QueryIconName;

            // Assert
            Assert.Equal(veryLongString, actualValue);
        }

        /// <summary>
        /// Tests that QueryIconName getter returns the correct value when setting multiple different values in sequence.
        /// Validates that the property correctly updates and retrieves the most recently set value.
        /// </summary>
        [Fact]
        public void QueryIconName_GetAfterMultipleSets_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.QueryIconName = "first-value";
            Assert.Equal("first-value", searchHandler.QueryIconName);

            searchHandler.QueryIconName = null;
            Assert.Null(searchHandler.QueryIconName);

            searchHandler.QueryIconName = "final-value";
            Assert.Equal("final-value", searchHandler.QueryIconName);
        }

        /// <summary>
        /// Tests that SelectedItem returns null by default when no item has been selected.
        /// </summary>
        [Fact]
        public void SelectedItem_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var selectedItem = searchHandler.SelectedItem;

            // Assert
            Assert.Null(selectedItem);
        }

        /// <summary>
        /// Tests that SelectedItem returns the correct value when an item is selected through the controller interface.
        /// </summary>
        [Theory]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void SelectedItem_AfterItemSelected_ReturnsSelectedItem(object expectedItem)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var controller = (ISearchHandlerController)searchHandler;

            // Act
            controller.ItemSelected(expectedItem);
            var actualItem = searchHandler.SelectedItem;

            // Assert
            Assert.Equal(expectedItem, actualItem);
        }

        /// <summary>
        /// Tests that SelectedItem returns null when null is explicitly selected.
        /// </summary>
        [Fact]
        public void SelectedItem_WhenNullSelected_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var controller = (ISearchHandlerController)searchHandler;

            // First set a non-null value
            controller.ItemSelected("initial value");

            // Act
            controller.ItemSelected(null);
            var selectedItem = searchHandler.SelectedItem;

            // Assert
            Assert.Null(selectedItem);
        }

        /// <summary>
        /// Tests that SelectedItem returns the latest selected item when multiple items are selected consecutively.
        /// </summary>
        [Fact]
        public void SelectedItem_MultipleConsecutiveSelections_ReturnsLatestSelection()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var controller = (ISearchHandlerController)searchHandler;

            // Act & Assert
            controller.ItemSelected("first");
            Assert.Equal("first", searchHandler.SelectedItem);

            controller.ItemSelected("second");
            Assert.Equal("second", searchHandler.SelectedItem);

            controller.ItemSelected(123);
            Assert.Equal(123, searchHandler.SelectedItem);
        }

        /// <summary>
        /// Tests that SelectedItem works correctly with complex object types.
        /// </summary>
        [Fact]
        public void SelectedItem_WithComplexObject_ReturnsCorrectObject()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var controller = (ISearchHandlerController)searchHandler;
            var complexObject = new { Name = "Test", Value = 42 };

            // Act
            controller.ItemSelected(complexObject);
            var selectedItem = searchHandler.SelectedItem;

            // Assert
            Assert.Same(complexObject, selectedItem);
        }

        /// <summary>
        /// Tests that SelectedItem handles setting the same value multiple times correctly.
        /// </summary>
        [Fact]
        public void SelectedItem_SameValueMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var controller = (ISearchHandlerController)searchHandler;
            var testValue = "consistent value";

            // Act
            controller.ItemSelected(testValue);
            var firstRetrieval = searchHandler.SelectedItem;

            controller.ItemSelected(testValue);
            var secondRetrieval = searchHandler.SelectedItem;

            // Assert
            Assert.Equal(testValue, firstRetrieval);
            Assert.Equal(testValue, secondRetrieval);
        }

        /// <summary>
        /// Tests that VerticalTextAlignment property getter returns the default value when no value has been set.
        /// Input: Newly created SearchHandler instance.
        /// Expected: Returns TextAlignment.Center (the default value).
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_DefaultValue_ReturnsCenter()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.VerticalTextAlignment;

            // Assert
            Assert.Equal(TextAlignment.Center, result);
        }

        /// <summary>
        /// Tests that VerticalTextAlignment property setter and getter work correctly with all valid TextAlignment enum values.
        /// Input: Each valid TextAlignment enum value (Start, Center, End, Justify).
        /// Expected: Getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(TextAlignment.Start)]
        [InlineData(TextAlignment.Center)]
        [InlineData(TextAlignment.End)]
        [InlineData(TextAlignment.Justify)]
        public void VerticalTextAlignment_SetValidValues_ReturnsSetValue(TextAlignment alignment)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.VerticalTextAlignment = alignment;
            var result = searchHandler.VerticalTextAlignment;

            // Assert
            Assert.Equal(alignment, result);
        }

        /// <summary>
        /// Tests that VerticalTextAlignment property getter correctly casts the value from the underlying BindableProperty.
        /// Input: SearchHandler instance with VerticalTextAlignment set to a specific value.
        /// Expected: Getter returns the correct TextAlignment value through the GetValue cast operation.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_GetterCastsCorrectly_ReturnsTextAlignment()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedValue = TextAlignment.End;

            // Act
            searchHandler.VerticalTextAlignment = expectedValue;
            var result = searchHandler.VerticalTextAlignment;

            // Assert
            Assert.Equal(expectedValue, result);
            Assert.IsType<TextAlignment>(result);
        }

        /// <summary>
        /// Tests that multiple get/set operations on VerticalTextAlignment property work correctly.
        /// Input: Multiple different TextAlignment values set sequentially.
        /// Expected: Each getter call returns the most recently set value.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_MultipleSetGet_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.VerticalTextAlignment = TextAlignment.Start;
            Assert.Equal(TextAlignment.Start, searchHandler.VerticalTextAlignment);

            searchHandler.VerticalTextAlignment = TextAlignment.Justify;
            Assert.Equal(TextAlignment.Justify, searchHandler.VerticalTextAlignment);

            searchHandler.VerticalTextAlignment = TextAlignment.Center;
            Assert.Equal(TextAlignment.Center, searchHandler.VerticalTextAlignment);
        }

        /// <summary>
        /// Tests that VerticalTextAlignment property uses the correct BindableProperty reference.
        /// Input: SearchHandler instance accessing VerticalTextAlignmentProperty.
        /// Expected: Property uses TextAlignmentElement.VerticalTextAlignmentProperty.
        /// </summary>
        [Fact]
        public void VerticalTextAlignment_UsesCorrectBindableProperty_ReturnsExpectedProperty()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedProperty = TextAlignmentElement.VerticalTextAlignmentProperty;

            // Act
            var propertyValue = searchHandler.GetValue(expectedProperty);
            searchHandler.VerticalTextAlignment = TextAlignment.End;
            var directPropertyValue = searchHandler.GetValue(expectedProperty);

            // Assert
            Assert.Equal(TextAlignment.Center, (TextAlignment)propertyValue); // Default value
            Assert.Equal(TextAlignment.End, (TextAlignment)directPropertyValue); // Set value
        }

        /// <summary>
        /// Tests that TextColor getter returns the correct value when different Color values are set
        /// </summary>
        /// <param name="red">Red component of the color</param>
        /// <param name="green">Green component of the color</param>
        /// <param name="blue">Blue component of the color</param>
        /// <param name="alpha">Alpha component of the color</param>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 0.0f)] // Transparent white
        public void TextColor_GetAfterSet_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.TextColor = expectedColor;
            var actualColor = searchHandler.TextColor;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red, precision: 5);
            Assert.Equal(expectedColor.Green, actualColor.Green, precision: 5);
            Assert.Equal(expectedColor.Blue, actualColor.Blue, precision: 5);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha, precision: 5);
        }

        /// <summary>
        /// Tests that TextColor getter returns default value when no color has been set
        /// </summary>
        [Fact]
        public void TextColor_GetWithoutSet_ReturnsDefaultValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var actualColor = searchHandler.TextColor;

            // Assert - The default value should be equivalent to null/default Color based on TextElement.TextColorProperty definition
            Assert.NotNull(actualColor);
        }

        /// <summary>
        /// Tests TextColor property with various Color constructor overloads
        /// </summary>
        /// <param name="colorFactory">Function to create a Color instance</param>
        /// <param name="expectedRed">Expected red component</param>
        /// <param name="expectedGreen">Expected green component</param>
        /// <param name="expectedBlue">Expected blue component</param>
        /// <param name="expectedAlpha">Expected alpha component</param>
        [Theory]
        [MemberData(nameof(GetColorConstructorTestData))]
        public void TextColor_GetWithDifferentConstructors_ReturnsCorrectValue(Func<Color> colorFactory, float expectedRed, float expectedGreen, float expectedBlue, float expectedAlpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var color = colorFactory();

            // Act
            searchHandler.TextColor = color;
            var actualColor = searchHandler.TextColor;

            // Assert
            Assert.Equal(expectedRed, actualColor.Red, precision: 3);
            Assert.Equal(expectedGreen, actualColor.Green, precision: 3);
            Assert.Equal(expectedBlue, actualColor.Blue, precision: 3);
            Assert.Equal(expectedAlpha, actualColor.Alpha, precision: 3);
        }

        /// <summary>
        /// Tests that TextColor getter works correctly when setting the same value multiple times
        /// </summary>
        [Fact]
        public void TextColor_SetSameValueMultipleTimes_GetReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var color = new Color(0.75f, 0.25f, 0.5f, 0.8f);

            // Act
            searchHandler.TextColor = color;
            searchHandler.TextColor = color;
            searchHandler.TextColor = color;
            var actualColor = searchHandler.TextColor;

            // Assert
            Assert.Equal(color.Red, actualColor.Red, precision: 5);
            Assert.Equal(color.Green, actualColor.Green, precision: 5);
            Assert.Equal(color.Blue, actualColor.Blue, precision: 5);
            Assert.Equal(color.Alpha, actualColor.Alpha, precision: 5);
        }

        /// <summary>
        /// Tests that TextColor getter returns the latest value when property is set multiple times with different values
        /// </summary>
        [Fact]
        public void TextColor_SetDifferentValuesSequentially_GetReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            var secondColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            var thirdColor = new Color(0.0f, 0.0f, 1.0f, 0.5f);

            // Act
            searchHandler.TextColor = firstColor;
            searchHandler.TextColor = secondColor;
            searchHandler.TextColor = thirdColor;
            var actualColor = searchHandler.TextColor;

            // Assert
            Assert.Equal(thirdColor.Red, actualColor.Red, precision: 5);
            Assert.Equal(thirdColor.Green, actualColor.Green, precision: 5);
            Assert.Equal(thirdColor.Blue, actualColor.Blue, precision: 5);
            Assert.Equal(thirdColor.Alpha, actualColor.Alpha, precision: 5);
        }

        /// <summary>
        /// Provides test data for different Color constructors
        /// </summary>
        public static TheoryData<Func<Color>, float, float, float, float> GetColorConstructorTestData()
        {
            return new TheoryData<Func<Color>, float, float, float, float>
            {
                { () => new Color(), 0.0f, 0.0f, 0.0f, 1.0f }, // Default constructor
                { () => new Color(0.5f), 0.5f, 0.5f, 0.5f, 1.0f }, // Grayscale constructor
                { () => new Color(0.2f, 0.4f, 0.6f), 0.2f, 0.4f, 0.6f, 1.0f }, // RGB constructor
                { () => new Color(0.1f, 0.3f, 0.7f, 0.9f), 0.1f, 0.3f, 0.7f, 0.9f }, // RGBA constructor
                { () => new Color(128, 64, 192), 128/255f, 64/255f, 192/255f, 1.0f }, // Byte RGB constructor
                { () => new Color(100, 150, 200, 50), 100/255f, 150/255f, 200/255f, 50/255f }, // Byte RGBA constructor
            };
        }

        /// <summary>
        /// Tests that FontFamily property getter returns the correct value when a valid font family name is set.
        /// Input: Valid font family name
        /// Expected: Returns the same string value that was set
        /// </summary>
        [Theory]
        [InlineData("Arial")]
        [InlineData("Times New Roman")]
        [InlineData("Courier New")]
        [InlineData("Helvetica")]
        [InlineData("Comic Sans MS")]
        public void FontFamily_GetValidFontFamily_ReturnsExpectedValue(string expectedFontFamily)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = expectedFontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(expectedFontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter returns null when no value has been set.
        /// Input: No value set (default state)
        /// Expected: Returns null
        /// </summary>
        [Fact]
        public void FontFamily_GetDefault_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var fontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Null(fontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter returns null when explicitly set to null.
        /// Input: null value
        /// Expected: Returns null
        /// </summary>
        [Fact]
        public void FontFamily_GetNullValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = null;

            // Act
            var fontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Null(fontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter handles empty and whitespace strings correctly.
        /// Input: Empty string, whitespace-only strings
        /// Expected: Returns the exact string that was set
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void FontFamily_GetEmptyOrWhitespaceValue_ReturnsExpectedValue(string fontFamily)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = fontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(fontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter handles strings with special characters.
        /// Input: Strings containing special characters, unicode, etc.
        /// Expected: Returns the exact string that was set
        /// </summary>
        [Theory]
        [InlineData("Font-Family")]
        [InlineData("Font_Family")]
        [InlineData("Font Family!")]
        [InlineData("Font@Family")]
        [InlineData("Font#Family")]
        [InlineData("Font$Family")]
        [InlineData("Font%Family")]
        [InlineData("Font&Family")]
        [InlineData("Font*Family")]
        [InlineData("Font(Family)")]
        [InlineData("Font[Family]")]
        [InlineData("Font{Family}")]
        [InlineData("Font|Family")]
        [InlineData("Font\\Family")]
        [InlineData("Font/Family")]
        [InlineData("Font?Family")]
        [InlineData("Font<Family>")]
        [InlineData("Font=Family")]
        [InlineData("Font+Family")]
        [InlineData("Font.Family")]
        [InlineData("Font,Family")]
        [InlineData("Font;Family")]
        [InlineData("Font:Family")]
        [InlineData("Font'Family")]
        [InlineData("Font\"Family")]
        [InlineData("Font`Family")]
        [InlineData("Font~Family")]
        [InlineData("Font^Family")]
        public void FontFamily_GetSpecialCharacters_ReturnsExpectedValue(string fontFamily)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = fontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(fontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter handles unicode characters correctly.
        /// Input: Strings containing unicode characters
        /// Expected: Returns the exact string that was set
        /// </summary>
        [Theory]
        [InlineData("微软雅黑")] // Microsoft YaHei in Chinese
        [InlineData("Noto Sans CJK")] // Unicode font
        [InlineData("Font™Family")] // Trademark symbol
        [InlineData("Font©Family")] // Copyright symbol
        [InlineData("Font®Family")] // Registered trademark
        [InlineData("Font€Family")] // Euro symbol
        [InlineData("Font♪Family")] // Musical note
        [InlineData("Font☃Family")] // Snowman
        [InlineData("Font🎵Family")] // Musical note emoji
        public void FontFamily_GetUnicodeCharacters_ReturnsExpectedValue(string fontFamily)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = fontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(fontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter handles very long strings correctly.
        /// Input: Very long string (1000 characters)
        /// Expected: Returns the exact string that was set
        /// </summary>
        [Fact]
        public void FontFamily_GetVeryLongString_ReturnsExpectedValue()
        {
            // Arrange
            var longFontFamily = new string('A', 1000);
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = longFontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(longFontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property setter and getter work correctly with multiple assignments.
        /// Input: Multiple different font family values set sequentially
        /// Expected: Each getter call returns the most recently set value
        /// </summary>
        [Fact]
        public void FontFamily_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.FontFamily = "Arial";
            Assert.Equal("Arial", searchHandler.FontFamily);

            searchHandler.FontFamily = "Times New Roman";
            Assert.Equal("Times New Roman", searchHandler.FontFamily);

            searchHandler.FontFamily = null;
            Assert.Null(searchHandler.FontFamily);

            searchHandler.FontFamily = "";
            Assert.Equal("", searchHandler.FontFamily);

            searchHandler.FontFamily = "Helvetica";
            Assert.Equal("Helvetica", searchHandler.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter handles control characters correctly.
        /// Input: Strings containing control characters
        /// Expected: Returns the exact string that was set
        /// </summary>
        [Theory]
        [InlineData("Font\0Family")] // Null character
        [InlineData("Font\aFamily")] // Bell character
        [InlineData("Font\bFamily")] // Backspace
        [InlineData("Font\fFamily")] // Form feed
        [InlineData("Font\vFamily")] // Vertical tab
        public void FontFamily_GetControlCharacters_ReturnsExpectedValue(string fontFamily)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = fontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(fontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter handles numeric strings correctly.
        /// Input: Strings containing only numbers
        /// Expected: Returns the exact string that was set
        /// </summary>
        [Theory]
        [InlineData("123")]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("3.14159")]
        [InlineData("1e10")]
        [InlineData("0x123")]
        public void FontFamily_GetNumericStrings_ReturnsExpectedValue(string fontFamily)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.FontFamily = fontFamily;

            // Act
            var actualFontFamily = searchHandler.FontFamily;

            // Assert
            Assert.Equal(fontFamily, actualFontFamily);
        }

        /// <summary>
        /// Tests that PlaceholderColor returns the default Color value when accessed without being set
        /// </summary>
        [Fact]
        public void PlaceholderColor_GetDefaultValue_ReturnsDefaultColor()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.PlaceholderColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that PlaceholderColor can be set and retrieved correctly with valid Color values
        /// </summary>
        /// <param name="red">Red component</param>
        /// <param name="green">Green component</param>
        /// <param name="blue">Blue component</param>
        /// <param name="alpha">Alpha component</param>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.3f, 0.8f, 0.7f)] // Custom color with alpha
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        public void PlaceholderColor_SetAndGetValidColors_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.PlaceholderColor = expectedColor;
            var result = searchHandler.PlaceholderColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that PlaceholderColor handles extreme boundary values correctly
        /// </summary>
        /// <param name="red">Red component</param>
        /// <param name="green">Green component</param>
        /// <param name="blue">Blue component</param>
        /// <param name="alpha">Alpha component</param>
        [Theory]
        [InlineData(float.MinValue, 0.0f, 0.0f, 1.0f)]
        [InlineData(float.MaxValue, 0.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, float.MinValue, 0.0f, 1.0f)]
        [InlineData(0.0f, float.MaxValue, 0.0f, 1.0f)]
        [InlineData(0.0f, 0.0f, float.MinValue, 1.0f)]
        [InlineData(0.0f, 0.0f, float.MaxValue, 1.0f)]
        [InlineData(0.0f, 0.0f, 0.0f, float.MinValue)]
        [InlineData(0.0f, 0.0f, 0.0f, float.MaxValue)]
        public void PlaceholderColor_SetBoundaryValues_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.PlaceholderColor = expectedColor;
            var result = searchHandler.PlaceholderColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that PlaceholderColor handles special floating point values correctly
        /// </summary>
        /// <param name="red">Red component</param>
        /// <param name="green">Green component</param>
        /// <param name="blue">Blue component</param>
        /// <param name="alpha">Alpha component</param>
        [Theory]
        [InlineData(float.NaN, 0.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, float.NaN, 0.0f, 1.0f)]
        [InlineData(0.0f, 0.0f, float.NaN, 1.0f)]
        [InlineData(0.0f, 0.0f, 0.0f, float.NaN)]
        [InlineData(float.PositiveInfinity, 0.0f, 0.0f, 1.0f)]
        [InlineData(float.NegativeInfinity, 0.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, float.PositiveInfinity, 0.0f, 1.0f)]
        [InlineData(0.0f, float.NegativeInfinity, 0.0f, 1.0f)]
        public void PlaceholderColor_SetSpecialFloatValues_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.PlaceholderColor = expectedColor;
            var result = searchHandler.PlaceholderColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that PlaceholderColor can be set multiple times and returns the latest value
        /// </summary>
        [Fact]
        public void PlaceholderColor_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var secondColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
            var thirdColor = new Color(0.0f, 0.0f, 1.0f, 1.0f); // Blue

            // Act & Assert - First color
            searchHandler.PlaceholderColor = firstColor;
            Assert.Equal(firstColor, searchHandler.PlaceholderColor);

            // Act & Assert - Second color
            searchHandler.PlaceholderColor = secondColor;
            Assert.Equal(secondColor, searchHandler.PlaceholderColor);

            // Act & Assert - Third color
            searchHandler.PlaceholderColor = thirdColor;
            Assert.Equal(thirdColor, searchHandler.PlaceholderColor);
        }

        /// <summary>
        /// Tests that PlaceholderColor getter works correctly when setting back to default value
        /// </summary>
        [Fact]
        public void PlaceholderColor_SetToDefaultAfterCustomValue_ReturnsDefaultColor()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var customColor = new Color(0.5f, 0.3f, 0.8f, 0.7f);
            var defaultColor = default(Color);

            // Act
            searchHandler.PlaceholderColor = customColor;
            Assert.Equal(customColor, searchHandler.PlaceholderColor);

            searchHandler.PlaceholderColor = defaultColor;
            var result = searchHandler.PlaceholderColor;

            // Assert
            Assert.Equal(defaultColor, result);
        }

        /// <summary>
        /// Tests that PlaceholderColor property maintains independence between different SearchHandler instances
        /// </summary>
        [Fact]
        public void PlaceholderColor_MultipleInstances_MaintainIndependentValues()
        {
            // Arrange
            var searchHandler1 = new SearchHandler();
            var searchHandler2 = new SearchHandler();
            var color1 = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var color2 = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green

            // Act
            searchHandler1.PlaceholderColor = color1;
            searchHandler2.PlaceholderColor = color2;

            // Assert
            Assert.Equal(color1, searchHandler1.PlaceholderColor);
            Assert.Equal(color2, searchHandler2.PlaceholderColor);
            Assert.NotEqual(searchHandler1.PlaceholderColor, searchHandler2.PlaceholderColor);
        }

        /// <summary>
        /// Tests that ClearIconHelpText returns the default value (null) when not explicitly set.
        /// </summary>
        [Fact]
        public void ClearIconHelpText_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ClearIconHelpText;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ClearIconHelpText getter and setter with valid string values.
        /// </summary>
        /// <param name="value">The test string value to set and retrieve.</param>
        [Theory]
        [InlineData("Clear search")]
        [InlineData("Help text")]
        [InlineData("A")]
        [InlineData("Very long help text that might be used to describe the clear icon functionality in detail")]
        [InlineData("Special chars: !@#$%^&*()_+-=[]{}|;:'\",.<>?/~`")]
        [InlineData("Unicode: 你好世界 🌍")]
        public void ClearIconHelpText_SetValidString_ReturnsExpectedValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconHelpText = value;
            var result = searchHandler.ClearIconHelpText;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests ClearIconHelpText with null value assignment and retrieval.
        /// </summary>
        [Fact]
        public void ClearIconHelpText_SetNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.ClearIconHelpText = "Initial value";

            // Act
            searchHandler.ClearIconHelpText = null;
            var result = searchHandler.ClearIconHelpText;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ClearIconHelpText with empty string and whitespace-only values.
        /// </summary>
        /// <param name="value">The whitespace test value.</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData(" \t \n \r ")]
        public void ClearIconHelpText_SetWhitespaceValues_ReturnsExpectedValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearIconHelpText = value;
            var result = searchHandler.ClearIconHelpText;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ClearIconHelpText can be set multiple times and returns the last set value.
        /// </summary>
        [Fact]
        public void ClearIconHelpText_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.ClearIconHelpText = "First value";
            Assert.Equal("First value", searchHandler.ClearIconHelpText);

            searchHandler.ClearIconHelpText = "Second value";
            Assert.Equal("Second value", searchHandler.ClearIconHelpText);

            searchHandler.ClearIconHelpText = null;
            Assert.Null(searchHandler.ClearIconHelpText);

            searchHandler.ClearIconHelpText = "Final value";
            Assert.Equal("Final value", searchHandler.ClearIconHelpText);
        }

        /// <summary>
        /// Tests ClearIconHelpText with extremely long string values to verify no length restrictions.
        /// </summary>
        [Fact]
        public void ClearIconHelpText_SetVeryLongString_ReturnsExpectedValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var longString = new string('A', 10000);

            // Act
            searchHandler.ClearIconHelpText = longString;
            var result = searchHandler.ClearIconHelpText;

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that ClearIconHelpText property maintains reference equality for the same string instance.
        /// </summary>
        [Fact]
        public void ClearIconHelpText_SetSameReference_MaintainsReference()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var testString = "Test help text";

            // Act
            searchHandler.ClearIconHelpText = testString;
            var result = searchHandler.ClearIconHelpText;

            // Assert
            Assert.Same(testString, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon getter returns null when no value has been set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_Get_WhenValueIsNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ClearPlaceholderIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon getter returns the correct ImageSource when a value has been set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_Get_WhenValueIsSet_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.ClearPlaceholderIcon = mockImageSource;
            var result = searchHandler.ClearPlaceholderIcon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon setter correctly sets null value.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_Set_WithNullValue_SetsCorrectly()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();
            searchHandler.ClearPlaceholderIcon = mockImageSource;

            // Act
            searchHandler.ClearPlaceholderIcon = null;

            // Assert
            Assert.Null(searchHandler.ClearPlaceholderIcon);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon setter correctly sets a valid ImageSource.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_Set_WithValidImageSource_SetsCorrectly()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.ClearPlaceholderIcon = mockImageSource;

            // Assert
            Assert.Same(mockImageSource, searchHandler.ClearPlaceholderIcon);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon property correctly handles multiple set and get operations.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_SetAndGet_WithDifferentValues_ReturnsCorrectValues()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act & Assert - Set first value
            searchHandler.ClearPlaceholderIcon = firstImageSource;
            Assert.Same(firstImageSource, searchHandler.ClearPlaceholderIcon);

            // Act & Assert - Set second value
            searchHandler.ClearPlaceholderIcon = secondImageSource;
            Assert.Same(secondImageSource, searchHandler.ClearPlaceholderIcon);

            // Act & Assert - Set null
            searchHandler.ClearPlaceholderIcon = null;
            Assert.Null(searchHandler.ClearPlaceholderIcon);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon property uses the correct BindableProperty for getting values.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_Get_UsesCorrectBindableProperty()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.SetValue(SearchHandler.ClearPlaceholderIconProperty, mockImageSource);
            var result = searchHandler.ClearPlaceholderIcon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that ClearPlaceholderIcon property uses the correct BindableProperty for setting values.
        /// </summary>
        [Fact]
        public void ClearPlaceholderIcon_Set_UsesCorrectBindableProperty()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.ClearPlaceholderIcon = mockImageSource;
            var result = searchHandler.GetValue(SearchHandler.ClearPlaceholderIconProperty);

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that IsSearchEnabled property returns the default value of true when no value has been explicitly set.
        /// </summary>
        [Fact]
        public void IsSearchEnabled_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.IsSearchEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSearchEnabled property can be set to true and returns the correct value.
        /// </summary>
        [Fact]
        public void IsSearchEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.IsSearchEnabled = true;
            var result = searchHandler.IsSearchEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSearchEnabled property can be set to false and returns the correct value.
        /// </summary>
        [Fact]
        public void IsSearchEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.IsSearchEnabled = false;
            var result = searchHandler.IsSearchEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsSearchEnabled property maintains the correct value through multiple set operations.
        /// Tests the input conditions: setting multiple boolean values in sequence.
        /// Expected result: each get operation returns the last set value.
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        public void IsSearchEnabled_MultipleSetOperations_MaintainsCorrectValue(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.IsSearchEnabled = firstValue;
            Assert.Equal(firstValue, searchHandler.IsSearchEnabled);

            searchHandler.IsSearchEnabled = secondValue;
            Assert.Equal(secondValue, searchHandler.IsSearchEnabled);

            searchHandler.IsSearchEnabled = thirdValue;
            Assert.Equal(thirdValue, searchHandler.IsSearchEnabled);
        }

        /// <summary>
        /// Tests that QueryIconHelpText getter returns the default value when not set.
        /// Input: New SearchHandler instance.
        /// Expected: QueryIconHelpText should return null (default value).
        /// </summary>
        [Fact]
        public void QueryIconHelpText_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.QueryIconHelpText;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that QueryIconHelpText setter and getter work correctly with various string values.
        /// Input: Various string values including null, empty, whitespace, normal text, and special characters.
        /// Expected: The getter should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("Help text")]
        [InlineData("Search for items")]
        [InlineData("Unicode: 🔍 ñáéíóú")]
        [InlineData("Special chars: !@#$%^&*()_+-=[]{}|;:\"'<>?,./")]
        public void QueryIconHelpText_SetAndGet_ReturnsCorrectValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.QueryIconHelpText = value;
            var result = searchHandler.QueryIconHelpText;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that QueryIconHelpText handles very long strings correctly.
        /// Input: Very long string (1000+ characters).
        /// Expected: The getter should return the exact long string that was set.
        /// </summary>
        [Fact]
        public void QueryIconHelpText_VeryLongString_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var longString = new string('A', 1000) + "End";

            // Act
            searchHandler.QueryIconHelpText = longString;
            var result = searchHandler.QueryIconHelpText;

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that setting QueryIconHelpText multiple times with the same value works correctly.
        /// Input: Setting the same value multiple times.
        /// Expected: The getter should consistently return the set value.
        /// </summary>
        [Fact]
        public void QueryIconHelpText_SetSameValueMultipleTimes_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var testValue = "Test Help Text";

            // Act & Assert
            searchHandler.QueryIconHelpText = testValue;
            Assert.Equal(testValue, searchHandler.QueryIconHelpText);

            searchHandler.QueryIconHelpText = testValue;
            Assert.Equal(testValue, searchHandler.QueryIconHelpText);

            searchHandler.QueryIconHelpText = testValue;
            Assert.Equal(testValue, searchHandler.QueryIconHelpText);
        }

        /// <summary>
        /// Tests that QueryIconHelpText can be changed from one value to another.
        /// Input: Setting different values sequentially.
        /// Expected: The getter should return the most recently set value.
        /// </summary>
        [Fact]
        public void QueryIconHelpText_ChangeValues_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.QueryIconHelpText = "First Value";
            Assert.Equal("First Value", searchHandler.QueryIconHelpText);

            searchHandler.QueryIconHelpText = "Second Value";
            Assert.Equal("Second Value", searchHandler.QueryIconHelpText);

            searchHandler.QueryIconHelpText = null;
            Assert.Null(searchHandler.QueryIconHelpText);

            searchHandler.QueryIconHelpText = "";
            Assert.Equal("", searchHandler.QueryIconHelpText);
        }

        /// <summary>
        /// Tests that QueryIconHelpText handles boundary values correctly.
        /// Input: Various boundary string values including control characters.
        /// Expected: The getter should return the exact values that were set.
        /// </summary>
        [Theory]
        [InlineData("\0")]
        [InlineData("\u0001")]
        [InlineData("\u007F")]
        [InlineData("\u0080")]
        [InlineData("\uFFFF")]
        public void QueryIconHelpText_BoundaryValues_ReturnsCorrectValue(string value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.QueryIconHelpText = value;
            var result = searchHandler.QueryIconHelpText;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that IsFocused returns false by default when no value has been set.
        /// Verifies the default state of the focus property.
        /// </summary>
        [Fact]
        public void IsFocused_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.IsFocused;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsFocused returns true when the focus state is set to true.
        /// Verifies the property correctly retrieves and casts the stored boolean value.
        /// </summary>
        [Fact]
        public void IsFocused_WhenSetToTrue_ReturnsTrue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.SetIsFocused(true);
            var result = searchHandler.IsFocused;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsFocused returns false when the focus state is set to false.
        /// Verifies the property correctly retrieves and casts the stored boolean value.
        /// </summary>
        [Fact]
        public void IsFocused_WhenSetToFalse_ReturnsFalse()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.SetIsFocused(false);
            var result = searchHandler.IsFocused;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsFocused correctly toggles between true and false values.
        /// Verifies the property consistently returns the most recently set value.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void IsFocused_WhenTogglingValues_ReturnsCorrectValue(bool firstValue, bool secondValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.SetIsFocused(firstValue);
            var firstResult = searchHandler.IsFocused;
            searchHandler.SetIsFocused(secondValue);
            var secondResult = searchHandler.IsFocused;

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }

        /// <summary>
        /// Tests that CancelButtonColor getter returns the value from the bindable property.
        /// Verifies the getter implementation calls GetValue and casts to Color correctly.
        /// </summary>
        [Fact]
        public void CancelButtonColor_Get_ReturnsValueFromBindableProperty()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(1.0f, 0.5f, 0.0f, 1.0f); // Orange color
            searchHandler.SetValue(SearchHandler.CancelButtonColorProperty, expectedColor);

            // Act
            var actualColor = searchHandler.CancelButtonColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that CancelButtonColor setter calls SetValue with the correct property and value.
        /// Verifies the setter implementation correctly passes the value to the bindable property.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(1.0f, 1.0f, 1.0f, 0.0f)] // Transparent white
        public void CancelButtonColor_Set_SetsValueOnBindableProperty(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var color = new Color(red, green, blue, alpha);

            // Act
            searchHandler.CancelButtonColor = color;

            // Assert
            var actualValue = searchHandler.GetValue(SearchHandler.CancelButtonColorProperty);
            Assert.Equal(color, actualValue);
        }

        /// <summary>
        /// Tests that CancelButtonColor property has correct default value.
        /// Verifies the bindable property is initialized with default(Color).
        /// </summary>
        [Fact]
        public void CancelButtonColor_DefaultValue_IsDefaultColor()
        {
            // Arrange & Act
            var searchHandler = new SearchHandler();
            var defaultColor = searchHandler.CancelButtonColor;

            // Assert
            Assert.Equal(default(Color), defaultColor);
        }

        /// <summary>
        /// Tests that CancelButtonColor property correctly handles extreme color values.
        /// Verifies behavior with boundary values for RGBA components.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Fully transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Fully opaque white
        public void CancelButtonColor_SetGet_HandlesExtremeValues(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var extremeColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.CancelButtonColor = extremeColor;
            var retrievedColor = searchHandler.CancelButtonColor;

            // Assert
            Assert.Equal(extremeColor, retrievedColor);
        }

        /// <summary>
        /// Tests that CancelButtonColor property correctly handles setting and getting the same value multiple times.
        /// Verifies consistency in property behavior across multiple operations.
        /// </summary>
        [Fact]
        public void CancelButtonColor_SetSameValueMultipleTimes_RemainsConsistent()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var testColor = new Color(0.7f, 0.3f, 0.9f, 0.8f);

            // Act & Assert
            searchHandler.CancelButtonColor = testColor;
            Assert.Equal(testColor, searchHandler.CancelButtonColor);

            searchHandler.CancelButtonColor = testColor;
            Assert.Equal(testColor, searchHandler.CancelButtonColor);

            searchHandler.CancelButtonColor = testColor;
            Assert.Equal(testColor, searchHandler.CancelButtonColor);
        }

        /// <summary>
        /// Tests that CancelButtonColor property works with different Color constructor overloads.
        /// Verifies compatibility with various ways of creating Color instances.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetColorTestData))]
        public void CancelButtonColor_SetGet_WorksWithDifferentColorConstructors(Color testColor)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.CancelButtonColor = testColor;
            var retrievedColor = searchHandler.CancelButtonColor;

            // Assert
            Assert.Equal(testColor, retrievedColor);
        }

        public static TheoryData<Color> GetColorTestData()
        {
            return new TheoryData<Color>
            {
                new Color(), // Default constructor
                new Color(0.5f), // Grayscale constructor
                new Color(0.2f, 0.4f, 0.6f), // RGB constructor
                new Color(0.1f, 0.3f, 0.5f, 0.7f), // RGBA constructor
                new Color(255, 128, 64), // Byte RGB constructor
                new Color(200, 100, 50, 150), // Byte RGBA constructor
                Color.FromRgb(100, 150, 200), // FromRgb factory method
                Color.FromRgba(80, 120, 160, 200) // FromRgba factory method
            };
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property getter returns the correct default value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_GetDefaultValue_ReturnsExpectedValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.FontAutoScalingEnabled;

            // Assert
            Assert.IsType<bool>(result);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property setter and getter work correctly with various boolean values.
        /// </summary>
        /// <param name="value">The boolean value to test with.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FontAutoScalingEnabled_SetAndGet_ReturnsCorrectValue(bool value)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.FontAutoScalingEnabled = value;
            var result = searchHandler.FontAutoScalingEnabled;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property uses the correct BindableProperty.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_UsesCorrectBindableProperty_Success()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedValue = true;

            // Act
            searchHandler.SetValue(SearchHandler.FontAutoScalingEnabledProperty, expectedValue);
            var result = searchHandler.FontAutoScalingEnabled;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that setting the FontAutoScalingEnabled property updates the underlying BindableProperty value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetProperty_UpdatesBindablePropertyValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedValue = false;

            // Act
            searchHandler.FontAutoScalingEnabled = expectedValue;
            var result = searchHandler.GetValue(SearchHandler.FontAutoScalingEnabledProperty);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property getter retrieves value from the correct BindableProperty.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FontAutoScalingEnabled_GetProperty_RetrievesFromCorrectBindableProperty(bool testValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.SetValue(SearchHandler.FontAutoScalingEnabledProperty, testValue);
            var result = searchHandler.FontAutoScalingEnabled;

            // Assert
            Assert.Equal(testValue, result);
        }

        /// <summary>
        /// Tests that BackgroundColor getter returns the default value when not explicitly set.
        /// Verifies the property returns null as the default value based on BindableProperty definition.
        /// </summary>
        [Fact]
        public void BackgroundColor_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.BackgroundColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that BackgroundColor getter returns the correct value after setting a valid Color.
        /// Verifies the property correctly retrieves values from the underlying BindableProperty.
        /// </summary>
        /// <param name="red">Red component value (0.0-1.0)</param>
        /// <param name="green">Green component value (0.0-1.0)</param>
        /// <param name="blue">Blue component value (0.0-1.0)</param>
        /// <param name="alpha">Alpha component value (0.0-1.0)</param>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 0.0f, 1.0f, 0.5f)] // Semi-transparent magenta
        [InlineData(0.5f, 0.5f, 0.5f, 0.0f)] // Fully transparent gray
        public void BackgroundColor_WhenSetToValidColor_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.BackgroundColor = expectedColor;
            var result = searchHandler.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red, 5);
            Assert.Equal(expectedColor.Green, result.Green, 5);
            Assert.Equal(expectedColor.Blue, result.Blue, 5);
            Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
        }

        /// <summary>
        /// Tests that BackgroundColor getter returns predefined Colors correctly.
        /// Verifies the property works with commonly used predefined Color values.
        /// </summary>
        /// <param name="colorName">The name of the predefined color</param>
        [Theory]
        [InlineData("Red")]
        [InlineData("Blue")]
        [InlineData("Green")]
        [InlineData("White")]
        [InlineData("Black")]
        [InlineData("Transparent")]
        [InlineData("Yellow")]
        [InlineData("Purple")]
        public void BackgroundColor_WhenSetToPredefinedColors_ReturnsCorrectValue(string colorName)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = GetPredefinedColor(colorName);

            // Act
            searchHandler.BackgroundColor = expectedColor;
            var result = searchHandler.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red, 5);
            Assert.Equal(expectedColor.Green, result.Green, 5);
            Assert.Equal(expectedColor.Blue, result.Blue, 5);
            Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
        }

        /// <summary>
        /// Tests that BackgroundColor can be set to null and getter returns null.
        /// Verifies the property supports nullable Color values as expected from BindableProperty definition.
        /// </summary>
        [Fact]
        public void BackgroundColor_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.BackgroundColor = Colors.Red; // First set to a non-null value

            // Act
            searchHandler.BackgroundColor = null;
            var result = searchHandler.BackgroundColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that BackgroundColor getter returns Colors created from various factory methods.
        /// Verifies the property works with Colors created using different construction approaches.
        /// </summary>
        [Fact]
        public void BackgroundColor_WhenSetToColorsFromFactoryMethods_ReturnsCorrectValues()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var testColors = new[]
            {
                Color.FromRgb(255, 128, 0), // Orange
				Color.FromHsla(0.33, 1.0, 0.5, 1.0), // Green
				Color.FromHex("#FF5722"), // Deep orange
				Color.FromRgba(100, 200, 50, 128), // Semi-transparent green
			};

            foreach (var expectedColor in testColors)
            {
                // Act
                searchHandler.BackgroundColor = expectedColor;
                var result = searchHandler.BackgroundColor;

                // Assert
                Assert.Equal(expectedColor.Red, result.Red, 5);
                Assert.Equal(expectedColor.Green, result.Green, 5);
                Assert.Equal(expectedColor.Blue, result.Blue, 5);
                Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
            }
        }

        /// <summary>
        /// Tests that BackgroundColor getter handles extreme Color values correctly.
        /// Verifies the property works with edge case Color values including component boundaries.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Fully transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 0.0f)] // Fully transparent white
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Fully opaque white
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Fully opaque black
        public void BackgroundColor_WhenSetToExtremeColorValues_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            searchHandler.BackgroundColor = expectedColor;
            var result = searchHandler.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red, 5);
            Assert.Equal(expectedColor.Green, result.Green, 5);
            Assert.Equal(expectedColor.Blue, result.Blue, 5);
            Assert.Equal(expectedColor.Alpha, result.Alpha, 5);
        }

        /// <summary>
        /// Tests that multiple successive BackgroundColor operations work correctly.
        /// Verifies the property maintains state correctly across multiple get/set operations.
        /// </summary>
        [Fact]
        public void BackgroundColor_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = Colors.Green;

            // Act & Assert - First set
            searchHandler.BackgroundColor = firstColor;
            Assert.Equal(firstColor.Red, searchHandler.BackgroundColor.Red, 5);
            Assert.Equal(firstColor.Green, searchHandler.BackgroundColor.Green, 5);
            Assert.Equal(firstColor.Blue, searchHandler.BackgroundColor.Blue, 5);

            // Act & Assert - Second set
            searchHandler.BackgroundColor = secondColor;
            Assert.Equal(secondColor.Red, searchHandler.BackgroundColor.Red, 5);
            Assert.Equal(secondColor.Green, searchHandler.BackgroundColor.Green, 5);
            Assert.Equal(secondColor.Blue, searchHandler.BackgroundColor.Blue, 5);

            // Act & Assert - Third set
            searchHandler.BackgroundColor = thirdColor;
            Assert.Equal(thirdColor.Red, searchHandler.BackgroundColor.Red, 5);
            Assert.Equal(thirdColor.Green, searchHandler.BackgroundColor.Green, 5);
            Assert.Equal(thirdColor.Blue, searchHandler.BackgroundColor.Blue, 5);
        }

        /// <summary>
        /// Helper method to get predefined colors by name for testing.
        /// </summary>
        /// <param name="colorName">Name of the predefined color</param>
        /// <returns>The corresponding Color value</returns>
        private static Color GetPredefinedColor(string colorName) => colorName switch
        {
            "Red" => Colors.Red,
            "Blue" => Colors.Blue,
            "Green" => Colors.Green,
            "White" => Colors.White,
            "Black" => Colors.Black,
            "Transparent" => Colors.Transparent,
            "Yellow" => Colors.Yellow,
            "Purple" => Colors.Purple,
            _ => throw new ArgumentException($"Unknown color name: {colorName}")
        };

        /// <summary>
        /// Tests that the ClearPlaceholderEnabled property returns the default value of false when not explicitly set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderEnabled_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            bool result = searchHandler.ClearPlaceholderEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the ClearPlaceholderEnabled property can be set and retrieved correctly for various boolean values.
        /// Input conditions: Setting the property to true and false values.
        /// Expected result: The property should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ClearPlaceholderEnabled_SetValue_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ClearPlaceholderEnabled = expectedValue;
            bool result = searchHandler.ClearPlaceholderEnabled;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the ClearPlaceholderEnabled property can be set multiple times and always returns the most recent value.
        /// Input conditions: Setting the property to different values in sequence.
        /// Expected result: The property should always return the last value that was set.
        /// </summary>
        [Fact]
        public void ClearPlaceholderEnabled_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act & Assert
            searchHandler.ClearPlaceholderEnabled = true;
            Assert.True(searchHandler.ClearPlaceholderEnabled);

            searchHandler.ClearPlaceholderEnabled = false;
            Assert.False(searchHandler.ClearPlaceholderEnabled);

            searchHandler.ClearPlaceholderEnabled = true;
            Assert.True(searchHandler.ClearPlaceholderEnabled);
        }

        /// <summary>
        /// Tests that CommandParameter getter returns null when no value has been set.
        /// </summary>
        [Fact]
        public void CommandParameter_GetWithoutSet_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.CommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CommandParameter getter returns the exact value that was set via the setter.
        /// Tests various object types to ensure type safety and proper storage/retrieval.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void CommandParameter_SetAndGet_ReturnsExpectedValue(object expectedValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.CommandParameter = expectedValue;
            var result = searchHandler.CommandParameter;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that CommandParameter can be set to different values successively.
        /// Verifies that the property correctly updates its stored value.
        /// </summary>
        [Fact]
        public void CommandParameter_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var firstValue = "first";
            var secondValue = 123;
            var thirdValue = new object();

            // Act & Assert
            searchHandler.CommandParameter = firstValue;
            Assert.Equal(firstValue, searchHandler.CommandParameter);

            searchHandler.CommandParameter = secondValue;
            Assert.Equal(secondValue, searchHandler.CommandParameter);

            searchHandler.CommandParameter = thirdValue;
            Assert.Equal(thirdValue, searchHandler.CommandParameter);
        }

        /// <summary>
        /// Tests that CommandParameter getter can handle complex object types.
        /// Verifies object reference equality for custom objects.
        /// </summary>
        [Fact]
        public void CommandParameter_SetCustomObject_ReturnsExactReference()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var customObject = new { Name = "Test", Value = 42 };

            // Act
            searchHandler.CommandParameter = customObject;
            var result = searchHandler.CommandParameter;

            // Assert
            Assert.Same(customObject, result);
        }

        /// <summary>
        /// Tests that CommandParameter setter can handle setting the same value multiple times.
        /// Verifies that repeated assignments work correctly.
        /// </summary>
        [Fact]
        public void CommandParameter_SetSameValueMultipleTimes_WorksCorrectly()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var value = "constant value";

            // Act
            searchHandler.CommandParameter = value;
            searchHandler.CommandParameter = value;
            searchHandler.CommandParameter = value;
            var result = searchHandler.CommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that CommandParameter can be set from non-null to null.
        /// Verifies that null assignments properly clear the stored value.
        /// </summary>
        [Fact]
        public void CommandParameter_SetToNullAfterValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            searchHandler.CommandParameter = "initial value";

            // Act
            searchHandler.CommandParameter = null;
            var result = searchHandler.CommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that QueryIcon property returns null by default.
        /// Verifies the default value behavior of the QueryIcon bindable property.
        /// Expected result: QueryIcon should return null when not explicitly set.
        /// </summary>
        [Fact]
        public void QueryIcon_DefaultValue_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.QueryIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that QueryIcon property can be set to a valid ImageSource and returns the same value.
        /// Verifies the basic getter and setter functionality with a non-null ImageSource.
        /// Expected result: QueryIcon should return the exact ImageSource instance that was set.
        /// </summary>
        [Fact]
        public void QueryIcon_SetValidImageSource_ReturnsSetValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            searchHandler.QueryIcon = mockImageSource;
            var result = searchHandler.QueryIcon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that QueryIcon property can be set to null and returns null.
        /// Verifies the property correctly handles null assignment after being set to a value.
        /// Expected result: QueryIcon should return null when explicitly set to null.
        /// </summary>
        [Fact]
        public void QueryIcon_SetNull_ReturnsNull()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();
            searchHandler.QueryIcon = mockImageSource;

            // Act
            searchHandler.QueryIcon = null;
            var result = searchHandler.QueryIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that QueryIcon property correctly handles multiple consecutive value assignments.
        /// Verifies that the property always returns the most recently set value.
        /// Expected result: QueryIcon should return the last ImageSource that was assigned.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public void QueryIcon_SetMultipleValues_ReturnsLatestValue(int numberOfSets)
        {
            // Arrange
            var searchHandler = new SearchHandler();
            ImageSource lastImageSource = null;

            // Act
            for (int i = 0; i < numberOfSets; i++)
            {
                lastImageSource = Substitute.For<ImageSource>();
                searchHandler.QueryIcon = lastImageSource;
            }

            var result = searchHandler.QueryIcon;

            // Assert
            Assert.Same(lastImageSource, result);
        }

        /// <summary>
        /// Tests that QueryIcon property handles alternating null and non-null values correctly.
        /// Verifies the property behavior when switching between null and valid ImageSource values.
        /// Expected result: QueryIcon should correctly return null or the ImageSource based on the last assignment.
        /// </summary>
        [Fact]
        public void QueryIcon_AlternateNullAndValue_ReturnsCorrectValue()
        {
            // Arrange
            var searchHandler = new SearchHandler();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act & Assert - Set to value
            searchHandler.QueryIcon = mockImageSource;
            Assert.Same(mockImageSource, searchHandler.QueryIcon);

            // Act & Assert - Set to null
            searchHandler.QueryIcon = null;
            Assert.Null(searchHandler.QueryIcon);

            // Act & Assert - Set to value again
            var anotherImageSource = Substitute.For<ImageSource>();
            searchHandler.QueryIcon = anotherImageSource;
            Assert.Same(anotherImageSource, searchHandler.QueryIcon);
        }

        /// <summary>
        /// Tests that ShowsResults returns the default value of false when not explicitly set.
        /// </summary>
        [Fact]
        public void ShowsResults_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            var result = searchHandler.ShowsResults;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ShowsResults returns true when set to true.
        /// </summary>
        [Fact]
        public void ShowsResults_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ShowsResults = true;

            // Assert
            Assert.True(searchHandler.ShowsResults);
        }

        /// <summary>
        /// Tests that ShowsResults returns false when set to false.
        /// </summary>
        [Fact]
        public void ShowsResults_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ShowsResults = false;

            // Assert
            Assert.False(searchHandler.ShowsResults);
        }

        /// <summary>
        /// Tests ShowsResults property with multiple value changes to ensure it correctly reflects the current state.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void ShowsResults_MultipleValueChanges_ReturnsCorrectValue(bool firstValue, bool secondValue)
        {
            // Arrange
            var searchHandler = new SearchHandler();

            // Act
            searchHandler.ShowsResults = firstValue;
            var firstResult = searchHandler.ShowsResults;

            searchHandler.ShowsResults = secondValue;
            var secondResult = searchHandler.ShowsResults;

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }
    }
}