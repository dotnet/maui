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
	// ====================================================================
	// Single tool, Debug level
	// Expected: exactly 2 entries
	//   [0] Debug: "Invoking GetWeather."
	//   [1] Debug: "GetWeather invocation completed. Duration: {timespan}"
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_SingleTool_Debug_ProducesExactly2Entries()
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
		Assert.Equal(2, logs.Count);

		// Entry 0: "Invoking GetWeather."
		Assert.Equal(LogLevel.Debug, logs[0].Level);
		Assert.Equal("Invoking GetWeather.", logs[0].Message);

		// Entry 1: "GetWeather invocation completed. Duration: ..."
		Assert.Equal(LogLevel.Debug, logs[1].Level);
		Assert.StartsWith("GetWeather invocation completed. Duration: ", logs[1].Message, StringComparison.Ordinal);

		// Debug must NOT leak arguments or results
		var allText = string.Join("\n", logs.Select(l => l.Message));
		Assert.DoesNotContain("Seattle", allText, StringComparison.Ordinal);
		Assert.DoesNotContain("72°F", allText, StringComparison.Ordinal);
	}

	// ====================================================================
	// Single tool, Trace level
	// Expected: exactly 2 entries
	//   [0] Trace: "Invoking GetWeather({"location": "Seattle"})."
	//   [1] Trace: "GetWeather invocation completed. Duration: {ts}. Result: \"Clear skies, 72°F in Seattle\""
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_SingleTool_Trace_ProducesExactly2EntriesWithSensitiveData()
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
		Assert.Equal(2, logs.Count);

		// Entry 0: Trace with arguments in parentheses
		Assert.Equal(LogLevel.Trace, logs[0].Level);
		Assert.StartsWith("Invoking GetWeather(", logs[0].Message, StringComparison.Ordinal);
		Assert.EndsWith(").", logs[0].Message, StringComparison.Ordinal);
		Assert.Contains("Seattle", logs[0].Message, StringComparison.Ordinal);

		// Entry 1: Trace with duration AND result
		Assert.Equal(LogLevel.Trace, logs[1].Level);
		Assert.StartsWith("GetWeather invocation completed. Duration: ", logs[1].Message, StringComparison.Ordinal);
		Assert.Contains(". Result: ", logs[1].Message, StringComparison.Ordinal);
		Assert.Contains("72°F", logs[1].Message, StringComparison.Ordinal);
	}

	// ====================================================================
	// No tools — must produce zero log entries even at Trace
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_NoTools_ProducesZeroEntries()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Trace);
		var client = new AppleIntelligenceChatClient(logCollector);

		await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What is 2+2?")]);

		Assert.Empty(logCollector.Entries);
	}

	// ====================================================================
	// Information level — tool calls happen but produce zero log entries
	// (our logging is Debug/Trace only)
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_InformationLevel_ProducesZeroEntries()
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

		Assert.Empty(logCollector.Entries);
	}

	// ====================================================================
	// Streaming, Debug — same 2 entries as non-streaming
	// ====================================================================
	[Fact]
	public async Task GetStreamingResponseAsync_SingleTool_Debug_ProducesExactly2Entries()
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
		{ }

		var logs = logCollector.Entries;
		Assert.Equal(2, logs.Count);

		Assert.Equal(LogLevel.Debug, logs[0].Level);
		Assert.Equal("Invoking GetWeather.", logs[0].Message);

		Assert.Equal(LogLevel.Debug, logs[1].Level);
		Assert.StartsWith("GetWeather invocation completed. Duration: ", logs[1].Message, StringComparison.Ordinal);
	}

	// ====================================================================
	// Streaming, Trace — same 2 entries as non-streaming, with sensitive data
	// ====================================================================
	[Fact]
	public async Task GetStreamingResponseAsync_SingleTool_Trace_ProducesExactly2EntriesWithSensitiveData()
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
		{ }

		var logs = logCollector.Entries;
		Assert.Equal(2, logs.Count);

		Assert.Equal(LogLevel.Trace, logs[0].Level);
		Assert.StartsWith("Invoking GetWeather(", logs[0].Message, StringComparison.Ordinal);
		Assert.Contains("Seattle", logs[0].Message, StringComparison.Ordinal);

		Assert.Equal(LogLevel.Trace, logs[1].Level);
		Assert.Contains(". Result: ", logs[1].Message, StringComparison.Ordinal);
		Assert.Contains("72°F", logs[1].Message, StringComparison.Ordinal);
	}

	// ====================================================================
	// Ordering: for a single tool, "Invoking" must come before "completed"
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_InvokingIsLoggedBeforeCompleted()
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
		Assert.Equal(2, logs.Count);
		Assert.StartsWith("Invoking GetWeather", logs[0].Message, StringComparison.Ordinal);
		Assert.StartsWith("GetWeather invocation completed", logs[1].Message, StringComparison.Ordinal);
	}

	// ====================================================================
	// Multiple tools — native framework invokes concurrently.
	// Exactly 4 entries: Invoking + completed for each tool.
	// Order across tools is non-deterministic, but each tool's
	// "Invoking" must precede its "completed".
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_MultipleTools_ProducesExactly4Entries()
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
		var messages = logs.Select(l => l.Message).ToList();

		// Exactly 4 entries: 2 per tool (Invoking + completed)
		Assert.Equal(4, logs.Count);

		// All entries at Debug level
		Assert.All(logs, l => Assert.Equal(LogLevel.Debug, l.Level));

		// Exactly 1 "Invoking GetWeather." and 1 "GetWeather invocation completed..."
		Assert.Single(messages, m => m == "Invoking GetWeather.");
		Assert.Single(messages, m => m.StartsWith("GetWeather invocation completed. Duration: ", StringComparison.Ordinal));

		// Exactly 1 "Invoking GetTime." and 1 "GetTime invocation completed..."
		Assert.Single(messages, m => m == "Invoking GetTime.");
		Assert.Single(messages, m => m.StartsWith("GetTime invocation completed. Duration: ", StringComparison.Ordinal));

		// Each tool's "Invoking" precedes its "completed" (ordering within each tool)
		var weatherInvoking = messages.FindIndex(m => m == "Invoking GetWeather.");
		var weatherCompleted = messages.FindIndex(m => m.StartsWith("GetWeather invocation completed", StringComparison.Ordinal));
		Assert.True(weatherInvoking < weatherCompleted, "GetWeather: Invoking should come before completed");

		var timeInvoking = messages.FindIndex(m => m == "Invoking GetTime.");
		var timeCompleted = messages.FindIndex(m => m.StartsWith("GetTime invocation completed", StringComparison.Ordinal));
		Assert.True(timeInvoking < timeCompleted, "GetTime: Invoking should come before completed");
	}

	// ====================================================================
	// Tool failure — exactly 2 entries: Invoking (Debug) + failed (Error)
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_ToolFailure_Produces2EntriesWithErrorLevel()
	{
		var logCollector = new DeviceTestLogCollector(LogLevel.Debug);
		var client = new AppleIntelligenceChatClient(logCollector);

		var failingTool = AIFunctionFactory.Create(
			string (string location) => throw new InvalidOperationException("API is down"),
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [failingTool] };
		// Native framework propagates tool errors as NSErrorException
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
		Assert.Equal(2, logs.Count);

		// Entry 0: Debug-level "Invoking GetWeather."
		Assert.Equal(LogLevel.Debug, logs[0].Level);
		Assert.Equal("Invoking GetWeather.", logs[0].Message);

		// Entry 1: Error-level "GetWeather invocation failed."
		Assert.Equal(LogLevel.Error, logs[1].Level);
		Assert.Equal("GetWeather invocation failed.", logs[1].Message);
	}

	// ====================================================================
	// No logger factory — tool invocation works without crashing
	// ====================================================================
	[Fact]
	public async Task GetResponseAsync_NoLoggerFactory_CompletesSuccessfully()
	{
		var client = new AppleIntelligenceChatClient();

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var options = new ChatOptions { Tools = [weatherTool] };
		var response = await client.GetResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options);

		Assert.NotNull(response);
		Assert.NotEmpty(response.Messages);
	}

	/// <summary>
	/// Thread-safe log collector for device tests. Uses lock because native
	/// Apple Intelligence invokes tools concurrently from different threads.
	/// </summary>
	private class DeviceTestLogCollector : ILoggerFactory, ILogger
	{
		private readonly LogLevel _minimumLevel;
		private readonly object _lock = new();

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
			{
				var entry = new DeviceTestLogEntry(logLevel, formatter(state, exception));
				lock (_lock)
				{
					Entries.Add(entry);
				}
			}
		}
	}

	private record DeviceTestLogEntry(LogLevel Level, string Message);
}

#endif
