using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public class JsonStreamChunkerTests
{
	[Fact]
	public void Process_SimpleProgression_ProducesValidJson()
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
			foreach (var chunk in chunker.Process(line))
				chunks.Add(chunk);
		}
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

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
	public void Process_FromJsonlFile_ProducesValidJsonMatchingFinalLine(string fileName)
	{
		// Arrange
		var chunker = new JsonStreamChunker();
		var filePath = Path.Combine("TestData", "ObjectStreams", fileName);
		var lines = File.ReadAllLines(filePath);

		// Act
		var chunks = new List<string>();
		foreach (var line in lines)
		{
			foreach (var chunk in chunker.Process(line))
				chunks.Add(chunk);
		}
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

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
	public void Process_NestedEmptyArraysThenContent_ProducesValidJson()
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
			foreach (var chunk in chunker.Process(line))
				chunks.Add(chunk);
		}
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

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
	public void Process_StringExtension_ProducesValidFinalJson()
	{
		// Arrange
		var chunker = new JsonStreamChunker();

		// Act
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"description": "Maui is a tropical"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"description": "Maui is a tropical paradise"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);

		// Assert - final result should be valid JSON with the final value
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Maui is a tropical paradise", doc.RootElement.GetProperty("description").GetString());
	}

	[Fact]
	public void Process_NewProperty_ClosesOpenStringAndAddsProperty()
	{
		// Arrange
		var chunker = new JsonStreamChunker();

		// Act
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"description": "Hello"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"description": "Hello", "name": "World"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);

		// Assert
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Hello", doc.RootElement.GetProperty("description").GetString());
		Assert.Equal("World", doc.RootElement.GetProperty("name").GetString());
	}

	[Fact]
	public void Process_NestedObject_HandlesCorrectly()
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
		var chunks = new List<string>();
		foreach (var line in lines)
		{
			foreach (var chunk in chunker.Process(line))
				chunks.Add(chunk);
		}
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);
		var concatenated = string.Concat(chunks);

		// Assert
		var doc = JsonDocument.Parse(concatenated);
		var activity = doc.RootElement
			.GetProperty("days")[0]
			.GetProperty("activities")[0];
		Assert.Equal("Visit the park", activity.GetProperty("description").GetString());
	}

	[Fact]
	public void Process_PropertyOrderChange_StillProducesValidJson()
	{
		// Arrange - simulates the AI model reordering properties
		var chunker = new JsonStreamChunker();
		var lines = new[]
		{
			"""{"a": "1", "b": "2"}""",
			"""{"b": "2", "a": "1", "c": "3"}"""  // a and b swapped, c added
		};

		// Act
		var chunks = new List<string>();
		foreach (var line in lines)
		{
			foreach (var chunk in chunker.Process(line))
				chunks.Add(chunk);
		}
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);
		var concatenated = string.Concat(chunks);

		// Assert
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("1", doc.RootElement.GetProperty("a").GetString());
		Assert.Equal("2", doc.RootElement.GetProperty("b").GetString());
		Assert.Equal("3", doc.RootElement.GetProperty("c").GetString());
	}

	[Fact]
	public void Process_SerengetiPattern_NewPropertyClosesString()
	{
		// This is the exact pattern from serengeti-itinerary-1.jsonl lines 1-2
		// When activities[] appears, it should close the subtitle string
		var chunker = new JsonStreamChunker();
		
		// Act
		var chunks = new List<string>();
		// Line 1
		foreach (var chunk in chunker.Process("""{"days": [{"subtitle": "Day"}]}"""))
			chunks.Add(chunk);
		// Line 2 - subtitle extended AND new property activities appears
		foreach (var chunk in chunker.Process("""{"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": []}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		// Should be valid JSON
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}\n\nChunks:\n[{string.Join("], [", chunks)}]");
		
		var doc = JsonDocument.Parse(concatenated);
		var day = doc.RootElement.GetProperty("days")[0];
		
		Assert.Equal("Day 1: Arrival and Wildlife Safari", day.GetProperty("subtitle").GetString());
		Assert.True(day.TryGetProperty("activities", out var activities), "activities property should exist");
		Assert.Equal(0, activities.GetArrayLength());
	}

	[Fact]
	public void Process_EmptyStringGrows_ProducesValidJson()
	{
		// Test that empty strings can grow to non-empty
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"title": ""}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"title": "Hello World"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Hello World", doc.RootElement.GetProperty("title").GetString());
	}

	[Fact]
	public void Process_MultipleNewStrings_AddsToPending()
	{
		// When multiple new strings appear at once, they should go to pending
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"title": "A", "subtitle": "B"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"title": "A", "subtitle": "B", "description": "C"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("A", doc.RootElement.GetProperty("title").GetString());
		Assert.Equal("B", doc.RootElement.GetProperty("subtitle").GetString());
		Assert.Equal("C", doc.RootElement.GetProperty("description").GetString());
	}

	[Fact]
	public void Process_ParentLevelChange_ClosesString()
	{
		// When a new array item appears, it should close strings in the previous item
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"items": [{"name": "First"}]}"""))
			chunks.Add(chunk);
		// New array item appears - should close "First"
		foreach (var chunk in chunker.Process("""{"items": [{"name": "First"}, {"name": "Second"}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("First", doc.RootElement.GetProperty("items")[0].GetProperty("name").GetString());
		Assert.Equal("Second", doc.RootElement.GetProperty("items")[1].GetProperty("name").GetString());
	}

	[Fact]
	public void Process_NonStringTypes_EmittedImmediately()
	{
		// Numbers, booleans, null should be emitted immediately
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"count": 42, "active": true, "data": null}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal(42, doc.RootElement.GetProperty("count").GetInt32());
		Assert.True(doc.RootElement.GetProperty("active").GetBoolean());
		Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);
	}

	[Fact]
	public void Process_EmptyObjectInArray_ProducesValidJson()
	{
		// Test pattern from mount-fuji line 35: {"activities": [{}, ...]}
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"activities": [{}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"activities": [{}, {"description": "Hello"}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal(2, doc.RootElement.GetProperty("activities").GetArrayLength());
	}

	[Fact]
	public void Process_RootLevelPropertyAppearsMidStream_ProducesValidJson()
	{
		// Test pattern: deep nesting first, then new root property appears
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"days": [{"activities": [{"title": "Hello"}]}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"days": [{"activities": [{"title": "Hello World"}]}], "title": "Trip"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Trip", doc.RootElement.GetProperty("title").GetString());
		Assert.Equal("Hello World", doc.RootElement.GetProperty("days")[0].GetProperty("activities")[0].GetProperty("title").GetString());
	}

	[Fact]
	public void Process_VeryShortStringGrows_ProducesValidJson()
	{
		// Test pattern: single char grows to full string
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"title": "M"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"title": "Maui Itinerary"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Maui Itinerary", doc.RootElement.GetProperty("title").GetString());
	}

	[Fact]
	public void Process_TypeFieldEmptyThenFilled_ProducesValidJson()
	{
		// Test pattern from serengeti: {"type": ""} → {"type": "Sightseeing"}
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"activities": [{"type": ""}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"activities": [{"type": "Sightseeing", "title": "Game Drive"}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		var activity = doc.RootElement.GetProperty("activities")[0];
		Assert.Equal("Sightseeing", activity.GetProperty("type").GetString());
		Assert.Equal("Game Drive", activity.GetProperty("title").GetString());
	}

	[Fact]
	public void Process_MultipleNewRootProperties_ProducesValidJson()
	{
		// Test pattern: multiple new root properties appear at once
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"description": "Hello"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"description": "Hello World", "title": "Trip", "rationale": "Fun"}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal("Hello World", doc.RootElement.GetProperty("description").GetString());
		Assert.Equal("Trip", doc.RootElement.GetProperty("title").GetString());
		Assert.Equal("Fun", doc.RootElement.GetProperty("rationale").GetString());
	}

	[Fact]
	public void Process_DeeplyNestedWithMultipleArrays_ProducesValidJson()
	{
		// Complex nesting: days[] with activities[] inside
		var chunker = new JsonStreamChunker();
		
		var chunks = new List<string>();
		foreach (var chunk in chunker.Process("""{"days": [{"activities": [{"title": "Drive"}]}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"days": [{"activities": [{"title": "Drive"}, {"title": "Lunch"}]}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Process("""{"days": [{"activities": [{"title": "Drive"}, {"title": "Lunch"}]}, {"activities": []}]}"""))
			chunks.Add(chunk);
		foreach (var chunk in chunker.Flush())
			chunks.Add(chunk);

		var concatenated = string.Concat(chunks);
		
		Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
		var doc = JsonDocument.Parse(concatenated);
		Assert.Equal(2, doc.RootElement.GetProperty("days").GetArrayLength());
		Assert.Equal(2, doc.RootElement.GetProperty("days")[0].GetProperty("activities").GetArrayLength());
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
