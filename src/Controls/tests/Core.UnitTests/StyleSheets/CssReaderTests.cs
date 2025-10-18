#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.StyleSheets;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public sealed class CssReaderTests
    {
        /// <summary>
        /// Tests that Read returns characters from cache when cache is not empty,
        /// specifically testing the cache.Count > 0 branch.
        /// </summary>
        [Fact]
        public void Read_WhenCacheHasCharacters_ReturnsCharacterFromCache()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns((int)'/', (int)'x', (int)'y');
            var cssReader = new CssReader(mockReader);

            // Act - First call should populate cache when '/' is not followed by '*'
            cssReader.Read();
            // Second call should return from cache
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'/', result);
        }

        /// <summary>
        /// Tests that Read returns end-of-stream indicator when underlying reader returns <= 0.
        /// </summary>
        [Fact]
        public void Read_WhenUnderlyingReaderReturnsEndOfStream_ReturnsEndOfStream()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns(-1);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that Read returns end-of-stream when underlying reader returns zero.
        /// </summary>
        [Fact]
        public void Read_WhenUnderlyingReaderReturnsZero_ReturnsZero()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns(0);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that Read returns regular characters that are not part of comment syntax.
        /// </summary>
        [Theory]
        [InlineData('a')]
        [InlineData('Z')]
        [InlineData('1')]
        [InlineData(' ')]
        [InlineData('\n')]
        [InlineData('\t')]
        [InlineData('*')]
        [InlineData('{')]
        [InlineData('}')]
        public void Read_WhenCharacterIsNotSlash_ReturnsCharacterDirectly(char character)
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns((int)character);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)character, result);
        }

        /// <summary>
        /// Tests that Read handles single slash not followed by asterisk,
        /// testing the cache enqueue and dequeue logic.
        /// </summary>
        [Theory]
        [InlineData('a')]
        [InlineData('x')]
        [InlineData(' ')]
        [InlineData('\n')]
        public void Read_WhenSlashNotFollowedByAsterisk_ReturnsSlash(char followingChar)
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns((int)'/', (int)followingChar);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'/', result);
        }

        /// <summary>
        /// Tests that Read handles slash followed by end-of-stream,
        /// returning the cached slash character.
        /// </summary>
        [Fact]
        public void Read_WhenSlashFollowedByEndOfStream_ReturnsSlash()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns((int)'/', -1);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'/', result);
        }

        /// <summary>
        /// Tests that Read properly skips simple CSS comments and returns the next character.
        /// </summary>
        [Fact]
        public void Read_WhenSimpleComment_SkipsCommentAndReturnsNextCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/*comment*/a"
            mockReader.Read().Returns(
                (int)'/', (int)'*',           // Start comment
                (int)'c', (int)'o', (int)'m', (int)'m', (int)'e', (int)'n', (int)'t', // Comment content
                (int)'*', (int)'/',           // End comment
                (int)'a'                      // Next character
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'a', result);
        }

        /// <summary>
        /// Tests that Read properly skips empty CSS comments.
        /// </summary>
        [Fact]
        public void Read_WhenEmptyComment_SkipsCommentAndReturnsNextCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/**/b"
            mockReader.Read().Returns(
                (int)'/', (int)'*',  // Start comment
                (int)'*', (int)'/',  // End comment immediately
                (int)'b'             // Next character
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'b', result);
        }

        /// <summary>
        /// Tests that Read handles comments containing asterisks that are not part of the closing sequence.
        /// </summary>
        [Fact]
        public void Read_WhenCommentContainsAsterisks_SkipsCommentCorrectly()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/* * text * more */c"
            mockReader.Read().Returns(
                (int)'/', (int)'*',                    // Start comment
                (int)' ', (int)'*', (int)' ',          // Asterisk in comment (not closing)
                (int)'t', (int)'e', (int)'x', (int)'t',
                (int)' ', (int)'*', (int)' ',          // Another asterisk (not closing)  
                (int)'m', (int)'o', (int)'r', (int)'e',
                (int)' ',
                (int)'*', (int)'/',                    // End comment
                (int)'c'                               // Next character
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'c', result);
        }

        /// <summary>
        /// Tests that Read handles comments ending with multiple asterisks.
        /// </summary>
        [Fact]
        public void Read_WhenCommentEndsWithMultipleAsterisks_SkipsCommentCorrectly()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/*text***/d"
            mockReader.Read().Returns(
                (int)'/', (int)'*',               // Start comment
                (int)'t', (int)'e', (int)'x', (int)'t',
                (int)'*', (int)'*', (int)'*',     // Multiple asterisks
                (int)'/',                         // End comment
                (int)'d'                          // Next character
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'d', result);
        }

        /// <summary>
        /// Tests that Read returns end-of-stream when comment is not properly closed.
        /// </summary>
        [Fact]
        public void Read_WhenCommentNotClosed_ReturnsEndOfStream()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/*comment" (no closing)
            mockReader.Read().Returns(
                (int)'/', (int)'*',               // Start comment
                (int)'c', (int)'o', (int)'m', (int)'m', (int)'e', (int)'n', (int)'t',
                -1                                // End of stream
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that Read handles multiple consecutive comments by recursively calling itself.
        /// </summary>
        [Fact]
        public void Read_WhenMultipleConsecutiveComments_SkipsAllCommentsAndReturnsNextCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/*first*//*second*/e"
            mockReader.Read().Returns(
                (int)'/', (int)'*',                    // Start first comment
                (int)'f', (int)'i', (int)'r', (int)'s', (int)'t',
                (int)'*', (int)'/',                    // End first comment
                (int)'/', (int)'*',                    // Start second comment  
                (int)'s', (int)'e', (int)'c', (int)'o', (int)'n', (int)'d',
                (int)'*', (int)'/',                    // End second comment
                (int)'e'                               // Next character
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal((int)'e', result);
        }

        /// <summary>
        /// Tests that Read handles boundary character values correctly.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(127)]
        [InlineData(255)]
        [InlineData(65535)]
        public void Read_WhenBoundaryCharacterValues_ReturnsCorrectValue(int charValue)
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Read().Returns(charValue);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal(charValue, result);
        }

        /// <summary>
        /// Tests that Read handles comment followed immediately by end-of-stream during asterisk checking.
        /// </summary>
        [Fact]
        public void Read_WhenCommentHasAsteriskFollowedByEndOfStream_ReturnsEndOfStream()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "/*text*" (end of stream after asterisk)
            mockReader.Read().Returns(
                (int)'/', (int)'*',               // Start comment
                (int)'t', (int)'e', (int)'x', (int)'t',
                (int)'*',                         // Asterisk
                -1                                // End of stream
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Read();

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests the constructor throws ArgumentNullException when reader is null.
        /// </summary>
        [Fact]
        public void Constructor_WhenReaderIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new CssReader(null));
            Assert.Equal("reader", exception.ParamName);
        }

        /// <summary>
        /// Tests that Read correctly processes a sequence of mixed content and comments.
        /// </summary>
        [Fact]
        public void Read_WhenMixedContentAndComments_ProcessesCorrectly()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // Sequence: "a/*comment*/b"
            mockReader.Read().Returns(
                (int)'a',                         // First character
                (int)'/', (int)'*',               // Start comment
                (int)'c', (int)'o', (int)'m', (int)'m', (int)'e', (int)'n', (int)'t',
                (int)'*', (int)'/',               // End comment
                (int)'b',                         // Next character
                -1                                // End of stream
            );
            var cssReader = new CssReader(mockReader);

            // Act & Assert
            Assert.Equal((int)'a', cssReader.Read());  // First character
            Assert.Equal((int)'b', cssReader.Read());  // Character after comment
            Assert.Equal(-1, cssReader.Read());        // End of stream
        }

        /// <summary>
        /// Tests that Peek returns the cached character when cache contains items.
        /// Input: Cache has a character 'a'.
        /// Expected: Returns 'a' without calling underlying reader.
        /// </summary>
        [Fact]
        public void Peek_WhenCacheHasItems_ReturnsCachedCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            var cssReader = new CssReader(mockReader);

            // Force cache to have an item by reading a '/' not followed by '*'
            mockReader.Peek().Returns((int)'/', 0);
            mockReader.Read().Returns((int)'/');
            cssReader.Peek(); // This will cache the '/'

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'/', result);
        }

        /// <summary>
        /// Tests that Peek returns -1 when underlying reader is at end of stream.
        /// Input: Reader.Peek() returns -1.
        /// Expected: Returns -1.
        /// </summary>
        [Fact]
        public void Peek_WhenReaderAtEndOfStream_ReturnsMinusOne()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns(-1);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that Peek returns 0 when underlying reader returns 0.
        /// Input: Reader.Peek() returns 0.
        /// Expected: Returns 0.
        /// </summary>
        [Fact]
        public void Peek_WhenReaderReturnsZero_ReturnsZero()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns(0);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that Peek returns regular character when not a forward slash.
        /// Input: Reader.Peek() returns 'a'.
        /// Expected: Returns 'a'.
        /// </summary>
        [Fact]
        public void Peek_WhenCharacterIsNotSlash_ReturnsCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns((int)'a');
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'a', result);
        }

        /// <summary>
        /// Tests that Peek handles forward slash not followed by asterisk.
        /// Input: Reader has '/' followed by 'a'.
        /// Expected: Returns '/' from cache on subsequent calls.
        /// </summary>
        [Fact]
        public void Peek_WhenSlashNotFollowedByAsterisk_ReturnsCachedSlash()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns((int)'/', (int)'a');
            mockReader.Read().Returns((int)'/');
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'/', result);
        }

        /// <summary>
        /// Tests that Peek returns cached character when slash is followed by EOF.
        /// Input: Reader has '/' followed by -1 (EOF).
        /// Expected: Returns '/' from cache.
        /// </summary>
        [Fact]
        public void Peek_WhenSlashFollowedByEOF_ReturnsCachedSlash()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns((int)'/', -1);
            mockReader.Read().Returns((int)'/');
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'/', result);
        }

        /// <summary>
        /// Tests that Peek skips complete CSS comment and returns next character.
        /// Input: Reader has "/* comment */a".
        /// Expected: Returns 'a' after skipping comment.
        /// </summary>
        [Fact]
        public void Peek_WhenCompleteComment_SkipsCommentAndReturnsNextCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns((int)'/', (int)'*', (int)'a');
            mockReader.Read().Returns((int)'/', (int)'*', (int)' ', (int)'c', (int)'o', (int)'m', (int)'*', (int)'/', (int)'a');
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'a', result);
        }

        /// <summary>
        /// Tests that Peek returns -1 when EOF is encountered within comment.
        /// Input: Comment starts but never ends (EOF encountered).
        /// Expected: Returns -1.
        /// </summary>
        [Fact]
        public void Peek_WhenEOFWithinComment_ReturnsMinusOne()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns((int)'/', (int)'*');
            mockReader.Read().Returns((int)'/', (int)'*', (int)'c', (int)'o', (int)'m', -1);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that Peek handles consecutive comments properly.
        /// Input: Reader has "/* comment1 *//* comment2 */b".
        /// Expected: Returns 'b' after skipping both comments.
        /// </summary>
        [Fact]
        public void Peek_WhenConsecutiveComments_SkipsBothCommentsAndReturnsNextCharacter()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            // First call setup
            mockReader.Peek().Returns((int)'/', (int)'*', (int)'/', (int)'*', (int)'b');
            // Read sequence: /* comment1 *//* comment2 */b
            mockReader.Read().Returns(
                (int)'/', (int)'*', // First comment start
                (int)'c', (int)'*', (int)'/', // First comment end
                (int)'/', (int)'*', // Second comment start  
                (int)'t', (int)'*', (int)'/', // Second comment end
                (int)'b'
            );
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'b', result);
        }

        /// <summary>
        /// Tests that Peek handles asterisk not followed by forward slash within comment.
        /// Input: Comment contains '*' not followed by '/'.
        /// Expected: Continues reading until proper comment end.
        /// </summary>
        [Fact]
        public void Peek_WhenAsteriskNotFollowedBySlashInComment_ContinuesReading()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns((int)'/', (int)'*', (int)'x');
            mockReader.Read().Returns((int)'/', (int)'*', (int)'*', (int)'a', (int)'*', (int)'/', (int)'x');
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal((int)'x', result);
        }

        /// <summary>
        /// Tests that Peek handles edge case characters around slash and asterisk ASCII values.
        /// Input: Characters with ASCII values adjacent to '/' and '*'.
        /// Expected: Returns characters that are not part of comment syntax.
        /// </summary>
        [Theory]
        [InlineData(46)] // '.' (ASCII 46, just before '/' which is 47)
        [InlineData(48)] // '0' (ASCII 48, just after '/' which is 47) 
        [InlineData(41)] // ')' (ASCII 41, just before '*' which is 42)
        [InlineData(43)] // '+' (ASCII 43, just after '*' which is 42)
        public void Peek_WhenCharacterNearSlashOrAsterisk_ReturnsCharacter(int asciiValue)
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.Peek().Returns(asciiValue);
            var cssReader = new CssReader(mockReader);

            // Act
            var result = cssReader.Peek();

            // Assert
            Assert.Equal(asciiValue, result);
        }

        /// <summary>
        /// Tests that the CssReader constructor throws ArgumentNullException when reader parameter is null.
        /// This test ensures proper input validation and covers the null check condition.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void Constructor_NullReader_ThrowsArgumentNullException()
        {
            // Arrange
            TextReader reader = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new CssReader(reader));
            Assert.Equal("reader", exception.ParamName);
        }

        /// <summary>
        /// Tests that the CssReader constructor successfully creates an instance when provided with a valid TextReader.
        /// This test verifies that valid input is accepted and the instance is properly initialized.
        /// Expected result: CssReader instance is created without throwing any exceptions.
        /// </summary>
        [Fact]
        public void Constructor_ValidReader_CreatesInstance()
        {
            // Arrange
            var reader = Substitute.For<TextReader>();

            // Act
            var cssReader = new CssReader(reader);

            // Assert
            Assert.NotNull(cssReader);
        }
    }
}