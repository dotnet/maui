using System;
using System.Runtime.CompilerServices;
using System.Text;
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
	/// </summary>
	[InterpolatedStringHandler]
	internal ref struct WarningInterpolatedStringHandler
	{
		StringBuilder? _builder;

		internal bool IsEnabled => _builder is not null;
		internal ILogger? Logger { get; }

		public WarningInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
		{
			Logger = Application.Current?.FindMauiContext()?.CreateLogger(typeof(MauiLog));
			if (Logger is not null && Logger.IsEnabled(LogLevel.Warning))
			{
				_builder = new StringBuilder(literalLength);
				shouldAppend = true;
			}
			else
			{
				_builder = null;
				shouldAppend = false;
			}
		}

		public void AppendLiteral(string value) => _builder!.Append(value);

		public void AppendFormatted<T>(T value) => _builder!.Append(value);

		public void AppendFormatted<T>(T value, string? format) => _builder!.AppendFormat($"{{0:{format}}}", value);

		internal string ToStringAndClear()
		{
			var result = _builder!.ToString();
			_builder = null;
			return result;
		}
	}
}
}
