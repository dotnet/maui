using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public class JsonStreamChunkerTests
{
	[Fact]
	public void ProcessLine_SimpleProgression_ProducesValidJson()
	{
		// Arrange
		var chunker = new JsonStreamChunker();
		var lines = new[]
		{
			"""{"name":"Matthew"}""",
			"""{"name":"Matthew Leibowitz"}""",
			"""{"age":32,"name":"Matthew Leibowitz"}"""
		};

		// Act
		var chunks = new List<string>();
		foreach (var line in lines)
		{
			var chunk = chunker.ProcessLine(line);
			chunks.Add(chunk);
		}
		chunks.Add(chunker.Finalize());

		var concatenated = string.Concat(chunks);

		// Debug output
		Assert.True(concatenated.Length > 0, $"Concatenated is empty. Chunks: [{string.Join("], [", chunks)}]");
		
		// Assert - concatenated should be valid JSON
		try
		{
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Matthew Leibowitz", doc.RootElement.GetProperty("name").GetString());
			Assert.Equal(32, doc.RootElement.GetProperty("age").GetInt32());
		}
		catch (JsonException ex)
		{
			Assert.Fail($"Invalid JSON: {ex.Message}\nConcatenated: {concatenated}\nChunks: [{string.Join("], [", chunks)}]");
		}
	}

	[Theory]
	[InlineData("maui-itinerary-1.jsonl")]
	[InlineData("mount-fuji-itinerary-1.jsonl")]
	[InlineData("sahara-itinerary-1.jsonl")]
	[InlineData("serengeti-itinerary-1.jsonl")]
	public void ProcessLines_FromJsonlFile_ProducesValidJsonMatchingFinalLine(string fileName)
	{
		// Arrange
		var chunker = new JsonStreamChunker();
		var filePath = Path.Combine("TestData", "ObjectStreams", fileName);
		var lines = File.ReadAllLines(filePath);

		// Act
		var chunks = new List<string>();
		foreach (var line in lines)
		{
			var chunk = chunker.ProcessLine(line);
			chunks.Add(chunk);
		}
		chunks.Add(chunker.Finalize());

		var concatenated = string.Concat(chunks);
		var finalLine = lines[^1];

		// Assert
		// 1. Concatenated chunks should be valid JSON
		JsonDocument concatenatedDoc;
		try
		{
			concatenatedDoc = JsonDocument.Parse(concatenated);
		}
		catch (JsonException ex)
		{
			Assert.Fail($"Concatenated chunks are not valid JSON: {ex.Message}\n\nConcatenated:\n{concatenated}");
			return;
		}

		// 2. Should match the final line when parsed
		var finalDoc = JsonDocument.Parse(finalLine);

		// Deep equality check
		var areEqual = JsonElementsAreEqual(finalDoc.RootElement, concatenatedDoc.RootElement);
		Assert.True(areEqual, 
			$"Concatenated JSON does not match final line.\n\nExpected:\n{finalLine}\n\nActual:\n{concatenated}");
	}

	[Fact]
	public void ProcessLine_NestedEmptyArraysThenContent_ProducesValidJson()
	{
		// Arrange - simulates progressive array/object construction
		var chunker = new JsonStreamChunker();
		var lines = new[]
		{
			"""{"days": []}""",
			"""{"days": [{"activities": []}]}""",
			"""{"days": [{"activities": [{"description": "Hello"}]}]}"""
		};

		// Act
		var chunks = new List<string>();
		foreach (var line in lines)
		{
			var chunk = chunker.ProcessLine(line);
			chunks.Add(chunk);
		}
		var finalChunk = chunker.Finalize();
		chunks.Add(finalChunk);

		var concatenated = string.Concat(chunks);

		// Assert - first check the concatenated output is parsable
		Assert.True(IsValidJson(concatenated), $"Invalid JSON produced:\n{concatenated}");
		
		var doc = JsonDocument.Parse(concatenated);
		var activity = doc.RootElement
			.GetProperty("days")[0]
			.GetProperty("activities")[0];
		Assert.Equal("Hello", activity.GetProperty("description").GetString());
	}
	
	private static bool IsValidJson(string json)
	{
		try
		{
			JsonDocument.Parse(json);
			return true;
		}
		catch
		{
			return false;
		}
	}

	[Fact]
	public void ProcessLine_StringExtension_ProducesValidFinalJson()
	{
		// Arrange
		var chunker = new JsonStreamChunker();

		// Act
		chunker.ProcessLine("""{"description": "Maui is a tropical"}""");
		chunker.ProcessLine("""{"description": "Maui is a tropical paradise"}""");
		chunker.Finalize();

		var concatenated = chunker.EmittedJson;

		// Assert - final result should be valid JSON with the final value
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Maui is a tropical paradise", doc.RootElement.GetProperty("description").GetString());
	}

	[Fact]
	public void ProcessLine_NewProperty_ClosesOpenStringAndAddsProperty()
	{
		// Arrange
		var chunker = new JsonStreamChunker();

		// Act
		var chunk1 = chunker.ProcessLine("""{"description": "Hello"}""");
		var chunk2 = chunker.ProcessLine("""{"description": "Hello", "name": "World"}""");
		var chunk3 = chunker.Finalize();

		var concatenated = chunk1 + chunk2 + chunk3;

		// Assert
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Hello", doc.RootElement.GetProperty("description").GetString());
		Assert.Equal("World", doc.RootElement.GetProperty("name").GetString());
	}

	[Fact]
	public void ProcessLine_NestedObject_HandlesCorrectly()
	{
		// Arrange
		var chunker = new JsonStreamChunker();
		var lines = new[]
		{
			"""{"days": [{"activities": []}]}""",
			"""{"days": [{"activities": [{"description": "Visit"}]}]}""",
			"""{"days": [{"activities": [{"description": "Visit the park"}]}]}"""
		};

		// Act
		var chunks = lines.Select(chunker.ProcessLine).ToList();
		chunks.Add(chunker.Finalize());
		var concatenated = string.Concat(chunks);

		// Assert
		var doc = JsonDocument.Parse(concatenated);
		var activity = doc.RootElement
			.GetProperty("days")[0]
			.GetProperty("activities")[0];
		Assert.Equal("Visit the park", activity.GetProperty("description").GetString());
	}

	[Fact]
	public void ProcessLine_PropertyOrderChange_StillProducesValidJson()
	{
		// Arrange - simulates the AI model reordering properties
		var chunker = new JsonStreamChunker();
		var lines = new[]
		{
			"""{"a": "1", "b": "2"}""",
			"""{"b": "2", "a": "1", "c": "3"}"""  // a and b swapped, c added
		};

		// Act
		var chunks = lines.Select(chunker.ProcessLine).ToList();
		chunks.Add(chunker.Finalize());
		var concatenated = string.Concat(chunks);

		// Assert
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("1", doc.RootElement.GetProperty("a").GetString());
		Assert.Equal("2", doc.RootElement.GetProperty("b").GetString());
		Assert.Equal("3", doc.RootElement.GetProperty("c").GetString());
	}

	/// <summary>
	/// Performs deep equality comparison of two JsonElements.
	/// </summary>
	private static bool JsonElementsAreEqual(JsonElement expected, JsonElement actual)
	{
		if (expected.ValueKind != actual.ValueKind)
			return false;

		switch (expected.ValueKind)
		{
			case JsonValueKind.Object:
				var expectedProps = expected.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
				var actualProps = actual.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

				if (expectedProps.Count != actualProps.Count)
					return false;

				foreach (var prop in expectedProps)
				{
					if (!actualProps.TryGetValue(prop.Key, out var actualValue))
						return false;
					if (!JsonElementsAreEqual(prop.Value, actualValue))
						return false;
				}
				return true;

			case JsonValueKind.Array:
				var expectedItems = expected.EnumerateArray().ToList();
				var actualItems = actual.EnumerateArray().ToList();

				if (expectedItems.Count != actualItems.Count)
					return false;

				for (int i = 0; i < expectedItems.Count; i++)
				{
					if (!JsonElementsAreEqual(expectedItems[i], actualItems[i]))
						return false;
				}
				return true;

			case JsonValueKind.String:
				return expected.GetString() == actual.GetString();

			case JsonValueKind.Number:
				return expected.GetRawText() == actual.GetRawText();

			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Null:
				return true;

			default:
				return false;
		}
	}
}
