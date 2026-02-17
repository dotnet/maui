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
	internal static void LogWarning<T>(
		[InterpolatedStringHandlerArgument()] ref LogInterpolatedStringHandler<T> handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogWarning(handler.ToStringAndClear());
		}
	}

	internal static void LogWarning<T>(Exception? exception,
		[InterpolatedStringHandlerArgument("exception")] ref LogInterpolatedStringHandler<T> handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogWarning(exception, handler.ToStringAndClear());
		}
	}

	internal static void LogError<T>(
		[InterpolatedStringHandlerArgument()] ref LogInterpolatedStringHandler<T> handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogError(handler.ToStringAndClear());
		}
	}

	internal static void LogError<T>(Exception? exception,
		[InterpolatedStringHandlerArgument("exception")] ref LogInterpolatedStringHandler<T> handler)
	{
		if (handler.IsEnabled)
		{
			handler.Logger!.LogError(exception, handler.ToStringAndClear());
		}
	}

	/// <summary>
	/// Gets a logger by category name (for callers that need structured logging).
	/// </summary>
	internal static ILogger? GetLogger(string categoryName)
		=> Application.Current?.FindMauiContext()?.CreateLogger(categoryName);

	static ILogger? GetLogger<T>()
		=> Application.Current?.FindMauiContext()?.CreateLogger<T>();

	/// <summary>
	/// Interpolated string handler that resolves the logger and checks
	/// <see cref="ILogger.IsEnabled"/> before formatting. When the log level
	/// is filtered out, <c>shouldAppend</c> is <c>false</c> and the compiler
	/// skips all Append calls — zero allocation.
	/// </summary>
	[InterpolatedStringHandler]
	internal ref struct LogInterpolatedStringHandler<T>
	{
		private StringBuilder? _builder;

		internal bool IsEnabled => _builder is not null;
		internal ILogger? Logger { get; }

		public LogInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
		{
			Logger = GetLogger<T>();
			if (Logger is not null && Logger.IsEnabled(LogLevel.Trace))
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

		public LogInterpolatedStringHandler(int literalLength, int formattedCount, Exception? exception, out bool shouldAppend)
			: this(literalLength, formattedCount, out shouldAppend)
		{
		}

		public void AppendLiteral(string value) => _builder!.Append(value);

		public void AppendFormatted<U>(U value) => _builder!.Append(value);

		public void AppendFormatted<U>(U value, string? format) => _builder!.AppendFormat($"{{0:{format}}}", value);

		internal string ToStringAndClear()
		{
			var result = _builder!.ToString();
			_builder = null;
			return result;
		}
	}
}
}
