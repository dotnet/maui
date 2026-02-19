using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class PlainTextStreamChunkerTests
{
	/// <summary>
	/// Tests for <see cref="PlainTextStreamChunker.Reset"/> which clears internal state
	/// so the next text is treated as a fresh stream (e.g., after a tool call boundary).
	/// </summary>
	public class ResetTests
	{
		[Fact]
		public void Reset_AfterProcessing_NextProcessEmitsFullText()
		{
			var chunker = new PlainTextStreamChunker();

			// Process some text
			chunker.Process("Hello World");

			// Reset
			chunker.Reset();

			// Next Process should emit the full text, not a delta from "Hello World"
			var chunk = chunker.Process("Goodbye");
			Assert.Equal("Goodbye", chunk);
		}

		[Fact]
		public void Reset_SimulatesToolCallBoundary_PreservesFullPostToolText()
		{
			// This is the exact scenario that caused the bug:
			// Before tool call: AI streams "Here are some landmarks..."
			// Tool executes, AI resets text stream and starts fresh
			// Without Reset(), chunker would compute delta from old text and drop characters
			var chunker = new PlainTextStreamChunker();

			// Pre-tool text streaming
			chunker.Process("null");
			chunker.Process("null");  // Apple Intelligence emits "null" before tool calls

			// Tool call boundary - reset the chunker
			chunker.Reset();

			// Post-tool text streaming starts fresh
			var chunk1 = chunker.Process("Here");
			var chunk2 = chunker.Process("Here are some");
			var chunk3 = chunker.Process("Here are some landmarks");

			// Concatenate all post-reset chunks
			var result = chunk1 + chunk2 + chunk3;
			Assert.Equal("Here are some landmarks", result);
		}

		[Fact]
		public void Reset_WithoutPriorProcessing_IsNoOp()
		{
			var chunker = new PlainTextStreamChunker();

			// Reset on fresh chunker should not throw or break anything
			chunker.Reset();

			var chunk = chunker.Process("Hello");
			Assert.Equal("Hello", chunk);
		}

		[Fact]
		public void Reset_MultipleTimes_IsIdempotent()
		{
			var chunker = new PlainTextStreamChunker();

			chunker.Process("First text");
			chunker.Reset();
			chunker.Reset();
			chunker.Reset();

			var chunk = chunker.Process("Second text");
			Assert.Equal("Second text", chunk);
		}

		[Fact]
		public void Reset_ThenFlush_ReturnsEmpty()
		{
			var chunker = new PlainTextStreamChunker();

			chunker.Process("Some text");
			chunker.Reset();

			var flush = chunker.Flush();
			Assert.Equal("", flush);
		}

		[Fact]
		public void Reset_MultipleToolCallBoundaries_AllDeltasCorrect()
		{
			// Simulates multiple rounds of tool calling within one streaming response
			var chunker = new PlainTextStreamChunker();

			// Round 1: pre-tool text
			var r1 = chunker.Process("Looking up weather...");
			Assert.Equal("Looking up weather...", r1);

			// Tool call 1 boundary
			chunker.Reset();

			// Round 1: post-tool text
			var r1Post1 = chunker.Process("The weather");
			var r1Post2 = chunker.Process("The weather in Seattle is sunny");
			Assert.Equal("The weather", r1Post1);
			Assert.Equal(" in Seattle is sunny", r1Post2);

			// Tool call 2 boundary (chained tool)
			chunker.Reset();

			// Round 2: post-tool text
			var r2Post1 = chunker.Process("Also");
			var r2Post2 = chunker.Process("Also, the temperature is 72F");
			Assert.Equal("Also", r2Post1);
			Assert.Equal(", the temperature is 72F", r2Post2);
		}

		[Fact]
		public void Reset_ConcatenatedChunks_ProduceCorrectOutput()
		{
			// Verify that concatenating all chunks from a multi-tool scenario
			// produces the expected final text for each segment
			var chunker = new PlainTextStreamChunker();

			// Segment 1
			var seg1Chunks = new List<string>();
			seg1Chunks.Add(chunker.Process("Hello"));
			seg1Chunks.Add(chunker.Process("Hello World"));

			chunker.Reset();

			// Segment 2
			var seg2Chunks = new List<string>();
			seg2Chunks.Add(chunker.Process("Goodbye"));
			seg2Chunks.Add(chunker.Process("Goodbye Moon"));

			Assert.Equal("Hello World", string.Concat(seg1Chunks));
			Assert.Equal("Goodbye Moon", string.Concat(seg2Chunks));
		}
	}
}
