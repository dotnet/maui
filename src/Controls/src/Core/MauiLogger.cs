using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Simplified internal logging helper that avoids the verbose
/// <c>Application.Current?.FindMauiContext()?.CreateLogger&lt;T&gt;()?.LogWarning(...)</c> chain
/// and uses an interpolated string handler to skip string formatting when logging is disabled.
/// </summary>
internal static class MauiLogger<T>
{
	private static ILogger<T>? CreateLogger()
		=> Application.Current?.FindMauiContext()?.CreateLogger<T>();

	internal static void Log(LogLevel level, string message)
	{
		var logger = CreateLogger();
		if (logger is not null && logger.IsEnabled(level))
			logger.Log(level, message);
	}

	internal static void Log(LogLevel level, string message, params object[] args)
	{
		var logger = CreateLogger();
		if (logger is not null && logger.IsEnabled(level))
			logger.Log(level, string.Format(message, args));
	}

	internal static void Log(LogLevel level, Exception? exception, string message)
	{
		var logger = CreateLogger();
		if (logger is not null && logger.IsEnabled(level))
			logger.Log(level, exception, message);
	}

#if !NETSTANDARD
	internal static void Log(LogLevel level, [InterpolatedStringHandlerArgument("level")] ref LogInterpolatedStringHandler handler)
	{
		handler.Log();
	}
	
	internal static void Log(LogLevel level, Exception? exception, [InterpolatedStringHandlerArgument("level")] ref LogInterpolatedStringHandler handler)
	{
		handler.Log(exception);
	}

	/// <summary>
	/// Interpolated string handler that resolves <c>ILogger&lt;T&gt;</c> and checks
	/// <see cref="ILogger.IsEnabled"/> at the given <see cref="LogLevel"/> before
	/// formatting. When the logger is null or the level is disabled,
	/// <c>shouldAppend</c> is <c>false</c> and the compiler skips all Append
	/// calls — zero allocation.
	/// </summary>
	[InterpolatedStringHandler]
	internal ref struct LogInterpolatedStringHandler
	{
		private DefaultInterpolatedStringHandler _inner;
		private readonly ILogger? _logger;
		private readonly LogLevel _level;

		public LogInterpolatedStringHandler(int literalLength, int formattedCount, LogLevel level, out bool shouldAppend)
		{
			_level = level;
			_logger = CreateLogger();

			if (_logger is not null && _logger.IsEnabled(level))
			{
				_inner = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
				shouldAppend = true;
			}
			else
			{
				_logger = null;
				_inner = default;
				shouldAppend = false;
			}
		}

		public void AppendLiteral(string value) => _inner.AppendLiteral(value);
		public void AppendFormatted<U>(U value) => _inner.AppendFormatted(value);
		public void AppendFormatted<U>(U value, string? format) => _inner.AppendFormatted(value, format);

		internal void Log()
		{
			if (_logger is not null)
			{
				_logger.Log(_level, _inner.ToStringAndClear());
			}
		}

		internal void Log(Exception? exception)
		{
			if (_logger is not null)
			{
				_logger.Log(_level, exception, _inner.ToStringAndClear());
			}
		}
	}
#endif
}
