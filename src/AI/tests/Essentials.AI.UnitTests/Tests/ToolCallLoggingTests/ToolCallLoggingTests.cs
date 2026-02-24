// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Tests that verify tool calls (FunctionCallContent, FunctionResultContent) are logged
/// by the standard LoggingChatClient at Trace level, ensuring debuggability.
/// </summary>
public class ToolCallLoggingTests
{
	[Theory]
	[InlineData(LogLevel.Trace)]
	[InlineData(LogLevel.Debug)]
	[InlineData(LogLevel.Information)]
	public async Task FunctionCallContentLoggedAtTraceLevel(LogLevel level)
	{
		var logCollector = new LogCollector(level);
		var mockClient = new MockToolCallClient();
		mockClient.AddFunctionCallContent("GetWeather", "call-1",
			new Dictionary<string, object?> { ["location"] = "Seattle" });
		mockClient.AddFunctionResultContent("call-1", "Sunny, 72°F");
		mockClient.AddTextContent("The weather is sunny.");

		using var client = new LoggingChatClient(mockClient, logCollector);
		var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "weather?")]);

		if (level is LogLevel.Trace)
		{
			// At Trace, LoggingChatClient serializes the full response including FunctionCallContent
			var allLogs = string.Join("\n", logCollector.Entries.Select(e => e.Message));
			Assert.Contains("GetWeather", allLogs, StringComparison.Ordinal);
			Assert.Contains("call-1", allLogs, StringComparison.Ordinal);
			Assert.Contains("Seattle", allLogs, StringComparison.Ordinal);
			Assert.Contains("Sunny, 72", allLogs, StringComparison.Ordinal);
		}
		else
		{
			// At Debug, only method invocation/completion is logged (no content details)
			var allLogs = string.Join("\n", logCollector.Entries.Select(e => e.Message));
			Assert.DoesNotContain("GetWeather", allLogs, StringComparison.Ordinal);
			Assert.DoesNotContain("Seattle", allLogs, StringComparison.Ordinal);
		}
	}

	[Theory]
	[InlineData(LogLevel.Trace)]
	[InlineData(LogLevel.Debug)]
	[InlineData(LogLevel.Information)]
	public async Task FunctionCallContentLoggedAtTraceLevelForStreaming(LogLevel level)
	{
		var logCollector = new LogCollector(level);
		var mockClient = new MockToolCallClient();
		mockClient.AddFunctionCallContent("GetWeather", "call-1",
			new Dictionary<string, object?> { ["location"] = "Boston" });
		mockClient.AddFunctionResultContent("call-1", "Rainy, 55°F");
		mockClient.AddTextContent("It's raining.");

		using var client = new LoggingChatClient(mockClient, logCollector);
		var updates = new List<ChatResponseUpdate>();
		await foreach (var update in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "weather?")]))
		{
			updates.Add(update);
		}

		if (level is LogLevel.Trace)
		{
			var allLogs = string.Join("\n", logCollector.Entries.Select(e => e.Message));
			Assert.Contains("GetWeather", allLogs, StringComparison.Ordinal);
			Assert.Contains("call-1", allLogs, StringComparison.Ordinal);
			Assert.Contains("Boston", allLogs, StringComparison.Ordinal);
		}
		else
		{
			var allLogs = string.Join("\n", logCollector.Entries.Select(e => e.Message));
			Assert.DoesNotContain("GetWeather", allLogs, StringComparison.Ordinal);
			Assert.DoesNotContain("Boston", allLogs, StringComparison.Ordinal);
		}
	}

	[Fact]
	public async Task NoToolCallContentWhenNoFunctions()
	{
		var logCollector = new LogCollector(LogLevel.Trace);
		var mockClient = new MockToolCallClient();
		mockClient.AddTextContent("Hello there!");

		using var client = new LoggingChatClient(mockClient, logCollector);
		var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

		var allLogs = string.Join("\n", logCollector.Entries.Select(e => e.Message));
		Assert.DoesNotContain("FunctionCallContent", allLogs, StringComparison.Ordinal);
		Assert.DoesNotContain("FunctionResultContent", allLogs, StringComparison.Ordinal);
	}

	[Fact]
	public async Task MultipleFunctionCallsLoggedAtTrace()
	{
		var logCollector = new LogCollector(LogLevel.Trace);
		var mockClient = new MockToolCallClient();
		mockClient.AddFunctionCallContent("GetWeather", "call-1",
			new Dictionary<string, object?> { ["location"] = "NYC" });
		mockClient.AddFunctionCallContent("GetTime", "call-2",
			new Dictionary<string, object?> { ["timezone"] = "EST" });
		mockClient.AddFunctionResultContent("call-1", "Cloudy, 60°F");
		mockClient.AddFunctionResultContent("call-2", "3:00 PM");
		mockClient.AddTextContent("Done.");

		using var client = new LoggingChatClient(mockClient, logCollector);
		var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "info?")]);

		var allLogs = string.Join("\n", logCollector.Entries.Select(e => e.Message));
		Assert.Contains("GetWeather", allLogs, StringComparison.Ordinal);
		Assert.Contains("GetTime", allLogs, StringComparison.Ordinal);
		Assert.Contains("NYC", allLogs, StringComparison.Ordinal);
		Assert.Contains("EST", allLogs, StringComparison.Ordinal);
	}

	/// <summary>
	/// Simple log collector for testing.
	/// </summary>
	private class LogCollector : ILogger
	{
		private readonly LogLevel _minimumLevel;

		public LogCollector(LogLevel minimumLevel) => _minimumLevel = minimumLevel;

		public List<LogEntry> Entries { get; } = [];

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (IsEnabled(logLevel))
				Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
		}
	}

	public record LogEntry(LogLevel Level, string Message);

	/// <summary>
	/// Mock chat client that returns predefined tool call responses.
	/// </summary>
	private class MockToolCallClient : IChatClient
	{
		private readonly List<AIContent> _content = [];

		public ChatClientMetadata Metadata => new("MockToolCallClient");

		public void AddTextContent(string text) =>
			_content.Add(new TextContent(text));

		public void AddFunctionCallContent(string name, string callId, Dictionary<string, object?>? arguments = null) =>
			_content.Add(new FunctionCallContent(callId, name, arguments) { InformationalOnly = true });

		public void AddFunctionResultContent(string callId, object? result) =>
			_content.Add(new FunctionResultContent(callId, result));

		public Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
		{
			var responseMessages = new List<ChatMessage>();
			var currentContents = new List<AIContent>();
			ChatRole currentRole = ChatRole.Assistant;

			foreach (var content in _content)
			{
				if (content is FunctionResultContent)
				{
					if (currentContents.Count > 0)
					{
						responseMessages.Add(new ChatMessage(currentRole, [.. currentContents]));
						currentContents.Clear();
					}
					responseMessages.Add(new ChatMessage(ChatRole.Tool, [content]));
				}
				else
				{
					currentContents.Add(content);
					currentRole = ChatRole.Assistant;
				}
			}

			if (currentContents.Count > 0)
				responseMessages.Add(new ChatMessage(currentRole, [.. currentContents]));

			return Task.FromResult(new ChatResponse(responseMessages));
		}

		public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages, ChatOptions? options = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			foreach (var content in _content)
			{
				await Task.Yield();
				var role = content is FunctionResultContent ? ChatRole.Tool : ChatRole.Assistant;
				yield return new ChatResponseUpdate { Role = role, Contents = [content] };
			}
		}

		public object? GetService(Type serviceType, object? serviceKey = null) => null;
		public TService? GetService<TService>(object? key = null) where TService : class => null;
		public void Dispose() { }
	}
}
