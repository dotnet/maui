#nullable enable

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Hosting.Internal
{
	class FallbackLoggerFactory : ILoggerFactory
	{
		public void AddProvider(ILoggerProvider provider) { }

		public ILogger CreateLogger(string categoryName) =>
			new FallbackLogger(categoryName);

		public void Dispose() { }

		class FallbackLogger : ILogger
		{
			private string _categoryName;

			public FallbackLogger(string categoryName)
			{
				_categoryName = categoryName;
			}

			public IDisposable BeginScope<TState>(TState state) =>
				FallbackScope.Instance;

			public bool IsEnabled(LogLevel logLevel) =>
				Debugger.IsAttached && logLevel != LogLevel.None;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
				if (!IsEnabled(logLevel))
					return;

				if (formatter == null)
					throw new ArgumentNullException(nameof(formatter));

				var message = formatter(state, exception);
				if (string.IsNullOrEmpty(message))
					return;

				message = $"{ logLevel }: {message}";
				if (exception != null)
					message += Environment.NewLine + Environment.NewLine + exception;

				Debug.WriteLine(message, category: _categoryName);
			}
		}

		class FallbackScope : IDisposable
		{
			public static FallbackScope Instance { get; } = new FallbackScope();

			public void Dispose() { }
		}
	}
}