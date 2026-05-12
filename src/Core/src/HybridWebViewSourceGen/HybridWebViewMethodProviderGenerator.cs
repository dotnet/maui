using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.HybridWebViewSourceGen;

[Generator(LanguageNames.CSharp)]
public class HybridWebViewMethodProviderGenerator : IIncrementalGenerator
{
	private const string ProviderAttributeFqn = "Microsoft.Maui.HybridWebViewDotNetMethodProviderAttribute";
	private const string CallableAttributeFqn = "Microsoft.Maui.HybridWebViewCallableAttribute";
	private const string ProviderInterfaceFqn = "Microsoft.Maui.IHybridWebViewDotNetMethodProvider";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var classDeclarations = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				ProviderAttributeFqn,
				predicate: static (node, _) => node is ClassDeclarationSyntax,
				transform: static (ctx, ct) => GetClassInfo(ctx, ct))
			.Where(static info => info is not null)
			.Select(static (info, _) => info!);

		context.RegisterSourceOutput(classDeclarations, static (spc, classInfo) =>
		{
			GenerateSource(spc, classInfo);
		});
	}

	private static HybridWebViewClassInfo? GetClassInfo(
		GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
	{
		if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
			return null;

		var classDecl = ctx.TargetNode as ClassDeclarationSyntax;
		if (classDecl is null)
			return null;

		// Check partial
		bool isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

		// Read attribute data
		var attrData = ctx.Attributes.FirstOrDefault();
		if (attrData is null)
			return null;

		// Get JsonSerializerContext type
		INamedTypeSymbol? jsonContextType = null;
		if (attrData.ConstructorArguments.Length > 0 && attrData.ConstructorArguments[0].Value is INamedTypeSymbol ctxType)
		{
			jsonContextType = ctxType;
		}

		// Get ExposeAllPublicMethods
		bool exposeAll = false;
		foreach (var namedArg in attrData.NamedArguments)
		{
			if (namedArg.Key == "ExposeAllPublicMethods" && namedArg.Value.Value is bool b)
			{
				exposeAll = b;
			}
		}

		// Collect methods
		var methods = new List<HybridWebViewMethodInfo>();
		bool hasAnyCallableAttribute = false;
		var allPublicMethods = classSymbol.GetMembers()
			.OfType<IMethodSymbol>()
			.Where(m => m.MethodKind == MethodKind.Ordinary
				&& m.DeclaredAccessibility == Accessibility.Public
				&& !m.IsStatic
				&& !IsObjectMethod(m));

		foreach (var method in allPublicMethods)
		{
			ct.ThrowIfCancellationRequested();

			var callableAttr = method.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == CallableAttributeFqn);

			if (callableAttr is not null)
				hasAnyCallableAttribute = true;
		}

		// Determine which methods to include
		foreach (var method in allPublicMethods)
		{
			ct.ThrowIfCancellationRequested();

			var callableAttr = method.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == CallableAttributeFqn);

			bool include;
			if (exposeAll)
			{
				// AllPublic mode: include all public methods
				include = true;
			}
			else
			{
				// Explicit mode: only [HybridWebViewCallable] methods
				include = callableAttr is not null;
			}

			if (!include)
				continue;

			// Read Name override
			string? jsName = null;
			INamedTypeSymbol? methodJsonContext = null;
			if (callableAttr is not null)
			{
				foreach (var namedArg in callableAttr.NamedArguments)
				{
					if (namedArg.Key == "Name" && namedArg.Value.Value is string name)
						jsName = name;
					else if (namedArg.Key == "JsonSerializerContext" && namedArg.Value.Value is INamedTypeSymbol mCtx)
						methodJsonContext = mCtx;
				}
			}

			// Check for unsupported params
			var diagnostics = new List<DiagnosticInfo>();
			foreach (var param in method.Parameters)
			{
				if (param.RefKind != RefKind.None)
				{
					diagnostics.Add(new DiagnosticInfo(
						"HWV003",
						$"Parameter '{param.Name}' on method '{method.Name}' uses unsupported modifier '{param.RefKind}'.",
						method));
				}
			}

			// Determine return type info
			var returnInfo = GetReturnTypeInfo(method);

			methods.Add(new HybridWebViewMethodInfo(
				methodName: method.Name,
				jsName: jsName ?? method.Name,
				parameters: method.Parameters.Select(p => new ParameterInfo(p.Name, p.Type)).ToImmutableArray(),
				returnTypeInfo: returnInfo,
				methodJsonContextType: methodJsonContext,
				diagnostics: diagnostics.ToImmutableArray()));
		}

		return new HybridWebViewClassInfo(
			classSymbol: classSymbol,
			isPartial: isPartial,
			jsonContextType: jsonContextType,
			exposeAllPublicMethods: exposeAll,
			methods: methods.ToImmutableArray());
	}

	private static ReturnTypeInfo GetReturnTypeInfo(IMethodSymbol method)
	{
		var returnType = method.ReturnType;

		if (returnType.SpecialType == SpecialType.System_Void)
			return new ReturnTypeInfo(ReturnTypeKind.Void, null);

		if (returnType.ToDisplayString() == "System.Threading.Tasks.Task")
			return new ReturnTypeInfo(ReturnTypeKind.Task, null);

		if (returnType is INamedTypeSymbol namedType
			&& namedType.IsGenericType
			&& namedType.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
		{
			return new ReturnTypeInfo(ReturnTypeKind.TaskOfT, namedType.TypeArguments[0]);
		}

		return new ReturnTypeInfo(ReturnTypeKind.Sync, returnType);
	}

	private static bool IsObjectMethod(IMethodSymbol method)
	{
		return method.Name is "ToString" or "Equals" or "GetHashCode" or "GetType"
			&& method.ContainingType.SpecialType == SpecialType.System_Object;
	}

	private static void GenerateSource(SourceProductionContext spc, HybridWebViewClassInfo classInfo)
	{
		// Report diagnostics
		if (!classInfo.IsPartial)
		{
			spc.ReportDiagnostic(Diagnostic.Create(
				Diagnostics.ClassMustBePartial,
				classInfo.ClassSymbol.Locations.FirstOrDefault(),
				classInfo.ClassSymbol.Name));
			return;
		}

		if (classInfo.Methods.IsEmpty)
		{
			spc.ReportDiagnostic(Diagnostic.Create(
				Diagnostics.NoCallableMethods,
				classInfo.ClassSymbol.Locations.FirstOrDefault(),
				classInfo.ClassSymbol.Name));
			return;
		}

		// Check for duplicate JS names
		var jsNames = new Dictionary<string, string>();
		foreach (var m in classInfo.Methods)
		{
			if (jsNames.TryGetValue(m.JsName, out var existing))
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.DuplicateJsName,
					classInfo.ClassSymbol.Locations.FirstOrDefault(),
					m.JsName, m.MethodName, existing));
			}
			else
			{
				jsNames[m.JsName] = m.MethodName;
			}
		}

		// Report per-method diagnostics
		foreach (var m in classInfo.Methods)
		{
			foreach (var d in m.Diagnostics)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.UnsupportedParameter,
					classInfo.ClassSymbol.Locations.FirstOrDefault(),
					d.Message));
			}
		}

		// Generate code
		var code = EmitSource(classInfo);
		var fileName = $"{classInfo.ClassSymbol.ToDisplayString().Replace('.', '_').Replace('<', '_').Replace('>', '_')}.HybridWebView.g.cs";
		spc.AddSource(fileName, code);
	}

	private static string EmitSource(HybridWebViewClassInfo classInfo)
	{
		var sb = new StringBuilder();
		var ns = classInfo.ClassSymbol.ContainingNamespace;
		var className = classInfo.ClassSymbol.Name;
		var accessibility = classInfo.ClassSymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";

		sb.AppendLine("// <auto-generated/>");
		sb.AppendLine("#nullable enable");
		sb.AppendLine();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Text.Json;");
		sb.AppendLine("using System.Threading.Tasks;");
		sb.AppendLine();

		if (!ns.IsGlobalNamespace)
		{
			sb.AppendLine($"namespace {ns.ToDisplayString()}");
			sb.AppendLine("{");
		}

		sb.AppendLine($"    {accessibility} partial class {className} : global::Microsoft.Maui.IHybridWebViewDotNetMethodProvider");
		sb.AppendLine("    {");
		sb.AppendLine("        async global::System.Threading.Tasks.Task<string?> global::Microsoft.Maui.IHybridWebViewDotNetMethodProvider.InvokeMethodAsync(string methodName, string[]? paramJsonValues)");
		sb.AppendLine("        {");
		sb.AppendLine("            switch (methodName)");
		sb.AppendLine("            {");

		foreach (var method in classInfo.Methods)
		{
			EmitMethodCase(sb, method, classInfo);
		}

		sb.AppendLine("                default:");
		sb.AppendLine($"                    throw new global::System.InvalidOperationException($\"Method '{{methodName}}' not found on type '{className}'.\");");
		sb.AppendLine("            }");
		sb.AppendLine("        }");
		sb.AppendLine("    }");

		if (!ns.IsGlobalNamespace)
		{
			sb.AppendLine("}");
		}

		return sb.ToString();
	}

	private static void EmitMethodCase(StringBuilder sb, HybridWebViewMethodInfo method, HybridWebViewClassInfo classInfo)
	{
		var contextType = method.MethodJsonContextType ?? classInfo.JsonContextType;
		if (contextType is null)
			return;

		var contextDefault = $"global::{contextType.ToDisplayString()}.Default";

		sb.AppendLine($"                case \"{method.JsName}\":");
		sb.AppendLine("                {");

		// Validate parameter count
		if (method.Parameters.Length > 0)
		{
			sb.AppendLine($"                    if (paramJsonValues is null || paramJsonValues.Length != {method.Parameters.Length})");
			sb.AppendLine($"                        throw new global::System.ArgumentException($\"Method '{method.JsName}' expects {method.Parameters.Length} parameter(s), but received {{paramJsonValues?.Length ?? 0}}.\");");
		}

		// Deserialize parameters
		for (int i = 0; i < method.Parameters.Length; i++)
		{
			var param = method.Parameters[i];
			var typeInfoProp = GetJsonTypeInfoPropertyName(param.Type);
			sb.AppendLine($"                    var _p{i} = global::System.Text.Json.JsonSerializer.Deserialize(paramJsonValues[{i}], {contextDefault}.{typeInfoProp})!;");
		}

		// Build call arguments
		var args = string.Join(", ", Enumerable.Range(0, method.Parameters.Length).Select(i => $"_p{i}"));

		// Invoke and handle return
		switch (method.ReturnTypeInfo.Kind)
		{
			case ReturnTypeKind.Void:
				sb.AppendLine($"                    {method.MethodName}({args});");
				sb.AppendLine("                    return null;");
				break;

			case ReturnTypeKind.Task:
				sb.AppendLine($"                    await {method.MethodName}({args});");
				sb.AppendLine("                    return null;");
				break;

			case ReturnTypeKind.TaskOfT:
				var resultTypeInfoProp = GetJsonTypeInfoPropertyName(method.ReturnTypeInfo.InnerType!);
				sb.AppendLine($"                    var _result = await {method.MethodName}({args});");
				sb.AppendLine($"                    return global::System.Text.Json.JsonSerializer.Serialize(_result, {contextDefault}.{resultTypeInfoProp});");
				break;

			case ReturnTypeKind.Sync:
				var syncResultTypeInfoProp = GetJsonTypeInfoPropertyName(method.ReturnTypeInfo.InnerType!);
				sb.AppendLine($"                    var _result = {method.MethodName}({args});");
				sb.AppendLine($"                    return global::System.Text.Json.JsonSerializer.Serialize(_result, {contextDefault}.{syncResultTypeInfoProp});");
				break;
		}

		sb.AppendLine("                }");
	}

	private static string GetJsonTypeInfoPropertyName(ITypeSymbol type)
	{
		// Map common types to their JsonSerializerContext property names
		switch (type.SpecialType)
		{
			case SpecialType.System_Boolean: return "Boolean";
			case SpecialType.System_Byte: return "Byte";
			case SpecialType.System_SByte: return "SByte";
			case SpecialType.System_Int16: return "Int16";
			case SpecialType.System_UInt16: return "UInt16";
			case SpecialType.System_Int32: return "Int32";
			case SpecialType.System_UInt32: return "UInt32";
			case SpecialType.System_Int64: return "Int64";
			case SpecialType.System_UInt64: return "UInt64";
			case SpecialType.System_Single: return "Single";
			case SpecialType.System_Double: return "Double";
			case SpecialType.System_Decimal: return "Decimal";
			case SpecialType.System_Char: return "Char";
			case SpecialType.System_String: return "String";
			case SpecialType.System_Object: return "Object";
		}

		// For arrays
		if (type is IArrayTypeSymbol arrayType)
		{
			return GetJsonTypeInfoPropertyName(arrayType.ElementType) + "Array";
		}

		// For generic types like Dictionary<string, int>
		if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
		{
			var baseName = namedType.Name;
			var args = string.Join("", namedType.TypeArguments.Select(t => GetJsonTypeInfoPropertyName(t)));
			return baseName + args;
		}

		// For nullable value types
		if (type.NullableAnnotation == NullableAnnotation.Annotated && type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
		{
			if (type is INamedTypeSymbol nullable && nullable.TypeArguments.Length == 1)
			{
				return "Nullable" + GetJsonTypeInfoPropertyName(nullable.TypeArguments[0]);
			}
		}

		// For regular types, use the type name
		return type.Name;
	}
}

// Model types for the generator pipeline

internal sealed class HybridWebViewClassInfo
{
	public HybridWebViewClassInfo(
		INamedTypeSymbol classSymbol,
		bool isPartial,
		INamedTypeSymbol? jsonContextType,
		bool exposeAllPublicMethods,
		ImmutableArray<HybridWebViewMethodInfo> methods)
	{
		ClassSymbol = classSymbol;
		IsPartial = isPartial;
		JsonContextType = jsonContextType;
		ExposeAllPublicMethods = exposeAllPublicMethods;
		Methods = methods;
	}

	public INamedTypeSymbol ClassSymbol { get; }
	public bool IsPartial { get; }
	public INamedTypeSymbol? JsonContextType { get; }
	public bool ExposeAllPublicMethods { get; }
	public ImmutableArray<HybridWebViewMethodInfo> Methods { get; }
}

internal sealed class HybridWebViewMethodInfo
{
	public HybridWebViewMethodInfo(
		string methodName,
		string jsName,
		ImmutableArray<ParameterInfo> parameters,
		ReturnTypeInfo returnTypeInfo,
		INamedTypeSymbol? methodJsonContextType,
		ImmutableArray<DiagnosticInfo> diagnostics)
	{
		MethodName = methodName;
		JsName = jsName;
		Parameters = parameters;
		ReturnTypeInfo = returnTypeInfo;
		MethodJsonContextType = methodJsonContextType;
		Diagnostics = diagnostics;
	}

	public string MethodName { get; }
	public string JsName { get; }
	public ImmutableArray<ParameterInfo> Parameters { get; }
	public ReturnTypeInfo ReturnTypeInfo { get; }
	public INamedTypeSymbol? MethodJsonContextType { get; }
	public ImmutableArray<DiagnosticInfo> Diagnostics { get; }
}

internal sealed class ParameterInfo
{
	public ParameterInfo(string name, ITypeSymbol type)
	{
		Name = name;
		Type = type;
	}

	public string Name { get; }
	public ITypeSymbol Type { get; }
}

internal sealed class ReturnTypeInfo
{
	public ReturnTypeInfo(ReturnTypeKind kind, ITypeSymbol? innerType)
	{
		Kind = kind;
		InnerType = innerType;
	}

	public ReturnTypeKind Kind { get; }
	public ITypeSymbol? InnerType { get; }
}

internal enum ReturnTypeKind
{
	Void,
	Task,
	TaskOfT,
	Sync,
}

internal sealed class DiagnosticInfo
{
	public DiagnosticInfo(string id, string message, IMethodSymbol method)
	{
		Id = id;
		Message = message;
		Method = method;
	}

	public string Id { get; }
	public string Message { get; }
	public IMethodSymbol Method { get; }
}
