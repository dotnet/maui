using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class PlainTextStreamChunkerTests
{
	/// <summary>
	/// Tests for edge cases and boundary conditions.
	/// </summary>
	public class EdgeCaseTests
	{
		[Fact]
		public void Process_ShorterText_EmitsEmptyDelta()
		{
			// If the AI somehow sends shorter text (shouldn't happen, but defensive)
			var chunker = new PlainTextStreamChunker();
			
			var chunk1 = chunker.Process("Hello World");
			var chunk2 = chunker.Process("Hello"); // Shorter - should emit nothing
			
			Assert.Equal("Hello World", chunk1);
			Assert.Equal("", chunk2);
		}

		[Fact]
		public void Process_EmptyThenContent_EmitsContent()
		{
			var chunker = new PlainTextStreamChunker();
			
			var chunk1 = chunker.Process("");
			var chunk2 = chunker.Process("Hello");
			
			Assert.Equal("", chunk1);
			Assert.Equal("Hello", chunk2);
		}

		[Fact]
		public void Process_MultipleFlushCalls_ReturnsEmpty()
		{
			var chunker = new PlainTextStreamChunker();
			chunker.Process("Hello");
			
			var flush1 = chunker.Flush();
			var flush2 = chunker.Flush();
			var flush3 = chunker.Flush();
			
			Assert.Equal("", flush1);
			Assert.Equal("", flush2);
			Assert.Equal("", flush3);
		}

		[Fact]
		public void Process_AfterFlush_ContinuesWorking()
		{
			var chunker = new PlainTextStreamChunker();
			
			// First sequence
			var chunk1 = chunker.Process("Hello");
			var flush1 = chunker.Flush();
			
			// Continue processing (chunker maintains state)
			var chunk2 = chunker.Process("Hello World");
			var flush2 = chunker.Flush();
			
			Assert.Equal("Hello", chunk1);
			Assert.Equal("", flush1);
			Assert.Equal(" World", chunk2); // Delta from "Hello" to "Hello World"
			Assert.Equal("", flush2);
		}

		[Fact]
		public void Process_WhitespaceOnly_HandlesCorrectly()
		{
			var chunker = new PlainTextStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process(" "));
			chunks.Add(chunker.Process("  "));
			chunks.Add(chunker.Process("   "));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			Assert.Equal("   ", concatenated);
		}

		[Fact]
		public void Process_VeryLongString_HandlesCorrectly()
		{
			var chunker = new PlainTextStreamChunker();
			
			// Create a very long string
			var longText = new string('x', 10000);
			
			var chunks = new List<string>();
			// Simulate progressive chunks
			for (int i = 1000; i <= longText.Length; i += 1000)
			{
				chunks.Add(chunker.Process(longText.Substring(0, i)));
			}
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			Assert.Equal(longText, concatenated);
		}
	}
}
