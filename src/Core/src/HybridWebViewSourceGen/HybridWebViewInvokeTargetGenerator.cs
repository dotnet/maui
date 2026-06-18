using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.HybridWebViewSourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class HybridWebViewInvokeTargetGenerator : IIncrementalGenerator
{
	private const string SetInvokeJavaScriptTargetMethodName = "SetInvokeJavaScriptTarget";
	private const string HybridWebViewTypeName = "Microsoft.Maui.Controls.HybridWebView";
	private const string IHybridWebViewTypeName = "Microsoft.Maui.IHybridWebView";
	private const string JsonSerializerContextTypeName = "System.Text.Json.Serialization.JsonSerializerContext";
	private const string HybridWebViewFullyQualifiedTypeName = "global::Microsoft.Maui.Controls.HybridWebView";
	private const string IHybridWebViewFullyQualifiedTypeName = "global::Microsoft.Maui.IHybridWebView";
	private static readonly DiagnosticDescriptor UnsupportedOverloadedMethods = new(
		"MAUIHWVSG001",
		"HybridWebView invoke targets cannot contain overloaded methods",
		"HybridWebView invoke target type '{0}' contains overloaded public method name(s): {1}. JavaScript-to-.NET invocation is dispatched by method name, so overloaded target methods are not supported.",
		"HybridWebView",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	private static readonly DiagnosticDescriptor OpenGenericInvokeTarget = new(
		"MAUIHWVSG002",
		"HybridWebView invoke target type must be closed",
		"HybridWebView invoke target type '{0}' is open generic. SetInvokeJavaScriptTarget<T> requires a closed target type so the source generator can generate AOT-safe dispatch.",
		"HybridWebView",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	private static readonly DiagnosticDescriptor InaccessibleInvokeTarget = new(
		"MAUIHWVSG003",
		"HybridWebView invoke target type must be accessible",
		"HybridWebView invoke target type '{0}' is not accessible to generated code. Use an internal or public invoke target type.",
		"HybridWebView",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var invocations = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (node, _) => IsSetInvokeJavaScriptTargetCandidate(node),
			transform: static (ctx, ct) => GetInvocationInfo(ctx, ct))
			.Where(static info => info is not null);

		context.RegisterImplementationSourceOutput(invocations.Collect(), GenerateSource);
	}

	private static bool IsSetInvokeJavaScriptTargetCandidate(SyntaxNode node)
	{
		if (node is not InvocationExpressionSyntax invocation)
			return false;

		var name = invocation.Expression switch
		{
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
			_ => null,
		};

		return name == SetInvokeJavaScriptTargetMethodName;
	}

	private static InvocationInfo? GetInvocationInfo(GeneratorSyntaxContext ctx, System.Threading.CancellationToken ct)
	{
		var invocation = (InvocationExpressionSyntax)ctx.Node;
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return null;

		// Must be the 2-arg overload: SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext ctx)
		if (invocation.ArgumentList.Arguments.Count != 2)
			return null;

		string receiverTypeName;
		ITypeSymbol? targetTypeSymbol;
		var method = ctx.SemanticModel.GetSymbolInfo(invocation.Expression, ct).Symbol as IMethodSymbol;
		if (method is not null)
		{
			if (method.Parameters.Length != 2)
				return null;

			var containingType = method.ContainingType?.ToDisplayString();
			if (containingType != HybridWebViewTypeName && containingType != IHybridWebViewTypeName)
				return null;

			var secondParamType = method.Parameters[1].Type?.ToDisplayString();
			if (secondParamType != JsonSerializerContextTypeName)
				return null;

			// Get the type argument T
			if (method.TypeArguments.Length != 1)
				return null;

			receiverTypeName = containingType == IHybridWebViewTypeName
				? IHybridWebViewFullyQualifiedTypeName
				: HybridWebViewFullyQualifiedTypeName;
			targetTypeSymbol = method.TypeArguments[0];
		}
		else
		{
			var receiverType = ctx.SemanticModel.GetTypeInfo(memberAccess.Expression, ct).Type;
			if (receiverType is null)
				return null;

			if (IsSameOrDerivedFrom(receiverType, HybridWebViewTypeName))
			{
				receiverTypeName = HybridWebViewFullyQualifiedTypeName;
			}
			else if (IsSameOrDerivedFrom(receiverType, IHybridWebViewTypeName) || ImplementsInterface(receiverType, IHybridWebViewTypeName))
			{
				receiverTypeName = IHybridWebViewFullyQualifiedTypeName;
			}
			else
			{
				return null;
			}

			var jsonSerializerContextType = ctx.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[1].Expression, ct).Type;
			if (jsonSerializerContextType is not null
				&& jsonSerializerContextType is not IErrorTypeSymbol
				&& !IsSameOrDerivedFrom(jsonSerializerContextType, JsonSerializerContextTypeName))
			{
				return null;
			}

			targetTypeSymbol = GetTargetType(ctx, memberAccess, invocation, ct);
		}

		if (targetTypeSymbol is null)
			return null;

		if (ContainsTypeParameter(targetTypeSymbol))
		{
			return new InvocationInfo(
				invocation.GetLocation(),
				targetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}

		if (targetTypeSymbol is not INamedTypeSymbol targetType)
			return null;

		if (!IsAccessibleFromGeneratedCode(targetType))
		{
			return InvocationInfo.CreateInaccessibleTarget(
				invocation.GetLocation(),
				targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}

		// Get interceptable location
		var interceptableLocation = ctx.SemanticModel.GetInterceptableLocation(invocation, ct);
		if (interceptableLocation is null)
			return null;

		var overloadedMethodNames = GetPublicInstanceMethods(targetType)
			.GroupBy(static method => method.Name)
			.Where(static group => group.Count() > 1)
			.Select(static group => group.Key)
			.OrderBy(static name => name)
			.ToArray();

		// Gather public instance methods on T, including inherited public methods
		// to match the legacy reflection invoker's dispatch behavior.
		var methods = new List<MethodInfo>();
		foreach (var m in GetPublicInstanceMethods(targetType))
		{
			if (m.IsGenericMethod)
				continue;

			// Check for ref/out/pointer params
			bool hasUnsupportedParam = false;
			var paramInfos = new List<ParamInfo>();
			foreach (var p in m.Parameters)
			{
				if (p.RefKind != RefKind.None)
				{
					hasUnsupportedParam = true;
					break;
				}
				paramInfos.Add(new ParamInfo(p.Name, p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
			}

			if (hasUnsupportedParam)
				continue;

			// Determine return kind
			var returnType = m.ReturnType;
			string returnKind;
			string? resultTypeName = null;

			if (returnType.SpecialType == SpecialType.System_Void)
			{
				returnKind = "void";
			}
			else if (returnType.ToDisplayString() == "System.Threading.Tasks.Task")
			{
				returnKind = "Task";
			}
			else if (returnType is INamedTypeSymbol namedReturn
				&& namedReturn.IsGenericType
				&& namedReturn.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
			{
				returnKind = "TaskOfT";
				resultTypeName = namedReturn.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			}
			else
			{
				returnKind = "sync";
				resultTypeName = returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			}

			methods.Add(new MethodInfo(m.Name, returnKind, resultTypeName, paramInfos.ToArray()));
		}

		return new InvocationInfo(
			interceptableLocation,
			invocation.GetLocation(),
			receiverTypeName,
			targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
			methods.ToArray(),
			overloadedMethodNames);
	}

	private static IEnumerable<IMethodSymbol> GetPublicInstanceMethods(INamedTypeSymbol targetType)
	{
		var seenSignatures = new HashSet<string>();

		if (targetType.TypeKind == TypeKind.Interface)
		{
			foreach (var method in GetPublicInstanceMethods(targetType, seenSignatures))
				yield return method;

			foreach (var interfaceType in targetType.AllInterfaces)
			{
				foreach (var method in GetPublicInstanceMethods(interfaceType, seenSignatures))
					yield return method;
			}

			yield break;
		}

		for (var current = targetType; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
		{
			foreach (var method in GetPublicInstanceMethods(current, seenSignatures))
				yield return method;
		}
	}

	private static IEnumerable<IMethodSymbol> GetPublicInstanceMethods(INamedTypeSymbol type, HashSet<string> seenSignatures)
	{
		foreach (var member in type.GetMembers())
		{
			if (member is not IMethodSymbol method)
				continue;
			if (method.DeclaredAccessibility != Accessibility.Public)
				continue;
			if (method.IsStatic)
				continue;
			if (method.MethodKind != MethodKind.Ordinary)
				continue;

			if (seenSignatures.Add(GetMethodSignature(method)))
				yield return method;
		}
	}

	private static string GetMethodSignature(IMethodSymbol method)
	{
		var parameters = string.Join(
			",",
			method.Parameters.Select(static parameter =>
				$"{parameter.RefKind}:{parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));

		return $"{method.Name}`{method.TypeParameters.Length}({parameters})";
	}

	private static bool IsAccessibleFromGeneratedCode(INamedTypeSymbol targetType)
	{
		for (var current = targetType; current is not null; current = current.ContainingType)
		{
			if (current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal))
				return false;
		}

		return true;
	}

	private static ITypeSymbol? GetTargetType(GeneratorSyntaxContext ctx, MemberAccessExpressionSyntax memberAccess, InvocationExpressionSyntax invocation, System.Threading.CancellationToken ct)
	{
		if (memberAccess.Name is GenericNameSyntax genericName
			&& genericName.TypeArgumentList.Arguments.Count == 1
			&& ctx.SemanticModel.GetSymbolInfo(genericName.TypeArgumentList.Arguments[0], ct).Symbol is ITypeSymbol explicitType)
		{
			return explicitType;
		}

		return ctx.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, ct).Type;
	}

	private static bool ContainsTypeParameter(ITypeSymbol type)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
			return true;

		return type switch
		{
			INamedTypeSymbol namedType => namedType.TypeArguments.Any(ContainsTypeParameter),
			IArrayTypeSymbol arrayType => ContainsTypeParameter(arrayType.ElementType),
			IPointerTypeSymbol pointerType => ContainsTypeParameter(pointerType.PointedAtType),
			_ => false
		};
	}

	private static bool IsSameOrDerivedFrom(ITypeSymbol type, string typeName)
	{
		for (var current = type; current is not null; current = current.BaseType)
		{
			if (current.ToDisplayString() == typeName)
				return true;
		}

		return false;
	}

	private static bool ImplementsInterface(ITypeSymbol type, string interfaceTypeName)
	{
		return type.AllInterfaces.Any(interfaceType => interfaceType.ToDisplayString() == interfaceTypeName);
	}

	private static void GenerateSource(SourceProductionContext spc, ImmutableArray<InvocationInfo?> invocations)
	{
		if (invocations.IsDefaultOrEmpty)
			return;

		var sb = new StringBuilder();
		sb.AppendLine("// <auto-generated />");
		sb.AppendLine("#nullable enable");
		sb.AppendLine();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Collections.Generic;");
		sb.AppendLine("using System.Text.Json;");
		sb.AppendLine("using System.Text.Json.Serialization;");
		sb.AppendLine("using System.Text.Json.Serialization.Metadata;");
		sb.AppendLine("using System.Threading.Tasks;");
		sb.AppendLine();
		sb.AppendLine("namespace System.Runtime.CompilerServices");
		sb.AppendLine("{");
		sb.AppendLine("    [global::System.Diagnostics.Conditional(\"DEBUG\")]");
		sb.AppendLine("    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]");
		sb.AppendLine("    file sealed class InterceptsLocationAttribute : Attribute");
		sb.AppendLine("    {");
		sb.AppendLine("        public InterceptsLocationAttribute(int version, string data) { }");
		sb.AppendLine("    }");
		sb.AppendLine("}");
		sb.AppendLine();
		sb.AppendLine("namespace Microsoft.Maui.Controls.Generated");
		sb.AppendLine("{");
		sb.AppendLine("    file static class HybridWebViewInterceptors");
		sb.AppendLine("    {");

		int index = 0;
		foreach (var info in invocations)
		{
			if (info is null)
				continue;

			if (info.OpenGenericTargetTypeName is not null)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					OpenGenericInvokeTarget,
					info.DiagnosticLocation,
					info.OpenGenericTargetTypeName));
				continue;
			}

			if (info.InaccessibleTargetTypeName is not null)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					InaccessibleInvokeTarget,
					info.DiagnosticLocation,
					info.InaccessibleTargetTypeName));
				continue;
			}

			var loc = info.Location!;

			if (info.OverloadedMethodNames.Length > 0)
			{
				spc.ReportDiagnostic(Diagnostic.Create(
					UnsupportedOverloadedMethods,
					info.DiagnosticLocation,
					info.TargetTypeName,
					string.Join(", ", info.OverloadedMethodNames)));
				continue;
			}

			// Generate the interceptor method
			sb.AppendLine($"        [global::System.Runtime.CompilerServices.InterceptsLocationAttribute({loc.Version}, @\"{loc.Data}\")]");
			sb.AppendLine($"        public static void SetInvokeJavaScriptTarget_{index}<T>(this {info.ReceiverTypeName} hybridWebView, T target, global::System.Text.Json.Serialization.JsonSerializerContext jsonSerializerContext) where T : class");
			sb.AppendLine("        {");
			sb.AppendLine("            if (target is null) throw new ArgumentNullException(nameof(target));");
			sb.AppendLine("            if (jsonSerializerContext is null) throw new ArgumentNullException(nameof(jsonSerializerContext));");
			sb.AppendLine();
			sb.AppendLine($"            if (target is not {info.TargetTypeName} typedTarget)");
			sb.AppendLine($"                throw new InvalidOperationException($\"Type mismatch: expected {info.TargetTypeName.Split('.').Last()} but got {{target.GetType().FullName}}\");");
			sb.AppendLine();
			sb.AppendLine($"            hybridWebView.Invoker = new Invoker_{index}(typedTarget, jsonSerializerContext);");
			sb.AppendLine("        }");
			sb.AppendLine();

			// Generate the HybridWebViewInvoker implementation
			sb.AppendLine($"        private sealed class Invoker_{index} : global::Microsoft.Maui.HybridWebViewInvoker");
			sb.AppendLine("        {");
			sb.AppendLine($"            private readonly {info.TargetTypeName} _target;");
			sb.AppendLine("            private readonly global::System.Text.Json.Serialization.JsonSerializerContext _ctx;");
			sb.AppendLine();
			sb.AppendLine($"            public Invoker_{index}({info.TargetTypeName} target, global::System.Text.Json.Serialization.JsonSerializerContext ctx)");
			sb.AppendLine($"                : base(target, typeof({info.TargetTypeName}))");
			sb.AppendLine("            {");
			sb.AppendLine("                _target = target;");
			sb.AppendLine("                _ctx = ctx;");
			sb.AppendLine("            }");
			sb.AppendLine();
			sb.AppendLine("            public override async global::System.Threading.Tasks.Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues)");
			sb.AppendLine("            {");
			sb.AppendLine("                switch (methodName)");
			sb.AppendLine("                {");

			foreach (var method in info.Methods)
			{
				sb.AppendLine($"                    case \"{method.Name}\":");
				sb.AppendLine("                    {");

				// Deserialize params
				if (method.Parameters.Length > 0)
				{
					sb.AppendLine($"                        if (paramJsonValues is null || paramJsonValues.Length != {method.Parameters.Length})");
					sb.AppendLine($"                            throw new global::System.InvalidOperationException(\"Method '{method.Name}' expects {method.Parameters.Length} argument(s) but received \" + (paramJsonValues?.Length ?? 0) + \".\");");

					for (int i = 0; i < method.Parameters.Length; i++)
					{
						var p = method.Parameters[i];
						sb.AppendLine($"                        var __{p.Name} = global::System.Text.Json.JsonSerializer.Deserialize<{p.TypeName}>(paramJsonValues[{i}], GetRequiredJsonTypeInfo<{p.TypeName}>(_ctx, \"{method.Name}\", \"{p.Name}\"))!;");
					}
				}

				var argList = string.Join(", ", Array.ConvertAll(method.Parameters, p => $"__{p.Name}"));

				switch (method.ReturnKind)
				{
					case "void":
						sb.AppendLine($"                        _target.{method.Name}({argList});");
						sb.AppendLine("                        return null;");
						break;
					case "Task":
						sb.AppendLine($"                        await _target.{method.Name}({argList});");
						sb.AppendLine("                        return null;");
						break;
					case "TaskOfT":
						sb.AppendLine($"                        return SerializeResult<{method.ResultTypeName}>(await _target.{method.Name}({argList}), _ctx, \"{method.Name}\");");
						break;
					case "sync":
						sb.AppendLine($"                        return SerializeResult<{method.ResultTypeName}>(_target.{method.Name}({argList}), _ctx, \"{method.Name}\");");
						break;
				}

				sb.AppendLine("                    }");
			}

			sb.AppendLine("                    default:");
			sb.AppendLine($"                        throw new global::System.InvalidOperationException($\"The method '{{methodName}}' was not found on type '{info.TargetTypeName}'.\");");
			sb.AppendLine("                }");
			sb.AppendLine("            }");
			sb.AppendLine("        }");
			sb.AppendLine();
			index++;
		}

		// Shared helper method
		sb.AppendLine("        private static string? SerializeResult<T>(T? result, global::System.Text.Json.Serialization.JsonSerializerContext ctx, string methodName)");
		sb.AppendLine("        {");
		sb.AppendLine("            if (result is null) return null;");
		sb.AppendLine("            return global::System.Text.Json.JsonSerializer.Serialize<T>(result, GetRequiredJsonTypeInfo<T>(ctx, methodName, \"return\"));");
		sb.AppendLine("        }");
		sb.AppendLine();
		sb.AppendLine("        private static global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> GetRequiredJsonTypeInfo<T>(global::System.Text.Json.Serialization.JsonSerializerContext ctx, string methodName, string paramName)");
		sb.AppendLine("        {");
		sb.AppendLine("            var typeInfo = ctx.GetTypeInfo(typeof(T));");
		sb.AppendLine("            if (typeInfo is not global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typed)");
		sb.AppendLine("                throw new global::System.InvalidOperationException($\"The JSON serializer context does not contain metadata for type '{typeof(T).FullName}' (used by {paramName} of method '{methodName}'). Add [JsonSerializable(typeof({typeof(T).Name}))] to your JsonSerializerContext.\");");
		sb.AppendLine("            return typed;");
		sb.AppendLine("        }");
		sb.AppendLine("    }");
		sb.AppendLine("}");

		spc.AddSource("HybridWebViewInterceptors.g.cs", sb.ToString());
	}

	private sealed class InvocationInfo
	{
		public InvocationInfo(Location diagnosticLocation, string openGenericTargetTypeName)
		{
			Location = null;
			DiagnosticLocation = diagnosticLocation;
			ReceiverTypeName = string.Empty;
			TargetTypeName = string.Empty;
			Methods = [];
			OverloadedMethodNames = [];
			OpenGenericTargetTypeName = openGenericTargetTypeName;
			InaccessibleTargetTypeName = null;
		}

		private InvocationInfo(Location diagnosticLocation, string inaccessibleTargetTypeName, bool _)
		{
			Location = null;
			DiagnosticLocation = diagnosticLocation;
			ReceiverTypeName = string.Empty;
			TargetTypeName = string.Empty;
			Methods = [];
			OverloadedMethodNames = [];
			OpenGenericTargetTypeName = null;
			InaccessibleTargetTypeName = inaccessibleTargetTypeName;
		}

		public InvocationInfo(InterceptableLocation location, Location diagnosticLocation, string receiverTypeName, string targetTypeName, MethodInfo[] methods, string[] overloadedMethodNames)
		{
			Location = location;
			DiagnosticLocation = diagnosticLocation;
			ReceiverTypeName = receiverTypeName;
			TargetTypeName = targetTypeName;
			Methods = methods;
			OverloadedMethodNames = overloadedMethodNames;
			OpenGenericTargetTypeName = null;
			InaccessibleTargetTypeName = null;
		}

		public static InvocationInfo CreateInaccessibleTarget(Location diagnosticLocation, string inaccessibleTargetTypeName) =>
			new(diagnosticLocation, inaccessibleTargetTypeName, true);

		public InterceptableLocation? Location { get; }
		public Location DiagnosticLocation { get; }
		public string ReceiverTypeName { get; }
		public string TargetTypeName { get; }
		public MethodInfo[] Methods { get; }
		public string[] OverloadedMethodNames { get; }
		public string? OpenGenericTargetTypeName { get; }
		public string? InaccessibleTargetTypeName { get; }
	}

	private sealed class MethodInfo
	{
		public MethodInfo(string name, string returnKind, string? resultTypeName, ParamInfo[] parameters)
		{
			Name = name;
			ReturnKind = returnKind;
			ResultTypeName = resultTypeName;
			Parameters = parameters;
		}
		public string Name { get; }
		public string ReturnKind { get; }
		public string? ResultTypeName { get; }
		public ParamInfo[] Parameters { get; }
	}

	private sealed class ParamInfo
	{
		public ParamInfo(string name, string typeName)
		{
			Name = name;
			TypeName = typeName;
		}
		public string Name { get; }
		public string TypeName { get; }
	}
}
