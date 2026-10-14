using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;

/// <summary>
/// Incremental source generator that generates <c>IQueryPropertyAttributable</c> implementations
/// for classes decorated with <c>[QueryProperty]</c> attributes.
/// </summary>
/// <remarks>
/// <para>
/// The <c>[QueryProperty]</c> attribute is traditionally processed at runtime via reflection in
/// <c>ShellContent.ApplyQueryAttributes</c>. This generator creates a compile-time implementation
/// that is trim-safe, NativeAOT-compatible, and avoids reflection overhead.
/// </para>
/// <para>
/// For each partial class with <c>[QueryProperty]</c> attributes, the generator emits a partial class
/// that explicitly implements <c>IQueryPropertyAttributable.ApplyQueryPropertyAttributes</c>. This
/// dedicated interface lets generated query-property setters replace only the reflection path while
/// preserving any user implementation of <c>IQueryAttributable</c>.
/// </para>
/// <para>
/// For non-partial classes, the generator emits a MAUI1200 warning and no code is generated.
/// The existing reflection fallback continues to work for these classes.
/// </para>
/// <para>
/// The generated code requests converted values from <c>QueryPropertyAttributeContext</c>, which
/// centralizes the same decoding, conversion, and clearing behavior used by the reflection fallback.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public class QueryPropertyGenerator : IIncrementalGenerator
{
	const string QueryPropertyAttributeFullName = "Microsoft.Maui.Controls.QueryPropertyAttribute";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Use ForAttributeWithMetadataName for better performance when filtering by attribute
		var classesWithQueryProperty = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				QueryPropertyAttributeFullName,
				predicate: static (node, _) => node is TypeDeclarationSyntax,
				transform: static (ctx, ct) => GetClassInfo(ctx, ct));

		// Combine with global options to check if reflection fallback is available
		var classesWithOptions = classesWithQueryProperty
			.Combine(context.AnalyzerConfigOptionsProvider);

		// Generate source for each class
		context.RegisterSourceOutput(classesWithOptions, static (spc, pair) =>
		{
			var classInfo = pair.Left;
			var options = pair.Right;

			// Check if the reflection fallback is disabled (trimmed/AOT apps)
			bool reflectionFallbackDisabled = options.GlobalOptions.IsFalse("build_property.MauiQueryPropertyAttributeSupport");

			// Report diagnostics — escalate non-partial warning to error if reflection is off
			foreach (var diagnostic in classInfo.Diagnostics)
			{
				if (reflectionFallbackDisabled
					&& diagnostic.Id == Descriptors.QueryPropertyClassMustBePartial.Id
					&& diagnostic.Severity != DiagnosticSeverity.Error)
				{
					// Re-create as error when reflection is disabled
					spc.ReportDiagnostic(Diagnostic.Create(
						Descriptors.QueryPropertyClassMustBePartialNoFallback,
						diagnostic.Location,
						diagnostic.AdditionalLocations,
						diagnostic.Properties,
						classInfo.ClassName));
				}
				else if (reflectionFallbackDisabled && diagnostic.Severity != DiagnosticSeverity.Error)
				{
					spc.ReportDiagnostic(CreateErrorDiagnostic(diagnostic));
				}
				else
				{
					spc.ReportDiagnostic(diagnostic);
				}
			}

			// Only generate if there are valid properties
			if (classInfo.Diagnostics.Length == 0 && classInfo.PropertyMappings.Length > 0)
			{
				var source = GenerateSource(classInfo);
				var typePath = classInfo.ContainingTypes.Length > 0
					? string.Join(".", classInfo.ContainingTypes) + "." + classInfo.ClassName
					: classInfo.ClassName;
				var hintName = SanitizeHintName(classInfo.Namespace is not null
					? $"{classInfo.Namespace}.{typePath}_QueryProperty.g.cs"
					: $"{typePath}_QueryProperty.g.cs");
				spc.AddSource(hintName, source);
			}
		});
	}

	private static ClassInfo GetClassInfo(GeneratorAttributeSyntaxContext context, System.Threading.CancellationToken cancellationToken)
	{
		var classDecl = (TypeDeclarationSyntax)context.TargetNode;
		var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

		var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

		// Check if the class is partial
		var isPartial = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		if (!isPartial)
		{
			var diagnostic = Diagnostic.Create(
				Descriptors.QueryPropertyClassMustBePartial,
				classDecl.Identifier.GetLocation(),
				classSymbol.Name);
			diagnostics.Add(diagnostic);
			return new ClassInfo(classSymbol.Name, classSymbol.ToFQDisplayString(), null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
		}

		if (classDecl is not ClassDeclarationSyntax ||
			classSymbol.Arity > 0 ||
			classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.FileKeyword)) ||
			HasGenericContainingType(classSymbol))
		{
			diagnostics.Add(Diagnostic.Create(
				Descriptors.QueryPropertyTypeNotSupported,
				classDecl.Identifier.GetLocation(),
				classSymbol.Name));
			return new ClassInfo(classSymbol.Name, classSymbol.ToFQDisplayString(), null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
		}

		// For nested classes, verify the entire containing type chain is partial.
		// The generator emits partial class wrappers for each containing type,
		// which requires the original declarations to also be partial.
		var containingType = classSymbol.ContainingType;
		while (containingType is not null)
		{
			// Check all syntax declarations of the containing type for the partial modifier
			bool containingIsPartial = false;
			foreach (var syntaxRef in containingType.DeclaringSyntaxReferences)
			{
				if (syntaxRef.GetSyntax(cancellationToken) is ClassDeclarationSyntax containingDecl
					&& containingDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
				{
					containingIsPartial = true;
					break;
				}
			}

			if (!containingIsPartial)
			{
				var diagnostic = Diagnostic.Create(
					Descriptors.QueryPropertyContainingClassMustBePartial,
					classDecl.Identifier.GetLocation(),
					containingType.Name,
					classSymbol.Name);
				diagnostics.Add(diagnostic);
				return new ClassInfo(classSymbol.Name, classSymbol.ToFQDisplayString(), null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
			}

			containingType = containingType.ContainingType;
		}

		// Skip generation if the class already explicitly implements the generated-code contract.
		var iQueryPropertyAttributable = context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IQueryPropertyAttributable");
		if (iQueryPropertyAttributable is not null)
		{
			foreach (var iface in classSymbol.Interfaces)
			{
				if (SymbolEqualityComparer.Default.Equals(iface, iQueryPropertyAttributable))
					return new ClassInfo(classSymbol.Name, classSymbol.ToFQDisplayString(), null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
			}
		}

		// Include inherited QueryProperty attributes because the runtime reflection path does.
		var queryPropertyAttributes = GetQueryPropertyAttributes(classSymbol);

		// Extract property mappings
		var propertyMappings = ImmutableArray.CreateBuilder<PropertyMapping>();

		foreach (var attr in queryPropertyAttributes)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (attr.ConstructorArguments.Length != 2)
				continue;

			var propertyName = attr.ConstructorArguments[0].Value as string;
			var queryId = attr.ConstructorArguments[1].Value as string;

			if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(queryId))
			{
				var diagnostic = Diagnostic.Create(
					Descriptors.QueryPropertyAttributeInvalidArguments,
					classDecl.Identifier.GetLocation(),
					classSymbol.Name);
				diagnostics.Add(diagnostic);
				continue;
			}

			// Find the property — walk the type hierarchy since the property may be inherited
			var property = FindPropertyInHierarchy(classSymbol, propertyName!);

			if (property is null)
			{
				var diagnostic = Diagnostic.Create(
					Descriptors.QueryPropertyNotFound,
					classDecl.Identifier.GetLocation(),
					propertyName,
					classSymbol.Name);
				diagnostics.Add(diagnostic);
				continue;
			}

			if (property.SetMethod is null || property.SetMethod.DeclaredAccessibility != Accessibility.Public)
			{
				var diagnostic = Diagnostic.Create(
					Descriptors.QueryPropertySetterNotPublic,
					classDecl.Identifier.GetLocation(),
					propertyName);
				diagnostics.Add(diagnostic);
				continue;
			}

			var propertyType = property.Type;
			var propertyTypeName = propertyType.ToFQDisplayString();
			var contextTypeName = propertyTypeName;
			if (propertyType is INamedTypeSymbol { IsGenericType: true } namedType &&
				namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
			{
				contextTypeName = namedType.TypeArguments[0].ToFQDisplayString();
			}

			propertyMappings.Add(new PropertyMapping(propertyName!, queryId!, propertyTypeName, contextTypeName));
		}

		var propertyMappingsArray = propertyMappings.ToImmutable();

		// Get namespace
		var namespaceName = classSymbol.ContainingNamespace?.ToString();
		if (string.IsNullOrEmpty(namespaceName) || namespaceName == "<global namespace>")
			namespaceName = null;

		// Collect containing type chain for nested classes (outermost first)
		var containingTypes = ImmutableArray.CreateBuilder<string>();
		var current = classSymbol.ContainingType;
		while (current is not null)
		{
			containingTypes.Insert(0, current.Name);
			current = current.ContainingType;
		}

		return new ClassInfo(
			classSymbol.Name,
			classSymbol.ToFQDisplayString(),
			namespaceName,
			containingTypes.ToImmutable(),
			propertyMappingsArray,
			diagnostics.ToImmutable());
	}

	private static ImmutableArray<AttributeData> GetQueryPropertyAttributes(INamedTypeSymbol classSymbol)
	{
		var attributes = ImmutableArray.CreateBuilder<AttributeData>();
		for (var current = classSymbol; current is not null; current = current.BaseType)
		{
			foreach (var attribute in current.GetAttributes())
			{
				if (attribute.AttributeClass?.ToFQDisplayString() == $"global::{QueryPropertyAttributeFullName}")
					attributes.Add(attribute);
			}
		}
		return attributes.ToImmutable();
	}

	private static bool HasGenericContainingType(INamedTypeSymbol classSymbol)
	{
		for (var current = classSymbol.ContainingType; current is not null; current = current.ContainingType)
		{
			if (current.Arity > 0)
				return true;
		}
		return false;
	}

	private static string SanitizeHintName(string hintName) =>
		new(hintName.Select(character =>
			char.IsLetterOrDigit(character) || character is '.' or '_' or '-'
				? character
				: '_').ToArray());

	private static Diagnostic CreateErrorDiagnostic(Diagnostic diagnostic)
	{
		var descriptor = new DiagnosticDescriptor(
			diagnostic.Id,
			diagnostic.Descriptor.Title,
			diagnostic.GetMessage(),
			diagnostic.Descriptor.Category,
			DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			diagnostic.Descriptor.Description,
			diagnostic.Descriptor.HelpLinkUri,
			diagnostic.Descriptor.CustomTags.ToArray());

		return Diagnostic.Create(
			descriptor,
			diagnostic.Location,
			diagnostic.AdditionalLocations,
			diagnostic.Properties);
	}

	private static string GenerateSource(ClassInfo classInfo)
	{
		var compilationUnit = BuildCompilationUnit(classInfo);
		var normalizedCode = compilationUnit.NormalizeWhitespace().ToFullString();

		// Prepend the auto-generated header comment
		return AutoGeneratedHeaderText + "\r\n" + normalizedCode;
	}

	private static CompilationUnitSyntax BuildCompilationUnit(ClassInfo classInfo)
	{
		// Create using directives
		var usingDirectives = new[]
		{
			SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
			SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
		};

		// Build class members
		var classMembers = new List<MemberDeclarationSyntax>
		{
			BuildQueryPropertyTargetTypeProperty(classInfo)
		};

		// Add ApplyQueryPropertyAttributes method
		classMembers.Add(BuildApplyQueryPropertyAttributesMethod(classInfo));

		// Build class declaration implementing the generated query-property contract.
		var classDeclaration = SyntaxFactory.ClassDeclaration(EscapeIdentifier(classInfo.ClassName))
			.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
			.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
				SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("global::Microsoft.Maui.Controls.IQueryPropertyAttributable")))))
			.WithMembers(SyntaxFactory.List(classMembers));

		// Wrap in containing type chain for nested classes (innermost → outermost)
		MemberDeclarationSyntax topMember = classDeclaration;
		for (int i = classInfo.ContainingTypes.Length - 1; i >= 0; i--)
		{
			topMember = SyntaxFactory.ClassDeclaration(EscapeIdentifier(classInfo.ContainingTypes[i]))
				.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
				.AddMembers(topMember);
		}

		// Create compilation unit with nullable enable directive
		var compilationUnit = SyntaxFactory.CompilationUnit()
			.AddUsings(usingDirectives);

		// Add nullable directive
		var nullableDirective = SyntaxFactory.Trivia(
			SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true));

		// Get the first using directive and add the nullable directive before it
		var firstUsing = compilationUnit.Usings[0];
		var updatedFirstUsing = firstUsing.WithLeadingTrivia(nullableDirective, SyntaxFactory.CarriageReturnLineFeed);
		compilationUnit = compilationUnit.WithUsings(
			compilationUnit.Usings.Replace(firstUsing, updatedFirstUsing));

		if (classInfo.Namespace is not null)
		{
			var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(EscapeNamespace(classInfo.Namespace)))
				.AddMembers(topMember);
			compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);
		}
		else
		{
			compilationUnit = compilationUnit.AddMembers(topMember);
		}

		return compilationUnit;
	}

	private static PropertyDeclarationSyntax BuildQueryPropertyTargetTypeProperty(ClassInfo classInfo)
	{
		return SyntaxFactory.PropertyDeclaration(
				SyntaxFactory.ParseTypeName("global::System.Type"),
				SyntaxFactory.Identifier("QueryPropertyTargetType"))
			.WithExplicitInterfaceSpecifier(
				SyntaxFactory.ExplicitInterfaceSpecifier(
					SyntaxFactory.ParseName("global::Microsoft.Maui.Controls.IQueryPropertyAttributable")))
			.WithExpressionBody(
				SyntaxFactory.ArrowExpressionClause(
					SyntaxFactory.TypeOfExpression(
						SyntaxFactory.ParseTypeName(classInfo.FullyQualifiedTypeName))))
			.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
	}

	private static MethodDeclarationSyntax BuildApplyQueryPropertyAttributesMethod(ClassInfo classInfo)
	{
		var statements = new List<StatementSyntax>();

		// if (query == null) return;
		statements.Add(SyntaxFactory.IfStatement(
			SyntaxFactory.BinaryExpression(
				SyntaxKind.EqualsExpression,
				SyntaxFactory.IdentifierName("query"),
				SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
			SyntaxFactory.ReturnStatement()));

		// Generate property setting code for each mapping
		var valueVariableNames = new HashSet<string>();
		foreach (var mapping in classInfo.PropertyMappings)
		{
			var valueVarName = $"{EscapeIdentifier(mapping.PropertyName)}__value";
			var suffix = 2;
			while (!valueVariableNames.Add(valueVarName))
				valueVarName = $"{EscapeIdentifier(mapping.PropertyName)}__value{suffix++}";

			statements.AddRange(BuildPropertyMappingStatements(mapping, valueVarName));
		}

		var method = SyntaxFactory.MethodDeclaration(
				SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
				SyntaxFactory.Identifier("ApplyQueryPropertyAttributes"))
			.WithExplicitInterfaceSpecifier(
				SyntaxFactory.ExplicitInterfaceSpecifier(
					SyntaxFactory.ParseName("global::Microsoft.Maui.Controls.IQueryPropertyAttributable")))
			.AddParameterListParameters(
				SyntaxFactory.Parameter(SyntaxFactory.Identifier("query"))
					.WithType(SyntaxFactory.ParseTypeName("global::Microsoft.Maui.Controls.QueryPropertyAttributeContext")))
			.WithBody(SyntaxFactory.Block(statements));

		// Add XML doc comment
		var docComment = SyntaxFactory.ParseLeadingTrivia(
			"/// <summary>\r\n" +
			"/// Applies generated query-property assignments from navigation parameters.\r\n" +
			"/// This method is generated by the QueryPropertyGenerator.\r\n" +
			"/// </summary>\r\n");
		method = method.WithLeadingTrivia(docComment);

		return method;
	}

	private static IEnumerable<StatementSyntax> BuildPropertyMappingStatements(PropertyMapping mapping, string valueVarName)
	{
		var statements = new List<StatementSyntax>();

		// if (query.TryGetValue("queryId", typeof(PropertyType), out var value))
		var tryGetValueCondition = SyntaxFactory.InvocationExpression(
			SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.IdentifierName("query"),
				SyntaxFactory.IdentifierName("TryGetValue")))
			.AddArgumentListArguments(
				SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
					SyntaxKind.StringLiteralExpression,
					SyntaxFactory.Literal(mapping.QueryId))),
				SyntaxFactory.Argument(
					SyntaxFactory.TypeOfExpression(
						SyntaxFactory.ParseTypeName(mapping.ContextType))),
				SyntaxFactory.Argument(
					SyntaxFactory.DeclarationExpression(
						SyntaxFactory.IdentifierName("var"),
						SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(valueVarName))))
					.WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword)));

		statements.Add(SyntaxFactory.IfStatement(
			tryGetValueCondition,
			SyntaxFactory.Block(
				SyntaxFactory.ExpressionStatement(
					SyntaxFactory.AssignmentExpression(
						SyntaxKind.SimpleAssignmentExpression,
						SyntaxFactory.IdentifierName(EscapeIdentifier(mapping.PropertyName)),
						SyntaxFactory.ParseExpression($"({mapping.PropertyType}){valueVarName}")))),
			SyntaxFactory.ElseClause(
				SyntaxFactory.IfStatement(
					SyntaxFactory.InvocationExpression(
						SyntaxFactory.MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							SyntaxFactory.IdentifierName("query"),
							SyntaxFactory.IdentifierName("WasRemoved")))
						.AddArgumentListArguments(
							SyntaxFactory.Argument(
								SyntaxFactory.LiteralExpression(
									SyntaxKind.StringLiteralExpression,
									SyntaxFactory.Literal(mapping.QueryId)))),
					SyntaxFactory.Block(
						SyntaxFactory.ExpressionStatement(
							SyntaxFactory.AssignmentExpression(
								SyntaxKind.SimpleAssignmentExpression,
								SyntaxFactory.IdentifierName(EscapeIdentifier(mapping.PropertyName)),
								SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))))))));

		return statements;
	}

	private static IPropertySymbol? FindPropertyInHierarchy(INamedTypeSymbol type, string propertyName)
	{
		var current = type;
		while (current is not null)
		{
			var property = current.GetMembers(propertyName)
				.OfType<IPropertySymbol>()
				.FirstOrDefault();

			if (property is not null)
				return property;

			current = current.BaseType;
		}
		return null;
	}

	private static string EscapeNamespace(string namespaceName) =>
		string.Join(".", namespaceName.Split('.').Select(EscapeIdentifier));

	private record struct PropertyMapping(string PropertyName, string QueryId, string PropertyType, string ContextType);

	private record struct ClassInfo(
		string ClassName,
		string FullyQualifiedTypeName,
		string? Namespace,
		ImmutableArray<string> ContainingTypes,
		ImmutableArray<PropertyMapping> PropertyMappings,
		ImmutableArray<Diagnostic> Diagnostics);
}
