using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	internal class TestLoggerProvider : ILoggerProvider
	{
		private readonly TestLogger _logger = new();

		/// <summary>
		/// Provides a snapshot of the current events.
		/// </summary>
		public LogEvent[] GetEvents() => _logger.GetEvents();

		public ILogger CreateLogger(string categoryName)
		{
			return _logger;
		}

		public void Dispose() { }

		private class TestLogger : ILogger
		{
			private readonly ConcurrentQueue<LogEvent> _events = new();

			internal LogEvent[] GetEvents() => _events.ToArray();

			public IDisposable BeginScope<TState>(TState state) => new Scope();

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				_events.Enqueue(new LogEvent()
				{
					LogLevel = logLevel,
					EventId = eventId,
					Message = formatter(state, exception)
				});
			}

			private class Scope : IDisposable
			{
				public void Dispose()
				{
				}
			}
		}
	}

	internal class LogEvent
	{
		public LogLevel LogLevel { get; set; }
		public EventId EventId { get; set; }
		public string Message { get; set; }
	}
}
