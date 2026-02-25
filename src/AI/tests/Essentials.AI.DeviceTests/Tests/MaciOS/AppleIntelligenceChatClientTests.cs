#if IOS || MACCATALYST
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
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

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientToolCallLoggingTests
{
	[Fact]
	public async Task GetResponseAsync_ToolCallsLoggedAtDebug()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Debug);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);

		var logs = logCollector.Entries;
		// Debug: "Invoking GetWeather." (before) and "GetWeather invocation completed. Duration: ..." (after)
		Assert.Contains(logs, l =>
			l.Message.Contains("Invoking GetWeather", StringComparison.Ordinal));
		Assert.Contains(logs, l =>
			l.Message.Contains("GetWeather invocation completed", StringComparison.Ordinal)
			&& l.Message.Contains("Duration:", StringComparison.Ordinal));

		// Debug should NOT contain arguments or result values
		Assert.DoesNotContain(logs, l =>
			l.Message.Contains("Seattle", StringComparison.Ordinal));
		Assert.DoesNotContain(logs, l =>
			l.Message.Contains("72°F", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetResponseAsync_ToolCallsLoggedAtTrace()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Trace);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);

		var logs = logCollector.Entries;
		// Trace: "Invoking GetWeather({arguments})." includes arguments
		Assert.Contains(logs, l =>
			l.Message.Contains("Invoking GetWeather(", StringComparison.Ordinal));
		// Trace: "GetWeather invocation completed. Duration: ... Result: ..." includes result
		Assert.Contains(logs, l =>
			l.Message.Contains("GetWeather invocation completed", StringComparison.Ordinal)
			&& l.Message.Contains("Result:", StringComparison.Ordinal)
			&& l.Message.Contains("72°F", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetResponseAsync_NoToolCallsNoLogging()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Trace);
		var client = new AppleIntelligenceChatClient(logCollector);

		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What is 2+2?")]);

		// No tool calls → no invocation logging
		Assert.DoesNotContain(logCollector.Entries, l =>
			l.Message.Contains("Invoking", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetStreamingResponseAsync_ToolCallsLoggedAtDebug()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Debug);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		await foreach (var _ in client.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options))
		{
		}

		var logs = logCollector.Entries;
		Assert.Contains(logs, l =>
			l.Message.Contains("Invoking GetWeather", StringComparison.Ordinal));
		Assert.Contains(logs, l =>
			l.Message.Contains("invocation completed", StringComparison.Ordinal)
			&& l.Message.Contains("Duration:", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetResponseAsync_ToolCallsNotLoggedAtInformation()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Information);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);

		// Tool call logging is Debug/Trace only — nothing at Information+
		Assert.Empty(logCollector.Entries);
	}

	[Fact]
	public async Task GetResponseAsync_MultipleToolsEachLoggedIndividually()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Debug);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var timeTool = AIFunctionFactory.Create(
			(string timezone) => $"3:00 PM in {timezone}",
			name: "GetTime",
			description: "Gets the current time in a timezone");

		var options = new ChatOptions { Tools = [weatherTool, timeTool] };
		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle and the time in EST?")], options);

		var logs = logCollector.Entries;
		// Both tools should have completed — native framework may invoke concurrently
		Assert.Contains(logs, l => l.Message.Contains("GetWeather invocation completed", StringComparison.Ordinal));
		Assert.Contains(logs, l => l.Message.Contains("GetTime invocation completed", StringComparison.Ordinal));
		// At least one Invoking log should be present
		Assert.Contains(logs, l => l.Message.Contains("Invoking Get", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetStreamingResponseAsync_ToolCallsLoggedAtTrace()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Trace);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		await foreach (var _ in client.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options))
		{
		}

		var logs = logCollector.Entries;
		// Trace includes arguments and results in streaming too
		Assert.Contains(logs, l =>
			l.Message.Contains("Invoking GetWeather(", StringComparison.Ordinal));
		Assert.Contains(logs, l =>
			l.Message.Contains("invocation completed", StringComparison.Ordinal)
			&& l.Message.Contains("Result:", StringComparison.Ordinal)
			&& l.Message.Contains("72°F", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetResponseAsync_InvokingLoggedBeforeCompleted()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Debug);
		var client = new AppleIntelligenceChatClient(logCollector);

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);

		var logs = logCollector.Entries;
		var invokingIndex = logs.FindIndex(l => l.Message.Contains("Invoking GetWeather", StringComparison.Ordinal));
		var completedIndex = logs.FindIndex(l => l.Message.Contains("GetWeather invocation completed", StringComparison.Ordinal));

		Assert.True(invokingIndex >= 0, "Should have 'Invoking' log entry");
		Assert.True(completedIndex >= 0, "Should have 'completed' log entry");
		Assert.True(invokingIndex < completedIndex, "Invoking should come before completed");
	}

	[Fact]
	public async Task GetResponseAsync_ToolFailureLoggedAtError()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Debug);
		var client = new AppleIntelligenceChatClient(logCollector);

		var failingTool = AIFunctionFactory.Create(
			string (string location) => throw new InvalidOperationException("API is down"),
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [failingTool] };
		// The native framework propagates tool errors as NSErrorException
		try
		{
			await client.GetResponseAsync(
				[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);
		}
		catch
		{
			// Expected — native framework may propagate tool errors
		}

		var logs = logCollector.Entries;
		// Should log the invocation attempt
		Assert.Contains(logs, l => l.Message.Contains("Invoking GetWeather", StringComparison.Ordinal));
		// Should log the failure at Error level
		Assert.Contains(logs, l =>
			l.Level == LogLevel.Error
			&& l.Message.Contains("GetWeather invocation failed", StringComparison.Ordinal));
	}

	[Fact]
	public async Task GetResponseAsync_NoLoggerFactory_DoesNotThrow()
	{
		// Parameterless constructor should work without crashing
		var client = new AppleIntelligenceChatClient();

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		var response = await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);

		// Should complete successfully without any logger
		Assert.NotNull(response);
		Assert.NotEmpty(response.Messages);
	}

	/// <summary>
	/// Simple log collector for device tests.
	/// </summary>
	private class DeviceTestLogCollector : ILoggerFactory, ILogger
	{
		private readonly LogLevel _minimumLevel;

		public DeviceTestLogCollector(LogLevel minimumLevel) => _minimumLevel = minimumLevel;

		public List<DeviceTestLogEntry> Entries { get; } = [];

		public ILogger CreateLogger(string categoryName) => this;
		public void AddProvider(ILoggerProvider provider) { }
		void IDisposable.Dispose() { }

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (IsEnabled(logLevel))
				Entries.Add(new DeviceTestLogEntry(logLevel, formatter(state, exception)));
		}
	}

	private record DeviceTestLogEntry(LogLevel Level, string Message);
}

#endif
