using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingResponseHandlerTests
{
	/// <summary>
	/// Tests for <see cref="StreamingResponseHandler.ProcessToolCall"/> and
	/// <see cref="StreamingResponseHandler.ProcessToolResult"/>.
	/// </summary>
	public class ToolTests
	{
		[Fact]
		public async Task ProcessToolCall_EmitsFunctionCallContent()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolCall("call-1", "GetWeather", "{\"location\":\"Boston\"}");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal(ChatRole.Assistant, updates[0].Role);
			var fc = Assert.Single(updates[0].Contents.OfType<FunctionCallContent>());
			Assert.Equal("call-1", fc.CallId);
			Assert.Equal("GetWeather", fc.Name);
			Assert.NotNull(fc.Arguments);
			Assert.Equal("Boston", fc.Arguments["location"]?.ToString());
		}

		[Fact]
		public async Task ProcessToolResult_EmitsWithToolRole()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolResult("call-1", "Sunny, 72°F");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Single(updates);
			Assert.Equal(ChatRole.Tool, updates[0].Role);
			var fr = Assert.Single(updates[0].Contents.OfType<FunctionResultContent>());
			Assert.Equal("call-1", fr.CallId);
			Assert.Equal("Sunny, 72°F", fr.Result?.ToString());
		}

		[Fact]
		public async Task ProcessToolCall_AfterContent_FlushesContentFirst()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessContent("Let me check");
			handler.ProcessToolCall("call-1", "GetWeather", "{\"location\":\"Boston\"}");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.True(updates.Count >= 2);

			var textIndex = updates.FindIndex(u => u.Contents.OfType<TextContent>().Any());
			var toolIndex = updates.FindIndex(u => u.Contents.OfType<FunctionCallContent>().Any());
			Assert.True(textIndex < toolIndex, "Text content should be emitted before tool call");
		}

		[Fact]
		public async Task ProcessToolCall_MalformedJson_Throws()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			var ex = Assert.Throws<System.Text.Json.JsonException>(() =>
				handler.ProcessToolCall("call-1", "GetWeather", "not valid json {"));

			handler.CompleteWithError(ex);

			await Assert.ThrowsAsync<System.Text.Json.JsonException>(async () => await ReadAll(handler));
		}

		[Fact]
		public async Task ContentAfterToolResult_StartsNewStream()
		{
			var handler = new StreamingResponseHandler(new PlainTextStreamChunker());

			handler.ProcessToolCall("call-1", "GetWeather", "{\"location\":\"Boston\"}");
			handler.ProcessToolResult("call-1", "Sunny");
			handler.ProcessContent("The weather is sunny");
			handler.Complete();

			var updates = await ReadAll(handler);

			Assert.Equal(3, updates.Count);
			Assert.True(updates[0].Contents.OfType<FunctionCallContent>().Any());
			Assert.True(updates[1].Contents.OfType<FunctionResultContent>().Any());
			Assert.Equal("The weather is sunny", updates[2].Contents.OfType<TextContent>().Single().Text);
		}
	}
}
