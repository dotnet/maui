#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.StyleSheets
{
	/// <summary>
	/// Resolves CSS variable references (var(--name) and var(--name, fallback)) in property values.
	/// </summary>
	static class CssVariableResolver
	{
		const int MaxRecursionDepth = 10;

		/// <summary>
		/// Resolves all var() references in a CSS value string using the provided variable dictionary.
		/// Returns the original value if no var() references are found.
		/// </summary>
		public static string Resolve(string value, IDictionary<string, string> variables)
		{
			if (value == null)
				return value;
			if (value.IndexOf("var(", StringComparison.Ordinal) < 0)
				return value;

			return ResolveReferences(value, variables, depth: 0);
		}

		static string ResolveReferences(string value, IDictionary<string, string> variables, int depth)
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

				// Find matching closing paren, handling nested parens for var(--a, var(--b, c))
				int parenDepth = 0;
				int contentStart = varStart + 4;
				int endIndex = contentStart;
				while (endIndex < value.Length)
				{
					if (value[endIndex] == '(')
						parenDepth++;
					else if (value[endIndex] == ')')
					{
						if (parenDepth == 0)
							break;
						parenDepth--;
					}
					endIndex++;
				}

				if (endIndex >= value.Length)
				{
					// No matching paren — append remainder as-is
					result.Append(value, varStart, value.Length - varStart);
					break;
				}

				string varContent = value.Substring(contentStart, endIndex - contentStart).Trim();
				string varName;
				string fallback = null;

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

				string resolved = null;
				if (variables != null && variables.TryGetValue(varName, out var varValue))
				{
					resolved = varValue;
				}
				else if (fallback != null)
				{
					resolved = fallback;
				}

				if (resolved != null)
				{
					// Recursively resolve nested var() references
					resolved = ResolveReferences(resolved, variables, depth + 1);
					result.Append(resolved);
				}
				// Unresolved with no fallback — omit (CSS spec: declaration becomes invalid)

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
				if (c == '(')
					depth++;
				else if (c == ')')
					depth--;
				else if (c == ',' && depth == 0)
					return i;
			}
			return -1;
		}
	}
}
