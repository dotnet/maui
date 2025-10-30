using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Maui.Controls.SourceGen;

public static class AnalyzerConfigOptionsExtensions
{
	public static bool IsEnabled(this AnalyzerConfigOptions options, string key)
		=> options.TryGetValue(key, out var value) && string.Compare(value, "enable", StringComparison.OrdinalIgnoreCase) == 0;

	public static bool IsDisabled(this AnalyzerConfigOptions options, string key)
		=> options.TryGetValue(key, out var value) && string.Compare(value, "disable", StringComparison.OrdinalIgnoreCase) == 0;

	public static bool IsTrue(this AnalyzerConfigOptions options, string key)
		=> options.TryGetValue(key, out var value) && string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0;

	public static bool IsFalse(this AnalyzerConfigOptions options, string key)
		=> options.TryGetValue(key, out var value) && string.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0;

	public static string GetValueOrDefault(this AnalyzerConfigOptions options, string key, string defaultValue)
		=> options.TryGetValue(key, out var value) ? value : defaultValue;

	public static string? GetValueOrNull(this AnalyzerConfigOptions options, string key)
		=> options.TryGetValue(key, out var value) ? value : null;
}
