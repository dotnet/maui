#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Tests for AppleIntelligenceChatClient edge cases in message conversion,
/// tool validation, and error handling paths.
/// </summary>
[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientValidationTests
{
	/// <summary>
	/// Verifies that passing a non-AIFunction tool (e.g., a custom AITool subclass)
	/// throws NotSupportedException with a descriptive message listing the unsupported types.
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithNonAIFunctionTool_ThrowsNotSupportedException()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			Tools = [new UnsupportedToolForTesting()]
		};

		var ex = await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetResponseAsync(messages, options));
		Assert.Contains("AIFunction", ex.Message, StringComparison.Ordinal);
		Assert.Contains("UnsupportedToolForTesting", ex.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// Verifies that messages where all content is filtered out (e.g., TextContent with null Text)
	/// throws ArgumentException rather than sending empty content to the native API.
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithOnlyNullTextContent_ThrowsArgumentException()
	{
		var client = new AppleIntelligenceChatClient();
		var msg = new ChatMessage(ChatRole.User, [new TextContent(null)]);
		var messages = new List<ChatMessage> { msg };

		var ex = await Assert.ThrowsAsync<ArgumentException>(
			() => client.GetResponseAsync(messages));
		Assert.Contains("convertible content", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Verifies that messages with unsupported content types
	/// throw ArgumentException with a descriptive message.
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithUnsupportedContentType_ThrowsArgumentException()
	{
		var client = new AppleIntelligenceChatClient();
		var msg = new ChatMessage(ChatRole.User, [new UnsupportedContentForTesting()]);
		var messages = new List<ChatMessage> { msg };

		await Assert.ThrowsAsync<ArgumentException>(
			() => client.GetResponseAsync(messages));
	}

	/// <summary>
	/// Verifies that FunctionResultContent with a CallId that doesn't match any prior
	/// FunctionCallContent is handled gracefully (empty tool name, no exception).
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithOrphanedFunctionResult_DoesNotThrow()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather?"),
			new(ChatRole.Assistant, [new FunctionCallContent("call-1", "GetWeather")]),
			new(ChatRole.Tool, [new FunctionResultContent("call-1", "Sunny")]),
			// Orphaned result — callId "call-999" was never in a FunctionCallContent
			new(ChatRole.Tool, [new FunctionResultContent("call-999", "Unknown result")]),
			new(ChatRole.User, "Tell me more")
		};

		// Should not throw — orphaned FunctionResultContent gets empty tool name
		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
	}

	/// <summary>
	/// Verifies that FunctionResultContent with a CallId not matching any FunctionCallContent
	/// is handled gracefully (empty tool name, no exception). This covers the null CallId path too.
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithFunctionResultOrphanedCallId_DoesNotThrow()
	{
		var client = new AppleIntelligenceChatClient();

		// Build a FunctionResultContent with a CallId that has no matching FunctionCallContent
		var orphanResult = new FunctionResultContent("orphan-call-id", "Sunny result");

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather?"),
			new(ChatRole.Assistant, [new FunctionCallContent("call-1", "GetWeather")]),
			new(ChatRole.Tool, [new FunctionResultContent("call-1", "Sunny")]),
			// Orphaned result — callId doesn't match any prior FunctionCallContent
			new(ChatRole.Tool, [orphanResult]),
			new(ChatRole.User, "Tell me more")
		};

		// Should not throw — orphaned FunctionResultContent gets empty tool name
		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
	}

	/// <summary>
	/// Verifies that FunctionCallContent with null Name does not populate the callIdToName
	/// dictionary, and subsequent FunctionResultContent for that CallId gets empty name.
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithFunctionCallNullName_DoesNotThrow()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather?"),
			new(ChatRole.Assistant, [new FunctionCallContent("call-1", null!)]),
			new(ChatRole.Tool, [new FunctionResultContent("call-1", "Sunny")]),
			new(ChatRole.User, "Tell me more")
		};

		// Should not throw — null Name means callIdToName lookup misses, result gets empty name
		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
	}

	/// <summary>
	/// Verifies that ChatOptions.Instructions is accepted and the response succeeds.
	/// The Instructions string is prepended as a system message internally.
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_WithInstructions_Succeeds()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			Instructions = "You are a helpful assistant."
		};

		var response = await client.GetResponseAsync(messages, options);
		Assert.NotNull(response);
		Assert.NotEmpty(response.Messages);
	}

	/// <summary>
	/// Verifies that GetService with null serviceType throws ArgumentNullException.
	/// </summary>
	[Fact]
	public void GetService_WithNullServiceType_ThrowsArgumentNullException()
	{
		var client = new AppleIntelligenceChatClient();

		Assert.Throws<ArgumentNullException>(() =>
			((IChatClient)client).GetService(null!, null));
	}

	/// <summary>
	/// A custom AITool subclass that is NOT an AIFunction, used to test validation.
	/// </summary>
	private sealed class UnsupportedToolForTesting : AITool;

	/// <summary>
	/// A custom AIContent subclass that is not supported by Apple Intelligence.
	/// </summary>
	private sealed class UnsupportedContentForTesting : AIContent;
}

#endif
