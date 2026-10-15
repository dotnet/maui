using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.HybridWebApps.SourceGen;

/// <summary>Emits direct action calls and their own AOT-safe JSON metadata, without depending on another generator's output.</summary>
[Generator]
public sealed class HybridActionGenerator : IIncrementalGenerator
{
	const string AttributeName = "Microsoft.Maui.Interop.HybridActionAttribute";
	const string EventAttributeName = "Microsoft.Maui.Interop.HybridEventAttribute";
	const string TopicAttributeName = "Microsoft.Maui.Interop.HybridTopicAttribute";
	const string ExportAttributeName = "Microsoft.Maui.Interop.HybridExportAttribute";
	static readonly DiagnosticDescriptor InvalidName = Rule("HYBRID001", "Invalid action name", "Action '{0}' has an invalid wire name");
	static readonly DiagnosticDescriptor InvalidMethod = Rule("HYBRID002", "Unsupported action method", "Action '{0}' must be accessible, nongeneric and non-async-void, without ref/out/in or delegate parameters: {1}");
	static readonly DiagnosticDescriptor Duplicate = Rule("HYBRID003", "Duplicate action name", "Action or topic name '{0}' is duplicated within an assembly or explicitly registered assemblies");
	static readonly DiagnosticDescriptor InvalidJson = Rule("HYBRID004", "Unsupported JSON contract", "Action '{0}' cannot generate strict JSON for '{1}': {2}");
	static readonly DiagnosticDescriptor InvalidBinding = Rule("HYBRID005", "Invalid parameter binding", "Action parameter '{0}' has conflicting or unsupported service/context/cancellation binding");
	static readonly DiagnosticDescriptor InvalidEvent = Rule("HYBRID006", "Invalid event contract", "Topic '{0}' requires an accessible void event delegate without ref/out/in, pointers, ref-like or inaccessible parameters, or paired parameterless Task start and void stop hooks");
	static readonly DiagnosticDescriptor InvalidRoot = Rule("HYBRID007", "Invalid native contract root", "Contract '{0}' cannot be imported or constructed: {1}");

	static DiagnosticDescriptor Rule(string id, string title, string message) =>
		new(id, title, message, "Microsoft.Maui.Controls.HybridWebApps", DiagnosticSeverity.Error, true);

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var methods = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName,
			static (node, _) => node is MethodDeclarationSyntax or AccessorDeclarationSyntax,
			static (syntax, _) => (IMethodSymbol)syntax.TargetSymbol).Collect();
		var events = context.SyntaxProvider.ForAttributeWithMetadataName(EventAttributeName,
			static (node, _) => node is MethodDeclarationSyntax or EventDeclarationSyntax or EventFieldDeclarationSyntax or VariableDeclaratorSyntax,
			static (syntax, _) => syntax.TargetSymbol).Collect();
		var topics = context.SyntaxProvider.ForAttributeWithMetadataName(TopicAttributeName,
			static (node, _) => node is ClassDeclarationSyntax,
			static (syntax, _) => (INamedTypeSymbol)syntax.TargetSymbol).Collect();
		var assemblies = context.SyntaxProvider.CreateSyntaxProvider(
			static (node, _) => node is InvocationExpressionSyntax,
			static (syntax, _) => (syntax.SemanticModel.GetSymbolInfo(syntax.Node).Symbol as IMethodSymbol)?.ContainingType)
			.Where(static type => type is not null && type.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Maui.Controls.HybridWebApps.HybridActionAssemblyAttribute")).Collect();
		context.RegisterSourceOutput(methods.Combine(events).Combine(topics).Combine(context.CompilationProvider).Combine(assemblies),
			static (output, inputs) => Generate(output, inputs.Left.Left.Left.Left, inputs.Left.Left.Left.Right,
				inputs.Left.Left.Right, inputs.Left.Right, inputs.Right));
	}

	static void Generate(SourceProductionContext output, ImmutableArray<IMethodSymbol> methods, ImmutableArray<ISymbol> eventSymbols,
		ImmutableArray<INamedTypeSymbol> topics, Compilation compilation, ImmutableArray<INamedTypeSymbol?> registeredAssemblies)
	{
		var assembly = compilation.AssemblyName!;
		var contractsId = "HybridContracts_" + StableId(assembly);
		var textType = compilation.GetSpecialType(SpecialType.System_String);
		var targets = new Dictionary<IMethodSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
		var selected = methods.Where(m => m.ContainingType.TypeKind == TypeKind.Class).ToList();
		foreach (var method in selected)
			targets[method] = method.ContainingType;
		var nativeEvents = new Dictionary<INamedTypeSymbol, List<IEventSymbol>>(SymbolEqualityComparer.Default);
		foreach (var ev in eventSymbols.OfType<IEventSymbol>().Where(ev => ev.ContainingType.TypeKind == TypeKind.Class))
		{
			if (!nativeEvents.TryGetValue(ev.ContainingType, out var list))
				nativeEvents[ev.ContainingType] = list = new();
			list.Add(ev);
		}
		var events = eventSymbols.OfType<IMethodSymbol>().ToArray();
		var imported = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
		foreach (var root in compilation.Assembly.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == ExportAttributeName))
		{
			if (root.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol type ||
				type.TypeKind is not (TypeKind.Class or TypeKind.Interface) || !Accessible(type) || type.Arity != 0)
			{
				output.ReportDiagnostic(Diagnostic.Create(InvalidRoot, Location.None, root.ToString(), "use an accessible nongeneric interface or class"));
				continue;
			}
			if (!imported.Add(type))
			{
				output.ReportDiagnostic(Diagnostic.Create(InvalidRoot, Location.None, type.Name, "duplicate root"));
				continue;
			}
			foreach (var member in Members(type).Distinct(SymbolEqualityComparer.Default))
			{
				var method = member is IMethodSymbol { MethodKind: MethodKind.Ordinary } ordinary ? ordinary : (member as IPropertySymbol)?.GetMethod;
				if (method is not null && method.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == AttributeName))
				{
					if (targets.ContainsKey(method))
						output.ReportDiagnostic(Diagnostic.Create(InvalidRoot, Location.None, type.Name, "member is imported more than once"));
					else
					{ selected.Add(method); targets[method] = type; }
				}
				if (member is IEventSymbol ev && ev.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == EventAttributeName))
				{
					if (!nativeEvents.TryGetValue(type, out var list))
						nativeEvents[type] = list = new();
					if (!list.Contains(ev, SymbolEqualityComparer.Default))
						list.Add(ev);
				}
			}
		}
		methods = selected.ToImmutableArray();
		var contexts = new Dictionary<ITypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
		foreach (var root in compilation.Assembly.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Maui.Controls.HybridWebApps.HybridJsonContextAttribute"))
		{
			if (root.ConstructorArguments.Length == 2 && root.ConstructorArguments[0].Value is ITypeSymbol contract &&
				root.ConstructorArguments[1].Value is INamedTypeSymbol context &&
				context.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonSerializableAttribute" &&
					SymbolEqualityComparer.Default.Equals(a.ConstructorArguments[0].Value as ITypeSymbol, contract)) &&
				!contexts.ContainsKey(contract))
				contexts.Add(contract, context);
			else
				output.ReportDiagnostic(Diagnostic.Create(InvalidRoot, Location.None, root.ToString(), "root the exact type in one source-declared JsonSerializerContext"));
		}
		var registeredNames = new HashSet<string>(StringComparer.Ordinal);
		foreach (var method in methods.Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>())
			if (!registeredNames.Add(Name(method)))
				output.ReportDiagnostic(Diagnostic.Create(Duplicate, method.Locations.FirstOrDefault(), Name(method)));
		foreach (var registered in registeredAssemblies.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
			foreach (var attribute in registered.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Maui.Controls.HybridWebApps.HybridActionAssemblyAttribute"))
				if (attribute.ConstructorArguments[0].Value is string name && !registeredNames.Add(name))
					output.ReportDiagnostic(Diagnostic.Create(Duplicate, methods.FirstOrDefault(m => Name(m) == name)?.Locations.FirstOrDefault() ?? Location.None, name));

		var providers = methods.Select(method => targets[method]).Concat(events.Select(method => method.ContainingType)).Concat(topics).Concat(nativeEvents.Keys)
			.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal);
		var emitters = new List<(INamedTypeSymbol Type, string Id, bool Instance)>();
		var topicNames = new HashSet<string>(StringComparer.Ordinal);
		var graph = new JsonGraph(contexts);
		foreach (var provider in providers)
		{
			var group = methods.Where(method => SymbolEqualityComparer.Default.Equals(targets[method], provider));
			var lifecycle = events.Where(method => SymbolEqualityComparer.Default.Equals(method.ContainingType, provider)).ToArray();
			var declaredTopics = provider.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == TopicAttributeName)
				.Select(a => a.ConstructorArguments[0].Value as string ?? "").ToArray();
			var hooks = new List<(string Name, string Start, string Stop)>();
			foreach (var eventGroup in lifecycle.GroupBy(method => method.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == EventAttributeName).ConstructorArguments[0].Value as string ?? ""))
			{
				var starts = eventGroup.Where(method => Hook(method, 0)).ToArray();
				var stops = eventGroup.Where(method => Hook(method, 1)).ToArray();
				if (starts.Length != 1 || stops.Length != 1 || eventGroup.Any(method => MethodIssue(method) is not null || method.IsStatic || method.Parameters.Length != 0) ||
					starts[0].ReturnType.ToDisplayString() != "System.Threading.Tasks.Task" || !stops[0].ReturnsVoid)
					output.ReportDiagnostic(Diagnostic.Create(InvalidEvent, provider.Locations[0], eventGroup.Key));
				else
					hooks.Add((eventGroup.Key, starts[0].Name, stops[0].Name));
			}
			foreach (var topic in declaredTopics.Concat(hooks.Select(hook => hook.Name)))
			{
				if (!ValidName(topic))
					output.ReportDiagnostic(Diagnostic.Create(InvalidName, provider.Locations[0], topic));
				if (!topicNames.Add(topic))
					output.ReportDiagnostic(Diagnostic.Create(Duplicate, provider.Locations[0], topic));
			}
			var selectedNativeEvents = new List<IEventSymbol>();
			foreach (var ev in nativeEvents.TryGetValue(provider, out var value) ? value : new List<IEventSymbol>())
			{
				var name = EventName(ev);
				if (!ValidName(name))
					output.ReportDiagnostic(Diagnostic.Create(InvalidName, Location.None, name));
				if (!topicNames.Add(name))
					output.ReportDiagnostic(Diagnostic.Create(Duplicate, Location.None, name));
				if (!ValidEvent(ev, compilation))
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidEvent, ev.Locations.FirstOrDefault(), name));
					continue;
				}
				try
				{
					foreach (var parameter in Payload(ev))
						graph.Add(parameter.Type, outputOnly: true);
					selectedNativeEvents.Add(ev);
				}
				catch (JsonContractException error)
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidJson, ev.Locations.FirstOrDefault(), name, error.Type.ToDisplayString(), error.Message));
				}
			}
			var valid = new List<IMethodSymbol>();
			var names = new HashSet<string>(StringComparer.Ordinal);
			bool contractsValid = true;
			foreach (var method in group)
			{
				var name = Name(method);
				if (!ValidName(name))
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidName, method.Locations.FirstOrDefault(), method.Name));
					continue;
				}
				if (!names.Add(name))
				{
					output.ReportDiagnostic(Diagnostic.Create(Duplicate, method.Locations.FirstOrDefault(), name));
					continue;
				}
				var issue = MethodIssue(method, imported.Contains(provider));
				if (issue is null && !compilation.IsSymbolAccessibleWithin(method, compilation.Assembly))
					issue = "referenced member is not accessible to the consuming assembly";
				if (issue is not null)
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidMethod, method.Locations.FirstOrDefault(), method.Name, issue));
					continue;
				}
				bool good = true;
				if (method.Parameters.Count(p => Bytes(p.Type)) > 1 ||
					Bytes(Result(method)) && Nullable(Result(method)!) ||
					method.Parameters.Any(p => Bytes(p.Type) && (Nullable(p.Type) || p.HasExplicitDefaultValue)) ||
					method.Parameters.Any(p => Bytes(p.Type)) && !Bytes(Result(method)) ||
					method.Parameters.Any(p => Channel(p.Type) is not null && (Nullable(p.Type) || p.HasExplicitDefaultValue)))
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidBinding, method.Locations.FirstOrDefault(), method.Name));
					good = false;
				}
				foreach (var parameter in method.Parameters)
				{
					var attributes = parameter.GetAttributes();
					var services = attributes.Count(a => a.AttributeClass?.ToDisplayString() is "Microsoft.Maui.Interop.FromServicesAttribute" or "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
					var keyed = attributes.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
					if (services > 1 || (services != 0 && (Special(parameter.Type) || Channel(parameter.Type) is not null)) ||
						keyed is not null && (keyed.ConstructorArguments.Length != 1 || !SupportedKey(keyed.ConstructorArguments[0])))
					{
						output.ReportDiagnostic(Diagnostic.Create(InvalidBinding, parameter.Locations.FirstOrDefault(), parameter.Name));
						good = false;
					}
				}
				try
				{
					foreach (var parameter in method.Parameters.Where(Ordinary))
					{
						var channel = Channel(parameter.Type);
						graph.Add(channel is null ? parameter.Type : textType);
						if (channel is not null && !Bytes(channel))
							graph.Add(channel);
					}
					var result = Result(method);
					if (result is not null && !Bytes(result))
						graph.Add(result);
				}
				catch (JsonContractException error)
				{
					output.ReportDiagnostic(Diagnostic.Create(InvalidJson, method.Locations.FirstOrDefault(), method.Name, error.Type.ToDisplayString(), error.Message));
					good = false;
					contractsValid = false;
				}
				if (good)
					valid.Add(method);
			}
			if (!contractsValid)
				continue;
			var emitter = new Emitter(provider, valid, graph, declaredTopics, hooks, textType, selectedNativeEvents, imported.Contains(provider), contractsId);
			output.AddSource(emitter.Id + ".g.cs", emitter.Emit());
			emitter.EmitActionFiles(output);
			emitters.Add((provider, emitter.Id, !imported.Contains(provider) && (valid.Any(method => !method.IsStatic) || hooks.Count != 0 || selectedNativeEvents.Any(ev => !ev.IsStatic))));
		}
		if (emitters.Count == 0)
			return;
		new Emitter(providers.First(), new(), graph, [], new(), textType, new(), false, contractsId).EmitContracts(output);
		var id = string.Concat(assembly.Where(char.IsLetterOrDigit));
		if (id.Length == 0 || char.IsDigit(id[0]))
			id = "Assembly" + id;
		var code = new StringBuilder("// <auto-generated/>\n#nullable enable\nusing Microsoft.Extensions.DependencyInjection;\nusing Microsoft.Extensions.DependencyInjection.Extensions;\nnamespace Microsoft.Maui.Controls.HybridWebApps.Generated;\n");
		code.AppendLine("/// <summary>Explicitly registers every attributed plugin compiled in " + assembly + ".</summary>");
		foreach (var name in methods.Select(Name).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal))
			code.AppendLine("[global::Microsoft.Maui.Controls.HybridWebApps.HybridActionAssembly(" + Literal(name) + ")]");
		code.AppendLine("public static class " + id + "ActionAssembly {");
		code.AppendLine("/// <summary>Adds this assembly's scoped actions and declarative event lifecycles, preserving host overrides.</summary>");
		code.AppendLine("/// <param name=\"services\">The owning host's service collection.</param><returns>The same collection.</returns>");
		code.AppendLine("public static IServiceCollection Add" + id + "Actions(this IServiceCollection services) {");
		foreach (var emitter in emitters.Where(e => e.Instance))
		{
			var constructors = emitter.Type.InstanceConstructors.Where(Accessible).ToArray();
			var marked = constructors.Where(c => c.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute")).ToArray();
			var constructor = marked.Length == 1 ? marked[0] : constructors.Length == 1 ? constructors[0] : null;
			if (constructor is null || emitter.Type.IsAbstract || constructor.Parameters.Any(p => p.RefKind != RefKind.None ||
				p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute" &&
					(a.ConstructorArguments.Length != 1 || !SupportedKey(a.ConstructorArguments[0])))))
			{
				output.ReportDiagnostic(Diagnostic.Create(InvalidRoot, emitter.Type.Locations.FirstOrDefault(), emitter.Type.Name, "select exactly one accessible typed constructor"));
				continue;
			}
			code.AppendLine("global::Microsoft.Maui.Controls.HybridWebApps.HybridActionAssemblies.Target<" + TypeName(emitter.Type) + ">(services, services => new " +
				TypeName(emitter.Type) + "(" + string.Join(", ", constructor.Parameters.Select(ServiceArgument)) + "), perViewOnly: " +
				(constructor.Parameters.Any(p => Identity(p.Type) is "Microsoft.Maui.Controls.HybridWebApps.UI.HybridViewContext" or "Microsoft.Maui.Controls.HybridWebApps.NativeNotifications" or
					"Microsoft.Maui.Controls.HybridWebApps.HybridAppTargetObject" or "Microsoft.Maui.Controls.HybridWebApps.CommandRegistry" or "Microsoft.Maui.Controls.Page" or
					"Microsoft.Maui.Controls.Window" or "Microsoft.Maui.Controls.HybridWebView" or "Microsoft.Maui.Dispatching.IDispatcher") ? "true" : "false") + ");");
		}
		code.AppendLine("return global::Microsoft.Maui.Controls.HybridWebApps.HybridActionAssemblies.Add(services, typeof(" + id + "ActionAssembly), Register); }");
		code.AppendLine("static void Register(global::Microsoft.Maui.Controls.HybridWebApps.CommandRegistry registry, global::System.IServiceProvider services) {");
		foreach (var emitter in emitters)
			code.AppendLine(emitter.Id + ".Register(registry, services);");
		code.AppendLine("} }");
		output.AddSource(id + ".Actions.g.cs", code.ToString());
	}

	static IEnumerable<ISymbol> Members(INamedTypeSymbol type)
	{
		for (var current = type; current is not null; current = current.BaseType)
			foreach (var member in current.GetMembers())
				yield return member;
		foreach (var inherited in type.AllInterfaces)
			foreach (var member in inherited.GetMembers())
				yield return member;
	}
	static string ServiceArgument(IParameterSymbol parameter)
	{
		var keyed = parameter.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
		if (parameter.HasExplicitDefaultValue)
		{
			var lookup = keyed is null ? "services.GetService(typeof(" + TypeName(parameter.Type) + "))" :
				"services.GetKeyedService(typeof(" + TypeName(parameter.Type) + "), " + Emitter.Constant(keyed.ConstructorArguments[0]) + ")";
			var pattern = parameter.Type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable ? nullable.TypeArguments[0] : parameter.Type;
			var value = "__service" + parameter.Ordinal;
			return "(" + lookup + " is " + TypeName(pattern) + " " + value + " ? " + value + " : (" + AnnotatedTypeName(parameter.Type) + ")" + Default(parameter) + ")";
		}
		if (keyed is not null)
			return "services.GetRequiredKeyedService<" + TypeName(parameter.Type) + ">(" + Emitter.Constant(keyed.ConstructorArguments[0]) + ")";
		return "services.GetRequiredService<" + TypeName(parameter.Type) + ">()";
	}
	static bool SupportedKey(TypedConstant key) => key.Kind is TypedConstantKind.Primitive or TypedConstantKind.Enum or TypedConstantKind.Type;
	static string StableId(string value)
	{
		ulong hash = 14695981039346656037UL;
		foreach (var character in value)
			hash = unchecked((hash ^ character) * 1099511628211UL);
		return hash.ToString("x16", CultureInfo.InvariantCulture);
	}
	static bool Hook(IMethodSymbol method, int lifecycle)
	{
		var attribute = method.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == EventAttributeName);
		return attribute.ConstructorArguments.Length == 2 && attribute.ConstructorArguments[1].Value is int value && value == lifecycle;
	}
	static string EventName(IEventSymbol ev) =>
		ev.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == EventAttributeName).ConstructorArguments[0].Value as string ?? "";
	static IParameterSymbol[] Payload(IEventSymbol ev)
	{
		var invoke = ((INamedTypeSymbol)ev.Type).DelegateInvokeMethod!;
		var parameters = invoke.Parameters;
		if (parameters.Length >= 2 && parameters[0].Type.SpecialType == SpecialType.System_Object)
			parameters = parameters.RemoveAt(0);
		return parameters.Length == 1 && parameters[0].Type.ToDisplayString() == "System.EventArgs" ? [] : parameters.ToArray();
	}
	static bool ValidEvent(IEventSymbol ev, Compilation compilation)
	{
		if (ev.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == EventAttributeName).ConstructorArguments.Length != 1 ||
			ev.IsStatic && ev.IsAbstract)
			return false;
		if (ev.Type is not INamedTypeSymbol { DelegateInvokeMethod: { } invoke } || !invoke.ReturnsVoid ||
			ev.AddMethod is null || ev.RemoveMethod is null ||
			!compilation.IsSymbolAccessibleWithin(ev.AddMethod, compilation.Assembly) ||
			!compilation.IsSymbolAccessibleWithin(ev.RemoveMethod, compilation.Assembly) ||
			!compilation.IsSymbolAccessibleWithin(ev.Type, compilation.Assembly))
			return false;
		for (var type = ev.ContainingType; type is not null; type = type.ContainingType)
			if (!Accessible(type) || type.Arity != 0 || type.IsFileLocal)
				return false;
		return invoke.Parameters.All(p => p.RefKind == RefKind.None &&
			p.Type.TypeKind is not (TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.TypeParameter) &&
			p.Type is not INamedTypeSymbol { IsRefLikeType: true } &&
			compilation.IsSymbolAccessibleWithin(p.Type, compilation.Assembly));
	}
	static bool ValidName(string name) => name.Length is >= 1 and <= 64 && name.All(c => c <= 127 && (char.IsLetterOrDigit(c) || c is '.' or '_' or '-'));
	static AttributeData Attribute(IMethodSymbol method) => method.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == AttributeName);
	static string Name(IMethodSymbol method) => Attribute(method).ConstructorArguments[0].Value as string ?? "";
	static bool Policy(IMethodSymbol method, string name) => Attribute(method).NamedArguments.Any(pair => pair.Key == name && pair.Value.Value is true);
	static bool Accessible(ISymbol symbol) => symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;
	static string? MethodIssue(IMethodSymbol method, bool imported = false)
	{
		if (method.ContainingType.TypeKind != TypeKind.Class && !imported)
			return "use a plugin class (its action methods may be static)";
		if (!Accessible(method))
			return "method is not accessible from generated code";
		for (var type = method.ContainingType; type is not null; type = type.ContainingType)
			if (!Accessible(type) || type.Arity != 0 || type.IsFileLocal)
				return "containing type is inaccessible, file-local or generic";
		if (method.IsGenericMethod || method.IsAbstract && !imported || method.ReturnsByRef || method.ReturnsByRefReadonly)
			return "generic, abstract or by-reference method";
		if (method.IsAsync && method.ReturnsVoid)
			return "async void is not awaitable";
		if (method.Parameters.Any(p => p.RefKind != RefKind.None || p.Type.TypeKind == TypeKind.Delegate))
			return "by-reference or delegate parameter";
		if (method.ReturnType.TypeKind == TypeKind.Delegate)
			return "delegate result";
		return null;
	}
	static string Identity(ITypeSymbol type) => type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
	static bool Bytes(ITypeSymbol? type) => type is IArrayTypeSymbol { Rank: 1, ElementType.SpecialType: SpecialType.System_Byte };
	static ITypeSymbol? Channel(ITypeSymbol type) => type is INamedTypeSymbol { Name: "HybridChannel", Arity: 1 } channel &&
		channel.ContainingNamespace.ToDisplayString() == "Microsoft.Maui.Controls.HybridWebApps" ? channel.TypeArguments[0] : null;
	static bool Special(ITypeSymbol type) => Bytes(type) || Identity(type) is "System.Threading.CancellationToken" or "Microsoft.Maui.Controls.HybridWebApps.HybridActionContext";
	static bool Ordinary(IParameterSymbol parameter) => !Special(parameter.Type) &&
		!parameter.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() is "Microsoft.Maui.Interop.FromServicesAttribute" or "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
	static ITypeSymbol? Result(IMethodSymbol method)
	{
		if (method.ReturnsVoid)
			return null;
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
	static bool Nullable(ITypeSymbol type) => type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.None ||
		type.NullableAnnotation == NullableAnnotation.Annotated ||
		type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };
	static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
	static string Default(IParameterSymbol parameter)
	{
		var value = parameter.ExplicitDefaultValue;
		if (value is null)
			return "default";
		var defaultType = parameter.Type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
			? nullable.TypeArguments[0] : parameter.Type;
		if (defaultType.TypeKind == TypeKind.Enum)
			return "(" + TypeName(defaultType) + ")" + Convert.ToString(value, CultureInfo.InvariantCulture);
		if (value is string text)
			return Literal(text);
		if (value is char character)
			return SymbolDisplay.FormatLiteral(character, true);
		if (value is bool boolean)
			return boolean ? "true" : "false";
		if (value is double numberValue && (double.IsNaN(numberValue) || double.IsInfinity(numberValue)))
			return "double." + (double.IsNaN(numberValue) ? "NaN" : numberValue > 0 ? "PositiveInfinity" : "NegativeInfinity");
		if (value is float floatValue && (float.IsNaN(floatValue) || float.IsInfinity(floatValue)))
			return "float." + (float.IsNaN(floatValue) ? "NaN" : floatValue > 0 ? "PositiveInfinity" : "NegativeInfinity");
		var number = Convert.ToString(value, CultureInfo.InvariantCulture)!;
		return value switch
		{
			float => number + "F",
			double => number + "D",
			decimal => number + "M",
			ulong => number + "UL",
			long => number + "L",
			uint => number + "U",
			_ => number,
		};
	}

	sealed class JsonContractException(ITypeSymbol type, string message) : Exception(message)
	{
		internal ITypeSymbol Type { get; } = type;
	}

	sealed class JsonGraph(Dictionary<ITypeSymbol, INamedTypeSymbol> contexts)
	{
		internal readonly Dictionary<ITypeSymbol, INamedTypeSymbol> Contexts = contexts;
		internal readonly Dictionary<ITypeSymbol, INamedTypeSymbol> Converters = new(SymbolEqualityComparer.IncludeNullability);
		internal readonly List<ITypeSymbol> Types = new();
		internal readonly Dictionary<ITypeSymbol, ObjectShape> Objects = new(SymbolEqualityComparer.IncludeNullability);
		internal readonly Dictionary<ITypeSymbol, EnumShape> Enums = new(SymbolEqualityComparer.IncludeNullability);
		internal int Index(ITypeSymbol type) => Types.FindIndex(t => SymbolEqualityComparer.IncludeNullability.Equals(t, type));
		internal void Add(ITypeSymbol type, bool outputOnly = false)
		{
			var count = Types.Count;
			try
			{ AddCore(type, outputOnly); }
			catch
			{
				foreach (var added in Types.Skip(count))
				{
					Objects.Remove(added);
					Enums.Remove(added);
					Converters.Remove(added);
				}
				Types.RemoveRange(count, Types.Count - count);
				throw;
			}
		}
		void RequireReadable(ITypeSymbol type, HashSet<ITypeSymbol> visited)
		{
			if (!visited.Add(type) || Contexts.ContainsKey(type) || Converters.ContainsKey(type))
				return;
			if (Objects.TryGetValue(type, out var shape))
			{
				if (shape.Constructor is null)
					throw new JsonContractException(type, "an input contract requires an accessible JSON constructor");
				foreach (var property in shape.Properties)
					RequireReadable(property.Type, visited);
			}
			else if (type is IArrayTypeSymbol array)
				RequireReadable(array.ElementType, visited);
			else if (type is INamedTypeSymbol named && (Collection(named) is not null || named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
				RequireReadable(named.TypeArguments.Last(), visited);
		}
		void AddCore(ITypeSymbol type, bool outputOnly)
		{
			if (Index(type) >= 0)
			{
				if (!outputOnly)
					RequireReadable(type, new(SymbolEqualityComparer.IncludeNullability));
				return;
			}
			if (Bytes(type))
				throw new JsonContractException(type, "byte arrays require the separate binary transport or a raw-byte channel");
			if (type.TypeKind is TypeKind.TypeParameter or TypeKind.Pointer or TypeKind.FunctionPointer or TypeKind.Delegate or TypeKind.Dynamic ||
				type.SpecialType == SpecialType.System_Object || type is INamedTypeSymbol { IsRefLikeType: true })
				throw new JsonContractException(type, "use a concrete JSON contract, not object, dynamic, a delegate, pointer or ref-like type");
			Types.Add(type);
			if (Contexts.ContainsKey(type))
				return;
			var custom = type.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonConverterAttribute");
			if (custom is not null)
			{
				if (custom.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol converter || !Accessible(converter) ||
					!converter.InstanceConstructors.Any(c => Accessible(c) && c.Parameters.Length == 0) ||
					!ConverterFor(converter, type))
					throw new JsonContractException(type, "use an accessible parameterless JsonConverter<T> or root a strict source-declared context");
				Converters.Add(type, converter);
				return;
			}
			if (type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
			{
				Enums.Add(type, new EnumShape(enumType));
				return;
			}

			static bool ConverterFor(INamedTypeSymbol converter, ITypeSymbol type)
			{
				for (var current = converter; current is not null; current = current.BaseType)
					if (current.OriginalDefinition.ToDisplayString() == "System.Text.Json.Serialization.JsonConverter<T>" &&
						SymbolEqualityComparer.Default.Equals(current.TypeArguments[0], type))
						return true;
				return false;
			}
			if (Scalar(type) is not null || type.ToDisplayString() == "System.Text.Json.JsonElement")
				return;
			if (type is IArrayTypeSymbol array)
			{
				if (array.Rank != 1)
					throw new JsonContractException(type, "only one-dimensional arrays are supported");
				Add(array.ElementType, outputOnly);
				return;
			}
			if (type is INamedTypeSymbol named)
			{
				if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
				{ Add(named.TypeArguments[0], outputOnly); return; }
				var collection = Collection(named);
				if (collection is not null)
				{
					if (collection == "dictionary" && named.TypeArguments[0].SpecialType != SpecialType.System_String)
						throw new JsonContractException(type, "dictionary keys must be strings");
					Add(named.TypeArguments.Last(), outputOnly);
					return;
				}
				var ns = named.ContainingNamespace.ToDisplayString();
				if ((ns == "System" || ns.StartsWith("System.", StringComparison.Ordinal)) || named.IsAbstract || named.TypeKind == TypeKind.Interface)
					throw new JsonContractException(type, "unsupported framework or polymorphic contract; use a concrete DTO");
				var shape = Shape(named, outputOnly);
				Objects.Add(type, shape);
				foreach (var property in shape.Properties)
					Add(property.Type, outputOnly);
				if (shape.Constructor is not null)
					foreach (var parameter in shape.Constructor.Parameters)
						Add(parameter.Type, outputOnly);
				return;
			}
			throw new JsonContractException(type, "unsupported JSON type");
		}

		static ObjectShape Shape(INamedTypeSymbol type, bool outputOnly)
		{
			var properties = new List<IPropertySymbol>();
			for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
				foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
				{
					if (property.IsStatic || property.IsIndexer || property.GetMethod is null || !Accessible(property.GetMethod))
						continue;
					var attributes = property.GetAttributes();
					if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonIgnoreAttribute"))
					{
						if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonIgnoreAttribute" && a.NamedArguments.Length != 0))
							throw new JsonContractException(type, "conditional JsonIgnore is not supported");
						continue;
					}
					if (attributes.Any(a => a.AttributeClass?.Name is "JsonConverterAttribute" or "JsonExtensionDataAttribute" or "JsonNumberHandlingAttribute"))
						throw new JsonContractException(type, "custom converters, number handling and extension data are not supported by automatic action metadata");
					if (!properties.Any(p => p.Name == property.Name))
						properties.Add(property);
				}
			if (!Accessible(type) || type.GetAttributes().Any(a => a.AttributeClass?.Name is "JsonConverterAttribute" or "JsonPolymorphicAttribute" or "JsonDerivedTypeAttribute"))
				throw new JsonContractException(type, "inaccessible or polymorphic/custom-converter DTO");
			if (properties.Select(WireName).Distinct(StringComparer.Ordinal).Count() != properties.Count)
				throw new JsonContractException(type, "duplicate JSON property names");
			var constructors = type.InstanceConstructors.Where(c => Accessible(c) &&
				c.Parameters.All(p => p.RefKind == RefKind.None && properties.Any(property => string.Equals(property.Name, p.Name, StringComparison.OrdinalIgnoreCase) &&
					SymbolEqualityComparer.Default.Equals(property.Type, p.Type)))).ToArray();
			var selected = constructors.Where(c => c.GetAttributes().Any(a => a.AttributeClass?.Name == "JsonConstructorAttribute")).ToArray();
			if (selected.Length > 1)
				throw new JsonContractException(type, "ambiguous JSON constructors");
			var constructor = selected.SingleOrDefault() ?? constructors.OrderByDescending(c => c.Parameters.Length).FirstOrDefault();
			if (constructor is null && outputOnly)
				return new ObjectShape(properties, null);
			if (constructor is null)
				throw new JsonContractException(type, "an accessible constructor must bind its parameters to JSON properties");
			foreach (var property in properties)
				if (!constructor.Parameters.Any(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)) &&
					(property.SetMethod is null || !Accessible(property.SetMethod)))
				{
					if (outputOnly)
						return new ObjectShape(properties, null);
					throw new JsonContractException(type, "read-only properties must bind to constructor parameters");
				}
				else if (property.SetMethod?.IsInitOnly == true && !property.IsRequired &&
					!constructor.Parameters.Any(p => string.Equals(p.Name, property.Name, StringComparison.OrdinalIgnoreCase)))
				{
					if (outputOnly)
						return new ObjectShape(properties, null);
					throw new JsonContractException(type, "optional init-only properties must bind to constructor parameters");
				}
			return new ObjectShape(properties, constructor);
		}
	}
	sealed class EnumShape
	{
		internal readonly List<(string Field, string Wire, ulong Bits)> Members;
		internal readonly bool Flags;
		internal readonly ulong WidthMask;
		internal readonly ulong KnownBits;
		internal EnumShape(INamedTypeSymbol type)
		{
			if (!Accessible(type) || type.GetAttributes().Any(a => a.AttributeClass?.Name == "JsonConverterAttribute"))
				throw new JsonContractException(type, "inaccessible or custom-converter enum");
			Flags = type.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");
			WidthMask = type.EnumUnderlyingType!.SpecialType switch
			{
				SpecialType.System_Byte or SpecialType.System_SByte => byte.MaxValue,
				SpecialType.System_Int16 or SpecialType.System_UInt16 => ushort.MaxValue,
				SpecialType.System_Int32 or SpecialType.System_UInt32 => uint.MaxValue,
				_ => ulong.MaxValue,
			};
			Members = type.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue)
				.OrderBy(field => field.Name, StringComparer.Ordinal).Select(field =>
				{
					var wire = field.GetAttributes().FirstOrDefault(a =>
						a.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute")?.ConstructorArguments[0].Value as string ??
						CamelCase(field.Name);
					var bits = field.ConstantValue switch
					{
						sbyte value => unchecked((ulong)value),
						short value => unchecked((ulong)value),
						int value => unchecked((ulong)value),
						long value => unchecked((ulong)value),
						_ => Convert.ToUInt64(field.ConstantValue, CultureInfo.InvariantCulture),
					};
					return (field.Name, wire, bits & WidthMask);
				}).ToList();
			if (Members.Any(member => member.Wire.Length == 0) ||
				Members.GroupBy(member => member.Wire, StringComparer.Ordinal).Any(group => group.Select(member => member.Bits).Distinct().Count() > 1))
				throw new JsonContractException(type, "enum wire names must be nonempty and unambiguous");
			// All = ~0 is a declared sentinel, not permission to pass arbitrary undeclared bits.
			foreach (var member in Members.Where(member => member.Bits != WidthMask))
				KnownBits |= member.Bits;
		}
	}
	sealed class ObjectShape(List<IPropertySymbol> properties, IMethodSymbol? constructor)
	{
		internal List<IPropertySymbol> Properties { get; } = properties;
		internal IMethodSymbol? Constructor { get; } = constructor;
	}
	static string WireName(IPropertySymbol property) =>
		property.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "JsonPropertyNameAttribute")?.ConstructorArguments[0].Value as string ??
		CamelCase(property.Name);
	static string CamelCase(string name)
	{
		var chars = name.ToCharArray();
		for (int i = 0; i < chars.Length && char.IsUpper(chars[i]); i++)
		{
			if (i > 0 && i + 1 < chars.Length && !char.IsUpper(chars[i + 1]))
				break;
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
		SpecialType.System_Byte => "Byte",
		SpecialType.System_SByte => "SByte",
		SpecialType.System_Int16 => "Int16",
		SpecialType.System_UInt16 => "UInt16",
		SpecialType.System_Int32 => "Int32",
		SpecialType.System_UInt32 => "UInt32",
		SpecialType.System_Int64 => "Int64",
		SpecialType.System_UInt64 => "UInt64",
		SpecialType.System_Single => "Single",
		SpecialType.System_Double => "Double",
		SpecialType.System_Decimal => "Decimal",
		SpecialType.System_Char => "Char",
		_ => Identity(type) switch
		{
			"System.DateTime" => "DateTime",
			"System.DateTimeOffset" => "DateTimeOffset",
			"System.Guid" => "Guid",
			"System.DateOnly" => "DateOnly",
			"System.TimeOnly" => "TimeOnly",
			"System.TimeSpan" => "TimeSpan",
			"System.Uri" => "Uri",
			_ => null,
		},
	};

	sealed class Emitter(INamedTypeSymbol provider, List<IMethodSymbol> methods, JsonGraph graph,
		string[] topics, List<(string Name, string Start, string Stop)> hooks, ITypeSymbol textType, List<IEventSymbol> nativeEvents, bool imported, string contractsId)
	{
		readonly StringBuilder _code = new();
		internal string Id => "HybridActions_" + StableId(provider.ToDisplayString()) + "_" + provider.Name;
		void Line(string text = "") => _code.AppendLine(text);
		string Info(ITypeSymbol type) => graph.Contexts.TryGetValue(type, out var context) ?
			"global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonContracts.Get<" + TypeName(type) + ">(" + TypeName(context) + ".Default)" : "Json.Default.T" + graph.Index(type);
		bool BuiltIn(ITypeSymbol type) => !graph.Contexts.ContainsKey(type) && !graph.Converters.ContainsKey(type) &&
			(Scalar(type) is not null || type.ToDisplayString() == "System.Text.Json.JsonElement");
		string Codec(ITypeSymbol type) => !BuiltIn(type) ? "Codec" + graph.Index(type) :
			Scalar(type) is string scalar ? "global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonPrimitives." + scalar :
			type.ToDisplayString() == "System.Text.Json.JsonElement" ? "global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonPrimitives.JsonElement" : "Codec" + graph.Index(type);
		string Read(ITypeSymbol type, string value) => Codec(type) + ".Read(" + value + ", " + (Nullable(type) ? "true" : "false") + ")";
		string Write(ITypeSymbol type, string value) => Codec(type) + ".Write(writer, " + value + (type.IsReferenceType ? "!" : "") + ", " + (Nullable(type) ? "true" : "false") + ");";
		internal string Emit()
		{
			ProviderHeader();
			var target = TypeName(provider);
			Line("internal static void Register(global::Microsoft.Maui.Controls.HybridWebApps.CommandRegistry registry, global::System.IServiceProvider services)");
			Line("{");
			foreach (var topic in topics)
				Line("registry.RegisterTopic(" + Literal(topic) + ");");
			if (hooks.Count != 0)
				Line("var source = services.GetRequiredService<" + target + ">();");
			foreach (var hook in hooks)
			{
				// Scope disposal has already disabled service resolution when the hub detaches active topics.
				Line("registry.RegisterTopic(" + Literal(hook.Name) + ", source.@" + hook.Start + ", source.@" + hook.Stop + ");");
			}
			EmitEvents(target);
			for (int i = 0; i < methods.Count; i += 8)
				Line("RegisterActions" + (i / 8) + "(registry, services);");
			Line("}");
			Line("}");
			return _code.ToString();
		}
		void ProviderHeader()
		{
			Line("// <auto-generated/>");
			Line("#nullable enable");
			Line("using System;");
			Line("using System.Linq;");
			Line("using System.Threading.Tasks;");
			Line("using System.Text.Json;");
			Line("using System.Text.Json.Serialization;");
			Line("using System.Text.Json.Serialization.Metadata;");
			Line("using Microsoft.Extensions.DependencyInjection;");
			Line("using static Microsoft.Maui.Controls.HybridWebApps.Generated." + contractsId + ";");
			Line("namespace Microsoft.Maui.Controls.HybridWebApps.Generated;");
			Line("internal static partial class " + Id);
			Line("{");
		}
		internal void EmitActionFiles(SourceProductionContext output)
		{
			for (int i = 0; i < methods.Count; i += 8)
			{
				_code.Clear();
				ProviderHeader();
				Line("static void RegisterActions" + (i / 8) + "(global::Microsoft.Maui.Controls.HybridWebApps.CommandRegistry registry, global::System.IServiceProvider services) {");
				foreach (var method in methods.Skip(i).Take(8))
					EmitAction(method, TypeName(provider));
				Line("}");
				Line("}");
				output.AddSource(Id + ".Actions" + (i / 8) + ".g.cs", _code.ToString());
			}
		}
		void EmitEvents(string target)
		{
			foreach (var ev in nativeEvents)
			{
				var receiver = TypeName(ev.ContainingType);
				var invoke = ((INamedTypeSymbol)ev.Type).DelegateInvokeMethod!;
				var payload = Payload(ev);
				var source = ev.IsStatic ? "" : TypeName(ev.ContainingType) + ", ";
				var resolve = ev.IsStatic ? "" : "() => (" + TypeName(ev.ContainingType) + ")services.GetRequiredService<" + target + ">(), ";
				Line("global::Microsoft.Maui.Controls.HybridWebApps.HybridEvents.Register<" + source + TypeName(ev.Type) + ">(registry, services, " + Literal(EventName(ev)) + ", " + resolve + "publisher => (" +
					string.Join(", ", invoke.Parameters.Select(p => "eventArg" + p.Ordinal)) + ") => publisher.Publish(writer => {");
				if (payload.Length == 0)
					Line("writer.WriteNullValue();");
				else if (payload.Length == 1)
					Line(Write(payload[0].Type, "eventArg" + payload[0].Ordinal));
				else
				{
					Line("writer.WriteStartObject();");
					foreach (var parameter in payload)
					{
						Line("writer.WritePropertyName(" + Literal(parameter.Name) + ");");
						Line(Write(parameter.Type, "eventArg" + parameter.Ordinal));
					}
					Line("writer.WriteEndObject();");
				}
				var callback = ev.IsStatic ? "handler" : "(nativeSource, handler)";
				receiver = ev.IsStatic ? receiver : "nativeSource";
				Line("}), " + callback + " => " + receiver + ".@" + ev.Name + " += handler, " + callback + " => " + receiver + ".@" + ev.Name + " -= handler);");
			}
		}
		void EmitAction(IMethodSymbol method, string target)
		{
			var ordinary = method.Parameters.Where(Ordinary).ToArray();
			var result = Result(method);
			var binary = Bytes(result);
			var infos = ordinary.Select(p => Channel(p.Type) is null ? p.Type : textType)
				.Concat(ordinary.Select(p => Channel(p.Type)).Where(t => t is not null && !Bytes(t)).OfType<ITypeSymbol>())
				.Concat(result is null || binary ? Enumerable.Empty<ITypeSymbol>() : new[] { result })
				.Distinct(SymbolEqualityComparer.Default).OfType<ITypeSymbol>();
			var metadata = infos.Select(Info).ToList();
			if (result is null)
				metadata.Add("Json.Default.Null");
			Line("registry." + (binary ? "RegisterGeneratedBinary" : "RegisterGenerated") + "(" + Literal(Name(method)) + ", new JsonTypeInfo[] { " + string.Join(", ", metadata) + " }, async (json, operation) =>");
			Line("{");
			if (binary && !method.Parameters.Any(p => Bytes(p.Type)))
				Line("if (operation.BinaryPayload.Length != 0) throw new global::Microsoft.Maui.Controls.HybridWebApps.CommandException(\"invalid_arguments\", \"This byte-result action has no binary input parameter.\");");
			Line("var args = new global::Microsoft.Maui.Controls.HybridWebApps.HybridActionArguments(json, new string[] { " + string.Join(", ", ordinary.Select(p => Literal(p.Name))) + " });");
			for (int i = 0; i < ordinary.Length; i++)
			{
				var parameter = ordinary[i];
				var read = "args.Read(" + i + ", " + Info(Channel(parameter.Type) is null ? parameter.Type : textType) + ", " + (Nullable(parameter.Type) ? "true" : "false") + ")";
				var channel = Channel(parameter.Type);
				if (channel is not null)
					read = "operation.BindChannel<" + TypeName(channel) + ">(" + read + ", " + (Bytes(channel) ? "null" : Info(channel)) + ")";
				Line("var arg" + i + " = " + (parameter.HasExplicitDefaultValue ? "args.Has(" + i + ") ? " + read + " : " + Default(parameter) : read) + ";");
			}
			if (imported)
				Line("global::Microsoft.Maui.Controls.HybridWebApps.HybridNativePolicy.Validate(services, " + Literal(Name(method)) + ", json);");
			Line("Task<" + (binary ? "byte[]" : "JsonElement") + "> Invoke()");
			Line("{");
			var arguments = method.Parameters.Select(parameter =>
			{
				if (Ordinary(parameter))
					return "arg" + Array.IndexOf(ordinary, parameter);
				if (Bytes(parameter.Type))
					return "operation.BinaryPayload";
				if (Identity(parameter.Type) == "System.Threading.CancellationToken")
					return "operation.Cancellation";
				if (Identity(parameter.Type) == "Microsoft.Maui.Controls.HybridWebApps.HybridActionContext")
					return "new global::Microsoft.Maui.Controls.HybridWebApps.HybridActionContext(operation)";
				var keyed = parameter.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute");
				var key = keyed is null ? "" : ", " + Constant(keyed.ConstructorArguments[0]);
				return "(services ?? throw new InvalidOperationException(\"This action requires the owning service scope.\"))." +
					(keyed is null ? "GetRequiredService" : "GetRequiredKeyedService") + "<" + TypeName(parameter.Type) + ">(" + key.TrimStart(',', ' ') + ")";
			});
			var receiver = method.IsStatic ? TypeName(method.ContainingType) : "services.GetRequiredService<" + target + ">()";
			if (imported && !method.IsStatic && !SymbolEqualityComparer.Default.Equals(method.ContainingType, provider))
				receiver = "((" + TypeName(method.ContainingType) + ")" + receiver + ")";
			var call = method.MethodKind == MethodKind.PropertyGet ? receiver + ".@" + method.AssociatedSymbol!.Name :
				receiver + ".@" + method.Name + "(" + string.Join(", ", arguments) + ")";
			if (Awaitable(method))
			{
				Line("return Await();");
				Line("async Task<" + (binary ? "byte[]" : "JsonElement") + "> Await()");
				Line("{");
				Line((result is null ? "" : "var result = ") + "await " + call + ";");
				if (Name(method) == "clipboard.read")
					Line("global::Microsoft.Maui.Controls.HybridWebApps.HybridNativePolicy.ValidateText(services, result);");
				EmitResult(result, false);
				Line("}");
			}
			else
			{
				Line((result is null ? "" : "var result = ") + call + ";");
				EmitResult(result, true);
			}
			Line("}");
			if (provider.ToDisplayString() is "Microsoft.Maui.Controls.Page" or "Microsoft.Maui.Devices.Sensors.IGeolocation")
				Line("return await services.GetRequiredService<global::Microsoft.Maui.Controls.HybridWebApps.NativeDispatcher>().RunAsync(() => services.GetRequiredService<global::Microsoft.Maui.Controls.HybridWebApps.UI.HybridViewContext>().InteractAsync(Invoke, operation.Cancellation), operation.Cancellation).ConfigureAwait(false);");
			else if (Policy(method, "MainThread"))
				Line("return await services.GetRequiredService<global::Microsoft.Maui.Controls.HybridWebApps.NativeDispatcher>().RunAsync(Invoke, operation.Cancellation).ConfigureAwait(false);");
			else
				Line("return await Invoke().ConfigureAwait(false);");
			var enabled = !Attribute(method).NamedArguments.Any(pair => pair.Key == "Enabled" && pair.Value.Value is false);
			Line("}, interactive: " + (Policy(method, "Interactive") ? "true" : "false") + ", enabled: " + (enabled ? "true" : "false") + ");");
		}
		void EmitResult(ITypeSymbol? result, bool task)
		{
			if (result is { IsReferenceType: true } && !Nullable(result))
				Line("if (result is null) throw new JsonException(\"An action returned null for a nonnullable contract.\");");
			var expression = Bytes(result) ? "result" : result is null ? "global::System.Text.Json.JsonSerializer.SerializeToElement<object?>(null, Json.Default.Null)" :
				"global::System.Text.Json.JsonSerializer.SerializeToElement(result" + (result.IsReferenceType ? "!" : "") + ", " + Info(result) + ")";
			Line("return " + (task ? "Task.FromResult(" + expression + ")" : expression) + ";");
		}
		internal static string Constant(TypedConstant constant)
		{
			if (constant.Kind == TypedConstantKind.Type && constant.Value is ITypeSymbol keyType)
				return "typeof(" + TypeName(keyType) + ")";
			if (constant.Value is ITypeSymbol type)
				return "typeof(" + TypeName(type) + ")";
			if (constant.Type?.TypeKind == TypeKind.Enum)
				return "(" + TypeName(constant.Type) + ")" + Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
			if (constant.Type?.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16)
				return "(" + TypeName(constant.Type) + ")" + Convert.ToString(constant.Value, CultureInfo.InvariantCulture);
			return constant.Value switch
			{
				null => "null",
				string value => Literal(value),
				char value => SymbolDisplay.FormatLiteral(value, true),
				bool boolean => boolean ? "true" : "false",
				long value => value.ToString(CultureInfo.InvariantCulture) + "L",
				ulong value => value.ToString(CultureInfo.InvariantCulture) + "UL",
				uint value => value.ToString(CultureInfo.InvariantCulture) + "U",
				float value => value.ToString(CultureInfo.InvariantCulture) + "F",
				double value => value.ToString(CultureInfo.InvariantCulture) + "D",
				_ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)!,
			};
		}
		internal void EmitContracts(SourceProductionContext output)
		{
			_code.Clear();
			ContractHeader();
			EmitJson();
			Line("}");
			output.AddSource("HybridContracts.Context.g.cs", _code.ToString());
			for (int i = 0; i < graph.Types.Count; i++)
			{
				var type = graph.Types[i];
				if (BuiltIn(type))
					continue;
				_code.Clear();
				ContractHeader();
				EmitType(i, type);
				Line("}");
				output.AddSource("HybridContracts." + StableId(type.ToDisplayString(TypeFormat)) + "." + i.ToString(CultureInfo.InvariantCulture) + ".g.cs", _code.ToString());
			}
		}
		void ContractHeader()
		{
			Line("// <auto-generated/>");
			Line("#nullable enable");
			Line("using System;");
			Line("using System.Linq;");
			Line("using System.Text.Json;");
			Line("using System.Text.Json.Serialization;");
			Line("using System.Text.Json.Serialization.Metadata;");
			Line("namespace Microsoft.Maui.Controls.HybridWebApps.Generated;");
			Line("internal static partial class " + contractsId + " {");
		}
		void EmitJson()
		{
			Line("internal sealed class Json : JsonSerializerContext");
			Line("{");
			Line("static JsonSerializerOptions Strict() => global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonDefaults.CreateOptions();");
			Line("internal static Json Default { get; } = new();");
			Line("protected override JsonSerializerOptions? GeneratedSerializerOptions => Strict();");
			Line("Json() : base(Strict()) {");
			for (int i = 0; i < graph.Types.Count; i++)
			{
				Line("T" + i + " = " + Codec(graph.Types[i]) + ".CreateInfo(this);");
			}
			Line("Null = global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonDefaults.Null.CreateInfo(this);");
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
		}
		void EmitType(int index, ITypeSymbol type)
		{
			var name = TypeName(type);
			if (graph.Enums.TryGetValue(type, out var shape))
			{
				var underlying = TypeName(((INamedTypeSymbol)type).EnumUnderlyingType!);
				Line("internal static readonly global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonCodec<" + name + "> Codec" + index +
					" = global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonEnums.Create<" + name + ">(");
				Line("new string[] { " + string.Join(", ", shape.Members.Select(m => Literal(m.Wire))) + " },");
				Line("new ulong[] { " + string.Join(", ", shape.Members.Select(m => m.Bits.ToString(CultureInfo.InvariantCulture) + "UL")) + " },");
				Line("value => unchecked((ulong)(" + underlying + ")value), bits => unchecked((" + name + ")(" + underlying + ")bits), " +
					(shape.Flags ? "true" : "false") + ", " + shape.WidthMask.ToString(CultureInfo.InvariantCulture) + "UL, " + shape.KnownBits.ToString(CultureInfo.InvariantCulture) + "UL);");
				return;
			}
			if (graph.Converters.TryGetValue(type, out var converter))
				Line("static readonly " + TypeName(converter) + " CustomConverter" + index + " = new();");
			Line("internal static readonly global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonCodec<" + name + "> Codec" + index +
				" = new(Read" + index + ", Write" + index + ");");
			Line("static " + name + " Read" + index + "(JsonElement json, bool nullable) {");
			if (graph.Contexts.ContainsKey(type) || graph.Converters.ContainsKey(type))
			{
				Line("if (json.ValueKind == JsonValueKind.Null && !nullable) throw new JsonException(\"Null is not allowed.\");");
				if (type.IsReferenceType && graph.Converters.ContainsKey(type))
					Line("if (json.ValueKind == JsonValueKind.Null && !CustomConverter" + index + ".HandleNull) return default!;");
				if (graph.Contexts.ContainsKey(type))
					Line("return JsonSerializer.Deserialize(json, " + Info(type) + ")!;");
				else
				{
					Line("var reader = new Utf8JsonReader(global::System.Text.Encoding.UTF8.GetBytes(json.GetRawText())); reader.Read();");
					Line("return CustomConverter" + index + ".Read(ref reader, typeof(" + name + "), Json.Default.Options)!;");
				}
				Line("}");
				Line("static void Write" + index + "(Utf8JsonWriter writer, " + name + " value, bool nullable) {");
				if (type.IsReferenceType)
					Line("if (value is null && !nullable) throw new JsonException(\"Null is not allowed.\");");
				if (type.IsReferenceType && graph.Converters.ContainsKey(type))
					Line("if (value is null && !CustomConverter" + index + ".HandleNull) { writer.WriteNullValue(); return; }");
				if (graph.Contexts.ContainsKey(type))
					Line("JsonSerializer.Serialize<" + name + ">(writer, value!, " + Info(type) + ");");
				else
					Line("CustomConverter" + index + ".Write(writer, value!, Json.Default.Options);");
				Line("}");
				return;
			}
			if (type.IsReferenceType || Nullable(type))
				Line("if (json.ValueKind == JsonValueKind.Null) { if (!nullable) throw new JsonException(\"Null is not allowed.\"); return default!; }");
			if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType)
				Line("return " + Read(nullableType.TypeArguments[0], "json") + ";");
			else if (type is IArrayTypeSymbol array)
				EmitReadCollection(array.ElementType, "array", name);
			else if (type is INamedTypeSymbol collection && Collection(collection) is string kind)
				EmitReadCollection(collection.TypeArguments.Last(), kind, name);
			else
				EmitReadObject(type, graph.Objects[type]);
			Line("}");
			Line("static void Write" + index + "(Utf8JsonWriter writer, " + name + " value, bool nullable) {");
			if (type.IsReferenceType || Nullable(type))
				Line("if (value is null) { if (!nullable) throw new JsonException(\"Null is not allowed.\"); writer.WriteNullValue(); return; }");
			if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } wrapper)
				Line(Write(wrapper.TypeArguments[0], "value.Value"));
			else if (type is IArrayTypeSymbol sequence)
				EmitWriteCollection(sequence.ElementType, false);
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
				Line("return global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonCollections.ReadDictionary<" + AnnotatedTypeName(element) + ">(json, item => " + Read(element, "item") + ");");
			else
			{
				var collection = "global::System.Collections.Generic." + (kind == "set" ? "HashSet" : "List") + "<" + AnnotatedTypeName(element) + ">";
				Line("return global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonCollections.Read<" + AnnotatedTypeName(element) + ", " + collection +
					">(json, () => new(), item => " + Read(element, "item") + ")" + (kind == "array" ? ".ToArray()" : "") + ";");
			}
		}
		void EmitWriteCollection(ITypeSymbol element, bool dictionary)
		{
			Line("global::Microsoft.Maui.Controls.HybridWebApps.HybridJsonCollections." + (dictionary ? "WriteDictionary" : "Write") +
				"(writer, value, (writer, item) => { " + Write(element, "item") + " });");
		}
		void EmitReadObject(ITypeSymbol type, ObjectShape shape)
		{
			if (shape.Constructor is null)
			{
				Line("throw new JsonException(\"This native output contract has no JSON constructor.\");");
				return;
			}
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
				if (!parameter.HasExplicitDefaultValue)
					Line("if (!has" + i + ") throw new JsonException(\"Missing constructor member.\");");
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
