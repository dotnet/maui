using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamChunkerTests
{
	/// <summary>
	/// Tests for property handling: new properties, multiple strings, property ordering.
	/// </summary>
	public class PropertyTests
	{
		[Fact]
		public void Process_NewProperty_ClosesOpenStringAndAddsProperty()
		{
			// Arrange
			var chunker = new JsonStreamChunker();

			// Act
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"description": "Hello"}"""));
			chunks.Add(chunker.Process("""{"description": "Hello", "name": "World"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);

			// Assert
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Hello", doc.RootElement.GetProperty("description").GetString());
			Assert.Equal("World", doc.RootElement.GetProperty("name").GetString());
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
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());
			var concatenated = string.Concat(chunks);

			// Assert
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("1", doc.RootElement.GetProperty("a").GetString());
			Assert.Equal("2", doc.RootElement.GetProperty("b").GetString());
			Assert.Equal("3", doc.RootElement.GetProperty("c").GetString());
		}

		[Fact]
		public void Process_MultipleNewStrings_AddsToPending()
		{
			// When multiple new strings appear at once, they should go to pending
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"title": "A", "subtitle": "B"}"""));
			chunks.Add(chunker.Process("""{"title": "A", "subtitle": "B", "description": "C"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("A", doc.RootElement.GetProperty("title").GetString());
			Assert.Equal("B", doc.RootElement.GetProperty("subtitle").GetString());
			Assert.Equal("C", doc.RootElement.GetProperty("description").GetString());
		}

		[Fact]
		public void Process_MultipleNewRootProperties_ProducesValidJson()
		{
			// Test pattern: multiple new root properties appear at once
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"description": "Hello"}"""));
			chunks.Add(chunker.Process("""{"description": "Hello World", "title": "Trip", "rationale": "Fun"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Hello World", doc.RootElement.GetProperty("description").GetString());
			Assert.Equal("Trip", doc.RootElement.GetProperty("title").GetString());
			Assert.Equal("Fun", doc.RootElement.GetProperty("rationale").GetString());
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
			chunks.Add(chunker.Process("""{"days": [{"subtitle": "Day"}]}"""));
			// Line 2 - subtitle extended AND new property activities appears
			chunks.Add(chunker.Process("""{"days": [{"subtitle": "Day 1: Arrival and Wildlife Safari", "activities": []}]}"""));
			chunks.Add(chunker.Flush());

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
		public void Process_TypeFieldEmptyThenFilled_ProducesValidJson()
		{
			// Test pattern from serengeti: {"type": ""} → {"type": "Sightseeing"}
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"activities": [{"type": ""}]}"""));
			chunks.Add(chunker.Process("""{"activities": [{"type": "Sightseeing", "title": "Game Drive"}]}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			var activity = doc.RootElement.GetProperty("activities")[0];
			Assert.Equal("Sightseeing", activity.GetProperty("type").GetString());
			Assert.Equal("Game Drive", activity.GetProperty("title").GetString());
		}

		[Fact]
		public void Process_StringAndArrayAtSameLevel_BothHandledCorrectly()
		{
			// When a string and an array appear at the same level, both are "growable"
			// We need to handle this without knowing which will change
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			// First: empty object in days array
			chunks.Add(chunker.Process("""{"days": [{}]}"""));
			// Second: subtitle (string) and activities (array) both appear at days[0] level
			chunks.Add(chunker.Process("""{"days": [{"subtitle": "", "activities": []}]}"""));
			// Third: subtitle grows, activities still empty
			chunks.Add(chunker.Process("""{"days": [{"subtitle": "Day 1", "activities": []}]}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}\n\nChunks:\n[{string.Join("], [", chunks)}]");
			var doc = JsonDocument.Parse(concatenated);
			var day = doc.RootElement.GetProperty("days")[0];
			Assert.Equal("Day 1", day.GetProperty("subtitle").GetString());
			Assert.True(day.TryGetProperty("activities", out var activities), "activities property should exist");
			Assert.Equal(0, activities.GetArrayLength());
		}

		[Fact]
		public void Process_StringAndArrayAppearTogether_ArrayGrows()
		{
			// Opposite case: array is the one that grows
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"days": [{}]}"""));
			chunks.Add(chunker.Process("""{"days": [{"subtitle": "", "activities": []}]}"""));
			// Array grows while string stays empty
			chunks.Add(chunker.Process("""{"days": [{"subtitle": "", "activities": [{"type": ""}]}]}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}\n\nChunks:\n[{string.Join("], [", chunks)}]");
			var doc = JsonDocument.Parse(concatenated);
			var day = doc.RootElement.GetProperty("days")[0];
			Assert.Equal("", day.GetProperty("subtitle").GetString());
			var activities = day.GetProperty("activities");
			Assert.Equal(1, activities.GetArrayLength());
		}

		[Fact]
		public void Process_MultipleGrowableTypes_AllHandled()
		{
			// Multiple growable types: string, array, object
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"title": "", "items": [], "meta": {}}"""));
			// String grows
			chunks.Add(chunker.Process("""{"title": "Hello", "items": [], "meta": {}}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}\n\nChunks:\n[{string.Join("], [", chunks)}]");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Hello", doc.RootElement.GetProperty("title").GetString());
			Assert.Equal(0, doc.RootElement.GetProperty("items").GetArrayLength());
			Assert.Equal(JsonValueKind.Object, doc.RootElement.GetProperty("meta").ValueKind);
		}
	}
}
