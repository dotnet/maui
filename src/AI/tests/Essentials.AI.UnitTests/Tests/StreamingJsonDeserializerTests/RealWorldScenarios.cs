using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class RealWorldScenarios
	{
		[Fact]
		public void ProcessChunk_LlmStreamingScenario_UpdatesInRealTime()
		{
			var deserializer = new StreamingJsonDeserializer<NestedModel>();

			// Simulate LLM token streaming where JSON grows token by token
			var llmChunks = new[]
			{
				@"{ ""text"": ""Hello",
				@" world"", ""meta"": { ""score"": 1",
				@" }",
				@" }"
			};

			NestedModel? model = null;
			foreach (var chunk in llmChunks)
			{
				model = deserializer.ProcessChunk(chunk);
				Assert.NotNull(model);
			}

			Assert.Equal("Hello world", model?.Text);
			Assert.NotNull(model?.Meta);
			Assert.Equal(1, model?.Meta.Score);
		}
	}
}
