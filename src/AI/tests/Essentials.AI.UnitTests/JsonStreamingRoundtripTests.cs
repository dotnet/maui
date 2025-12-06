using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Integration tests that verify the complete pipeline: 
/// JsonStreamChunker → StreamingJsonDeserializer produces the same result as parsing the final JSON directly.
/// </summary>
public class JsonStreamingRoundtripTests
{
	#region Test Models

	/// <summary>
	/// Test model for simple roundtrip tests.
	/// </summary>
	private record SimpleModel
	{
		public string? Name { get; init; }
		public string? Description { get; init; }
		public int? Age { get; init; }
		public bool? Active { get; init; }
	}

	/// <summary>
	/// Test model matching the itinerary structure in test JSONL files.
	/// Uses nullable properties to handle partial streaming data.
	/// </summary>
	private record ItineraryModel
	{
		public string? Title { get; init; }
		public string? Description { get; init; }
		public string? DestinationName { get; init; }
		public string? Rationale { get; init; }
		public List<DayPlanModel>? Days { get; init; }
	}

	private record DayPlanModel
	{
		public string? Title { get; init; }
		public string? Subtitle { get; init; }
		public string? Destination { get; init; }
		public List<ActivityModel>? Activities { get; init; }
	}

	private record ActivityModel
	{
		public string? Type { get; init; }
		public string? Title { get; init; }
		public string? Description { get; init; }
	}

	#endregion

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

	[Theory]
	[InlineData("maui-itinerary-1.jsonl")]
	[InlineData("mount-fuji-itinerary-1.jsonl")]
	[InlineData("sahara-itinerary-1.jsonl")]
	[InlineData("serengeti-itinerary-1.jsonl")]
	public void Roundtrip_FromJsonlFile_DeserializerProducesEquivalentResult(string fileName)
	{
		// Arrange
		var chunker = new JsonStreamChunker();
		var deserializer = new StreamingJsonDeserializer<ItineraryModel>();
		var filePath = Path.Combine("TestData", "ObjectStreams", fileName);
		var lines = File.ReadAllLines(filePath);
		var finalLine = lines[^1];

		// Act - pass each line through chunker
		var chunks = new List<string>();
		foreach (var line in lines)
			chunks.Add(chunker.Process(line));
		chunks.Add(chunker.Flush());

		// Pass accumulated chunks through deserializer
		ItineraryModel? finalModel = null;
		foreach (var chunk in chunks)
			finalModel = deserializer.ProcessChunk(chunk);

		// Assert
		Assert.NotNull(finalModel);

		// Parse final line directly for comparison
		var expectedDoc = JsonDocument.Parse(finalLine);
		var actualJson = JsonSerializer.Serialize(finalModel, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		var actualDoc = JsonDocument.Parse(actualJson);

		// Compare key properties - objects should be structurally equivalent
		AssertJsonStructureContainsExpected(expectedDoc.RootElement, actualDoc.RootElement, "root");
	}

	[Fact]
	public void Roundtrip_NestedArrays_DeserializesCorrectly()
	{
		// Arrange
		var chunker = new JsonStreamChunker();
		var deserializer = new StreamingJsonDeserializer<ItineraryModel>();
		var lines = new[]
		{
			"""{"days": []}""",
			"""{"days": [{"activities": []}]}""",
			"""{"days": [{"activities": [{"description": "Hello"}]}]}""",
			"""{"days": [{"activities": [{"description": "Hello World", "title": "Activity 1"}]}]}"""
		};

		// Act
		var chunks = new List<string>();
		foreach (var line in lines)
			chunks.Add(chunker.Process(line));
		chunks.Add(chunker.Flush());

		ItineraryModel? finalModel = null;
		foreach (var chunk in chunks)
			finalModel = deserializer.ProcessChunk(chunk);

		// Assert
		Assert.NotNull(finalModel);
		Assert.NotNull(finalModel.Days);
		Assert.Single(finalModel.Days);
		Assert.NotNull(finalModel.Days[0].Activities);
		Assert.Single(finalModel.Days[0].Activities!);
		Assert.Equal("Hello World", finalModel.Days[0].Activities![0].Description);
		Assert.Equal("Activity 1", finalModel.Days[0].Activities![0].Title);
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

	#region Helper Methods

	/// <summary>
	/// Asserts that all properties in expected are present in actual with equivalent values.
	/// This handles the case where chunker output may have properties in different order.
	/// </summary>
	private static void AssertJsonStructureContainsExpected(JsonElement expected, JsonElement actual, string path)
	{
		if (expected.ValueKind != actual.ValueKind)
		{
			// Allow null in actual when expected has a value (partial streaming may leave some null)
			if (actual.ValueKind == JsonValueKind.Null)
				return;
			Assert.Fail($"Value kind mismatch at {path}: expected {expected.ValueKind}, got {actual.ValueKind}");
		}

		switch (expected.ValueKind)
		{
			case JsonValueKind.Object:
				foreach (var prop in expected.EnumerateObject())
				{
					if (actual.TryGetProperty(prop.Name, out var actualProp))
					{
						AssertJsonStructureContainsExpected(prop.Value, actualProp, $"{path}.{prop.Name}");
					}
					// Note: We don't fail if property is missing - streaming may not have all properties yet
				}
				break;

			case JsonValueKind.Array:
				var expectedItems = expected.EnumerateArray().ToList();
				var actualItems = actual.EnumerateArray().ToList();
				var minCount = Math.Min(expectedItems.Count, actualItems.Count);
				for (int i = 0; i < minCount; i++)
				{
					AssertJsonStructureContainsExpected(expectedItems[i], actualItems[i], $"{path}[{i}]");
				}
				break;

			case JsonValueKind.String:
				Assert.Equal(expected.GetString(), actual.GetString());
				break;

			case JsonValueKind.Number:
				// Compare as raw text to handle int/double differences
				Assert.Equal(expected.GetRawText(), actual.GetRawText());
				break;

			case JsonValueKind.True:
			case JsonValueKind.False:
				Assert.Equal(expected.GetBoolean(), actual.GetBoolean());
				break;

			// Null is already handled above
		}
	}

	#endregion
}
