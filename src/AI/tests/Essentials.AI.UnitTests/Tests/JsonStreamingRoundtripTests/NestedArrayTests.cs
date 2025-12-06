using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamingRoundtripTests
{
	/// <summary>
	/// Tests for nested arrays and complex structures.
	/// </summary>
	public class NestedArrayTests
	{
		[Fact]
		public void Roundtrip_NestedArrays_DeserializesCorrectly()
		{
			// Arrange
			var chunker = new JsonStreamChunker();
			var deserializer = new StreamingJsonDeserializer<ItineraryModel>();
			var lines = new[]
			{
				"""{"days": []}""",
				"""{"days": [{"activities": []}]}""",
				"""{"days": [{"activities": [{"description": "Hello"}]}]}""",
				"""{"days": [{"activities": [{"description": "Hello World", "title": "Activity 1"}]}]}"""
			};

			// Act
			var chunks = new List<string>();
			foreach (var line in lines)
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

			ItineraryModel? finalModel = null;
			foreach (var chunk in chunks)
				finalModel = deserializer.ProcessChunk(chunk);

			// Assert
			Assert.NotNull(finalModel);
			Assert.NotNull(finalModel.Days);
			Assert.Single(finalModel.Days);
			Assert.NotNull(finalModel.Days[0].Activities);
			Assert.Single(finalModel.Days[0].Activities!);
			Assert.Equal("Hello World", finalModel.Days[0].Activities![0].Description);
			Assert.Equal("Activity 1", finalModel.Days[0].Activities![0].Title);
		}
	}
}
