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
	internal static void LogWarning(
		[InterpolatedStringHandlerArgument()] ref WarningInterpolatedStringHandler handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogWarning(handler.Exception, handler.ToStringAndClear());
		}
	}

	internal static void LogWarning(Exception? exception,
		[InterpolatedStringHandlerArgument("exception")] ref WarningInterpolatedStringHandler handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogWarning(exception, handler.ToStringAndClear());
		}
	}

	internal static void LogError(
		[InterpolatedStringHandlerArgument()] ref ErrorInterpolatedStringHandler handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogError(handler.Exception, handler.ToStringAndClear());
		}
	}

	internal static void LogError(Exception? exception,
		[InterpolatedStringHandlerArgument("exception")] ref ErrorInterpolatedStringHandler handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogError(exception, handler.ToStringAndClear());
		}
	}

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
		private StringBuilder? _builder;
		private Exception? _exception;

		internal bool IsEnabled => _builder is not null;
		internal ILogger? Logger { get; }
		internal Exception? Exception => _exception;

		public WarningInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
		{
			Logger = Application.Current?.FindMauiContext()?.CreateLogger(typeof(MauiLog));
			_exception = null;
			if (Logger is not null && Logger.IsEnabled(LogLevel.Warning))
			{
				_builder = new StringBuilder(literalLength + formattedCount * 16);
				shouldAppend = true;
			}
			else
			{
				_builder = null;
				shouldAppend = false;
			}
		}

		public WarningInterpolatedStringHandler(int literalLength, int formattedCount, Exception? exception, out bool shouldAppend)
			: this(literalLength, formattedCount, out shouldAppend)
		{
			_exception = exception;
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

	/// <summary>
	/// Interpolated string handler for error-level logging.
	/// </summary>
	[InterpolatedStringHandler]
	internal ref struct ErrorInterpolatedStringHandler
	{
		private StringBuilder? _builder;
		private Exception? _exception;

		internal bool IsEnabled => _builder is not null;
		internal ILogger? Logger { get; }
		internal Exception? Exception => _exception;

		public ErrorInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
		{
			Logger = Application.Current?.FindMauiContext()?.CreateLogger(typeof(MauiLog));
			_exception = null;
			if (Logger is not null && Logger.IsEnabled(LogLevel.Error))
			{
				_builder = new StringBuilder(literalLength + formattedCount * 16);
				shouldAppend = true;
			}
			else
			{
				_builder = null;
				shouldAppend = false;
			}
		}

		public ErrorInterpolatedStringHandler(int literalLength, int formattedCount, Exception? exception, out bool shouldAppend)
			: this(literalLength, formattedCount, out shouldAppend)
		{
			_exception = exception;
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
