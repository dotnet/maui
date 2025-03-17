using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Maui;
/// <summary>
/// Inspired on https://github.com/dotnet/aspnetcore/blob/befc463e34b17edf8aee1fbf9dec44c75f13000b/src/Shared/Debugger/DebuggerHelpers.cs from Asp.NET
/// </summary>
static class DebuggerDisplayHelpers
{
	const string NullString = "(null)";

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDebugText(string key1, object? value1, bool includeNullValues = true)
	{
		return GetDebugText([Create(key1, value1)], includeNullValues);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDebugText(string key1, object? value1, string key2, object? value2, bool includeNullValues = true)
	{
		return GetDebugText([Create(key1, value1), Create(key2, value2)], includeNullValues);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDebugText(string key1, object? value1, string key2, object? value2, string key3, object? value3, bool includeNullValues = true)
	{
		return GetDebugText([Create(key1, value1), Create(key2, value2), Create(key3, value3)], includeNullValues);
	}

	public static string GetDebugText(ReadOnlySpan<KeyValuePair<string, object?>> values, bool includeNullValues = true)
	{
		var size = values.Length;
		if (size is 0)
		{
			return string.Empty;
		}

		var sb = new StringBuilder();

		var first = true;
		for (var i = 0; i < size; i++)
		{
			var kvp = values[i];

			if (HasValue(kvp.Value) || includeNullValues)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					sb.Append(", ");
				}

				sb.Append(kvp.Key);
				sb.Append(" = ");
				if (kvp.Value is null)
				{
					sb.Append(NullString);
				}
				else if (kvp.Value is string s)
				{
					sb.Append(s);
				}
				else if (kvp.Value is IEnumerable enumerable)
				{
					var firstItem = true;
					foreach (var item in enumerable)
					{
						if (firstItem)
						{
							firstItem = false;
						}
						else
						{
							sb.Append(',');
						}
						sb.Append(item);
					}
				}
				else
				{
					sb.Append(kvp.Value);
				}
			}
		}

		return sb.ToString();
	}

	static bool HasValue([NotNullWhen(true)] object? value)
	{
		if (value is null)
		{
			return false;
		}

		// Empty collections don't have a value.
		if (value is not string && value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())
		{
			return false;
		}

		return true;
	}

	static KeyValuePair<string, object?> Create(string key, object? value) => new KeyValuePair<string, object?>(key, value);
}
