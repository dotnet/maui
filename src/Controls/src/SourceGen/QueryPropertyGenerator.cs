using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Replaces the reflection-based processing of <c>[QueryProperty]</c> attributes with generated code.
/// </summary>
/// <remarks>
/// <para>
/// For every class whose <c>[QueryProperty]</c> attributes (declared or inherited) can be applied without reflection,
/// the generator emits strongly-typed setters and registers them from a module initializer with the internal
/// <c>Microsoft.Maui.Controls.QueryPropertyRegistry</c>, which it reaches through <c>UnsafeAccessor</c>.
/// No user code changes and no public API are required.
/// </para>
/// <para>
/// The generated data mirrors what <c>ShellContent</c> computes with reflection: the attributes in
/// <c>GetCustomAttributes(typeof(QueryPropertyAttribute), true)</c> order, the property found by
/// <c>Type.GetRuntimeProperty</c>, and only properties with a public setter. <c>ShellContent</c> keeps
/// converting the values, so both paths share the same conversion logic.
/// </para>
/// <para>
/// Classes that can't be handled (for example generic or private nested classes) are not registered and keep
/// using reflection. When the trim analyzer is enabled, MAUI1200 reports them.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class QueryPropertyGenerator : IIncrementalGenerator
{
	const string QueryPropertyAttributeMetadataName = "Microsoft.Maui.Controls.QueryPropertyAttribute";
	const string RegistryTypeName = "Microsoft.Maui.Controls.QueryPropertyRegistry";
	internal const string HintName = "QueryPropertyRegistrations.g.cs";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var canGenerate = context.CompilationProvider.Select(static (compilation, _) => GetRegistryAssemblyName(compilation));

		var types = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (node, _) => node is TypeDeclarationSyntax { AttributeLists.Count: > 0 } or TypeDeclarationSyntax { BaseList: not null },
				transform: static (ctx, ct) => GetTypeModel(ctx, ct))
			.Where(static model => model is not null)
			.Select(static (model, _) => model!)
			.Collect();

		var reportDiagnostics = context.AnalyzerConfigOptionsProvider
			.Select(static (options, _) => options.GlobalOptions.IsTrue("build_property.EnableTrimAnalyzer"));

		context.RegisterSourceOutput(types.Combine(canGenerate).Combine(reportDiagnostics), static (spc, input) =>
		{
			var ((models, registryAssemblyName), reportDiagnostics) = input;
			if (registryAssemblyName is null || models.IsEmpty)
				return;

			// Partial classes are seen once per declaration.
			var distinctModels = models
				.GroupBy(static m => m.TypeName)
				.Select(static g => g.First())
				.OrderBy(static m => m.TypeName, System.StringComparer.Ordinal)
				.ToList();

			if (reportDiagnostics)
			{
				foreach (var model in distinctModels)
				{
					if (model.UnsupportedReason is not null)
						spc.ReportDiagnostic(Diagnostic.Create(Descriptors.QueryPropertyRequiresReflection, model.Location?.ToLocation(), model.DisplayName, model.UnsupportedReason));
				}
			}

			var registrations = distinctModels.Where(static m => m.Registration is not null).Select(static m => m.Registration!).ToList();
			if (registrations.Count > 0)
				spc.AddSource(HintName, SourceText.From(GenerateSource(registryAssemblyName, registrations), Encoding.UTF8));
		});
	}

	static string? GetRegistryAssemblyName(Compilation compilation)
	{
		if (compilation is not CSharpCompilation { LanguageVersion: >= LanguageVersion.CSharp9 })
			return null;

		if (compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute") is null
			|| compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.UnsafeAccessorTypeAttribute") is null)
			return null;

		return compilation.GetTypeByMetadataName(QueryPropertyAttributeMetadataName)?.ContainingAssembly.Name;
	}

	static TypeModel? GetTypeModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var declaration = (TypeDeclarationSyntax)context.Node;
		if (context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol { TypeKind: TypeKind.Class, IsStatic: false } type)
			return null;

		var compilation = context.SemanticModel.Compilation;
		var attributeType = compilation.GetTypeByMetadataName(QueryPropertyAttributeMetadataName);
		if (attributeType is null)
			return null;

		// Same order as Type.GetCustomAttributes(typeof(QueryPropertyAttribute), inherit: true):
		// the attributes of the type itself, then those of each base type.
		var attributes = new List<AttributeData>();
		for (var current = type; current is not null; current = current.BaseType)
		{
			foreach (var attribute in current.GetAttributes())
			{
				if (attribute.AttributeClass is { } attributeClass && InheritsFromOrEquals(attributeClass, attributeType))
					attributes.Add(attribute);
			}
		}

		if (attributes.Count == 0)
			return null;

		var typeName = type.ToFQDisplayString();
		var displayName = type.ToString();
		var location = LocationModel.From(declaration.Identifier.GetLocation());

		var registration = TryCreateRegistration(compilation, type, typeName, attributes, attributeType, out var unsupportedReason);
		return new TypeModel(typeName, displayName, registration, unsupportedReason, location);
	}

	static string? TryCreateRegistration(Compilation compilation, INamedTypeSymbol type, string typeName, List<AttributeData> attributes, INamedTypeSymbol attributeType, out string? unsupportedReason)
	{
		unsupportedReason = null;

		for (var current = type; current is not null; current = current.ContainingType)
		{
			if (current.IsGenericType)
			{
				unsupportedReason = "generic types are not supported";
				return null;
			}
		}

		if (type.IsFileLocal || !compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
		{
			unsupportedReason = "the type is not accessible from other types in the assembly";
			return null;
		}

		if (IsObsoleteError(type))
		{
			unsupportedReason = "the type is obsolete";
			return null;
		}

		var queryIds = new List<string>();
		var propertyTypes = new List<string>();
		var setters = new List<string>();
		var resetters = new List<string>();

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

			if (!TryFindProperty(type, name, out var property))
			{
				unsupportedReason = $"property '{name}' can't be resolved without reflection";
				return null;
			}

			// Reflection only sets properties with a public setter and ignores the attribute otherwise.
			if (property is null || property.SetMethod is not { DeclaredAccessibility: Accessibility.Public } setMethod)
				continue;

			if (setMethod.IsInitOnly
				|| property.Type.IsRefLikeType
				|| property.Type is IPointerTypeSymbol or IFunctionPointerTypeSymbol
				|| property.Type.TypeKind == TypeKind.Error
				|| !compilation.IsSymbolAccessibleWithin(property.Type, compilation.Assembly)
				|| !compilation.IsSymbolAccessibleWithin(property.ContainingType, compilation.Assembly)
				|| IsObsoleteError(property) || IsObsoleteError(setMethod))
			{
				unsupportedReason = $"property '{name}' can't be set from generated code";
				return null;
			}

			var propertyType = property.Type.ToFQDisplayString();
			var target = property.IsStatic
				? property.ContainingType.ToFQDisplayString()
				: $"(({property.ContainingType.ToFQDisplayString()})target)";
			var member = $"{target}.{EscapeIdentifier(property.Name)}";
			var isNonNullableValueType = property.Type.IsValueType && property.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;

			queryIds.Add(SymbolDisplay.FormatLiteral(queryId, quote: true));
			propertyTypes.Add($"typeof({propertyType})");
			// PropertyInfo.SetValue(target, null) stores the default value of a value type.
			setters.Add(isNonNullableValueType
				? $"static (target, value) => {member} = value is null ? default({propertyType}) : ({propertyType})value"
				: $"static (target, value) => {member} = ({propertyType})value");
			// Matches Activator.CreateInstance(propertyType) for value types and null otherwise.
			resetters.Add(isNonNullableValueType
				? $"static target => {member} = new {propertyType}()"
				: $"static target => {member} = default({propertyType})");
		}

		var builder = new StringBuilder();
		builder.AppendLine($"\t\t\tRegister(null, typeof({typeName}),");
		AppendArray(builder, "string", queryIds);
		builder.AppendLine(",");
		AppendArray(builder, "global::System.Type", propertyTypes);
		builder.AppendLine(",");
		AppendArray(builder, "global::System.Action<object, object>", setters);
		builder.AppendLine(",");
		AppendArray(builder, "global::System.Action<object>", resetters);
		builder.AppendLine(");");
		return builder.ToString();

		static void AppendArray(StringBuilder builder, string elementType, List<string> items)
		{
			if (items.Count == 0)
			{
				builder.Append($"\t\t\t\tglobal::System.Array.Empty<{elementType}>()");
				return;
			}

			builder.AppendLine($"\t\t\t\tnew {elementType}[]");
			builder.AppendLine("\t\t\t\t{");
			foreach (var item in items)
				builder.AppendLine($"\t\t\t\t\t{item},");
			builder.Append("\t\t\t\t}");
		}
	}

	/// <summary>
	/// Resolves a property name the same way as <c>Type.GetRuntimeProperty(name)</c>: public instance properties of
	/// the type and its base types, public static properties of the type itself. Returns <c>false</c> when reflection
	/// would throw or bind differently than C# member access.
	/// </summary>
	static bool TryFindProperty(INamedTypeSymbol type, string name, out IPropertySymbol? property)
	{
		property = null;

		for (var current = type; current is not null; current = current.BaseType)
		{
			foreach (var member in current.GetMembers(name))
			{
				if (member is not IPropertySymbol candidate || candidate.DeclaredAccessibility != Accessibility.Public)
					continue;

				if (candidate.IsIndexer || candidate.Parameters.Length > 0)
					return false;

				if (candidate.IsStatic && !SymbolEqualityComparer.Default.Equals(current, type))
					continue;

				if (property is null)
				{
					property = candidate;
				}
				else if (!SymbolEqualityComparer.Default.Equals(property.Type, candidate.Type))
				{
					// A property hidden by a property of another type makes GetRuntimeProperty ambiguous.
					property = null;
					return false;
				}
			}
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

	static bool InheritsFromOrEquals(INamedTypeSymbol type, INamedTypeSymbol baseType)
	{
		for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
		{
			if (SymbolEqualityComparer.Default.Equals(current, baseType))
				return true;
		}

		return false;
	}

	static bool IsObsoleteError(ISymbol symbol) =>
		symbol.GetAttributes().Any(static a =>
			a.AttributeClass?.ToString() == "System.ObsoleteAttribute"
			&& a.ConstructorArguments.Length == 2
			&& a.ConstructorArguments[1].Value is true);

	static string EscapeIdentifier(string identifier) =>
		SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None ? "@" + identifier : identifier;

	static string GenerateSource(string registryAssemblyName, List<string> registrations)
	{
		var registryType = SymbolDisplay.FormatLiteral($"{RegistryTypeName}, {registryAssemblyName}", quote: true);
		var builder = new StringBuilder();
		builder.AppendLine("// <auto-generated/>");
		builder.AppendLine("#nullable disable");
		builder.AppendLine("#pragma warning disable");
		builder.AppendLine();
		builder.AppendLine("namespace Microsoft.Maui.Controls.Generated");
		builder.AppendLine("{");
		builder.AppendLine("\t[global::System.CodeDom.Compiler.GeneratedCode(\"Microsoft.Maui.Controls.SourceGen\", \"1.0.0.0\")]");
		builder.AppendLine("\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
		builder.AppendLine("\tinternal static class QueryPropertyRegistrations");
		builder.AppendLine("\t{");
		builder.AppendLine("\t\t[global::System.Runtime.CompilerServices.ModuleInitializer]");
		builder.AppendLine("\t\tinternal static void Initialize()");
		builder.AppendLine("\t\t{");
		foreach (var registration in registrations)
			builder.Append(registration);
		builder.AppendLine("\t\t}");
		builder.AppendLine();
		builder.AppendLine("\t\t[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = \"Register\")]");
		builder.AppendLine("\t\tstatic extern void Register(");
		builder.AppendLine($"\t\t\t[global::System.Runtime.CompilerServices.UnsafeAccessorType({registryType})] object registry,");
		builder.AppendLine("\t\t\tglobal::System.Type type,");
		builder.AppendLine("\t\t\tstring[] queryIds,");
		builder.AppendLine("\t\t\tglobal::System.Type[] propertyTypes,");
		builder.AppendLine("\t\t\tglobal::System.Action<object, object>[] setters,");
		builder.AppendLine("\t\t\tglobal::System.Action<object>[] resetters);");
		builder.AppendLine("\t}");
		builder.AppendLine("}");
		return builder.ToString();
	}

	sealed record TypeModel(string TypeName, string DisplayName, string? Registration, string? UnsupportedReason, LocationModel? Location);

	sealed record LocationModel(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
	{
		public static LocationModel? From(Location location) =>
			location.SourceTree is null ? null : new LocationModel(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);

		public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
	}
}
