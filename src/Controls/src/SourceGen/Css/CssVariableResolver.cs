using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.SourceGen.Css;

/// <summary>
/// Resolves CSS variable references (var(--name) and var(--name, fallback)) at compile time.
/// This is the source-generator counterpart of the runtime CssVariableResolver.
/// </summary>
static class CssVariableResolver
{
	const int MaxRecursionDepth = 10;

	/// <summary>
	/// Collects all custom property declarations (--*) from the given rules into a flat dictionary.
	/// </summary>
	public static Dictionary<string, string> CollectVariables(IReadOnlyList<CssRule> rules)
	{
		Dictionary<string, string>? variables = null;
		foreach (var rule in rules)
		{
			foreach (var decl in rule.Declarations)
			{
				if (decl.Key.StartsWith("--", StringComparison.Ordinal))
				{
					variables ??= new Dictionary<string, string>(StringComparer.Ordinal);
					variables[decl.Key] = decl.Value;
				}
			}
		}
		return variables ?? new Dictionary<string, string>(StringComparer.Ordinal);
	}

	/// <summary>
	/// Returns a new list of rules with all var() references in declaration values resolved
	/// using the provided variables dictionary. Custom property declarations are preserved as-is.
	/// </summary>
	public static IReadOnlyList<CssRule> ResolveRules(IReadOnlyList<CssRule> rules, Dictionary<string, string> variables)
	{
		if (variables.Count == 0)
			return rules;

		var resolved = new List<CssRule>(rules.Count);
		foreach (var rule in rules)
		{
			bool anyResolved = false;
			List<KeyValuePair<string, string>>? resolvedDecls = null;

			for (int i = 0; i < rule.Declarations.Count; i++)
			{
				var decl = rule.Declarations[i];
				if (decl.Value.IndexOf("var(", StringComparison.Ordinal) >= 0)
				{
					if (!anyResolved)
					{
						resolvedDecls = new List<KeyValuePair<string, string>>(rule.Declarations.Count);
						for (int j = 0; j < i; j++)
							resolvedDecls.Add(rule.Declarations[j]);
						anyResolved = true;
					}
					var resolvedValue = Resolve(decl.Value, variables);
					resolvedDecls!.Add(new KeyValuePair<string, string>(decl.Key, resolvedValue));
				}
				else if (anyResolved)
				{
					resolvedDecls!.Add(decl);
				}
			}

			resolved.Add(anyResolved ? new CssRule(rule.Selector, resolvedDecls!) : rule);
		}
		return resolved;
	}

	/// <summary>
	/// Resolves all var() references in a single value string.
	/// </summary>
	public static string Resolve(string value, Dictionary<string, string> variables)
	{
		if (value is null || value.IndexOf("var(", StringComparison.Ordinal) < 0)
			return value!;
		return ResolveReferences(value, variables, depth: 0);
	}

	static string ResolveReferences(string value, Dictionary<string, string> variables, int depth)
	{
		if (depth > MaxRecursionDepth)
			return value;

		var result = new StringBuilder(value.Length);
		int pos = 0;

		while (pos < value.Length)
		{
			int varStart = value.IndexOf("var(", pos, StringComparison.Ordinal);
			if (varStart < 0)
			{
				result.Append(value, pos, value.Length - pos);
				break;
			}

			result.Append(value, pos, varStart - pos);

			int parenDepth = 0;
			int contentStart = varStart + 4;
			int endIndex = contentStart;
			while (endIndex < value.Length)
			{
				if (value[endIndex] == '(') parenDepth++;
				else if (value[endIndex] == ')')
				{
					if (parenDepth == 0) break;
					parenDepth--;
				}
				endIndex++;
			}

			if (endIndex >= value.Length)
			{
				result.Append(value, varStart, value.Length - varStart);
				break;
			}

			string varContent = value.Substring(contentStart, endIndex - contentStart).Trim();
			string varName;
			string? fallback = null;

			int commaIndex = FindTopLevelComma(varContent);
			if (commaIndex >= 0)
			{
				varName = varContent.Substring(0, commaIndex).Trim();
				fallback = varContent.Substring(commaIndex + 1).Trim();
			}
			else
			{
				varName = varContent;
			}

			string? resolved = null;
			if (variables.TryGetValue(varName, out var varValue))
				resolved = varValue;
			else if (fallback != null)
				resolved = fallback;

			if (resolved != null)
			{
				resolved = ResolveReferences(resolved, variables, depth + 1);
				result.Append(resolved);
			}

			pos = endIndex + 1;
		}

		return result.ToString();
	}

	static int FindTopLevelComma(string s)
	{
		int depth = 0;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c == '(') depth++;
			else if (c == ')') depth--;
			else if (c == ',' && depth == 0) return i;
		}
		return -1;
	}
}
