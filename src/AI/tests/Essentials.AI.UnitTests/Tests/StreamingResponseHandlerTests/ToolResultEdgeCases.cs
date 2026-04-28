using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for edge cases in tool result processing (empty, null, whitespace).
	/// </summary>
	public class ToolResultEdgeCaseTests
	{
		[Fact]
		public async Task ProcessToolResult_EmptyString_EmitsWithEmptyResult()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolResult("call-1", string.Empty);
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal(ChatRole.Tool, updates[0].Role);
			var fr = Assert.Single(updates[0].Contents.OfType<FunctionResultContent>());
			Assert.Equal("call-1", fr.CallId);
			Assert.Equal(string.Empty, fr.Result?.ToString());
		}

		[Fact]
		public async Task ProcessToolResult_NullResult_EmitsWithNullResult()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolResult("call-1", null);
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal(ChatRole.Tool, updates[0].Role);

			// The null-forgiving operator in ProcessToolResult converts null to null!
			// FunctionResultContent should still be created
			var fr = Assert.Single(updates[0].Contents.OfType<FunctionResultContent>());
			Assert.Equal("call-1", fr.CallId);
		}

		[Fact]
		public async Task ProcessToolResult_WhitespaceResult_EmitsWithWhitespace()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolResult("call-1", "   ");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			var fr = Assert.Single(updates[0].Contents.OfType<FunctionResultContent>());
			Assert.Equal("   ", fr.Result?.ToString());
		}

		[Fact]
		public async Task ContentAfterEmptyToolResult_ContinuesNormally()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolCall("call-1", "GetWeather", null);
			handler.ProcessToolResult("call-1", string.Empty);
			handler.ProcessContent("No weather data available");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Equal(3, updates.Count);
			Assert.True(updates[0].Contents.OfType<FunctionCallContent>().Any());
			Assert.True(updates[1].Contents.OfType<FunctionResultContent>().Any());
			Assert.Equal("No weather data available", updates[2].Contents.OfType<TextContent>().Single().Text);
		}

		[Fact]
		public async Task MultipleToolResults_MixedEmptyAndPopulated_AllEmitted()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolCall("call-1", "GetWeather", null);
			handler.ProcessToolResult("call-1", "Sunny");
			handler.ProcessToolCall("call-2", "GetTime", null);
			handler.ProcessToolResult("call-2", string.Empty);
			handler.Complete();

			var updates = await ReadAll(handler);

			var toolResults = updates.Where(u => u.Role == ChatRole.Tool).ToList();
			Assert.Equal(2, toolResults.Count);

			var fr1 = toolResults[0].Contents.OfType<FunctionResultContent>().Single();
			Assert.Equal("call-1", fr1.CallId);
			Assert.Equal("Sunny", fr1.Result?.ToString());

			var fr2 = toolResults[1].Contents.OfType<FunctionResultContent>().Single();
			Assert.Equal("call-2", fr2.CallId);
			Assert.Equal(string.Empty, fr2.Result?.ToString());
		}
	}
}
