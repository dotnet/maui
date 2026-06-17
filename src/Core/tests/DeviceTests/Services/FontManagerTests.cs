#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Fonts)]
public partial class FontManagerTests : TestBase
{
	static (ServiceProvider Services, FontManagerLoggerProvider LoggerProvider) CreateFontManagerLoggerServices()
	{
		var loggerProvider = new FontManagerLoggerProvider();
		var services = new ServiceCollection()
			.AddLogging(builder => builder.AddProvider(loggerProvider))
			.BuildServiceProvider();

		return (services, loggerProvider);
	}

	sealed class FontManagerLogEntry
	{
		public FontManagerLogEntry(LogLevel logLevel, string message, Exception? exception)
		{
			LogLevel = logLevel;
			Message = message;
			Exception = exception;
		}

		public LogLevel LogLevel { get; }

		public string Message { get; }

		public Exception? Exception { get; }
	}

	sealed class FontManagerLoggerProvider : ILoggerProvider, ILogger
	{
		public List<FontManagerLogEntry> Logs { get; } = new();

		public ILogger CreateLogger(string categoryName) => this;

		public IDisposable? BeginScope<TState>(TState state)
			where TState : notnull =>
			null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			Logs.Add(new FontManagerLogEntry(logLevel, formatter(state, exception), exception));
		}

		public void Dispose()
		{
		}
	}

}
