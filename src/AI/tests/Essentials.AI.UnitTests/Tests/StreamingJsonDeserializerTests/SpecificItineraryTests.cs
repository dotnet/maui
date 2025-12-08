using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.Models;
using Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	/// <summary>
	/// Specific tests for particular itinerary files that verify exact expected values.
	/// Generic file-based tests are in FileBasedTests.cs.
	/// </summary>
	public class SpecificItineraryTests(ITestOutputHelper output)
	{
		[Fact]
		public void ProcessChunk_MauiItineraryOpenai1_HasExpectedStructure()
		{
			// This test verifies the specific structure of maui-itinerary-openai-1.txt
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");

			Itinerary? lastItinerary = null;
			int successfulParseCount = 0;

			foreach (var chunk in chunks)
			{
				var result = deserializer.ProcessChunk(chunk);
				if (result != null && !string.IsNullOrEmpty(result.Title))
				{
					successfulParseCount++;
					lastItinerary = result;
				}
			}

			output.WriteLine($"Successful parses: {successfulParseCount} / {chunks.Length}");

			Assert.NotNull(lastItinerary);
			Assert.Equal("Paradise Unveiled: Maui's Marvels", lastItinerary.Title);
			Assert.Equal("Maui", lastItinerary.DestinationName);
			Assert.NotNull(lastItinerary.Days);
			Assert.Equal(3, lastItinerary.Days.Count); // Should have 3 complete days

			// Verify each day has the expected structure
			foreach (var day in lastItinerary.Days)
			{
				Assert.NotEmpty(day.Title);
				Assert.NotEmpty(day.Subtitle);
				Assert.NotEmpty(day.Destination);
				Assert.NotNull(day.Activities);
				Assert.Equal(3, day.Activities.Count); // Each day should have 3 activities
			}
		}

		[Fact]
		public void ProcessChunk_MountFujiItineraryShort1_HasExpectedStructure()
		{
			// This test verifies the specific structure of mount-fuji-itinerary-short-1.txt
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/mount-fuji-itinerary-short-1.txt");

			Itinerary? finalItinerary = null;

			foreach (var chunk in chunks)
			{
				finalItinerary = deserializer.ProcessChunk(chunk);
			}

			Assert.NotNull(finalItinerary);
			Assert.Equal("Mount Fuji Itinerary", finalItinerary.Title);
			Assert.Equal("Mount Fuji", finalItinerary.DestinationName);
			Assert.NotNull(finalItinerary.Description);
			Assert.Contains("Mount Fuji", finalItinerary.Description, StringComparison.Ordinal);
			Assert.NotNull(finalItinerary.Days);
			Assert.True(finalItinerary.Days.Count >= 1, $"Expected at least 1 day, got {finalItinerary.Days.Count}");
			
			// Verify first day has activities
			Assert.NotEmpty(finalItinerary.Days[0].Activities);
		}

		[Fact]
		public void ProcessChunk_MauiItineraryOpenai1_ShouldProgressThroughDeserialization()
		{
			// This test specifically validates the maui-itinerary-openai-1.txt file has good progression
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");
			
			var results = new List<DeserializationResult>();
			Itinerary? previousModel = null;

			// Process each chunk and track results
			foreach (var chunk in chunks)
			{
				var currentModel = deserializer.ProcessChunk(chunk);
				
				var result = new DeserializationResult
				{
					ChunkNumber = results.Count + 1,
					Chunk = chunk,
					Model = currentModel,
					IsSameInstanceAsPrevious = ReferenceEquals(currentModel, previousModel),
					IsNull = currentModel == null
				};
				
				results.Add(result);
				previousModel = currentModel;
			}

			output.WriteLine($"Total chunks: {chunks.Length}");
			output.WriteLine($"Total results: {results.Count}");
			
			var uniqueInstances = results
				.Where(r => !r.IsNull)
				.Select(r => r.Model)
				.Distinct()
				.Count();
			
			output.WriteLine($"Unique instances: {uniqueInstances}");
			
			// Final result should not be null
			Assert.NotNull(results[^1].Model);
			
			// We should have substantial unique instances throughout the stream (at least 400 for this 494-chunk stream)
			Assert.True(uniqueInstances >= 400, 
				$"Only {uniqueInstances} unique instances found. Expected at least 400 to show excellent progression.");
			
			// Final result should match direct deserialization
			var fullJson = string.Concat(chunks);
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, DeserializationOptions);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, results[^1].Model, strict: false);
		}

		[Fact]
		public void ProcessChunk_ItineraryWithRequiredProperties_SuccessfullyDeserializes()
		{
			// This test proves that the TypeInfoResolver bypasses 'required' constraints
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			
			string partialJson = "{\"title\": \"Test Trip\", \"destinationName\": \"Test\", \"description\": \"Test desc\", \"rationale\": \"Test\", \"days\": []}";
			
			var result = deserializer.ProcessChunk(partialJson);
			
			// Should successfully deserialize even though properties have 'required' modifier
			Assert.NotNull(result);
			Assert.Equal("Test Trip", result.Title);
		}

		[Fact]
		public void ProcessChunk_IncompleteItineraryJson_ReturnsPartialObject()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			
			// Only title and partial description
			var chunk1 = "{\"title\": \"Paris Trip\", \"destinationName\": \"Paris\"";
			var result1 = deserializer.ProcessChunk(chunk1);
			
			// Should return empty or previous state (incomplete JSON)
			Assert.NotNull(result1);
			
			// Complete the JSON
			var chunk2 = ", \"description\": \"City of lights\", \"rationale\": \"Romantic\", \"days\": []}";
			var result2 = deserializer.ProcessChunk(chunk2);
			
			// Now should have complete object despite 'required' properties
			Assert.NotNull(result2);
			Assert.Equal("Paris Trip", result2.Title);
			Assert.Equal("Paris", result2.DestinationName);
			Assert.Equal("City of lights", result2.Description);

			// Deep comparison: streaming result should equal direct deserialization
			var fullJson = chunk1 + chunk2;
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, DeserializationOptions);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, result2, strict: true);
		}

		private class DeserializationResult
		{
			public int ChunkNumber { get; set; }
			public string Chunk { get; set; } = "";
			public Itinerary? Model { get; set; }
			public bool IsSameInstanceAsPrevious { get; set; }
			public bool IsNull { get; set; }
		}
	}
}
