// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Maui.Controls.Sample.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Tests for the NonFunctionInvokingChatClient function logging functionality.
/// These tests verify that function calls and results are properly logged at the appropriate log levels.
/// </summary>
public class FunctionLoggingTests
{
	[Theory]
	[InlineData(LogLevel.Trace)]
	[InlineData(LogLevel.Debug)]
	[InlineData(LogLevel.Information)]
	public async Task FunctionInvocationsLogged(LogLevel level)
	{
		// Arrange
		var logCollector = new LogCollector(level);

		var mockClient = new MockChatClient();
		mockClient.AddFunctionCallContent("Func1", "callId1", new Dictionary<string, object?> { ["arg1"] = "value1" });
		mockClient.AddFunctionResultContent("callId1", "Result 1");
		mockClient.AddTextContent("world");

		using var client = new NonFunctionInvokingChatClient(mockClient, logCollector);

		// Act
		var messages = new List<ChatMessage> { new(ChatRole.User, "hello") };
		var response = await client.GetResponseAsync(messages);

		// Assert
		var logs = logCollector.Entries;
		if (level is LogLevel.Trace)
		{
			Assert.Equal(2, logs.Count);
			Assert.True(logs[0].Message.Contains("Received tool call: Func1", StringComparison.Ordinal) && logs[0].Message.Contains("\"arg1\": \"value1\"", StringComparison.Ordinal));
			Assert.True(logs[1].Message.Contains("Received tool result for call ID: callId1", StringComparison.Ordinal) && logs[1].Message.Contains("Result 1", StringComparison.Ordinal));
		}
		else if (level is LogLevel.Debug)
		{
			Assert.Equal(2, logs.Count);
			Assert.True(logs[0].Message.Contains("Received tool call: Func1", StringComparison.Ordinal) && !logs[0].Message.Contains("arg1", StringComparison.Ordinal));
			Assert.True(logs[1].Message.Contains("Received tool result for call ID: callId1", StringComparison.Ordinal) && !logs[1].Message.Contains("Result", StringComparison.Ordinal));
		}
		else
		{
			Assert.Empty(logs);
		}
	}

	[Theory]
	[InlineData(LogLevel.Trace)]
	[InlineData(LogLevel.Debug)]
	[InlineData(LogLevel.Information)]
	public async Task FunctionInvocationsLoggedForStreaming(LogLevel level)
	{
		// Arrange
		var logCollector = new LogCollector(level);

		var mockClient = new MockChatClient();
		mockClient.AddFunctionCallContent("Func1", "callId1", new Dictionary<string, object?> { ["arg1"] = "value1" });
		mockClient.AddFunctionResultContent("callId1", "Result 1");
		mockClient.AddTextContent("world");

		using var client = new NonFunctionInvokingChatClient(mockClient, logCollector);

		// Act
		var messages = new List<ChatMessage> { new(ChatRole.User, "hello") };
		var updates = new List<ChatResponseUpdate>();
		await foreach (var update in client.GetStreamingResponseAsync(messages))
		{
			updates.Add(update);
		}

		// Assert
		var logs = logCollector.Entries;
		if (level is LogLevel.Trace)
		{
			Assert.Equal(2, logs.Count);
			Assert.True(logs[0].Message.Contains("Received tool call: Func1", StringComparison.Ordinal) && logs[0].Message.Contains("\"arg1\": \"value1\"", StringComparison.Ordinal));
			Assert.True(logs[1].Message.Contains("Received tool result for call ID: callId1", StringComparison.Ordinal) && logs[1].Message.Contains("Result 1", StringComparison.Ordinal));
		}
		else if (level is LogLevel.Debug)
		{
			Assert.Equal(2, logs.Count);
			Assert.True(logs[0].Message.Contains("Received tool call: Func1", StringComparison.Ordinal) && !logs[0].Message.Contains("arg1", StringComparison.Ordinal));
			Assert.True(logs[1].Message.Contains("Received tool result for call ID: callId1", StringComparison.Ordinal) && !logs[1].Message.Contains("Result", StringComparison.Ordinal));
		}
		else
		{
			Assert.Empty(logs);
		}
	}

	[Fact]
	public async Task NoLoggingWhenNoFunctionCalls()
	{
		// Arrange
		var logCollector = new LogCollector(LogLevel.Trace);

		var mockClient = new MockChatClient();
		mockClient.AddTextContent("Hello there!");

		using var client = new NonFunctionInvokingChatClient(mockClient, logCollector);

		// Act
		var messages = new List<ChatMessage> { new(ChatRole.User, "hello") };
		var response = await client.GetResponseAsync(messages);

		// Assert - no tool logs since there were no function calls
		Assert.Empty(logCollector.Entries);
	}

	[Fact]
	public async Task MultipleFunctionCallsLogged()
	{
		// Arrange
		var logCollector = new LogCollector(LogLevel.Debug);

		var mockClient = new MockChatClient();
		mockClient.AddFunctionCallContent("Func1", "callId1", new Dictionary<string, object?> { ["arg1"] = "value1" });
		mockClient.AddFunctionCallContent("Func2", "callId2", new Dictionary<string, object?> { ["arg2"] = "value2" });
		mockClient.AddFunctionResultContent("callId1", "Result 1");
		mockClient.AddFunctionResultContent("callId2", "Result 2");
		mockClient.AddTextContent("done");

		using var client = new NonFunctionInvokingChatClient(mockClient, logCollector);

		// Act
		var messages = new List<ChatMessage> { new(ChatRole.User, "hello") };
		var response = await client.GetResponseAsync(messages);

		// Assert
		var logs = logCollector.Entries;
		Assert.Equal(4, logs.Count);
		Assert.Contains(logs, l => l.Message.Contains("Func1", StringComparison.Ordinal));
		Assert.Contains(logs, l => l.Message.Contains("Func2", StringComparison.Ordinal));
		Assert.Contains(logs, l => l.Message.Contains("callId1", StringComparison.Ordinal));
		Assert.Contains(logs, l => l.Message.Contains("callId2", StringComparison.Ordinal));
	}

	/// <summary>
	/// Simple log collector for testing that captures log messages.
	/// </summary>
	private class LogCollector : ILoggerFactory, ILogger
	{
		private readonly LogLevel _minimumLevel;

		public LogCollector(LogLevel minimumLevel)
		{
			_minimumLevel = minimumLevel;
		}

		public List<LogEntry> Entries { get; } = [];

		// ILoggerFactory
		public ILogger CreateLogger(string categoryName) => this;
		public void AddProvider(ILoggerProvider provider) { }
		public void Dispose() { }

		// ILogger
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (IsEnabled(logLevel))
			{
				Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
			}
		}
	}

	public record LogEntry(LogLevel Level, string Message);

	/// <summary>
	/// Mock chat client for testing that allows adding predefined responses including function calls and results.
	/// </summary>
	private class MockChatClient : IChatClient
	{
		private readonly List<AIContent> _streamContent = [];
		private string? _nonStreamingResponse;

		public ChatClientMetadata Metadata => new("MockClient");

		public void AddTextContent(string text)
		{
			_streamContent.Add(new TextContent(text));
		}

		public void AddFunctionCallContent(string name, string callId, Dictionary<string, object?>? arguments = null)
		{
			_streamContent.Add(new FunctionCallContent(callId, name, arguments));
		}

		public void AddFunctionResultContent(string callId, object? result)
		{
			_streamContent.Add(new FunctionResultContent(callId, result));
		}

		public void SetNonStreamingResponse(string response)
		{
			_nonStreamingResponse = response;
		}

		public Task<ChatResponse> GetResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			var responseMessages = new List<ChatMessage>();

			// Group contents by type for more realistic message structure
			var currentContents = new List<AIContent>();
			ChatRole currentRole = ChatRole.Assistant;

			foreach (var content in _streamContent)
			{
				if (content is FunctionResultContent)
				{
					// Function results should be in a Tool message
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
			{
				responseMessages.Add(new ChatMessage(currentRole, [.. currentContents]));
			}

			if (responseMessages.Count == 0 && _nonStreamingResponse is not null)
			{
				responseMessages.Add(new ChatMessage(ChatRole.Assistant, _nonStreamingResponse));
			}

			return Task.FromResult(new ChatResponse(responseMessages));
		}

		public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages,
			ChatOptions? options = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			foreach (var content in _streamContent)
			{
				await Task.Yield();

				var role = content is FunctionResultContent ? ChatRole.Tool : ChatRole.Assistant;

				yield return new ChatResponseUpdate
				{
					Role = role,
					Contents = [content]
				};
			}
		}

		public object? GetService(Type serviceType, object? serviceKey = null) => null;

		public TService? GetService<TService>(object? key = null) where TService : class => null;

		public void Dispose() { }
	}
}
