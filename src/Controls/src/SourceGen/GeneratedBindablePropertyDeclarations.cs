using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.BindingSourceGen;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Source generators in the same project can't see each other's output, so this generator never sees the
/// bindable properties another library's generator emits. A library describes what its generator emits by marking
/// its own attribute class with <c>Microsoft.Maui.Controls.Xaml.GeneratesBindablePropertyAttribute</c>, whose named
/// arguments spell out the shape (see <see cref="GeneratedBindablePropertyContract"/>). For every use of such an
/// attribute this produces stub declarations of those members. The stubs only exist in the compilation this generator
/// analyzes; the real compilation gets the library generator's real members.
/// </summary>
static class GeneratedBindablePropertyDeclarations
{
	public sealed record ContractProblem(string FilePath, TextSpan Span, LinePositionSpan LineSpan, string AttributeName, string ArgumentName, string Value);

	public sealed record StubDeclarations(EquatableArray<string> Stubs, EquatableArray<ContractProblem> Problems);

	public sealed record MarkedAttributesInReferences(EquatableArray<string> Names, bool AnyReferenceDeclaresMarker);

	sealed record AttributeUse(AttributeSyntax Syntax, string SimpleName, bool IsPossiblyAlias);

	sealed record SourceScan(List<AttributeUse> Uses, HashSet<string> MarkerClassNames, HashSet<string> GlobalUsingAliasNames);

	internal const string MarkerAttributeName = "Microsoft.Maui.Controls.Xaml.GeneratesBindablePropertyAttribute";

	const string MarkerSimpleName = "GeneratesBindableProperty";

	// A library puts the marker in the same assembly as its attributes, so only referenced assemblies that declare it need scanning.
	public static MarkedAttributesInReferences GetMarkedAttributeNamesInReferences(Compilation compilation, CancellationToken cancellationToken)
	{
		var names = new SortedSet<string>(StringComparer.Ordinal);
		var anyReferenceDeclaresMarker = false;
		foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
			anyReferenceDeclaresMarker |= AddMarkedAttributeNames(assembly, names, cancellationToken);

		return new MarkedAttributesInReferences(new EquatableArray<string>([.. names]), anyReferenceDeclaresMarker);
	}

	public static StubDeclarations GetStubDeclarations(Compilation compilation, MarkedAttributesInReferences referenceScan, CancellationToken cancellationToken)
	{
		// Most projects never see the marker, and then no attribute use can need a stub, so skip walking every declaration.
		if (!referenceScan.AnyReferenceDeclaresMarker && compilation.Assembly.GetTypeByMetadataName(MarkerAttributeName) is null)
			return new StubDeclarations(new EquatableArray<string>([]), new EquatableArray<ContractProblem>([]));

		var markedAttributeNamesInReferences = referenceScan.Names;
		var scan = ScanSyntaxTrees(compilation, cancellationToken);
		var markedAttributeNames = GetMarkedAttributeNames(markedAttributeNamesInReferences, scan.MarkerClassNames);

		var stubs = new List<string>();
		var problems = new List<ContractProblem>();
		var semanticModels = new Dictionary<SyntaxTree, SemanticModel>();
		foreach (var use in scan.Uses)
		{
			if (!CanBeMarked(use, markedAttributeNames, scan.GlobalUsingAliasNames))
				continue;

			if (!semanticModels.TryGetValue(use.Syntax.SyntaxTree, out var semanticModel))
				semanticModels[use.Syntax.SyntaxTree] = semanticModel = compilation.GetSemanticModel(use.Syntax.SyntaxTree);

			var stub = GetStubDeclaration(semanticModel, use.Syntax, problems, cancellationToken);
			if (stub is not null)
				stubs.Add(stub);
		}

		return new StubDeclarations(new EquatableArray<string>([.. stubs]), new EquatableArray<ContractProblem>([.. problems]));
	}

	public static Compilation AddStubs(Compilation compilation, StubDeclarations declarations, CancellationToken cancellationToken)
	{
		var stubs = declarations.Stubs;
		if (stubs.Length == 0)
			return compilation;

		var source = new StringBuilder("#nullable enable\n");
		foreach (var stub in stubs.Distinct(StringComparer.Ordinal).OrderBy(static stub => stub, StringComparer.Ordinal))
			source.AppendLine(stub);

		var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
		return compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source.ToString(), options, cancellationToken: cancellationToken));
	}

	// Descends only through declarations: creating syntax nodes for method bodies of a whole project would cost far more than the work it saves.
	static SourceScan ScanSyntaxTrees(Compilation compilation, CancellationToken cancellationToken)
	{
		var scan = new SourceScan([], new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal));
		foreach (var tree in compilation.SyntaxTrees)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (tree.GetRoot(cancellationToken) is not CompilationUnitSyntax unit)
				continue;

			foreach (var directive in unit.Usings)
			{
				if (directive.Alias is not null && directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
					scan.GlobalUsingAliasNames.Add(directive.Alias.Name.Identifier.ValueText);
			}

			ScanMembers(unit.Members, scan);
		}

		return scan;
	}

	static void ScanMembers(SyntaxList<MemberDeclarationSyntax> members, SourceScan scan)
	{
		foreach (var member in members)
		{
			switch (member)
			{
				case BaseNamespaceDeclarationSyntax @namespace:
					ScanMembers(@namespace.Members, scan);
					break;
				case TypeDeclarationSyntax type when type is not InterfaceDeclarationSyntax:
					AddAttributeUses(type.AttributeLists, scan, canDeclareMarkerClass: type is ClassDeclarationSyntax, type.Identifier.ValueText);
					ScanMembers(type.Members, scan);
					break;
				case ConstructorDeclarationSyntax constructor:
					AddAttributeUses(constructor.AttributeLists, scan, canDeclareMarkerClass: false, null);
					break;
				case PropertyDeclarationSyntax property:
					AddAttributeUses(property.AttributeLists, scan, canDeclareMarkerClass: false, null);
					break;
			}
		}
	}

	static void AddAttributeUses(SyntaxList<AttributeListSyntax> attributeLists, SourceScan scan, bool canDeclareMarkerClass, string? declaredClassName)
	{
		foreach (var list in attributeLists)
		{
			foreach (var attribute in list.Attributes)
			{
				var simpleName = GetSimpleName(attribute.Name);
				if (canDeclareMarkerClass && declaredClassName is not null && IsAliasOfAttribute(simpleName, MarkerSimpleName))
					scan.MarkerClassNames.Add(declaredClassName);

				scan.Uses.Add(new AttributeUse(attribute, simpleName, HasUsingAlias(attribute, simpleName)));
			}
		}
	}

	static IReadOnlyCollection<string> GetMarkedAttributeNames(EquatableArray<string> namesInReferences, HashSet<string> markerClassNamesInSource)
	{
		var names = new HashSet<string>(namesInReferences, StringComparer.Ordinal);
		foreach (var className in markerClassNamesInSource)
		{
			names.Add(className);
			const string suffix = "Attribute";
			if (className.EndsWith(suffix, StringComparison.Ordinal))
				names.Add(className.Substring(0, className.Length - suffix.Length));
		}

		return names;
	}

	static bool CanBeMarked(AttributeUse use, IReadOnlyCollection<string> markedAttributeNames, HashSet<string> globalUsingAliasNames)
		=> use.IsPossiblyAlias
			|| markedAttributeNames.Contains(use.SimpleName)
			|| globalUsingAliasNames.Any(alias => IsAliasOfAttribute(alias, use.SimpleName));

	static string? GetStubDeclaration(SemanticModel semanticModel, AttributeSyntax attributeSyntax, List<ContractProblem> problems, CancellationToken cancellationToken)
	{
		if (attributeSyntax.Parent?.Parent is null)
			return null;

		var constructorInfo = semanticModel.GetSymbolInfo(attributeSyntax, cancellationToken);
		var attributeClass = (constructorInfo.Symbol ?? constructorInfo.CandidateSymbols.FirstOrDefault())?.ContainingType;
		if (attributeClass is null)
			return null;

		var contract = GeneratedBindablePropertyContract.TryCreate(attributeClass);
		if (contract is null)
			return null;

		var target = semanticModel.GetDeclaredSymbol(attributeSyntax.Parent.Parent, cancellationToken);
		if (target is null)
			return null;

		var attribute = target.GetAttributes().FirstOrDefault(data =>
			data.ApplicationSyntaxReference is not null
			&& data.ApplicationSyntaxReference.SyntaxTree == attributeSyntax.SyntaxTree
			&& data.ApplicationSyntaxReference.Span == attributeSyntax.Span);

		if (attribute is null)
			return null;

		var location = attributeSyntax.GetLocation();
		var reporter = new ContractProblemReporter(problems, location.SourceTree?.FilePath ?? string.Empty, location.SourceSpan, location.GetLineSpan().Span, attributeClass.Name);

		return target switch
		{
			INamedTypeSymbol host when contract.IsAttached => CreateStub(host, attribute, contract, reporter),
			IMethodSymbol { MethodKind: MethodKind.Constructor } constructor when contract.IsAttached => CreateStub(constructor.ContainingType, attribute, contract, reporter),
			IPropertySymbol property when !contract.IsAttached => CreateStub(property.ContainingType, attribute, contract, reporter, property),
			_ => null,
		};
	}

	static bool AddMarkedAttributeNames(IAssemblySymbol assembly, SortedSet<string> names, CancellationToken cancellationToken)
	{
		if (assembly.GetTypeByMetadataName(MarkerAttributeName) is null)
			return false;

		var pending = new Stack<INamespaceOrTypeSymbol>();
		pending.Push(assembly.GlobalNamespace);
		while (pending.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (var member in pending.Pop().GetMembers())
			{
				if (member is INamespaceOrTypeSymbol container)
					pending.Push(container);

				if (member is INamedTypeSymbol type && type.GetAttributes().Any(static data => string.Equals(data.AttributeClass?.ToString(), MarkerAttributeName, StringComparison.Ordinal)))
				{
					names.Add(type.Name);
					const string suffix = "Attribute";
					if (type.Name.EndsWith(suffix, StringComparison.Ordinal))
						names.Add(type.Name.Substring(0, type.Name.Length - suffix.Length));
				}
			}
		}

		return true;
	}

	static bool IsAliasOfAttribute(string aliasName, string usedName)
		=> string.Equals(aliasName, usedName, StringComparison.Ordinal) || string.Equals(aliasName, usedName + "Attribute", StringComparison.Ordinal);

	static string GetSimpleName(NameSyntax name) => name switch
	{
		QualifiedNameSyntax qualified => GetSimpleName(qualified.Right),
		AliasQualifiedNameSyntax aliasQualified => GetSimpleName(aliasQualified.Name),
		SimpleNameSyntax simple => simple.Identifier.ValueText,
		_ => string.Empty,
	};

	static bool HasUsingAlias(SyntaxNode attributeSyntax, string simpleName)
	{
		foreach (var ancestor in attributeSyntax.Ancestors())
		{
			var usings = ancestor switch
			{
				BaseNamespaceDeclarationSyntax @namespace => @namespace.Usings,
				CompilationUnitSyntax unit => unit.Usings,
				_ => default,
			};

			if (usings.Any(directive => directive.Alias is not null && IsAliasOfAttribute(directive.Alias.Name.Identifier.ValueText, simpleName)))
				return true;
		}

		return false;
	}

	static string? CreateStub(INamedTypeSymbol host, AttributeData attribute, GeneratedBindablePropertyContract contract, ContractProblemReporter reporter, IPropertySymbol? decoratedProperty = null)
	{
		var name = contract.GetName(attribute, decoratedProperty);
		if (name is null)
			return null;

		var members = new List<string>();

		var fieldName = GeneratedBindablePropertyContract.Format(contract.FieldNameFormat, name);
		var fieldAccessibility = contract.GetAccessibility(attribute, contract.FieldAccessibilityNamedArgument, reporter);
		if (fieldName is not null && fieldAccessibility is not null && !HasMember(host, fieldName))
			members.Add($"{fieldAccessibility} static readonly global::Microsoft.Maui.Controls.BindableProperty {fieldName} = default!;");

		if (contract.IsAttached && contract.GetValueType(attribute) is ITypeSymbol valueType)
		{
			var type = valueType.ToFQDisplayString();

			var getterName = GeneratedBindablePropertyContract.Format(contract.GetterNameFormat, name);
			var getterAccessibility = contract.GetAccessibility(attribute, contract.GetterAccessibilityNamedArgument, reporter);
			if (getterName is not null && getterAccessibility is not null && !HasMember(host, getterName))
				members.Add($"{getterAccessibility} static {type} {getterName}(global::Microsoft.Maui.Controls.BindableObject bindable) => default!;");

			var setterName = GeneratedBindablePropertyContract.Format(contract.SetterNameFormat, name);
			var setterAccessibility = contract.GetAccessibility(attribute, contract.SetterAccessibilityNamedArgument, reporter);
			if (!contract.GetIsReadOnly(attribute) && setterName is not null && setterAccessibility is not null && !HasMember(host, setterName))
				members.Add($"{setterAccessibility} static void {setterName}(global::Microsoft.Maui.Controls.BindableObject bindable, {type} value) {{ }}");
		}

		return members.Count == 0 ? null : WrapInHost(host, members);
	}

	// A member declared in source, even the library's own hand-written partial, must not be declared twice.
	static bool HasMember(INamedTypeSymbol host, string name) => host.GetMembers(name).Length > 0;

	static string GetNamespaceName(INamespaceSymbol namespaceSymbol)
	{
		var names = new Stack<string>();
		for (var current = namespaceSymbol; !current.IsGlobalNamespace; current = current.ContainingNamespace)
			names.Push(current.Name);
		return string.Join(".", names);
	}

	static string GetTypeKeywords(INamedTypeSymbol type) => (type.TypeKind == TypeKind.Struct, type.IsRecord) switch
	{
		(true, true) => "record struct",
		(true, false) => "struct",
		(false, true) => "record",
		_ => "class",
	};

	static string WrapInHost(INamedTypeSymbol host, IEnumerable<string> members)
	{
		var types = new Stack<INamedTypeSymbol>();
		for (var type = host; type is not null; type = type.ContainingType)
			types.Push(type);

		var builder = new StringBuilder();
		var hasNamespace = !host.ContainingNamespace.IsGlobalNamespace;
		if (hasNamespace)
			builder.Append("namespace ").Append(GetNamespaceName(host.ContainingNamespace)).Append(" { ");

		foreach (var type in types)
		{
			builder.Append("partial ").Append(GetTypeKeywords(type)).Append(' ').Append(type.Name);
			if (type.TypeParameters.Length > 0)
				builder.Append('<').Append(string.Join(", ", type.TypeParameters.Select(static parameter => parameter.Name))).Append('>');
			builder.Append(" { ");
		}

		builder.Append(string.Join(" ", members));

		foreach (var _ in types)
			builder.Append(" }");
		if (hasNamespace)
			builder.Append(" }");

		return builder.ToString();
	}
}

/// <summary>
/// What a library's generator emits for one attribute class, read from the named arguments of the library's own copy of
/// <c>Microsoft.Maui.Controls.Xaml.GeneratesBindablePropertyAttribute</c>. No naming convention lives in MAUI: every
/// default below is only the value an omitted named argument means.
/// </summary>
sealed class GeneratedBindablePropertyContract
{
	const int NotSpecified = -1;
	const string NamePlaceholder = "{name}";

	GeneratedBindablePropertyContract(IReadOnlyDictionary<string, TypedConstant> arguments)
	{
		Arguments = arguments;
		IsAttached = GetBool("IsAttached", false);
		NameConstructorArgument = GetInt("NameConstructorArgument");
		NameNamedArgument = GetString("NameNamedArgument");
		ValueTypeGenericArgument = GetInt("ValueTypeGenericArgument");
		ValueTypeConstructorArgument = GetInt("ValueTypeConstructorArgument");
		FieldNameFormat = arguments.ContainsKey("FieldNameFormat") ? GetString("FieldNameFormat") : "{name}Property";
		GetterNameFormat = arguments.ContainsKey("GetterNameFormat") ? GetString("GetterNameFormat") : "Get{name}";
		SetterNameFormat = arguments.ContainsKey("SetterNameFormat") ? GetString("SetterNameFormat") : "Set{name}";
		IsReadOnly = GetBool("IsReadOnly", false);
		IsReadOnlyNamedArgument = GetString("IsReadOnlyNamedArgument");
		FieldAccessibilityNamedArgument = GetString("FieldAccessibilityNamedArgument");
		GetterAccessibilityNamedArgument = GetString("GetterAccessibilityNamedArgument");
		SetterAccessibilityNamedArgument = GetString("SetterAccessibilityNamedArgument");
		AccessibilityValues = GetStrings("AccessibilityValues");
	}

	IReadOnlyDictionary<string, TypedConstant> Arguments { get; }

	public bool IsAttached { get; }
	public int NameConstructorArgument { get; }
	public string? NameNamedArgument { get; }
	public int ValueTypeGenericArgument { get; }
	public int ValueTypeConstructorArgument { get; }
	public string? FieldNameFormat { get; }
	public string? GetterNameFormat { get; }
	public string? SetterNameFormat { get; }
	public bool IsReadOnly { get; }
	public string? IsReadOnlyNamedArgument { get; }
	public string? FieldAccessibilityNamedArgument { get; }
	public string? GetterAccessibilityNamedArgument { get; }
	public string? SetterAccessibilityNamedArgument { get; }
	public ImmutableArray<string> AccessibilityValues { get; }

	public static GeneratedBindablePropertyContract? TryCreate(INamedTypeSymbol attributeClass)
	{
		var marker = attributeClass.OriginalDefinition.GetAttributes()
			.FirstOrDefault(static data => string.Equals(data.AttributeClass?.ToString(), GeneratedBindablePropertyDeclarations.MarkerAttributeName, StringComparison.Ordinal));

		if (marker is null)
			return null;

		return new GeneratedBindablePropertyContract(marker.NamedArguments.ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal));
	}

	public string? GetName(AttributeData attribute, IPropertySymbol? decoratedProperty)
	{
		string? name = null;
		if (NameConstructorArgument != NotSpecified)
			name = GetConstructorArgument(attribute, NameConstructorArgument) as string;
		else if (NameNamedArgument is not null)
			name = GetNamedArgument(attribute, NameNamedArgument) as string;
		else
			name = decoratedProperty?.Name;

		return name is not null && SyntaxFacts.IsValidIdentifier(name) ? name : null;
	}

	public ITypeSymbol? GetValueType(AttributeData attribute)
	{
		ITypeSymbol? valueType = null;
		if (ValueTypeGenericArgument != NotSpecified)
		{
			var typeArguments = attribute.AttributeClass?.TypeArguments ?? default;
			valueType = ValueTypeGenericArgument < typeArguments.Length ? typeArguments[ValueTypeGenericArgument] : null;
		}
		else if (ValueTypeConstructorArgument != NotSpecified)
		{
			valueType = GetConstructorArgument(attribute, ValueTypeConstructorArgument) as ITypeSymbol;
		}

		return valueType is null || valueType.TypeKind == TypeKind.Error ? null : valueType;
	}

	public bool GetIsReadOnly(AttributeData attribute)
		=> IsReadOnly || (IsReadOnlyNamedArgument is not null && GetNamedArgument(attribute, IsReadOnlyNamedArgument) is true);

	/// <returns>The C# keywords, or <c>null</c> when the library says the member is not generated or the value can't be read (reported).</returns>
	public string? GetAccessibility(AttributeData attribute, string? namedArgument, ContractProblemReporter reporter)
	{
		if (namedArgument is null || !TryGetNamedArgument(attribute, namedArgument, out var constant))
			return "public";

		if (constant.Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } && constant.Value is not null)
		{
			var index = Convert.ToInt64(constant.Value, CultureInfo.InvariantCulture);
			if (index >= 0 && index < AccessibilityValues.Length && TryNormalizeAccessibility(AccessibilityValues[(int)index], out var mapped))
				return mapped;

			reporter.Report(namedArgument, constant.Value.ToString() ?? string.Empty);
			return null;
		}

		if (constant.Value is string keywords && TryNormalizeAccessibility(keywords, out var normalized))
			return normalized;

		reporter.Report(namedArgument, constant.Value?.ToString() ?? string.Empty);
		return null;
	}

	// The library's own spelling must be C# keywords; an empty string means "not generated".
	static bool TryNormalizeAccessibility(string keywords, out string? normalized)
	{
		var tokens = keywords.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
		Array.Sort(tokens, StringComparer.Ordinal);
		normalized = string.Join(" ", tokens) switch
		{
			"public" => "public",
			"internal" => "internal",
			"internal protected" => "protected internal",
			"protected" => "protected",
			"private protected" => "private protected",
			"private" => "private",
			_ => null,
		};

		return normalized is not null || tokens.Length == 0;
	}

	public static string? Format(string? format, string name)
		=> string.IsNullOrEmpty(format) ? null : format!.Replace(NamePlaceholder, name);

	static object? GetConstructorArgument(AttributeData attribute, int index)
		=> index >= 0 && index < attribute.ConstructorArguments.Length ? attribute.ConstructorArguments[index].Value : null;

	static object? GetNamedArgument(AttributeData attribute, string name)
		=> TryGetNamedArgument(attribute, name, out var constant) ? constant.Value : null;

	static bool TryGetNamedArgument(AttributeData attribute, string name, out TypedConstant constant)
	{
		foreach (var pair in attribute.NamedArguments)
		{
			if (string.Equals(pair.Key, name, StringComparison.Ordinal))
			{
				constant = pair.Value;
				return true;
			}
		}

		constant = default;
		return false;
	}

	bool GetBool(string name, bool fallback) => Arguments.TryGetValue(name, out var constant) && constant.Value is bool value ? value : fallback;

	int GetInt(string name) => Arguments.TryGetValue(name, out var constant) && constant.Value is int value ? value : NotSpecified;

	string? GetString(string name) => Arguments.TryGetValue(name, out var constant) ? constant.Value as string : null;

	ImmutableArray<string> GetStrings(string name)
		=> Arguments.TryGetValue(name, out var constant) && constant.Kind == TypedConstantKind.Array
			? [.. constant.Values.Select(static value => value.Value as string ?? string.Empty)]
			: [];
}

sealed class ContractProblemReporter(List<GeneratedBindablePropertyDeclarations.ContractProblem> problems, string filePath, TextSpan span, LinePositionSpan lineSpan, string attributeName)
{
	public void Report(string argumentName, string value)
		=> problems.Add(new GeneratedBindablePropertyDeclarations.ContractProblem(filePath, span, lineSpan, attributeName, argumentName, value));
}
