#nullable disable

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.StyleSheets;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TextReaderExtensionsTests
    {
        /// <summary>
        /// Tests ReadIdent with null reader to verify it throws NullReferenceException.
        /// Input: null TextReader
        /// Expected: NullReferenceException
        /// </summary>
        [Fact]
        public void ReadIdent_NullReader_ThrowsNullReferenceException()
        {
            // Arrange
            TextReader reader = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => reader.ReadIdent());
        }

        /// <summary>
        /// Tests ReadIdent with empty reader to verify it returns empty string.
        /// Input: Empty TextReader (Peek returns -1)
        /// Expected: Empty string returned
        /// </summary>
        [Fact]
        public void ReadIdent_EmptyReader_ReturnsEmptyString()
        {
            // Arrange
            var reader = new StringReader("");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("", result);
        }

        /// <summary>
        /// Tests ReadIdent with invalid first character to verify it throws Exception.
        /// Input: TextReader starting with digit '5'
        /// Expected: Exception thrown
        /// </summary>
        [Fact]
        public void ReadIdent_InvalidFirstCharacterDigit_ThrowsException()
        {
            // Arrange
            var reader = new StringReader("5abc");

            // Act & Assert
            Assert.Throws<Exception>(() => reader.ReadIdent());
        }

        /// <summary>
        /// Tests ReadIdent with invalid first character to verify it throws Exception.
        /// Input: TextReader starting with space ' '
        /// Expected: Exception thrown
        /// </summary>
        [Fact]
        public void ReadIdent_InvalidFirstCharacterSpace_ThrowsException()
        {
            // Arrange
            var reader = new StringReader(" abc");

            // Act & Assert
            Assert.Throws<Exception>(() => reader.ReadIdent());
        }

        /// <summary>
        /// Tests ReadIdent with invalid first character to verify it throws Exception.
        /// Input: TextReader starting with special character '@'
        /// Expected: Exception thrown
        /// </summary>
        [Fact]
        public void ReadIdent_InvalidFirstCharacterSpecial_ThrowsException()
        {
            // Arrange
            var reader = new StringReader("@abc");

            // Act & Assert
            Assert.Throws<Exception>(() => reader.ReadIdent());
        }

        /// <summary>
        /// Tests ReadIdent with leading dash followed by valid nmstart character.
        /// Input: TextReader with "-abc"
        /// Expected: "-abc" returned
        /// </summary>
        [Fact]
        public void ReadIdent_LeadingDashWithValidNmStart_ReturnsValidIdent()
        {
            // Arrange
            var reader = new StringReader("-abc");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("-abc", result);
        }

        /// <summary>
        /// Tests ReadIdent with leading dash followed by underscore.
        /// Input: TextReader with "-_test"
        /// Expected: "-_test" returned
        /// </summary>
        [Fact]
        public void ReadIdent_LeadingDashWithUnderscore_ReturnsValidIdent()
        {
            // Arrange
            var reader = new StringReader("-_test");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("-_test", result);
        }

        /// <summary>
        /// Tests ReadIdent with leading dash followed by invalid character.
        /// Input: TextReader with "-5abc"
        /// Expected: Exception thrown
        /// </summary>
        [Fact]
        public void ReadIdent_LeadingDashWithInvalidNmStart_ThrowsException()
        {
            // Arrange
            var reader = new StringReader("-5abc");

            // Act & Assert
            Assert.Throws<Exception>(() => reader.ReadIdent());
        }

        /// <summary>
        /// Tests ReadIdent with only dash character.
        /// Input: TextReader with "-"
        /// Expected: Exception thrown
        /// </summary>
        [Fact]
        public void ReadIdent_OnlyDash_ThrowsException()
        {
            // Arrange
            var reader = new StringReader("-");

            // Act & Assert
            Assert.Throws<Exception>(() => reader.ReadIdent());
        }

        /// <summary>
        /// Tests ReadIdent with valid identifier starting with letter.
        /// Input: TextReader with "abc123"
        /// Expected: "abc123" returned
        /// </summary>
        [Fact]
        public void ReadIdent_ValidIdentifierStartingWithLetter_ReturnsCompleteIdent()
        {
            // Arrange
            var reader = new StringReader("abc123");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("abc123", result);
        }

        /// <summary>
        /// Tests ReadIdent with valid identifier starting with underscore.
        /// Input: TextReader with "_test"
        /// Expected: "_test" returned
        /// </summary>
        [Fact]
        public void ReadIdent_ValidIdentifierStartingWithUnderscore_ReturnsCompleteIdent()
        {
            // Arrange
            var reader = new StringReader("_test");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("_test", result);
        }

        /// <summary>
        /// Tests ReadIdent with identifier containing all valid nmchar characters.
        /// Input: TextReader with "test_123-abc"
        /// Expected: "test_123-abc" returned
        /// </summary>
        [Fact]
        public void ReadIdent_IdentifierWithAllValidNmChars_ReturnsCompleteIdent()
        {
            // Arrange
            var reader = new StringReader("test_123-abc");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("test_123-abc", result);
        }

        /// <summary>
        /// Tests ReadIdent stops at invalid nmchar character.
        /// Input: TextReader with "abc@def"
        /// Expected: "abc" returned (stops at @)
        /// </summary>
        [Fact]
        public void ReadIdent_StopsAtInvalidNmChar_ReturnsPartialIdent()
        {
            // Arrange
            var reader = new StringReader("abc@def");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("abc", result);
        }

        /// <summary>
        /// Tests ReadIdent stops at space character.
        /// Input: TextReader with "abc def"
        /// Expected: "abc" returned (stops at space)
        /// </summary>
        [Fact]
        public void ReadIdent_StopsAtSpace_ReturnsPartialIdent()
        {
            // Arrange
            var reader = new StringReader("abc def");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("abc", result);
        }

        /// <summary>
        /// Tests ReadIdent with uppercase letters.
        /// Input: TextReader with "ABC"
        /// Expected: "ABC" returned
        /// </summary>
        [Fact]
        public void ReadIdent_UppercaseLetters_ReturnsValidIdent()
        {
            // Arrange
            var reader = new StringReader("ABC");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("ABC", result);
        }

        /// <summary>
        /// Tests ReadIdent with mixed case letters and all valid characters.
        /// Input: TextReader with "Test_123-Value"
        /// Expected: "Test_123-Value" returned
        /// </summary>
        [Fact]
        public void ReadIdent_MixedCaseWithValidChars_ReturnsCompleteIdent()
        {
            // Arrange
            var reader = new StringReader("Test_123-Value");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("Test_123-Value", result);
        }

        /// <summary>
        /// Tests ReadIdent with single valid character.
        /// Input: TextReader with "a"
        /// Expected: "a" returned
        /// </summary>
        [Fact]
        public void ReadIdent_SingleValidCharacter_ReturnsCharacter()
        {
            // Arrange
            var reader = new StringReader("a");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("a", result);
        }

        /// <summary>
        /// Tests ReadIdent with very long valid identifier.
        /// Input: TextReader with 500 character identifier
        /// Expected: Complete 500 character identifier returned
        /// </summary>
        [Fact]
        public void ReadIdent_VeryLongIdentifier_ReturnsCompleteIdent()
        {
            // Arrange
            var longIdent = "a" + new string('b', 499);
            var reader = new StringReader(longIdent);

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal(longIdent, result);
            Assert.Equal(500, result.Length);
        }

        /// <summary>
        /// Tests ReadIdent with identifier followed by EOF.
        /// Input: TextReader with "test" at end of stream
        /// Expected: "test" returned
        /// </summary>
        [Fact]
        public void ReadIdent_IdentifierAtEndOfStream_ReturnsCompleteIdent()
        {
            // Arrange
            var reader = new StringReader("test");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("test", result);
        }

        /// <summary>
        /// Tests ReadIdent with Unicode letters as valid nmstart.
        /// Input: TextReader with Unicode letter
        /// Expected: Unicode letter returned
        /// </summary>
        [Fact]
        public void ReadIdent_UnicodeLetterStart_ReturnsValidIdent()
        {
            // Arrange
            var reader = new StringReader("αβγ");

            // Act
            var result = reader.ReadIdent();

            // Assert
            Assert.Equal("αβγ", result);
        }

        /// <summary>
        /// Tests ReadIdent leaves reader position correctly after reading.
        /// Input: TextReader with "abc def"
        /// Expected: "abc" returned and reader positioned at space
        /// </summary>
        [Fact]
        public void ReadIdent_LeavesReaderPositionCorrect_ReturnsPartialAndPositionsCorrectly()
        {
            // Arrange
            var reader = new StringReader("abc def");

            // Act
            var result = reader.ReadIdent();
            var nextChar = reader.Read();

            // Assert
            Assert.Equal("abc", result);
            Assert.Equal(' ', (char)nextChar);
        }
    }
}
