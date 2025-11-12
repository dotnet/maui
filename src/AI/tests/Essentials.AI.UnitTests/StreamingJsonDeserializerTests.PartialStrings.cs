using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	/// <summary>
	/// Tests for partial string handling - strings that are cut mid-value during streaming.
	/// This is common in LLM streaming where long strings arrive in chunks.
	/// </summary>
	public class PartialStrings
	{
		[Fact]
		public void ProcessChunk_StringCutMidValue_ReconstructsCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// String is cut in the middle: "Hel
			var result1 = deserializer.ProcessChunk(@"{""text"": ""Hel");
			
			// Complete the string: lo World"
			var result2 = deserializer.ProcessChunk(@"lo World"", ""score"": 42}");
			
			Assert.NotNull(result2);
			Assert.Equal("Hello World", result2.Text);
			Assert.Equal(42, result2.Score);
		}

		[Fact]
		public void ProcessChunk_LongStringInMultipleChunks_BuildsGradually()
		{
			var deserializer = new StreamingJsonDeserializer<EmotionalResponse>();

			// Simulate streaming a long reply in chunks (like LLM output)
			var chunk1 = @"{""anger"": 0, ""reply"": ""Hello! I'm just a";
			var chunk2 = @" virtual assistant, so I don't have";
			var chunk3 = @" feelings, but I'm here"", ""happiness"": 90}";

			var result1 = deserializer.ProcessChunk(chunk1);
			var result2 = deserializer.ProcessChunk(chunk2);
			var result3 = deserializer.ProcessChunk(chunk3);

			Assert.NotNull(result3);
			Assert.Equal("Hello! I'm just a virtual assistant, so I don't have feelings, but I'm here", result3.Reply);
			Assert.Equal(0, result3.Anger);
			Assert.Equal(90, result3.Happiness);
		}

		[Fact]
		public void ProcessChunk_StringWithEscapedCharacters_HandlesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// String with escaped quote cut mid-escape
			var result1 = deserializer.ProcessChunk(@"{""text"": ""He said \""Hel");
			var result2 = deserializer.ProcessChunk(@"lo\"""", ""score"": 10}");

			Assert.NotNull(result2);
			Assert.Equal(@"He said ""Hello""", result2.Text);
			Assert.Equal(10, result2.Score);
		}

		[Fact]
		public void ProcessChunk_StringWithNewlines_PreservesContent()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Multi-line string split across chunks
			var chunk1 = @"{""text"": ""Line 1\nLine ";
			var chunk2 = @"2\nLine 3"", ""score"": 5}";

			var result1 = deserializer.ProcessChunk(chunk1);
			var result2 = deserializer.ProcessChunk(chunk2);

			Assert.NotNull(result2);
			Assert.Equal("Line 1\nLine 2\nLine 3", result2.Text);
			Assert.Equal(5, result2.Score);
		}

		[Fact]
		public void ProcessChunk_StringCutBeforeClosingQuote_WaitsForCompletion()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// String value complete but missing closing quote
			var result1 = deserializer.ProcessChunk(@"{""text"": ""Complete text");
			
			// Add closing quote and rest
			var result2 = deserializer.ProcessChunk(@""", ""score"": 100}");

			Assert.NotNull(result2);
			Assert.Equal("Complete text", result2.Text);
			Assert.Equal(100, result2.Score);
		}

		[Fact]
		public void ProcessChunk_EmptyString_HandlesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Empty string split (unlikely but possible)
			var result1 = deserializer.ProcessChunk(@"{""text"": """);
			var result2 = deserializer.ProcessChunk(@""", ""score"": 0}");

			Assert.NotNull(result2);
			Assert.Equal("", result2.Text);
			Assert.Equal(0, result2.Score);
		}

		[Fact]
		public void ProcessChunk_VeryLongString_StreamsEfficiently()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Build a long string in many small chunks (simulating token streaming)
			var baseText = "This is a very long text that represents LLM output streaming. ";
			var result = deserializer.ProcessChunk(@"{""text"": """);
			
			// Stream the long text in small chunks
			for (int i = 0; i < 10; i++)
			{
				result = deserializer.ProcessChunk(baseText);
			}
			
			// Close the string and object
			result = deserializer.ProcessChunk(@""", ""score"": 999}");

			Assert.NotNull(result);
			Assert.Equal(string.Concat(Enumerable.Repeat(baseText, 10)), result.Text);
			Assert.Equal(999, result.Score);
		}

		[Fact]
		public void ProcessChunk_MultipleStringSplitPatterns_AllParseCorrectly()
		{
			// Test pattern 1: Simple mid-word split
			var deserializer1 = new StreamingJsonDeserializer<SimpleModel>();
			deserializer1.ProcessChunk(@"{""text"": ""Hel");
			var result1 = deserializer1.ProcessChunk(@"lo"", ""score"": 1}");
			Assert.NotNull(result1);
			Assert.Equal("Hello", result1.Text);

			// Test pattern 2: Empty string split
			var deserializer2 = new StreamingJsonDeserializer<SimpleModel>();
			deserializer2.ProcessChunk(@"{""text"": """);
			var result2 = deserializer2.ProcessChunk(@"Test"", ""score"": 2}");
			Assert.NotNull(result2);
			Assert.Equal("Test", result2.Text);

			// Test pattern 3: Multi-word split
			var deserializer3 = new StreamingJsonDeserializer<SimpleModel>();
			deserializer3.ProcessChunk(@"{""text"": ""A B C ");
			var result3 = deserializer3.ProcessChunk(@"D E F"", ""score"": 3}");
			Assert.NotNull(result3);
			Assert.Equal("A B C D E F", result3.Text);

			// Test pattern 4: Multiple chunks
			var deserializer4 = new StreamingJsonDeserializer<SimpleModel>();
			deserializer4.ProcessChunk(@"{""text"": ""First");
			deserializer4.ProcessChunk(@" Second");
			var result4 = deserializer4.ProcessChunk(@" Third"", ""score"": 4}");
			Assert.NotNull(result4);
			Assert.Equal("First Second Third", result4.Text);
		}
	}
}
