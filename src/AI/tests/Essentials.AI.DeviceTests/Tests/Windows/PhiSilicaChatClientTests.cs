#if WINDOWS
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Wraps PhiSilicaChatClient with PromptBasedSchemaClient so that JSON schema
/// requests are converted to prompt instructions (Phi Silica has no native
/// structured output support).
/// </summary>
public class PhiSilicaSchemaClient : DelegatingChatClient
{
	public PhiSilicaSchemaClient() : base(new PromptBasedSchemaClient(new PhiSilicaChatClient())) { }
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientCancellationTests : ChatClientCancellationTestsBase<PhiSilicaChatClient>
{
}

/// <summary>
/// Wraps PhiSilicaChatClient with PromptBasedToolCallingClient so the model can
/// do tool calling via prompt engineering. The test base class wraps with
/// FunctionInvokingChatClient via EnableFunctionCalling().
/// </summary>
public class PhiSilicaToolCallingClient : DelegatingChatClient
{
	public PhiSilicaToolCallingClient() : base(new PromptBasedToolCallingClient(new PhiSilicaChatClient())) { }
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientFunctionCallingTests : ChatClientFunctionCallingTestsBase<PhiSilicaToolCallingClient>
{
	protected override IChatClient EnableFunctionCalling(PhiSilicaToolCallingClient client)
	{
		return client.AsBuilder()
			.UseFunctionInvocation()
			.Build();
	}

	/// <summary>
	/// Skip: InformationalOnly is for native tool callers (Apple Intelligence) where the model
	/// invokes tools itself. Phi Silica uses prompt-based tool calling — FICC handles invocation,
	/// so InformationalOnly is never set by our client.
	/// </summary>
	[Fact(Skip = "Phi Silica uses prompt-based tool calling. InformationalOnly applies only to native tool callers like Apple Intelligence.")]
	public override Task GetStreamingResponseAsync_InformationalOnlyFunctionCalls_NotInvokedByFICC()
		=> Task.CompletedTask;
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientGetServiceTests : ChatClientGetServiceTestsBase<PhiSilicaChatClient>
{
	protected override string ExpectedProviderName => "windows";
	protected override string ExpectedDefaultModelId => "phi-silica";
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientInstantiationTests : ChatClientInstantiationTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientMessagesTests : ChatClientMessagesTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientOptionsTests : ChatClientOptionsTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientResponseTests : ChatClientResponseTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientStreamingTests : ChatClientStreamingTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientJsonSchemaTests : ChatClientJsonSchemaTestsBase<PhiSilicaSchemaClient>
{
	[Fact(Skip = "Phi Silica does not support JSON format without a schema — PromptBasedSchemaClient requires a schema to rewrite.")]
	public override Task GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
		=> base.GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();

	[Fact(Skip = "Phi Silica does not support JSON format without a schema — PromptBasedSchemaClient requires a schema to rewrite.")]
	public override Task GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
		=> base.GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientValidationTests
{
	[Fact]
	public async Task GetResponseAsync_WithNonAIFunctionTool_ThrowsNotSupportedException()
	{
		var client = new PhiSilicaChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Hello")
		};
		var options = new ChatOptions
		{
			Tools = [new UnsupportedToolForTesting()]
		};

		await Assert.ThrowsAsync<NotSupportedException>(
			() => client.GetResponseAsync(messages, options));
	}

	[Fact]
	public async Task GetResponseAsync_WithOnlyNullTextContent_DoesNotThrow()
	{
		var client = new PhiSilicaChatClient();
		var msg = new ChatMessage(ChatRole.User, [new TextContent(null)]);
		var messages = new List<ChatMessage> { msg };

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
	}

	[Fact]
	public async Task GetResponseAsync_WithUnsupportedContentType_ThrowsArgumentException()
	{
		var client = new PhiSilicaChatClient();
		var msg = new ChatMessage(ChatRole.User, [new UnsupportedContentForTesting()]);
		var messages = new List<ChatMessage> { msg };

		await Assert.ThrowsAsync<ArgumentException>(
			() => client.GetResponseAsync(messages));
	}

	[Fact]
	public async Task GetResponseAsync_WithOrphanedFunctionResult_DoesNotThrow()
	{
		var client = new PhiSilicaChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather?"),
			new(ChatRole.Assistant, [new FunctionCallContent("call-1", "GetWeather")]),
			new(ChatRole.Tool, [new FunctionResultContent("call-1", "Sunny")]),
			new(ChatRole.Tool, [new FunctionResultContent("call-999", "Unknown result")]),
			new(ChatRole.User, "Tell me more")
		};

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
	}

	[Fact]
	public async Task GetResponseAsync_WithFunctionCallEmptyName_DoesNotThrow()
	{
		var client = new PhiSilicaChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather?"),
			new(ChatRole.Assistant, [new FunctionCallContent("call-1", "")]),
			new(ChatRole.Tool, [new FunctionResultContent("call-1", "Sunny")]),
			new(ChatRole.User, "Tell me more")
		};

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
	}

	[Fact]
	public async Task GetResponseAsync_WithInstructions_Succeeds()
	{
		var client = new PhiSilicaChatClient();
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

	[Fact]
	public void GetService_WithNullServiceType_ThrowsArgumentNullException()
	{
		var client = new PhiSilicaChatClient();

		Assert.Throws<ArgumentNullException>(() =>
			((IChatClient)client).GetService(null!, null));
	}

	private sealed class UnsupportedToolForTesting : AITool;

	private sealed class UnsupportedContentForTesting : AIContent;
}

#endif
