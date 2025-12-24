using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamChunkerTests
{
	/// <summary>
	/// Tests for nested structures: arrays, objects, deeply nested paths.
	/// </summary>
	public class NestedTests
	{
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
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);

			// Assert - first check the concatenated output is parsable
			Assert.True(IsValidJson(concatenated), $"Invalid JSON produced:\n{concatenated}");
			
			var doc = JsonDocument.Parse(concatenated);
			var activity = doc.RootElement
				.GetProperty("days")[0]
				.GetProperty("activities")[0];
			Assert.Equal("Hello", activity.GetProperty("description").GetString());
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
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());
			var concatenated = string.Concat(chunks);

			// Assert
			var doc = JsonDocument.Parse(concatenated);
			var activity = doc.RootElement
				.GetProperty("days")[0]
				.GetProperty("activities")[0];
			Assert.Equal("Visit the park", activity.GetProperty("description").GetString());
		}

		[Fact]
		public void Process_ParentLevelChange_ClosesString()
		{
			// When a new array item appears, it should close strings in the previous item
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"items": [{"name": "First"}]}"""));
			// New array item appears - should close "First"
			chunks.Add(chunker.Process("""{"items": [{"name": "First"}, {"name": "Second"}]}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("First", doc.RootElement.GetProperty("items")[0].GetProperty("name").GetString());
			Assert.Equal("Second", doc.RootElement.GetProperty("items")[1].GetProperty("name").GetString());
		}

		[Fact]
		public void Process_EmptyObjectInArray_ProducesValidJson()
		{
			// Test pattern from mount-fuji line 35: {"activities": [{}, ...]}
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"activities": [{}]}"""));
			chunks.Add(chunker.Process("""{"activities": [{}, {"description": "Hello"}]}"""));
			chunks.Add(chunker.Flush());

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
			chunks.Add(chunker.Process("""{"days": [{"activities": [{"title": "Hello"}]}]}"""));
			chunks.Add(chunker.Process("""{"days": [{"activities": [{"title": "Hello World"}]}], "title": "Trip"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Trip", doc.RootElement.GetProperty("title").GetString());
			Assert.Equal("Hello World", doc.RootElement.GetProperty("days")[0].GetProperty("activities")[0].GetProperty("title").GetString());
		}

		[Fact]
		public void Process_DeeplyNestedWithMultipleArrays_ProducesValidJson()
		{
			// Complex nesting: days[] with activities[] inside
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"days": [{"activities": [{"title": "Drive"}]}]}"""));
			chunks.Add(chunker.Process("""{"days": [{"activities": [{"title": "Drive"}, {"title": "Lunch"}]}]}"""));
			chunks.Add(chunker.Process("""{"days": [{"activities": [{"title": "Drive"}, {"title": "Lunch"}]}, {"activities": []}]}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal(2, doc.RootElement.GetProperty("days").GetArrayLength());
			Assert.Equal(2, doc.RootElement.GetProperty("days")[0].GetProperty("activities").GetArrayLength());
		}
	}
}
