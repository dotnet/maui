using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
			// Any prefix:Name pattern is a markup extension (custom or known)
			// C# doesn't use this syntax, so it's safe to treat as markup
			return true;
		}

		// Check known extensions first (fast path)
		if (IsKnownMarkupExtension(identifier))
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

		// Transform operator aliases (AND -> &&, OR -> ||) to avoid XML escaping
		code = TransformOperatorAliases(code);

		// Transform single-quoted strings to double-quoted strings (only for attribute values)
		return transformQuotes ? TransformQuotes(code) : code;
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
	/// Transforms single-quoted literals to double-quoted strings based on semantic context.
	/// Uses Roslyn to analyze the expression and determine if each char literal should be a string.
	/// </summary>
	/// <param name="code">The C# expression code with single quotes</param>
	/// <param name="compilation">The compilation for type lookups</param>
	/// <param name="contextTypes">Types to search for method signatures (dataType, thisType, etc.)</param>
	/// <returns>Code with char literals converted to strings where the target expects string</returns>
	public static string TransformQuotesWithSemantics(string code, Compilation compilation, params ITypeSymbol?[] contextTypes)
	{
		// Parse the expression to find char literals and their contexts
		var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();

		// Find all char literals and determine which should become strings
		var charLiterals = root.DescendantNodes()
			.OfType<LiteralExpressionSyntax>()
			.Where(l => l.IsKind(SyntaxKind.CharacterLiteralExpression))
			.ToList();

		if (charLiterals.Count == 0)
			return code; // No char literals to transform

		// Collect positions that need transformation (char -> string)
		var transformPositions = new List<(int start, int length, string replacement)>();

		foreach (var literal in charLiterals)
		{
			var expectedType = DetermineExpectedType(literal, compilation, contextTypes);
			
			// If expected type is string (or unknown and length > 1), transform to string
			bool shouldBeString = expectedType?.SpecialType == SpecialType.System_String;
			
			if (shouldBeString)
			{
				// Get the char content and create a string literal
				var charText = literal.Token.ValueText;
				var stringLiteral = $"\"{EscapeForString(charText)}\"";
				transformPositions.Add((literal.SpanStart, literal.Span.Length, stringLiteral));
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
	static string EscapeForString(string value)
	{
		var sb = new StringBuilder();
		foreach (var c in value)
		{
			switch (c)
			{
				case '"': sb.Append("\\\""); break;
				case '\\': sb.Append("\\\\"); break;
				case '\n': sb.Append("\\n"); break;
				case '\r': sb.Append("\\r"); break;
				case '\t': sb.Append("\\t"); break;
				default: sb.Append(c); break;
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
}
