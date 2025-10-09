using System;

using Microsoft.Maui;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Unit tests for StringExtensions class
    /// </summary>
    public class StringExtensionsTests
    {
        /// <summary>
        /// Tests IndexOfChar method with a character that exists in the string.
        /// Verifies that the method returns the correct zero-based index of the first occurrence.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="character">The character to find</param>
        /// <param name="expectedIndex">The expected index of the character</param>
        [Theory]
        [InlineData("hello", 'h', 0)]
        [InlineData("hello", 'e', 1)]
        [InlineData("hello", 'l', 2)]
        [InlineData("hello", 'o', 4)]
        [InlineData("a", 'a', 0)]
        [InlineData("abc", 'c', 2)]
        [InlineData("Hello World", 'W', 6)]
        [InlineData("test\nstring", '\n', 4)]
        [InlineData("tab\there", '\t', 3)]
        [InlineData("null\0char", '\0', 4)]
        [InlineData("unicode™test", '™', 7)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_CharacterExists_ReturnsCorrectIndex(string input, char character, int expectedIndex)
        {
            // Act
#if NETSTANDARD2_0
            int result = input.IndexOfChar(character);
#else
            int result = input.IndexOfChar(character);
#endif

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOfChar method with a character that does not exist in the string.
        /// Verifies that the method returns -1 when the character is not found.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="character">The character to find</param>
        [Theory]
        [InlineData("hello", 'x')]
        [InlineData("hello", 'H')]
        [InlineData("", 'a')]
        [InlineData("abc", 'd')]
        [InlineData("test", '\n')]
        [InlineData("noSpecialChars", '™')]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_CharacterNotFound_ReturnsMinusOne(string input, char character)
        {
            // Act
#if NETSTANDARD2_0
            int result = input.IndexOfChar(character);
#else
            int result = input.IndexOfChar(character);
#endif

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOfChar method with a string containing multiple occurrences of the same character.
        /// Verifies that the method returns the index of the first occurrence.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="character">The character to find</param>
        /// <param name="expectedIndex">The expected index of the first occurrence</param>
        [Theory]
        [InlineData("hello", 'l', 2)]
        [InlineData("aaaaaa", 'a', 0)]
        [InlineData("banana", 'a', 1)]
        [InlineData("test test", 't', 0)]
        [InlineData("abcabc", 'b', 1)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_MultipleOccurrences_ReturnsFirstIndex(string input, char character, int expectedIndex)
        {
            // Act
#if NETSTANDARD2_0
            int result = input.IndexOfChar(character);
#else
            int result = input.IndexOfChar(character);
#endif

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOfChar method with an empty string.
        /// Verifies that the method returns -1 for any character when searching in an empty string.
        /// </summary>
        /// <param name="character">The character to find</param>
        [Theory]
        [InlineData('a')]
        [InlineData(' ')]
        [InlineData('\0')]
        [InlineData('™')]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_EmptyString_ReturnsMinusOne(char character)
        {
            // Arrange
            string input = string.Empty;

            // Act
#if NETSTANDARD2_0
            int result = input.IndexOfChar(character);
#else
            int result = input.IndexOfChar(character);
#endif

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests IndexOfChar method with a null string.
        /// Verifies that the method throws ArgumentNullException when the input string is null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        [Trait("Category", "ProductionBugSuspected")]
        public void IndexOfChar_NullString_ThrowsArgumentNullException()
        {
            // Arrange
            string input = null;
            char character = 'a';

            // Act & Assert
#if NETSTANDARD2_0
            Assert.Throws<ArgumentNullException>(() => input.IndexOfChar(character));
#else
            Assert.Throws<ArgumentNullException>(() => input.IndexOfChar(character));
#endif
        }

        /// <summary>
        /// Tests IndexOfChar method with various special characters.
        /// Verifies that the method correctly handles special characters including whitespace, control characters, and unicode.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="character">The special character to find</param>
        /// <param name="expectedIndex">The expected index of the character</param>
        [Theory]
        [InlineData("hello world", ' ', 5)]
        [InlineData("line1\r\nline2", '\r', 5)]
        [InlineData("line1\r\nline2", '\n', 6)]
        [InlineData("tab\there", '\t', 3)]
        [InlineData("quote\"test", '"', 5)]
        [InlineData("slash\\test", '\\', 5)]
        [InlineData("zero\0byte", '\0', 4)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_SpecialCharacters_ReturnsCorrectIndex(string input, char character, int expectedIndex)
        {
            // Act
#if NETSTANDARD2_0
            int result = input.IndexOfChar(character);
#else
            int result = input.IndexOfChar(character);
#endif

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOfChar method with very long strings to ensure performance and correctness.
        /// Verifies that the method works correctly with large input strings.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_LongString_ReturnsCorrectIndex()
        {
            // Arrange
            string longString = new string('a', 10000) + "b" + new string('c', 10000);
            char searchChar = 'b';
            int expectedIndex = 10000;

            // Act
#if NETSTANDARD2_0
            int result = longString.IndexOfChar(searchChar);
#else
            int result = longString.IndexOfChar(searchChar);
#endif

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        /// <summary>
        /// Tests IndexOfChar method with a character at the very end of the string.
        /// Verifies that the method correctly finds characters at the last position.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IndexOfChar_CharacterAtEnd_ReturnsCorrectIndex()
        {
            // Arrange
            string input = "abcdefghijklmnopqrstuvwxyz";
            char character = 'z';
            int expectedIndex = 25;

            // Act
#if NETSTANDARD2_0
            int result = input.IndexOfChar(character);
#else
            int result = input.IndexOfChar(character);
#endif

            // Assert
            Assert.Equal(expectedIndex, result);
        }
    }
}