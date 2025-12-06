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
	public class MauiItineraryProgressionTests
	{
		private readonly ITestOutputHelper _output;

		public MauiItineraryProgressionTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void ProcessChunk_MauiItinerary1_ShouldProgressThroughDeserialization()
		{
			// Arrange
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamLoader.LoadStream("maui-itinerary-1.txt");
			
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
			_output.WriteLine($"Total chunks: {chunks.Length}");
			_output.WriteLine($"Total results: {results.Count}");
			
			var nullResults = results.Count(r => r.IsNull);
			var sameInstanceResults = results.Count(r => r.IsSameInstanceAsPrevious && r.ChunkNumber > 1);
			var uniqueInstances = results
				.Where(r => !r.IsNull)
				.Select(r => r.Model)
				.Distinct()
				.Count();
			
			_output.WriteLine($"Null results: {nullResults}");
			_output.WriteLine($"Same instance as previous: {sameInstanceResults}");
			_output.WriteLine($"Unique instances: {uniqueInstances}");
			
			// Calculate percentage of same instances (excluding first chunk)
			var totalComparisons = results.Count - 1; // Exclude first chunk
			var sameInstancePercentage = totalComparisons > 0 
				? (double)sameInstanceResults / totalComparisons * 100 
				: 0;
			
			_output.WriteLine($"Same instance percentage: {sameInstancePercentage:F2}%");
			
			// Log first 20 results for debugging
			_output.WriteLine("\nFirst 20 chunks:");
			foreach (var result in results.Take(20))
			{
				var status = result.IsNull ? "NULL" 
					: result.IsSameInstanceAsPrevious ? "SAME" 
					: "NEW";
				_output.WriteLine($"Chunk {result.ChunkNumber}: {status} - '{result.Chunk.Substring(0, Math.Min(30, result.Chunk.Length))}'");
			}
			
			// Log chunks where we got same instance
			var sameInstanceChunks = results
				.Where(r => r.IsSameInstanceAsPrevious && r.ChunkNumber > 1)
				.ToList();
			
			if (sameInstanceChunks.Any())
			{
				_output.WriteLine($"\nChunks with SAME instance (first 20 of {sameInstanceChunks.Count}):");
				foreach (var result in sameInstanceChunks.Take(20))
				{
					_output.WriteLine($"Chunk {result.ChunkNumber}: '{result.Chunk}'");
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
			var chunks = DataStreamLoader.LoadStream("maui-itinerary-1.txt");
			
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
						_output.WriteLine($"Chunk {i + 1}: Title = '{currentTitle}'");
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
			var chunks = DataStreamLoader.LoadStream("maui-itinerary-1.txt");
			
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
						_output.WriteLine($"Chunk {i + 1}: Days={daysCount}, Activities in first day={activitiesCount}");
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
