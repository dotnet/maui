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
	}
}
