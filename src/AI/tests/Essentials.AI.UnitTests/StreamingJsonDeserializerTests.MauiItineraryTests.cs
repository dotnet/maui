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
	public class MauiItineraryTests
	{
		private readonly ITestOutputHelper _output;

		public MauiItineraryTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void ProcessChunk_MauiItinerary1_DeserializesAllChunksCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamLoader.LoadStream("maui-itinerary-1.txt");

			Itinerary? lastItinerary = null;
			int successfulParseCount = 0;
			int totalChunks = 0;

			_output.WriteLine($"Processing {chunks.Length} chunks from maui-itinerary-1.txt");

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
						_output.WriteLine($"Chunk {totalChunks}: Successfully parsed. Title: {result.Title}, Days: {result.Days?.Count ?? 0}");
					}
				}
			}

			_output.WriteLine($"Total chunks: {totalChunks}");
			_output.WriteLine($"Successful parses: {successfulParseCount}");
			_output.WriteLine($"Success rate: {(double)successfulParseCount / totalChunks:P2}");

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
			var chunks = DataStreamLoader.LoadStream("maui-itinerary-1.txt");

			int parsedCount = 0;
			int totalChunks = 0;
			
			_output.WriteLine("Testing first 50 chunks to debug reconstruction logic:");

			foreach (var chunk in chunks.Take(50))
			{
				totalChunks++;
				var result = deserializer.ProcessChunk(chunk);
				
				if (result != null && !string.IsNullOrEmpty(result.Title))
				{
					parsedCount++;
					
					// Show partial JSON buffer
					var partialJson = deserializer.PartialJson;
					_output.WriteLine($"\n--- Chunk {totalChunks} (Parse #{parsedCount}) ---");
					_output.WriteLine($"Added chunk: '{chunk}'");
					_output.WriteLine($"Buffer length: {partialJson.Length} chars");
					_output.WriteLine($"Last 50 chars of buffer: ...{(partialJson.Length > 50 ? partialJson.Substring(partialJson.Length - 50) : partialJson)}");
					_output.WriteLine($"Title: {result.Title}");
					_output.WriteLine($"Days count: {result.Days?.Count ?? 0}");
				}
			}

			_output.WriteLine($"\nTotal processed: {totalChunks} chunks");
			_output.WriteLine($"Successful parses: {parsedCount}");
			_output.WriteLine($"Success rate: {(double)parsedCount / totalChunks:P2}");
		}

		private static JsonSerializerOptions CreateItineraryDeserializationOptions()
		{
			return new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				Converters = { new JsonStringEnumConverter() }
			};
		}
	}
}
