using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Represents a C# expression to be emitted directly in generated code.
/// Used as the Value of a ValueNode when the value is a C# expression rather than a literal string.
/// </summary>
/// <param name="Code">The C# expression code to emit.</param>
/// <param name="TransformQuotes">Whether single quotes should be transformed to double quotes. 
/// True for attribute values (XML requires escaping), false for element content/CDATA (natural C# allowed).</param>
internal sealed record Expression(string Code, bool TransformQuotes = true);

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
		@")",
		RegexOptions.Compiled);

	// Operators that indicate C# expressions (checked after first identifier)
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
		" + ",  // Addition (with spaces to avoid {Binding Path=+...})
		" - ",  // Subtraction (with spaces)
		" * ",  // Multiplication
		" / ",  // Division
		" % ",  // Modulo
		" < ",  // Less than
		" > ",  // Greater than
		" ? ",  // Ternary (start)
		" : ",  // Ternary (else) - note: may conflict with namespace:type syntax
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
	{
		if (string.IsNullOrEmpty(value))
			return false;

		var trimmed = value!.Trim();
		if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
			return false;

		// Check for escape sequence
		if (trimmed.StartsWith("{}", StringComparison.Ordinal))
			return false;

		// If it looks like a markup extension call (starts with known extension name), 
		// don't treat it as an implicit expression even if it contains operators
		if (StartsWithMarkupExtension(trimmed))
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
			if (trimmed.IndexOf(op, StringComparison.Ordinal) >= 0)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if the markup string starts with a known markup extension name.
	/// </summary>
	static bool StartsWithMarkupExtension(string trimmed)
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
		
		// Handle prefixed identifiers like x:Type
		var colonIndex = identifier.IndexOf(':');
		if (colonIndex >= 0)
		{
			// Check if it's a known prefixed extension
			if (KnownMarkupExtensions.Contains(identifier))
				return true;
			// Get the part after the prefix
			identifier = identifier.Substring(colonIndex + 1);
		}

		return IsKnownMarkupExtension(identifier);
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
		if (IsImplicitExpression(markupString))
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

	static bool IsKnownMarkupExtension(string name)
	{
		return KnownMarkupExtensions.Contains(name) 
			|| KnownMarkupExtensions.Contains(name + "Extension");
	}

	/// <summary>
	/// Extracts the C# code from an expression value and optionally transforms quotes.
	/// </summary>
	/// <param name="value">The raw XAML value including braces.</param>
	/// <param name="transformQuotes">Whether to transform single quotes to double quotes. 
	/// True for attribute values, false for element content/CDATA.</param>
	/// <returns>The C# code ready to be emitted.</returns>
	public static string GetExpressionCode(string value, bool transformQuotes = true)
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

		// Transform single-quoted strings to double-quoted strings (only for attribute values)
		return transformQuotes ? TransformQuotes(code) : code;
	}

	/// <summary>
	/// Transforms single-quoted strings to double-quoted strings in C# code.
	/// Single-character strings remain as char literals.
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
							// \" stays as \" in double-quoted
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

					// Single character (not an escape sequence) = char literal, keep as is
					// Multiple characters or escape sequences = string literal, convert to double quotes
					if (contentStr.Length == 1)
					{
						// Char literal - keep single quotes
						result.Append('\'');
						result.Append(contentStr);
						result.Append('\'');
					}
					else
					{
						// String literal - convert to double quotes
						// Escape any unescaped double quotes in the content
						var escapedContent = contentStr.Replace("\"", "\\\"");
						result.Append('"');
						result.Append(escapedContent);
						result.Append('"');
					}
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
}
