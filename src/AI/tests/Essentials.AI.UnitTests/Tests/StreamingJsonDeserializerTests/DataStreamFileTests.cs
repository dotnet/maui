using System.Text.Json;
using Maui.Controls.Sample.Services;
using Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class DataStreamFileTests
	{
		[Theory]
		[InlineData("emotional-response-1.txt", 90.0, 0.0)]
		[InlineData("emotional-response-2.txt", 9.5, 0.0)]
		[InlineData("emotional-response-3.txt", 0.0, 0.0)]
		public void ProcessChunk_EmotionalResponses_DeserializeCorrectly(string fileName, double expectedHappiness, double expectedAnger)
		{
			var deserializer = new StreamingJsonDeserializer<EmotionalResponse>();
			var chunks = DataStreamsHelper.GetFileLines(fileName);

			EmotionalResponse? finalResponse = null;
			foreach (var chunk in chunks)
			{
				finalResponse = deserializer.ProcessChunk(chunk);
			}

			Assert.NotNull(finalResponse);
			Assert.Equal(expectedHappiness, finalResponse.Happiness);
			Assert.Equal(expectedAnger, finalResponse.Anger);
			Assert.NotNull(finalResponse.Reply);
			Assert.Contains("Hello", finalResponse.Reply, StringComparison.OrdinalIgnoreCase);

			// Deep comparison with direct deserialization
			var fullJson = string.Concat(chunks);
			var directDeserialized = JsonSerializer.Deserialize<EmotionalResponse>(fullJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
			});
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, finalResponse, strict: true);
		}

		[Fact]
		public void ProcessChunk_NumbersFirst_DeserializesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<EmotionalResponse>();
			var chunks = DataStreamsHelper.GetFileLines("numbers-first.txt");

			EmotionalResponse? finalResponse = null;
			foreach (var chunk in chunks)
			{
				finalResponse = deserializer.ProcessChunk(chunk);
			}

			Assert.NotNull(finalResponse);
			Assert.Equal(5.0, finalResponse.Happiness);
			Assert.Equal(0.0, finalResponse.Anger);
			Assert.NotNull(finalResponse.Reply);

			// Deep comparison with direct deserialization
			var fullJson = string.Concat(chunks);
			var directDeserialized = JsonSerializer.Deserialize<EmotionalResponse>(fullJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
			});
			
			Assert.NotNull(directDeserialized);
			Assert.Equivalent(directDeserialized, finalResponse, strict: true);
		}
	}
}
