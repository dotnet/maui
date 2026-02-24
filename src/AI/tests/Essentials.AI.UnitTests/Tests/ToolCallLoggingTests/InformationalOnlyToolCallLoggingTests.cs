// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Xunit;
using static Microsoft.Maui.Essentials.AI.UnitTests.ToolCallLoggingHelpers;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Tests the pipeline with InformationalOnly=true tool calls (our pattern).
/// The native Apple Intelligence model invokes tools itself; FICC sees
/// InformationalOnly and skips re-invocation. LoggingChatClient still
/// serializes the full response at Trace level.
/// Pipeline: MockClient → FunctionInvokingChatClient → LoggingChatClient
/// </summary>
public class InformationalOnlyToolCallLoggingTests
{
	[Fact]
	public async Task Trace_LogsFullToolCallContent()
	{
		var (pipeline, logs, options) = BuildPipeline(LogLevel.Trace, informationalOnly: true);

		await pipeline.GetResponseAsync([new ChatMessage(ChatRole.User, "weather?")], options);

		var allLogs = CombineLogs(logs);

		// LoggingChatClient serializes full response at Trace — tool details visible
		Assert.Contains("GetWeather", allLogs, StringComparison.Ordinal);
		Assert.Contains("call-1", allLogs, StringComparison.Ordinal);
		Assert.Contains("Seattle", allLogs, StringComparison.Ordinal);
		Assert.Contains("Sunny, 72", allLogs, StringComparison.Ordinal);

		// FICC does NOT log "Invoking" because InformationalOnly=true → skipped
		Assert.DoesNotContain("Invoking GetWeather", allLogs, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Debug_LogsLifecycleOnly()
	{
		var (pipeline, logs, options) = BuildPipeline(LogLevel.Debug, informationalOnly: true);

		await pipeline.GetResponseAsync([new ChatMessage(ChatRole.User, "weather?")], options);

		var debugMessages = logs.Entries.Where(e => e.Level == LogLevel.Debug).Select(e => e.Message).ToList();

		// LoggingChatClient lifecycle at Debug
		Assert.Contains(debugMessages, m => m.Contains("GetResponseAsync invoked", StringComparison.Ordinal));
		Assert.Contains(debugMessages, m => m.Contains("GetResponseAsync completed", StringComparison.Ordinal));

		// No content details at Debug — tool names, args, results are Trace-only
		var allLogs = CombineLogs(logs);
		Assert.DoesNotContain("GetWeather", allLogs, StringComparison.Ordinal);
		Assert.DoesNotContain("Seattle", allLogs, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Information_NoLogsEmitted()
	{
		var (pipeline, logs, options) = BuildPipeline(LogLevel.Information, informationalOnly: true);

		await pipeline.GetResponseAsync([new ChatMessage(ChatRole.User, "weather?")], options);

		// Both LoggingChatClient and FICC only log at Debug/Trace — nothing at Information+
		Assert.Empty(logs.Entries);
	}

	[Fact]
	public async Task Streaming_Trace_LogsEachUpdate()
	{
		var (pipeline, logs, options) = BuildPipeline(LogLevel.Trace, informationalOnly: true);

		await foreach (var _ in pipeline.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "weather?")], options))
		{
		}

		var traceLogs = logs.Entries.Where(e => e.Level == LogLevel.Trace).Select(e => e.Message).ToList();
		Assert.Contains(traceLogs, m => m.Contains("received update", StringComparison.Ordinal));

		var allTrace = string.Join("\n", traceLogs);
		Assert.Contains("GetWeather", allTrace, StringComparison.Ordinal);
		Assert.Contains("call-1", allTrace, StringComparison.Ordinal);
	}

	[Fact]
	public async Task MultipleFunctionCalls_AllLoggedAtTrace()
	{
		var logs = new LogCollector(LogLevel.Trace);
		var loggerFactory = new SingleLoggerFactory(logs);

		var mockClient = new MockToolCallClient(informationalOnly: true);
		mockClient.AddFunctionCallContent("GetWeather", "call-1",
			new Dictionary<string, object?> { ["location"] = "NYC" });
		mockClient.AddFunctionCallContent("GetTime", "call-2",
			new Dictionary<string, object?> { ["timezone"] = "EST" });
		mockClient.AddFunctionResultContent("call-1", "Cloudy");
		mockClient.AddFunctionResultContent("call-2", "3:00 PM");
		mockClient.AddTextContent("Done.");

		using var pipeline = new ChatClientBuilder(mockClient)
			.UseFunctionInvocation(loggerFactory)
			.Use(inner => new LoggingChatClient(inner, logs))
			.Build();

		await pipeline.GetResponseAsync([new ChatMessage(ChatRole.User, "info?")]);

		var allLogs = CombineLogs(logs);
		Assert.Contains("GetWeather", allLogs, StringComparison.Ordinal);
		Assert.Contains("GetTime", allLogs, StringComparison.Ordinal);
		Assert.Contains("NYC", allLogs, StringComparison.Ordinal);
		Assert.Contains("EST", allLogs, StringComparison.Ordinal);
	}

	[Fact]
	public async Task NoTools_NoFunctionContentInLogs()
	{
		var logs = new LogCollector(LogLevel.Trace);
		var mockClient = new MockToolCallClient(informationalOnly: true);
		mockClient.AddTextContent("Hello there!");

		using var pipeline = new ChatClientBuilder(mockClient)
			.Use(inner => new LoggingChatClient(inner, logs))
			.Build();

		await pipeline.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

		var allLogs = CombineLogs(logs);
		Assert.DoesNotContain("FunctionCallContent", allLogs, StringComparison.Ordinal);
		Assert.DoesNotContain("FunctionResultContent", allLogs, StringComparison.Ordinal);
	}
}
