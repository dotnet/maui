#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners;

internal static class NativeTestArguments
{
	public static string[] GetNameArguments(string? names, string option)
	{
		if (string.IsNullOrWhiteSpace(names))
			return [];
		if (names.Length > 32768 || names.Any(char.IsControl))
			throw new ArgumentException("Native test selection contains control characters or exceeds 32768 characters.", nameof(names));
		var values = names.Split([',', ';']).Select(name => name.Trim()).Distinct().ToArray();
		if (values.Any(string.IsNullOrEmpty))
			throw new ArgumentException("Native test selection contains an empty name.", nameof(names));
		return values.SelectMany(name => new[]
		{
			option,
			option == "--filter-method" && !name.Contains('.', StringComparison.Ordinal) ? $"*.{name}" : name
		}).ToArray();
	}

	public static string[] GetFilterArguments(string? filter)
	{
		if (string.IsNullOrEmpty(filter))
			return [];

		if (filter.StartsWith("Category=", StringComparison.Ordinal))
		{
			var category = filter["Category=".Length..];
			if (string.IsNullOrWhiteSpace(category) || category.Contains('=', StringComparison.Ordinal))
				throw new ArgumentException("MauiTestFilter must specify a nonempty Category.", nameof(filter));
			return ["--filter-trait", $"Category={category}"];
		}

		if (filter.StartsWith("SkipCategories=", StringComparison.Ordinal))
		{
			var categories = filter["SkipCategories=".Length..].Split([',', ';']);
			if (categories.Any(category => string.IsNullOrWhiteSpace(category) || category.Contains('=', StringComparison.Ordinal)))
				throw new ArgumentException("MauiTestFilter contains an empty or invalid excluded category.", nameof(filter));
			return categories.SelectMany(category => new[] { "--filter-not-trait", $"Category={category.Trim()}" }).ToArray();
		}

		throw new ArgumentException("MauiTestFilter must be Category=<name> or SkipCategories=<name>,<name>.", nameof(filter));
	}
}
