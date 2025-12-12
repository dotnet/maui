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
	public class FileBasedTests(ITestOutputHelper output)
	{
		[Theory]
		[MemberData(nameof(DataStreamsHelper.TxtItineraries), MemberType = typeof(DataStreamsHelper))]
		public void ProcessChunk_TxtFile_DeserializesProgressively(string fileName, string filePath)
		{
			_ = fileName; // Used for test display name
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = File.ReadAllLines(filePath);

			Itinerary? lastItinerary = null;

			foreach (var chunk in chunks)
			{
				lastItinerary = deserializer.ProcessChunk(chunk);
				Assert.NotNull(lastItinerary);
			}

			Assert.NotNull(lastItinerary);
			Assert.NotEmpty(lastItinerary.Title);
			Assert.NotEmpty(lastItinerary.DestinationName);
			Assert.NotNull(lastItinerary.Days);
			Assert.NotEmpty(lastItinerary.Days);
		}

		[Theory]
		[MemberData(nameof(DataStreamsHelper.TxtItineraries), MemberType = typeof(DataStreamsHelper))]
		public void ProcessChunk_TxtFile_FinalResultMatchesDirectDeserialization(string fileName, string filePath)
		{
			_ = fileName; // Used for test display name
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = File.ReadAllLines(filePath);

			Itinerary? finalItinerary = null;

			foreach (var chunk in chunks)
			{
				finalItinerary = deserializer.ProcessChunk(chunk);
			}

			Assert.NotNull(finalItinerary);

			// Deep comparison: streaming result should equal direct deserialization
			var fullJson = string.Concat(chunks);
			var directDeserialized = JsonSerializer.Deserialize<Itinerary>(fullJson, DeserializationOptions);
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, finalItinerary, strict: true);
		}

		[Theory]
		[MemberData(nameof(DataStreamsHelper.TxtItineraries), MemberType = typeof(DataStreamsHelper))]
		public void ProcessChunk_TxtFile_ShouldProgressDaysArray(string fileName, string filePath)
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = File.ReadAllLines(filePath);
			
			var daysProgression = new List<(int ChunkNumber, int? DaysCount, int? ActivitiesInFirstDay)>();

			// Track days array progression
			for (int i = 0; i < chunks.Length; i++)
			{
				var currentModel = deserializer.ProcessChunk(chunks[i]);
				Assert.NotNull(currentModel);

				var daysCount = currentModel.Days?.Count;
				var activitiesCount = currentModel.Days?.FirstOrDefault()?.Activities?.Count;
				
				var lastEntry = daysProgression.LastOrDefault();
				if (daysCount != lastEntry.DaysCount || activitiesCount != lastEntry.ActivitiesInFirstDay)
				{
					daysProgression.Add((i + 1, daysCount, activitiesCount));
					output.WriteLine($"Chunk {i + 1}: Days={daysCount}, Activities in first day={activitiesCount}");
				}
			}

			Assert.NotEmpty(daysProgression);
			
			// We should see the days array build up
			var finalDaysCount = daysProgression[^1].DaysCount;
			Assert.True(finalDaysCount > 0, $"Expected days array to have items in {fileName}");
		}

		[Theory]
		[MemberData(nameof(DataStreamsHelper.TxtItineraries), MemberType = typeof(DataStreamsHelper))]
		public void ProcessChunk_TxtFile_ShouldProgressTitleField(string fileName, string filePath)
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = File.ReadAllLines(filePath);
			
			var titleProgression = new List<string?>();

			// Track title field progression
			for (int i = 0; i < chunks.Length; i++)
			{
				var currentModel = deserializer.ProcessChunk(chunks[i]);
				Assert.NotNull(currentModel);

				var currentTitle = currentModel.Title;
				if (titleProgression.Count == 0 || currentTitle != titleProgression[^1])
				{
					titleProgression.Add(currentTitle);
					output.WriteLine($"Chunk {i + 1}: Title = '{currentTitle}'");
				}
			}

			Assert.NotEmpty(titleProgression);
			
			// Final title should not be empty
			var finalTitle = titleProgression[^1];
			Assert.False(string.IsNullOrEmpty(finalTitle), $"Final title should not be empty in {fileName}");
		}

		[Theory]
		[MemberData(nameof(DataStreamsHelper.TxtItineraries), MemberType = typeof(DataStreamsHelper))]
		public void ProcessChunk_TxtFile_EachChunkProducesNonEmptyObject(string fileName, string filePath)
		{
			_ = fileName; // Used for test display name
			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = File.ReadAllLines(filePath);

			foreach (var chunk in chunks)
			{
				var currentItinerary = deserializer.ProcessChunk(chunk);
				
				// Each chunk should produce a non-null result
				Assert.NotNull(currentItinerary);
			}
		}

		[Theory]
		[MemberData(nameof(DataStreamsHelper.TxtItineraries), MemberType = typeof(DataStreamsHelper))]
		public void ProcessChunk_TxtFile_UpdateRate(string fileName, string filePath)
		{
			// This test ensures we get a constant stream of updated models.
			// At least 70% of chunks should produce a NEW serialized JSON output,
			// indicating that the model is continuously updating rather than stalling.
			
			// Note: Some chunks may not produce updates due to whitespace-only chunks,
			// partial property values, or unchanged content - 70% is a reasonable threshold.

			var deserializer = new StreamingJsonDeserializer<Itinerary>(DeserializationOptions);
			var chunks = File.ReadAllLines(filePath);

			string? previousJson = null;
			int totalChunks = 0;
			int updatedChunks = 0;
			var unchangedChunks = new List<(int chunkNumber, string chunk)>();

			foreach (var chunk in chunks)
			{
				totalChunks++;
				var currentModel = deserializer.ProcessChunk(chunk);
				Assert.NotNull(currentModel);

				var currentJson = JsonSerializer.Serialize(currentModel, DeserializationOptions);

				if (previousJson == null || currentJson != previousJson)
				{
					updatedChunks++;
				}
				else
				{
					// Log chunks that didn't produce an update
					unchangedChunks.Add((totalChunks, chunk));
				}

				previousJson = currentJson;
			}

			var updateRate = (double)updatedChunks / totalChunks * 100;
			output.WriteLine($"File: {fileName}");
			output.WriteLine($"Total chunks: {totalChunks}");
			output.WriteLine($"Updated chunks: {updatedChunks}");
			output.WriteLine($"Update rate: {updateRate:F2}%");

			if (unchangedChunks.Count > 0)
			{
				output.WriteLine($"\nChunks that resulted in same model ({unchangedChunks.Count}):");
				foreach (var (chunkNumber, chunk) in unchangedChunks)
				{
					var displayChunk = chunk.Length > 50 ? chunk.Substring(0, 50) + "..." : chunk;
					output.WriteLine($"  Chunk {chunkNumber}: '{displayChunk}'");
				}
			}

			Assert.True(updateRate >= 70.0,
				$"Expected at least 70% update rate for {fileName}, but got {updateRate:F2}%. " +
				$"Only {updatedChunks}/{totalChunks} chunks produced new serialized output. " +
				"This indicates the streaming deserializer is not continuously updating the model.");
		}
	}
}
