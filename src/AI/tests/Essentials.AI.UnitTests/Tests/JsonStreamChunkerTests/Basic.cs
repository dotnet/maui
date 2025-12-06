using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamChunkerTests
{
	/// <summary>
	/// Tests for basic single-property progressions and fundamental chunker behavior.
	/// </summary>
	public class BasicTests
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
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

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

		[Fact]
		public void Process_StringExtension_ProducesValidFinalJson()
		{
			// Arrange
			var chunker = new JsonStreamChunker();

			// Act
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"description": "Maui is a tropical"}"""));
			chunks.Add(chunker.Process("""{"description": "Maui is a tropical paradise"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);

			// Assert - final result should be valid JSON with the final value
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Maui is a tropical paradise", doc.RootElement.GetProperty("description").GetString());
		}

		[Fact]
		public void Process_VeryShortStringGrows_ProducesValidJson()
		{
			// Test pattern: single char grows to full string
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"title": "M"}"""));
			chunks.Add(chunker.Process("""{"title": "Maui Itinerary"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Maui Itinerary", doc.RootElement.GetProperty("title").GetString());
		}

		[Fact]
		public void Process_EmptyStringGrows_ProducesValidJson()
		{
			// Test that empty strings can grow to non-empty
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"title": ""}"""));
			chunks.Add(chunker.Process("""{"title": "Hello World"}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal("Hello World", doc.RootElement.GetProperty("title").GetString());
		}

		[Fact]
		public void Process_NonStringTypes_EmittedImmediately()
		{
			// Numbers, booleans, null should be emitted immediately
			var chunker = new JsonStreamChunker();
			
			var chunks = new List<string>();
			chunks.Add(chunker.Process("""{"count": 42, "active": true, "data": null}"""));
			chunks.Add(chunker.Flush());

			var concatenated = string.Concat(chunks);
			
			Assert.True(IsValidJson(concatenated), $"Invalid JSON: {concatenated}");
			var doc = JsonDocument.Parse(concatenated);
			Assert.Equal(42, doc.RootElement.GetProperty("count").GetInt32());
			Assert.True(doc.RootElement.GetProperty("active").GetBoolean());
			Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);
		}
	}
}
