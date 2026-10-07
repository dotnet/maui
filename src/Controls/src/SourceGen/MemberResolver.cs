using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

	/// <summary>True if the expression starts with a resolvable static type reference.</summary>
	public bool ResolvesToStaticType { get; }

	public MemberResolutionResult(MemberLocation location, string expression, string rootIdentifier, bool conflictsWithStaticType = false, bool resolvesToStaticType = false)
	{
		Location = location;
		Expression = expression;
		RootIdentifier = rootIdentifier;
		ConflictsWithStaticType = conflictsWithStaticType;
		ResolvesToStaticType = resolvesToStaticType;
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
	private static readonly ConditionalWeakTable<Compilation, GlobalUsingScope> GlobalUsingScopes = new();

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

		var resolvesToStaticType = false;
		var conflictsWithStatic = false;
		if (compilation != null)
		{
			if (onThis || onDataType)
				conflictsWithStatic = ResolvesToType(compilation, rootIdentifier, GetContainingNamespace(thisType));
			else
				resolvesToStaticType = StartsWithTypeReference(compilation, trimmed, GetContainingNamespace(thisType));
		}

		MemberLocation location;
		if (onThis && onDataType)
			location = MemberLocation.Both;
		else if (onThis)
			location = MemberLocation.This;
		else if (onDataType)
			location = MemberLocation.DataType;
		else
			location = MemberLocation.Neither;

		return new MemberResolutionResult(location, trimmed, rootIdentifier, conflictsWithStatic, resolvesToStaticType);
	}

	/// <summary>
	/// Checks if an identifier resolves to a type in the compilation (including via global usings).
	/// </summary>
	public static bool StartsWithTypeReference(Compilation compilation, string expression, string? containingNamespace = null)
	{
		foreach (var typeName in GetPossibleTypeNames(expression))
		{
			if (ResolvesToType(compilation, typeName, containingNamespace))
				return true;
		}

		return false;
	}

	public static bool ResolvesToType(Compilation compilation, string typeName, string? containingNamespace = null)
	{
		var normalizedTypeName = NormalizeTypeName(typeName);
		if (string.IsNullOrEmpty(normalizedTypeName))
			return false;

		if (GetPredefinedType(compilation, normalizedTypeName) != null)
			return true;

		if (compilation.GetTypeByMetadataName(normalizedTypeName) != null)
			return true;

		if (!string.IsNullOrEmpty(containingNamespace) &&
			compilation.GetTypeByMetadataName($"{containingNamespace}.{normalizedTypeName}") != null)
		{
			return true;
		}

		var globalUsings = GetGlobalUsingScope(compilation);
		var firstSeparator = normalizedTypeName.IndexOf('.');
		var rootIdentifier = firstSeparator < 0 ? normalizedTypeName : normalizedTypeName.Substring(0, firstSeparator);
		if (globalUsings.Aliases.TryGetValue(rootIdentifier, out var aliasTarget))
		{
			if (firstSeparator < 0 && aliasTarget is INamedTypeSymbol)
				return true;

			if (firstSeparator >= 0 && aliasTarget is INamespaceSymbol namespaceAlias)
			{
				var aliasNamespace = GetNamespaceName(namespaceAlias);
				if (!string.IsNullOrEmpty(aliasNamespace))
				{
					var aliasCandidate = aliasNamespace + normalizedTypeName.Substring(firstSeparator);
					if (compilation.GetTypeByMetadataName(aliasCandidate) != null)
						return true;
				}
			}
		}

		foreach (var ns in globalUsings.Namespaces)
		{
			var fullName = $"{ns}.{normalizedTypeName}";
			if (compilation.GetTypeByMetadataName(fullName) != null)
				return true;
		}

		return false;
	}

	private static GlobalUsingScope GetGlobalUsingScope(Compilation compilation)
	{
		return GlobalUsingScopes.GetValue(compilation, CreateGlobalUsingScope);
	}

	private static GlobalUsingScope CreateGlobalUsingScope(Compilation compilation)
	{
		var globalNamespaces = new HashSet<string>(StringComparer.Ordinal);
		var globalAliases = new Dictionary<string, INamespaceOrTypeSymbol>(StringComparer.Ordinal);

		foreach (var tree in compilation.SyntaxTrees)
		{
			if (tree.GetRoot() is not CompilationUnitSyntax compilationUnit)
				continue;

			SemanticModel? semanticModel = null;
			foreach (var usingDirective in compilationUnit.Usings)
			{
				if (!usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) ||
					usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
					continue;

				var namespaceName = NormalizeTypeName(usingDirective.Name?.WithoutTrivia().ToString() ?? string.Empty);
				if (string.IsNullOrEmpty(namespaceName))
					continue;

				if (usingDirective.Alias is { } alias)
				{
					semanticModel ??= compilation.GetSemanticModel(tree);
					if (semanticModel.GetSymbolInfo(usingDirective.Name!).Symbol is INamespaceOrTypeSymbol target)
						globalAliases[alias.Name.Identifier.ValueText] = target;
				}
				else
					globalNamespaces.Add(namespaceName);
			}
		}

		return new GlobalUsingScope(globalNamespaces, globalAliases);
	}

	private static INamedTypeSymbol? GetPredefinedType(Compilation compilation, string identifier)
	{
		var specialType = identifier switch
		{
			"bool" => SpecialType.System_Boolean,
			"byte" => SpecialType.System_Byte,
			"sbyte" => SpecialType.System_SByte,
			"short" => SpecialType.System_Int16,
			"ushort" => SpecialType.System_UInt16,
			"int" => SpecialType.System_Int32,
			"uint" => SpecialType.System_UInt32,
			"long" => SpecialType.System_Int64,
			"ulong" => SpecialType.System_UInt64,
			"nint" => SpecialType.System_IntPtr,
			"nuint" => SpecialType.System_UIntPtr,
			"char" => SpecialType.System_Char,
			"float" => SpecialType.System_Single,
			"double" => SpecialType.System_Double,
			"decimal" => SpecialType.System_Decimal,
			"string" => SpecialType.System_String,
			"object" => SpecialType.System_Object,
			_ => SpecialType.None,
		};

		return specialType == SpecialType.None ? null : compilation.GetSpecialType(specialType);
	}

	public static string? GetContainingNamespace(ITypeSymbol? typeSymbol)
		=> GetNamespaceName(typeSymbol?.ContainingNamespace);

	private static string? GetNamespaceName(INamespaceSymbol? namespaceSymbol)
	{
		if (namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace)
			return null;

		var names = new Stack<string>();
		var current = namespaceSymbol;
		while (current != null && !current.IsGlobalNamespace)
		{
			names.Push(current.Name);
			current = current.ContainingNamespace;
		}

		return string.Join(".", names);
	}

	private static string NormalizeTypeName(string typeName)
	{
		var normalized = typeName.Trim();
		const string GlobalAlias = "global::";
		if (normalized.StartsWith(GlobalAlias, StringComparison.Ordinal))
			normalized = normalized.Substring(GlobalAlias.Length);
		return normalized;
	}

	private static IEnumerable<string> GetPossibleTypeNames(string expression)
	{
		var leadingMemberAccess = ReadLeadingMemberAccess(expression);
		if (string.IsNullOrEmpty(leadingMemberAccess))
			yield break;

		var normalized = NormalizeTypeName(leadingMemberAccess);
		var parts = normalized.Split('.');
		for (var i = parts.Length - 1; i >= 1; i--)
			yield return string.Join(".", parts.Take(i));
	}

	private static string ReadLeadingMemberAccess(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return string.Empty;

		var trimmed = expression.TrimStart();
		var start = 0;
		var position = 0;
		const string GlobalAlias = "global::";
		if (trimmed.StartsWith(GlobalAlias, StringComparison.Ordinal))
			position = GlobalAlias.Length;

		if (!TryReadIdentifier(trimmed, ref position))
			return string.Empty;

		while (position < trimmed.Length && trimmed[position] == '.')
		{
			var beforeDot = position;
			position++;
			if (!TryReadIdentifier(trimmed, ref position))
			{
				position = beforeDot;
				break;
			}
		}

		return trimmed.Substring(start, position - start);
	}

	private static bool TryReadIdentifier(string text, ref int position)
	{
		if (position >= text.Length || (!char.IsLetter(text[position]) && text[position] != '_'))
			return false;

		position++;
		while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_'))
			position++;

		return true;
	}

	private sealed class GlobalUsingScope
	{
		public GlobalUsingScope(HashSet<string> namespaces, Dictionary<string, INamespaceOrTypeSymbol> aliases)
		{
			Namespaces = namespaces;
			Aliases = aliases;
		}

		public HashSet<string> Namespaces { get; }
		public Dictionary<string, INamespaceOrTypeSymbol> Aliases { get; }
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
	/// Checks if a type has a member with the given name.
	/// </summary>
	public static bool HasMember(ITypeSymbol? type, string memberName, bool includeMethods = false)
	{
		if (type == null)
			return false;

		var currentType = type;
		while (currentType != null)
		{
			foreach (var member in currentType.GetMembers(memberName))
			{
				if (member is IPropertySymbol || member is IFieldSymbol || (includeMethods && member is IMethodSymbol))
					return true;
			}
			currentType = currentType.BaseType;
		}

		if (type.TypeKind == TypeKind.Interface)
		{
			foreach (var interfaceType in type.AllInterfaces)
			{
				foreach (var member in interfaceType.GetMembers(memberName))
				{
					if (member is IPropertySymbol || member is IFieldSymbol || (includeMethods && member is IMethodSymbol))
						return true;
				}
			}
		}

		return false;
	}
}
