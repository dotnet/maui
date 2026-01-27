using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Represents a handler for INPC subscription in a TypedBinding.
/// </summary>
internal readonly struct BindingHandler
{
	/// <summary>The expression to get the parent object (e.g., "vm => vm" or "vm => vm.User").</summary>
	public string ParentExpression { get; }
	
	/// <summary>The property name to subscribe to (e.g., "Name").</summary>
	public string PropertyName { get; }

	public BindingHandler(string parentExpression, string propertyName)
	{
		ParentExpression = parentExpression;
		PropertyName = propertyName;
	}
}

/// <summary>
/// Represents a local value that needs to be captured before creating a binding.
/// </summary>
internal readonly struct LocalCapture
{
	/// <summary>The original expression (e.g., "this.TaxRate").</summary>
	public string OriginalExpression { get; }
	
	/// <summary>The capture variable name (e.g., "__capture_TaxRate").</summary>
	public string CaptureVariable { get; }
	
	/// <summary>The member name being captured (e.g., "TaxRate").</summary>
	public string MemberName { get; }

	public LocalCapture(string originalExpression, string captureVariable, string memberName)
	{
		OriginalExpression = originalExpression;
		CaptureVariable = captureVariable;
		MemberName = memberName;
	}
}

/// <summary>
/// Result of expression analysis including handlers and local captures.
/// </summary>
internal readonly struct ExpressionAnalysisResult
{
	/// <summary>Handlers for INPC subscription.</summary>
	public List<BindingHandler> Handlers { get; }
	
	/// <summary>Local values that need to be captured.</summary>
	public List<LocalCapture> Captures { get; }
	
	/// <summary>The transformed expression with this.X replaced by __capture_X.</summary>
	public string TransformedExpression { get; }
	
	/// <summary>Whether this expression has any binding properties (needs TypedBinding).</summary>
	public bool HasBindingProperties => Handlers.Count > 0;
	
	/// <summary>Whether this expression has local captures.</summary>
	public bool HasLocalCaptures => Captures.Count > 0;

	/// <summary>
	/// Whether this expression is a simple property chain that can have a setter generated.
	/// True for expressions like "Name" or "User.DisplayName" (no operators, method calls, or captures).
	/// </summary>
	public bool IsSettable { get; }

	public ExpressionAnalysisResult(List<BindingHandler> handlers, List<LocalCapture> captures, string transformedExpression, bool isSettable = false)
	{
		Handlers = handlers;
		Captures = captures;
		TransformedExpression = transformedExpression;
		IsSettable = isSettable;
	}
}

/// <summary>
/// Analyzes C# expressions to extract property access paths for INPC handlers.
/// </summary>
internal static class ExpressionAnalyzer
{
	// Pattern to match this.PropertyName
	private static readonly Regex ThisPrefixPattern = new Regex(
		@"\bthis\.(\w+)",
		RegexOptions.Compiled);

	/// <summary>
	/// Checks if the match is followed by a method call (opening parenthesis).
	/// </summary>
	private static bool IsMethodCall(Match match, string text)
	{
		var afterMatch = match.Index + match.Length;
		while (afterMatch < text.Length && char.IsWhiteSpace(text[afterMatch]))
			afterMatch++;
		return afterMatch < text.Length && text[afterMatch] == '(';
	}

	/// <summary>
	/// Analyzes an expression for mixed local+binding scenarios.
	/// </summary>
	/// <param name="expression">The C# expression (e.g., "Price * this.TaxRate")</param>
	/// <param name="sourceParameterName">The lambda parameter name (e.g., "__source")</param>
	/// <param name="dataType">Optional DataType symbol to filter handlers (only include members on this type)</param>
	/// <returns>Analysis result with handlers, captures, and transformed expression</returns>
	public static ExpressionAnalysisResult Analyze(string expression, string sourceParameterName = "__source", ITypeSymbol? dataType = null)
	{
		var captures = new List<LocalCapture>();
		var transformedExpression = expression;

		if (string.IsNullOrWhiteSpace(expression))
			return new ExpressionAnalysisResult(new List<BindingHandler>(), captures, expression);

		// Handle special prefixes first
		// Strip leading dot prefix (e.g., ".Name" -> "Name")
		if (transformedExpression.TrimStart().StartsWith(".", StringComparison.Ordinal) 
			&& !transformedExpression.TrimStart().StartsWith("..", StringComparison.Ordinal))
		{
			var trimmed = transformedExpression.TrimStart();
			transformedExpression = trimmed.Substring(1); // Remove the leading dot
		}
		
		// Strip BindingContext. prefix (e.g., "BindingContext.Name" -> "Name")
		if (transformedExpression.TrimStart().StartsWith("BindingContext.", StringComparison.Ordinal))
		{
			var trimmed = transformedExpression.TrimStart();
			transformedExpression = trimmed.Substring("BindingContext.".Length);
		}

		// Find all this.X patterns (excluding method calls) and replace with __capture_X
		var matches = ThisPrefixPattern.Matches(transformedExpression);
		var seenCaptures = new HashSet<string>();
		
		foreach (Match match in matches)
		{
			// Skip method calls - they can't be captured as they return values, not references
			if (IsMethodCall(match, transformedExpression))
				continue;
				
			var memberName = match.Groups[1].Value;
			if (seenCaptures.Add(memberName))
			{
				var captureVar = $"__capture_{memberName}";
				captures.Add(new LocalCapture(match.Value, captureVar, memberName));
			}
		}

		// Replace this.X with __capture_X in the expression
		foreach (var capture in captures)
		{
			transformedExpression = transformedExpression.Replace(capture.OriginalExpression, capture.CaptureVariable);
		}

		// Extract handlers from the transformed expression (which no longer has this.X)
		var handlers = ExtractHandlers(transformedExpression, sourceParameterName);

		// Filter handlers to only include:
		// 1. Top-level handlers (parent == __source) where property exists on DataType
		// 2. Nested handlers (parent != __source) - always keep since they're on intermediate objects
		// This excludes things like ToString() and __capture_X at the root level
		// Also exclude handlers whose parent is a capture variable (__capture_*) - those are local, not bindings
		if (dataType != null)
		{
			handlers = handlers.Where(h => 
				h.PropertyName == "." || 
				(h.ParentExpression != sourceParameterName &&  // Keep nested handlers...
				 !h.ParentExpression.Contains("__capture_")) ||  // ...unless parent is a capture variable
				HasProperty(dataType, h.PropertyName)  // Top-level must exist on DataType
			).ToList();
		}

		// Transform the expression to prefix root identifiers that are on DataType with __source.
		// This handles complex expressions like "(Price * __capture_TaxRate).ToString()" -> "(__source.Price * __capture_TaxRate).ToString()"
		if (dataType != null)
		{
			transformedExpression = TransformRootIdentifiers(transformedExpression, sourceParameterName, dataType, seenCaptures);
		}

		// Determine if this is a simple settable property chain (no captures, just member access)
		var isSettable = captures.Count == 0 && IsSimplePropertyChain(expression);

		return new ExpressionAnalysisResult(handlers, captures, transformedExpression, isSettable);
	}

	/// <summary>
	/// Checks if the expression is a simple property chain (e.g., "Name" or "User.DisplayName").
	/// Returns false for expressions with operators, method calls, or other complex constructs.
	/// </summary>
	private static bool IsSimplePropertyChain(string expression)
	{
		var trimmed = expression.Trim();
		
		// Strip leading prefixes that we normalize
		if (trimmed.StartsWith(".", StringComparison.Ordinal) && !trimmed.StartsWith("..", StringComparison.Ordinal))
			trimmed = trimmed.Substring(1);
		if (trimmed.StartsWith("BindingContext.", StringComparison.Ordinal))
			trimmed = trimmed.Substring("BindingContext.".Length);

		// Parse and check the syntax
		var tree = CSharpSyntaxTree.ParseText(trimmed, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		
		// Get the expression (skip compilation unit wrapper)
		var expr = root.DescendantNodes().OfType<ExpressionSyntax>().FirstOrDefault();
		if (expr == null)
			return false;

		return IsSettableExpression(expr);
	}

	/// <summary>
	/// Recursively checks if an expression is a simple settable property chain.
	/// </summary>
	private static bool IsSettableExpression(SyntaxNode node)
	{
		return node switch
		{
			// Simple identifier: "Name"
			IdentifierNameSyntax => true,
			// Member access: "User.Name" - check both parts
			MemberAccessExpressionSyntax memberAccess => 
				IsSettableExpression(memberAccess.Expression) && memberAccess.Name is IdentifierNameSyntax,
			// Anything else (operators, method calls, etc.) is not settable
			_ => false
		};
	}

	/// <summary>
	/// Transforms root identifiers that are on the DataType to use the source parameter prefix.
	/// </summary>
	private static string TransformRootIdentifiers(string expression, string sourceParameterName, ITypeSymbol dataType, HashSet<string> capturedIdentifiers)
	{
		// Parse as an expression and walk the syntax tree
		var tree = CSharpSyntaxTree.ParseText(expression, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();
		
		// Find all root identifiers that are directly on the DataType
		var identifiersToTransform = new List<(int start, int length, string replacement)>();
		
		foreach (var node in root.DescendantNodes())
		{
			if (node is IdentifierNameSyntax identifier)
			{
				var identifierName = identifier.Identifier.Text;
				
				// Skip if it's a captured variable
				if (identifierName.StartsWith("__capture_", StringComparison.Ordinal))
					continue;
				
				// Skip if it's part of this.X (already handled as capture)
				if (capturedIdentifiers.Contains(identifierName))
					continue;
				
				// Skip if it's the right side of a member access (e.g., User.Name - skip "Name")
				if (identifier.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Name == identifier)
					continue;
				
				// Check if this is a root-level invocation (not already prefixed)
				// e.g., GetDisplayName() should become __source.GetDisplayName()
				if (identifier.Parent is InvocationExpressionSyntax invocation && invocation.Expression == identifier)
				{
					// This is a standalone method call - check if method exists on DataType
					if (HasMember(dataType, identifierName))
					{
						identifiersToTransform.Add((identifier.SpanStart, identifier.Span.Length, $"{sourceParameterName}.{identifierName}"));
					}
					continue;
				}
				
				// Check if this identifier exists on the DataType
				if (HasMember(dataType, identifierName))
				{
					identifiersToTransform.Add((identifier.SpanStart, identifier.Span.Length, $"{sourceParameterName}.{identifierName}"));
				}
			}
		}
		
		// Apply transformations in reverse order to preserve positions
		var result = expression;
		foreach (var (start, length, replacement) in identifiersToTransform.OrderByDescending(t => t.start))
		{
			result = result.Substring(0, start) + replacement + result.Substring(start + length);
		}
		
		return result;
	}

	/// <summary>
	/// Checks if a type has a member (property, field, or method) with the given name.
	/// </summary>
	private static bool HasMember(ITypeSymbol type, string memberName)
	{
		var currentType = type;
		while (currentType != null)
		{
			foreach (var member in currentType.GetMembers(memberName))
			{
				if (member is IPropertySymbol || member is IFieldSymbol || member is IMethodSymbol)
					return true;
			}
			currentType = currentType.BaseType;
		}
		return false;
	}

	/// <summary>
	/// Checks if a type has a property or field (not method) with the given name.
	/// Used for INPC handlers which only track properties/fields.
	/// </summary>
	private static bool HasProperty(ITypeSymbol type, string memberName)
	{
		var currentType = type;
		while (currentType != null)
		{
			foreach (var member in currentType.GetMembers(memberName))
			{
				if (member is IPropertySymbol || member is IFieldSymbol)
					return true;
			}
			currentType = currentType.BaseType;
		}
		return false;
	}

	/// <summary>
	/// Extracts all property access handlers from an expression.
	/// </summary>
	/// <param name="expression">The C# expression (e.g., "User.Address.City")</param>
	/// <param name="sourceParameterName">The lambda parameter name (e.g., "__source")</param>
	/// <returns>List of handlers for INPC subscription</returns>
	public static List<BindingHandler> ExtractHandlers(string expression, string sourceParameterName = "__source")
	{
		var handlers = new List<BindingHandler>();

		if (string.IsNullOrWhiteSpace(expression))
			return handlers;

		// Parse as an expression
		var tree = CSharpSyntaxTree.ParseText(expression, new CSharpParseOptions(kind: SourceCodeKind.Script));
		var root = tree.GetRoot();

		// Find all member access expressions
		var memberAccesses = new List<MemberAccessExpressionSyntax>();
		var identifiers = new List<IdentifierNameSyntax>();
		CollectExpressions(root, memberAccesses, identifiers);

		// Build handlers from member access chains
		// For "User.Address.City", we want:
		// - (vm => vm, "User")
		// - (vm => vm.User, "Address")
		// - (vm => vm.User.Address, "City")

		foreach (var memberAccess in memberAccesses)
		{
			var chain = BuildPropertyChain(memberAccess);
			if (chain.Count > 0)
			{
				AddHandlersForChain(handlers, chain, sourceParameterName);
			}
		}

		// Handle bare identifiers (e.g., just "Name") that are not part of a member access
		foreach (var identifier in identifiers)
		{
			// Skip if this identifier is part of a member access expression
			if (identifier.Parent is MemberAccessExpressionSyntax)
				continue;
			
			// Skip if this identifier is the method name in an invocation
			// e.g., GetDisplayName in "GetDisplayName()" - parent chain: IdentifierName -> InvocationExpression
			if (identifier.Parent is InvocationExpressionSyntax)
				continue;
			
			// This is a standalone identifier - add handler for it
			handlers.Add(new BindingHandler(sourceParameterName, identifier.Identifier.Text));
		}

		// If no handlers were found but expression is non-empty, add a "." handler
		// This handles cases like method calls: {.GetDisplayName()} should subscribe to BindingContext
		if (handlers.Count == 0 && !string.IsNullOrWhiteSpace(expression))
		{
			handlers.Add(new BindingHandler(sourceParameterName, "."));
		}

		// Deduplicate handlers (same parent + property)
		return DeduplicateHandlers(handlers);
	}

	/// <summary>
	/// Recursively collects all MemberAccessExpressionSyntax and IdentifierNameSyntax nodes.
	/// </summary>
	private static void CollectExpressions(SyntaxNode node, List<MemberAccessExpressionSyntax> memberAccesses, List<IdentifierNameSyntax> identifiers)
	{
		if (node is MemberAccessExpressionSyntax memberAccess)
		{
			memberAccesses.Add(memberAccess);
		}
		else if (node is IdentifierNameSyntax identifier)
		{
			identifiers.Add(identifier);
		}

		foreach (var child in node.ChildNodes())
		{
			CollectExpressions(child, memberAccesses, identifiers);
		}
	}

	/// <summary>
	/// Builds a property chain from a member access expression.
	/// Returns the chain from root to leaf (e.g., ["User", "Address", "City"]).
	/// </summary>
	private static List<string> BuildPropertyChain(MemberAccessExpressionSyntax memberAccess)
	{
		var chain = new List<string>();
		ExpressionSyntax current = memberAccess;

		while (current is MemberAccessExpressionSyntax ma)
		{
			// Only include if accessing a simple name (property/field)
			if (ma.Name is SimpleNameSyntax simpleName)
			{
				chain.Insert(0, simpleName.Identifier.Text);
			}
			current = ma.Expression;
		}

		// The root should be an identifier (the first property after the source)
		if (current is IdentifierNameSyntax rootIdentifier)
		{
			chain.Insert(0, rootIdentifier.Identifier.Text);
		}

		return chain;
	}

	/// <summary>
	/// Adds handlers for each level of a property chain.
	/// </summary>
	private static void AddHandlersForChain(List<BindingHandler> handlers, List<string> chain, string sourceParameterName)
	{
		if (chain.Count == 0)
			return;

		// Build handlers for each level
		// Chain: ["User", "Address", "City"]
		// Handlers:
		//   - parent: __source, property: "User"
		//   - parent: __source.User, property: "Address"
		//   - parent: __source.User.Address, property: "City"

		var parentPath = sourceParameterName;
		for (int i = 0; i < chain.Count; i++)
		{
			var propertyName = chain[i];
			handlers.Add(new BindingHandler(parentPath, propertyName));
			
			// Build up the parent path for the next level
			parentPath = parentPath + "." + propertyName;
		}
	}

	/// <summary>
	/// Removes duplicate handlers (same parent expression and property name).
	/// </summary>
	private static List<BindingHandler> DeduplicateHandlers(List<BindingHandler> handlers)
	{
		var seen = new HashSet<string>();
		var result = new List<BindingHandler>();

		foreach (var handler in handlers)
		{
			var key = handler.ParentExpression + "|" + handler.PropertyName;
			if (seen.Add(key))
			{
				result.Add(handler);
			}
		}

		return result;
	}
}
