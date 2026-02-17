using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

#if NETSTANDARD
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	internal sealed class InterpolatedStringHandlerAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
	{
		public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) => Arguments = arguments;
		public string[] Arguments { get; }
	}
}
#endif

namespace Microsoft.Maui.Controls
{

/// <summary>
/// Simplified internal logging helper that avoids the verbose
/// <c>Application.Current?.FindMauiContext()?.CreateLogger&lt;T&gt;()?.LogWarning(...)</c> chain
/// and uses an interpolated string handler to skip string formatting when logging is disabled.
/// </summary>
internal static class MauiLog
{
	internal static void Warning<T>(
		[InterpolatedStringHandlerArgument()] ref WarningInterpolatedStringHandler handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogWarning(handler.ToStringAndClear());
		}
	}

	internal static void Warning<T>(string message)
	{
		var logger = GetLogger<T>();
		if (logger is not null && logger.IsEnabled(LogLevel.Warning))
			logger.LogWarning(message);
	}

	internal static void Warning<T>(string message, params object?[] args)
	{
		var logger = GetLogger<T>();
		if (logger is not null && logger.IsEnabled(LogLevel.Warning))
			logger.LogWarning(message, args);
	}

	internal static void Warning<T>(Exception? exception, string? message, params object?[] args)
	{
		var logger = GetLogger<T>();
		if (logger is not null && logger.IsEnabled(LogLevel.Warning))
			logger.LogWarning(exception, message, args);
	}

	internal static void Error<T>(string message)
	{
		var logger = GetLogger<T>();
		if (logger is not null && logger.IsEnabled(LogLevel.Error))
			logger.LogError(message);
	}

	internal static void Error<T>(Exception? exception, string? message, params object?[] args)
	{
		var logger = GetLogger<T>();
		if (logger is not null && logger.IsEnabled(LogLevel.Error))
			logger.LogError(exception, message, args);
	}

	internal static void Error<T>(string message, params object?[] args)
	{
		var logger = GetLogger<T>();
		if (logger is not null && logger.IsEnabled(LogLevel.Error))
			logger.LogError(message, args);
	}

	static ILogger? GetLogger<T>()
		=> Application.Current?.FindMauiContext()?.CreateLogger<T>();

	/// <summary>
	/// Gets a logger by category name (for non-generic callers like TypeConversionHelper).
	/// </summary>
	internal static ILogger? GetLogger(string categoryName)
		=> Application.Current?.FindMauiContext()?.CreateLogger(categoryName);

	/// <summary>
	/// Interpolated string handler that resolves the logger and checks
	/// <see cref="ILogger.IsEnabled"/> before formatting. When the log level
	/// is filtered out, <c>shouldAppend</c> is <c>false</c> and the compiler
	/// skips all Append calls — zero allocation.
	///
	/// Uses inline fields and <see cref="string.Concat(string?, string?, string?, string?)"/>
	/// instead of StringBuilder to match (or beat) plain <c>$"..."</c>
	/// interpolation performance for the short messages typical of MAUI logging.
	/// </summary>
	[InterpolatedStringHandler]
	internal ref struct WarningInterpolatedStringHandler
	{
		private string? _s0, _s1, _s2, _s3, _s4, _s5, _s6;
		private int _count;

		internal bool IsEnabled { get; }
		internal ILogger? Logger { get; }

		public WarningInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
		{
			_count = 0;
			_s0 = _s1 = _s2 = _s3 = _s4 = _s5 = _s6 = null;
			Logger = Application.Current?.FindMauiContext()?.CreateLogger(typeof(MauiLog));
			if (Logger is not null && Logger.IsEnabled(LogLevel.Warning))
			{
				IsEnabled = true;
				shouldAppend = true;
			}
			else
			{
				IsEnabled = false;
				shouldAppend = false;
			}
		}

		public void AppendLiteral(string value) => Add(value);

		public void AppendFormatted<T>(T value) => Add(value?.ToString() ?? "");

		public void AppendFormatted<T>(T value, string? format) => Add(string.Format($"{{0:{format}}}", value));

		private void Add(string value)
		{
			switch (_count++)
			{
				case 0: _s0 = value; break;
				case 1: _s1 = value; break;
				case 2: _s2 = value; break;
				case 3: _s3 = value; break;
				case 4: _s4 = value; break;
				case 5: _s5 = value; break;
				default: _s6 = (_s6 ?? "") + value; break;
			}
		}

		internal string ToStringAndClear()
		{
			var result = _count switch
			{
				0 => "",
				1 => _s0!,
				2 => string.Concat(_s0, _s1),
				3 => string.Concat(_s0, _s1, _s2),
				4 => string.Concat(_s0, _s1, _s2, _s3),
				_ => string.Concat(_s0, _s1, _s2, _s3) + string.Concat(_s4, _s5, _s6),
			};
			_count = 0;
			return result;
		}
	}
}
}
