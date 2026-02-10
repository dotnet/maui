using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class PartialAndIncompleteJson
	{
		[Fact]
		public void ProcessChunk_PartialString_HandlesGracefully()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			// String value is cut in the middle
			var result1 = deserializer.ProcessChunk(@"{""text"": ""Hel");
			// Should return default model since JSON is too incomplete
			Assert.NotNull(result1);

			// Complete the string
			var result2 = deserializer.ProcessChunk(@"lo"", ""score"": 99}");
			Assert.NotNull(result2);
			Assert.Equal("Hello", result2.Text);
			Assert.Equal(99, result2.Score);
		}

		[Fact]
		public void ProcessChunk_VerySmallChunks_BuildsModelGradually()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>(DeserializationOptions);

			// Simulate character-by-character streaming
			var chunks = new[] { "{", "\"", "t", "e", "x", "t", "\"", ":", "\"", "H", "i", "\"", ",", "\"", "s", "c", "o", "r", "e", "\"", ":", "5", "}" };

			SimpleModel? lastResult = null;
			foreach (var chunk in chunks)
			{
				lastResult = deserializer.ProcessChunk(chunk);
			}

			Assert.NotNull(lastResult);
			Assert.Equal("Hi", lastResult.Text);
			Assert.Equal(5, lastResult.Score);
		}
	}
}
