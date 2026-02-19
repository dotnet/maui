using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample;

/// <summary>
/// A simple file-based logging provider for capturing ILogger output during UI tests.
/// Enabled when the MAUI_LOG_FILE environment variable is set to the desired log file path.
/// </summary>
internal class FileLoggingProvider : ILoggerProvider
{
	private readonly StreamWriter _writer;
	private readonly LogLevel _minLevel;

	public FileLoggingProvider(string filePath, LogLevel minLevel = LogLevel.Debug)
	{
		_minLevel = minLevel;
		var directory = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(directory))
		{
			Directory.CreateDirectory(directory);
		}
		_writer = new StreamWriter(filePath, append: false) { AutoFlush = true };
		_writer.WriteLine($"=== MAUI HostApp File Logger Started at {DateTime.Now} ===");
		_writer.WriteLine($"Log file: {filePath}");
		_writer.WriteLine($"Minimum log level: {minLevel}");
		_writer.WriteLine();
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new FileLogger(categoryName, _writer, _minLevel);
	}

	public void Dispose()
	{
		_writer?.Dispose();
	}
}

internal class FileLogger : ILogger
{
	private readonly string _categoryName;
	private readonly StreamWriter _writer;
	private readonly LogLevel _minLevel;
	private static readonly object _lock = new object();

	public FileLogger(string categoryName, StreamWriter writer, LogLevel minLevel)
	{
		_categoryName = categoryName;
		_writer = writer;
		_minLevel = minLevel;
	}

	public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

	public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		var message = formatter(state, exception);
		var levelString = logLevel switch
		{
			LogLevel.Trace => "TRACE",
			LogLevel.Debug => "DEBUG",
			LogLevel.Information => "INFO",
			LogLevel.Warning => "WARN",
			LogLevel.Error => "ERROR",
			LogLevel.Critical => "CRIT",
			_ => "NONE"
		};

		var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
		var logLine = $"[{timestamp}] [{levelString}] {_categoryName}: {message}";

		lock (_lock)
		{
			_writer.WriteLine(logLine);
			if (exception != null)
			{
				_writer.WriteLine($"  Exception: {exception}");
			}
		}

		// Also write to console for local debugging visibility
		Console.WriteLine(logLine);
		if (exception != null)
		{
			Console.WriteLine($"  Exception: {exception}");
		}
	}

	private class NullScope : IDisposable
	{
		public static NullScope Instance { get; } = new NullScope();
		public void Dispose() { }
	}
}
