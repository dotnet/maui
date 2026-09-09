using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.SourceGen.Css;

/// <summary>
/// Emits a single C# expression that creates a compiled StyleSheet from pre-parsed rules.
/// Used for inline CSS in XAML (as opposed to .css file factories which are classes).
/// </summary>
static class CssInlineEmitter
{
	/// <summary>
	/// Emits a C# expression that calls StyleSheet.CreateCompiled() with pre-parsed rules.
	/// </summary>
	public static string EmitInline(IReadOnlyList<CssRule> rules)
	{
		// Resolve var() references at compile time
		var variables = CssVariableResolver.CollectVariables(rules);
		var resolvedRules = CssVariableResolver.ResolveRules(rules, variables);

		var sb = new StringBuilder();
		sb.Append("global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.CreateCompiled(new global::Microsoft.Maui.Controls.StyleSheets.CompiledCssRule[] { ");

		for (int i = 0; i < resolvedRules.Count; i++)
		{
			if (i > 0)
				sb.Append(", ");

			var rule = resolvedRules[i];
			sb.Append($"new global::Microsoft.Maui.Controls.StyleSheets.CompiledCssRule(\"{EscapeString(rule.Selector)}\", ");

			if (rule.Declarations.Count == 0)
			{
				sb.Append("global::System.Array.Empty<global::System.Collections.Generic.KeyValuePair<string, string>>()");
			}
			else
			{
				sb.Append("new global::System.Collections.Generic.KeyValuePair<string, string>[] { ");
				for (int j = 0; j < rule.Declarations.Count; j++)
				{
					if (j > 0)
						sb.Append(", ");
					var decl = rule.Declarations[j];
					sb.Append($"new global::System.Collections.Generic.KeyValuePair<string, string>(\"{EscapeString(decl.Key)}\", \"{EscapeString(decl.Value)}\")");
				}
				sb.Append(" }");
			}

			sb.Append(")");
		}

		sb.Append(" })");
		return sb.ToString();
	}

	static string EscapeString(string s)
		=> s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
}
