using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Tests for <see cref="StreamingResponseHandler"/> — validates channel, chunker, and
/// error handling logic without requiring the native Apple Intelligence layer.
/// </summary>
public class StreamingResponseHandlerTests
{
	[Fact]
	public async Task ProcessUpdate_ContentUpdate_ProducesTextDelta()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "Hello"));
		handler.Complete();

		var updates = await ReadAll(handler);

		Assert.Single(updates);
		Assert.Equal(ChatRole.Assistant, updates[0].Role);
		var text = Assert.Single(updates[0].Contents.OfType<TextContent>());
		Assert.Equal("Hello", text.Text);
	}

	[Fact]
	public async Task ProcessUpdate_ContentUpdates_ProducePlainTextDeltas()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "Hello"));
		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "Hello world"));
		handler.Complete();

		var updates = await ReadAll(handler);

		Assert.Equal(2, updates.Count);
		Assert.Equal("Hello", updates[0].Contents.OfType<TextContent>().Single().Text);
		Assert.Equal(" world", updates[1].Contents.OfType<TextContent>().Single().Text);
	}

	[Fact]
	public async Task ProcessUpdate_ToolCall_EmitsFunctionCallContent()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(
			ResponseUpdateTypeNative.ToolCall,
			ToolCallId: "call-1",
			ToolCallName: "GetWeather",
			ToolCallArguments: "{\"location\":\"Boston\"}"));
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
	public async Task ProcessUpdate_ToolResult_EmitsWithToolRole()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(
			ResponseUpdateTypeNative.ToolResult,
			ToolCallId: "call-1",
			ToolCallResult: "Sunny, 72°F"));
		handler.Complete();

		var updates = await ReadAll(handler);

		Assert.Single(updates);
		Assert.Equal(ChatRole.Tool, updates[0].Role);
		var fr = Assert.Single(updates[0].Contents.OfType<FunctionResultContent>());
		Assert.Equal("call-1", fr.CallId);
		Assert.Equal("Sunny, 72°F", fr.Result?.ToString());
	}

	[Fact]
	public async Task ProcessUpdate_ToolCallAfterContent_FlushesContentFirst()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		// Stream some text, then get a tool call
		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "Let me check"));
		handler.ProcessUpdate(new StreamUpdate(
			ResponseUpdateTypeNative.ToolCall,
			ToolCallId: "call-1",
			ToolCallName: "GetWeather",
			ToolCallArguments: "{\"location\":\"Boston\"}"));
		handler.Complete();

		var updates = await ReadAll(handler);

		// First update should be text, second should be tool call
		Assert.True(updates.Count >= 2);
		Assert.Contains(updates, u => u.Contents.OfType<TextContent>().Any());
		Assert.Contains(updates, u => u.Contents.OfType<FunctionCallContent>().Any());

		// Text should come before tool call
		var textIndex = updates.FindIndex(u => u.Contents.OfType<TextContent>().Any());
		var toolIndex = updates.FindIndex(u => u.Contents.OfType<FunctionCallContent>().Any());
		Assert.True(textIndex < toolIndex, "Text content should be emitted before tool call");
	}

	[Fact]
	public async Task ProcessUpdate_MalformedToolCallJson_SurfacesAsError()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		// This should throw JsonException in ProcessUpdate
		var ex = Assert.Throws<System.Text.Json.JsonException>(() =>
			handler.ProcessUpdate(new StreamUpdate(
				ResponseUpdateTypeNative.ToolCall,
				ToolCallId: "call-1",
				ToolCallName: "GetWeather",
				ToolCallArguments: "not valid json {")));

		// In real usage, the caller catches this and calls CompleteWithError
		handler.CompleteWithError(ex);

		await Assert.ThrowsAsync<System.Text.Json.JsonException>(async () => await ReadAll(handler));
	}

	[Fact]
	public async Task CompleteWithError_SurfacesExceptionToReader()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "Hello"));
		handler.CompleteWithError(new InvalidOperationException("test error"));

		// Should throw when reading
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await ReadAll(handler));
	}

	[Fact]
	public void DoubleComplete_DoesNotThrow()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.CompleteWithError(new InvalidOperationException("first error"));
		handler.Complete(); // Should not throw
		handler.CompleteWithError(new InvalidOperationException("second error")); // Should not throw
	}

	[Fact]
	public async Task Complete_FlushesRemainingContent()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "Hello"));
		handler.Complete();

		var updates = await ReadAll(handler);

		Assert.Single(updates);
		Assert.Equal("Hello", updates[0].Contents.OfType<TextContent>().Single().Text);
	}

	[Fact]
	public async Task ProcessUpdate_ContentAfterToolResult_StartsNewStream()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		// Tool call
		handler.ProcessUpdate(new StreamUpdate(
			ResponseUpdateTypeNative.ToolCall,
			ToolCallId: "call-1",
			ToolCallName: "GetWeather",
			ToolCallArguments: "{\"location\":\"Boston\"}"));

		// Tool result
		handler.ProcessUpdate(new StreamUpdate(
			ResponseUpdateTypeNative.ToolResult,
			ToolCallId: "call-1",
			ToolCallResult: "Sunny"));

		// Text after tool (fresh stream after reset)
		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: "The weather is sunny"));
		handler.Complete();

		var updates = await ReadAll(handler);

		// Should have: tool call, tool result, text content
		Assert.Equal(3, updates.Count);
		Assert.True(updates[0].Contents.OfType<FunctionCallContent>().Any());
		Assert.True(updates[1].Contents.OfType<FunctionResultContent>().Any());
		Assert.Equal("The weather is sunny", updates[2].Contents.OfType<TextContent>().Single().Text);
	}

	[Fact]
	public async Task ProcessUpdate_EmptyTextUpdate_ProducesNoOutput()
	{
		var handler = new StreamingResponseHandler(useJsonChunker: false);

		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content, Text: ""));
		handler.ProcessUpdate(new StreamUpdate(ResponseUpdateTypeNative.Content));
		handler.Complete();

		var updates = await ReadAll(handler);
		Assert.Empty(updates);
	}

	private static async Task<List<ChatResponseUpdate>> ReadAll(StreamingResponseHandler handler)
	{
		var updates = new List<ChatResponseUpdate>();
		await foreach (var update in handler.ReadAllAsync(CancellationToken.None))
		{
			updates.Add(update);
		}
		return updates;
	}
}
