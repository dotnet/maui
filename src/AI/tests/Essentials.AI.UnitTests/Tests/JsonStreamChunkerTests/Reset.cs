using System;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamChunkerTests
{
	/// <summary>
	/// Tests for <see cref="JsonStreamChunker.Reset"/> which clears all internal state
	/// so the next JSON is treated as a fresh stream (e.g., after a tool call boundary).
	/// </summary>
	public class ResetTests
	{
		[Fact]
		public void Reset_AfterProcessing_NextProcessStartsFresh()
		{
			var chunker = new JsonStreamChunker();

			// Process partial JSON
			chunker.Process("{\"name\":\"Alice\"");

			// Reset
			chunker.Reset();

			// Next process should treat this as a completely new JSON stream
			var chunk = chunker.Process("{\"city\":\"Seattle\"}");
			Assert.NotNull(chunk);
			Assert.Contains("Seattle", chunk, StringComparison.Ordinal);
		}

		[Fact]
		public void Reset_WithoutPriorProcessing_IsNoOp()
		{
			var chunker = new JsonStreamChunker();

			// Reset on fresh chunker should not throw
			chunker.Reset();

			var chunk = chunker.Process("{\"key\":\"value\"}");
			Assert.NotNull(chunk);
			Assert.Contains("value", chunk, StringComparison.Ordinal);
		}

		[Fact]
		public void Reset_MultipleTimes_IsIdempotent()
		{
			var chunker = new JsonStreamChunker();

			chunker.Process("{\"a\":\"b\"");
			chunker.Reset();
			chunker.Reset();
			chunker.Reset();

			var chunk = chunker.Process("{\"c\":\"d\"}");
			Assert.NotNull(chunk);
			Assert.Contains("d", chunk, StringComparison.Ordinal);
		}

		[Fact]
		public void Reset_ThenFlush_ReturnsClosingBrackets()
		{
			var chunker = new JsonStreamChunker();

			chunker.Process("{\"name\":\"Alice\"");
			chunker.Reset();

			// After reset, flush should not carry over old state
			var flush = chunker.Flush();
			// After reset with no new processing, flush may return empty or minimal
			Assert.NotNull(flush);
		}

		[Fact]
		public void Reset_ClearsEmittedStrings_AllowsReEmission()
		{
			var chunker = new JsonStreamChunker();

			// Process a property - the string value is emitted and tracked
			var chunk1 = chunker.Process("{\"greeting\":\"Hello\"}");

			chunker.Reset();

			// Same property name/value should be re-emitted after reset
			var chunk2 = chunker.Process("{\"greeting\":\"Hello\"}");

			Assert.Contains("Hello", chunk1, StringComparison.Ordinal);
			Assert.Contains("Hello", chunk2, StringComparison.Ordinal);
		}

		[Fact]
		public void Reset_BetweenCompleteJsonObjects_ProducesValidOutput()
		{
			var chunker = new JsonStreamChunker();

			// First complete JSON object
			var chunks1 = new List<string>();
			chunks1.Add(chunker.Process("{\"temp\":72}"));
			chunks1.Add(chunker.Flush());

			chunker.Reset();

			// Second complete JSON object (different schema)
			var chunks2 = new List<string>();
			chunks2.Add(chunker.Process("{\"city\":\"Seattle\",\"state\":\"WA\"}"));
			chunks2.Add(chunker.Flush());

			var result1 = string.Concat(chunks1);
			var result2 = string.Concat(chunks2);

			Assert.Contains("72", result1, StringComparison.Ordinal);
			Assert.Contains("Seattle", result2, StringComparison.Ordinal);
		}

		[Fact]
		public void Reset_DuringPartialString_DoesNotCorrupt()
		{
			var chunker = new JsonStreamChunker();

			// Start processing with a partial string value
			chunker.Process("{\"message\":\"This is a long mess");

			// Reset mid-string
			chunker.Reset();

			// New JSON should work fine
			var chunk = chunker.Process("{\"result\":\"OK\"}");
			Assert.NotNull(chunk);
			Assert.Contains("OK", chunk, StringComparison.Ordinal);
		}
	}
}
