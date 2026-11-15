using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Maui.Controls.BindingSourceGen;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Represents a C# expression to be emitted directly in generated code.
/// Used as the Value of a ValueNode when the value is a C# expression rather than a literal string.
/// </summary>
/// <param name="Code">The C# expression code to emit (already has single quotes transformed to double quotes).</param>
internal sealed record Expression(string Code);

internal readonly struct InterpolatedStringIdentifier
{
	public string Identifier { get; }
	public string TypeReferenceCandidate { get; }

	public InterpolatedStringIdentifier(string identifier, string? typeReferenceCandidate = null)
	{
		Identifier = identifier;
		TypeReferenceCandidate = typeReferenceCandidate ?? identifier;
	}
}

/// <summary>
/// Helper methods for detecting and transforming C# expressions embedded in XAML.
/// </summary>
static class CSharpExpressionHelpers
{
	// Patterns that indicate unambiguous C# syntax
	static readonly Regex UnambiguousCSharpPattern = new Regex(
		@"^{(?:" +
			@"\s*\(" +                      // Method call or lambda: {(...
			@"|\s*!" +                       // Negation: {!...
			@"|\s*new\s+" +                  // Instantiation: {new ...
			@"|\s*\$['""]" +                 // String interpolation: {$'...' or {$"...
			@"|\s*typeof\s*\(" +             // typeof: {typeof(...
			@"|\s*nameof\s*\(" +             // nameof: {nameof(...
			@"|\s*default\s*\(" +            // default: {default(...
			@"|\s*sizeof\s*\(" +             // sizeof: {sizeof(...
			@"|\s*\.[A-Za-z_]" +             // Dot prefix for binding: {.Name
			@"|\s*this\." +                  // this. prefix for local: {this.Foo
			@"|\s*BindingContext\." +        // BindingContext. prefix for binding: {BindingContext.Foo
			@"|\s*[A-Za-z_]\w*\.\(" +        // Attached BP target syntax: {target.(Grid.Row)
		@")",
		RegexOptions.Compiled);

	// Operators that indicate C# expressions (checked after first identifier)
	// Note: Single-char operators like +, -, *, / require surrounding spaces to avoid
	// false positives with markup extension syntax like {Binding Path=+...}.
	// Multi-char operators (==, !=, etc.) are unambiguous and don't need spaces.
	// This is a heuristic - the actual expression parsing handles all valid C# syntax.
	static readonly string[] CSharpOperators = new[]
	{
		"=>",   // Lambda
		"??",   // Null-coalescing
		"?.",   // Null-conditional
		"&&",   // Logical AND
		"||",   // Logical OR
		"==",   // Equality
		"!=",   // Inequality
		"<=",   // Less than or equal
		">=",   // Greater than or equal
		"<<",   // Left shift
		">>",   // Right shift
		"++",   // Increment
		"--",   // Decrement
		"+",    // Addition
		"-",    // Subtraction
		"*",    // Multiplication
		"/",    // Division
		"%",    // Modulo
		"<",    // Less than
		">",    // Greater than
		"?",    // Ternary (start)
		":",    // Ternary (else) / null-forgiving - checked after markup extension detection
		// Word aliases require spaces as word boundaries
		" AND ", // Alias for && (case-insensitive)
		" OR ",  // Alias for || (case-insensitive)
		" LT ",  // Alias for < (case-insensitive)
		" GT ",  // Alias for > (case-insensitive)
		" LTE ", // Alias for <= (case-insensitive)
		" GTE ", // Alias for >= (case-insensitive)
	};

	// Pattern to detect method call: identifier followed by ( with optional content
	static readonly Regex MethodCallPattern = new Regex(
		@"{\s*\w+\s*\(",
		RegexOptions.Compiled);

	// Pattern to detect member access: {Identifier.Identifier} (not a namespace-qualified markup extension)
	// This distinguishes C# property access like {User.Name} from markup extensions like {local:MyExtension}
	static readonly Regex MemberAccessPattern = new Regex(
		@"^\{\s*\w+\.\w+",  // Must have letter.letter (not letter:letter which is namespace)
		RegexOptions.Compiled);

	/// <summary>
	/// Determines if the value is an explicit C# expression using {= ...} syntax.
	/// </summary>
	public static bool IsExplicitExpression(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return false;

		var trimmed = value!.Trim();
		return trimmed.Length > 3
			&& trimmed[0] == '{'
			&& trimmed[1] == '='
			&& trimmed[trimmed.Length - 1] == '}';
	}

	/// <summary>
	/// Determines if the value is an implicit C# expression detected by unambiguous syntax.
	/// </summary>
	public static bool IsImplicitExpression(string? value)
		=> IsImplicitExpression(value, null);

	/// <summary>
	/// Determines if the value is an implicit C# expression detected by unambiguous syntax.
	/// </summary>
	/// <param name="value">The markup string to check.</param>
	/// <param name="canResolveMarkupExtension">Optional callback to check if a name resolves to a markup extension type.</param>
	public static bool IsImplicitExpression(string? value, Func<string, bool>? canResolveMarkupExtension)
	{
		if (string.IsNullOrEmpty(value))
			return false;

		var trimmed = value!.Trim();
		if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
			return false;

		// Check for escape sequence
		if (trimmed.StartsWith("{}", StringComparison.Ordinal))
			return false;

		// If it looks like a markup extension call (starts with known or resolvable extension name), 
		// don't treat it as an implicit expression even if it contains operators
		if (StartsWithMarkupExtension(trimmed, canResolveMarkupExtension))
			return false;

		// Check for patterns at the start that are unambiguously C#
		if (UnambiguousCSharpPattern.IsMatch(trimmed))
			return true;

		// Check for method call pattern: {identifier(
		if (MethodCallPattern.IsMatch(trimmed))
			return true;

		// Check for member access pattern: {Identifier.Identifier} (C# property access, not namespace:type)
		if (MemberAccessPattern.IsMatch(trimmed))
			return true;

		// Check for C# operators anywhere in the expression
		foreach (var op in CSharpOperators)
		{
			// Use case-insensitive for word-based aliases (AND, OR, LT, GT, LTE, GTE)
			var comparison = (op == " AND " || op == " OR " || op == " LT " || op == " GT " || op == " LTE " || op == " GTE ")
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;
			if (trimmed.IndexOf(op, comparison) >= 0)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the markup string starts with a known or resolvable markup extension name.
	/// </summary>
	static bool StartsWithMarkupExtension(string trimmed, Func<string, bool>? canResolveMarkupExtension = null)
	{
		// Extract the first identifier after the opening brace
		// Pattern: {Identifier or {Identifier ... or {prefix:Identifier
		int start = 1; // Skip '{'
		while (start < trimmed.Length && char.IsWhiteSpace(trimmed[start]))
			start++;

		int end = start;
		while (end < trimmed.Length && (char.IsLetterOrDigit(trimmed[end]) || trimmed[end] == ':'))
			end++;

		if (end <= start)
			return false;

		var identifier = trimmed.Substring(start, end - start);

		// Handle prefixed identifiers like x:Type or local:MyExtension
		var colonIndex = identifier.IndexOf(':');
		if (colonIndex >= 0)
		{
			// If prefix:Name is followed by '.', it's a type reference in an expression
			// (e.g., {local:Helper.GetValue()} or {this.(ios:Page.Prop)}), not a markup extension
			if (end < trimmed.Length && trimmed[end] == '.')
				return false;

			// Otherwise, prefix:Name is a markup extension (custom or known)
			return true;
		}

		// Check known extensions first (fast path)
		if (IsKnownMarkupExtensionName(identifier))
			return true;

		// Check if the type can be resolved as a markup extension via semantic lookup
		if (canResolveMarkupExtension != null && canResolveMarkupExtension(identifier))
			return true;

		return false;
	}

	/// <summary>
	/// Determines if the value is a C# expression (explicit or implicit).
	/// </summary>
	public static bool IsExpression(string? value)
		=> IsExplicitExpression(value) || IsImplicitExpression(value);

	/// <summary>
	/// Result of bare identifier classification.
	/// </summary>
	internal readonly struct BareIdentifierResult
	{
		/// <summary>True if should be treated as C# expression, false for markup extension.</summary>
		public bool IsExpression { get; init; }

		/// <summary>True if both markup extension and property exist (ambiguous).</summary>
		public bool IsAmbiguous { get; init; }

		/// <summary>The bare identifier name (for diagnostic reporting).</summary>
		public string? Name { get; init; }
	}

	/// <summary>
	/// Attempts to classify the markup string as a C# expression.
	/// Consolidates all expression detection logic including bare identifier resolution.
	/// </summary>
	/// <param name="markupString">The raw markup string including braces.</param>
	/// <param name="canResolveMarkupExtension">
	/// Callback to check if a bare identifier can be resolved as a markup extension type.
	/// Only called for non-prefixed bare identifiers that aren't known markup extensions.
	/// </param>
	/// <returns>True if the markup should be treated as a C# expression, false for markup extension.</returns>
	public static bool IsExpression(string? markupString, Func<string, bool> canResolveMarkupExtension)
	{
		return ClassifyExpression(markupString, canResolveMarkupExtension, null).IsExpression;
	}

	/// <summary>
	/// Attempts to classify the markup string as a C# expression, with additional property checking for ambiguity detection.
	/// </summary>
	/// <param name="markupString">The raw markup string including braces.</param>
	/// <param name="canResolveMarkupExtension">
	/// Callback to check if a bare identifier can be resolved as a markup extension type.
	/// </param>
	/// <param name="canResolveProperty">
	/// Optional callback to check if a bare identifier can be resolved as a property.
	/// Used for detecting ambiguity between markup extensions and properties.
	/// </param>
	/// <returns>Classification result including ambiguity information.</returns>
	public static BareIdentifierResult ClassifyExpression(string? markupString, Func<string, bool> canResolveMarkupExtension, Func<string, bool>? canResolveProperty)
	{
		if (string.IsNullOrEmpty(markupString))
			return new BareIdentifierResult { IsExpression = false };

		// Explicit expressions {= ...} are always expressions
		if (IsExplicitExpression(markupString))
			return new BareIdentifierResult { IsExpression = true };

		// Unambiguous C# syntax (operators, method calls, etc.)
		// Pass the resolver to handle custom markup extensions properly
		if (IsImplicitExpression(markupString, canResolveMarkupExtension))
			return new BareIdentifierResult { IsExpression = true };

		// Bare identifiers need special handling
		if (IsBareIdentifier(markupString!))
		{
			var (prefix, name) = ParseBareIdentifier(markupString!);

			// Prefixed identifiers (e.g., {local:Foo}) are always markup extensions
			if (prefix != null)
				return new BareIdentifierResult { IsExpression = false };

			var isMarkup = canResolveMarkupExtension(name);
			var isProperty = canResolveProperty?.Invoke(name) ?? false;

			if (isMarkup)
			{
				// Markup extension wins (backward compatible), but check for ambiguity
				return new BareIdentifierResult
				{
					IsExpression = false,
					IsAmbiguous = isProperty,
					Name = name
				};
			}

			// Not a markup extension - treat as C# expression (property access)
			return new BareIdentifierResult { IsExpression = true, Name = name };
		}

		// Not an expression
		return new BareIdentifierResult { IsExpression = false };
	}

	// Pattern to match a bare identifier: {Identifier} or {prefix:Identifier}
	// This is ambiguous - could be a property or a markup extension
	static readonly Regex BareIdentifierPattern = new Regex(
		@"^\{\s*(?:(\w+):)?(\w+)\s*\}$",
		RegexOptions.Compiled);

	/// <summary>
	/// Determines if the value is a bare identifier that could be ambiguous.
	/// Returns true for {Foo} or {prefix:Foo} with no additional content.
	/// </summary>
	public static bool IsBareIdentifier(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return false;
		return BareIdentifierPattern.IsMatch(value!.Trim());
	}

	/// <summary>
	/// Parses a bare identifier into its prefix and local name components.
	/// </summary>
	public static (string? prefix, string name) ParseBareIdentifier(string value)
	{
		var match = BareIdentifierPattern.Match(value.Trim());
		if (!match.Success)
			return (null, value);

		var prefix = match.Groups[1].Success ? match.Groups[1].Value : null;
		var name = match.Groups[2].Value;
		return (prefix, name);
	}

	/// <summary>
	/// List of well-known MAUI markup extensions.
	/// Used by StartsWithMarkupExtension to avoid treating {Binding Path=X} as C# expression.
	/// </summary>
	static readonly HashSet<string> KnownMarkupExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"Binding",
		"StaticResource",
		"DynamicResource",
		"Static",
		"Reference",
		"x:Reference",
		"Type",
		"x:Type",
		"Null",
		"x:Null",
		"Array",
		"x:Array",
		"TemplateBinding",
		"RelativeSource",
		"OnPlatform",
		"OnIdiom",
		"AppThemeBinding",
		"DataTemplate",
		"FontImage",
	};

	internal static bool IsKnownMarkupExtensionName(string name)
	{
		return KnownMarkupExtensions.Contains(name)
			|| KnownMarkupExtensions.Contains(name + "Extension");
	}

	/// <summary>
	/// Extracts the C# code from an expression value and transforms quotes.
	/// Single-quoted strings are always transformed to double-quoted strings since C# doesn't support single-quoted strings.
	/// </summary>
	/// <param name="value">The raw XAML value including braces.</param>
	/// <returns>The C# code ready to be emitted.</returns>
	public static string GetExpressionCode(string value)
	{
		var trimmed = value.Trim();

		// Remove outer braces
		string code;
		if (IsExplicitExpression(value))
		{
			// {= expression} -> expression
			code = trimmed.Substring(2, trimmed.Length - 3).Trim();
		}
		else
		{
			// {expression} -> expression
			code = trimmed.Substring(1, trimmed.Length - 2).Trim();
		}

		// Transform operator aliases (AND -> &&, OR -> ||) to avoid XML escaping
		code = TransformOperatorAliases(code);

		// Parenthesize ternary expressions that sit directly inside a string
		// interpolation hole. In C#, the first top-level ':' inside a hole starts the
		// format specifier, so `$"{cond ? a : b}"` is invalid (CS8361). Wrapping the
		// conditional in parentheses (`$"{(cond ? a : b)}"`) keeps the ':' inside the
		// expression. This runs before quote translation so it can operate on the
		// single-quoted source. (issue #36155)
		code = ParenthesizeInterpolationTernaries(code);

		// Always transform single-quoted strings to double-quoted strings
		// C# doesn't support single-quoted strings, only char literals
		// TransformQuotesWithSemantics will later convert back to char where appropriate
		return TransformQuotes(code);
	}

	/// <summary>
	/// Transforms word-based operator aliases to C# operators.
	/// Allows writing readable expressions without XML escaping.
	/// </summary>
	static string TransformOperatorAliases(string code)
	{
		// Replace word-based aliases with C# operators (case-insensitive, with spaces)
		var result = code;

		// Logical operators
		result = ReplaceWordOperator(result, " AND ", " && ");
		result = ReplaceWordOperator(result, " OR ", " || ");

		// Comparison operators (must do multi-char first to avoid partial replacements)
		result = ReplaceWordOperator(result, " LTE ", " <= ");
		result = ReplaceWordOperator(result, " GTE ", " >= ");
		result = ReplaceWordOperator(result, " LT ", " < ");
		result = ReplaceWordOperator(result, " GT ", " > ");

		return result;
	}

	/// <summary>
	/// Walks the expression and parenthesizes any ternary/conditional expression that
	/// appears directly inside a string interpolation hole. Without parentheses, the C#
	/// compiler treats the first top-level ':' inside a hole as the start of a format
	/// specifier, so `$"{cond ? a : b}"` fails to compile (CS8361/CS8076). Wrapping the
	/// conditional — `$"{(cond ? a : b)}"` — keeps the ':' part of the expression.
	/// Format specifiers (`{value:F2}`), null-coalescing (`??`), null-conditional (`?.`,
	/// `?[`) and ternaries outside interpolation holes are left untouched.
	/// </summary>
	static string ParenthesizeInterpolationTernaries(string code)
		=> TransformInterpolationHoles(code, WrapHoleTernary);

	static string ParenthesizeInterpolationGlobalAliases(string code)
	{
		if (code.IndexOf('$') < 0 || code.IndexOf("::", StringComparison.Ordinal) < 0)
			return code;

		return TransformInterpolationHoles(code, WrapHoleGlobalAlias);
	}

	static string TransformInterpolationHoles(string code, Func<string, string> transformHole)
	{
		var result = new StringBuilder(code.Length);
		int i = 0;

		while (i < code.Length)
		{
			// Interpolated string: '$' immediately followed by a quote delimiter.
			if (code[i] == '$' && i + 1 < code.Length && (code[i + 1] == '\'' || code[i + 1] == '"'))
			{
				char delimiter = code[i + 1];
				result.Append(code[i]);     // $
				result.Append(code[i + 1]); // opening delimiter
				i += 2;

				// Walk the interpolated string body until the closing delimiter.
				while (i < code.Length)
				{
					char c = code[i];

					// Escape sequence in literal text - copy verbatim.
					if (c == '\\' && i + 1 < code.Length)
					{
						result.Append(code[i]);
						result.Append(code[i + 1]);
						i += 2;
						continue;
					}

					// Closing delimiter ends the interpolated string.
					if (c == delimiter)
					{
						result.Append(c);
						i++;
						break;
					}

					if (c == '{')
					{
						// Escaped brace '{{' is literal text, not a hole.
						if (i + 1 < code.Length && code[i + 1] == '{')
						{
							result.Append("{{");
							i += 2;
							continue;
						}

						// Capture the hole content up to its matching '}'.
						int holeStart = i + 1;
						int j = FindHoleEnd(code, holeStart);
						string holeContent = code.Substring(holeStart, j - holeStart);

						// Recurse so nested interpolated strings are handled too.
						holeContent = TransformInterpolationHoles(holeContent, transformHole);
						holeContent = transformHole(holeContent);

						result.Append('{');
						result.Append(holeContent);
						if (j < code.Length && code[j] == '}')
						{
							result.Append('}');
							i = j + 1;
						}
						else
						{
							// Unterminated hole - leave as-is.
							i = j;
						}
						continue;
					}

					if (c == '}')
					{
						// Escaped brace '}}'.
						if (i + 1 < code.Length && code[i + 1] == '}')
						{
							result.Append("}}");
							i += 2;
							continue;
						}
						result.Append(c);
						i++;
						continue;
					}

					result.Append(c);
					i++;
				}

				continue;
			}

			// Skip non-interpolated string literals so their contents are never
			// mistaken for interpolation structure.
			if (code[i] == '\'' || code[i] == '"')
			{
				char quote = code[i];
				result.Append(code[i]);
				i++;
				while (i < code.Length)
				{
					if (code[i] == '\\' && i + 1 < code.Length)
					{
						result.Append(code[i]);
						result.Append(code[i + 1]);
						i += 2;
						continue;
					}
					result.Append(code[i]);
					if (code[i] == quote)
					{
						i++;
						break;
					}
					i++;
				}
				continue;
			}

			result.Append(code[i]);
			i++;
		}

		return result.ToString();
	}

	/// <summary>
	/// Returns the index of the '}' that closes the interpolation hole starting at
	/// <paramref name="start"/>, tracking nested braces and skipping string literals.
	/// </summary>
	static int FindHoleEnd(string code, int start)
	{
		int depth = 1;
		int j = start;
		while (j < code.Length)
		{
			char c = code[j];
			if (c == '\\' && j + 1 < code.Length)
			{
				j += 2;
				continue;
			}
			if (c == '\'' || c == '"')
			{
				char quote = c;
				j++;
				while (j < code.Length)
				{
					if (code[j] == '\\' && j + 1 < code.Length)
					{
						j += 2;
						continue;
					}
					if (code[j] == quote)
					{
						j++;
						break;
					}
					j++;
				}
				continue;
			}
			if (c == '{')
			{
				depth++;
				j++;
				continue;
			}
			if (c == '}')
			{
				depth--;
				if (depth == 0)
					return j;
				j++;
				continue;
			}
			j++;
		}
		return j;
	}

	/// <summary>
	/// Wraps the content of an interpolation hole in parentheses when it contains a
	/// top-level ternary conditional. Detection skips string literals, nested
	/// parentheses/brackets/braces and the non-ternary '?' forms ('??', '?.', '?[').
	/// </summary>
	static string WrapHoleTernary(string holeContent)
		=> ContainsTopLevelTernary(holeContent) ? ParenthesizeTrimmed(holeContent) : holeContent;

	static string WrapHoleGlobalAlias(string holeContent)
	{
		if (!TryFindTopLevelGlobalAliasSplit(holeContent, out int clauseIndex))
			return holeContent;

		return clauseIndex >= 0
			? ParenthesizeTrimmed(holeContent.Substring(0, clauseIndex)) + holeContent.Substring(clauseIndex)
			: ParenthesizeTrimmed(holeContent);
	}

	static string ParenthesizeTrimmed(string expr)
	{
		// Preserve leading/trailing whitespace outside the parentheses for readability.
		int startTrim = 0;
		while (startTrim < expr.Length && char.IsWhiteSpace(expr[startTrim]))
			startTrim++;
		int endTrim = expr.Length;
		while (endTrim > startTrim && char.IsWhiteSpace(expr[endTrim - 1]))
			endTrim--;

		var leading = expr.Substring(0, startTrim);
		var inner = expr.Substring(startTrim, endTrim - startTrim);
		var trailing = expr.Substring(endTrim);

		return leading + "(" + inner + ")" + trailing;
	}

	static int SkipQuotedLiteral(string expr, int start)
	{
		char quote = expr[start];
		int i = start + 1;
		while (i < expr.Length)
		{
			if (expr[i] == '\\' && i + 1 < expr.Length)
			{
				i += 2;
				continue;
			}
			if (expr[i] == quote)
			{
				i++;
				break;
			}
			i++;
		}
		return i;
	}

	static bool TryFindTopLevelGlobalAliasSplit(string expr, out int clauseIndex)
	{
		clauseIndex = TryFindInterpolationClauseIndex(expr);
		int limit = clauseIndex >= 0 ? clauseIndex : expr.Length;

		int depth = 0;
		int i = 0;
		while (i < limit)
		{
			char c = expr[i];

			if (c == '\\' && i + 1 < limit)
			{
				i += 2;
				continue;
			}

			if (c == '\'' || c == '"')
			{
				i = SkipQuotedLiteral(expr, i);
				continue;
			}

			if (c == '(' || c == '[' || c == '{')
			{
				depth++;
				i++;
				continue;
			}

			if (c == ')' || c == ']' || c == '}')
			{
				if (depth > 0)
					depth--;
				i++;
				continue;
			}

			if (depth == 0 && c == ':' && i + 1 < limit && expr[i + 1] == ':')
			{
				return true;
			}

			i++;
		}

		return false;
	}

	static int TryFindInterpolationClauseIndex(string expr)
	{
		string masked = MaskAliasQualifiers(expr);
		int maxConsecutiveQuotes = CountMaxConsecutive(masked, '"');
		string quotes = new string('"', Math.Max(3, maxConsecutiveQuotes + 1));
		string prefix = "$" + quotes + "\n{";
		string wrapped = prefix + masked + "}\n" + quotes;

		var tree = CSharpSyntaxTree.ParseText(wrapped, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		var interpolation = root.DescendantNodes()
			.OfType<InterpolationSyntax>()
			.FirstOrDefault(n => n.OpenBraceToken.Span.End == prefix.Length);

		if (interpolation != null
			&& !interpolation.CloseBraceToken.IsMissing
			&& interpolation.CloseBraceToken.SpanStart == prefix.Length + masked.Length
			&& !interpolation.ContainsDiagnostics)
		{
			if (interpolation.AlignmentClause != null)
				return interpolation.AlignmentClause.SpanStart - prefix.Length;

			if (interpolation.FormatClause != null)
				return interpolation.FormatClause.SpanStart - prefix.Length;

			return -1;
		}

		return FindInterpolationClauseIndexFallback(expr);
	}

	static string MaskAliasQualifiers(string expr)
	{
		if (expr.IndexOf("::", StringComparison.Ordinal) < 0)
			return expr;

		var sb = new StringBuilder(expr.Length);
		int i = 0;
		while (i < expr.Length)
		{
			char c = expr[i];
			if (c == '\\' && i + 1 < expr.Length)
			{
				sb.Append(c);
				sb.Append(expr[i + 1]);
				i += 2;
				continue;
			}

			if (c == '\'' || c == '"')
			{
				int next = SkipQuotedLiteral(expr, i);
				sb.Append(expr, i, next - i);
				i = next;
				continue;
			}

			if (c == ':' && i + 1 < expr.Length && expr[i + 1] == ':')
			{
				// Preserve exact string length so syntax spans match original expr offsets.
				sb.Append(". ");
				i += 2;
				continue;
			}

			sb.Append(c);
			i++;
		}

		return sb.ToString();
	}

	static int CountMaxConsecutive(string text, char target)
	{
		int max = 0;
		int current = 0;
		foreach (char c in text)
		{
			if (c == target)
			{
				current++;
				if (current > max)
					max = current;
			}
			else
			{
				current = 0;
			}
		}

		return max;
	}

	static int FindInterpolationClauseIndexFallback(string expr)
	{
		int depth = 0;
		int i = 0;

		while (i < expr.Length)
		{
			char c = expr[i];

			if (c == '\\' && i + 1 < expr.Length)
			{
				i += 2;
				continue;
			}

			if (c == '\'' || c == '"')
			{
				i = SkipQuotedLiteral(expr, i);
				continue;
			}

			if (c == '(' || c == '[' || c == '{')
			{
				depth++;
				i++;
				continue;
			}

			if (c == ')' || c == ']' || c == '}')
			{
				if (depth > 0)
					depth--;
				i++;
				continue;
			}

			if (depth == 0)
			{
				if (c == '<' && TrySkipBalancedGenericArguments(expr, i, out int afterGenerics))
				{
					i = afterGenerics;
					continue;
				}

				if (c == ':')
				{
					if (i + 1 < expr.Length && expr[i + 1] == ':')
					{
						i += 2;
						continue;
					}

					return i;
				}

				if (c == ',')
					return i;
			}

			i++;
		}

		return -1;
	}

	static bool TrySkipBalancedGenericArguments(string expr, int start, out int afterGenerics)
	{
		afterGenerics = start;
		if (start >= expr.Length || expr[start] != '<')
			return false;

		// Skip shift and comparison operators (<<, <=).
		if (start + 1 < expr.Length && (expr[start + 1] == '<' || expr[start + 1] == '='))
			return false;

		int angleDepth = 0;
		int parenDepth = 0;
		int bracketDepth = 0;
		int i = start;

		while (i < expr.Length)
		{
			char c = expr[i];
			if (c == '(')
				parenDepth++;
			else if (c == ')')
			{
				if (parenDepth == 0)
					return false;
				parenDepth--;
			}
			else if (c == '[')
				bracketDepth++;
			else if (c == ']')
			{
				if (bracketDepth == 0)
					return false;
				bracketDepth--;
			}
			else if (parenDepth == 0 && bracketDepth == 0)
			{
				if (c == '<')
				{
					angleDepth++;
				}
				else if (c == '>')
				{
					angleDepth--;
					if (angleDepth == 0)
					{
						int next = SkipWhitespace(expr, i + 1);
						if (next >= expr.Length
							|| expr[next] == '(' || expr[next] == '.' || expr[next] == '['
							|| expr[next] == ',' || expr[next] == ':'
							|| expr[next] == ')' || expr[next] == ']' || expr[next] == '}')
						{
							afterGenerics = i + 1;
							return true;
						}
						return false;
					}
				}
				else if (!char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)
					&& c != '_' && c != '.' && c != ',' && c != ':' && c != '?' && c != '*')
				{
					return false;
				}
			}

			i++;
		}

		return false;
	}

	/// <summary>
	/// Determines whether the expression contains a ternary '?' at the top nesting level
	/// (outside strings, parentheses, brackets and braces). Returns false for '??', '?.'
	/// and '?[' which are not conditional expressions.
	/// </summary>
	static bool ContainsTopLevelTernary(string expr)
	{
		int depth = 0;
		int i = 0;
		while (i < expr.Length)
		{
			char c = expr[i];

			if (c == '\\' && i + 1 < expr.Length)
			{
				i += 2;
				continue;
			}

			if (c == '\'' || c == '"')
			{
				i = SkipQuotedLiteral(expr, i);
				continue;
			}

			if (c == '(' || c == '[' || c == '{')
			{
				depth++;
				i++;
				continue;
			}

			if (c == ')' || c == ']' || c == '}')
			{
				if (depth > 0)
					depth--;
				i++;
				continue;
			}

			if (c == '?' && depth == 0)
			{
				char next = i + 1 < expr.Length ? expr[i + 1] : '\0';
				// Skip '??' (null-coalescing), '?.' and '?[' (null-conditional).
				if (next == '?')
				{
					i += 2;
					continue;
				}
				if (next == '.' || next == '[')
				{
					i++;
					continue;
				}
				return true;
			}

			i++;
		}

		return false;
	}

	/// <summary>
	/// Replaces a word operator case-insensitively, but only outside of string literals.
	/// </summary>
	static string ReplaceWordOperator(string code, string word, string replacement)
	{
		var result = new StringBuilder();
		int i = 0;

		while (i < code.Length)
		{
			// Skip string literals (single or double quoted)
			if (code[i] == '\'' || code[i] == '"')
			{
				var quote = code[i];
				result.Append(code[i]);
				i++;
				while (i < code.Length && code[i] != quote)
				{
					if (code[i] == '\\' && i + 1 < code.Length)
					{
						result.Append(code[i]);
						result.Append(code[i + 1]);
						i += 2;
					}
					else
					{
						result.Append(code[i]);
						i++;
					}
				}
				if (i < code.Length)
				{
					result.Append(code[i]);
					i++;
				}
				continue;
			}

			// Check for word match (case-insensitive)
			if (i + word.Length <= code.Length)
			{
				var segment = code.Substring(i, word.Length);
				if (segment.Equals(word, StringComparison.OrdinalIgnoreCase))
				{
					result.Append(replacement);
					i += word.Length;
					continue;
				}
			}

			result.Append(code[i]);
			i++;
		}

		return result.ToString();
	}

	/// <summary>
	/// Transforms ALL single-quoted strings to double-quoted strings in C# code.
	/// This is the safe default - use TransformQuotesWithSemantics when you have type context
	/// to preserve char literals where appropriate.
	/// </summary>
	static string TransformQuotes(string code)
	{
		var result = new StringBuilder(code.Length);
		int i = 0;

		while (i < code.Length)
		{
			if (code[i] == '\'')
			{
				// Find the end of the single-quoted string
				int start = i;
				i++; // Skip opening quote

				var content = new StringBuilder();
				while (i < code.Length)
				{
					if (code[i] == '\\' && i + 1 < code.Length)
					{
						// Escape sequence
						char escaped = code[i + 1];
						if (escaped == '\'')
						{
							// \' in single-quoted -> just ' in double-quoted
							content.Append('\'');
						}
						else if (escaped == '"')
						{
							// \" stays as \" in double-quoted (already escaped)
							content.Append("\\\"");
						}
						else
						{
							// Other escapes (\n, \t, \\, etc.) - keep as-is
							content.Append(code[i]);
							content.Append(code[i + 1]);
						}
						i += 2;
					}
					else if (code[i] == '\'')
					{
						// End of string
						break;
					}
					else
					{
						content.Append(code[i]);
						i++;
					}
				}

				if (i < code.Length && code[i] == '\'')
				{
					i++; // Skip closing quote

					var contentStr = content.ToString();

					// Always convert to string literal (double quotes)
					// Use TransformQuotesWithSemantics when semantic context is available
					// to preserve char literals where the target type expects char
					var sb = new StringBuilder();
					for (int j = 0; j < contentStr.Length; j++)
					{
						if (contentStr[j] == '"')
						{
							// Count preceding backslashes to determine if quote is escaped
							// An odd number means the quote is escaped, even means it's not
							// e.g., \" = escaped quote, \\" = escaped backslash + unescaped quote
							int backslashCount = 0;
							for (int k = j - 1; k >= 0 && contentStr[k] == '\\'; k--)
							{
								backslashCount++;
							}

							if (backslashCount % 2 == 1)
							{
								// Odd backslashes: quote is already escaped
								sb.Append('"');
							}
							else
							{
								// Even backslashes (including 0): quote is not escaped
								sb.Append("\\\"");
							}
						}
						else
						{
							sb.Append(contentStr[j]);
						}
					}
					result.Append('"');
					result.Append(sb);
					result.Append('"');
				}
				else
				{
					// Unterminated string - copy as-is
					result.Append(code.Substring(start, i - start));
				}
			}
			else
			{
				result.Append(code[i]);
				i++;
			}
		}

		return result.ToString();
	}

	/// <summary>
	/// Analyzes double-quoted string literals and converts single-character strings to char literals
	/// where the target type expects a char (e.g., method parameters).
	/// </summary>
	/// <param name="code">The C# expression code with double-quoted strings (after TransformQuotes)</param>
	/// <param name="compilation">The compilation for type lookups</param>
	/// <param name="contextTypes">Types to search for method signatures (dataType, thisType, etc.)</param>
	/// <returns>Code with single-char strings converted to char literals where appropriate</returns>
	public static string TransformQuotesWithSemantics(string code, Compilation compilation, params ITypeSymbol?[] contextTypes)
	{
		// Parse the expression to find string literals
		var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();

		// Find all string literals that are single characters (candidates for char conversion)
		var stringLiterals = root.DescendantNodes()
			.OfType<LiteralExpressionSyntax>()
			.Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
			.Where(l => IsSingleCharStringLiteral(l))
			.ToList();

		if (stringLiterals.Count == 0)
			return code; // No single-char strings to potentially convert

		// Collect positions that need transformation (string -> char)
		var transformPositions = new List<(int start, int length, string replacement)>();

		foreach (var literal in stringLiterals)
		{
			var expectedType = DetermineExpectedType(literal, compilation, contextTypes);

			// If expected type is char, convert back to char literal
			bool shouldBeChar = expectedType?.SpecialType == SpecialType.System_Char;

			if (shouldBeChar)
			{
				// Get the string content and create a char literal
				var stringText = literal.Token.ValueText;
				var charLiteral = $"'{EscapeForChar(stringText)}'";
				transformPositions.Add((literal.SpanStart, literal.Span.Length, charLiteral));
			}
		}

		// Apply transformations in reverse order to preserve positions
		var result = code;
		foreach (var (start, length, replacement) in transformPositions.OrderByDescending(t => t.start))
		{
			result = result.Substring(0, start) + replacement + result.Substring(start + length);
		}

		return result;
	}

	/// <summary>
	/// Checks if a string literal is a single character (candidate for char conversion).
	/// </summary>
	static bool IsSingleCharStringLiteral(LiteralExpressionSyntax literal)
	{
		var valueText = literal.Token.ValueText;
		return valueText.Length == 1;
	}

	/// <summary>
	/// Escapes a character for use in a C# char literal.
	/// </summary>
	static string EscapeForChar(string value)
	{
		if (value.Length != 1)
			return value;

		return value[0] switch
		{
			'\'' => "\\'",
			'\\' => "\\\\",
			'\n' => "\\n",
			'\r' => "\\r",
			'\t' => "\\t",
			'\0' => "\\0",
			_ => value
		};
	}

	/// <summary>
	/// Determines the expected type for a literal based on its syntactic context.
	/// </summary>
	static ITypeSymbol? DetermineExpectedType(LiteralExpressionSyntax literal, Compilation compilation, ITypeSymbol?[] contextTypes)
	{
		// Walk up to find the context
		var parent = literal.Parent;

		while (parent != null)
		{
			switch (parent)
			{
				// Method argument: MethodName('x') - check method signature
				case ArgumentSyntax arg when arg.Parent is ArgumentListSyntax argList && argList.Parent is InvocationExpressionSyntax invocation:
					var argIndex = argList.Arguments.IndexOf(arg);
					var methodType = FindMethodParameterType(invocation, argIndex, compilation, contextTypes);
					if (methodType != null)
						return methodType;
					break;

				// Ternary expression: cond ? 'a' : 'b' - check the other branch or parent context
				case ConditionalExpressionSyntax conditional:
					// If one branch is already a string, the other should be too
					var otherBranch = literal.Parent == conditional.WhenTrue ? conditional.WhenFalse : conditional.WhenTrue;
					if (otherBranch is LiteralExpressionSyntax otherLiteral && otherLiteral.IsKind(SyntaxKind.StringLiteralExpression))
						return compilation.GetSpecialType(SpecialType.System_String);
					// Continue walking up
					break;

				// Null coalescing: value ?? 'default' - check left side type
				case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
					// The right side should match the left side type
					// For simplicity, assume string if the literal is multi-char or we can't determine
					break;

				// Assignment or return - we'd need more context
				case AssignmentExpressionSyntax:
				case ReturnStatementSyntax:
					break;
			}

			parent = parent.Parent;
		}

		return null; // Unknown context
	}

	/// <summary>
	/// Finds the parameter type for a method invocation at a given argument index.
	/// </summary>
	static ITypeSymbol? FindMethodParameterType(InvocationExpressionSyntax invocation, int argIndex, Compilation compilation, ITypeSymbol?[] contextTypes)
	{
		// Get the method name
		string? methodName = invocation.Expression switch
		{
			IdentifierNameSyntax id => id.Identifier.Text,
			MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
			_ => null
		};

		if (methodName == null)
			return null;

		// Search for the method in context types
		foreach (var type in contextTypes.Where(t => t != null))
		{
			var method = FindMethod(type!, methodName, invocation.ArgumentList.Arguments.Count);
			if (method != null && argIndex < method.Parameters.Length)
			{
				return method.Parameters[argIndex].Type;
			}
		}

		// Check well-known types (string methods like Format, etc.)
		var stringType = compilation.GetSpecialType(SpecialType.System_String);
		var stringMethod = FindMethod(stringType, methodName, invocation.ArgumentList.Arguments.Count);
		if (stringMethod != null && argIndex < stringMethod.Parameters.Length)
		{
			return stringMethod.Parameters[argIndex].Type;
		}

		return null;
	}

	/// <summary>
	/// Finds a method by name and parameter count on a type.
	/// </summary>
	static IMethodSymbol? FindMethod(ITypeSymbol type, string methodName, int paramCount)
	{
		var currentType = type;
		while (currentType != null)
		{
			foreach (var member in currentType.GetMembers(methodName))
			{
				if (member is IMethodSymbol method && method.Parameters.Length == paramCount)
					return method;
			}
			currentType = currentType.BaseType;
		}
		return null;
	}

	/// <summary>
	/// Escapes a string for use in a C# string literal.
	/// </summary>
	internal static string EscapeForString(string value)
	{
		var sb = new StringBuilder();
		foreach (var c in value)
		{
			switch (c)
			{
				case '"':
					sb.Append("\\\"");
					break;
				case '\\':
					sb.Append("\\\\");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				default:
					sb.Append(c);
					break;
			}
		}
		return sb.ToString();
	}

	// Pattern to match lambda expressions: () => ... or (params) => ...
	// Note: async lambdas are NOT supported
	static readonly Regex LambdaPattern = new Regex(
		@"^\s*\([^)]*\)\s*=>",
		RegexOptions.Compiled);

	// Pattern to detect async lambda (to reject it)
	static readonly Regex AsyncLambdaPattern = new Regex(
		@"^\s*async\s+\([^)]*\)\s*=>",
		RegexOptions.Compiled);

	/// <summary>
	/// Checks if the expression code is an async lambda expression.
	/// </summary>
	public static bool IsAsyncLambdaExpression(string code)
	{
		return AsyncLambdaPattern.IsMatch(code);
	}

	/// <summary>
	/// Checks if the expression code is a lambda expression.
	/// </summary>
	public static bool IsLambdaExpression(string code)
	{
		return LambdaPattern.IsMatch(code);
	}

	/// <summary>
	/// Checks if a markup string contains a lambda event handler.
	/// E.g., {() => DoSomething()} or {(s, e) => Log(s)}
	/// </summary>
	public static bool IsLambdaEventHandler(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return false;

		var trimmed = value!.Trim();
		if (trimmed.Length < 5 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
			return false;

		// Extract inner content
		string inner;
		if (trimmed.Length > 3 && trimmed[1] == '=')
		{
			// Explicit: {= () => ...}
			inner = trimmed.Substring(2, trimmed.Length - 3).Trim();
		}
		else
		{
			// Implicit: {() => ...}
			inner = trimmed.Substring(1, trimmed.Length - 2).Trim();
		}

		return IsLambdaExpression(inner);
	}

	/// <summary>
	/// Checks if a markup string contains an async lambda event handler.
	/// Async lambdas are not supported.
	/// </summary>
	public static bool IsAsyncLambdaEventHandler(string? value)
	{
		if (string.IsNullOrEmpty(value))
			return false;

		var trimmed = value!.Trim();
		if (trimmed.Length < 5 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
			return false;

		// Extract inner content
		string inner;
		if (trimmed.Length > 3 && trimmed[1] == '=')
		{
			// Explicit: {= async () => ...}
			inner = trimmed.Substring(2, trimmed.Length - 3).Trim();
		}
		else
		{
			// Implicit: {async () => ...}
			inner = trimmed.Substring(1, trimmed.Length - 2).Trim();
		}

		return IsAsyncLambdaExpression(inner);
	}

	/// <summary>
	/// Extracts the lambda code from a markup value and transforms it for C# emission.
	/// </summary>
	public static string GetLambdaCode(string value)
	{
		// Use same extraction as expressions
		return GetExpressionCode(value);
	}

	// Pattern to match xmlns prefix usage: prefix:TypeName (not at start of expression where it's a markup ext)
	static readonly Regex XmlnsPrefixPattern = new Regex(
		@"(?<![a-zA-Z0-9_])([a-zA-Z_]\w*):([A-Z]\w*)",
		RegexOptions.Compiled);

	/// <summary>
	/// Resolves xmlns prefixes in expression code to fully qualified CLR type names.
	/// E.g., "local:ExprHelper.GetValue()" -> "global::Microsoft.Maui.Controls.Xaml.UnitTests.ExprHelper.GetValue()"
	/// </summary>
	public static string ResolveXmlnsPrefixes(string code, IXmlNamespaceResolver nsResolver, SourceGenContext context, ITypeSymbol? dataTypeSymbol = null, ITypeSymbol? rootType = null)
		=> ResolveXmlnsPrefixes(code, nsResolver, context.Compilation, context.XmlnsCache, context.TypeCache, dataTypeSymbol, rootType ?? context.RootType);

	/// <summary>
	/// Resolves xmlns prefixes in expression code to fully qualified CLR type names.
	/// Falls back to global::ClrNamespace.TypeName when the XAML namespace maps to a CLR namespace
	/// even if the type is not yet present in <paramref name="compilation"/> (e.g., emitted by another source generator).
	/// </summary>
	public static string ResolveXmlnsPrefixes(
		string code,
		IXmlNamespaceResolver nsResolver,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		ITypeSymbol? dataTypeSymbol = null,
		ITypeSymbol? rootType = null)
	{
		// Re-run after parenthesizing global:: interpolation expressions so Roslyn re-parses
		// nested calls/named arguments that initially fell inside an unparenthesized format clause.
		const int maxPasses = 4;
		var current = code;
		for (var pass = 0; pass < maxPasses; pass++)
		{
			var next = ResolveXmlnsPrefixesPass(
				current,
				nsResolver,
				compilation,
				xmlnsCache,
				typeCache,
				dataTypeSymbol,
				rootType);
			next = ParenthesizeInterpolationGlobalAliases(next);
			if (string.Equals(next, current, StringComparison.Ordinal))
				break;
			current = next;
		}

		return current;
	}

	static string ResolveXmlnsPrefixesPass(
		string code,
		IXmlNamespaceResolver nsResolver,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType)
	{
		return ReplaceOutsideLiteralSegments(code, XmlnsPrefixPattern, (match, overlapsFormatClause, literalSpans) =>
		{
			var prefix = match.Groups[1].Value;
			var typeName = match.Groups[2].Value;

			// Skip the "x" prefix (x:Static, etc.) and known C# patterns
			if (prefix == "x" || prefix == "global")
				return match.Value;

			var namespaceUri = nsResolver.LookupNamespace(prefix);
			if (namespaceUri == null)
				return match.Value;

			if (IsPrecededByMemberAccess(code, match.Index))
				return match.Value;

			// Inside an interpolation hole, Roslyn parses "ident:Text" as a format specifier.
			// Never rewrite matches wholly inside the format string token (e.g. "{Value:res:Labels.Gold}").
			// For matches spanning the interpolation ':' boundary, keep format specifiers untouched.
			if (overlapsFormatClause &&
				(!SpansFormatClauseStartBoundary(match.Index, match.Length, literalSpans)
				 || !IsFollowedByMemberAccess(code, match.Index + match.Length)
				 || HasMemberInExpressionScope(code, match.Index, prefix, dataTypeSymbol, rootType, compilation)))
			{
				return match.Value;
			}

			// Preserve ternary conditional ':' when there is no whitespace around ':' (e.g. "IsVip ? Gold:Standard")
			// and tuple element labels whose value root is in scope (e.g. "(local:Name, Count)").
			if (!overlapsFormatClause &&
				(IsConditionalSeparatorColon(code, match)
				 || IsTupleElementLabelSyntax(code, match, compilation, dataTypeSymbol, rootType)))
			{
				return match.Value;
			}

			// Resolve to CLR type
			var xmlType = new XmlType(namespaceUri, typeName, null);
			var hasResolvedType = xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var typeSymbol);

			// Preserve C# named arguments (e.g. "Formatter.Format(local:Name)", "Formatter.Format(local:User.Name)").
			if (!overlapsFormatClause && IsNamedArgumentSyntax(
				code,
				match,
				literalSpans,
				nsResolver,
				compilation,
				xmlnsCache,
				typeCache,
				dataTypeSymbol,
				rootType,
				hasResolvedType))
			{
				return match.Value;
			}

			if (hasResolvedType)
			{
				return typeSymbol!.ToFQDisplayString();
			}

			var clrNamespace = namespaceUri.GetClrNamespace();
			if (string.IsNullOrEmpty(clrNamespace) && xmlnsCache.ClrNamespacesForXmlns.TryGetValue(namespaceUri, out var clrNamespaces) && clrNamespaces.Count == 1)
			{
				clrNamespace = clrNamespaces[0];
			}

			if (!string.IsNullOrEmpty(clrNamespace))
			{
				return $"global::{clrNamespace}.{typeName}";
			}

			return match.Value;
		}, allowInterpolationFormatClause: true);
	}

	static bool SpansFormatClauseStartBoundary(
		int start,
		int length,
		List<(int Start, int End, bool IsFormatClause)> literalSpans)
	{
		var end = start + length;
		foreach (var span in literalSpans)
		{
			if (span.IsFormatClause && start < span.Start && end > span.Start)
				return true;
		}

		return false;
	}

	// Pattern to match attached bindable property syntax: target.(Type.Property) or standalone (Type.Property)
	static readonly Regex AttachedPropertyPattern = new Regex(
		@"(?:(?<target>[A-Za-z_]\w*)\.)?\((?<member>(?:global::)?[A-Za-z_]\w*(?:\.[A-Za-z_]\w*)+)\)",
		RegexOptions.Compiled);

	public static bool TryGetStandaloneAttachedProperty(string code, Compilation compilation, out string attachedProperty)
	{
		foreach (Match match in MatchesOutsideLiteralSegments(code, AttachedPropertyPattern))
		{
			if (!match.Groups["target"].Success
				&& IsAttachedProperty(match.Groups["member"].Value, compilation))
			{
				attachedProperty = match.Value;
				return true;
			}
		}

		attachedProperty = string.Empty;
		return false;
	}

	/// <summary>
	/// Transforms attached bindable property syntax to static Get method calls.
	/// E.g., "gridChild.(Grid.Row)" → "Grid.GetRow(gridChild)"
	/// E.g., "this.(Grid.Row)" → "Grid.GetRow(this)"
	/// </summary>
	public static string TransformAttachedProperties(string code)
	{
		return ReplaceOutsideLiteralSegments(code, AttachedPropertyPattern, match =>
		{
			var target = match.Groups["target"].Value; // "gridChild", "this", or empty
			var member = match.Groups["member"].Value; // "Grid.Row" or "global::Namespace.Grid.Row"

			if (string.IsNullOrEmpty(target))
				return match.Value;

			if (!TrySplitAttachedProperty(member, out var typeName, out var propertyName))
				return match.Value;

			return $"{typeName}.Get{propertyName}({target})";
		});
	}

	static bool TrySplitAttachedProperty(string member, out string typeName, out string propertyName)
	{
		typeName = string.Empty;
		propertyName = string.Empty;

		var lastDot = member.LastIndexOf('.');
		if (lastDot <= 0 || lastDot == member.Length - 1)
			return false;

		typeName = member.Substring(0, lastDot);
		propertyName = member.Substring(lastDot + 1);

		if (!IsIdentifier(propertyName) || !LooksLikeTypeName(typeName))
			return false;

		return true;
	}

	static bool IsAttachedProperty(string member, Compilation compilation)
	{
		if (!TrySplitAttachedProperty(member, out var typeName, out var propertyName))
			return false;

		foreach (var type in ResolveTypeSymbols(typeName, compilation))
		{
			if (HasAttachedProperty(type, propertyName, compilation))
				return true;
		}

		return false;
	}

	static IEnumerable<INamedTypeSymbol> ResolveTypeSymbols(string typeName, Compilation compilation)
	{
		var metadataName = typeName.StartsWith("global::", StringComparison.Ordinal)
			? typeName.Substring("global::".Length)
			: typeName;
		var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
		var foundDirectMatch = false;

		foreach (var type in compilation.GetTypesByMetadataName(metadataName).OfType<INamedTypeSymbol>())
		{
			foundDirectMatch = true;
			if (seen.Add(type))
				yield return type;
		}

		if (foundDirectMatch)
			yield break;

		foreach (var type in FindTypes(compilation.GlobalNamespace, metadataName))
		{
			if (seen.Add(type))
				yield return type;
		}
	}

	static IEnumerable<INamedTypeSymbol> FindTypes(INamespaceSymbol namespaceSymbol, string metadataName)
	{
		foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
		{
			foreach (var type in FindTypes(nestedNamespace, metadataName))
				yield return type;
		}

		foreach (var type in namespaceSymbol.GetTypeMembers())
		{
			foreach (var candidate in FindTypes(type, metadataName))
				yield return candidate;
		}
	}

	static IEnumerable<INamedTypeSymbol> FindTypes(INamedTypeSymbol typeSymbol, string metadataName)
	{
		if (typeSymbol.Name == metadataName
			|| typeSymbol.ToFQDisplayString() == $"global::{metadataName}")
		{
			yield return typeSymbol;
		}

		foreach (var nestedType in typeSymbol.GetTypeMembers())
		{
			foreach (var candidate in FindTypes(nestedType, metadataName))
				yield return candidate;
		}
	}

	static bool HasAttachedProperty(INamedTypeSymbol type, string propertyName, Compilation compilation)
	{
		var bindablePropertyType = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableProperty");
		var bindablePropertyName = $"{propertyName}Property";

		foreach (var member in type.GetMembers(bindablePropertyName))
		{
			if (member is IFieldSymbol field
				&& field.IsStatic
				&& (bindablePropertyType is null || SymbolEqualityComparer.Default.Equals(field.Type, bindablePropertyType)))
			{
				return true;
			}
		}

		var getterName = $"Get{propertyName}";
		foreach (var member in type.GetMembers(getterName))
		{
			if (member is IMethodSymbol method
				&& method.IsStatic
				&& method.Parameters.Length == 1
				&& method.ReturnsVoid == false)
			{
				return true;
			}
		}

		return false;
	}

	static bool LooksLikeTypeName(string typeName)
	{
		var candidate = typeName.StartsWith("global::", StringComparison.Ordinal)
			? typeName.Substring("global::".Length)
			: typeName;

		var lastDot = candidate.LastIndexOf('.');
		var typeSegment = lastDot >= 0 ? candidate.Substring(lastDot + 1) : candidate;

		return typeSegment.Length > 0 && char.IsUpper(typeSegment[0]);
	}

	static bool IsIdentifier(string value)
	{
		if (string.IsNullOrEmpty(value) || !(char.IsLetter(value[0]) || value[0] == '_'))
			return false;

		for (var i = 1; i < value.Length; i++)
		{
			if (!(char.IsLetterOrDigit(value[i]) || value[i] == '_'))
				return false;
		}

		return true;
	}

	static int SkipWhitespace(string text, int index)
	{
		while (index < text.Length && char.IsWhiteSpace(text[index]))
			index++;
		return index;
	}

	static bool IsPrecededByMemberAccess(string code, int index)
		=> TrySkipWhitespaceBackwards(code, index, out char prevChar) && prevChar == '.';

	static bool IsFollowedByMemberAccess(string code, int index)
	{
		if (index < code.Length && code[index] == '<' && TrySkipBalancedGenericArguments(code, index, out int afterGenerics))
			index = SkipWhitespace(code, afterGenerics);

		return index + 1 < code.Length
			&& code[index] == '.'
			&& (char.IsLetter(code[index + 1]) || code[index + 1] == '_');
	}

	static bool HasMemberInExpressionScope(
		string code,
		int matchIndex,
		string identifier,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType,
		Compilation compilation,
		bool includeMethods = false)
	{
		if (MemberResolver.HasMember(dataTypeSymbol, identifier, includeMethods: includeMethods, compilation)
			|| MemberResolver.HasMember(rootType, identifier, includeMethods: includeMethods, compilation))
		{
			return true;
		}

		return HasEnclosingLocalOrParameter(code, matchIndex, identifier);
	}

	static bool HasExpressionRootInScope(
		string code,
		int matchIndex,
		string identifier,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType,
		Compilation compilation)
	{
		return HasMemberInExpressionScope(code, matchIndex, identifier, dataTypeSymbol, rootType, compilation, includeMethods: true)
			|| HasNamespaceOrTypeInCompilationScope(identifier, rootType, compilation);
	}

	static bool HasNamespaceOrTypeInCompilationScope(string identifier, ITypeSymbol? rootType, Compilation compilation)
	{
		for (var ns = rootType?.ContainingNamespace; ns != null && !ns.IsGlobalNamespace; ns = ns.ContainingNamespace)
		{
			if (ns.GetMembers(identifier).Any())
				return true;
		}

		if (compilation.GlobalNamespace.GetMembers(identifier).Any())
			return true;

		if (compilation is CSharpCompilation csharpCompilation)
		{
			foreach (var defaultUsing in csharpCompilation.Options.Usings)
			{
				if (ResolveNamespaceSymbol(compilation.GlobalNamespace, defaultUsing) is INamespaceSymbol usingNamespace
					&& usingNamespace.GetMembers(identifier).Any())
				{
					return true;
				}
			}
		}

		foreach (var tree in compilation.SyntaxTrees)
		{
			if (tree.GetRoot() is not CompilationUnitSyntax compilationUnit)
				continue;

			SemanticModel? semanticModel = null;
			foreach (var usingDirective in compilationUnit.Usings)
			{
				if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.None))
					continue;

				if (usingDirective.Alias is not null)
				{
					if (string.Equals(usingDirective.Alias.Name.Identifier.ValueText, identifier, StringComparison.Ordinal))
						return true;
					continue;
				}

				if (usingDirective.NamespaceOrType is null)
					continue;

				semanticModel ??= compilation.GetSemanticModel(tree);
				var targetSymbol = semanticModel.GetSymbolInfo(usingDirective.NamespaceOrType).Symbol;
				if (!usingDirective.StaticKeyword.IsKind(SyntaxKind.None))
				{
					if (targetSymbol is INamedTypeSymbol staticTypeSymbol
						&& HasStaticOrNestedMember(staticTypeSymbol, identifier))
					{
						return true;
					}
					continue;
				}

				var importedNamespace = targetSymbol as INamespaceSymbol
					?? ResolveNamespaceSymbol(compilation.GlobalNamespace, usingDirective.NamespaceOrType.ToString());
				if (importedNamespace is not null && importedNamespace.GetMembers(identifier).Any())
					return true;
			}
		}

		return false;
	}

	static INamespaceSymbol? ResolveNamespaceSymbol(INamespaceSymbol globalNamespace, string namespaceName)
	{
		if (string.IsNullOrWhiteSpace(namespaceName))
			return null;

		var trimmed = namespaceName.Trim();
		if (trimmed.StartsWith("global::", StringComparison.Ordinal))
			trimmed = trimmed.Substring("global::".Length);

		INamespaceSymbol? current = globalNamespace;
		foreach (var part in trimmed.Split('.'))
		{
			var segment = part.Trim();
			if (string.IsNullOrEmpty(segment))
				return null;

			current = current.GetMembers(segment).OfType<INamespaceSymbol>().FirstOrDefault();
			if (current is null)
				return null;
		}

		return current;
	}

	static bool HasStaticOrNestedMember(INamedTypeSymbol typeSymbol, string identifier)
	{
		for (var current = typeSymbol; current is not null; current = current.BaseType)
		{
			if (current.GetMembers(identifier).Any(m => m.IsStatic || m is ITypeSymbol))
				return true;
		}

		return false;
	}

	static bool IsConditionalSeparatorColon(string code, Match match)
	{
		var colonIndex = match.Index + match.Groups[1].Length;
		if (colonIndex < 0 || colonIndex >= code.Length || code[colonIndex] != ':')
			return false;

		var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		var token = root.FindToken(colonIndex);
		if (!token.IsKind(SyntaxKind.ColonToken) || token.SpanStart != colonIndex)
			return false;

		if (token.Parent is not ConditionalExpressionSyntax)
			return false;

		var replaced = string.Concat(
			code.Substring(0, match.Index),
			"__x",
			code.Substring(match.Index + match.Length));
		var replacedTree = CSharpSyntaxTree.ParseText(replaced, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var replacedRoot = replacedTree.GetRoot();
		if (replacedRoot.FullSpan.Length == 0)
			return false;

		var replacedToken = replacedRoot.FindToken(Math.Min(match.Index, replacedRoot.FullSpan.End - 1));
		for (var node = replacedToken.Parent; node != null; node = node.Parent)
		{
			if (node is ConditionalExpressionSyntax replacedConditional)
				return replacedConditional.ColonToken.IsMissing;
		}

		return false;
	}

	static bool IsTupleElementLabelSyntax(
		string code,
		Match match,
		Compilation compilation,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType)
	{
		var colonIndex = match.Index + match.Groups[1].Length;
		if (colonIndex < 0 || colonIndex >= code.Length || code[colonIndex] != ':')
			return false;

		var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		var token = root.FindToken(colonIndex);
		if (!token.IsKind(SyntaxKind.ColonToken) || token.SpanStart != colonIndex)
			return false;

		if (token.Parent is not NameColonSyntax nameColon
			|| nameColon.Parent is not ArgumentSyntax argument
			|| argument.Parent is not TupleExpressionSyntax)
		{
			return false;
		}

		var valueStart = colonIndex + 1;
		return HasExpressionRootInScope(code, valueStart, match.Groups[2].Value, dataTypeSymbol, rootType, compilation);
	}

	static bool HasEnclosingLocalOrParameter(string code, int position, string identifier)
	{
		var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		if (root.FullSpan.Length == 0)
			return false;

		var safePosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var node = root.FindToken(safePosition).Parent;
		while (node != null)
		{
			switch (node)
			{
				case SimpleLambdaExpressionSyntax simpleLambda
					when simpleLambda.Parameter.Identifier.ValueText == identifier:
					return true;
				case ParenthesizedLambdaExpressionSyntax parenthesizedLambda
					when parenthesizedLambda.ParameterList.Parameters.Any(p => p.Identifier.ValueText == identifier):
					return true;
				case AnonymousMethodExpressionSyntax anonymousMethod
					when anonymousMethod.ParameterList?.Parameters.Any(p => p.Identifier.ValueText == identifier) == true:
					return true;
				case LocalFunctionStatementSyntax localFunction
					when localFunction.Identifier.ValueText == identifier
						|| localFunction.ParameterList.Parameters.Any(p => p.Identifier.ValueText == identifier):
					return true;
				case ForEachStatementSyntax forEachStmt
					when forEachStmt.Identifier.ValueText == identifier
						&& forEachStmt.Statement.Span.Contains(safePosition):
					return true;
				case ForEachVariableStatementSyntax forEachVarStmt
					when forEachVarStmt.Statement.Span.Contains(safePosition)
						&& HasShallowVariableDeclaration(forEachVarStmt.Variable, identifier, safePosition):
					return true;
				case ForStatementSyntax forStmt
					when forStmt.Declaration is not null
						&& HasShallowVariableDeclaration(forStmt.Declaration, identifier, safePosition):
					return true;
				case UsingStatementSyntax usingStmt
					when usingStmt.Statement.Span.Contains(safePosition)
						&& usingStmt.Declaration is not null
						&& HasShallowVariableDeclaration(usingStmt.Declaration, identifier, safePosition):
					return true;
				case CatchClauseSyntax catchClause
					when catchClause.Declaration?.Identifier.ValueText == identifier
						&& catchClause.Block.Span.Contains(safePosition):
					return true;
				case IfStatementSyntax ifStmt
					when ifStmt.Statement.Span.Contains(safePosition)
						&& HasShallowVariableDeclaration(ifStmt.Condition, identifier, safePosition):
					return true;
				case WhileStatementSyntax whileStmt
					when whileStmt.Statement.Span.Contains(safePosition)
						&& HasShallowVariableDeclaration(whileStmt.Condition, identifier, safePosition):
					return true;
				case SwitchExpressionArmSyntax switchArm
					when HasShallowVariableDeclaration(switchArm.Pattern, identifier, safePosition):
					return true;
				case SwitchSectionSyntax switchSection:
					foreach (var label in switchSection.Labels)
					{
						if (label is CasePatternSwitchLabelSyntax patternLabel
							&& HasShallowVariableDeclaration(patternLabel.Pattern, identifier, safePosition))
						{
							return true;
						}
					}

					foreach (var stmt in switchSection.Statements)
					{
						if (stmt.SpanStart >= safePosition)
							break;
						if (HasShallowStatementDeclaration(stmt, identifier, safePosition))
							return true;
					}
					break;
				case BinaryExpressionSyntax binaryExpr
					when binaryExpr.IsKind(SyntaxKind.LogicalAndExpression)
						&& binaryExpr.Right.Span.Contains(safePosition)
						&& HasShallowVariableDeclaration(binaryExpr.Left, identifier, safePosition):
					return true;
				case ConditionalExpressionSyntax condExpr
					when condExpr.WhenTrue.Span.Contains(safePosition)
						&& HasShallowVariableDeclaration(condExpr.Condition, identifier, safePosition):
					return true;
				case BlockSyntax block:
					foreach (var stmt in block.Statements)
					{
						if (stmt is LocalFunctionStatementSyntax localFn
							&& localFn.Identifier.ValueText == identifier)
						{
							return true;
						}

						if (stmt.SpanStart < safePosition
							&& HasShallowStatementDeclaration(stmt, identifier, safePosition))
						{
							return true;
						}
					}
					break;
			}

			node = node.Parent;
		}

		return false;
	}

	static bool HasShallowStatementDeclaration(StatementSyntax stmt, string identifier, int beforePosition)
	{
		if (stmt is BlockSyntax
			or LocalFunctionStatementSyntax
			or ForStatementSyntax
			or ForEachStatementSyntax
			or ForEachVariableStatementSyntax
			or UsingStatementSyntax
			or WhileStatementSyntax
			or DoStatementSyntax
			or LockStatementSyntax
			or FixedStatementSyntax
			or CheckedStatementSyntax
			or UnsafeStatementSyntax)
		{
			return false;
		}

		return HasShallowVariableDeclaration(stmt, identifier, beforePosition, rootNode: stmt);
	}

	static bool HasShallowVariableDeclaration(SyntaxNode node, string identifier, int beforePosition, SyntaxNode? rootNode = null)
	{
		rootNode ??= node;
		foreach (var desc in node.DescendantNodesAndSelf(n =>
		{
			if (n == rootNode)
				return true;

			return n is not (BlockSyntax
				or StatementSyntax
				or ElseClauseSyntax
				or CatchClauseSyntax
				or SwitchSectionSyntax
				or SwitchExpressionArmSyntax
				or LambdaExpressionSyntax
				or AnonymousMethodExpressionSyntax);
		}))
		{
			switch (desc)
			{
				case VariableDeclaratorSyntax vd
					when vd.SpanStart < beforePosition
						&& string.Equals(vd.Identifier.ValueText, identifier, StringComparison.Ordinal):
					return true;
				case SingleVariableDesignationSyntax svd
					when svd.SpanStart < beforePosition
						&& string.Equals(svd.Identifier.ValueText, identifier, StringComparison.Ordinal):
					return true;
			}
		}

		return false;
	}

	static bool IsNamedArgumentSyntax(
		string code,
		Match match,
		List<(int Start, int End, bool IsFormatClause)> literalSpans,
		IXmlNamespaceResolver nsResolver,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType,
		bool hasResolvedType)
	{
		if (!TryFindEnclosingArgumentListOpener(code, match.Index, literalSpans, out int openIndex, out bool isIndexer))
			return false;

		var prefix = match.Groups[1].Value;
		var typeName = match.Groups[2].Value;
		var isFollowedByMemberAccess = IsFollowedByMemberAccess(code, match.Index + match.Length);

		if (openIndex == SkipWhitespaceBackwardsIndex(code, match.Index - 1) && !isIndexer)
		{
			// Inside "(...)", distinguish casts like "(missing:Box)x" from invocation arguments like "Format(local:Name) + '!'".
			int afterIdx = SkipWhitespace(code, match.Index + match.Length);
			if (afterIdx < code.Length && code[afterIdx] == ')')
			{
				int afterClose = SkipWhitespace(code, afterIdx + 1);
				if (afterClose < code.Length
					&& LooksLikeCastOperand(code[afterClose])
					&& !TryExtractInvocationTarget(code, openIndex, allowGenerics: true, literalSpans, out _, out _))
				{
					return false;
				}
			}
		}

		var hasMatchingParam = TryHasMatchingParameterOnEnclosingCall(
			code,
			openIndex,
			isIndexer,
			prefix,
			nsResolver,
			compilation,
			xmlnsCache,
			typeCache,
			dataTypeSymbol,
			rootType,
			literalSpans,
			out bool callTargetResolved);

		if (hasResolvedType)
		{
			return hasMatchingParam
				&& (!isFollowedByMemberAccess || HasExpressionRootInScope(code, match.Index, typeName, dataTypeSymbol, rootType, compilation));
		}

		if (isFollowedByMemberAccess)
		{
			if (!HasExpressionRootInScope(code, match.Index, typeName, dataTypeSymbol, rootType, compilation))
				return false;

			return !callTargetResolved || hasMatchingParam;
		}

		if (callTargetResolved && !hasMatchingParam)
			return false;

		return !callTargetResolved || HasExpressionRootInScope(code, match.Index, typeName, dataTypeSymbol, rootType, compilation);
	}

	static int SkipWhitespaceBackwardsIndex(string text, int index)
	{
		while (index >= 0 && char.IsWhiteSpace(text[index]))
			index--;
		return index;
	}

	static bool TryFindEnclosingArgumentListOpener(
		string code,
		int matchIndex,
		List<(int Start, int End, bool IsFormatClause)> literalSpans,
		out int openIndex,
		out bool isIndexer)
	{
		openIndex = -1;
		isIndexer = false;

		int prevIdx = SkipWhitespaceBackwardsIndex(code, matchIndex - 1);
		if (prevIdx < 0)
			return false;

		char prevChar = code[prevIdx];
		if (prevChar != '(' && prevChar != ',' && prevChar != '[')
			return false;

		if (prevChar == '(' || prevChar == '[')
		{
			openIndex = prevIdx;
			isIndexer = prevChar == '[';
		}
		else
		{
			int parenDepth = 0;
			int bracketDepth = 0;
			int braceDepth = 0;
			for (int i = prevIdx - 1; i >= 0; i--)
			{
				if (IsInLiteralSpan(i, literalSpans))
					continue;

				switch (code[i])
				{
					case ')':
						parenDepth++;
						break;
					case ']':
						bracketDepth++;
						break;
					case '}':
						braceDepth++;
						break;
					case '(':
						if (parenDepth > 0)
						{
							parenDepth--;
						}
						else if (bracketDepth == 0 && braceDepth == 0)
						{
							openIndex = i;
							isIndexer = false;
							i = 0;
						}
						break;
					case '[':
						if (bracketDepth > 0)
						{
							bracketDepth--;
						}
						else if (parenDepth == 0 && braceDepth == 0)
						{
							openIndex = i;
							isIndexer = true;
							i = 0;
						}
						break;
					case '{':
						if (braceDepth > 0)
							braceDepth--;
						else
							return false;
						break;
					case ';':
						if (parenDepth == 0 && bracketDepth == 0 && braceDepth == 0)
							return false;
						break;
				}
			}
		}

		if (openIndex < 0)
			return false;

		if (isIndexer)
		{
			if (!TrySkipWhitespaceBackwards(code, openIndex, out char beforeBracket)
				|| (!char.IsLetterOrDigit(beforeBracket) && beforeBracket != '_' && beforeBracket != ')' && beforeBracket != ']'))
			{
				return false;
			}

			// Exclude array creation expressions like "new int[missing:Strings.Count]".
			if (TryExtractInvocationTarget(code, openIndex, allowGenerics: true, literalSpans, out _, out bool isNewTarget) && isNewTarget)
				return false;

			return true;
		}

		if (TryReadPrecedingWord(code, openIndex, out var word))
			return !IsNonInvocationKeyword(word);

		if (!TrySkipWhitespaceBackwards(code, openIndex, out char beforeParen))
			return false;

		if (beforeParen == ')' || beforeParen == ']')
			return true;

		if (beforeParen == '>')
		{
			int beforeParenIdx = SkipWhitespaceBackwardsIndex(code, openIndex - 1);
			return TrySkipBalancedAngleBracketsBackwards(code, beforeParenIdx, out int beforeGenerics)
				&& TryReadPrecedingWord(code, beforeGenerics, out var genericWord)
				&& !IsNonInvocationKeyword(genericWord);
		}

		return false;
	}

	static bool IsNonInvocationKeyword(string word)
		=> word is "typeof" or "sizeof" or "default" or "nameof" or "return" or "throw" or "await" or "yield"
			or "case" or "in" or "is" or "as" or "var" or "if" or "while" or "for" or "foreach" or "using"
			or "lock" or "fixed" or "switch" or "catch" or "when" or "checked" or "unchecked" or "else" or "do";

	static bool IsInLiteralSpan(int index, List<(int Start, int End, bool IsFormatClause)> literalSpans)
	{
		foreach (var span in literalSpans)
		{
			if (!span.IsFormatClause && index >= span.Start && index < span.End)
				return true;
		}

		return false;
	}

	static bool TryHasMatchingParameterOnEnclosingCall(
		string code,
		int openIndex,
		bool isIndexer,
		string paramName,
		IXmlNamespaceResolver nsResolver,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType,
		List<(int Start, int End, bool IsFormatClause)> literalSpans,
		out bool callTargetResolved)
	{
		callTargetResolved = false;
		if (!TryExtractInvocationTarget(code, openIndex, !isIndexer, literalSpans, out var targetChain, out var isObjectCreation))
			return false;

		if (isIndexer)
		{
			var targetTypes = TryResolveExpressionTargetTypes(targetChain, nsResolver, compilation, xmlnsCache, typeCache, dataTypeSymbol, rootType);
			var hasIndexers = false;
			foreach (var targetType in targetTypes)
			{
				foreach (var prop in targetType.GetAllProperties(context: null))
				{
					if (!prop.IsIndexer)
						continue;

					hasIndexers = true;
					if (HasParameter(prop.Parameters, paramName))
					{
						callTargetResolved = true;
						return true;
					}
				}
			}

			callTargetResolved = hasIndexers;
			return false;
		}

		if (isObjectCreation)
		{
			if (string.Equals(targetChain, "new", StringComparison.Ordinal))
				return false;

			var constructedTypes = TryResolveExpressionTargetTypes(targetChain, nsResolver, compilation, xmlnsCache, typeCache, dataTypeSymbol, rootType);
			var hasConstructors = false;
			foreach (var constructedType in constructedTypes.OfType<INamedTypeSymbol>())
			{
				if (constructedType.InstanceConstructors.Length > 0)
					hasConstructors = true;
				if (constructedType.InstanceConstructors.Any(c => HasParameter(c.Parameters, paramName)))
				{
					callTargetResolved = true;
					return true;
				}
			}

			callTargetResolved = hasConstructors;
			return false;
		}

		var lastDot = LastTopLevelDotIndex(targetChain);
		if (lastDot < 0)
		{
			if (targetChain.IndexOf(':') >= 0)
				return false;

			var methodNameOnly = StripGenericAndCallSuffixes(targetChain);
			if (string.IsNullOrEmpty(methodNameOnly))
				return false;

			return HasCallableWithParameter(dataTypeSymbol, methodNameOnly, paramName, ref callTargetResolved)
				| HasCallableWithParameter(rootType, methodNameOnly, paramName, ref callTargetResolved);
		}

		var receiver = targetChain.Substring(0, lastDot);
		var methodName = StripGenericAndCallSuffixes(targetChain.Substring(lastDot + 1));
		if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(methodName))
			return false;

		var receiverTypes = TryResolveExpressionTargetTypes(receiver, nsResolver, compilation, xmlnsCache, typeCache, dataTypeSymbol, rootType);
		foreach (var receiverType in receiverTypes)
		{
			if (HasCallableWithParameter(receiverType, methodName, paramName, ref callTargetResolved))
				return true;
		}

		return false;
	}

	static bool HasCallableWithParameter(ITypeSymbol? targetType, string methodName, string paramName, ref bool foundCallable)
	{
		if (targetType == null)
			return false;

		foreach (var method in targetType.GetAllMethods(methodName, context: null))
		{
			foundCallable = true;
			if (HasParameter(method.Parameters, paramName))
				return true;
		}

		foreach (var prop in targetType.GetAllProperties(methodName, context: null))
		{
			if (prop.Type is INamedTypeSymbol { DelegateInvokeMethod: { } invoke })
			{
				foundCallable = true;
				if (HasParameter(invoke.Parameters, paramName))
					return true;
			}
		}

		foreach (var field in targetType.GetAllFields(methodName, context: null))
		{
			if (field.Type is INamedTypeSymbol { DelegateInvokeMethod: { } invoke })
			{
				foundCallable = true;
				if (HasParameter(invoke.Parameters, paramName))
					return true;
			}
		}

		return false;
	}

	static bool HasParameter(System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters, string paramName)
	{
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].Name == paramName)
				return true;
		}

		return false;
	}

	static bool TryExtractInvocationTarget(
		string code,
		int openIndex,
		bool allowGenerics,
		List<(int Start, int End, bool IsFormatClause)> literalSpans,
		out string targetChain,
		out bool isObjectCreation)
	{
		targetChain = string.Empty;
		isObjectCreation = false;

		int idx = SkipWhitespaceBackwardsIndex(code, openIndex - 1);
		if (idx < 0)
			return false;

		var segments = new List<string>();
		var hasMemberOrCallAfterSegment = false;
		while (idx >= 0)
		{
			while (idx >= 0 && (code[idx] == ')' || code[idx] == ']'))
			{
				char closeChar = code[idx];
				char openChar = closeChar == ')' ? '(' : '[';
				if (!TrySkipBalancedGroupBackwards(code, idx, openChar, closeChar, literalSpans, out int groupOpen))
					return false;

				segments.Add(closeChar == ')' ? "()" : "[]");
				hasMemberOrCallAfterSegment = true;
				idx = SkipWhitespaceBackwardsIndex(code, groupOpen - 1);
				if (idx < 0)
					return false;
			}

			if (allowGenerics && code[idx] == '>')
			{
				if (!TrySkipBalancedAngleBracketsBackwards(code, idx, out int beforeGenerics))
					return false;

				idx = SkipWhitespaceBackwardsIndex(code, beforeGenerics - 1);
				if (idx < 0)
					return false;
			}

			int end = idx;
			while (idx >= 0 && (char.IsLetterOrDigit(code[idx]) || code[idx] == '_'))
				idx--;

			int start = idx + 1;
			if (start > end)
				return false;

			var segment = code.Substring(start, end - start + 1);
			segments.Add(segment);

			int sepIdx = SkipWhitespaceBackwardsIndex(code, start - 1);
			if (sepIdx < 0)
			{
				if (IsNonInvocationKeyword(segment))
					return false;
				break;
			}

			if (code[sepIdx] == '.')
			{
				segments.Add(".");
				hasMemberOrCallAfterSegment = true;
				idx = SkipWhitespaceBackwardsIndex(code, sepIdx - 1);
				continue;
			}

			if (code[sepIdx] == ':')
			{
				if (sepIdx > 0 && code[sepIdx - 1] == ':')
				{
					segments.Add("::");
					idx = SkipWhitespaceBackwardsIndex(code, sepIdx - 2);
					continue;
				}

				if (TryReadPrecedingWord(code, sepIdx, out var prefixWord))
				{
					int prefixStart = sepIdx - 1;
					while (prefixStart >= 0 && char.IsWhiteSpace(code[prefixStart]))
						prefixStart--;
					prefixStart -= prefixWord.Length - 1;

					// Stop only if the colon is preceded by another member-incompatible token rather than the xmlns prefix word.
					if (prefixStart >= 0)
					{
						segments.Add(":");
						segments.Add(prefixWord);
						sepIdx = SkipWhitespaceBackwardsIndex(code, prefixStart - 1);
						if (!hasMemberOrCallAfterSegment && sepIdx >= 0 && TryReadKeywordNew(code, prefixStart))
							isObjectCreation = true;
						break;
					}
				}

				return false;
			}

			if (IsNonInvocationKeyword(segment))
				return false;

			if (!hasMemberOrCallAfterSegment && TryReadKeywordNew(code, start))
				isObjectCreation = true;
			break;
		}

		if (segments.Count == 0)
			return false;

		segments.Reverse();
		targetChain = string.Concat(segments);
		if (string.Equals(targetChain, "new", StringComparison.Ordinal))
			isObjectCreation = true;
		return true;
	}

	static bool TrySkipBalancedGroupBackwards(
		string code,
		int closeIndex,
		char openChar,
		char closeChar,
		List<(int Start, int End, bool IsFormatClause)> literalSpans,
		out int openIndex)
	{
		openIndex = -1;
		if (closeIndex < 0 || code[closeIndex] != closeChar)
			return false;

		int depth = 1;
		for (int i = closeIndex - 1; i >= 0; i--)
		{
			if (IsInLiteralSpan(i, literalSpans))
				continue;

			char ch = code[i];
			if (ch == closeChar)
			{
				depth++;
			}
			else if (ch == openChar)
			{
				depth--;
				if (depth == 0)
				{
					openIndex = i;
					return true;
				}
			}
			else if (ch == ';')
			{
				return false;
			}
		}

		return false;
	}

	static bool TryReadKeywordNew(string code, int beforeTokenIndex)
	{
		if (!TryReadPrecedingWord(code, beforeTokenIndex, out var word) || word != "new")
			return false;

		int end = beforeTokenIndex - 1;
		while (end >= 0 && char.IsWhiteSpace(code[end]))
			end--;
		int newStart = end - 3;
		int prevIdx = SkipWhitespaceBackwardsIndex(code, newStart);
		return prevIdx < 0 || code[prevIdx] != '.';
	}

	static bool TrySkipBalancedAngleBracketsBackwards(string text, int closeIndex, out int openBracketIndex)
	{
		openBracketIndex = -1;
		if (closeIndex < 0 || text[closeIndex] != '>')
			return false;

		int depth = 1;
		for (int i = closeIndex - 1; i >= 0; i--)
		{
			char ch = text[i];
			if (ch == '>')
			{
				depth++;
			}
			else if (ch == '<')
			{
				depth--;
				if (depth == 0)
				{
					openBracketIndex = i;
					return true;
				}
			}
			else if (ch is ';' or '{' or '}' or '(' or ')' or '[' or ']' or '=' or '&' or '|' or '?')
			{
				return false;
			}
		}

		return false;
	}

	static int LastTopLevelDotIndex(string targetChain)
	{
		int angleDepth = 0;
		for (int i = targetChain.Length - 1; i >= 0; i--)
		{
			switch (targetChain[i])
			{
				case '>':
					angleDepth++;
					break;
				case '<':
					if (angleDepth > 0)
						angleDepth--;
					break;
				case '.' when angleDepth == 0:
					return i;
			}
		}

		return -1;
	}

	static string StripGenericAndCallSuffixes(string segment)
	{
		var idx = 0;
		while (idx < segment.Length && (char.IsLetterOrDigit(segment[idx]) || segment[idx] == '_'))
			idx++;
		return idx == segment.Length ? segment : segment.Substring(0, idx);
	}

	static List<string> SplitTopLevelDotSegments(string chain)
	{
		var parts = new List<string>();
		int start = 0;
		int angleDepth = 0;
		for (int i = 0; i < chain.Length; i++)
		{
			switch (chain[i])
			{
				case '<':
					angleDepth++;
					break;
				case '>':
					if (angleDepth > 0)
						angleDepth--;
					break;
				case '.' when angleDepth == 0:
					parts.Add(chain.Substring(start, i - start));
					start = i + 1;
					break;
			}
		}

		parts.Add(chain.Substring(start));
		return parts;
	}

	static List<ITypeSymbol> TryResolveExpressionTargetTypes(
		string targetChain,
		IXmlNamespaceResolver nsResolver,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType)
	{
		var results = new List<ITypeSymbol>();
		if (string.IsNullOrEmpty(targetChain))
			return results;

		var forceGlobal = false;
		if (targetChain.StartsWith("global::", StringComparison.Ordinal))
		{
			forceGlobal = true;
			targetChain = targetChain.Substring("global::".Length);
		}

		var segments = SplitTopLevelDotSegments(targetChain);
		if (segments.Count == 0 || segments.Any(string.IsNullOrEmpty))
			return results;

		var singleColonIdx = segments[0].IndexOf(':');
		if (!forceGlobal && singleColonIdx > 0 && (singleColonIdx + 1 >= segments[0].Length || segments[0][singleColonIdx + 1] != ':'))
		{
			var prefix = segments[0].Substring(0, singleColonIdx);
			var remainder = segments[0].Substring(singleColonIdx + 1);
			var nsUri = nsResolver.LookupNamespace(prefix);
			if (nsUri == null)
				return results;

			ParseSegment(remainder, out var baseTypeName, out var suffixes);
			if (string.IsNullOrEmpty(baseTypeName))
				return results;

			var xmlType = new XmlType(nsUri, baseTypeName, null);
			if (!xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out var resolvedType) || resolvedType == null)
				return results;

			var candidates = suffixes.Count == 0
				? new List<ITypeSymbol> { resolvedType }
				: ApplyCallOrIndexerSuffixes([resolvedType], suffixes, isConstructorOrStaticRoot: true);

			for (int i = 1; i < segments.Count && candidates.Count > 0; i++)
				candidates = StepSegmentOnTypes(candidates, segments[i], compilation);
			return candidates;
		}

		if (!forceGlobal)
		{
			if (TryResolveChainFromRoots(segments, dataTypeSymbol, rootType, compilation, out var fromInstance))
				return fromInstance;

			if (segments.All(s => s.IndexOf('(') < 0 && s.IndexOf('[') < 0))
			{
				var segArray = segments.ToArray();
				var fromRootNs = WalkNamespaceOrTypeChain(rootType?.ContainingNamespace, segArray, compilation);
				if (fromRootNs != null)
					return [fromRootNs];

				var fromDataNs = WalkNamespaceOrTypeChain(dataTypeSymbol?.ContainingNamespace, segArray, compilation);
				if (fromDataNs != null)
					return [fromDataNs];
			}
			else if (TryResolveNamespacePrefixThenMembers(segments, rootType?.ContainingNamespace, compilation, out var fromNsThenCalls))
			{
				return fromNsThenCalls;
			}
		}

		if (segments.All(s => s.IndexOf('(') < 0 && s.IndexOf('[') < 0))
		{
			var globalType = WalkNamespaceOrTypeChain(compilation.GlobalNamespace, segments.ToArray(), compilation);
			if (globalType != null)
				return [globalType];
		}
		else if (TryResolveNamespacePrefixThenMembers(segments, compilation.GlobalNamespace, compilation, out var fromGlobalCalls))
		{
			return fromGlobalCalls;
		}

		return results;
	}

	static bool TryResolveChainFromRoots(
		List<string> segments,
		ITypeSymbol? dataTypeSymbol,
		ITypeSymbol? rootType,
		Compilation compilation,
		out List<ITypeSymbol> resolvedTypes)
	{
		resolvedTypes = [];
		ParseSegment(segments[0], out var firstName, out var firstSuffixes);
		List<ITypeSymbol> current;
		if (string.Equals(firstName, "this", StringComparison.Ordinal))
		{
			if (rootType == null)
				return false;

			current = firstSuffixes.Count == 0
				? [rootType]
				: ApplyCallOrIndexerSuffixes([rootType], firstSuffixes, isConstructorOrStaticRoot: false);
		}
		else
		{
			current = [];
			if (dataTypeSymbol != null)
				current.AddRange(StepSegmentOnTypes([dataTypeSymbol], segments[0], compilation));
			if (rootType != null && !SymbolEqualityComparer.Default.Equals(rootType, dataTypeSymbol))
				current.AddRange(StepSegmentOnTypes([rootType], segments[0], compilation));
		}

		for (int i = 1; i < segments.Count && current.Count > 0; i++)
			current = StepSegmentOnTypes(current, segments[i], compilation);

		if (current.Count > 0)
		{
			resolvedTypes = current;
			return true;
		}

		return false;
	}

	static bool TryResolveNamespacePrefixThenMembers(
		List<string> segments,
		INamespaceSymbol? startNs,
		Compilation compilation,
		out List<ITypeSymbol> resolvedTypes)
	{
		resolvedTypes = [];
		if (startNs == null)
			return false;

		var currentNs = startNs;
		for (int i = 0; i < segments.Count; i++)
		{
			ParseSegment(segments[i], out var name, out var suffixes);
			if (string.IsNullOrEmpty(name))
				return false;

			var typeMatch = currentNs.GetTypeMembers(name).FirstOrDefault();
			if (typeMatch != null)
			{
				var candidates = suffixes.Count == 0
					? new List<ITypeSymbol> { typeMatch }
					: ApplyCallOrIndexerSuffixes([typeMatch], suffixes, isConstructorOrStaticRoot: true);
				for (int j = i + 1; j < segments.Count && candidates.Count > 0; j++)
					candidates = StepSegmentOnTypes(candidates, segments[j], compilation);
				if (candidates.Count > 0)
				{
					resolvedTypes = candidates;
					return true;
				}
				return false;
			}

			if (suffixes.Count > 0)
				return false;

			var nsMatch = currentNs.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == name);
			if (nsMatch == null)
				return false;
			currentNs = nsMatch;
		}

		return false;
	}

	static void ParseSegment(string segment, out string baseName, out List<string> suffixes)
	{
		suffixes = [];
		int idx = 0;
		while (idx < segment.Length && (char.IsLetterOrDigit(segment[idx]) || segment[idx] == '_'))
			idx++;
		baseName = segment.Substring(0, idx);

		if (idx < segment.Length && segment[idx] == '<')
		{
			int depth = 0;
			while (idx < segment.Length)
			{
				if (segment[idx] == '<') depth++;
				else if (segment[idx] == '>')
				{
					depth--;
					if (depth == 0) { idx++; break; }
				}
				idx++;
			}
		}

		while (idx + 1 < segment.Length)
		{
			var pair = segment.Substring(idx, 2);
			if (pair is "()" or "[]")
			{
				suffixes.Add(pair);
				idx += 2;
			}
			else
			{
				break;
			}
		}
	}

	static List<ITypeSymbol> StepSegmentOnTypes(List<ITypeSymbol> inputTypes, string segment, Compilation compilation)
	{
		ParseSegment(segment, out var memberName, out var suffixes);
		var results = new List<ITypeSymbol>();
		if (string.IsNullOrEmpty(memberName))
			return results;

		if (suffixes.Count > 0 && suffixes[0] == "()")
		{
			var methodReturnTypes = new List<ITypeSymbol>();
			foreach (var type in inputTypes)
			{
				foreach (var method in type.GetAllMethods(memberName, context: null))
				{
					if (!method.ReturnsVoid && method.ReturnType != null)
						methodReturnTypes.Add(method.ReturnType);
				}

				var delegateMember = StepTypeOrMember(type, memberName, compilation);
				if (delegateMember is INamedTypeSymbol { DelegateInvokeMethod: { ReturnsVoid: false, ReturnType: { } delRet } })
					methodReturnTypes.Add(delRet);
			}

			if (suffixes.Count == 1)
				return DeduplicateTypes(methodReturnTypes);

			return ApplyCallOrIndexerSuffixes(methodReturnTypes, suffixes.GetRange(1, suffixes.Count - 1), isConstructorOrStaticRoot: false);
		}

		var memberTypes = new List<ITypeSymbol>();
		foreach (var type in inputTypes)
		{
			var stepped = StepTypeOrMember(type, memberName, compilation);
			if (stepped != null)
				memberTypes.Add(stepped);
		}

		if (suffixes.Count == 0)
			return DeduplicateTypes(memberTypes);

		return ApplyCallOrIndexerSuffixes(memberTypes, suffixes, isConstructorOrStaticRoot: false);
	}

	static List<ITypeSymbol> ApplyCallOrIndexerSuffixes(List<ITypeSymbol> types, List<string> suffixes, bool isConstructorOrStaticRoot)
	{
		var current = DeduplicateTypes(types);
		for (int i = 0; i < suffixes.Count && current.Count > 0; i++)
		{
			var suffix = suffixes[i];
			var next = new List<ITypeSymbol>();
			if (suffix == "()")
			{
				if (i == 0 && isConstructorOrStaticRoot)
				{
					// e.g. "new Type()" keeps the constructed type itself rather than invoking a delegate on it.
					continue;
				}
				foreach (var t in current)
				{
					if (t is INamedTypeSymbol { DelegateInvokeMethod: { ReturnsVoid: false, ReturnType: { } ret } })
						next.Add(ret);
				}
			}
			else if (suffix == "[]")
			{
				foreach (var t in current)
				{
					foreach (var prop in t.GetAllProperties(context: null))
					{
						if (prop.IsIndexer && prop.Type != null)
							next.Add(prop.Type);
					}
				}
			}
			current = DeduplicateTypes(next);
		}

		return current;
	}

	static List<ITypeSymbol> DeduplicateTypes(List<ITypeSymbol> types)
	{
		var seen = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
		var result = new List<ITypeSymbol>(types.Count);
		foreach (var t in types)
		{
			if (seen.Add(t))
				result.Add(t);
		}
		return result;
	}

	static ITypeSymbol? WalkNamespaceOrTypeChain(INamespaceSymbol? startNs, string[] segments, Compilation compilation)
	{
		var currentNs = startNs;
		ITypeSymbol? currentType = null;
		for (int i = 0; i < segments.Length; i++)
		{
			var seg = StripGenericAndCallSuffixes(segments[i]);
			if (currentType != null)
			{
				currentType = StepTypeOrMember(currentType, seg, compilation);
				if (currentType == null)
					return null;
				continue;
			}

			if (currentNs == null)
				return null;

			var typeMatch = currentNs.GetTypeMembers(seg).FirstOrDefault();
			if (typeMatch != null)
			{
				currentType = typeMatch;
				continue;
			}

			var nsMatch = currentNs.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == seg);
			if (nsMatch != null)
			{
				currentNs = nsMatch;
				continue;
			}

			return null;
		}

		return currentType;
	}

	static ITypeSymbol? StepTypeOrMember(ITypeSymbol type, string memberName, Compilation compilation)
	{
		for (var current = type; current != null; current = current.BaseType)
		{
			var nestedType = current.GetTypeMembers(memberName).FirstOrDefault();
			if (nestedType != null)
				return nestedType;
		}

		var prop = type.GetAllProperties(memberName, context: null).FirstOrDefault();
		if (prop != null)
			return prop.Type;

		var field = type.GetAllFields(memberName, context: null).FirstOrDefault();
		if (field != null)
			return field.Type;

		if (type.TryGetObservablePropertyType(memberName, compilation, out var observableType))
			return observableType;

		return null;
	}

	static bool TryReadPrecedingWord(string text, int beforeIndex, out string word)
	{
		word = string.Empty;
		int end = beforeIndex - 1;
		while (end >= 0 && char.IsWhiteSpace(text[end]))
			end--;

		if (end < 0 || (!char.IsLetterOrDigit(text[end]) && text[end] != '_'))
			return false;

		int start = end;
		while (start > 0 && (char.IsLetterOrDigit(text[start - 1]) || text[start - 1] == '_'))
			start--;

		word = text.Substring(start, end - start + 1);
		return true;
	}

	static bool TrySkipWhitespaceBackwards(string text, int beforeIndex, out char ch)
	{
		int idx = beforeIndex - 1;
		while (idx >= 0 && char.IsWhiteSpace(text[idx]))
			idx--;

		if (idx >= 0)
		{
			ch = text[idx];
			return true;
		}

		ch = '\0';
		return false;
	}

	static bool LooksLikeCastOperand(char ch)
		=> char.IsLetterOrDigit(ch) || ch == '_' || ch == '(' || ch == '"' || ch == '\''
			|| ch == '@' || ch == '$' || ch == '!' || ch == '~' || ch == '+' || ch == '-';

	static string ReplaceOutsideLiteralSegments(string code, Regex pattern, MatchEvaluator evaluator)
		=> ReplaceOutsideLiteralSegments(code, pattern, (match, _, _) => evaluator(match), allowInterpolationFormatClause: false);

	static string ReplaceOutsideLiteralSegments(
		string code,
		Regex pattern,
		Func<Match, bool, List<(int Start, int End, bool IsFormatClause)>, string> evaluator,
		bool allowInterpolationFormatClause)
	{
		var matches = pattern.Matches(code).Cast<Match>().ToArray();
		if (matches.Length == 0)
			return code;

		var literalSpans = GetLiteralSegmentSpans(code);
		var result = new StringBuilder(code.Length);
		var currentIndex = 0;

		foreach (var match in matches)
		{
			if (OverlapsLiteralSegment(match.Index, match.Length, literalSpans, out var overlapsFormatClause)
				|| (overlapsFormatClause && !allowInterpolationFormatClause))
				continue;

			result.Append(code, currentIndex, match.Index - currentIndex);
			result.Append(evaluator(match, overlapsFormatClause, literalSpans));
			currentIndex = match.Index + match.Length;
		}

		result.Append(code, currentIndex, code.Length - currentIndex);
		return result.ToString();
	}

	static IEnumerable<Match> MatchesOutsideLiteralSegments(string code, Regex pattern)
	{
		var matches = pattern.Matches(code).Cast<Match>().ToArray();
		if (matches.Length == 0)
			yield break;

		var literalSpans = GetLiteralSegmentSpans(code);

		foreach (var match in matches)
		{
			if (!OverlapsLiteralSegment(match.Index, match.Length, literalSpans, out var overlapsFormatClause)
				&& !overlapsFormatClause)
				yield return match;
		}
	}

	static List<(int Start, int End, bool IsFormatClause)> GetLiteralSegmentSpans(string code)
	{
		var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		var spans = new List<(int Start, int End, bool IsFormatClause)>();

		foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
		{
			if (!IsLiteralSegmentToken(token))
				continue;

			spans.Add((token.SpanStart, token.Span.End, token.Parent is InterpolationFormatClauseSyntax));
		}

		return spans;
	}

	static bool IsLiteralSegmentToken(SyntaxToken token)
	{
		var kind = token.Kind().ToString();
		return kind.Contains("StringLiteralToken", StringComparison.Ordinal)
			|| kind.Contains("CharacterLiteralToken", StringComparison.Ordinal)
			|| kind.Equals("InterpolatedStringTextToken", StringComparison.Ordinal)
			|| kind.Equals("InterpolatedRawStringTextToken", StringComparison.Ordinal);
	}

	static bool OverlapsLiteralSegment(int start, int length, List<(int Start, int End, bool IsFormatClause)> literalSpans, out bool overlapsFormatClause)
	{
		overlapsFormatClause = false;
		var end = start + length;
		foreach (var span in literalSpans)
		{
			if (start < span.End && end > span.Start)
			{
				if (!span.IsFormatClause)
				{
					overlapsFormatClause = false;
					return true;
				}

				overlapsFormatClause = true;
			}
		}

		return false;
	}

	/// <summary>
	/// Extracts identifiers from interpolated string holes.
	/// E.g., for <c>$"Hello, {Name1}!"</c> returns ["Name1"].
	/// For <c>$"Hello, {User.Name}!"</c> returns ["User"].
	/// Only returns root identifiers (first segment before any dot).
	/// </summary>
	public static List<string> ExtractInterpolatedStringIdentifiers(string expressionCode)
		=> ExtractInterpolatedStringIdentifierReferences(expressionCode)
			.Select(reference => reference.Identifier)
			.Distinct()
			.ToList();

	public static List<InterpolatedStringIdentifier> ExtractInterpolatedStringIdentifierReferences(string expressionCode)
	{
		var identifiers = new List<InterpolatedStringIdentifier>();
		var seen = new HashSet<string>(StringComparer.Ordinal);

		// Try parsing as a C# expression to extract interpolation holes
		var tree = CSharpSyntaxTree.ParseText(expressionCode, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();

		foreach (var interpolation in root.DescendantNodes().OfType<InterpolationSyntax>())
		{
			// Get the expression inside the interpolation hole
			var expr = interpolation.Expression;
			if (expr == null)
				continue;

			ExtractInterpolatedExpressionIdentifiers(expr, identifiers, seen);
		}

		return identifiers;
	}

	static void ExtractInterpolatedExpressionIdentifiers(ExpressionSyntax expression, List<InterpolatedStringIdentifier> identifiers, HashSet<string> seen)
	{
		switch (expression)
		{
			case IdentifierNameSyntax idName:
				AddInterpolatedIdentifier(idName.Identifier.Text, idName.Identifier.Text, identifiers, seen);
				break;
			case MemberAccessExpressionSyntax memberAccess:
				AddMemberAccessRoot(memberAccess, identifiers, seen);
				break;
			case InvocationExpressionSyntax invocation:
				AddInvocationIdentifiers(invocation, identifiers, seen);
				break;
			case BinaryExpressionSyntax binary:
				ExtractInterpolatedExpressionIdentifiers(binary.Left, identifiers, seen);
				ExtractInterpolatedExpressionIdentifiers(binary.Right, identifiers, seen);
				break;
			case ConditionalExpressionSyntax conditional:
				ExtractInterpolatedExpressionIdentifiers(conditional.Condition, identifiers, seen);
				ExtractInterpolatedExpressionIdentifiers(conditional.WhenTrue, identifiers, seen);
				ExtractInterpolatedExpressionIdentifiers(conditional.WhenFalse, identifiers, seen);
				break;
			case ParenthesizedExpressionSyntax parenthesized:
				ExtractInterpolatedExpressionIdentifiers(parenthesized.Expression, identifiers, seen);
				break;
			case PrefixUnaryExpressionSyntax prefixUnary:
				ExtractInterpolatedExpressionIdentifiers(prefixUnary.Operand, identifiers, seen);
				break;
			case PostfixUnaryExpressionSyntax postfixUnary:
				ExtractInterpolatedExpressionIdentifiers(postfixUnary.Operand, identifiers, seen);
				break;
			case ConditionalAccessExpressionSyntax conditionalAccess:
				ExtractInterpolatedExpressionIdentifiers(conditionalAccess.Expression, identifiers, seen);
				ExtractConditionalAccessIdentifiers(conditionalAccess.WhenNotNull, identifiers, seen);
				break;
			case ElementAccessExpressionSyntax elementAccess:
				ExtractInterpolatedExpressionIdentifiers(elementAccess.Expression, identifiers, seen);
				foreach (var argument in elementAccess.ArgumentList.Arguments)
					ExtractInterpolatedExpressionIdentifiers(argument.Expression, identifiers, seen);
				break;
			case CastExpressionSyntax cast:
				ExtractInterpolatedExpressionIdentifiers(cast.Expression, identifiers, seen);
				break;
			case AwaitExpressionSyntax awaitExpression:
				ExtractInterpolatedExpressionIdentifiers(awaitExpression.Expression, identifiers, seen);
				break;
		}
	}

	static void AddInvocationIdentifiers(InvocationExpressionSyntax invocation, List<InterpolatedStringIdentifier> identifiers, HashSet<string> seen)
	{
		switch (invocation.Expression)
		{
			case IdentifierNameSyntax identifier when IsNameOfInvocation(identifier):
				return;
			case IdentifierNameSyntax identifier:
				AddInterpolatedIdentifier(identifier.Identifier.Text, identifier.Identifier.Text, identifiers, seen);
				break;
			case MemberAccessExpressionSyntax memberAccess:
				if (!AddMemberAccessRoot(memberAccess, identifiers, seen))
					ExtractInterpolatedExpressionIdentifiers(memberAccess.Expression, identifiers, seen);
				break;
			default:
				ExtractInterpolatedExpressionIdentifiers(invocation.Expression, identifiers, seen);
				break;
		}

		foreach (var argument in invocation.ArgumentList.Arguments)
			ExtractInterpolatedExpressionIdentifiers(argument.Expression, identifiers, seen);
	}

	static bool IsNameOfInvocation(IdentifierNameSyntax identifier)
		=> string.Equals(identifier.Identifier.Text, "nameof", StringComparison.Ordinal);

	static void ExtractConditionalAccessIdentifiers(ExpressionSyntax expression, List<InterpolatedStringIdentifier> identifiers, HashSet<string> seen)
	{
		switch (expression)
		{
			case InvocationExpressionSyntax invocation:
				foreach (var argument in invocation.ArgumentList.Arguments)
					ExtractInterpolatedExpressionIdentifiers(argument.Expression, identifiers, seen);
				break;
			case MemberAccessExpressionSyntax memberAccess:
				if (!AddMemberAccessRoot(memberAccess, identifiers, seen))
					ExtractInterpolatedExpressionIdentifiers(memberAccess.Expression, identifiers, seen);
				break;
			default:
				ExtractInterpolatedExpressionIdentifiers(expression, identifiers, seen);
				break;
		}
	}

	static bool AddMemberAccessRoot(MemberAccessExpressionSyntax memberAccess, List<InterpolatedStringIdentifier> identifiers, HashSet<string> seen)
	{
		if (!TryGetMemberAccessParts(memberAccess, out var parts) || parts.Count == 0)
			return false;

		var rootIdentifier = parts[0];
		var typeReferenceCandidate = parts.Count > 1
			? string.Join(".", parts.Take(parts.Count - 1))
			: rootIdentifier;
		AddInterpolatedIdentifier(rootIdentifier, typeReferenceCandidate, identifiers, seen);
		return true;
	}

	static bool TryGetMemberAccessParts(ExpressionSyntax expression, out List<string> parts)
	{
		parts = new List<string>();
		return TryAppendMemberAccessParts(expression, parts);
	}

	static bool TryAppendMemberAccessParts(ExpressionSyntax expression, List<string> parts)
	{
		switch (expression)
		{
			case IdentifierNameSyntax identifier:
				parts.Add(identifier.Identifier.Text);
				return true;
			case MemberAccessExpressionSyntax memberAccess:
				if (!TryAppendMemberAccessParts(memberAccess.Expression, parts))
					return false;
				parts.Add(memberAccess.Name.Identifier.Text);
				return true;
			default:
				return false;
		}
	}

	static void AddInterpolatedIdentifier(string identifier, string typeReferenceCandidate, List<InterpolatedStringIdentifier> identifiers, HashSet<string> seen)
	{
		if (string.IsNullOrEmpty(identifier))
			return;

		var key = $"{identifier}|{typeReferenceCandidate}";
		if (seen.Add(key))
			identifiers.Add(new InterpolatedStringIdentifier(identifier, typeReferenceCandidate));
	}

	/// <summary>
	/// Checks if a lambda expression body is a method group reference (member access without invocation).
	/// E.g., <c>(s, e) => this.OnClicked</c> is a method group (missing parentheses).
	/// Returns the method group expression and the lambda parameters if detected, null otherwise.
	/// </summary>
	public static (string methodGroup, string suggestion)? DetectLambdaMethodGroupReference(string expressionCode)
	{
		// Parse the expression as C#
		var tree = CSharpSyntaxTree.ParseText(expressionCode, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();

		// Find lambda expressions
		var lambda = root.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().FirstOrDefault();
		if (lambda == null)
			return null;

		// Get the lambda body - should be an expression (not a block)
		var body = lambda.ExpressionBody;
		if (body == null)
			return null;

		// Check if the body is a member access (this.Method or just Method) without invocation
		if (body is MemberAccessExpressionSyntax memberAccess)
		{
			// Check that the parent is NOT an InvocationExpression
			// (If it were "this.Method()", the body would be InvocationExpressionSyntax, not MemberAccessExpressionSyntax)
			var methodGroup = memberAccess.ToString();
			return (methodGroup, CreateLambdaMethodGroupSuggestion(methodGroup, lambda.ParameterList.Parameters));
		}

		// Also check for bare identifier (e.g., (s, e) => OnClicked)
		if (body is IdentifierNameSyntax identifier)
		{
			var methodGroup = identifier.Identifier.Text;
			return (methodGroup, CreateLambdaMethodGroupSuggestion(methodGroup, lambda.ParameterList.Parameters));
		}

		return null;
	}

	static string CreateLambdaMethodGroupSuggestion(string methodGroup, SeparatedSyntaxList<ParameterSyntax> parameters)
	{
		var parameterNames = parameters.Select(p => p.Identifier.Text).ToList();
		if (parameterNames.Any(name => string.IsNullOrEmpty(name) || name == "_"))
			return string.Empty;

		return $" Did you mean '{methodGroup}({string.Join(", ", parameterNames)})'?";
	}
}
