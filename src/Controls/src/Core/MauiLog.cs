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
			GetLogger<T>()?.LogWarning(handler.ToStringAndClear());
		}
	}

	internal static void Warning<T>(string message)
	{
		GetLogger<T>()?.LogWarning(message);
	}

	internal static void Warning<T>(string message, params object?[] args)
	{
		GetLogger<T>()?.LogWarning(message, args);
	}

	internal static void Warning<T>(Exception? exception, string? message, params object?[] args)
	{
		GetLogger<T>()?.LogWarning(exception, message, args);
	}

	internal static void Error<T>(string message)
	{
		GetLogger<T>()?.LogError(message);
	}

	internal static void Error<T>(Exception? exception, string? message, params object?[] args)
	{
		GetLogger<T>()?.LogError(exception, message, args);
	}

	internal static void Error<T>(string message, params object?[] args)
	{
		GetLogger<T>()?.LogError(message, args);
	}

	static ILogger? GetLogger<T>()
		=> Application.Current?.FindMauiContext()?.CreateLogger<T>();

	/// <summary>
	/// Gets a logger by category name (for non-generic callers like TypeConversionHelper).
	/// </summary>
	internal static ILogger? GetLogger(string categoryName)
		=> Application.Current?.FindMauiContext()?.CreateLogger(categoryName);

	/// <summary>
	/// Interpolated string handler that only formats the string when logging is enabled.
	/// When <see cref="Application.Current"/> is null or no <see cref="IMauiContext"/> is available,
	/// the handler skips all <c>AppendLiteral</c>/<c>AppendFormatted</c> calls — zero allocation.
	/// </summary>
	[InterpolatedStringHandler]
	internal ref struct WarningInterpolatedStringHandler
	{
		StringBuilder? _builder;

		internal bool IsEnabled => _builder is not null;

		public WarningInterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend)
		{
			if (Application.Current?.FindMauiContext() is not null)
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
