using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class PlainTextStreamChunkerTests
{
	/// <summary>
	/// Tests for basic plain text delta computation.
	/// </summary>
	public class BasicTests
	{
		[Fact]
		public void Process_SimpleProgression_EmitsDeltas()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunks = new List<string>();
			chunks.Add(chunker.Process("Hello"));
			chunks.Add(chunker.Process("Hello World"));
			chunks.Add(chunker.Process("Hello World!"));
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal("Hello World!", concatenated);
		}

		[Fact]
		public void Process_FirstChunk_EmitsEntireText()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunk = chunker.Process("Hello World");

			// Assert
			Assert.Equal("Hello World", chunk);
		}

		[Fact]
		public void Process_SameTextTwice_EmitsEmptyDelta()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunk1 = chunker.Process("Hello");
			var chunk2 = chunker.Process("Hello");

			// Assert
			Assert.Equal("Hello", chunk1);
			Assert.Equal("", chunk2);
		}

		[Fact]
		public void Process_EmptyString_ReturnsEmpty()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunk = chunker.Process("");

			// Assert
			Assert.Equal("", chunk);
		}

		[Fact]
		public void Process_NullString_ReturnsEmpty()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunk = chunker.Process(null!);

			// Assert
			Assert.Equal("", chunk);
		}

		[Fact]
		public void Flush_WithNoPriorState_ReturnsEmpty()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunk = chunker.Flush();

			// Assert
			Assert.Equal("", chunk);
		}

		[Fact]
		public void Flush_AfterProcessing_ReturnsEmpty()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			chunker.Process("Hello World");
			
			// Act
			var chunk = chunker.Flush();

			// Assert - plain text has no pending state
			Assert.Equal("", chunk);
		}
	}
}
