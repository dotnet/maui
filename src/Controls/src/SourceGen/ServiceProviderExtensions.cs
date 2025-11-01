using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

static class ServiceProviderExtensions
{
	public static ILocalValue GetOrCreateServiceProvider(this INode node, IndentedTextWriter writer, SourceGenContext context, ImmutableArray<ITypeSymbol>? requiredServices)
	{
		IFieldSymbol? bpFieldSymbol = null;
		IPropertySymbol? propertySymbol = null;
		if (node.TryGetPropertyName(node.Parent, out var propertyName))
		{
			var localName = propertyName.LocalName;
			if (context.Variables.TryGetValue(node.Parent, out var parentVar))
			{
				bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, null);
				propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
			}
		}

		//TODO based on the node, the service provider, or some of the services, could be reused
		var serviceProviderSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.Internals.XamlServiceProvider")!;
		var serviceProviderVariableName = NamingHelpers.CreateUniqueVariableName(context, serviceProviderSymbol);

		var rootVar = context.Variables.FirstOrDefault(kvp => kvp.Key is SGRootNode).Value;
		writer.WriteLine($"var {serviceProviderVariableName} = new {serviceProviderSymbol.ToFQDisplayString()}({rootVar?.ValueAccessor ?? "this"});");

		node.AddServices(writer, serviceProviderVariableName, requiredServices, context, bpFieldSymbol, propertySymbol);

		return context.ServiceProviders[node] = new LocalVariable(serviceProviderSymbol, serviceProviderVariableName);
	}

	static void AddServices(this INode node, IndentedTextWriter writer, string serviceProviderVariableName, ImmutableArray<ITypeSymbol>? requiredServices, SourceGenContext context, IFieldSymbol? bpFieldSymbol, IPropertySymbol? propertySymbol)
	{
		var createAllServices = requiredServices is null;
		var parentObjects = node.ParentObjects(context).Select(v => v.ValueAccessor).ToArray();
		if (parentObjects.Length == 0)
			parentObjects = ["null"];
		if (createAllServices
				|| requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IProvideParentValues")!, SymbolEqualityComparer.Default)
				|| requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IProvideValueTarget")!, SymbolEqualityComparer.Default)
				|| requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IReferenceProvider")!, SymbolEqualityComparer.Default))
		{
			var simpleValueTargetProvider = NamingHelpers.CreateUniqueVariableName(context, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IProvideValueTarget")!);
			writer.WriteLine($"var {simpleValueTargetProvider} = new global::Microsoft.Maui.Controls.Xaml.Internals.SimpleValueTargetProvider(");
			writer.Indent++;
			writer.WriteLine($"new object?[] {{{string.Join(", ", parentObjects)}}},");
			var bpinfo = bpFieldSymbol?.ToFQDisplayString() ?? String.Empty;
			var pinfo = $"typeof({propertySymbol?.ContainingSymbol.ToFQDisplayString()}).GetProperty(\"{propertySymbol?.Name}\")" ?? string.Empty;
			writer.WriteLine($"{(bpFieldSymbol != null ? bpFieldSymbol : propertySymbol != null ? pinfo : "null")},");
			if (context.Scopes.TryGetValue(node, out var scope))
			{
				List<string> scopes = [scope.namescope.ValueAccessor];
				var values = context.ParentContext?.Scopes.Select(s => s.Value.namescope.ValueAccessor).Distinct();
				scopes.AddRange(values ?? Enumerable.Empty<string>());
				using (PrePost.NewConditional(writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE", orElse: () => writer.WriteLine($"null,")))
				{
					writer.WriteLine($"new [] {{ {string.Join(", ", scopes)} }},");
				}
			}
			else
				writer.WriteLine($"null,");

			var rootVar = context.Variables.FirstOrDefault(kvp => kvp.Key is SGRootNode).Value;
			writer.WriteLine($"{rootVar?.ValueAccessor ?? "this"});");
			writer.Indent--;
			writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IReferenceProvider), {simpleValueTargetProvider});");
			writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IProvideValueTarget), {simpleValueTargetProvider});");
		}

		if (createAllServices
			|| requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IXamlTypeResolver")!, SymbolEqualityComparer.Default))
		{
			var nsResolver = NamingHelpers.CreateUniqueVariableName(context, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.Internals.XmlNamespaceResolver")!);
			writer.WriteLine($"var {nsResolver} = new global::Microsoft.Maui.Controls.Xaml.Internals.XmlNamespaceResolver();");

			foreach (var kvp in node.NamespaceResolver!.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml))
				writer.WriteLine($"{nsResolver}.Add(\"{kvp.Key}\", \"{kvp.Value}\");");

			writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IXamlTypeResolver), new global::Microsoft.Maui.Controls.Xaml.Internals.XamlTypeResolver({nsResolver}, typeof({context.RootType.ToFQDisplayString()}).Assembly));");
		}
	}

	static IEnumerable<ILocalValue> ParentObjects(this INode node, SourceGenContext context)
	{
		var currentCtx = context;
		while (currentCtx != null)
		{
			var n = node.Parent;
			while (n is not null)
			{
				if (n is ElementNode en && currentCtx.Variables.TryGetValue(en, out var parentVariable))
					yield return parentVariable;
				n = n.Parent;
			}
			currentCtx = currentCtx.ParentContext;
		}
	}

	public static (bool acceptEmptyServiceProvider, ImmutableArray<ITypeSymbol>? requiredServices) GetServiceProviderAttributes(this ITypeSymbol typeConverter, SourceGenContext context)
	{
		var acceptEmptyServiceProviderAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProviderAttribute")!;
		var acceptEmptyServiceProvider = typeConverter.GetAttributes(acceptEmptyServiceProviderAttribute).Any();
		;
		var requireServiceAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RequireServiceAttribute")!;
		var requiredServices = typeConverter.GetAttributes(requireServiceAttribute).FirstOrDefault()?.ConstructorArguments[0].Values.Where(ca => ca.Value is ITypeSymbol).Select(ca => (ca.Value as ITypeSymbol)!).ToImmutableArray() ?? null;
		return (acceptEmptyServiceProvider, requiredServices);
	}
}
