using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class MtpTestArguments
{
	public static string[] DesktopFilter(string filter, bool nunit)
	{
		if (string.IsNullOrWhiteSpace(filter))
			return Array.Empty<string>();
		if (nunit)
			return new[] { "--filter", filter };
		filter = filter.Trim();
		if (filter.StartsWith("/", StringComparison.Ordinal))
			return new[] { "--filter-query", filter };
		var depth = 0;
		foreach (var character in filter)
		{
			if (character == '(')
				depth++;
			else if (character == ')' && --depth < 0)
				throw UnsupportedFilter(filter);
		}
		if (depth != 0)
			throw UnsupportedFilter(filter);

		var terms = filter.Split('&');
		var arguments = new List<string>();
		var includedKinds = new HashSet<string>(StringComparer.Ordinal);
		foreach (var term in terms)
		{
			if (terms.Length > 1 && term.Contains('|', StringComparison.Ordinal) &&
				!(term.Trim().StartsWith("(", StringComparison.Ordinal) && term.Trim().EndsWith(")", StringComparison.Ordinal)))
				throw UnsupportedFilter(filter);
			var alternatives = term.Trim().Trim('(', ')').Split('|');
			string groupKind = null;
			bool positiveGroup = false;
			foreach (var alternative in alternatives)
			{
				var match = Regex.Match(alternative.Trim(), @"^(FullyQualifiedName|TestCategory|Category)(!~|!=|=|~)(.+)$",
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
				var property = match.Success ? match.Groups[1].Value : "FullyQualifiedName";
				var operation = match.Success ? match.Groups[2].Value : "~";
				var value = match.Success ? match.Groups[3].Value.Trim() : alternative.Trim();
				if (string.IsNullOrEmpty(value) || value.Any(character => char.IsControl(character) ||
					"()|&=!*".Contains(character, StringComparison.Ordinal)))
					throw UnsupportedFilter(filter);
				var trait = property.Equals("Category", StringComparison.OrdinalIgnoreCase) ||
					property.Equals("TestCategory", StringComparison.OrdinalIgnoreCase);
				var kind = trait ? "trait" : "method";
				if (groupKind != null && groupKind != kind)
					throw UnsupportedFilter(filter);
				groupKind = kind;
				var negated = operation.StartsWith("!", StringComparison.Ordinal);
				positiveGroup |= !negated;
				if (alternatives.Length > 1 && negated)
					throw UnsupportedFilter(filter);
				if (operation.Contains('~', StringComparison.Ordinal))
					value = "*" + value + "*";
				arguments.Add("--filter-" + (negated ? "not-" : "") + kind);
				arguments.Add(trait ? "Category=" + value : value);
			}
			// Repeated simple includes are ORed by xUnit, not ANDed.
			if (positiveGroup && !includedKinds.Add(groupKind))
				throw UnsupportedFilter(filter);
		}
		return arguments.ToArray();
	}

	static ArgumentException UnsupportedFilter(string filter)
	{
		return new ArgumentException("This legacy filter cannot be represented by xUnit simple filters. " +
			"Use an xUnit query beginning with '/' for --test-filter: " + filter, nameof(filter));
	}

	public static string[] DesktopReport(bool nunit, string filename)
	{
		return nunit ? new[] { "--report-trx", "--report-trx-filename", filename } :
			new[] { "--report-xunit-trx", "--report-xunit-trx-filename", filename };
	}

	public static string[] Native(string project, string configuration, string framework, string runtimeIdentifier,
		string platform, string filter, string device, string resultsDirectory, bool useCoreClr)
	{
		var arguments = new List<string>
		{
			"test", "--project", project, "--configuration", configuration, "--framework", framework
		};
		if (!string.IsNullOrEmpty(runtimeIdentifier))
			arguments.Add("-p:RuntimeIdentifier=" + runtimeIdentifier);
		if (useCoreClr)
			arguments.Add("-p:UseMonoRuntime=false");
		if (!string.IsNullOrEmpty(filter))
		{
			if (filter.Any(char.IsControl) ||
				!(filter.StartsWith("Category=", StringComparison.Ordinal) ||
				  filter.StartsWith("SkipCategories=", StringComparison.Ordinal)) ||
				filter.Substring(filter.IndexOf('=', StringComparison.Ordinal) + 1).Split(',', ';').Any(value =>
					string.IsNullOrWhiteSpace(value) || value.Contains('=', StringComparison.Ordinal)))
				throw new ArgumentException("Native --test-filter must be Category=<name> or SkipCategories=<name>,<name>.", nameof(filter));
			arguments.Add("-p:MauiTestFilter=" + EscapeProperty(filter));
		}
		if (!string.IsNullOrWhiteSpace(device))
		{
			arguments.Add("--device");
			arguments.Add(device);
		}
		if (platform == "android")
		{
			arguments.Add("--report-trx");
			arguments.Add("--results-directory");
			arguments.Add(resultsDirectory);
		}
		else if (platform == "catalyst")
			arguments.Add("-p:MauiTestResultsDirectory=" + EscapeProperty(resultsDirectory));
		else if (platform != "ios")
			throw new ArgumentException("Unsupported native platform.", nameof(platform));
		return arguments.ToArray();
	}

	static string EscapeProperty(string value)
	{
		return value.Replace("%", "%25", StringComparison.Ordinal)
			.Replace(",", "%2C", StringComparison.Ordinal).Replace(";", "%3B", StringComparison.Ordinal);
	}
}
