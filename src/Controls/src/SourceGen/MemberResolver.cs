using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Indicates where a member was found during resolution.
/// </summary>
internal enum MemberLocation
{
	/// <summary>Member exists only on the page/view type (this).</summary>
	This,
	
	/// <summary>Member exists only on the x:DataType type (BindingContext).</summary>
	DataType,
	
	/// <summary>Member exists on both types - ambiguous, requires explicit prefix.</summary>
	Both,
	
	/// <summary>Member not found on either type.</summary>
	Neither,
	
	/// <summary>Explicitly prefixed with 'this.' - forced to local.</summary>
	ForcedThis,
	
	/// <summary>Explicitly prefixed with '.' or 'BindingContext.' - forced to binding.</summary>
	ForcedDataType,
}

/// <summary>
/// Result of member resolution including location and the resolved expression.
/// </summary>
internal readonly struct MemberResolutionResult
{
	public MemberLocation Location { get; }
	
	/// <summary>The expression with prefix stripped (if any).</summary>
	public string Expression { get; }
	
	/// <summary>The first identifier in the expression (for member lookup).</summary>
	public string RootIdentifier { get; }
	
	/// <summary>True if the root identifier matches a well-known static type name.</summary>
	public bool ConflictsWithStaticType { get; }
	
	public MemberResolutionResult(MemberLocation location, string expression, string rootIdentifier, bool conflictsWithStaticType = false)
	{
		Location = location;
		Expression = expression;
		RootIdentifier = rootIdentifier;
		ConflictsWithStaticType = conflictsWithStaticType;
	}
	
	public bool IsBinding => Location == MemberLocation.DataType || Location == MemberLocation.ForcedDataType;
	public bool IsLocal => Location == MemberLocation.This || Location == MemberLocation.ForcedThis;
	public bool IsAmbiguous => Location == MemberLocation.Both;
	public bool IsNotFound => Location == MemberLocation.Neither;
}

/// <summary>
/// Resolves member expressions to determine if they reference 'this' members or x:DataType members.
/// </summary>
internal static class MemberResolver
{
	private const string ThisPrefix = "this.";
	private const string BindingContextPrefix = "BindingContext.";
	private const string DotPrefix = ".";

	/// <summary>
	/// Resolves a member expression to determine its location.
	/// </summary>
	/// <param name="expression">The expression (e.g., "User.Name", "this.Title", ".Count")</param>
	/// <param name="thisType">The type of the page/view (this)</param>
	/// <param name="dataType">The x:DataType type (may be null if not specified)</param>
	/// <param name="compilation">The compilation to check for type resolution (optional)</param>
	/// <returns>Resolution result with location and cleaned expression</returns>
	public static MemberResolutionResult Resolve(string expression, ITypeSymbol? thisType, ITypeSymbol? dataType, Compilation? compilation = null)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return new MemberResolutionResult(MemberLocation.Neither, expression, string.Empty);

		var trimmed = expression.Trim();
		
		// Check for explicit prefixes first
		if (trimmed.StartsWith(ThisPrefix, StringComparison.Ordinal))
		{
			var stripped = trimmed.Substring(ThisPrefix.Length);
			var root = GetRootIdentifier(stripped);
			return new MemberResolutionResult(MemberLocation.ForcedThis, stripped, root);
		}
		
		if (trimmed.StartsWith(BindingContextPrefix, StringComparison.Ordinal))
		{
			var stripped = trimmed.Substring(BindingContextPrefix.Length);
			var root = GetRootIdentifier(stripped);
			return new MemberResolutionResult(MemberLocation.ForcedDataType, stripped, root);
		}
		
		// "." prefix means BindingContext (shorthand)
		if (trimmed.StartsWith(DotPrefix, StringComparison.Ordinal) && trimmed.Length > 1 && char.IsLetter(trimmed[1]))
		{
			var stripped = trimmed.Substring(1);
			var root = GetRootIdentifier(stripped);
			return new MemberResolutionResult(MemberLocation.ForcedDataType, stripped, root);
		}

		// No explicit prefix - need to resolve
		var rootIdentifier = GetRootIdentifier(trimmed);
		if (string.IsNullOrEmpty(rootIdentifier))
			return new MemberResolutionResult(MemberLocation.Neither, trimmed, string.Empty);

		var onThis = thisType != null && HasMember(thisType, rootIdentifier);
		var onDataType = dataType != null && HasMember(dataType, rootIdentifier);
		
		// Check if root identifier also resolves to a type in the compilation
		var conflictsWithStatic = (onThis || onDataType) && 
			compilation != null && 
			ResolvesToType(compilation, rootIdentifier);

		MemberLocation location;
		if (onThis && onDataType)
			location = MemberLocation.Both;
		else if (onThis)
			location = MemberLocation.This;
		else if (onDataType)
			location = MemberLocation.DataType;
		else
			location = MemberLocation.Neither;

		return new MemberResolutionResult(location, trimmed, rootIdentifier, conflictsWithStatic);
	}
	
	/// <summary>
	/// Checks if an identifier resolves to a type in the compilation (including via global usings).
	/// </summary>
	private static bool ResolvesToType(Compilation compilation, string identifier)
	{
		// Collect all global using namespaces from the compilation's syntax trees
		var globalNamespaces = new HashSet<string>();
		
		foreach (var tree in compilation.SyntaxTrees)
		{
			var root = tree.GetRoot();
			foreach (var usingDirective in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>())
			{
				// Check for global usings (global using System;)
				if (usingDirective.GlobalKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GlobalKeyword))
				{
					var namespaceName = usingDirective.Name?.ToString();
					if (!string.IsNullOrEmpty(namespaceName))
						globalNamespaces.Add(namespaceName!);
				}
			}
		}
		
		// Check if the identifier resolves to a type in any of the global namespaces
		foreach (var ns in globalNamespaces)
		{
			var fullName = $"{ns}.{identifier}";
			var type = compilation.GetTypeByMetadataName(fullName);
			if (type != null)
				return true;
		}
		
		// Also check in the global namespace itself
		var globalType = compilation.GetTypeByMetadataName(identifier);
		if (globalType != null)
			return true;
		
		return false;
	}

	/// <summary>
	/// Extracts the first identifier from an expression.
	/// </summary>
	/// <remarks>
	/// Examples:
	/// - "User" → "User"
	/// - "User.Name" → "User"
	/// - "GetText()" → "GetText"
	/// - "Items.Count > 0" → "Items"
	/// </remarks>
	private static string GetRootIdentifier(string expression)
	{
		if (string.IsNullOrEmpty(expression))
			return string.Empty;

		int i = 0;
		// Skip leading whitespace
		while (i < expression.Length && char.IsWhiteSpace(expression[i]))
			i++;

		if (i >= expression.Length)
			return string.Empty;

		// First char must be letter or underscore
		if (!char.IsLetter(expression[i]) && expression[i] != '_')
			return string.Empty;

		int start = i;
		// Continue while valid identifier char
		while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
			i++;

		return expression.Substring(start, i - start);
	}

	/// <summary>
	/// Checks if an expression is a simple identifier (no operators, method calls, etc.).
	/// Used to determine if a "not found" error should be reported.
	/// </summary>
	public static bool IsSimpleIdentifier(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return false;

		var trimmed = expression.Trim();
		
		// Simple identifier: letters, digits, underscores only (and dots for member access)
		foreach (char c in trimmed)
		{
			if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
				return false;
		}
		
		// Must start with letter or underscore
		return char.IsLetter(trimmed[0]) || trimmed[0] == '_';
	}

	/// <summary>
	/// Checks if a type has a member (property or field) with the given name.
	/// </summary>
	private static bool HasMember(ITypeSymbol type, string memberName)
	{
		// Check this type and all base types
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
}
