using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.SourceGen.Css;

/// <summary>
/// Compile-time CSS parser that works on strings (no TextReader).
/// Mirrors the runtime parser in Microsoft.Maui.Controls.StyleSheets.
/// </summary>
static class CssParser
{
	public static (IReadOnlyList<CssRule> Rules, IReadOnlyList<CssMediaGroup> MediaGroups, IReadOnlyList<CssDiagnostic> Diagnostics) Parse(string css)
	{
		var rules = new List<CssRule>();
		var mediaGroups = new List<CssMediaGroup>();
		var diagnostics = new List<CssDiagnostic>();
		int pos = 0;
		int line = 1;
		int col = 1;

		SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);

		while (pos < css.Length)
		{
			var c = css[pos];

			if (c == '@')
			{
				ParseAtRule(css, ref pos, ref line, ref col, rules, mediaGroups, diagnostics);
				SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);
				continue;
			}

			// Read selector
			int selectorStart = pos;
			int selectorStartLine = line;
			int selectorStartCol = col;
			if (!AdvanceTo(css, '{', ref pos, ref line, ref col))
			{
				if (pos > selectorStart) // trailing text with no block
					diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Error, "Expected '{' after selector.", selectorStartLine, selectorStartCol));
				break;
			}

			string selectorText = css.Substring(selectorStart, pos - selectorStart).Trim();
			pos++;
			col++; // skip '{'

			// Read declarations
			int declStart = pos;
			int declStartLine = line;
			if (!AdvanceTo(css, '}', ref pos, ref line, ref col))
			{
				diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Error, "Expected '}' to close declaration block.", declStartLine, 1));
				break;
			}

			string declBlock = css.Substring(declStart, pos - declStart);
			pos++;
			col++; // skip '}'

			if (string.IsNullOrWhiteSpace(selectorText))
			{
				diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Error, "Empty selector.", selectorStartLine, selectorStartCol));
				SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);
				continue;
			}

			var (declarations, importantProps) = ParseDeclarations(declBlock, diagnostics, declStartLine);
			rules.Add(new CssRule(selectorText, declarations, importantProps.Count > 0 ? importantProps : null));

			SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);
		}

		return (rules, mediaGroups, diagnostics);
	}

	static (List<KeyValuePair<string, string>> Declarations, HashSet<string> ImportantProperties) ParseDeclarations(string block, List<CssDiagnostic> diagnostics, int baseLine)
	{
		var declarations = new List<KeyValuePair<string, string>>();
		var importantProps = new HashSet<string>();
		int pos = 0;
		int line = baseLine;
		int col = 1;

		SkipWhiteSpaceAndComments(block, ref pos, ref line, ref col);

		while (pos < block.Length)
		{
			// Read property name
			int nameStart = pos;
			while (pos < block.Length && block[pos] != ':' && block[pos] != ';' && block[pos] != '}')
			{
				if (block[pos] == '\n')
				{ line++; col = 1; }
				else
				{ col++; }
				pos++;
			}

			string name = block.Substring(nameStart, pos - nameStart).Trim();

			if (pos >= block.Length || block[pos] != ':')
			{
				if (!string.IsNullOrWhiteSpace(name))
					diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Warning, $"Expected ':' after property name '{name}'.", line, col));
				if (pos < block.Length && block[pos] == ';')
				{
					pos++;
					col++;
					SkipWhiteSpaceAndComments(block, ref pos, ref line, ref col);
				}
				continue;
			}

			pos++;
			col++; // skip ':'
			SkipWhiteSpaceAndComments(block, ref pos, ref line, ref col);

			// Read property value
			int valueStart = pos;
			while (pos < block.Length && block[pos] != ';' && block[pos] != '}')
			{
				if (block[pos] == '\n')
				{ line++; col = 1; }
				else
				{ col++; }
				pos++;
			}

			string value = block.Substring(valueStart, pos - valueStart).Trim();

			if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
			{
				// Strip !important at compile time
				bool important = false;
				if (value.EndsWith("!important", StringComparison.OrdinalIgnoreCase))
				{
					value = value.Substring(0, value.Length - "!important".Length).Trim();
					important = true;
				}

				// Resolve calc/rem/em at compile time
				value = ResolveUnits(value);

				// Expand shorthands at compile time
				if (!TryExpandShorthand(declarations, importantProps, name, value, important))
				{
					declarations.Add(new KeyValuePair<string, string>(name, value));
					if (important)
						importantProps.Add(name);
				}
			}

			if (pos < block.Length && block[pos] == ';')
			{
				pos++;
				col++;
			}

			SkipWhiteSpaceAndComments(block, ref pos, ref line, ref col);
		}

		return (declarations, importantProps);
	}

	static bool TryExpandShorthand(List<KeyValuePair<string, string>> decls, HashSet<string> importantProps, string name, string value, bool important)
	{
		switch (name)
		{
			case "border":
				ExpandBorderShorthand(decls, importantProps, value, important);
				return true;
			case "font":
				ExpandFontShorthand(decls, importantProps, value, important);
				return true;
			default:
				return false;
		}
	}

	static void AddExpanded(List<KeyValuePair<string, string>> decls, HashSet<string> importantProps, string name, string value, bool important)
	{
		decls.Add(new KeyValuePair<string, string>(name, value));
		if (important)
			importantProps.Add(name);
	}

	static void ExpandBorderShorthand(List<KeyValuePair<string, string>> decls, HashSet<string> importantProps, string value, bool important)
	{
		var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var part in parts)
		{
			var p = part.Trim();
			if (IsBorderStyle(p))
				continue;
			if (IsNumericOrUnit(p))
				AddExpanded(decls, importantProps, "border-width", p, important);
			else
				AddExpanded(decls, importantProps, "border-color", p, important);
		}
	}

	static void ExpandFontShorthand(List<KeyValuePair<string, string>> decls, HashSet<string> importantProps, string value, bool important)
	{
		var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var part in parts)
		{
			var p = part.Trim();
			if (p.Equals("italic", StringComparison.OrdinalIgnoreCase) || p.Equals("oblique", StringComparison.OrdinalIgnoreCase))
				AddExpanded(decls, importantProps, "font-style", p, important);
			else if (p.Equals("bold", StringComparison.OrdinalIgnoreCase) || p.Equals("normal", StringComparison.OrdinalIgnoreCase)
				|| p.Equals("bolder", StringComparison.OrdinalIgnoreCase) || p.Equals("lighter", StringComparison.OrdinalIgnoreCase))
				AddExpanded(decls, importantProps, "font-weight", p, important);
			else if (IsNumericOrUnit(p))
				AddExpanded(decls, importantProps, "font-size", p, important);
			else
				AddExpanded(decls, importantProps, "font-family", p, important);
		}
	}

	static bool IsBorderStyle(string v)
		=> v.Equals("none", StringComparison.OrdinalIgnoreCase) || v.Equals("solid", StringComparison.OrdinalIgnoreCase)
		|| v.Equals("dashed", StringComparison.OrdinalIgnoreCase) || v.Equals("dotted", StringComparison.OrdinalIgnoreCase)
		|| v.Equals("double", StringComparison.OrdinalIgnoreCase) || v.Equals("groove", StringComparison.OrdinalIgnoreCase)
		|| v.Equals("ridge", StringComparison.OrdinalIgnoreCase) || v.Equals("inset", StringComparison.OrdinalIgnoreCase)
		|| v.Equals("outset", StringComparison.OrdinalIgnoreCase);

	static bool IsNumericOrUnit(string v)
		=> double.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)
		|| v.EndsWith("px", StringComparison.OrdinalIgnoreCase)
		|| v.EndsWith("rem", StringComparison.OrdinalIgnoreCase)
		|| v.EndsWith("em", StringComparison.OrdinalIgnoreCase);

	/// <summary>Resolve calc/rem/em at compile time — mirrors runtime CssValueResolver.</summary>
	static string ResolveUnits(string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		var trimmed = value.Trim();

		if (trimmed.StartsWith("calc(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(")", StringComparison.Ordinal))
		{
			var expr = trimmed.Substring(5, trimmed.Length - 6).Trim();
			if (TryEvaluateCalc(expr, out double result))
				return result.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return value;
		}

		if (TryParseWithUnit(trimmed, out double resolved))
			return resolved.ToString(System.Globalization.CultureInfo.InvariantCulture);

		return value;
	}

	const double DefaultRootFontSize = 16.0;

	static bool TryParseWithUnit(string value, out double result)
	{
		result = 0;
		if (value.EndsWith("rem", StringComparison.OrdinalIgnoreCase))
		{
			if (double.TryParse(value.Substring(0, value.Length - 3).Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double n))
			{ result = n * DefaultRootFontSize; return true; }
		}
		else if (value.EndsWith("em", StringComparison.OrdinalIgnoreCase))
		{
			if (double.TryParse(value.Substring(0, value.Length - 2).Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double n))
			{ result = n * DefaultRootFontSize; return true; }
		}
		else if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
		{
			if (double.TryParse(value.Substring(0, value.Length - 2).Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double n))
			{ result = n; return true; }
		}
		return false;
	}

	static bool TryEvaluateCalc(string expr, out double result)
	{
		result = 0;
		int opIndex = -1;
		char op = '\0';

		for (int i = expr.Length - 1; i >= 1; i--)
		{
			char c = expr[i];
			if ((c == '+' || c == '-') && char.IsWhiteSpace(expr[i - 1]))
			{ opIndex = i; op = c; break; }
		}
		if (opIndex < 0)
		{
			for (int i = expr.Length - 1; i >= 1; i--)
			{
				char c = expr[i];
				if (c == '*' || c == '/')
				{ opIndex = i; op = c; break; }
			}
		}
		if (opIndex < 0)
			return TryResolveCalcValue(expr.Trim(), out result);

		if (!TryResolveCalcValue(expr.Substring(0, opIndex).Trim(), out double left)
			|| !TryResolveCalcValue(expr.Substring(opIndex + 1).Trim(), out double right))
			return false;

		switch (op)
		{
			case '+': result = left + right; return true;
			case '-': result = left - right; return true;
			case '*': result = left * right; return true;
			case '/': if (right == 0) return false; result = left / right; return true;
		}
		return false;
	}

	static bool TryResolveCalcValue(string value, out double result)
	{
		result = 0;
		if (string.IsNullOrWhiteSpace(value)) return false;
		if (TryParseWithUnit(value, out result)) return true;
		return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
	}

	static void ParseAtRule(string css, ref int pos, ref int line, ref int col,
		List<CssRule> rules, List<CssMediaGroup> mediaGroups, List<CssDiagnostic> diagnostics)
	{
		// pos is on '@'
		pos++; col++; // skip '@'

		// Read keyword
		int kwStart = pos;
		while (pos < css.Length && char.IsLetterOrDigit(css[pos]) && css[pos] != '{' && css[pos] != ';')
		{ pos++; col++; }

		string keyword = css.Substring(kwStart, pos - kwStart).Trim();

		if (string.Equals(keyword, "charset", StringComparison.OrdinalIgnoreCase))
		{
			// @charset "UTF-8"; — skip to semicolon
			while (pos < css.Length && css[pos] != ';')
			{
				if (css[pos] == '\n') { line++; col = 1; } else { col++; }
				pos++;
			}
			if (pos < css.Length) { pos++; col++; } // skip ';'
			return;
		}

		if (string.Equals(keyword, "media", StringComparison.OrdinalIgnoreCase))
		{
			SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);

			// Read condition up to '{'
			int condStart = pos;
			if (!AdvanceTo(css, '{', ref pos, ref line, ref col))
			{
				diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Error, "Expected '{' after @media condition.", line, col));
				return;
			}
			string condition = css.Substring(condStart, pos - condStart).Trim();
			pos++; col++; // skip '{'

			// Parse rules inside @media block until matching '}'
			var mediaRules = new List<CssRule>();
			SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);

			while (pos < css.Length && css[pos] != '}')
			{
				// Read selector
				int selStart = pos;
				int selLine = line;
				int selCol = col;
				if (!AdvanceTo(css, '{', ref pos, ref line, ref col))
				{
					diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Error, "Expected '{' after selector in @media block.", selLine, selCol));
					break;
				}

				string selectorText = css.Substring(selStart, pos - selStart).Trim();
				pos++; col++; // skip '{'

				// Read declarations
				int declStart = pos;
				int declLine = line;
				if (!AdvanceTo(css, '}', ref pos, ref line, ref col))
				{
					diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Error, "Expected '}' in @media rule.", declLine, 1));
					break;
				}

				string declBlock = css.Substring(declStart, pos - declStart);
				pos++; col++; // skip '}'

				if (!string.IsNullOrWhiteSpace(selectorText))
				{
					var (declarations, importantProps) = ParseDeclarations(declBlock, diagnostics, declLine);
					mediaRules.Add(new CssRule(selectorText, declarations, importantProps.Count > 0 ? importantProps : null));
				}

				SkipWhiteSpaceAndComments(css, ref pos, ref line, ref col);
			}

			if (pos < css.Length && css[pos] == '}')
			{ pos++; col++; } // skip closing '}'

			if (mediaRules.Count > 0)
				mediaGroups.Add(new CssMediaGroup(condition, mediaRules));

			return;
		}

		// Unknown @-rule — skip
		diagnostics.Add(new CssDiagnostic(CssDiagnosticSeverity.Warning, $"Unsupported @{keyword} rule.", line, col));
		SkipAtRule(css, ref pos, ref line, ref col);
	}

	static void SkipWhiteSpaceAndComments(string text, ref int pos, ref int line, ref int col)
	{
		while (pos < text.Length)
		{
			var c = text[pos];
			if (c == '/' && pos + 1 < text.Length && text[pos + 1] == '*')
			{
				pos += 2;
				col += 2;
				while (pos + 1 < text.Length)
				{
					if (text[pos] == '\n')
					{ line++; col = 1; }
					else
					{ col++; }
					if (text[pos] == '*' && text[pos + 1] == '/')
					{
						pos += 2;
						col += 2;
						break;
					}
					pos++;
				}
				continue;
			}

			if (char.IsWhiteSpace(c))
			{
				if (c == '\n')
				{ line++; col = 1; }
				else
				{ col++; }
				pos++;
				continue;
			}

			break;
		}
	}

	static bool AdvanceTo(string text, char target, ref int pos, ref int line, ref int col)
	{
		// Track nesting for comments
		while (pos < text.Length)
		{
			var c = text[pos];

			// Skip comments
			if (c == '/' && pos + 1 < text.Length && text[pos + 1] == '*')
			{
				pos += 2;
				col += 2;
				while (pos + 1 < text.Length)
				{
					if (text[pos] == '\n')
					{ line++; col = 1; }
					else
					{ col++; }
					if (text[pos] == '*' && text[pos + 1] == '/')
					{
						pos += 2;
						col += 2;
						break;
					}
					pos++;
				}
				continue;
			}

			if (c == target)
				return true;

			if (c == '\n')
			{ line++; col = 1; }
			else
			{ col++; }
			pos++;
		}
		return false;
	}

	static void SkipAtRule(string text, ref int pos, ref int line, ref int col)
	{
		while (pos < text.Length)
		{
			var c = text[pos];
			if (c == '\n')
			{ line++; col = 1; }
			else
			{ col++; }
			pos++;

			if (c == ';')
				return;
			if (c == '{')
			{
				// Skip entire block
				int depth = 1;
				while (pos < text.Length && depth > 0)
				{
					c = text[pos];
					if (c == '{')
						depth++;
					else if (c == '}')
						depth--;
					if (c == '\n')
					{ line++; col = 1; }
					else
					{ col++; }
					pos++;
				}
				return;
			}
		}
	}
}

readonly record struct CssDiagnostic(CssDiagnosticSeverity Severity, string Message, int Line, int Column);

enum CssDiagnosticSeverity
{
	Warning,
	Error
}
