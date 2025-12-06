using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamingRoundtripTests
{
	/// <summary>
	/// Tests for simple progressions and basic scenarios.
	/// </summary>
	public class SimpleProgressionTests
	{
		[Fact]
		public void Roundtrip_SimpleProgression_DeserializesCorrectly()
		{
			// Arrange
			var chunker = new JsonStreamChunker();
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();
			var lines = new[]
			{
				"""{"name":"Matthew"}""",
				"""{"name":"Matthew Leibowitz"}""",
				"""{"age":32,"name":"Matthew Leibowitz"}"""
			};

			// Act - pass through chunker
			var chunks = new List<string>();
			foreach (var line in lines)
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

			// Pass chunks through deserializer
			SimpleModel? finalModel = null;
			foreach (var chunk in chunks)
				finalModel = deserializer.ProcessChunk(chunk);

			// Assert
			Assert.NotNull(finalModel);
			Assert.Equal("Matthew Leibowitz", finalModel.Name);
			Assert.Equal(32, finalModel.Age);

			// Verify against direct parse of final line
			var expected = JsonSerializer.Deserialize<SimpleModel>(lines[^1], new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			Assert.NotNull(expected);
			Assert.Equal(expected.Name, finalModel.Name);
			Assert.Equal(expected.Age, finalModel.Age);
		}

		[Fact]
		public void Roundtrip_EmptyStringGrows_DeserializesCorrectly()
		{
			// Arrange
			var chunker = new JsonStreamChunker();
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();
			var lines = new[]
			{
				"""{"name": ""}""",
				"""{"name": "Matthew"}""",
				"""{"name": "Matthew", "description": "Developer"}"""
			};

			// Act
			var chunks = new List<string>();
			foreach (var line in lines)
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

			SimpleModel? finalModel = null;
			foreach (var chunk in chunks)
				finalModel = deserializer.ProcessChunk(chunk);

			// Assert
			Assert.NotNull(finalModel);
			Assert.Equal("Matthew", finalModel.Name);
			Assert.Equal("Developer", finalModel.Description);
		}

		[Fact]
		public void Roundtrip_PropertyReordering_DeserializesCorrectly()
		{
			// This tests the scenario where AI reorders properties between chunks
			var chunker = new JsonStreamChunker();
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();
			var lines = new[]
			{
				"""{"name": "A", "description": "B"}""",
				"""{"description": "B", "name": "A", "active": true}"""  // reordered + new property
			};

			// Act
			var chunks = new List<string>();
			foreach (var line in lines)
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

			SimpleModel? finalModel = null;
			foreach (var chunk in chunks)
				finalModel = deserializer.ProcessChunk(chunk);

			// Assert
			Assert.NotNull(finalModel);
			Assert.Equal("A", finalModel.Name);
			Assert.Equal("B", finalModel.Description);
			Assert.True(finalModel.Active);
		}
	}
}
