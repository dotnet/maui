using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public static class UnitTestLoggerExtensions
	{
		public static ILoggingBuilder AddUnitTestLogger(
			this ILoggingBuilder builder)
		{
			// ensure a new UnitTestLoggerProvider is created every test
			// to guarantee MockApplication.MockLogger is cleared between tests
			var loggerProvider = new UnitTestLoggerProvider();
			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<ILoggerProvider>(loggerProvider));

			return builder;
		}

	}
	public sealed class UnitTestLoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, UnitTestLogger> _loggers = new();

		public UnitTestLoggerProvider()
		{
			MockApplication.MockLogger = new UnitTestLogger();
		}

		public ILogger CreateLogger(string categoryName) =>
			_loggers.GetOrAdd(categoryName, name => MockApplication.MockLogger);

		public void Dispose()
		{
			_loggers.Clear();
		}
	}

	public class UnitTestLogger : ILogger
	{
		readonly List<string> _messages = new();
		public IReadOnlyList<string> Messages => _messages;

		public IDisposable BeginScope<TState>(TState state) => default;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			_messages.Add($"{formatter(state, exception)}");
		}
	}
}
