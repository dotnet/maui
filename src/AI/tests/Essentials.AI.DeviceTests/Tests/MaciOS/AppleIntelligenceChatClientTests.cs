#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientCancellationTests : ChatClientCancellationTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientFunctionCallingTestsBase : ChatClientFunctionCallingTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientGetServiceTests : ChatClientGetServiceTestsBase<AppleIntelligenceChatClient>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "apple-intelligence";
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientInstantiationTests : ChatClientInstantiationTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientMessagesTests : ChatClientMessagesTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientOptionsTests : ChatClientOptionsTestsBase<AppleIntelligenceChatClient>
{
	/// <summary>
	/// Apple Intelligence requires a JSON schema for structured responses.
	/// Unlike the base test, this expects an InvalidOperationException when using ChatResponseFormat.Json without a schema.
	/// </summary>
	[Fact]
	public override async Task GetResponseAsync_WithResponseFormat_AcceptsJsonFormat()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => client.GetResponseAsync(messages, options));

		Assert.Contains("JSON schema", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Apple Intelligence requires a JSON schema for structured responses.
	/// Unlike the base test, this expects an InvalidOperationException when using ChatResponseFormat.Json without a schema.
	/// </summary>
	[Fact]
	public override async Task GetStreamingResponseAsync_WithResponseFormat_AcceptsJsonFormat()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, options))
			{
				// Should not reach here
			}
		});

		Assert.Contains("JSON schema", exception.Message, StringComparison.OrdinalIgnoreCase);
	}
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientResponseTests : ChatClientResponseTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientStreamingTests : ChatClientStreamingTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientJsonSchemaTests : ChatClientJsonSchemaTestsBase<AppleIntelligenceChatClient>
{
	[Fact(Skip = "Apple Intelligence requires a JSON schema for structured responses, so this test is not applicable.")]
	public override Task GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
	{
		return base.GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();
	}

	[Fact(Skip = "Apple Intelligence requires a JSON schema for structured responses, so this test is not applicable.")]
	public override Task GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
	{
		return base.GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();
	}

	[Fact]
	public async Task GetResponseAsync_WithJsonFormatWithoutSchema_ThrowsInvalidOperationException()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => client.GetResponseAsync(messages, options));
		
		Assert.Contains("JSON schema", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithJsonFormatWithoutSchema_ThrowsInvalidOperationException()
	{
		var client = new AppleIntelligenceChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(messages, options))
			{
				// Should not reach here
			}
		});
		
		Assert.Contains("JSON schema", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

}

#endif
