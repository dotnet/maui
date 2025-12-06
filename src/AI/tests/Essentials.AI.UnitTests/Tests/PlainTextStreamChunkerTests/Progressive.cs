using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class PlainTextStreamChunkerTests
{
	/// <summary>
	/// Tests for progressive text streaming scenarios.
	/// </summary>
	public class ProgressiveTests
	{
		[Fact]
		public void Process_CharacterByCharacter_ProducesCorrectResult()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			var text = "Hello";
			
			// Act - simulate character-by-character streaming
			var chunks = new List<string>();
			for (int i = 1; i <= text.Length; i++)
			{
				chunks.Add(chunker.Process(text.Substring(0, i)));
			}
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal("Hello", concatenated);
		}

		[Fact]
		public void Process_WordByWord_ProducesCorrectResult()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			var progressiveText = new[]
			{
				"The",
				"The quick",
				"The quick brown",
				"The quick brown fox"
			};
			
			// Act
			var chunks = new List<string>();
			foreach (var text in progressiveText)
			{
				chunks.Add(chunker.Process(text));
			}
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal("The quick brown fox", concatenated);
		}

		[Fact]
		public void Process_LongText_HandlesCorrectly()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			var paragraph = "This is a longer piece of text that simulates a real AI response. " +
			                "It contains multiple sentences and should be processed correctly. " +
			                "The chunker should handle this without any issues.";
			
			// Simulate streaming in chunks
			var chunks = new List<string>();
			var chunkSize = 20;
			for (int i = chunkSize; i <= paragraph.Length; i += chunkSize)
			{
				chunks.Add(chunker.Process(paragraph.Substring(0, i)));
			}
			chunks.Add(chunker.Process(paragraph)); // Final complete text
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal(paragraph, concatenated);
		}

		[Fact]
		public void Process_WithNewlines_PreservesNewlines()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunks = new List<string>();
			chunks.Add(chunker.Process("Line 1"));
			chunks.Add(chunker.Process("Line 1\nLine 2"));
			chunks.Add(chunker.Process("Line 1\nLine 2\nLine 3"));
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal("Line 1\nLine 2\nLine 3", concatenated);
		}

		[Fact]
		public void Process_WithSpecialCharacters_HandlesCorrectly()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunks = new List<string>();
			chunks.Add(chunker.Process("Special chars: "));
			chunks.Add(chunker.Process("Special chars: <>&\"'"));
			chunks.Add(chunker.Process("Special chars: <>&\"' and unicode: 日本語"));
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal("Special chars: <>&\"' and unicode: 日本語", concatenated);
		}

		[Fact]
		public void Process_WithEmoji_HandlesCorrectly()
		{
			// Arrange
			var chunker = new PlainTextStreamChunker();
			
			// Act
			var chunks = new List<string>();
			chunks.Add(chunker.Process("Hello "));
			chunks.Add(chunker.Process("Hello 👋"));
			chunks.Add(chunker.Process("Hello 👋 World 🌍"));
			chunks.Add(chunker.Flush());

			// Assert
			var concatenated = string.Concat(chunks);
			Assert.Equal("Hello 👋 World 🌍", concatenated);
		}
	}
}
