using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Maui.HybridApp.Generators;

/// <summary>Emits direct action calls and their own AOT-safe JSON metadata, without depending on another generator's output.</summary>
[Generator]
public sealed class HybridActionGenerator : IIncrementalGenerator
{
	const string AttributeName = "Maui.HybridApp.HybridActionAttribute";
	static readonly DiagnosticDescriptor InvalidName = Rule("HYBRID001", "Invalid action name", "Action '{0}' has an invalid wire name");
	static readonly DiagnosticDescriptor InvalidMethod = Rule("HYBRID002", "Unsupported action method", "Action '{0}' must be accessible, nongeneric and non-async-void, without ref/out/in or delegate parameters: {1}");
	static readonly DiagnosticDescriptor Duplicate = Rule("HYBRID003", "Duplicate action name", "Action name '{0}' is duplicated within a provider or known host registrations");
	static readonly DiagnosticDescriptor InvalidJson = Rule("HYBRID004", "Unsupported JSON contract", "Action '{0}' cannot generate strict JSON for '{1}': {2}");
	static readonly DiagnosticDescriptor InvalidBinding = Rule("HYBRID005", "Invalid parameter binding", "Action parameter '{0}' has conflicting or unsupported service/context/cancellation binding");

	static DiagnosticDescriptor Rule(string id, string title, string message) =>
		new(id, title, message, "Maui.HybridApp", DiagnosticSeverity.Error, true);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var methods = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName,
			static (node, _) => node is MethodDeclarationSyntax,
			static (syntax, _) => (IMethodSymbol)syntax.TargetSymbol).Collect();
		var registrations = context.SyntaxProvider.CreateSyntaxProvider(
			static (node, _) => node is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax { Identifier.ValueText: "AddHybridActions" } } } ||
				node is InvocationExpressionSyntax { Expression: GenericNameSyntax { Identifier.ValueText: "AddHybridActions" } },
			static (syntax, _) => (syntax.SemanticModel.GetSymbolInfo(syntax.Node).Symbol as IMethodSymbol)?.TypeArguments.FirstOrDefault())
			.Where(static symbol => symbol is INamedTypeSymbol).Collect();
		context.RegisterSourceOutput(methods.Combine(registrations), static (output, inputs) => Generate(output, inputs.Left, inputs.Right));
	}

	static void Generate(SourceProductionContext output, ImmutableArray<IMethodSymbol> methods, ImmutableArray<ITypeSymbol?> registrations)
	{
		var registeredNames = new HashSet<string>(StringComparer.Ordinal);
		foreach (var provider in registrations.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
			foreach (var method in provider.GetMembers().OfType<IMethodSymbol>().Where(IsAction))
				if (!registeredNames.Add(Name(method)))
					output.ReportDiagnostic(Diagnostic.Create(Duplicate, method.Locations.FirstOrDefault(), Name(method)));

		var groups = methods.Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>()
			.GroupBy(method => method.ContainingType, SymbolEqualityComparer.Default);
		int index = 0;
		foreach (var group in groups)
		{
			var provider = (INamedTypeSymbol)group.Key!;
			var graph = new JsonGraph();
			var valid = new List<IMethodSymbol>();
			var names = new HashSet<string>(StringComparer.Ordinal);
			bool contractsValid = true;
			foreach (var method in group)
			{
				var name = Name(method);
				if (name.Length is < 1 or > 64 || name.Any(c => c > 127 || !(char.IsLetterOrDigit(c) || c is '.' or '_' or '-')))
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidName, method.Locations[0], method.Name));
					continue;
				}
				if (!names.Add(name))
				{
					output.ReportDiagnostic(Diagnostic.Create(Duplicate, method.Locations[0], name));
					continue;
				}
				var issue = MethodIssue(method);
				if (issue is not null)
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidMethod, method.Locations[0], method.Name, issue));
					continue;
				}
				bool good = true;
				foreach (var parameter in method.Parameters)
				{
					var attributes = parameter.GetAttributes();
					var services = attributes.Count(a => a.AttributeClass?.ToDisplayString() is "Maui.HybridApp.FromServicesAttribute" or "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
					var keyed = attributes.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
					if (services > 1 || (services != 0 && Special(parameter.Type)) || keyed is not null && keyed.ConstructorArguments.Length != 1)
					{
						output.ReportDiagnostic(Diagnostic.Create(InvalidBinding, parameter.Locations[0], parameter.Name));
						good = false;
					}
				}
				try
				{
					foreach (var parameter in method.Parameters.Where(Ordinary)) graph.Add(parameter.Type);
					var result = Result(method);
					if (result is not null) graph.Add(result);
				}
				catch (JsonContractException error)
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidJson, method.Locations[0], method.Name, error.Type.ToDisplayString(), error.Message));
					good = false;
					contractsValid = false;
				}
				if (good) valid.Add(method);
			}
			if (valid.Count == 0 || !contractsValid) continue;
			// One provider owns its metadata, including private implementation DTOs accessible within this assembly.
			var emitter = new Emitter(provider, valid, graph, index++);
			output.AddSource(emitter.Id + ".g.cs", emitter.Emit());
		}
	}

	static bool IsAction(IMethodSymbol method) => method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == AttributeName);
	static AttributeData Attribute(IMethodSymbol method) => method.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == AttributeName);
	static string Name(IMethodSymbol method) => Attribute(method).ConstructorArguments[0].Value as string ?? "";
	static bool Policy(IMethodSymbol method, string name) => Attribute(method).NamedArguments.Any(pair => pair.Key == name && pair.Value.Value is true);
	static bool Accessible(ISymbol symbol) => symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;
	static string? MethodIssue(IMethodSymbol method)
	{
		if (method.ContainingType.TypeKind != TypeKind.Class)
			return "use a provider class (its action methods may be static)";
		if (!Accessible(method)) return "method is not accessible from generated code";
		for (var type = method.ContainingType; type is not null; type = type.ContainingType)
			if (!Accessible(type) || type.Arity != 0 || type.IsFileLocal) return "containing type is inaccessible, file-local or generic";
		if (method.IsGenericMethod || method.IsAbstract || method.ReturnsByRef || method.ReturnsByRefReadonly) return "generic, abstract or by-reference method";
		if (method.IsAsync && method.ReturnsVoid) return "async void is not awaitable";
		if (method.Parameters.Any(p => p.RefKind != RefKind.None || p.Type.TypeKind == TypeKind.Delegate)) return "by-reference or delegate parameter";
		if (method.ReturnType.TypeKind == TypeKind.Delegate) return "delegate result";
		return null;
	}
	static string Identity(ITypeSymbol type) => type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
	static bool Special(ITypeSymbol type) => Identity(type) is "System.Threading.CancellationToken" or "Maui.HybridApp.HybridActionContext";
	static bool Ordinary(IParameterSymbol parameter) => !Special(parameter.Type) &&
		!parameter.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() is "Maui.HybridApp.FromServicesAttribute" or "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
	static ITypeSymbol? Result(IMethodSymbol method)
	{
		if (method.ReturnsVoid) return null;
		if (method.ReturnType is INamedTypeSymbol type && type.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
			type.Name is "Task" or "ValueTask")
			return type.Arity == 0 ? null : type.TypeArguments[0];
		return method.ReturnType;
	}
	static bool Awaitable(IMethodSymbol method) => method.ReturnType is INamedTypeSymbol type &&
		type.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" && type.Name is "Task" or "ValueTask";
	static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
		SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
	static string TypeName(ITypeSymbol type) => type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(TypeFormat);
	static string AnnotatedTypeName(ITypeSymbol type) => type.ToDisplayString(TypeFormat);
	static bool Nullable(ITypeSymbol type) => type.NullableAnnotation == NullableAnnotation.Annotated ||
		type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };
	static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
	static string Default(IParameterSymbol parameter)
	{
		var value = parameter.ExplicitDefaultValue;
		if (value is null) return "default";
		var defaultType = parameter.Type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
			? nullable.TypeArguments[0] : parameter.Type;
		if (defaultType.TypeKind == TypeKind.Enum) return "(" + TypeName(defaultType) + ")" + Convert.ToString(value, CultureInfo.InvariantCulture);
		if (value is string text) return Literal(text);
		if (value is char character) return SymbolDisplay.FormatLiteral(character, true);
		if (value is bool boolean) return boolean ? "true" : "false";
		if (value is double numberValue && (double.IsNaN(numberValue) || double.IsInfinity(numberValue)))
			return "double." + (double.IsNaN(numberValue) ? "NaN" : numberValue > 0 ? "PositiveInfinity" : "NegativeInfinity");
		if (value is float floatValue && (float.IsNaN(floatValue) || float.IsInfinity(floatValue)))
			return "float." + (float.IsNaN(floatValue) ? "NaN" : floatValue > 0 ? "PositiveInfinity" : "NegativeInfinity");
		var number = Convert.ToString(value, CultureInfo.InvariantCulture)!;
		return value switch
		{
			float => number + "F", double => number + "D", decimal => number + "M",
			ulong => number + "UL", long => number + "L", uint => number + "U", _ => number,
		};
	}

	sealed class JsonContractException(ITypeSymbol type, string message) : Exception(message)
	{
		internal ITypeSymbol Type { get; } = type;
	}

	sealed class JsonGraph
	{
		internal readonly List<ITypeSymbol> Types = new();
		internal readonly Dictionary<ITypeSymbol, ObjectShape> Objects = new(SymbolEqualityComparer.IncludeNullability);
		internal int Index(ITypeSymbol type) => Types.FindIndex(t => SymbolEqualityComparer.IncludeNullability.Equals(t, type));
		internal void Add(ITypeSymbol type)
		{
			if (Index(type) >= 0) return;
			if (type.TypeKind is TypeKind.TypeParameter or TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.Delegate or TypeKind.Dynamic ||
				type.SpecialType == SpecialType.System_Object || type is INamedTypeSymbol { IsRefLikeType: true })
				throw new JsonContractException(type, "use a concrete JSON contract, not object, dynamic, a delegate, pointer or ref-like type");
			Types.Add(type);
			if (Scalar(type) is not null || type.TypeKind == TypeKind.Enum || type.ToDisplayString() == "System.Text.Json.JsonElement") return;
			if (type is IArrayTypeSymbol array)
			{
				if (array.Rank != 1) throw new JsonContractException(type, "only one-dimensional arrays are supported");
				Add(array.ElementType); return;
			}
			if (type is INamedTypeSymbol named)
			{
				if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) { Add(named.TypeArguments[0]); return; }
				var collection = Collection(named);
				if (collection is not null)
				{
					if (collection == "dictionary" && named.TypeArguments[0].SpecialType != SpecialType.System_String)
						throw new JsonContractException(type, "dictionary keys must be strings");
					Add(named.TypeArguments.Last()); return;
				}
				var ns = named.ContainingNamespace.ToDisplayString();
				if ((ns == "System" || ns.StartsWith("System.", StringComparison.Ordinal)) || named.IsAbstract || named.TypeKind == TypeKind.Interface)
					throw new JsonContractException(type, "unsupported framework or polymorphic contract; use a concrete DTO");
				var shape = Shape(named);
				Objects.Add(type, shape);
				foreach (var property in shape.Properties) Add(property.Type);
				foreach (var parameter in shape.Constructor.Parameters) Add(parameter.Type);
				return;
			}
			throw new JsonContractException(type, "unsupported JSON type");
		}

		static ObjectShape Shape(INamedTypeSymbol type)
		{
			var properties = new List<IPropertySymbol>();
			for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
				foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
				{
					if (property.IsStatic || property.IsIndexer || property.GetMethod is null || !Accessible(property.GetMethod)) continue;
					var attributes = property.GetAttributes();
					if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonIgnoreAttribute"))
					{
						if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonIgnoreAttribute" && a.NamedArguments.Length != 0))
							throw new JsonContractException(type, "conditional JsonIgnore is not supported");
						continue;
					}
					if (attributes.Any(a => a.AttributeClass?.Name is "JsonConverterAttribute" or "JsonExtensionDataAttribute" or "JsonNumberHandlingAttribute"))
						throw new JsonContractException(type, "custom converters, number handling and extension data are not supported by automatic action metadata");
					if (!properties.Any(p => p.Name == property.Name)) properties.Add(property);
				}
			if (!Accessible(type) || type.GetAttributes().Any(a => a.AttributeClass?.Name is "JsonConverterAttribute" or "JsonPolymorphicAttribute" or "JsonDerivedTypeAttribute"))
				throw new JsonContractException(type, "inaccessible or polymorphic/custom-converter DTO");
			if (properties.Select(WireName).Distinct(StringComparer.Ordinal).Count() != properties.Count)
				throw new JsonContractException(type, "duplicate JSON property names");
			var constructors = type.InstanceConstructors.Where(c => Accessible(c) &&
				c.Parameters.All(p => p.RefKind == RefKind.None && properties.Any(property => string.Equals(property.Name, p.Name, StringComparison.OrdinalIgnoreCase) &&
					SymbolEqualityComparer.Default.Equals(property.Type, p.Type)))).ToArray();
			var selected = constructors.Where(c => c.GetAttributes().Any(a => a.AttributeClass?.Name == "JsonConstructorAttribute")).ToArray();
			if (selected.Length > 1) throw new JsonContractException(type, "ambiguous JSON constructors");
			var constructor = selected.SingleOrDefault() ?? constructors.OrderByDescending(c => c.Parameters.Length).FirstOrDefault();
			if (constructor is null) throw new JsonContractException(type, "an accessible constructor must bind its parameters to JSON properties");
			foreach (var property in properties)
				if (!constructor.Parameters.Any(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)) &&
					(property.SetMethod is null || !Accessible(property.SetMethod)))
					throw new JsonContractException(type, "read-only properties must bind to constructor parameters");
				else if (property.SetMethod?.IsInitOnly == true && !property.IsRequired &&
					!constructor.Parameters.Any(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)))
					throw new JsonContractException(type, "optional init-only properties must bind to constructor parameters");
			return new ObjectShape(properties, constructor);
		}
	}
	sealed class ObjectShape(List<IPropertySymbol> properties, IMethodSymbol constructor)
	{
		internal List<IPropertySymbol> Properties { get; } = properties;
		internal IMethodSymbol Constructor { get; } = constructor;
	}
	static string WireName(IPropertySymbol property) =>
		property.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "JsonPropertyNameAttribute")?.ConstructorArguments[0].Value as string ??
		CamelCase(property.Name);
	static string CamelCase(string name)
	{
		var chars = name.ToCharArray();
		for (int i = 0; i < chars.Length && char.IsUpper(chars[i]); i++)
		{
			if (i > 0 && i + 1 < chars.Length && !char.IsUpper(chars[i + 1])) break;
			chars[i] = char.ToLowerInvariant(chars[i]);
		}
		return new string(chars);
	}
	static string? Collection(INamedTypeSymbol type) =>
		type.ContainingNamespace.ToDisplayString() == "System.Collections.Generic" ? type.Name switch
		{
			"Dictionary" or "IDictionary" or "IReadOnlyDictionary" => "dictionary",
			"List" or "IList" or "IReadOnlyList" or "ICollection" or "IReadOnlyCollection" or "IEnumerable" => "list",
			"HashSet" or "ISet" or "IReadOnlySet" => "set",
			_ => null,
		} : null;
	static string? Scalar(ITypeSymbol type) => type.SpecialType switch
	{
		SpecialType.System_String => "String",
		SpecialType.System_Boolean => "Boolean",
		SpecialType.System_Byte => "Byte", SpecialType.System_SByte => "SByte",
		SpecialType.System_Int16 => "Int16", SpecialType.System_UInt16 => "UInt16",
		SpecialType.System_Int32 => "Int32", SpecialType.System_UInt32 => "UInt32",
		SpecialType.System_Int64 => "Int64", SpecialType.System_UInt64 => "UInt64",
		SpecialType.System_Single => "Single", SpecialType.System_Double => "Double",
		SpecialType.System_Decimal => "Decimal", SpecialType.System_Char => "Char",
		_ => Identity(type) switch
		{
			"System.DateTime" => "DateTime", "System.DateTimeOffset" => "DateTimeOffset",
			"System.Guid" => "Guid", "System.DateOnly" => "DateOnly", "System.TimeOnly" => "TimeOnly",
			"System.TimeSpan" => "TimeSpan", "System.Uri" => "Uri", _ => null,
		},
	};

	sealed class Emitter(INamedTypeSymbol provider, List<IMethodSymbol> methods, JsonGraph graph, int index)
	{
		readonly StringBuilder _code = new();
		internal string Id => "HybridActions_" + index.ToString(CultureInfo.InvariantCulture) + "_" + provider.Name;
		void Line(string text = "") => _code.AppendLine(text);
		string Info(ITypeSymbol type) => "Json.Default.T" + graph.Index(type);
		string Read(ITypeSymbol type, string value) => "Read" + graph.Index(type) + "(" + value + ", " + (Nullable(type) ? "true" : "false") + ")";
		string Write(ITypeSymbol type, string value) => "Write" + graph.Index(type) + "(writer, " + value + (type.IsReferenceType ? "!" : "") + ", " + (Nullable(type) ? "true" : "false") + ");";
		internal string Emit()
		{
			Line("// <auto-generated/>");
			Line("#nullable enable");
			Line("#pragma warning disable CS1591, CA2255");
			Line("using System;");
			Line("using System.Linq;");
			Line("using System.Threading.Tasks;");
			Line("using System.Text.Json;");
			Line("using System.Text.Json.Serialization;");
			Line("using System.Text.Json.Serialization.Metadata;");
			Line("using Microsoft.Extensions.DependencyInjection;");
			Line("namespace Maui.HybridApp.Generated;");
			Line((provider.IsStatic ? "public" : "internal") + " static class " + Id);
			Line("{");
			var target = TypeName(provider);
			var registrationType = provider.IsStatic ? "ProviderMarker" : target;
			if (provider.IsStatic)
			{
				Line("sealed class ProviderMarker { public ProviderMarker() { } }");
				Line("/// <summary>Explicitly opts in to the direct calls generated for " + provider.Name + ".</summary>");
				Line("public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection Add" + provider.Name +
					"HybridActions(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services) => global::Maui.HybridApp.HybridActionServiceCollectionExtensions.AddHybridActions<ProviderMarker>(services);");
			}
			Line("[global::System.Runtime.CompilerServices.ModuleInitializer]");
			var requiresInstance = methods.Any(method => !method.IsStatic) || provider.AllInterfaces.Any(type => type.ToDisplayString() == "Maui.HybridApp.IHybridActionSource");
			Line("internal static void Initialize() => global::Maui.HybridApp.HybridActionRegistration<" + registrationType + ">.Initialize(Register, requiresInstance: " + (requiresInstance ? "true" : "false") + ");");
			Line("static void Register(global::Maui.HybridApp.CommandRegistry registry, global::System.IServiceProvider? services, " + registrationType + "? instance, global::Maui.HybridApp.NativeDispatcher? dispatcher)");
			Line("{");
			foreach (var method in methods) EmitAction(method, target);
			Line("}");
			EmitJson();
			Line("}");
			return _code.ToString();
		}
		void EmitAction(IMethodSymbol method, string target)
		{
			var ordinary = method.Parameters.Where(Ordinary).ToArray();
			var result = Result(method);
			var infos = ordinary.Select(p => p.Type).Concat(result is null ? Enumerable.Empty<ITypeSymbol>() : new[] { result })
				.Distinct(SymbolEqualityComparer.Default).OfType<ITypeSymbol>();
			var metadata = infos.Select(Info).ToList();
			if (result is null) metadata.Add("Json.Default.Null");
			Line("registry.RegisterGenerated(" + Literal(Name(method)) + ", new JsonTypeInfo[] { " + string.Join(", ", metadata) + " }, async (json, operation) =>");
			Line("{");
			Line("var args = new global::Maui.HybridApp.HybridActionArguments(json, new string[] { " + string.Join(", ", ordinary.Select(p => Literal(p.Name))) + " });");
			for (int i = 0; i < ordinary.Length; i++)
			{
				var parameter = ordinary[i];
				var read = "args.Read(" + i + ", " + Info(parameter.Type) + ", " + (Nullable(parameter.Type) ? "true" : "false") + ")";
				Line("var arg" + i + " = " + (parameter.HasExplicitDefaultValue ? "args.Has(" + i + ") ? " + read + " : " + Default(parameter) : read) + ";");
			}
			Line("Task<JsonElement> Invoke()"); Line("{");
			var arguments = method.Parameters.Select(parameter =>
			{
				if (Ordinary(parameter)) return "arg" + Array.IndexOf(ordinary, parameter);
				if (Identity(parameter.Type) == "System.Threading.CancellationToken") return "operation.Cancellation";
				if (Identity(parameter.Type) == "Maui.HybridApp.HybridActionContext") return "new global::Maui.HybridApp.HybridActionContext(operation)";
				var keyed = parameter.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
				var key = keyed is null ? "" : ", " + Constant(keyed.ConstructorArguments[0]);
				return "(services ?? throw new InvalidOperationException(\"This action requires the owning service scope.\"))." +
					(keyed is null ? "GetRequiredService" : "GetRequiredKeyedService") + "<" + TypeName(parameter.Type) + ">(" + key.TrimStart(',', ' ') + ")";
			});
			var call = (method.IsStatic ? target : "(instance ?? (services ?? throw new InvalidOperationException(\"This action requires its owning service scope.\")).GetRequiredService<" + target + ">())") +
				".@" + method.Name + "(" + string.Join(", ", arguments) + ")";
			if (Awaitable(method))
			{
				Line("return Await();");
				Line("async Task<JsonElement> Await()"); Line("{");
				Line((result is null ? "" : "var result = ") + "await " + call + ";");
				EmitResult(result, false);
				Line("}");
			}
			else
			{
				Line((result is null ? "" : "var result = ") + call + ";");
				EmitResult(result, true);
			}
			Line("}");
			if (Policy(method, "MainThread"))
				Line("return await (dispatcher ?? (services ?? throw new InvalidOperationException(\"This action requires a native dispatcher.\")).GetRequiredService<global::Maui.HybridApp.NativeDispatcher>()).RunAsync(Invoke, operation.Cancellation).ConfigureAwait(false);");
			else Line("return await Invoke().ConfigureAwait(false);");
			Line("}, interactive: " + (Policy(method, "Interactive") ? "true" : "false") + ");");
		}
		void EmitResult(ITypeSymbol? result, bool task)
		{
			if (result is { IsReferenceType: true } && !Nullable(result))
				Line("if (result is null) throw new JsonException(\"An action returned null for a nonnullable contract.\");");
			var expression = result is null ? "global::System.Text.Json.JsonSerializer.SerializeToElement<object?>(null, Json.Default.Null)" :
				"global::System.Text.Json.JsonSerializer.SerializeToElement(result" + (result.IsReferenceType ? "!" : "") + ", " + Info(result) + ")";
			Line("return " + (task ? "Task.FromResult(" + expression + ")" : expression) + ";");
		}
		static string Constant(TypedConstant constant)
		{
			if (constant.Value is ITypeSymbol type) return "typeof(" + TypeName(type) + ")";
			if (constant.Type?.TypeKind == TypeKind.Enum)
				return "(" + TypeName(constant.Type) + ")" + Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
			if (constant.Type?.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16)
				return "(" + TypeName(constant.Type) + ")" + Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
			return constant.Value switch
			{
				null => "null", string value => Literal(value), char value => SymbolDisplay.FormatLiteral(value, true),
				bool boolean => boolean ? "true" : "false",
				long value => value.ToString(CultureInfo.InvariantCulture) + "L",
				ulong value => value.ToString(CultureInfo.InvariantCulture) + "UL",
				uint value => value.ToString(CultureInfo.InvariantCulture) + "U",
				float value => value.ToString(CultureInfo.InvariantCulture) + "F",
				double value => value.ToString(CultureInfo.InvariantCulture) + "D",
				_ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)!,
			};
		}
		void EmitJson()
		{
			Line("sealed class Json : JsonSerializerContext");
			Line("{");
			Line("static JsonSerializerOptions Strict() => new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };");
			Line("internal static Json Default { get; } = new();");
			Line("protected override JsonSerializerOptions? GeneratedSerializerOptions => Strict();");
			Line("Json() : base(Strict()) {");
			for (int i = 0; i < graph.Types.Count; i++)
			{
				Line("T" + i + " = JsonMetadataServices.CreateValueInfo<" + TypeName(graph.Types[i]) + ">(Options, new Converter" + i + "());");
				Line("T" + i + ".OriginatingResolver = this;");
			}
			Line("Null = JsonMetadataServices.CreateValueInfo<object?>(Options, new NullConverter()); Null.OriginatingResolver = this;");
			Line("}");
			Line("internal JsonTypeInfo<object?> Null { get; }");
			for (int i = 0; i < graph.Types.Count; i++)
				Line("internal JsonTypeInfo<" + TypeName(graph.Types[i]) + "> T" + i + " { get; }");
			Line("public override JsonTypeInfo? GetTypeInfo(Type type) {");
			for (int i = 0; i < graph.Types.Count; i++)
				Line("if (type == typeof(" + TypeName(graph.Types[i]) + ")) return T" + i + ";");
			Line("return type == typeof(object) ? Null : null;");
			Line("}");
			Line("}");
			Line("sealed class NullConverter : JsonConverter<object?> { public override object? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => throw new JsonException(); public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options) { if (value is not null) throw new JsonException(); writer.WriteNullValue(); } }");
			for (int i = 0; i < graph.Types.Count; i++) EmitType(i, graph.Types[i]);
		}
		void EmitType(int index, ITypeSymbol type)
		{
			var name = TypeName(type);
			Line("sealed class Converter" + index + " : JsonConverter<" + name + "> {");
			Line("public override bool HandleNull => true;");
			Line("public override " + name + " Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) { using var document = JsonDocument.ParseValue(ref reader); return Read" + index + "(document.RootElement, true); }");
			Line("public override void Write(Utf8JsonWriter writer, " + name + " value, JsonSerializerOptions options) => Write" + index + "(writer, value, true);");
			Line("}");
			Line("static " + name + " Read" + index + "(JsonElement json, bool nullable) {");
			if (type.IsReferenceType || Nullable(type))
				Line("if (json.ValueKind == JsonValueKind.Null) { if (!nullable) throw new JsonException(\"Null is not allowed.\"); return default!; }");
			var scalar = Scalar(type);
			if (scalar == "Char")
				Line("var text = json.GetString(); return text is { Length: 1 } ? text[0] : throw new JsonException(\"Expected one character.\");");
			else if (scalar is "DateOnly" or "TimeOnly" or "TimeSpan")
				Line("return " + name + ".Parse(json.GetString()!, global::System.Globalization.CultureInfo.InvariantCulture);");
			else if (scalar == "Uri") Line("return new global::System.Uri(json.GetString()!, global::System.UriKind.RelativeOrAbsolute);");
			else if (scalar is not null) Line("return json.Get" + scalar + "()" + (scalar == "String" ? "!" : "") + ";");
			else if (type.TypeKind == TypeKind.Enum)
				Line("return (" + name + ")json.Get" + Scalar(((INamedTypeSymbol)type).EnumUnderlyingType!) + "();");
			else if (type.ToDisplayString() == "System.Text.Json.JsonElement") Line("return json.Clone();");
			else if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType)
				Line("return " + Read(nullableType.TypeArguments[0], "json") + ";");
			else if (type is IArrayTypeSymbol array) EmitReadCollection(array.ElementType, "array", name);
			else if (type is INamedTypeSymbol collection && Collection(collection) is string kind)
				EmitReadCollection(collection.TypeArguments.Last(), kind, name);
			else EmitReadObject(type, graph.Objects[type]);
			Line("}");
			Line("static void Write" + index + "(Utf8JsonWriter writer, " + name + " value, bool nullable) {");
			if (type.IsReferenceType || Nullable(type))
				Line("if (value is null) { if (!nullable) throw new JsonException(\"Null is not allowed.\"); writer.WriteNullValue(); return; }");
			if (scalar is "String" or "Char" or "DateTime" or "DateTimeOffset" or "Guid")
				Line("writer.WriteStringValue(" + (scalar == "Char" ? "value.ToString()" : "value") + ");");
			else if (scalar is "DateOnly" or "TimeOnly" or "TimeSpan")
				Line("writer.WriteStringValue(value.ToString(" + Literal(scalar == "TimeSpan" ? "c" : "O") + ", global::System.Globalization.CultureInfo.InvariantCulture));");
			else if (scalar == "Uri") Line("writer.WriteStringValue(value.OriginalString);");
			else if (scalar == "Boolean") Line("writer.WriteBooleanValue(value);");
			else if (scalar is not null) Line("writer.WriteNumberValue(value);");
			else if (type.TypeKind == TypeKind.Enum)
				Line("writer.WriteNumberValue((" + TypeName(((INamedTypeSymbol)type).EnumUnderlyingType!) + ")value);");
			else if (type.ToDisplayString() == "System.Text.Json.JsonElement") Line("value.WriteTo(writer);");
			else if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } wrapper)
				Line(Write(wrapper.TypeArguments[0], "value.Value"));
			else if (type is IArrayTypeSymbol sequence) EmitWriteCollection(sequence.ElementType, false);
			else if (type is INamedTypeSymbol items && Collection(items) is string collectionKind)
				EmitWriteCollection(items.TypeArguments.Last(), collectionKind == "dictionary");
			else
			{
				Line("writer.WriteStartObject();");
				foreach (var property in graph.Objects[type].Properties)
				{
					Line("writer.WritePropertyName(" + Literal(WireName(property)) + ");");
					Line(Write(property.Type, "value.@" + property.Name));
				}
				Line("writer.WriteEndObject();");
			}
			Line("}");
		}
		void EmitReadCollection(ITypeSymbol element, string kind, string name)
		{
			if (kind == "dictionary")
			{
				Line("if (json.ValueKind != JsonValueKind.Object) throw new JsonException();");
				Line("var result = new global::System.Collections.Generic.Dictionary<string, " + AnnotatedTypeName(element) + ">(StringComparer.Ordinal);");
				Line("foreach (var item in json.EnumerateObject()) if (!result.TryAdd(item.Name, " + Read(element, "item.Value") + ")) throw new JsonException(\"Duplicate key.\");");
			}
			else
			{
				Line("if (json.ValueKind != JsonValueKind.Array) throw new JsonException();");
				Line("var result = new global::System.Collections.Generic." + (kind == "set" ? "HashSet" : "List") + "<" + AnnotatedTypeName(element) + ">();");
				Line("foreach (var item in json.EnumerateArray()) result.Add(" + Read(element, "item") + ");");
			}
			Line("return result" + (kind == "array" ? ".ToArray()" : "") + ";");
		}
		void EmitWriteCollection(ITypeSymbol element, bool dictionary)
		{
			Line(dictionary ? "writer.WriteStartObject();" : "writer.WriteStartArray();");
			Line("foreach (var item in value) {");
			if (dictionary) Line("writer.WritePropertyName(item.Key);");
			Line(Write(element, dictionary ? "item.Value" : "item"));
			Line("}");
			Line(dictionary ? "writer.WriteEndObject();" : "writer.WriteEndArray();");
		}
		void EmitReadObject(ITypeSymbol type, ObjectShape shape)
		{
			Line("if (json.ValueKind != JsonValueKind.Object) throw new JsonException();");
			for (int i = 0; i < shape.Properties.Count; i++)
			{
				Line("JsonElement p" + i + " = default; bool has" + i + " = false;");
			}
			Line("foreach (var property in json.EnumerateObject()) { switch (property.Name) {");
			for (int i = 0; i < shape.Properties.Count; i++)
				Line("case " + Literal(WireName(shape.Properties[i])) + ": if (has" + i + ") throw new JsonException(\"Duplicate member.\"); p" + i + " = property.Value; has" + i + " = true; break;");
			Line("default: throw new JsonException(\"Unknown member.\"); } }");
			foreach (var property in shape.Properties.Where(p => p.IsRequired || p.GetAttributes().Any(a => a.AttributeClass?.Name == "JsonRequiredAttribute")))
				Line("if (!has" + shape.Properties.IndexOf(property) + ") throw new JsonException(\"Missing required member.\");");
			var arguments = new List<string>();
			foreach (var parameter in shape.Constructor.Parameters)
			{
				var i = shape.Properties.FindIndex(p => string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
				var read = Read(parameter.Type, "p" + i);
				if (!parameter.HasExplicitDefaultValue) Line("if (!has" + i + ") throw new JsonException(\"Missing constructor member.\");");
				arguments.Add(parameter.HasExplicitDefaultValue ? "has" + i + " ? " + read + " : " + Default(parameter) : read);
			}
			var setters = shape.Properties.Where(p => !shape.Constructor.Parameters.Any(parameter => string.Equals(parameter.Name, p.Name, StringComparison.OrdinalIgnoreCase))).ToArray();
			// Construct first to preserve initializers for omitted optional properties. Init-only members are set during construction.
			var initializers = shape.Properties.Where(p => p.IsRequired || setters.Contains(p, SymbolEqualityComparer.Default) && p.SetMethod!.IsInitOnly).ToArray();
			Line("var result = new " + TypeName(type) + "(" + string.Join(", ", arguments) + ")" +
				(initializers.Length == 0 ? "" : " { " + string.Join(", ", initializers.Select(p => "@" + p.Name + " = " + Read(p.Type, "p" + shape.Properties.IndexOf(p)))) + " }") + ";");
			foreach (var property in setters.Except(initializers))
			{
				var i = shape.Properties.IndexOf(property);
				Line("if (has" + i + ") result.@" + property.Name + " = " + Read(property.Type, "p" + i) + ";");
			}
			Line("return result;");
		}
	}
}
