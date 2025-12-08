using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class BasicFunctionality
	{
		[Fact]
		public void ProcessChunk_EmptyChunk_ReturnsNull()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			var result = deserializer.ProcessChunk("");

			// With no data and no previous model, should return null
			Assert.Null(result);
		}

		[Fact]
		public void ProcessChunk_CompleteJsonInSingleChunk_DeserializesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			var result = deserializer.ProcessChunk(@"{""text"": ""Hello"", ""score"": 42}");

			Assert.NotNull(result);
			Assert.Equal("Hello", result.Text);
			Assert.Equal(42, result.Score);
		}

		[Fact]
		public void ProcessChunk_IncrementalChunks_UpdatesModelProgressively()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			// Chunk 1: First complete property
			var result1 = deserializer.ProcessChunk(@"{""text"": ""Hello""}");
			Assert.NotNull(result1);
			Assert.Equal("Hello", result1.Text);

			// Start new JSON with both properties
			deserializer.Reset();
			
			// Chunk 2: Both properties complete
			var result2 = deserializer.ProcessChunk(@"{""text"": ""Hello"", ""score"": 42}");
			Assert.NotNull(result2);
			Assert.Equal("Hello", result2.Text);
			Assert.Equal(42, result2.Score);
		}

		[Fact]
		public void Reset_ClearsStateAndModel()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			var before = deserializer.ProcessChunk(@"{""text"": ""Before"", ""score"": 100}");
			Assert.NotNull(before);
			Assert.Equal("Before", before.Text);

			deserializer.Reset();

			Assert.Empty(deserializer.PartialJson);
			Assert.Null(deserializer.LastGoodModel);
		}

		[Fact]
		public void PartialJson_ReflectsCurrentBuffer()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			deserializer.ProcessChunk(@"{""text"": ""Test");
			Assert.Contains(@"{""text"": ""Test", deserializer.PartialJson, StringComparison.Ordinal);

			deserializer.ProcessChunk(@"""}");
			Assert.Contains(@"{""text"": ""Test""}", deserializer.PartialJson, StringComparison.Ordinal);
		}
	}
}
