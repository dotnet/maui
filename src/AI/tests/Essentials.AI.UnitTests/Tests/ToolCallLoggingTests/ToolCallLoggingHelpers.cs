// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Shared infrastructure for tool call logging tests.
/// </summary>
internal static class ToolCallLoggingHelpers
{
	/// <summary>
	/// Builds a pipeline: MockClient → FunctionInvokingChatClient → LoggingChatClient.
	/// </summary>
	public static (IChatClient Pipeline, LogCollector Logs, ChatOptions Options) BuildPipeline(
		LogLevel level, bool informationalOnly)
	{
		var logs = new LogCollector(level);
		var loggerFactory = new SingleLoggerFactory(logs);

		var mockClient = new MockToolCallClient(informationalOnly);
		mockClient.AddFunctionCallContent("GetWeather", "call-1",
			new Dictionary<string, object?> { ["location"] = "Seattle" });
		mockClient.AddFunctionResultContent("call-1", "Sunny, 72°F");
		mockClient.AddTextContent("The weather is sunny.");

		var pipeline = new ChatClientBuilder(mockClient)
			.UseFunctionInvocation(loggerFactory)
			.Use(inner => new LoggingChatClient(inner, logs))
			.Build();

		// For invocable tools, register the tool in ChatOptions so FICC can find it
		var options = new ChatOptions();
		if (!informationalOnly)
		{
			options.Tools = [AIFunctionFactory.Create(
				(string location) => $"Sunny, 72°F in {location}",
				name: "GetWeather",
				description: "Gets the weather")];
		}

		return (pipeline, logs, options);
	}

	public static string CombineLogs(LogCollector logs) =>
		string.Join("\n", logs.Entries.Select(e => e.Message));
}

/// <summary>
/// Collects log entries with minimum level filtering.
/// </summary>
internal class LogCollector : ILogger
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

internal record LogEntry(LogLevel Level, string Message);

/// <summary>
/// Minimal ILoggerFactory that returns a single shared logger instance.
/// Required by FunctionInvokingChatClient constructor.
/// </summary>
internal class SingleLoggerFactory : ILoggerFactory
{
	private readonly ILogger _logger;
	public SingleLoggerFactory(ILogger logger) => _logger = logger;
	public ILogger CreateLogger(string categoryName) => _logger;
	public void AddProvider(ILoggerProvider provider) { }
	public void Dispose() { }
}

/// <summary>
/// Mock chat client that returns predefined content.
/// When informationalOnly=false, FunctionCallContent is invocable by FICC,
/// and a matching tool is registered in ChatOptions.
/// </summary>
internal class MockToolCallClient : IChatClient
{
	private readonly bool _informationalOnly;
	private readonly List<AIContent> _content = [];

	public MockToolCallClient(bool informationalOnly) => _informationalOnly = informationalOnly;

	public ChatClientMetadata Metadata => new("MockToolCallClient");

	public void AddTextContent(string text) =>
		_content.Add(new TextContent(text));

	public void AddFunctionCallContent(string name, string callId, Dictionary<string, object?>? arguments = null) =>
		_content.Add(new FunctionCallContent(callId, name, arguments) { InformationalOnly = _informationalOnly });

	public void AddFunctionResultContent(string callId, object? result) =>
		_content.Add(new FunctionResultContent(callId, result));

	public Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
	{
		// When FICC invokes tools and re-calls us, return just the text
		if (messages.Any(m => m.Role == ChatRole.Tool))
			return Task.FromResult(new ChatResponse([new ChatMessage(ChatRole.Assistant, "Weather result processed.")]));

		var responseMessages = new List<ChatMessage>();
		var currentContents = new List<AIContent>();

		foreach (var content in _content)
		{
			if (content is FunctionResultContent)
			{
				if (currentContents.Count > 0)
				{
					responseMessages.Add(new ChatMessage(ChatRole.Assistant, [.. currentContents]));
					currentContents.Clear();
				}
				responseMessages.Add(new ChatMessage(ChatRole.Tool, [content]));
			}
			else
			{
				currentContents.Add(content);
			}
		}

		if (currentContents.Count > 0)
			responseMessages.Add(new ChatMessage(ChatRole.Assistant, [.. currentContents]));

		return Task.FromResult(new ChatResponse(responseMessages));
	}

	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages, ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		// When FICC re-calls after invocation, return processed text
		if (messages.Any(m => m.Role == ChatRole.Tool))
		{
			yield return new ChatResponseUpdate { Role = ChatRole.Assistant, Contents = [new TextContent("Weather result processed.")] };
			yield break;
		}

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
