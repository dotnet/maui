using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.BindingSourceGen;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Replaces the reflection-based processing of <c>[QueryProperty]</c> attributes with generated code.
/// </summary>
/// <remarks>
/// <para>
/// For every partial class with <c>[QueryProperty]</c> attributes (declared or inherited), the generator adds an explicit
/// implementation of <c>IQueryAttributable</c> that sets the properties directly. <c>ShellContent</c> calls it
/// instead of using reflection.
/// </para>
/// <para>
/// The generated code mirrors what <c>ShellContent</c> does with reflection: the attributes in
/// <c>GetCustomAttributes(typeof(QueryPropertyAttribute), true)</c> order, the property found by
/// <c>Type.GetRuntimeProperty</c>, only properties with a public setter, and the same value conversions and resets.
/// The conversions that depend on where a parameter came from use <c>QueryPropertyConversion</c>.
/// </para>
/// <para>
/// Classes that can't be handled (for example classes that aren't partial) fail at runtime if their
/// <c>[QueryProperty]</c> attributes are applied. When the trim analyzer is enabled, MAUI1200 reports them.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class QueryPropertyGenerator : IIncrementalGenerator
{
	const string QueryPropertyAttributeMetadataName = "Microsoft.Maui.Controls.QueryPropertyAttribute";
	const string InterfaceMetadataName = "Microsoft.Maui.Controls.IQueryAttributable";
	const string InterfaceName = "global::Microsoft.Maui.Controls.IQueryAttributable";
	const string ConversionName = "global::Microsoft.Maui.Controls.QueryPropertyConversion";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var types = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
				transform: static (ctx, ct) => GetTypeModel(ctx, ct))
			.Where(static model => model is not null)
			.Select(static (model, _) => model!)
			.Collect();

		var reportDiagnostics = context.AnalyzerConfigOptionsProvider
			.Select(static (options, _) => options.GlobalOptions.IsTrue("build_property.EnableTrimAnalyzer"));

		context.RegisterSourceOutput(types.Combine(reportDiagnostics), static (spc, input) =>
		{
			var (models, reportDiagnostics) = input;

			// Partial classes are seen once per declaration.
			foreach (var model in models.GroupBy(static m => m.TypeName).Select(static g => g.First()))
			{
				if (model.Source is not null)
					spc.AddSource(model.HintName, SourceText.From(model.Source, Encoding.UTF8));
				else if (reportDiagnostics && model.UnsupportedReason is not null)
					spc.ReportDiagnostic(Diagnostic.Create(Descriptors.QueryPropertyRequiresReflection, model.Location?.ToLocation(), model.DisplayName, model.UnsupportedReason));
			}
		});
	}

	static TypeModel? GetTypeModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var declaration = (TypeDeclarationSyntax)context.Node;
		if (context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol { TypeKind: TypeKind.Class, IsStatic: false } type)
			return null;

		var compilation = context.SemanticModel.Compilation;
		var attributeType = compilation.GetTypeByMetadataName(QueryPropertyAttributeMetadataName);
		var interfaceType = compilation.GetTypeByMetadataName(InterfaceMetadataName);
		if (attributeType is null || interfaceType is null)
			return null;

		// Same order as Type.GetCustomAttributes(typeof(QueryPropertyAttribute), inherit: true):
		// the attributes of the type itself, then those of each base type.
		var attributes = type.GetAllAttributes(context: null)
			.Where(attribute => attribute.AttributeClass is { } attributeClass && attributeClass.InheritsFrom(attributeType, context: null))
			.ToList();

		if (attributes.Count == 0)
			return null;

		IMethodSymbol? customQueryPropertiesHandler = null;
		if (type.Implements(interfaceType))
		{
			var applyQueryAttributes = type.FindImplementationForInterfaceMember(interfaceType.GetMembers("ApplyQueryAttributes").Single());
			// A derived class must replace a generated base implementation to include its own attributes.
			if (!IsGeneratedQueryPropertyImplementation(applyQueryAttributes))
				return null;

			var applyQueryProperties = (IMethodSymbol)interfaceType.GetMembers("ApplyQueryProperties").Single();
			var implementation = type.FindImplementationForInterfaceMember(applyQueryProperties);
			if (implementation is IMethodSymbol userImplementation
				&& SymbolEqualityComparer.Default.Equals(userImplementation.ContainingType, type)
				&& !IsGeneratedQueryPropertyImplementation(userImplementation))
				return null;

			customQueryPropertiesHandler = type.GetMembers("ApplyQueryProperties")
				.OfType<IMethodSymbol>()
				.FirstOrDefault(method =>
					method.MethodKind == MethodKind.Ordinary
					&& !method.IsStatic
					&& method.ReturnsVoid
					&& method.Parameters.Length == applyQueryProperties.Parameters.Length
					&& method.Parameters.Select(static parameter => parameter.Type)
						.SequenceEqual(applyQueryProperties.Parameters.Select(static parameter => parameter.Type), SymbolEqualityComparer.Default));
		}

		var typeName = type.ToFQDisplayString();
		var location = LocationModel.From(declaration.Identifier.GetLocation());
		var hintName = GetHintName(typeName);

		var source = TryGenerateSource(compilation, type, attributes, attributeType, customQueryPropertiesHandler, out var unsupportedReason);
		return new TypeModel(typeName, type.ToString(), hintName, source, unsupportedReason, location);
	}

	internal static bool IsGeneratedQueryPropertyImplementation(ISymbol? implementation) =>
		implementation is not null && implementation.GetAttributes().Any(static attribute =>
			attribute.AttributeClass?.ToString() == "System.CodeDom.Compiler.GeneratedCodeAttribute"
			&& attribute.ConstructorArguments.Length > 0
			&& attribute.ConstructorArguments[0].Value is "Microsoft.Maui.Controls.SourceGen");

	static string? TryGenerateSource(Compilation compilation, INamedTypeSymbol type, List<AttributeData> attributes, INamedTypeSymbol attributeType, IMethodSymbol? customQueryPropertiesHandler, out string? unsupportedReason)
	{
		unsupportedReason = null;

		for (var current = type; current is not null; current = current.ContainingType)
		{
			if (current.IsFileLocal)
			{
				unsupportedReason = "file-local types are not supported";
				return null;
			}

			if (!IsPartial(current))
			{
				unsupportedReason = SymbolEqualityComparer.Default.Equals(current, type)
					? "the type is not partial"
					: $"the containing type '{current.ToString()}' is not partial";
				return null;
			}
		}

		if (IsObsoleteError(type))
		{
			unsupportedReason = "the type is obsolete";
			return null;
		}

		var body = new StringBuilder();
		foreach (var attribute in attributes)
		{
			if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
			{
				unsupportedReason = $"'{attribute.AttributeClass!.ToString()}' derives from QueryPropertyAttribute";
				return null;
			}

			if (attribute.ConstructorArguments.Length != 2
				|| attribute.ConstructorArguments[0].Value is not string name
				|| attribute.ConstructorArguments[1].Value is not string queryId)
			{
				unsupportedReason = "the attribute arguments are not constant strings";
				return null;
			}

			if (!TryFindProperty(compilation, type, name, out var property, out var propertyType, out var isGeneratedProperty))
			{
				unsupportedReason = $"property '{name}' can't be resolved without reflection";
				return null;
			}

			// Reflection only sets properties with a public setter and ignores the attribute otherwise.
			if (propertyType is null || (!isGeneratedProperty && property?.SetMethod is not { DeclaredAccessibility: Accessibility.Public }))
				continue;

			if ((!isGeneratedProperty && property?.SetMethod is { IsInitOnly: true })
				|| propertyType.IsRefLikeType
				|| propertyType is IPointerTypeSymbol or IFunctionPointerTypeSymbol
				|| propertyType.TypeKind == TypeKind.Error
				|| ContainsTypeParameter(propertyType)
				|| !compilation.IsSymbolAccessibleWithin(propertyType, type)
				|| (property is not null && !compilation.IsSymbolAccessibleWithin(property.ContainingType, type))
				|| (property is not null && (IsObsoleteError(property) || property.SetMethod is { } setter && IsObsoleteError(setter))))
			{
				unsupportedReason = $"property '{name}' can't be set from generated code";
				return null;
			}

			AppendPropertyAssignment(body, type, property, propertyType, name, queryId);
		}

		return GenerateSource(type, body.ToString(), customQueryPropertiesHandler);
	}

	static void AppendPropertyAssignment(StringBuilder builder, INamedTypeSymbol type, IPropertySymbol? property, ITypeSymbol propertyTypeSymbol, string propertyName, string queryId)
	{
		var propertyType = propertyTypeSymbol.ToFQDisplayString();
		var memberName = EscapeIdentifier(propertyName);
		var member = property is null
			? $"this.{memberName}"
			: property.IsStatic
				? $"{property.ContainingType.ToFQDisplayString()}.{memberName}"
				: SymbolEqualityComparer.Default.Equals(property.ContainingType, type)
					? $"this.{memberName}"
					// Cast to the declaring type so a member hidden by a more derived type isn't used instead.
					: $"(({property.ContainingType.ToFQDisplayString()})this).{memberName}";
		var id = SymbolDisplay.FormatLiteral(queryId, quote: true);
		var isNullableValueType = propertyTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
		var isNonNullableValueType = propertyTypeSymbol.IsValueType && !isNullableValueType;

		string value;
		if (propertyTypeSymbol.SpecialType == SpecialType.System_String)
		{
			value = $"{ConversionName}.ConvertToString(query, {id}, value)";
		}
		else
		{
			var conversionType = isNullableValueType
				? ((INamedTypeSymbol)propertyTypeSymbol).TypeArguments[0].ToFQDisplayString()
				: propertyTypeSymbol.TypeKind == TypeKind.Dynamic
					? "global::System.Object"
					: propertyType;
			// PropertyInfo.SetValue(target, null) stores the default value of a value type.
			value = $"value is null ? default({propertyType}) : ({propertyType})global::System.Convert.ChangeType(value, typeof({conversionType}), {ConversionName}.GetFormatProvider(query, {id}))";
		}

		builder.AppendLine($"			if (query.TryGetValue({id}, out value))");
		builder.AppendLine($"				{member} = {value};");
		builder.AppendLine($"			else if (previousQuery.ContainsKey({id}))");
		// Matches Activator.CreateInstance(propertyType) for value types and null otherwise.
		builder.AppendLine(isNonNullableValueType && !propertyTypeSymbol.IsTupleType
			? $"				{member} = new {propertyType}();"
			: $"				{member} = default({propertyType});");
	}

	static string GenerateSource(INamedTypeSymbol type, string body, IMethodSymbol? customQueryPropertiesHandler)
	{
		var containingTypes = new List<INamedTypeSymbol>();
		for (var current = type; current is not null; current = current.ContainingType)
			containingTypes.Insert(0, current);

		var builder = new StringBuilder();
		builder.AppendLine("// <auto-generated/>");
		builder.AppendLine("#nullable disable");
		builder.AppendLine("#pragma warning disable");
		builder.AppendLine();

		var indent = "";
		if (!type.ContainingNamespace.IsGlobalNamespace)
		{
			builder.AppendLine($"namespace {type.ContainingNamespace.ToString()}");
			builder.AppendLine("{");
			indent = "	";
		}

		foreach (var current in containingTypes)
		{
			var baseList = SymbolEqualityComparer.Default.Equals(current, type) ? $" : {InterfaceName}" : "";
			builder.AppendLine($"{indent}partial {GetTypeKeyword(current)} {GetTypeName(current)}{baseList}");
			builder.AppendLine($"{indent}{{");
			indent += "	";
		}

		builder.AppendLine($"{indent}[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");
		builder.AppendLine($"{indent}void {InterfaceName}.ApplyQueryAttributes(global::System.Collections.Generic.IDictionary<string, object> query) {{ }}");
		builder.AppendLine();
		builder.AppendLine("#if !NETSTANDARD");
		builder.AppendLine($"{indent}[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");
		builder.AppendLine($"{indent}void {InterfaceName}.ApplyQueryProperties(global::System.Collections.Generic.IDictionary<string, object> query, global::System.Collections.Generic.IDictionary<string, object> previousQuery)");
		builder.AppendLine($"{indent}{{");
		if (customQueryPropertiesHandler is not null)
		{
			builder.AppendLine($"{indent}	this.ApplyQueryProperties(query, previousQuery);");
		}
		else
		{
			builder.AppendLine($"{indent}	{ConversionName}.ValidateGeneratedQueryProperties(this, typeof({GetTypeName(type)}));");
			builder.AppendLine($"{indent}	object value;");
			foreach (var line in body.Split('\n'))
			{
				if (line.Length > 0)
					builder.AppendLine(indent + line.TrimEnd('\r'));
			}
		}
		builder.AppendLine($"{indent}}}");
		builder.AppendLine("#endif");

		for (var i = 0; i < containingTypes.Count; i++)
		{
			indent = indent.Substring(1);
			builder.AppendLine($"{indent}}}");
		}

		if (!type.ContainingNamespace.IsGlobalNamespace)
			builder.AppendLine("}");

		return builder.ToString();
	}

	static bool IsPartial(INamedTypeSymbol type) =>
		type.DeclaringSyntaxReferences.Length > 0
		&& type.DeclaringSyntaxReferences.All(static r => r.GetSyntax() is TypeDeclarationSyntax declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

	static string GetTypeKeyword(INamedTypeSymbol type) => type switch
	{
		{ IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
		{ IsRecord: true } => "record",
		{ TypeKind: TypeKind.Struct } => "struct",
		{ TypeKind: TypeKind.Interface } => "interface",
		_ => "class",
	};

	static string GetTypeName(INamedTypeSymbol type) =>
		type.TypeParameters.Length == 0
			? EscapeIdentifier(type.Name)
			: $"{EscapeIdentifier(type.Name)}<{string.Join(", ", type.TypeParameters.Select(static p => EscapeIdentifier(p.Name)))}>";

	static string GetHintName(string typeName)
	{
		var builder = new StringBuilder();
		foreach (var c in typeName.StartsWith("global::", System.StringComparison.Ordinal) ? typeName.Substring("global::".Length) : typeName)
			builder.Append(((int)c).ToString("X4")).Append('_');
		return builder.Append("QueryProperties.g.cs").ToString();
	}

	static bool ContainsTypeParameter(ITypeSymbol type) => type switch
	{
		ITypeParameterSymbol => true,
		IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
		INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter) || (named.ContainingType is { } containing && ContainsTypeParameter(containing)),
		_ => false,
	};

	/// <summary>
	/// Resolves a property name the same way as <c>Type.GetRuntimeProperty(name)</c>: public instance properties of
	/// the type and its base types, public static properties of the type itself. Returns <c>false</c> when reflection
	/// would throw or bind differently than C# member access.
	/// </summary>
	static bool TryFindProperty(Compilation compilation, INamedTypeSymbol type, string name, out IPropertySymbol? property, out ITypeSymbol? propertyType, out bool isGeneratedProperty)
	{
		property = null;
		propertyType = null;
		isGeneratedProperty = false;
		var hiddenByNonPublicProperty = false;

		for (var current = type; current is not null; current = current.BaseType)
		{
			foreach (var member in current.GetMembers(name))
			{
				if (member is not IPropertySymbol candidate)
					continue;

				if (candidate.DeclaredAccessibility != Accessibility.Public)
				{
					hiddenByNonPublicProperty = true;
					continue;
				}

				if (candidate.IsIndexer || candidate.Parameters.Length > 0)
					return false;

				if (candidate.IsStatic && !SymbolEqualityComparer.Default.Equals(current, type))
					continue;

				if (property is null)
				{
					property = candidate;
					propertyType = candidate.Type;
				}
				else if (!SymbolEqualityComparer.Default.Equals(property.Type, candidate.Type))
				{
					// A property hidden by a property of another type makes GetRuntimeProperty ambiguous.
					property = null;
					return false;
				}
			}
		}

		if (property is not null && hiddenByNonPublicProperty)
			return false;

		if (property is null && type.TryGetObservablePropertyType(name, compilation, out var generatedPropertyType))
		{
			propertyType = generatedPropertyType;
			isGeneratedProperty = true;
			return true;
		}

		if (property?.SetMethod is null && property?.OverriddenProperty is not null)
		{
			// Reflection reports the setter declared in the overridden property; C# can't call it through the override.
			for (var overridden = property.OverriddenProperty; overridden is not null; overridden = overridden.OverriddenProperty)
			{
				if (overridden.SetMethod is not null)
				{
					property = null;
					return false;
				}
			}
		}

		return true;
	}

	static bool IsObsoleteError(ISymbol symbol) =>
		symbol.GetAttributes().Any(static a =>
			a.AttributeClass?.ToString() == "System.ObsoleteAttribute"
			&& a.ConstructorArguments.Length == 2
			&& a.ConstructorArguments[1].Value is true);

	sealed record TypeModel(string TypeName, string DisplayName, string HintName, string? Source, string? UnsupportedReason, LocationModel? Location);

	sealed record LocationModel(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
	{
		public static LocationModel? From(Location location) =>
			location.SourceTree is null ? null : new LocationModel(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);

		public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
	}
}
