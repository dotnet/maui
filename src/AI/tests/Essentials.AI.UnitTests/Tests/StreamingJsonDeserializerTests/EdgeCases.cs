using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	/// <summary>
	/// Tests for edge cases and corner scenarios that might break the deserializer.
	/// </summary>
	public class EdgeCases
	{
		[Fact]
		public void ProcessChunk_SingleCharacterChunks_BuildsCompleteObject()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();
			var json = @"{""text"":""Hi"",""score"":5}";

			SimpleModel? result = null;
			foreach (var ch in json)
			{
				result = deserializer.ProcessChunk(ch.ToString());
			}

			Assert.NotNull(result);
			Assert.Equal("Hi", result.Text);
			Assert.Equal(5, result.Score);
		}

		[Fact]
		public void ProcessChunk_WhitespaceVariations_ParsesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Lots of extra whitespace
			var chunk1 = @"{  ""text""  :  ""Test"" ";
			var chunk2 = @" ,  ""score""  :  42  }";

			var result1 = deserializer.ProcessChunk(chunk1);
			var result2 = deserializer.ProcessChunk(chunk2);

			Assert.NotNull(result2);
			Assert.Equal("Test", result2.Text);
			Assert.Equal(42, result2.Score);
		}

		[Fact]
		public void ProcessChunk_DeeplyNestedObjects_HandlesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<NestedModel>();

			// Nested object split across chunks
			var chunk1 = @"{""text"":""Root"",""meta"":{""score"":10";
			var chunk2 = @",""author"":""John""}}";

			var result1 = deserializer.ProcessChunk(chunk1);
			var result2 = deserializer.ProcessChunk(chunk2);

			Assert.NotNull(result2);
			Assert.Equal("Root", result2.Text);
			Assert.NotNull(result2.Meta);
			Assert.Equal(10, result2.Meta.Score);
			Assert.Equal("John", result2.Meta.Author);
		}

		[Fact]
		public void ProcessChunk_ArrayWithManyElements_StreamsCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<ComplexModel>();

			// Start with title and array
			var result = deserializer.ProcessChunk(@"{""title"":""Store"",""items"":[");
			
			// Add multiple items one by one
			for (int i = 0; i < 5; i++)
			{
				if (i > 0)
					result = deserializer.ProcessChunk(",");
				
				result = deserializer.ProcessChunk($@"{{""name"":""Item{i}"",""price"":{i * 10},""category"":""Food""}}");
			}
			
			// Close array and object
			result = deserializer.ProcessChunk("]}");

			Assert.NotNull(result);
			Assert.Equal("Store", result.Title);
			Assert.NotNull(result.Items);
			Assert.Equal(5, result.Items.Count);
			Assert.Equal("Item4", result.Items[4].Name);
			Assert.Equal(40m, result.Items[4].Price);
		}

		[Fact]
		public void ProcessChunk_EmptyArray_ParsesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<ComplexModel>();

			var result = deserializer.ProcessChunk(@"{""title"":""Empty"",""items"":[]}");

			Assert.NotNull(result);
			Assert.Equal("Empty", result.Title);
			Assert.NotNull(result.Items);
			Assert.Empty(result.Items);
		}

		[Fact]
		public void ProcessChunk_EmptyObject_ParsesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			var result = deserializer.ProcessChunk(@"{}");

			Assert.NotNull(result);
			Assert.Null(result.Text);
			Assert.Equal(0, result.Score);
		}

		[Fact]
		public void ProcessChunk_OnlyOpeningBrace_ReturnsLastGood()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			var result = deserializer.ProcessChunk(@"{");

			// Should try to reconstruct and return a new empty object
			Assert.NotNull(result);
		}

		[Fact]
		public void ProcessChunk_TrailingCommas_HandlesGracefully()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Trailing comma before closing brace
			var result = deserializer.ProcessChunk(@"{""text"":""Test"",""score"":10,}");

			Assert.NotNull(result);
			Assert.Equal("Test", result.Text);
			Assert.Equal(10, result.Score);
		}

		[Fact]
		public void ProcessChunk_NumbersWithoutDecimals_ParseAsIntegers()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			var chunk1 = @"{""integer"":100";
			var chunk2 = @",""decimal"":3.14,""isActive"":true}";

			var result1 = deserializer.ProcessChunk(chunk1);
			var result2 = deserializer.ProcessChunk(chunk2);

			Assert.NotNull(result2);
			Assert.Equal(100, result2.Integer);
			Assert.Equal(3.14, result2.Decimal);
		}

		[Fact]
		public void ProcessChunk_LargeNumbers_HandlesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			var result = deserializer.ProcessChunk(@"{""integer"":2147483647,""decimal"":1.7976931348623157E+308}");

			Assert.NotNull(result);
			Assert.Equal(2147483647, result.Integer);
			Assert.Equal(1.7976931348623157E+308, result.Decimal);
		}

		[Fact]
		public void ProcessChunk_UnicodeCharacters_PreservesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Unicode characters in string
			var chunk1 = @"{""text"":""Hello 世界 ";
			var chunk2 = @"🌍"",""score"":42}";

			var result1 = deserializer.ProcessChunk(chunk1);
			var result2 = deserializer.ProcessChunk(chunk2);

			Assert.NotNull(result2);
			Assert.Equal("Hello 世界 🌍", result2.Text);
		}

		[Fact]
		public void ProcessChunk_MixedPropertyOrder_ParsesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Properties in different order than defined in class
			var result = deserializer.ProcessChunk(@"{""score"":99,""text"":""Reversed""}");

			Assert.NotNull(result);
			Assert.Equal("Reversed", result.Text);
			Assert.Equal(99, result.Score);
		}

		[Fact]
		public void ProcessChunk_DuplicateProperties_UsesLastValue()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Same property appears twice (JSON spec says last wins)
			var result = deserializer.ProcessChunk(@"{""text"":""First"",""text"":""Second"",""score"":1}");

			Assert.NotNull(result);
			Assert.Equal("Second", result.Text);
		}

		[Fact]
		public void ProcessChunk_UnicodeEscapeSequences_DecodesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Test common Unicode escape sequences
			var result = deserializer.ProcessChunk(@"{""text"":""Copyright \u00A9 2024"",""score"":1}");

			Assert.NotNull(result);
			Assert.Equal("Copyright © 2024", result.Text);
		}

		[Fact]
		public void ProcessChunk_UnicodeEscapeSequences_MultipleInString()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Multiple Unicode escapes in one string
			var result = deserializer.ProcessChunk(@"{""text"":""\u0048\u0065\u006C\u006C\u006F"",""score"":2}");

			Assert.NotNull(result);
			Assert.Equal("Hello", result.Text);
		}

		[Fact]
		public void ProcessChunk_UnicodeEscapeSequences_MixedWithRegularText()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Mix of regular text and Unicode escapes
			var result = deserializer.ProcessChunk(@"{""text"":""Price: \u20AC50"",""score"":3}");

			Assert.NotNull(result);
			Assert.Equal("Price: €50", result.Text);
		}

		[Fact]
		public void ProcessChunk_UnicodeEscapeSequences_SplitAcrossChunks()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Unicode escape split across chunks
			var chunk1 = @"{""text"":""Symbol: \u00";
			var chunk2 = @"A9"",""score"":4}";

			deserializer.ProcessChunk(chunk1);
			var result = deserializer.ProcessChunk(chunk2);

			Assert.NotNull(result);
			Assert.Equal("Symbol: ©", result.Text);
		}

		[Fact]
		public void ProcessChunk_UnicodeEscapeSequences_HighCodePoints()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Higher Unicode code points (emojis require surrogate pairs, but BMP chars work)
			var result = deserializer.ProcessChunk(@"{""text"":""\u2764 \u2605 \u263A"",""score"":5}");

			Assert.NotNull(result);
			Assert.Equal("❤ ★ ☺", result.Text);
		}

		[Fact]
		public void ProcessChunk_UnicodeEscapeSequences_WithOtherEscapes()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// Unicode escapes combined with other escape sequences
			var result = deserializer.ProcessChunk(@"{""text"":""Line1\n\u00A9 Corp\tTab"",""score"":6}");

			Assert.NotNull(result);
			Assert.Equal("Line1\n© Corp\tTab", result.Text);
		}
	}
}
