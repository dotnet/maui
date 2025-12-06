using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.Models;
using Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class ItineraryDomainTests
	{
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
		[InlineData("mount-fuji-itinerary-1.txt")]
		[InlineData("mount-fuji-itinerary-2.txt")]
		public void ProcessChunk_RealItineraryDataStream_DeserializesProgressively(string fileName)
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamLoader.LoadStream(fileName);

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
			var chunks = DataStreamLoader.LoadStream("mount-fuji-itinerary-1.txt");

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
	}
}
