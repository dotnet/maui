using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class ErrorHandling
	{
		[Fact]
		public void ProcessChunk_MalformedJson_ReturnsLastGoodModel()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// First, establish a good model
			var result1 = deserializer.ProcessChunk(@"{""text"": ""Valid"", ""score"": 10}");
			Assert.NotNull(result1);
			Assert.Equal("Valid", result1.Text);
			Assert.Equal(10, result1.Score);

			// Now send malformed chunk (won't parse)
			var result2 = deserializer.ProcessChunk(@"GARBAGE{{{");

			// Should return the last good model
			Assert.NotNull(result2);
			Assert.Equal("Valid", result2.Text);
			Assert.Equal(10, result2.Score);
		}
	}
}
