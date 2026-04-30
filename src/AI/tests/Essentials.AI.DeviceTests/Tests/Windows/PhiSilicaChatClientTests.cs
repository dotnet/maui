#if WINDOWS
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Single wrapper for all Phi Silica tests — handles both structured output and tool calling.
/// Pipeline: PhiSilicaToolsAndSchemaClient → PhiSilicaChatClient
/// </summary>
public class PhiSilicaWrappedClient : DelegatingChatClient
{
public PhiSilicaWrappedClient() : base(new PhiSilicaToolsAndSchemaClient(new PhiSilicaChatClient())) { }
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientCancellationTests : ChatClientCancellationTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientFunctionCallingTests : ChatClientFunctionCallingTestsBase<PhiSilicaWrappedClient>
{
	protected override IChatClient EnableFunctionCalling(PhiSilicaWrappedClient client)
	{
		return client.AsBuilder()
			.UseFunctionInvocation()
			.Build();
	}

	/// <summary>
	/// Skip: InformationalOnly is for native tool callers (Apple Intelligence) where the model
	/// invokes tools itself. Phi Silica uses structured output tool calling — FICC handles invocation.
	/// </summary>
	[Fact(Skip = "Phi Silica uses structured output tool calling. InformationalOnly applies only to native tool callers like Apple Intelligence.")]
	public override Task GetStreamingResponseAsync_InformationalOnlyFunctionCalls_NotInvokedByFICC()
		=> Task.CompletedTask;

	/// <summary>
	/// SLM Best Practice: For dependent tool chains, add a generic system message
	/// that teaches the model to check tool parameter requirements before calling.
	/// This is a reusable pattern — not specific to any particular tool.
	/// </summary>
	[Fact]
	public override async Task GetResponseAsync_ChainedFunctionCalls_TimeAndWeather()
	{
		int timeCallCount = 0;
		int weatherCallCount = 0;
		string? capturedDate = null;

		var timeTool = AIFunctionFactory.Create(
			() => { timeCallCount++; return "2025-12-02 12:00:00"; },
			name: "GetCurrentTime",
			description: "Gets the current date and time. No parameters needed.");

		var weatherTool = AIFunctionFactory.Create(
			(string date) =>
			{
				weatherCallCount++;
				capturedDate = date;
				return $"{{\"date\":\"{date}\",\"condition\":\"sunny\",\"temperature\":72,\"humidity\":45}}";
			},
			name: "GetWeather",
			description: "Gets the weather forecast for a specific date. Requires the date in YYYY-MM-DD format.");

		var client = EnableFunctionCalling(new PhiSilicaWrappedClient());
		var messages = new List<ChatMessage>
		{
			// SLM Best Practice: For tool chains, describe dependencies in the system message.
			// This tells the model which tool provides data another tool needs.
			new(ChatRole.System,
				"GetWeather requires a date parameter. If the user does not provide a specific date, " +
				"call GetCurrentTime first to get the current date."),
			new(ChatRole.User, "What's the weather like today?")
		};
		var options = new ChatOptions { Tools = [timeTool, weatherTool] };

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.True(timeCallCount > 0, "GetCurrentTime should have been called");
		Assert.True(weatherCallCount > 0, "GetWeather should have been called");
		Assert.NotNull(capturedDate);
		Assert.Contains("2025-12-02", capturedDate, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Streaming version with generic SLM dependency guidance.
	/// </summary>
	[Fact]
	public override async Task GetStreamingResponseAsync_ChainedFunctionCalls_TimeAndWeather()
	{
		int timeCallCount = 0;
		int weatherCallCount = 0;

		var timeTool = AIFunctionFactory.Create(
			() => { timeCallCount++; return "2025-12-02 12:00:00"; },
			name: "GetCurrentTime",
			description: "Gets the current date and time. No parameters needed.");

		var weatherTool = AIFunctionFactory.Create(
			(string date) =>
			{
				weatherCallCount++;
				return $"{{\"date\":\"{date}\",\"condition\":\"cloudy\",\"temperature\":68,\"humidity\":55}}";
			},
			name: "GetWeather",
			description: "Gets the weather forecast for a specific date. Requires the date in YYYY-MM-DD format.");

		var client = EnableFunctionCalling(new PhiSilicaWrappedClient());
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System,
				"GetWeather requires a date parameter. If the user does not provide a specific date, " +
				"call GetCurrentTime first to get the current date."),
			new(ChatRole.User, "What's the weather like today?")
		};
		var options = new ChatOptions { Tools = [timeTool, weatherTool] };

		var updates = new List<ChatResponseUpdate>();
		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			updates.Add(update);
		}

		Assert.True(updates.Count > 0, "Should receive streaming updates");
		Assert.True(timeCallCount > 0, "GetCurrentTime should have been called");
		Assert.True(weatherCallCount > 0, "GetWeather should have been called");
	}

	/// <summary>
	/// Chain: GetUserProfile(username) → GetOrderHistory(userId).
	/// Different domain from TimeAndWeather — tests that chaining works
	/// when the dependency is data-based (userId from profile result).
	/// </summary>
	[Fact]
	public async Task GetResponseAsync_ChainedFunctionCalls_ProfileToOrders()
	{
		int profileCallCount = 0;
		int ordersCallCount = 0;

		var profileTool = AIFunctionFactory.Create(
			(string username) =>
			{
				profileCallCount++;
				return "{\"userId\": \"U12345\", \"name\": \"John Doe\"}";
			},
			name: "GetUserProfile",
			description: "Looks up a user profile by username. Returns userId and name.");

		var ordersTool = AIFunctionFactory.Create(
			(string userId) =>
			{
				ordersCallCount++;
				return "[{\"orderId\": \"ORD-001\", \"item\": \"Widget\"}]";
			},
			name: "GetOrderHistory",
			description: "Gets order history for a user. Requires the userId.");

		var client = EnableFunctionCalling(new PhiSilicaWrappedClient());
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System,
				"GetOrderHistory requires a userId. Call GetUserProfile with the username to get the userId first."),
			new(ChatRole.User, "What are the recent orders for username 'johndoe'?")
		};
		var options = new ChatOptions { Tools = [profileTool, ordersTool] };

		var response = await client.GetResponseAsync(messages, options);
		Assert.NotNull(response);
		Assert.True(profileCallCount > 0, $"GetUserProfile should be called. Got: {profileCallCount}");
		Assert.True(ordersCallCount > 0, $"GetOrderHistory should be called. Got: {ordersCallCount}");
	}
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
public class PhiSilicaChatClientJsonSchemaTests : ChatClientJsonSchemaTestsBase<PhiSilicaWrappedClient>
{
	[Fact(Skip = "Phi Silica does not support JSON format without a schema — PhiSilicaToolsAndSchemaClient requires a schema to rewrite.")]
	public override Task GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
		=> base.GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();

	[Fact(Skip = "Phi Silica does not support JSON format without a schema — PhiSilicaToolsAndSchemaClient requires a schema to rewrite.")]
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
