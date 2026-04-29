using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;

/// <summary>
/// Incremental source generator that generates <see cref="IQueryAttributable"/> implementations
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
/// that explicitly implements <c>IQueryAttributable.ApplyQueryAttributes</c> and is marked with
/// <c>[GeneratedCode("QueryPropertyGenerator", ...)]</c>. At runtime, <c>ShellContent</c> detects
/// this attribute and skips the reflection-based property setting, avoiding double application.
/// </para>
/// <para>
/// For non-partial classes, the generator emits a MAUI1200 warning and no code is generated.
/// The existing reflection fallback continues to work for these classes.
/// </para>
/// <para>
/// The generated code handles: URL decoding for string properties, <c>Convert.ChangeType</c>
/// for non-string properties (using the underlying type for <c>Nullable&lt;T&gt;</c>),
/// and clearing properties that were set in a previous navigation but are absent in the current one.
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
				predicate: static (node, _) => node is ClassDeclarationSyntax,
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
					&& diagnostic.Severity < DiagnosticSeverity.Error)
				{
					// Re-create as error when reflection is disabled
					spc.ReportDiagnostic(Diagnostic.Create(
						Descriptors.QueryPropertyClassMustBePartialNoFallback,
						diagnostic.Location,
						diagnostic.AdditionalLocations,
						diagnostic.Properties,
						classInfo.ClassName));
				}
				else
				{
					spc.ReportDiagnostic(diagnostic);
				}
			}

			// Only generate if there are valid properties
			if (classInfo.PropertyMappings.Length > 0)
			{
				var source = GenerateSource(classInfo);
				var typePath = classInfo.ContainingTypes.Length > 0
					? string.Join(".", classInfo.ContainingTypes) + "." + classInfo.ClassName
					: classInfo.ClassName;
				var hintName = classInfo.Namespace is not null
					? $"{classInfo.Namespace}.{typePath}_QueryProperty.g.cs"
					: $"{typePath}_QueryProperty.g.cs";
				spc.AddSource(hintName, source);
			}
		});
	}

	private static ClassInfo GetClassInfo(GeneratorAttributeSyntaxContext context, System.Threading.CancellationToken cancellationToken)
	{
		var classDecl = (ClassDeclarationSyntax)context.TargetNode;
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
			return new ClassInfo(classSymbol.Name, null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
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
				return new ClassInfo(classSymbol.Name, null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
			}

			containingType = containingType.ContainingType;
		}

		// Skip generation if the class already explicitly implements IQueryAttributable
		var iQueryAttributable = context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IQueryAttributable");
		if (iQueryAttributable is not null)
		{
			foreach (var iface in classSymbol.Interfaces)
			{
				if (SymbolEqualityComparer.Default.Equals(iface, iQueryAttributable))
					return new ClassInfo(classSymbol.Name, null, ImmutableArray<string>.Empty, ImmutableArray<PropertyMapping>.Empty, diagnostics.ToImmutable());
			}
		}

		// Get all QueryPropertyAttribute instances on this class
		var queryPropertyAttributes = context.Attributes;

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

			// Determine the conversion type for non-string properties
			// Convert.ChangeType doesn't support Nullable<T>, so use the underlying type
			var propertyType = property.Type;
			var propertyTypeName = propertyType.ToFQDisplayString();
			string conversionTypeName = propertyTypeName;

			if (propertyType is INamedTypeSymbol { IsGenericType: true } namedType &&
				namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
			{
				conversionTypeName = namedType.TypeArguments[0].ToFQDisplayString();
			}

			propertyMappings.Add(new PropertyMapping(propertyName!, queryId!, propertyTypeName, conversionTypeName));
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
			namespaceName,
			containingTypes.ToImmutable(),
			propertyMappingsArray,
			diagnostics.ToImmutable());
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
		var classMembers = new List<MemberDeclarationSyntax>();

		// Add ApplyQueryAttributes method
		classMembers.Add(BuildApplyQueryAttributesMethod(classInfo));

		// Build class declaration implementing IQueryAttributable
		var classDeclaration = SyntaxFactory.ClassDeclaration(classInfo.ClassName)
			.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
			.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
				SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("global::Microsoft.Maui.Controls.IQueryAttributable")))))
			.WithMembers(SyntaxFactory.List(classMembers));

		// Wrap in containing type chain for nested classes (innermost → outermost)
		MemberDeclarationSyntax topMember = classDeclaration;
		for (int i = classInfo.ContainingTypes.Length - 1; i >= 0; i--)
		{
			topMember = SyntaxFactory.ClassDeclaration(classInfo.ContainingTypes[i])
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
			var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(classInfo.Namespace))
				.AddMembers(topMember);
			compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);
		}
		else
		{
			compilationUnit = compilationUnit.AddMembers(topMember);
		}

		return compilationUnit;
	}

	private static MethodDeclarationSyntax BuildApplyQueryAttributesMethod(ClassInfo classInfo)
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
		foreach (var mapping in classInfo.PropertyMappings)
		{
			statements.AddRange(BuildPropertyMappingStatements(mapping));
		}

		var method = SyntaxFactory.MethodDeclaration(
				SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
				SyntaxFactory.Identifier("ApplyQueryAttributes"))
			.WithExplicitInterfaceSpecifier(
				SyntaxFactory.ExplicitInterfaceSpecifier(
					SyntaxFactory.ParseName("global::Microsoft.Maui.Controls.IQueryAttributable")))
			.AddParameterListParameters(
				SyntaxFactory.Parameter(SyntaxFactory.Identifier("query"))
					.WithType(SyntaxFactory.ParseTypeName("global::System.Collections.Generic.IDictionary<string, object>")))
			.WithBody(SyntaxFactory.Block(statements));

		// Add XML doc comment
		var docComment = SyntaxFactory.ParseLeadingTrivia(
			"/// <summary>\r\n" +
			"/// Applies query attributes from navigation parameters.\r\n" +
			"/// This method is generated by the QueryPropertyGenerator.\r\n" +
			"/// </summary>\r\n");
		method = method.WithLeadingTrivia(docComment);

		return method;
	}

	private static IEnumerable<StatementSyntax> BuildPropertyMappingStatements(PropertyMapping mapping)
	{
		var statements = new List<StatementSyntax>();
		// Use the property name as the variable prefix — it's already a valid C# identifier
		// and unique per class, avoiding collisions from QueryIds that sanitize to the same name
		var valueVarName = $"{EscapeIdentifier(mapping.PropertyName)}__value";

		// if (query.TryGetValue("queryId", out var queryIdValue))
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
					SyntaxFactory.DeclarationExpression(
						SyntaxFactory.IdentifierName("var"),
						SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(valueVarName))))
					.WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword)));

		var ifTrueStatements = new List<StatementSyntax>();

		if (mapping.PropertyType == "string")
		{
			// For string properties: if (value != null) Property = UrlDecode(value); else Property = null;
			ifTrueStatements.Add(BuildStringPropertyAssignment(mapping, valueVarName));
		}
		else
		{
			// For non-string properties: if (value != null) { var converted = Convert.ChangeType(...); Property = (Type)converted; }
			ifTrueStatements.Add(BuildNonStringPropertyAssignment(mapping, valueVarName));
		}

		statements.Add(SyntaxFactory.IfStatement(
			tryGetValueCondition,
			SyntaxFactory.Block(ifTrueStatements)));

		return statements;
	}

	private static StatementSyntax BuildStringPropertyAssignment(PropertyMapping mapping, string valueVarName)
	{
		// if (valueVarName != null) Property = WebUtility.UrlDecode(valueVarName.ToString()); else Property = null;
		return SyntaxFactory.IfStatement(
			SyntaxFactory.BinaryExpression(
				SyntaxKind.NotEqualsExpression,
				SyntaxFactory.IdentifierName(valueVarName),
				SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
			SyntaxFactory.ExpressionStatement(
				SyntaxFactory.AssignmentExpression(
					SyntaxKind.SimpleAssignmentExpression,
					SyntaxFactory.IdentifierName(mapping.PropertyName),
					SyntaxFactory.InvocationExpression(
						SyntaxFactory.MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							SyntaxFactory.ParseName("global::System.Net.WebUtility"),
							SyntaxFactory.IdentifierName("UrlDecode")))
						.AddArgumentListArguments(
							SyntaxFactory.Argument(
								SyntaxFactory.InvocationExpression(
									SyntaxFactory.MemberAccessExpression(
										SyntaxKind.SimpleMemberAccessExpression,
										SyntaxFactory.IdentifierName(valueVarName),
										SyntaxFactory.IdentifierName("ToString"))))))),
			SyntaxFactory.ElseClause(
				SyntaxFactory.ExpressionStatement(
					SyntaxFactory.AssignmentExpression(
						SyntaxKind.SimpleAssignmentExpression,
						SyntaxFactory.IdentifierName(mapping.PropertyName),
						SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))));
	}

	private static StatementSyntax BuildNonStringPropertyAssignment(PropertyMapping mapping, string valueVarName)
	{
		// Use the conversion type (underlying type for Nullable<T>) since Convert.ChangeType doesn't support Nullable<T>
		// Cast to the conversion type; for nullable value types, implicit conversion from T to T? handles the rest
		return SyntaxFactory.IfStatement(
			SyntaxFactory.BinaryExpression(
				SyntaxKind.NotEqualsExpression,
				SyntaxFactory.IdentifierName(valueVarName),
				SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
			SyntaxFactory.Block(
				SyntaxFactory.LocalDeclarationStatement(
					SyntaxFactory.VariableDeclaration(
						SyntaxFactory.IdentifierName("var"))
					.AddVariables(
						SyntaxFactory.VariableDeclarator("convertedValue")
							.WithInitializer(SyntaxFactory.EqualsValueClause(
								SyntaxFactory.InvocationExpression(
									SyntaxFactory.MemberAccessExpression(
										SyntaxKind.SimpleMemberAccessExpression,
										SyntaxFactory.ParseName("global::System.Convert"),
										SyntaxFactory.IdentifierName("ChangeType")))
									.AddArgumentListArguments(
										SyntaxFactory.Argument(SyntaxFactory.IdentifierName(valueVarName)),
										SyntaxFactory.Argument(
											SyntaxFactory.TypeOfExpression(
												SyntaxFactory.ParseTypeName(mapping.ConversionType)))))))),
				SyntaxFactory.ExpressionStatement(
					SyntaxFactory.AssignmentExpression(
						SyntaxKind.SimpleAssignmentExpression,
						SyntaxFactory.IdentifierName(mapping.PropertyName),
						SyntaxFactory.CastExpression(
							SyntaxFactory.ParseTypeName(mapping.ConversionType),
							SyntaxFactory.IdentifierName("convertedValue"))))));
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

	private record struct PropertyMapping(string PropertyName, string QueryId, string PropertyType, string ConversionType);

	private record struct ClassInfo(
		string ClassName,
		string? Namespace,
		ImmutableArray<string> ContainingTypes,
		ImmutableArray<PropertyMapping> PropertyMappings,
		ImmutableArray<Diagnostic> Diagnostics);
}
