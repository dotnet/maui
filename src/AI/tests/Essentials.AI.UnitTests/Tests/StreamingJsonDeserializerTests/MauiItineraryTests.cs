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
	public class MauiItineraryTests(ITestOutputHelper output)
	{
		[Fact]
		public void ProcessChunk_MauiItinerary1_DeserializesAllChunksCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");

			Itinerary? lastItinerary = null;
			int successfulParseCount = 0;
			int totalChunks = 0;

			output.WriteLine($"Processing {chunks.Length} chunks");

			foreach (var chunk in chunks)
			{
				totalChunks++;
				var result = deserializer.ProcessChunk(chunk);
				
				if (result != null && !string.IsNullOrEmpty(result.Title))
				{
					successfulParseCount++;
					lastItinerary = result;
					
					// Log progress
					if (successfulParseCount <= 5 || successfulParseCount % 20 == 0)
					{
						output.WriteLine($"Chunk {totalChunks}: Successfully parsed. Title: {result.Title}, Days: {result.Days?.Count ?? 0}");
					}
				}
			}

			output.WriteLine($"Total chunks: {totalChunks}");
			output.WriteLine($"Successful parses: {successfulParseCount}");
			output.WriteLine($"Success rate: {(double)successfulParseCount / totalChunks:P2}");

			// Validate that we successfully parsed a reasonable number of chunks
			// The streaming parser works very well and should parse the majority of chunks
			Assert.True(successfulParseCount > 10, 
				$"Expected more than 10 successful parses from {totalChunks} chunks, but got {successfulParseCount}. " +
				"This suggests the closing logic is not working properly.");
			
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

			// Deep comparison: streaming result should equal direct deserialization
			var fullJson = string.Concat(chunks);
			var options = CreateItineraryDeserializationOptions();
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, options);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, lastItinerary, strict: true);
		}

		[Fact]
		public void ProcessChunk_MauiItinerary1_DebugReconstructionLogic()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");

			int parsedCount = 0;
			int totalChunks = 0;
			
			output.WriteLine("Testing first 50 chunks to debug reconstruction logic:");

			foreach (var chunk in chunks.Take(50))
			{
				totalChunks++;
				var result = deserializer.ProcessChunk(chunk);
				
				if (result != null && !string.IsNullOrEmpty(result.Title))
				{
					parsedCount++;
					
					// Show partial JSON buffer
					var partialJson = deserializer.PartialJson;
					output.WriteLine($"\n--- Chunk {totalChunks} (Parse #{parsedCount}) ---");
					output.WriteLine($"Added chunk: '{chunk}'");
					output.WriteLine($"Buffer length: {partialJson.Length} chars");
					output.WriteLine($"Last 50 chars of buffer: ...{(partialJson.Length > 50 ? partialJson.Substring(partialJson.Length - 50) : partialJson)}");
					output.WriteLine($"Title: {result.Title}");
					output.WriteLine($"Days count: {result.Days?.Count ?? 0}");
				}
			}

			output.WriteLine($"\nTotal processed: {totalChunks} chunks");
			output.WriteLine($"Successful parses: {parsedCount}");
			output.WriteLine($"Success rate: {(double)parsedCount / totalChunks:P2}");
		}

		[Fact]
		public void ProcessChunk_MauiItinerary1_ShouldProgressThroughDeserialization()
		{
			// Arrange
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");
			
			var results = new List<DeserializationResult>();
			Itinerary? previousModel = null;

			// Act - Process each chunk and track results
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

			// Assert - Analyze progression
			output.WriteLine($"Total chunks: {chunks.Length}");
			output.WriteLine($"Total results: {results.Count}");
			
			var nullResults = results.Count(r => r.IsNull);
			var sameInstanceResults = results.Count(r => r.IsSameInstanceAsPrevious && r.ChunkNumber > 1);
			var uniqueInstances = results
				.Where(r => !r.IsNull)
				.Select(r => r.Model)
				.Distinct()
				.Count();
			
			output.WriteLine($"Null results: {nullResults}");
			output.WriteLine($"Same instance as previous: {sameInstanceResults}");
			output.WriteLine($"Unique instances: {uniqueInstances}");
			
			// Calculate percentage of same instances (excluding first chunk)
			var totalComparisons = results.Count - 1; // Exclude first chunk
			var sameInstancePercentage = totalComparisons > 0 
				? (double)sameInstanceResults / totalComparisons * 100 
				: 0;
			
			output.WriteLine($"Same instance percentage: {sameInstancePercentage:F2}%");
			
			// Log first 20 results for debugging
			output.WriteLine("\nFirst 20 chunks:");
			foreach (var result in results.Take(20))
			{
				var status = result.IsNull ? "NULL" 
					: result.IsSameInstanceAsPrevious ? "SAME" 
					: "NEW";
				output.WriteLine($"Chunk {result.ChunkNumber}: {status} - '{result.Chunk.Substring(0, Math.Min(30, result.Chunk.Length))}'");
			}
			
			// Log chunks where we got same instance
			var sameInstanceChunks = results
				.Where(r => r.IsSameInstanceAsPrevious && r.ChunkNumber > 1)
				.ToList();
			
			if (sameInstanceChunks.Any())
			{
				output.WriteLine($"\nChunks with SAME instance (first 20 of {sameInstanceChunks.Count}):");
				foreach (var result in sameInstanceChunks.Take(20))
				{
					output.WriteLine($"Chunk {result.ChunkNumber}: '{result.Chunk}'");
				}
			}
			
			// Assertions
			Assert.NotNull(results[^1].Model); // Final result should not be null
			
			// We should have substantial unique instances throughout the stream (at least 400 for this 494-chunk stream)
			// With the comma-handling fix, we achieve ~95% update rate (473/494 unique instances)
			// Note: A few chunks still return same instance due to:
			// - Partial enum values that can't be parsed (e.g., "Hotel" when enum expects "HotelAndLodging")  
			// - Some whitespace or punctuation-only chunks
			Assert.True(uniqueInstances >= 400, 
				$"Only {uniqueInstances} unique instances found. Expected at least 400 to show excellent progression.");
			
			// Final result should match direct deserialization  
			// Use the same options as the streaming deserializer for fair comparison
			var fullJson = string.Concat(chunks);
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				Converters = { new JsonStringEnumConverter() },
				AllowTrailingCommas = true,
			});
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, results[^1].Model, strict: false);
		}

		[Fact]
		public void ProcessChunk_MauiItinerary1_ShouldProgressTitleField()
		{
			// Arrange
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");
			
			var titleProgression = new List<string?>();
			var chunkNumbers = new List<int>();

			// Act - Track title field progression
			for (int i = 0; i < chunks.Length; i++)
			{
				var currentModel = deserializer.ProcessChunk(chunks[i]);
				if (currentModel != null)
				{
					var currentTitle = currentModel.Title;
					if (titleProgression.Count == 0 || currentTitle != titleProgression[^1])
					{
						titleProgression.Add(currentTitle);
						chunkNumbers.Add(i + 1);
						output.WriteLine($"Chunk {i + 1}: Title = '{currentTitle}'");
					}
				}
			}

			// Assert
			Assert.NotEmpty(titleProgression);
			
			// We should see the title change at least once from initial empty to final complete title
			// Note: The streaming parser may not capture every intermediate string state for required string properties
			// since the object may not deserialize successfully until the string is complete
			Assert.True(titleProgression.Count >= 2, 
				$"Expected at least 2 title progressions (empty -> final), got {titleProgression.Count}. " +
				$"Titles: {string.Join(" -> ", titleProgression.Select(t => $"'{t}'"))}");
		}

		[Fact]
		public void ProcessChunk_MauiItinerary1_ShouldProgressDaysArray()
		{
			// Arrange
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamsHelper.GetFileLines("Itinerary/maui-itinerary-openai-1.txt");
			
			var daysProgression = new List<(int ChunkNumber, int? DaysCount, int? ActivitiesInFirstDay)>();

			// Act - Track days array progression
			for (int i = 0; i < chunks.Length; i++)
			{
				var currentModel = deserializer.ProcessChunk(chunks[i]);
				if (currentModel != null)
				{
					var daysCount = currentModel.Days?.Count;
					var activitiesCount = currentModel.Days?.FirstOrDefault()?.Activities?.Count;
					
					var lastEntry = daysProgression.LastOrDefault();
					if (daysCount != lastEntry.DaysCount || activitiesCount != lastEntry.ActivitiesInFirstDay)
					{
						daysProgression.Add((i + 1, daysCount, activitiesCount));
						output.WriteLine($"Chunk {i + 1}: Days={daysCount}, Activities in first day={activitiesCount}");
					}
				}
			}

			// Assert
			Assert.NotEmpty(daysProgression);
			
			// We should see the days array build up from null/0 to 3 days
			var finalDaysCount = daysProgression[^1].DaysCount;
			Assert.Equal(3, finalDaysCount);
			
			// We should see progression in the days array
			Assert.True(daysProgression.Count >= 3, 
				$"Expected at least 3 changes in days array, got {daysProgression.Count}");
		}

		[Fact]
		public void ProcessChunk_ItineraryWithRequiredProperties_SuccessfullyDeserializes()
		{
			// This test proves that the TypeInfoResolver bypasses 'required' constraints
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			
			string partialJson = "{\"title\": \"Test Trip\", \"destinationName\": \"Test\", \"description\": \"Test desc\", \"rationale\": \"Test\", \"days\": []}";
			
			var result = deserializer.ProcessChunk(partialJson);
			
			// Should successfully deserialize even though properties have 'required' modifier
			Assert.NotNull(result);
			Assert.Equal("Test Trip", result.Title);
		}

		[Theory]
		[InlineData("Itinerary/mount-fuji-itinerary-short-1.txt")]
		[InlineData("Itinerary/mount-fuji-itinerary-short-2.txt")]
		public void ProcessChunk_RealItineraryDataStream_DeserializesProgressively(string fileName)
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamsHelper.GetFileLines(fileName);

			Itinerary? lastItinerary = null;
			int successfulParseCount = 0;

			foreach (var chunk in chunks)
			{
				var result = deserializer.ProcessChunk(chunk);
				
				if (result != null && !string.IsNullOrEmpty(result.Title))
				{
					lastItinerary = result;
					successfulParseCount++;
				}
			}

			// Should have successfully parsed at least some chunks
			Assert.True(successfulParseCount > 0, $"Expected successful parses but got {successfulParseCount}");
			Assert.NotNull(lastItinerary);
			Assert.NotEmpty(lastItinerary.Title);
			Assert.NotEmpty(lastItinerary.DestinationName);
			Assert.NotNull(lastItinerary.Days);
			Assert.NotEmpty(lastItinerary.Days);

			// Deep comparison: streaming result should equal direct deserialization
			var fullJson = string.Concat(chunks);
			var options = CreateItineraryDeserializationOptions();
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, options);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, lastItinerary, strict: true);
		}

		[Fact]
		public void ProcessChunk_MountFujiItinerary1_ReturnsCompleteItinerary()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
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

			// Deep comparison: streaming result should equal direct deserialization
			var fullJson = string.Concat(chunks);
			var options = CreateItineraryDeserializationOptions();
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, options);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, finalItinerary, strict: true);
		}

		[Fact]
		public void ProcessChunk_IncompleteItineraryJson_ReturnsPartialObject()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			
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
			var options = CreateItineraryDeserializationOptions();
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, options);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, result2, strict: true);
		}

		private static JsonSerializerOptions CreateItineraryDeserializationOptions()
		{
			return new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				Converters = { new JsonStringEnumConverter() }
			};
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
