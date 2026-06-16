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

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var invocations = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (node, _) => IsSetInvokeJavaScriptTargetCandidate(node),
			transform: static (ctx, ct) => GetInvocationInfo(ctx, ct))
			.Where(static info => info is not null);

		context.RegisterSourceOutput(invocations.Collect(), GenerateSource);
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
		var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocation, ct);
		if (symbolInfo.Symbol is not IMethodSymbol method)
			return null;

		// Must be the 2-arg overload: SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext ctx)
		if (method.Parameters.Length != 2)
			return null;

		var containingType = method.ContainingType?.ToDisplayString();
		if (containingType != HybridWebViewTypeName && containingType != IHybridWebViewTypeName)
			return null;

		var receiverTypeName = containingType == IHybridWebViewTypeName
			? IHybridWebViewFullyQualifiedTypeName
			: HybridWebViewFullyQualifiedTypeName;

		var secondParamType = method.Parameters[1].Type?.ToDisplayString();
		if (secondParamType != JsonSerializerContextTypeName)
			return null;

		// Get the type argument T
		if (method.TypeArguments.Length != 1)
			return null;

		var targetType = method.TypeArguments[0] as INamedTypeSymbol;
		if (targetType is null)
			return null;

		// Get interceptable location
		var interceptableLocation = ctx.SemanticModel.GetInterceptableLocation(invocation, ct);
		if (interceptableLocation is null)
			return null;

		// Gather public instance methods on T (DeclaredOnly equivalent)
		var methods = new List<MethodInfo>();
		foreach (var member in targetType.GetMembers())
		{
			if (member is not IMethodSymbol m)
				continue;
			if (m.DeclaredAccessibility != Accessibility.Public)
				continue;
			if (m.IsStatic || m.IsAbstract)
				continue;
			if (m.MethodKind != MethodKind.Ordinary)
				continue;
			if (m.IsGenericMethod)
				continue;
			if (!SymbolEqualityComparer.Default.Equals(m.ContainingType, targetType))
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

		var overloadedMethodNames = methods
			.GroupBy(static method => method.Name)
			.Where(static group => group.Count() > 1)
			.Select(static group => group.Key)
			.OrderBy(static name => name)
			.ToArray();

		return new InvocationInfo(
			interceptableLocation,
			invocation.GetLocation(),
			receiverTypeName,
			targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
			methods.ToArray(),
			overloadedMethodNames);
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

			var loc = info.Location;

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
			if (info.ReceiverTypeName == IHybridWebViewFullyQualifiedTypeName)
			{
				sb.AppendLine("            if (hybridWebView is not global::Microsoft.Maui.Controls.HybridWebView concreteHybridWebView)");
				sb.AppendLine("                throw new InvalidOperationException(\"The AOT-safe HybridWebView source-generated invoker can only be registered on Microsoft.Maui.Controls.HybridWebView instances.\");");
				sb.AppendLine();
				sb.AppendLine($"            concreteHybridWebView.SetInvoker(new Invoker_{index}(typedTarget, jsonSerializerContext));");
			}
			else
			{
				sb.AppendLine($"            hybridWebView.SetInvoker(new Invoker_{index}(typedTarget, jsonSerializerContext));");
			}
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
						sb.AppendLine($"                        var __{p.Name} = global::System.Text.Json.JsonSerializer.Deserialize<{p.TypeName}>(paramJsonValues[{i}], GetRequiredJsonTypeInfo<{p.TypeName}>(_ctx, \"{method.Name}\", \"{p.Name}\"));");
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
						sb.AppendLine($"                        var __result = await _target.{method.Name}({argList});");
						sb.AppendLine($"                        if (__result is null) return null;");
						sb.AppendLine($"                        return global::System.Text.Json.JsonSerializer.Serialize<{method.ResultTypeName}>(__result, GetRequiredJsonTypeInfo<{method.ResultTypeName}>(_ctx, \"{method.Name}\", \"return\"));");
						break;
					case "sync":
						sb.AppendLine($"                        var __result = _target.{method.Name}({argList});");
						sb.AppendLine($"                        if (__result is null) return null;");
						sb.AppendLine($"                        return global::System.Text.Json.JsonSerializer.Serialize<{method.ResultTypeName}>(__result, GetRequiredJsonTypeInfo<{method.ResultTypeName}>(_ctx, \"{method.Name}\", \"return\"));");
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
		public InvocationInfo(InterceptableLocation location, Location diagnosticLocation, string receiverTypeName, string targetTypeName, MethodInfo[] methods, string[] overloadedMethodNames)
		{
			Location = location;
			DiagnosticLocation = diagnosticLocation;
			ReceiverTypeName = receiverTypeName;
			TargetTypeName = targetTypeName;
			Methods = methods;
			OverloadedMethodNames = overloadedMethodNames;
		}
		public InterceptableLocation Location { get; }
		public Location DiagnosticLocation { get; }
		public string ReceiverTypeName { get; }
		public string TargetTypeName { get; }
		public MethodInfo[] Methods { get; }
		public string[] OverloadedMethodNames { get; }
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
