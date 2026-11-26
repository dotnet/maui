using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

static class ServiceProviderExtensions
{
	// Available when the generator runs on a compiler with C# 14 extension-block symbols.
	static readonly System.Reflection.PropertyInfo? s_extensionGroupingName = typeof(INamedTypeSymbol).GetProperty("ExtensionGroupingName");

	public static ILocalValue GetOrCreateServiceProvider(this INode node, IndentedTextWriter writer, SourceGenContext context, ImmutableArray<ITypeSymbol>? requiredServices)
	{
		IFieldSymbol? bpFieldSymbol = null;
		IPropertySymbol? propertySymbol = null;
		IPropertySymbol? extensionProperty = null;
		if (node.TryGetPropertyName(node.Parent, out var propertyName))
		{
			var localName = propertyName.LocalName;
			if (context.Variables.TryGetValue(node.Parent, out var parentVar))
			{
				bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, null);
				propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
				if (bpFieldSymbol is null)
				{
					extensionProperty = SetPropertyHelpers.FindExtensionProperty(parentVar.Type, propertyName, node, context);
					propertySymbol = extensionProperty ?? propertySymbol;
				}
			}
		}

		//TODO based on the node, the service provider, or some of the services, could be reused
		var serviceProviderSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.Internals.XamlServiceProvider")!;
		var serviceProviderVariableName = NamingHelpers.CreateUniqueVariableName(context, serviceProviderSymbol);

		var rootVar = context.Variables.FirstOrDefault(kvp => kvp.Key is SGRootNode).Value;
		writer.WriteLine($"var {serviceProviderVariableName} = new {serviceProviderSymbol.ToFQDisplayString()}({rootVar?.ValueAccessor ?? "this"});");

		node.AddServices(writer, serviceProviderVariableName, requiredServices, context, bpFieldSymbol, propertySymbol, extensionProperty);

		return context.ServiceProviders[node] = new LocalVariable(serviceProviderSymbol, serviceProviderVariableName);
	}

	static void AddServices(this INode node, IndentedTextWriter writer, string serviceProviderVariableName, ImmutableArray<ITypeSymbol>? requiredServices, SourceGenContext context, IFieldSymbol? bpFieldSymbol, IPropertySymbol? propertySymbol, IPropertySymbol? extensionProperty)
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
			var pinfo = extensionProperty is not null
				? GetExtensionPropertyInfo(extensionProperty)
				: $"typeof({propertySymbol?.ContainingSymbol.ToFQDisplayString()}).GetProperty(\"{propertySymbol?.Name}\")";
			writer.WriteLine($"{(bpFieldSymbol != null ? bpinfo : propertySymbol != null ? pinfo : "null")},");
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

		static string GetExtensionPropertyInfo(IPropertySymbol property)
		{
			var nestedNames = new Stack<string>();
			var type = property.ContainingType;
			while (type is not null && (!type.CanBeReferencedByName || !ExtensionPropertyConventions.IsSpeakable(type.MetadataName)))
			{
				// Newer Roslyn projects the marker as the containing type; the runtime property is on its grouping type.
				nestedNames.Push(s_extensionGroupingName?.GetValue(type) as string ?? type.MetadataName);
				type = type.ContainingType;
			}

			if (type is null || nestedNames.Count == 0)
				throw new InvalidOperationException("The extension property has no accessible declaring container.");

			var expression = $"typeof({type.ToFQDisplayString()})";
			while (nestedNames.Count > 0)
				expression += $".GetNestedType({SymbolDisplay.FormatLiteral(nestedNames.Pop(), true)}, global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic)!";
			return $"{expression}.GetProperty({SymbolDisplay.FormatLiteral(property.Name, true)}, global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.Static)!";
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
		if (node is IXmlLineInfo xmlLineInfo
			&& (createAllServices
			   || requiredServices!.Value.Contains(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IXmlLineInfoProvider")!, SymbolEqualityComparer.Default)))
		{
			writer.WriteLine($"{serviceProviderVariableName}.Add(typeof(global::Microsoft.Maui.Controls.Xaml.IXmlLineInfoProvider), new global::Microsoft.Maui.Controls.Xaml.Internals.XmlLineInfoProvider(new global::Microsoft.Maui.Controls.Xaml.XmlLineInfo({xmlLineInfo.LineNumber}, {xmlLineInfo.LinePosition})));");

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
